﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Drawing.Printing;
using WizMes_ParkPro.PopUP;
using WizMes_ParkPro.PopUp;
using WPF.MDI;

/**************************************************************************************************
'** 프로그램명 : Win_Qul_DefectRepair_Q
'** 설명       : 검사실적 등록
'** 작성일자   : 2023.04.03
'** 작성자     : 장시영
'**------------------------------------------------------------------------------------------------
'**************************************************************************************************
' 변경일자  , 변경자, 요청자    , 요구사항ID      , 요청 및 작업내용
'**************************************************************************************************
' 2023.04.03, 장시영, 저장시 메인 저장 후 서브 저장되도록 수정,
                    , LotNo 플러스 파인더 조회후 기존에 InspectAuto에 저장되어 있다면 
                      가져오는 부분 삭제 (fn_getInspectID)
                    , 측정값 저장 로직 변경
'**************************************************************************************************/

namespace WizMes_ParkPro
{
    /// <summary>
    /// Win_Qul_InspectAuto_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_Qul_InspectAuto_U : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        //불량을 체크하는 리스트 
        List<DataRow> defectCheck1 = new List<DataRow>(); //sub1
        List<DataRow> defectCheck2 = new List<DataRow>(); //sub2
        Lib lib = new Lib();

        int DFCount1 = 0;
        int DFCount2 = 0;
        int DFCount3 = 0;
        int DFCount4 = 0;
        int DFCount5 = 0;

        //검사성적서에는 5가지 수량 밖에 안나와서...  데이터 그리드에 값은 10까지 있지만.. 안 쓸 듯
        int DFCount6 = 0;
        int DFCount7 = 0;
        int DFCount8 = 0;
        int DFCount9 = 0;
        int DFCount10 = 0;


        string strPoint = string.Empty;     //  1: 수입, 3:자주, 5:출하
        string strFlag = string.Empty;

        int Wh_Ar_SelectedLastIndex = 0;        // 그리드 마지막 선택 줄 임시저장 그릇

        string strBasisID = string.Empty;
        int BasisSeq = 1;

        string strTotalCount = string.Empty;
        string strDefectYN = string.Empty;

        Win_Qul_InspectAuto_U_CodeView WinInsAuto = new Win_Qul_InspectAuto_U_CodeView();
        Win_Qul_InspectAuto_U_Sub_CodeView WinInsAutoSub = new Win_Qul_InspectAuto_U_Sub_CodeView();
        ObservableCollection<EcoNoAndBasisID> ovcEvoBasis = new ObservableCollection<EcoNoAndBasisID>();

        List<Win_Qul_InspectAuto_U_CodeView> listLotLabelPrint = new List<Win_Qul_InspectAuto_U_CodeView>();

        private Microsoft.Office.Interop.Excel.Application excelapp;
        private Microsoft.Office.Interop.Excel.Workbook workbook;
        private Microsoft.Office.Interop.Excel.Worksheet worksheet;
        private Microsoft.Office.Interop.Excel.Range workrange;
        private Microsoft.Office.Interop.Excel.Worksheet copysheet;
        private Microsoft.Office.Interop.Excel.Worksheet pastesheet;

        string rowHeaderNum = string.Empty;

        WizMes_ParkPro.PopUp.NoticeMessage msg = new PopUp.NoticeMessage();

        // FTP 활용모음.
        string FullPath1 = string.Empty;
        string FullPath2 = string.Empty;

        private FTP_EX _ftp = null;
        List<string[]> listFtpFile = new List<string[]>();


        //string FTP_ADDRESS = "ftp://wizis.iptime.org/ImageData/AutoInspect";
        //string FTP_ADDRESS = "ftp://wizis.iptime.org/ImageData/AutoInspect";
        string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":" + LoadINI.FTPPort + LoadINI.FtpImagePath + "/AutoInspect";
        string FTP_ADDRESS_ARTICLE = "ftp://" + LoadINI.FileSvr + ":" + LoadINI.FTPPort + LoadINI.FtpImagePath + "/Article";
        //string FTP_ADDRESS = "ftp://222.104.222.145:25000/ImageData/AutoInspect";
        //string FTP_ADDRESS = "ftp://192.168.0.95/ImageData/AutoInspect";
        private const string FTP_ID = "wizuser";
        private const string FTP_PASS = "wiz9999";
        private const string LOCAL_DOWN_PATH = "C:\\Temp";

        public Win_Qul_InspectAuto_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            lib.UiLoading(sender);
            chkDate.IsChecked = true;
            btnToday_Click(null, null);
            SetComboBox();
            dtpInOutDate.SelectedDate = DateTime.Today;
            dtpInspectDate.SelectedDate = DateTime.Today;

            strPoint = "5"; // 출하검사로 시작

            tbnInspect.IsChecked = false;
            tbnIncomeInspect.IsChecked = false;
            tbnProcessCycle.IsChecked = false;
            tbnOutcomeInspect.IsChecked = true;

            SetControlsToggleChangedHidden();
            lblMilsheet.Visibility = Visibility.Hidden;
            txtMilSheetNo.Visibility = Visibility.Hidden;

            cboFML.SelectedIndex = 1;

            tbnOutcomeInspect_Click(null, null);
        }

        //
        private void SetComboBox()
        {
            ObservableCollection<CodeView> oveInspectGbn = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "INSPECTGBN", "Y", "", "");
            cboInspectGbn.ItemsSource = oveInspectGbn;
            cboInspectGbn.DisplayMemberPath = "code_name";
            cboInspectGbn.SelectedValuePath = "code_id";
            cboInspectGbn.SelectedIndex = 0;

            ObservableCollection<CodeView> oveInspectClss = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "INSPECTCLSS", "Y", "", "");
            cboInspectClss.ItemsSource = oveInspectClss;
            cboInspectClss.DisplayMemberPath = "code_name";
            cboInspectClss.SelectedValuePath = "code_id";
            cboInspectClss.SelectedIndex = 0;

            ObservableCollection<CodeView> oveIRELevel = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "INSDNGRLVL", "Y", "", "");
            cboIRELevel.ItemsSource = oveIRELevel;
            cboIRELevel.DisplayMemberPath = "code_name";
            cboIRELevel.SelectedValuePath = "code_id";

            ObservableCollection<CodeView> ovcProcess = ComboBoxUtil.Instance.GetWorkProcess(0, "");
            ovcProcess.RemoveAt(0); //여기서 전체는 빼고 추가해준다.
            cboProcess.ItemsSource = ovcProcess;
            cboProcess.DisplayMemberPath = "code_name";
            cboProcess.SelectedValuePath = "code_id";
            cboProcess.SelectedIndex = 0;

            ObservableCollection<CodeView> ovcMachineAutoMC = ComboBoxUtil.Instance.GetMachine(cboProcess.SelectedValue.ToString());
            this.cboMachine.ItemsSource = ovcMachineAutoMC;
            this.cboMachine.DisplayMemberPath = "code_name";
            this.cboMachine.SelectedValuePath = "code_id";

            List<string[]> strArrayValue = new List<string[]>();
            string[] strArrayOne = { "Y", "불합격" };
            string[] strArrayTwo = { "N", "합격" };
            strArrayValue.Add(strArrayOne);
            strArrayValue.Add(strArrayTwo);

            ObservableCollection<CodeView> ovcDefectYN = ComboBoxUtil.Instance.Direct_SetComboBox(strArrayValue);
            this.cboResultSrh.ItemsSource = ovcDefectYN;
            this.cboResultSrh.DisplayMemberPath = "code_name";
            this.cboResultSrh.SelectedValuePath = "code_id";

            this.cboDefectYN.ItemsSource = ovcDefectYN;
            this.cboDefectYN.DisplayMemberPath = "code_name";
            this.cboDefectYN.SelectedValuePath = "code_id";

            List<string[]> strArray = new List<string[]>();
            string[] strOne = { "1", "초" };
            string[] strTwo = { "2", "중" };
            string[] strThree = { "3", "종" };
            strArray.Add(strOne);
            strArray.Add(strTwo);
            strArray.Add(strThree);

            ObservableCollection<CodeView> ovcFML = ComboBoxUtil.Instance.Direct_SetComboBox(strArray);
            this.cboFML.ItemsSource = ovcFML;
            this.cboFML.DisplayMemberPath = "code_name";
            this.cboFML.SelectedValuePath = "code_id";
            this.cboFML.SelectedIndex = 0;
        }

        #region 상단 이벤트

        private void SetControlsToggleChangedVisible()
        {
            lblInOutCustom.Visibility = Visibility.Visible;
            lblInOutDate.Visibility = Visibility.Visible;
            txtInOutCustom.Visibility = Visibility.Visible;
            dtpInOutDate.Visibility = Visibility.Visible;
            btnPfInOutCustom.Visibility = Visibility.Visible;
        }

        private void SetControlsToggleChangedHidden()
        {
            lblInOutCustom.Visibility = Visibility.Hidden;
            lblInOutDate.Visibility = Visibility.Hidden;
            txtInOutCustom.Visibility = Visibility.Hidden;
            dtpInOutDate.Visibility = Visibility.Hidden;
            btnPfInOutCustom.Visibility = Visibility.Hidden;
        }

        //수입검사
        private void tbnIncomeInspect_Click(object sender, RoutedEventArgs e)
        {
            if (tbnIncomeInspect.IsChecked == true)
            {
                strPoint = "1";     //  1: 수입, 3:공정, 5:출하, 9:자주
                tbnProcessCycle.IsChecked = false;
                tbnInspect.IsChecked = false;
                tbnOutcomeInspect.IsChecked = false;

                SetControlsToggleChangedVisible();
                lblMilsheet.Visibility = Visibility.Visible;
                txtMilSheetNo.Visibility = Visibility.Visible;

                tbkInOutCustom.Text = "입고거래처";
                tbkInOutDate.Text = "입고일";

                cboFML.SelectedIndex = 0;

                //수입검사의 경우 공정과 호기를 선택하지 않아도 된다.
                lblProcess.Visibility = Visibility.Hidden;
                cboProcess.Visibility = Visibility.Hidden;
                lblMachine.Visibility = Visibility.Hidden;
                cboMachine.Visibility = Visibility.Hidden;

                btnPrint.Visibility = Visibility.Hidden;
            }
            else
            {
                tbnIncomeInspect.IsChecked = true;
            }
        }

        //공정순회
        private void tbnProcessCycle_Click(object sender, RoutedEventArgs e)
        {
            if (tbnProcessCycle.IsChecked == true)
            {
                strPoint = "3";    //  1: 수입, 3:공정, 5:출하, 9:자주
                tbnIncomeInspect.IsChecked = false;
                tbnInspect.IsChecked = false;
                tbnOutcomeInspect.IsChecked = false;

                SetControlsToggleChangedHidden();
                lblMilsheet.Visibility = Visibility.Hidden;
                txtMilSheetNo.Visibility = Visibility.Hidden;

                cboFML.SelectedIndex = 1;

                //공정순회의 경우 공정과 호기를 선택해야 하니까 .
                lblProcess.Visibility = Visibility.Visible;
                cboProcess.Visibility = Visibility.Visible;
                lblMachine.Visibility = Visibility.Visible;
                cboMachine.Visibility = Visibility.Visible;

                btnPrint.Visibility = Visibility.Hidden;
            }
            else
            {
                tbnProcessCycle.IsChecked = true;
            }
        }

        //자주검사
        private void tbnInspect_Click(object sender, RoutedEventArgs e)
        {
            if (tbnInspect.IsChecked == true)
            {
                strPoint = "9";     //  1: 수입, 3:공정, 5:출하, 9:자주
                tbnProcessCycle.IsChecked = false;
                tbnIncomeInspect.IsChecked = false;
                tbnOutcomeInspect.IsChecked = false;

                SetControlsToggleChangedHidden();
                lblMilsheet.Visibility = Visibility.Hidden;
                txtMilSheetNo.Visibility = Visibility.Hidden;

                cboFML.SelectedIndex = 0;


                //자주검사의 경우 공정과 호기를 선택해야 하니까 .
                lblProcess.Visibility = Visibility.Visible;
                cboProcess.Visibility = Visibility.Visible;
                lblMachine.Visibility = Visibility.Visible;
                cboMachine.Visibility = Visibility.Visible;

                btnPrint.Visibility = Visibility.Hidden;
            }
            else
            {
                tbnInspect.IsChecked = true;
            }
        }

        //출하검사
        private void tbnOutcomeInspect_Click(object sender, RoutedEventArgs e)
        {
            if (tbnOutcomeInspect.IsChecked == true)
            {
                strPoint = "5";     //  1: 수입, 3:공정, 5:출하, 9:자주
                tbnProcessCycle.IsChecked = false;
                tbnInspect.IsChecked = false;
                tbnIncomeInspect.IsChecked = false;

                SetControlsToggleChangedVisible();
                lblMilsheet.Visibility = Visibility.Hidden;
                txtMilSheetNo.Visibility = Visibility.Hidden;

                tbkInOutCustom.Text = "출고거래처";
                tbkInOutDate.Text = "출고일";

                cboFML.SelectedIndex = 2;


                //출하검사의 경우 공정과 호기를 선택하지 않는다.
                lblProcess.Visibility = Visibility.Hidden;
                cboProcess.Visibility = Visibility.Hidden;
                lblMachine.Visibility = Visibility.Hidden;
                cboMachine.Visibility = Visibility.Hidden;

                btnPrint.Visibility = Visibility.Visible;
            }
            else
            {
                tbnOutcomeInspect.IsChecked = true;
            }
        }

        //검사일자
        private void lblDate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkDate.IsChecked == true) { chkDate.IsChecked = false; }
            else { chkDate.IsChecked = true; }
        }

        //검사일자
        private void chkDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;
        }

        //검사일자
        private void chkDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }

        //금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = lib.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = lib.BringThisMonthDatetimeList()[1];
        }

        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        //품명
        private void txtArticleSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                MainWindow.pf.ReturnCode(txtArticleSrh, 77, txtArticleSrh.Text);
            }
        }

        //품명
        private void btnPFArticleSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh, 77, txtArticleSrh.Text);
        }

        //판정결과
        private void lblResultSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkResultSrh.IsChecked == true) { chkResultSrh.IsChecked = false; }
            else { chkResultSrh.IsChecked = true; }
        }

        //판정결과
        private void chkResultSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboResultSrh.IsEnabled = true;
        }

        //판정결과
        private void chkResultSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboResultSrh.IsEnabled = false;
        }

        //Lotid 유지추가
        private void lblRemainAddSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkRemainAddSrh.IsChecked == true) { chkRemainAddSrh.IsChecked = false; }
            else { chkRemainAddSrh.IsChecked = true; }
        }

        #endregion

        #region 상단 버튼 이벤트

        /// <summary>
        /// 수정,추가 저장 후
        /// </summary>
        private void CanBtnControl()
        {
            lib.UiButtonEnableChange_IUControl(this);
            //grdInput.IsEnabled = false;
            grdInput.IsHitTestVisible = false;
        }

        /// <summary>
        /// 수정,추가 진행 중
        /// </summary>
        private void CantBtnControl()
        {
            lib.UiButtonEnableChange_SCControl(this);
            //grdInput.IsEnabled = true;
            grdInput.IsHitTestVisible = true;
        }

        private void SetControlsWhenAdd()
        {
            dtpInOutDate.SelectedDate = DateTime.Today;
            dtpInspectDate.SelectedDate = DateTime.Today;
            cboProcess.SelectedIndex = 0;
            cboInspectGbn.SelectedIndex = 0;
            cboInspectClss.SelectedIndex = 0;
            cboFML.SelectedIndex = 0;
            txtInspectUserID.Text = MainWindow.CurrentPerson;
            txtInspectUserID.Tag = MainWindow.CurrentPersonID;
            txtArticleName.Text = "";
            txtArticleName.Tag = "";
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (chkRemainAddSrh.IsChecked == true)
            {
                WinInsAuto = dgdMain.SelectedItem as Win_Qul_InspectAuto_U_CodeView;

                if (WinInsAuto != null)
                {
                    CantBtnControl();
                    strFlag = "I";

                    lblMsg.Visibility = Visibility.Visible;
                    tbkMsg.Text = "자료 입력 중";

                    if (dgdMain.Items.Count > 0)
                    {
                        Wh_Ar_SelectedLastIndex = dgdMain.SelectedIndex;
                    }
                    else
                    {
                        Wh_Ar_SelectedLastIndex = 0;
                    }

                    dgdMain.IsHitTestVisible = false;
                    this.DataContext = null;
                    txtLotNO.Text = WinInsAuto.LotID;
                    SetControlsWhenAdd();
                }
                else
                {
                    MessageBox.Show("유지추가 항목을 먼저 선택해주세요");
                }
            }
            else
            {
                CantBtnControl();
                strFlag = "I";

                lblMsg.Visibility = Visibility.Visible;
                tbkMsg.Text = "자료 입력 중";

                if (dgdMain.Items.Count > 0)
                {
                    Wh_Ar_SelectedLastIndex = dgdMain.SelectedIndex;
                }
                else
                {
                    Wh_Ar_SelectedLastIndex = 0;
                }


                dgdMain.IsHitTestVisible = false;
                this.DataContext = null;
                SetControlsWhenAdd();

                //유지추가가 아니면 sub1 sub2 모두 비워줘야 한다.
                if (dgdSub1.Items.Count > 0)
                    dgdSub1.Items.Clear();

                if (dgdSub2.Items.Count > 0)
                    dgdSub2.Items.Clear();

                txtLotNO.Focus();
            }

            //이전 받아 온 데이터가 남아있어서 추가 누르면 비워주자. 
            cboEcoNO.ItemsSource = null;
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            WinInsAuto = dgdMain.SelectedItem as Win_Qul_InspectAuto_U_CodeView;

            if (WinInsAuto != null)
            {
                Wh_Ar_SelectedLastIndex = dgdMain.SelectedIndex;
                dgdMain.IsHitTestVisible = false;
                tbkMsg.Text = "자료 수정 중";
                lblMsg.Visibility = Visibility.Visible;
                CantBtnControl();
                strFlag = "U";
                txtInspectQty.Text = GetValueCount().ToString();
            }
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            using (Loading ld = new Loading(beDelete))
            {
                ld.ShowDialog();
            }
        }

        private void beDelete()
        {
            btnDelete.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (listLotLabelPrint.Count == 0)
                {
                    MessageBox.Show("삭제할 데이터가 지정되지 않았습니다. 삭제 데이터를 지정하고 눌러주세요.");
                }
                else
                {
                    if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        foreach (Win_Qul_InspectAuto_U_CodeView RemoveData in listLotLabelPrint)
                            DeleteData(RemoveData.InspectID);

                        Wh_Ar_SelectedLastIndex = 0;
                        re_Search(Wh_Ar_SelectedLastIndex);
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnDelete.IsEnabled = true;
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            int i = 0;
            foreach (MenuViewModel mvm in MainWindow.mMenulist)
            {
                if (mvm.subProgramID.ToString().Contains("MDI"))
                {
                    if (this.ToString().Equals((mvm.subProgramID as MdiChild).Content.ToString()))
                    {
                        (MainWindow.mMenulist[i].subProgramID as MdiChild).Close();
                        break;
                    }
                }
                i++;
            }
        }

        #region 검사성적서 이벤트

        //검사성적서...
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = btnPrint.ContextMenu;
            menu.StaysOpen = true;
            menu.IsOpen = true;
        }

        //인쇄 미리보기
        private void menuSeeAhead_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMain.Items.Count < 1)
            {
                MessageBox.Show("먼저 검색해 주세요.");
                return;
            }
            else
            {
                if (dgdMain.SelectedItem == null)
                {
                    MessageBox.Show("인쇄할 대상을 선택하세요.");
                    return;
                }
                else
                {
                    WinInsAuto = dgdMain.SelectedItem as Win_Qul_InspectAuto_U_CodeView;

                    if (WinInsAuto == null)
                    {
                        MessageBox.Show("정상적인 검사성적서가 아닙니다.");
                        return;
                    }
                }
            }

            msg.Show();
            msg.Topmost = true;
            msg.Refresh();

            //PrintWork(true);
        }

        //인쇄 바로
        private void menuRightPrint_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMain.Items.Count < 1)
            {
                MessageBox.Show("먼저 검색해 주세요.");
                return;
            }
            else
            {
                if (dgdMain.SelectedItem == null)
                {
                    MessageBox.Show("인쇄할 대상을 선택하세요.");
                    return;
                }
                else
                {
                    WinInsAuto = dgdMain.SelectedItem as Win_Qul_InspectAuto_U_CodeView;

                    if (WinInsAuto == null)
                    {
                        MessageBox.Show("정상적인 검사성적서가 아닙니다.");
                        return;
                    }
                }
            }
            DataStore.Instance.InsertLogByForm(this.GetType().Name, "P");
            msg.Show();
            msg.Topmost = true;
            msg.Refresh();

            //PrintWork(false);
        }

        //인쇄 닫기
        private void menuClose_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu menu = btnPrint.ContextMenu;
            menu.StaysOpen = false;
            menu.IsOpen = false;
        }

        //인쇄 실질 동작
        /*private void PrintWork(bool preview_click)
        {
            excelapp = new Microsoft.Office.Interop.Excel.Application();

            string MyBookPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\Report\\검사성적서(출하).xls";
            workbook = excelapp.Workbooks.Add(MyBookPath);
            worksheet = workbook.Sheets["Form"];
            pastesheet = workbook.Sheets["Report"];

            var InspectInfo = dgdMain.SelectedItem as Win_Qul_InspectAuto_U_CodeView;
            var InspectInfoSub1 = dgdSub1.SelectedItem as Win_Qul_InspectAuto_U_Sub_CodeView;
            var IIS = InspectInfo.InspectQty;

            int copyLine = 0;
            int insertline = 0;

            //작성일
            workrange = worksheet.get_Range("AJ3", "AQ3");//셀 범위 지정
            workrange.Value2 = DateTime.Now.ToString("yyyy년 MM월 dd일");
            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

            //품명
            workrange = worksheet.get_Range("E7", "O7");//셀 범위 지정
            workrange.Value2 = "HK스틸";
            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

            //품명
            workrange = worksheet.get_Range("E5", "O5");//셀 범위 지정
            workrange.Value2 = InspectInfo.Article.ToString();
            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

            //품번
            workrange = worksheet.get_Range("T5", "AC5");//셀 범위 지정
            workrange.Value2 = InspectInfo.BuyerArticleNo.ToString();
            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

            //차종
            workrange = worksheet.get_Range("T7", "AC7");//셀 범위 지정
            workrange.Value2 = InspectInfo.BuyerModel.ToString();
            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

            //LOT NO
            workrange = worksheet.get_Range("E9", "O9");//셀 범위 지정
            workrange.Value2 = InspectInfo.LotID.ToString();
            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

            //출고 수량
            workrange = worksheet.get_Range("AJ15", "AQ15");//셀 범위 지정
            workrange.Value2 = InspectInfo.SumInspectQty + "EA";
            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

            //샘플 수량
            workrange = worksheet.get_Range("AJ23", "AM23");//셀 범위 지정
            workrange.Value2 = (InspectInfoSub1 != null ? InspectInfoSub1.InsSampleQty : "");  // 왜 null이라는 걸까
            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;


            for (int j = 0; j < dgdSub2.Items.Count; j++)
            {
                var WinInsAutoSub2 = dgdSub2.Items[j] as Win_Qul_InspectAuto_U_Sub_CodeView;

                //System.Diagnostics.Debug.WriteLine("==========-=-=-=-= " + WinInsAutoSub1.InspectValue1.ToString());

                if (returnYN(WinInsAutoSub2) == false)
                {
                    //DFCount 값을 구하기 위해 그냥 일단 태우자                       
                }
                else
                {
                    //true면.. 불량이 없다는 거니까 불량 수 늘려 줄 필요가 없지요?
                }
            }

            int count = 0;

            //리스트에 있는 외관 값이 양호가 아닌 경우(검사실적서에 5개 값까지 밖에 없으니까...거기까지만 비교)
            for (int i = 0; i < defectCheck1.Count; i++)
            {
                if (!defectCheck1[i][19].ToString().Equals("양호") && !defectCheck1[i][19].ToString().Equals(""))
                {
                    if (!DFCount1.Equals(1))
                    {
                        count += 1;
                    }
                }
                if (!defectCheck1[i][20].ToString().Equals("양호") && !defectCheck1[i][20].ToString().Equals(""))
                {
                    if (!DFCount2.Equals(1))
                    {
                        count += 1;
                    }
                }
                if (!defectCheck1[i][21].ToString().Equals("양호") && !defectCheck1[i][21].ToString().Equals(""))
                {
                    if (!DFCount3.Equals(1))
                    {
                        count += 1;
                    }
                }
                if (!defectCheck1[i][22].ToString().Equals("양호") && !defectCheck1[i][22].ToString().Equals(""))
                {
                    if (!DFCount4.Equals(1))
                    {
                        count += 1;
                    }
                }
                if (!defectCheck1[i][23].ToString().Equals("양호") && !defectCheck1[i][23].ToString().Equals(""))
                {
                    if (!DFCount5.Equals(1))
                    {
                        count += 1;
                    }
                }
            }

            //샘플 중 불량 수량
            int total = count + DFCount1 + DFCount2 + DFCount3 + DFCount4 + DFCount5;

            //불량수
            workrange = worksheet.get_Range("AN23", "AQ23");//셀 범위 지정
            workrange.Value2 = total;
            workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;


            int NumCount = 0;
            NumCount = dgdSub1.Items.Count + dgdSub2.Items.Count;
            //MessageBox.Show(NumCount + "건");

            insertline = 35;

            for (int i = 0; i < NumCount; i++)
            {
                workrange = worksheet.get_Range("A" + (insertline + i), "B" + (insertline + i));//셀 범위 지정
                workrange.Value2 = i + 1;
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            }


            for (int i = 0; i < dgdSub1.Items.Count; i++)
            {
                WinInsAutoSub = dgdSub1.Items[i] as Win_Qul_InspectAuto_U_Sub_CodeView;

                insertline = 35;

                //검사항목
                workrange = worksheet.get_Range("C" + Convert.ToInt32(insertline + i), "F" + Convert.ToInt32(insertline + i));
                if (WinInsAutoSub.insType.Trim().Equals("1"))
                {
                    workrange.Value2 = "외관";
                }
                else
                {
                    workrange.Value2 = "DIM'S";
                }
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //규격
                workrange = worksheet.get_Range("G" + Convert.ToInt32(insertline + i), "O" + Convert.ToInt32(insertline + i));
                workrange.Value2 = WinInsAutoSub.insItemName;
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //외관1
                workrange = worksheet.get_Range("P" + Convert.ToInt32(insertline + i), "Q" + Convert.ToInt32(insertline + i));    //외관1
                workrange.Value2 = WinInsAutoSub.arrInspectText[0];
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //외관2
                workrange = worksheet.get_Range("R" + Convert.ToInt32(insertline + i), "S" + Convert.ToInt32(insertline + i));    //외관2
                workrange.Value2 = WinInsAutoSub.arrInspectText[1];
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //외관3
                workrange = worksheet.get_Range("T" + Convert.ToInt32(insertline + i), "U" + Convert.ToInt32(insertline + i));    //외관3
                workrange.Value2 = WinInsAutoSub.arrInspectText[2];
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //외관4
                workrange = worksheet.get_Range("V" + Convert.ToInt32(insertline + i), "W" + Convert.ToInt32(insertline + i));    //외관4
                workrange.Value2 = WinInsAutoSub.arrInspectText[3];
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //외관5
                workrange = worksheet.get_Range("X" + Convert.ToInt32(insertline + i), "Y" + Convert.ToInt32(insertline + i));    //외관5
                workrange.Value2 = WinInsAutoSub.arrInspectText[4];
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //판정
                workrange = worksheet.get_Range("Z" + Convert.ToInt32(insertline + i), "AC" + Convert.ToInt32(insertline + i));    //판정

                for (int j = 0; j < defectCheck1.Count; j++)
                {
                    if (!defectCheck1[i][19].ToString().Equals("양호") && !defectCheck1[i][19].ToString().Equals(""))
                        workrange.Value2 = "불";
                    else if (!defectCheck1[i][20].ToString().Equals("양호") && !defectCheck1[i][20].ToString().Equals(""))
                        workrange.Value2 = "불";
                    else if (!defectCheck1[i][21].ToString().Equals("양호") && !defectCheck1[i][21].ToString().Equals(""))
                        workrange.Value2 = "불";
                    else if (!defectCheck1[i][22].ToString().Equals("양호") && !defectCheck1[i][22].ToString().Equals(""))
                        workrange.Value2 = "불";
                    else if (!defectCheck1[i][23].ToString().Equals("양호") && !defectCheck1[i][23].ToString().Equals(""))
                        workrange.Value2 = "불";
                    else
                        workrange.Value2 = "합";

                    workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                }
            }
            for (int j = 0; j < dgdSub2.Items.Count; j++)
            {
                var WinInsAutoSub2 = dgdSub2.Items[j] as Win_Qul_InspectAuto_U_Sub_CodeView;

                insertline = 36;

                //검사항목
                workrange = worksheet.get_Range("C" + Convert.ToInt32(insertline + j), "F" + Convert.ToInt32(insertline + j));
                if (WinInsAutoSub2.insType.Trim().Equals("1"))
                {
                    workrange.Value2 = "외관";
                }
                else
                {
                    workrange.Value2 = "DIM'S";
                }

                //규격
                workrange = worksheet.get_Range("I" + Convert.ToInt32(insertline + j), "O" + Convert.ToInt32(insertline + j));
                workrange.Value2 = WinInsAutoSub2.insItemName;
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //정량적검사1
                workrange = worksheet.get_Range("P" + Convert.ToInt32(insertline + j), "Q" + Convert.ToInt32(insertline + j));
                workrange.Value2 = WinInsAutoSub2.arrInspectValue[0];
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //정량적검사1
                workrange = worksheet.get_Range("R" + Convert.ToInt32(insertline + j), "S" + Convert.ToInt32(insertline + j));
                workrange.Value2 = WinInsAutoSub2.arrInspectValue[1];
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //정량적검사3
                workrange = worksheet.get_Range("T" + Convert.ToInt32(insertline + j), "U" + Convert.ToInt32(insertline + j));
                workrange.Value2 = WinInsAutoSub2.arrInspectValue[2];
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //정량적검사4
                workrange = worksheet.get_Range("V" + Convert.ToInt32(insertline + j), "W" + Convert.ToInt32(insertline + j));
                workrange.Value2 = WinInsAutoSub2.arrInspectValue[3];
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                //정량적검사5
                workrange = worksheet.get_Range("X" + Convert.ToInt32(insertline + j), "Y" + Convert.ToInt32(insertline + j));
                workrange.Value2 = WinInsAutoSub2.arrInspectValue[4];
                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

                workrange = worksheet.get_Range("Z" + Convert.ToInt32(insertline + j), "AC" + Convert.ToInt32(insertline + j));    //판정

                if (returnYN(WinInsAutoSub2))
                    workrange.Value2 = "합";
                else
                    workrange.Value2 = "불";

                workrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            }

            // Form 시트 내용 Print 시트에 복사 붙여넣기
            worksheet.Select();
            worksheet.UsedRange.EntireRow.Copy();
            pastesheet.Select();
            workrange = pastesheet.Cells[copyLine + 1, 1];
            workrange.Select();
            pastesheet.Paste();

            pastesheet.UsedRange.EntireRow.Select();
            msg.Hide();

            if (preview_click == true)      //미리보기 버튼이 클릭이라면
            {
                excelapp.Visible = true;
                pastesheet.PrintPreview();
            }
            else
            {
                excelapp.Visible = true;
                pastesheet.PrintOutEx();
            }


        }*/

        //
        /*private bool returnYN(Win_Qul_InspectAuto_U_Sub_CodeView WinInsAutoSubCodeView)
        {
            bool flag = false;

            if (!WinInsAutoSubCodeView.InspectValue1.Equals(string.Empty))
            {
                if (lib.IsNumOrAnother(WinInsAutoSubCodeView.InspectValue1))
                {
                    if (double.Parse(WinInsAutoSubCodeView.InspectValue1) >= double.Parse(WinInsAutoSubCodeView.SpecMin) &&
                        double.Parse(WinInsAutoSubCodeView.InspectValue1) <= double.Parse(WinInsAutoSubCodeView.SpecMax))
                    {
                        flag = true;
                    }
                    else
                    {
                        DFCount1 = 1;
                        return false;
                    }
                }
            }
            if (!WinInsAutoSubCodeView.InspectValue2.Equals(string.Empty))
            {
                if (lib.IsNumOrAnother(WinInsAutoSubCodeView.InspectValue2))
                {
                    if (double.Parse(WinInsAutoSubCodeView.InspectValue2) >= double.Parse(WinInsAutoSubCodeView.SpecMin) &&
                        double.Parse(WinInsAutoSubCodeView.InspectValue2) <= double.Parse(WinInsAutoSubCodeView.SpecMax))
                    {
                        flag = true;
                    }
                    else
                    {
                        DFCount2 = 1;
                        return false;
                    }
                }
            }
            if (!WinInsAutoSubCodeView.InspectValue3.Equals(string.Empty))
            {
                if (lib.IsNumOrAnother(WinInsAutoSubCodeView.InspectValue3))
                {
                    if (double.Parse(WinInsAutoSubCodeView.InspectValue3) >= double.Parse(WinInsAutoSubCodeView.SpecMin) &&
                        double.Parse(WinInsAutoSubCodeView.InspectValue3) <= double.Parse(WinInsAutoSubCodeView.SpecMax))
                    {
                        flag = true;
                    }
                    else
                    {
                        DFCount3 = 1;
                        return false;
                    }
                }
            }
            if (!WinInsAutoSubCodeView.InspectValue4.Equals(string.Empty))
            {
                if (lib.IsNumOrAnother(WinInsAutoSubCodeView.InspectValue4))
                {
                    if (double.Parse(WinInsAutoSubCodeView.InspectValue4) >= double.Parse(WinInsAutoSubCodeView.SpecMin) &&
                        double.Parse(WinInsAutoSubCodeView.InspectValue4) <= double.Parse(WinInsAutoSubCodeView.SpecMax))
                    {
                        flag = true;
                    }
                    else
                    {
                        DFCount4 = 1;
                        return false;
                    }
                }
            }
            if (!WinInsAutoSubCodeView.InspectValue5.Equals(string.Empty))
            {
                if (lib.IsNumOrAnother(WinInsAutoSubCodeView.InspectValue5))
                {
                    if (double.Parse(WinInsAutoSubCodeView.InspectValue5) >= double.Parse(WinInsAutoSubCodeView.SpecMin) &&
                        double.Parse(WinInsAutoSubCodeView.InspectValue5) <= double.Parse(WinInsAutoSubCodeView.SpecMax))
                    {
                        flag = true;
                    }
                    else
                    {
                        DFCount5 = 1;
                        return false;
                    }
                }
            }

            return flag;
        }*/

        #endregion

        //검색
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            using (Loading ld = new Loading(beSearch))
            {
                ld.ShowDialog();
            }
        }

        private void beSearch()
        {
            //검색버튼 비활성화
            btnSearch.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //로직
                clear();
                Wh_Ar_SelectedLastIndex = 0;
                re_Search(Wh_Ar_SelectedLastIndex);

            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSearch.IsEnabled = true;
        }

        //저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            using (Loading ld = new Loading(beSave))
            {
                ld.ShowDialog();
            }
        }

        private void beSave()
        {
            //저장버튼 비활성화
            btnSave.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //로직
                if (SaveData(strFlag))
                {
                    CanBtnControl();
                    lblMsg.Visibility = Visibility.Hidden;
                    dgdMain.IsHitTestVisible = true;

                    if (strFlag == "I")     //1. 추가 > 저장했다면,
                    {
                        if (dgdMain.Items.Count > 0)
                        {
                            re_Search(dgdMain.Items.Count - 1);
                            dgdMain.Focus();
                        }
                        else
                        { re_Search(0); }
                    }
                    else        //2. 수정 > 저장했다면,
                    {
                        re_Search(Wh_Ar_SelectedLastIndex);
                        dgdMain.Focus();

                        dgdSub1.SelectedIndex = 0;
                    }

                    strFlag = string.Empty;  // 추가했는지, 수정했는지 알려면 맨 마지막에 flag 값을 비워야 한다.
                }

            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSave.IsEnabled = true;
        }

        //취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            clear();
            CanBtnControl();

            if (strFlag == "I") // 1. 추가하다가 취소했다면,
            {
                if (dgdMain.Items.Count > 0)
                {
                    re_Search(Wh_Ar_SelectedLastIndex);
                    dgdMain.Focus();
                }
                else
                { re_Search(0); }
            }
            else        //2. 수정하다가 취소했다면
            {
                re_Search(Wh_Ar_SelectedLastIndex);
                dgdMain.Focus();
            }

            strFlag = string.Empty;
            //dgdMain.IsEnabled = true;
            dgdMain.IsHitTestVisible = true;
        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[6];
            lst[0] = "검사성적";
            lst[1] = "외관 검사성적";
            lst[2] = "Dims 검사성적";
            lst[3] = dgdMain.Name;
            lst[4] = dgdSub1.Name;
            lst[5] = dgdSub2.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);
            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdMain.Name))
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                    if (ExpExc.Check.Equals("Y"))
                        dt = lib.DataGridToDTinHidden(dgdMain);
                    else
                        dt = lib.DataGirdToDataTable(dgdMain);

                    Name = dgdMain.Name;

                    if (lib.GenerateExcel(dt, Name))
                    {
                        lib.excel.Visible = true;
                        lib.ReleaseExcelObject(lib.excel);
                    }
                }
                else if (ExpExc.choice.Equals(dgdSub1.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = lib.DataGridToDTinHidden(dgdSub1);
                    else
                        dt = lib.DataGirdToDataTable(dgdSub1);

                    Name = dgdSub1.Name;
                    if (lib.GenerateExcel(dt, Name))
                    {
                        lib.excel.Visible = true;
                        lib.ReleaseExcelObject(lib.excel);
                    }
                }
                else if (ExpExc.choice.Equals(dgdSub2.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = lib.DataGridToDTinHidden(dgdSub2);
                    else
                        dt = lib.DataGirdToDataTable(dgdSub2);

                    Name = dgdSub2.Name;

                    if (lib.GenerateExcel(dt, Name))
                    {
                        lib.excel.Visible = true;
                        lib.ReleaseExcelObject(lib.excel);
                    }
                }
                else
                {
                    if (dt != null)
                    {
                        dt.Clear();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 재검색(수정,삭제,추가 저장후에 자동 재검색)
        /// </summary>
        /// <param name="selectedIndex"></param>
        private void re_Search(int selectedIndex)
        {
            listLotLabelPrint.Clear();

            FillGrid();

            if (dgdMain.Items.Count > 0)
            {
                dgdMain.Focus();
                dgdMain.SelectedIndex = selectedIndex;
                dgdMain.CurrentCell = dgdMain.SelectedCells.Count > 0 ? dgdMain.SelectedCells[0] : new DataGridCellInfo();
            }
        }

        /// <summary>
        /// 실조회
        /// </summary>
        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
                dgdMain.Items.Clear();

            if (dgdSub1.Items.Count > 0)
                dgdSub1.Items.Clear();

            if (dgdSub2.Items.Count > 0)
                dgdSub2.Items.Clear();

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("InspectPoint", strPoint);
                sqlParameter.Add("FromDate", chkDate.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("ToDate", chkDate.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("nchkDefectYN", chkResultSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("sDefectYN", chkResultSrh.IsChecked == true ? cboResultSrh.SelectedValue.ToString() : "");
                //sqlParameter.Add("BuyerArticleNo", chkArticleNo.IsChecked == true ? txtArticleNo.Text : "");

                sqlParameter.Add("ChkBuyerArticle", chkArticleSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("BuyerArticle", chkArticleSrh.IsChecked == true && !txtArticleSrh.Text.Trim().Equals("") ? txtArticleSrh.Text : "");
                ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_Inspect_sAutoInspect", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinQulInsAuto = new Win_Qul_InspectAuto_U_CodeView()
                            {
                                Num = i + 1,
                                Article = dr["Article"].ToString(),
                                ArticleID = dr["ArticleID"].ToString(),
                                AttachedFile = dr["AttachedFile"].ToString(),
                                AttachedPath = dr["AttachedPath"].ToString(),
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                BuyerModel = dr["BuyerModel"].ToString(),
                                BuyerModelID = dr["BuyerModelID"].ToString(),
                                Comments = dr["Comments"].ToString(),
                                DefectYN = dr["DefectYN"].ToString(),
                                ECONo = dr["ECONo"].ToString(),
                                FMLGubun = dr["FMLGubun"].ToString(),
                                FMLGubunName = dr["FMLGubunName"].ToString(),
                                ImportImpYN = dr["ImportImpYN"].ToString(),
                                ImportlawYN = dr["ImportlawYN"].ToString(),
                                ImportNorYN = dr["ImportNorYN"].ToString(),
                                ImportSecYN = dr["ImportSecYN"].ToString(),
                                InpCustomID = dr["InpCustomID"].ToString(),
                                InpCustomName = dr["InpCustomName"].ToString(),
                                InpDate = dr["InpDate"].ToString().Replace(" ", ""),
                                InspectBasisID = dr["InspectBasisID"].ToString(),
                                InspectDate = dr["InspectDate"].ToString().Replace(" ", ""),
                                InspectGubun = dr["InspectGubun"].ToString(),
                                InspectID = dr["InspectID"].ToString(),
                                InspectLevel = dr["InspectLevel"].ToString(),
                                InspectPoint = dr["InspectPoint"].ToString(),
                                InspectQty = dr["InspectQty"].ToString(),
                                InspectUserID = dr["InspectUserID"].ToString(),
                                IRELevel = dr["IRELevel"].ToString(),
                                IRELevelName = dr["IRELevelName"].ToString(),
                                LotID = dr["LotID"].ToString().Trim(),
                                MachineID = dr["MachineID"].ToString(),
                                MilSheetNo = dr["MilSheetNo"].ToString(),
                                Name = dr["Name"].ToString(),
                                OutCustomID = dr["OutCustomID"].ToString(),
                                OutCustomName = dr["OutCustomName"].ToString(),
                                OutDate = dr["OutDate"].ToString().Replace(" ", ""),
                                Process = dr["Process"].ToString(),
                                ProcessID = dr["ProcessID"].ToString(),
                                SketchFile = dr["SketchFile"].ToString(),
                                SketchPath = dr["SketchPath"].ToString(),
                                TotalDefectQty = dr["TotalDefectQty"].ToString(),
                                SumInspectQty = dr["SumInspectQty"].ToString(),
                                SumDefectQty = dr["SumDefectQty"].ToString(),
                                INOUTCustomID = "",
                                InOutCustom = "",
                                INOUTCustomDate = ""
                            };

                            //if (WinQulInsAuto.SumInspectQty.Trim().Length > 0 && lib.IsNumOrAnother(WinQulInsAuto.SumInspectQty.Trim()))
                            //{
                            //    WinQulInsAuto.SumInspectQty = string.Format("{0:N0}", double.Parse(WinQulInsAuto.SumInspectQty.Trim()));
                            //}

                            if (WinQulInsAuto.InpDate.Length > 0)
                                WinQulInsAuto.InpDate_CV = lib.StrDateTimeBar(WinQulInsAuto.InpDate);

                            if (WinQulInsAuto.InspectDate.Length > 0)
                                WinQulInsAuto.InspectDate_CV = lib.StrDateTimeBar(WinQulInsAuto.InspectDate);

                            if (WinQulInsAuto.OutDate.Length > 0)
                                WinQulInsAuto.OutDate_CV = lib.StrDateTimeBar(WinQulInsAuto.OutDate);

                            if (strPoint.Equals("1"))
                            {
                                if (WinQulInsAuto.InpCustomID.Replace(" ", "").Length > 0)
                                {
                                    WinQulInsAuto.INOUTCustomID = WinQulInsAuto.InpCustomID;
                                    WinQulInsAuto.InOutCustom = WinQulInsAuto.InpCustomName;
                                }

                                if (string.IsNullOrEmpty(WinQulInsAuto.InpDate_CV) == false)
                                {
                                    WinQulInsAuto.INOUTCustomDate = WinQulInsAuto.InpDate_CV;
                                    dtpInOutDate.SelectedDate = lib.strConvertDate(WinQulInsAuto.InpDate);
                                }
                            }
                            else if (strPoint.Equals("5"))
                            {
                                if (WinQulInsAuto.OutCustomID.Replace(" ", "").Length > 0)
                                {
                                    WinQulInsAuto.INOUTCustomID = WinQulInsAuto.OutCustomID;
                                    WinQulInsAuto.InOutCustom = WinQulInsAuto.OutCustomName;
                                }

                                if (string.IsNullOrEmpty(WinQulInsAuto.OutDate_CV) == false)
                                {
                                    WinQulInsAuto.INOUTCustomDate = WinQulInsAuto.OutDate_CV;
                                    dtpInOutDate.SelectedDate = lib.strConvertDate(WinQulInsAuto.OutDate);
                                }
                            }

                            dgdMain.Items.Add(WinQulInsAuto);
                            i++;
                        }

                        tbkIndexCount.Text = "▶검색결과 : " + i + " 건";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        //메인 그리드 선택시
        private void dgdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string tmpBasisID = string.Empty;
                string tmpMachineID = string.Empty;
                WinInsAuto = dgdMain.SelectedItem as Win_Qul_InspectAuto_U_CodeView;

                if (WinInsAuto != null)
                {
                    tmpBasisID = WinInsAuto.InspectBasisID;
                    tmpMachineID = WinInsAuto.MachineID;

                    txtArticleName.Tag = WinInsAuto.ArticleID;

                    this.DataContext = WinInsAuto;


                    SetEcoNoCombo(WinInsAuto.ArticleID, strPoint);

                    if (cboEcoNO.Items.Count > 0)
                    {
                        cboEcoNO.SelectedValue = tmpBasisID;

                        if (cboEcoNO.SelectedValue != null)
                            strBasisID = cboEcoNO.SelectedValue.ToString();
                    }

                    cboProcess_SelectionChanged(null, null);
                    if (!tmpMachineID.Equals(string.Empty))
                    {
                        //cboMachine.SelectedValue = WinInsAuto.MachineID;
                        cboMachine.SelectedValue = tmpMachineID;
                    }

                    FillGridSub(WinInsAuto.InspectID, "1");
                    FillGridSub(WinInsAuto.InspectID, "2");

                    dgdSub1.SelectedIndex = 0;

                    txtInspectQty.Text = GetValueCount().ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //
        private void FillGridSub(string strID, string strType)
        {
            if (strType.Equals("1"))
            {
                if (dgdSub1.Items.Count > 0)
                    dgdSub1.Items.Clear();
            }
            else if (strType.Equals("2"))
            {
                if (dgdSub2.Items.Count > 0)
                    dgdSub2.Items.Clear();
            }

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("InspectID", strID);
                sqlParameter.Add("InsType", strType);
                ds = DataStore.Instance.ProcedureToDataSet("xp_Inspect_sAutoInspectSub", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int idx = 0;

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinQulInsAutoSub = new Win_Qul_InspectAuto_U_Sub_CodeView()
                            {
                                Num = idx + 1,
                                InspectBasisID = dr["InspectBasisID"].ToString(),
                                Seq = dr["Seq"].ToString(),
                                SubSeq = dr["SubSeq"].ToString(),
                                insType = dr["insType"].ToString(),
                                insItemName = dr["insItemName"].ToString(),
                                SpecMin = lib.returnNumStringTwo(dr["SpecMin"].ToString()),
                                SpecMax = lib.returnNumStringTwo(dr["SpecMax"].ToString()),
                                InsTPSpecMin = dr["InsTPSpecMin"].ToString(),
                                InsTPSpecMax = dr["InsTPSpecMax"].ToString(),
                                InsSampleQty = dr["InsSampleQty"].ToString(),
                                insSpec = dr["insSpec"].ToString(),
                                R = dr["R"].ToString(),
                                Sigma = "",  //dr["Sigma"].ToString(),
                                xBar = dr["xBar"].ToString(),
                            };

                            for (int i = 0; i < 10; i++)
                            {
                                int num = i + 1;
                                WinQulInsAutoSub.arrInspectValue[i] = lib.returnNumStringTwo(dr["InspectValue" + num.ToString()].ToString());
                                WinQulInsAutoSub.arrInspectText[i] = dr["InspectText" + num.ToString()].ToString();
                            }

                            if (strType.Equals("1"))
                            {
                                dgdSub1.Items.Add(WinQulInsAutoSub);

                                defectCheck1.Clear(); //이전에 들어있던 데이터는 지우고 추가해보자
                                defectCheck1.Add(dr);
                            }
                            else if (strType.Equals("2"))
                            {
                                double maxValue = WinQulInsAutoSub.SpecMax.Equals("") ? 0.0 : Convert.ToDouble(WinQulInsAutoSub.SpecMax);
                                double minValue = WinQulInsAutoSub.SpecMin.Equals("") ? 0.0 : Convert.ToDouble(WinQulInsAutoSub.SpecMin);

                                for (int i = 0; i < WinQulInsAutoSub.arrInspectValue.Length; i++)
                                {
                                    string inspectValue = WinQulInsAutoSub.arrInspectValue[0];
                                    double value = inspectValue.Equals("") ? 0.0 : Convert.ToDouble(inspectValue);

                                    if (!(value >= minValue && value <= maxValue))
                                        WinQulInsAutoSub.arrValueDefect[0] = "true";
                                }

                                dgdSub2.Items.Add(WinQulInsAutoSub);

                                defectCheck2.Clear(); //이전에 들어있던 데이터는 지우고 추가해보자
                                defectCheck2.Add(dr);
                            }

                            WinQulInsAutoSub.RefreshTextBlock(0, WinQulInsAutoSub.arrInspectValue);
                            WinQulInsAutoSub.RefreshTextBlock(1, WinQulInsAutoSub.arrInspectText);
                            WinQulInsAutoSub.RefreshTextBlock(2, WinQulInsAutoSub.arrValueDefect);

                            idx++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        /// <summary>
        /// 실삭제
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        private bool DeleteData(string strID)
        {
            bool flag = false;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("InspectID", strID);

                string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_Inspect_DAutoInspect", sqlParameter, "D");

                if (result[0].Equals("success"))
                {
                    //MessageBox.Show("성공 *^^*");
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

            return flag;
        }

        /// <summary>
        /// 저장
        /// </summary>
        /// <param name="strFlag"></param>
        /// <returns></returns>
        private bool SaveData(string strFlag)
        {
            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                string strID = strFlag.Equals("I") ? "" : (string.IsNullOrEmpty(txtinspectID.Text) ? "" : txtinspectID.Text);

                if (CheckData())
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "C");

                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Clear();
                    sqlParameter.Add("InspectID", strID);
                    sqlParameter.Add("ArticleID", txtArticleName.Tag.ToString());
                    sqlParameter.Add("InspectGubun", cboInspectGbn.SelectedValue.ToString());
                    sqlParameter.Add("InspectDate", dtpInspectDate.SelectedDate.Value.ToString("yyyyMMdd"));
                    sqlParameter.Add("LotID", txtLotNO.Text);

                    sqlParameter.Add("InspectQty", lib.CheckNullZero(txtInspectQty.Text));
                    sqlParameter.Add("ECONo", cboEcoNO.SelectedValue != null ? cboEcoNO.SelectedValue.ToString() : "");
                    sqlParameter.Add("Comments", txtComments.Text);
                    sqlParameter.Add("InspectLevel", cboInspectClss.SelectedValue.ToString());
                    sqlParameter.Add("SketchPath", "");  // txtSKetch.Tag != null ? txtSKetch.Tag.ToString() :

                    sqlParameter.Add("SketchFile", "");
                    sqlParameter.Add("AttachedPath", "");  //txtFile.Tag !=null ? txtFile.Tag.ToString() :
                    sqlParameter.Add("AttachedFile", "");
                    sqlParameter.Add("InspectUserID", txtInspectUserID.Tag.ToString());
                    //sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                    sqlParameter.Add("sInspectBasisID", strBasisID);
                    sqlParameter.Add("InspectBasisIDSeq", BasisSeq);
                    sqlParameter.Add("sDefectYN", cboDefectYN.SelectedValue == null ? "" : cboDefectYN.SelectedValue.ToString());
                    sqlParameter.Add("sProcessID", cboProcess.SelectedValue == null ? "" : cboProcess.SelectedValue.ToString());
                    sqlParameter.Add("InspectPoint", strPoint);

                    sqlParameter.Add("ImportSecYN", chkImportSecYN.IsChecked == true ? "Y" : "N");
                    sqlParameter.Add("ImportlawYN", chkImportSecYN.IsChecked == true ? "Y" : "N");
                    sqlParameter.Add("ImportImpYN", chkImportSecYN.IsChecked == true ? "Y" : "N");
                    sqlParameter.Add("ImportNorYN", chkImportSecYN.IsChecked == true ? "Y" : "N");
                    sqlParameter.Add("IRELevel", cboIRELevel.SelectedValue != null ?
                        cboIRELevel.SelectedValue.ToString() : "");

                    sqlParameter.Add("InpCustomID", (strPoint.Equals("1") && txtInOutCustom.Tag != null) ? txtInOutCustom.Tag.ToString() : "");
                    sqlParameter.Add("InpDate", strPoint.Equals("1") ?
                        dtpInOutDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                    sqlParameter.Add("OutCustomID", (strPoint.Equals("5") && txtInOutCustom.Tag != null) ? txtInOutCustom.Tag.ToString() : "");
                    sqlParameter.Add("OutDate", strPoint.Equals("5") ?
                        dtpInOutDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                    sqlParameter.Add("MachineID", cboMachine.SelectedValue != null ?
                        cboMachine.SelectedValue.ToString() : "");

                    sqlParameter.Add("BuyerModelID", txtBuyerModel.Tag != null ? txtBuyerModel.Tag.ToString() : "");
                    sqlParameter.Add("FMLGubun", cboFML.SelectedValue == null ? "" : cboFML.SelectedValue.ToString());
                    sqlParameter.Add("TotalDefectQty", lib.CheckNullZero(txtTotalDefectQty.Text));
                    sqlParameter.Add("MilSheetNo", txtMilSheetNo.Text);

                    sqlParameter.Add("SumInspectQty", lib.CheckNullZero(txtSumInspectQty.Text.Replace(",", "")));
                    sqlParameter.Add("SumDefectQty", lib.CheckNullZero(txtSumDefectQty.Text.Replace(",", "")));
                    
                    #region 추가
                    if (strFlag.Equals("I"))
                    {
                        sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Inspect_iAutoInspect";
                        pro1.OutputUseYN = "Y";
                        pro1.OutputName = "InspectID";
                        pro1.OutputLength = "12";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        List<KeyValue> list_Result = new List<KeyValue>();
                        list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS_NewLog(Prolist, ListParameter, "C");                        

                        if (list_Result[0].key.ToLower() == "success")
                        {
                            list_Result.RemoveAt(0);
                            for (int i = 0; i < list_Result.Count; i++)
                            {
                                KeyValue kv = list_Result[i];
                                if (kv.key == "InspectID")
                                {
                                    strID = kv.value;
                                    flag = true;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("[저장실패]\r\n" + list_Result[0].value.ToString());
                            return false;
                        }

                        Prolist.Clear();
                        ListParameter.Clear();
                    }
                    #endregion

                    #region 수정
                    else if (strFlag.Equals("U"))
                    {
                        sqlParameter.Add("UpdateUserID", MainWindow.CurrentUser);

                        Procedure pro2 = new Procedure();
                        pro2.Name = "xp_Inspect_uAutoInspect";
                        pro2.OutputUseYN = "N";
                        pro2.OutputName = "InspectID";
                        pro2.OutputLength = "12";

                        Prolist.Add(pro2);
                        ListParameter.Add(sqlParameter);
                    }
                    #endregion

                    // Sub 그리드 추가
                    if (!string.IsNullOrEmpty(strID))
                    {
                        for (int i = 0; i < dgdSub1.Items.Count; i++)
                        {
                            WinInsAutoSub = dgdSub1.Items[i] as Win_Qul_InspectAuto_U_Sub_CodeView;

                            for (int j = 0; j < WinInsAutoSub.ValueCount; j++)
                            {
                                sqlParameter = new Dictionary<string, object>();
                                sqlParameter.Clear();
                                sqlParameter.Add("InspectID", strID);
                                sqlParameter.Add("InspectBasisID", WinInsAutoSub.InspectBasisID);
                                sqlParameter.Add("InspectBasisSeq", WinInsAutoSub.Seq);
                                sqlParameter.Add("InspectBasisSubSeq", WinInsAutoSub.SubSeq);
                                sqlParameter.Add("InspectValue", 0);
                                sqlParameter.Add("InspectText", lib.CheckNull(WinInsAutoSub.arrInspectText[j]));
                                sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                                Procedure pro2 = new Procedure();
                                pro2.Name = "xp_Inspect_iAutoInspectSub";
                                pro2.OutputUseYN = "N";
                                pro2.OutputName = "InspectID";
                                pro2.OutputLength = "10";

                                Prolist.Add(pro2);
                                ListParameter.Add(sqlParameter);
                            }
                        }

                        for (int i = 0; i < dgdSub2.Items.Count; i++)
                        {
                            WinInsAutoSub = dgdSub2.Items[i] as Win_Qul_InspectAuto_U_Sub_CodeView;

                            for (int j = 0; j < WinInsAutoSub.ValueCount; j++)
                            {
                                sqlParameter = new Dictionary<string, object>();
                                sqlParameter.Clear();
                                sqlParameter.Add("InspectID", strID);
                                sqlParameter.Add("InspectBasisID", WinInsAutoSub.InspectBasisID);
                                sqlParameter.Add("InspectBasisSeq", WinInsAutoSub.Seq);
                                sqlParameter.Add("InspectBasisSubSeq", WinInsAutoSub.SubSeq);
                                sqlParameter.Add("InspectText", "");

                                string inspectValue = WinInsAutoSub.arrInspectValue[j] != "" ? lib.CheckNullZero(WinInsAutoSub.arrInspectValue[j].Replace(",", "")) : "0";
                                sqlParameter.Add("InspectValue", inspectValue);
                                sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                                Procedure pro3 = new Procedure();
                                pro3.Name = "xp_Inspect_iAutoInspectSub";
                                pro3.OutputUseYN = "N";
                                pro3.OutputName = "InspectID";
                                pro3.OutputLength = "10";

                                Prolist.Add(pro3);
                                ListParameter.Add(sqlParameter);
                            }
                        }

                        // 첨부파일 등록
                        if (txtSKetch.Text != string.Empty || txtFile.Text != string.Empty)
                        {
                            bool AttachYesNo = false;
                            if (FTP_Save_File(listFtpFile, strID))
                            {
                                if (!txtSKetch.Text.Equals(string.Empty)) { txtSKetch.Tag = "/ImageData/AutoInspect/" + strID; }
                                if (!txtFile.Text.Equals(string.Empty)) { txtFile.Tag = "/ImageData/AutoInspect/" + strID; }

                                AttachYesNo = true;
                            }
                            else
                            {
                                string strWord = strFlag.Equals("I") ? "저장" : "수정";
                                MessageBox.Show(string.Format("데이터 {0}이 완료되었지만, 첨부문서 등록에 실패하였습니다.", strWord));
                            }

                            if (AttachYesNo == true)
                                AttachFileUpdate(strID);      //첨부문서 정보 DB 업데이트.
                        }

                        string[] Confirm = new string[2];
                        Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew_NewLog(Prolist, ListParameter, "U");
                        if (Confirm[0] != "success")
                        {
                            MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                            flag = false;
                        }
                        else
                            flag = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

            return flag;
        }

        /// <summary>
        /// 입력사항 체크
        /// </summary>
        /// <returns></returns>
        private bool CheckData()
        {

            bool flag = true;

            //if (txtLotNO.Text.Length <= 0 || txtLotNO.Text.Equals(""))
            //{
            //    MessageBox.Show("LOTNO가 입력되지 않았습니다.");
            //    flag = false;
            //    return flag;
            //}

            //if (txtArticleName.Text.Length <= 0 || txtArticleName.Text.Equals(""))
            //{
            //    MessageBox.Show("품명이 입력되지 않았습니다.");
            //    flag = false;
            //    return flag;
            //}

            if ((txtLotNO.Text.Length <= 0 || txtLotNO.Text.Equals("")) && (txtArticleName.Text.Length <= 0 || txtArticleName.Text.Equals("")))
            {
                MessageBox.Show("LotNO 또는 품명이 입력되지 않았습니다. LotNO가 없다면 품명을 입력해주세요.");
                flag = false;
                return flag;
            }


            if (cboEcoNO.SelectedValue == null)
            {
                MessageBox.Show("EO-기준-순번이 선택되지 않았습니다.");
                flag = false;
                return flag;
            }

            //입고, 출하 검사시에는 공정, 호기를 선택하지 않는다. Hidden시킬 것이니까 그게 아닐 경우에만 checkdata
            if (tbnIncomeInspect.IsChecked != true && tbnOutcomeInspect.IsChecked != true)
            {
                if (cboProcess.SelectedValue == null)
                {
                    MessageBox.Show("공정이 선택되지 않았습니다.");
                    flag = false;
                    return flag;
                }
            }


            if (cboInspectClss.SelectedValue == null)
            {
                MessageBox.Show("검사수준이 선택되지 않았습니다.");
                flag = false;
                return flag;
            }

            if (cboInspectGbn.SelectedValue == null)
            {
                MessageBox.Show("검사구분이 선택되지 않았습니다.");
                flag = false;
                return flag;
            }

            if (strPoint == "5" && dtpInOutDate.SelectedDate == null)
            {
                MessageBox.Show("출고일이 선택되지 않았습니다.");
                flag = false;
                return flag;
            }

            return flag;
        }


        // 1) 첨부문서가 있을경우, 2) FTP에 정상적으로 업로드가 완료된 경우.  >> DB에 정보 업데이트 
        private void AttachFileUpdate(string ID)
        {
            try
            {
                string SketchPath = string.Empty;
                string AttachedPath = string.Empty;


                if (txtSKetch.Text.Equals(string.Empty))
                {
                    SketchPath = "";
                }
                else
                {
                    SketchPath = txtSKetch.Tag.ToString();
                }

                if (txtFile.Text.Equals(string.Empty))
                {
                    AttachedPath = "";
                }
                else
                {
                    AttachedPath = txtFile.Tag.ToString();
                }


                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("InspectID", ID);

                sqlParameter.Add("SketchPath", SketchPath);
                sqlParameter.Add("SketchFile", txtSKetch.Text);
                sqlParameter.Add("AttachedPath", AttachedPath);
                sqlParameter.Add("AttachedFile", txtFile.Text);

                sqlParameter.Add("UpdateUserID", MainWindow.CurrentUser);

                string[] result = DataStore.Instance.ExecuteProcedure("xp_Inspect_uAutoInspect_Ftp", sqlParameter, true);
                if (!result[0].Equals("success"))
                {
                    MessageBox.Show("이상발생, 관리자에게 문의하세요");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }







        #region 중간 입력 이벤트

        //차종
        private void txtBuyerModel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtBuyerModel, (int)Defind_CodeFind.DCF_BUYERMODEL, "");
            }
        }

        //차종
        private void btnPfBuyerModel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtBuyerModel, (int)Defind_CodeFind.DCF_BUYERMODEL, "");
        }

        //품명(품번으로 보이게 수정요청, 2020.03.19, 장가빈)
        private void txtArticleName_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    MainWindow.pf.ReturnCode(txtArticleName, 84, txtArticleName.Text);

                    if (txtArticleName.Tag != null)
                    {
                        SetEcoNoCombo(txtArticleName.Tag.ToString(), strPoint);
                        GetArticelData(txtArticleName.Tag.ToString());

                        if (cboEcoNO.ItemsSource != null)
                        {
                            cboEcoNO.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        //품명
        private void btnPfArticleName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow.pf.ReturnCode(txtArticleName, 84, txtArticleName.Text);

                if (txtArticleName.Tag != null)
                {
                    SetEcoNoCombo(txtArticleName.Tag.ToString(), strPoint);
                    GetArticelData(txtArticleName.Tag.ToString());

                    if (cboEcoNO.ItemsSource != null)
                    {
                        cboEcoNO.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        //검사자
        private void txtInspectUserID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtInspectUserID, (int)Defind_CodeFind.DCF_PERSON, "");
            }
        }

        //검사자
        private void btnPfUser_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtInspectUserID, (int)Defind_CodeFind.DCF_PERSON, "");
        }

        //어쨋든 거래처임
        private void txtInOutCustom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtInOutCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }

        //어쨋든 거래처임
        private void btnPfInOutCustom_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtInOutCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //공정 선택시 
        private void cboProcess_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboMachine.ItemsSource != null)
            {
                cboMachine.ItemsSource = null;
            }

            if (cboMachine.Items.Count > 0)
            {
                cboMachine.Items.Clear();
            }

            if (cboProcess.SelectedValue != null)
            {
                ObservableCollection<CodeView> ovcMachineAutoMC = ComboBoxUtil.Instance.GetMachine(cboProcess.SelectedValue.ToString());
                this.cboMachine.ItemsSource = ovcMachineAutoMC;
                this.cboMachine.DisplayMemberPath = "code_name";
                this.cboMachine.SelectedValuePath = "code_id";
            }
        }

        //
        private void SetEcoNoCombo(string strArticleID, string strPoint)
        {
            if (cboEcoNO.ItemsSource != null)
                cboEcoNO.ItemsSource = null;

            if (ovcEvoBasis.Count > 0)
                ovcEvoBasis.Clear();

            ObservableCollection<CodeView> setCollection = new ObservableCollection<CodeView>();

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("ArticleID", strArticleID);
                sqlParameter.Add("InspectPoint", strPoint);
                ds = DataStore.Instance.ProcedureToDataSet("xp_Code_sInspectAutoBasisByArticleID", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;


                        foreach (DataRow dr in drc)
                        {
                            var WinEcoNo = new CodeView()
                            {
                                code_id = dr[1].ToString().Trim(),
                                code_name = dr[0].ToString().Trim() + "-" + dr[1].ToString().Trim() + "-" + dr[2].ToString().Trim()
                            };

                            setCollection.Add(WinEcoNo);
                        }

                        foreach (DataRow dr in drc)
                        {
                            var WinEcoNo = new EcoNoAndBasisID()
                            {
                                EcoNo = dr["EcoNo"].ToString(),
                                InspectBasisID = dr["InspectBasisID"].ToString(),
                                Seq = dr["Seq"].ToString()
                            };

                            ovcEvoBasis.Add(WinEcoNo);
                        }
                    }

                    cboEcoNO.ItemsSource = setCollection;
                    this.cboEcoNO.DisplayMemberPath = "code_name";
                    this.cboEcoNO.SelectedValuePath = "code_id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        //
        private void GetArticelData(string strArticleID)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("ArticleID", strArticleID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Order_sArticleData", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];

                        var articleData = new ArticleData
                        {
                            //(품번으로 보이게 수정요청, 2020.03.19, 장가빈)
                            Article = dr["Article"].ToString(),
                        };

                        txtBuyerArticle.Text = articleData.Article;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        #endregion

        #region 서브그리드 관련

        //ECoNO 콤보박스 선택 -> SubDataGrid Fill
        private void cboEcoNO_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                try
                {
                    if (cboEcoNO.SelectedValue == null)
                    {
                        strBasisID = string.Empty;
                        BasisSeq = 1;

                        if (dgdSub1.Items.Count > 0)
                            dgdSub1.Items.Clear();

                        if (dgdSub2.Items.Count > 0)
                            dgdSub2.Items.Clear();

                        return;
                    }

                    strBasisID = string.Empty;
                    BasisSeq = 1;
                    for (int i = 0; i < ovcEvoBasis.Count; i++)
                    {
                        if (cboEcoNO.SelectedValue.ToString().Equals(ovcEvoBasis[i].InspectBasisID))
                        {
                            strBasisID = ovcEvoBasis[i].InspectBasisID;
                            BasisSeq = int.Parse(ovcEvoBasis[i].Seq);
                            FillSubDataByBasisID(strBasisID, BasisSeq);

                            //EO-금형-순번 콤보박스 선택시, 그에 해당하는 공정을 찾아 셀렉트인덱스 시켜준다.
                            //(하나의 품명에 여러 공정 검사기준이 있을 수 있으므로, GLS는 공정별로 관리한다.)
                            string sql = "select InspectBasisID, ProcessID from mt_InspectAutoBasis";
                            sql += " where InspectBasisID = " + strBasisID;

                            try
                            {
                                string processid = string.Empty;

                                DataSet ds = DataStore.Instance.QueryToDataSet(sql);
                                if (ds != null && ds.Tables.Count > 0)
                                {
                                    DataTable dt = ds.Tables[0];
                                    if (dt.Rows.Count == 0)
                                    {
                                    }
                                    else
                                    {
                                        DataRowCollection drc = dt.Rows;

                                        foreach (DataRow item in drc)
                                        {
                                            var Get = new Win_Qul_InspectAuto_U_CodeView();
                                            {
                                                processid = item[1].ToString().Trim();
                                            }
                                        }

                                        //해당 공정아이디를 콤보박스에 반영
                                        cboProcess.SelectedValue = processid;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("콤보박스 생성 중 오류 발생 : " + ex.ToString());

                            }
                            finally
                            {
                                DataStore.Instance.CloseConnection();
                            }


                            break;
                        }
                    }

                    if (strFlag.Equals("U"))
                    {
                        var One = win_Qul_InspectAuto_U_Sub_CodeViewsByU("1");
                        var Two = win_Qul_InspectAuto_U_Sub_CodeViewsByU("2");

                        for (int i = 0; i < dgdSub1.Items.Count; i++)
                        {
                            var dgr1 = dgdSub1.Items[i] as Win_Qul_InspectAuto_U_Sub_CodeView;

                            if (dgr1 != null && One != null)
                            {
                                int k = 0;
                                for (int j = 0; j < One.Count; j++)
                                {
                                    var subdg = One[j];
                                    if (dgr1.SubSeq == subdg.SubSeq)
                                    {
                                        for (int textIdx = 0; textIdx < subdg.arrInspectText.Length; textIdx++)
                                        {
                                            string inspectText = subdg.arrInspectText[textIdx];
                                            dgr1.arrInspectText[textIdx] = inspectText;

                                            if (!inspectText.Equals(""))
                                                k++;
                                        }

                                        dgr1.RefreshTextBlock(1, dgr1.arrInspectText);
                                        dgr1.ValueCount = k;
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < dgdSub2.Items.Count; i++)
                        {
                            var dgr2 = dgdSub2.Items[i] as Win_Qul_InspectAuto_U_Sub_CodeView;

                            if (dgr2 != null && Two != null)
                            {
                                int k = 0;
                                for (int j = 0; j < Two.Count; j++)
                                {
                                    var subdg = Two[j];
                                    if (dgr2.SubSeq == subdg.SubSeq)
                                    {
                                        for (int textIdx = 0; textIdx < subdg.arrInspectValue.Length; textIdx++)
                                        {
                                            string inspectValue = subdg.arrInspectValue[textIdx];
                                            dgr2.arrInspectValue[textIdx] = inspectValue;

                                            if (lib.IsNumOrAnother(inspectValue))
                                                k++;
                                        }

                                        dgr2.RefreshTextBlock(1, dgr2.arrInspectValue);
                                        dgr2.ValueCount = k;
                                    }
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
                }
                finally
                {
                    DataStore.Instance.CloseConnection();
                }
            }
        }

        private ObservableCollection<Win_Qul_InspectAuto_U_Sub_CodeView> win_Qul_InspectAuto_U_Sub_CodeViewsByU(string strType)
        {
            ObservableCollection<Win_Qul_InspectAuto_U_Sub_CodeView> returnData =
                new ObservableCollection<Win_Qul_InspectAuto_U_Sub_CodeView>();

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("InspectID", txtinspectID.Text);
                sqlParameter.Add("InsType", strType);
                ds = DataStore.Instance.ProcedureToDataSet("xp_Inspect_sAutoInspectSub", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int idx = 0;

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinQulInsAutoSub = new Win_Qul_InspectAuto_U_Sub_CodeView()
                            {
                                Num = idx + 1,
                                InspectBasisID = dr["InspectBasisID"].ToString(),
                                Seq = dr["Seq"].ToString(),
                                SubSeq = dr["SubSeq"].ToString(),
                                insType = dr["insType"].ToString(),
                                insItemName = dr["insItemName"].ToString(),
                                SpecMin = lib.returnNumStringTwo(dr["SpecMin"].ToString()),
                                SpecMax = lib.returnNumStringTwo(dr["SpecMax"].ToString()),
                                InsTPSpecMin = dr["InsTPSpecMin"].ToString(),
                                InsTPSpecMax = dr["InsTPSpecMax"].ToString(),
                                InsSampleQty = dr["InsSampleQty"].ToString(),
                                insSpec = dr["insSpec"].ToString(),
                                R = dr["R"].ToString(),
                                Sigma = dr["Sigma"].ToString(),
                                xBar = dr["xBar"].ToString()
                            };

                            for (int i = 0; i < 10; i++)
                            {
                                int num = i + 1;
                                WinQulInsAutoSub.arrInspectValue[i] = lib.returnNumStringTwo(dr["InspectValue" + num.ToString()].ToString());
                                WinQulInsAutoSub.arrInspectText[i] = dr["InspectText" + num.ToString()].ToString();
                            }

                            WinQulInsAutoSub.RefreshTextBlock(0, WinQulInsAutoSub.arrInspectValue);
                            WinQulInsAutoSub.RefreshTextBlock(1, WinQulInsAutoSub.arrInspectText);

                            returnData.Add(WinQulInsAutoSub);
                            idx++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

            return returnData;
        }

        //Sub 그리드 채우기(BasisID 있을시)
        private void FillSubDataByBasisID(string strID, int Seq)
        {
            if (dgdSub1.Items.Count > 0)
            {
                dgdSub1.Items.Clear();
                defectCheck1.Clear();
            }

            if (dgdSub2.Items.Count > 0)
            {
                dgdSub2.Items.Clear();
                defectCheck2.Clear();
            }

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("InspectBasisID", strID);
                sqlParameter.Add("Seq", Seq);
                sqlParameter.Add("SubSeq", 0);
                ds = DataStore.Instance.ProcedureToDataSet("xp_Code_sInspectAutoBasisSub", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;
                    int j = 0;

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinQulInsAutoByBasis = new Win_Qul_InspectAuto_U_Sub_CodeView()
                            {
                                InspectBasisID = dr["InspectBasisID"].ToString(),
                                Seq = dr["Seq"].ToString(),
                                SubSeq = dr["SubSeq"].ToString(),
                                insType = dr["insType"].ToString(),
                                insItemName = dr["insItemName"].ToString(),
                                InsSampleQty = dr["InsSampleQty"].ToString(),
                                ValueCount = 0,

                                InsTPSpecMax = dr["InsTPSpecMax"].ToString(),
                                InsTPSpecMin = dr["InsTPSpecMin"].ToString()
                            };

                            if (WinQulInsAutoByBasis.insType.Replace(" ", "").Equals("1"))
                            {
                                i++;
                                WinQulInsAutoByBasis.Num = i;
                                WinQulInsAutoByBasis.insSpec = dr["InsTPSpec"].ToString();
                                WinQulInsAutoByBasis.SpecMax = dr["InsTPSpecMax"].ToString();
                                WinQulInsAutoByBasis.SpecMin = dr["InsTPSpecMin"].ToString();

                                dgdSub1.Items.Add(WinQulInsAutoByBasis);
                            }
                            else if (WinQulInsAutoByBasis.insType.Replace(" ", "").Equals("2"))
                            {
                                j++;
                                WinQulInsAutoByBasis.Num = j;

                                if (dr["InspectCycleGubun"].ToString().Replace(" ", "").Equals("1"))
                                {
                                    WinQulInsAutoByBasis.Spec_CV = dr["insRaSpec"].ToString()
                                        + "(-" + dr["InsRaSpecMin"].ToString() + "~ +"
                                        + dr["insRASpecMax"].ToString() + ")";
                                    WinQulInsAutoByBasis.insSpec = dr["insRaSpec"].ToString();
                                    WinQulInsAutoByBasis.SpecMax = lib.returnNumStringTwo(dr["insRASpecMax"].ToString());
                                    WinQulInsAutoByBasis.SpecMin = lib.returnNumStringTwo(dr["InsRaSpecMin"].ToString());

                                    if (lib.IsNumOrAnother(WinQulInsAutoByBasis.insSpec) &&
                                        lib.IsNumOrAnother(WinQulInsAutoByBasis.SpecMax))
                                    {
                                        WinQulInsAutoByBasis.SpecMax = string.Format("{0:N2}",
                                            double.Parse(WinQulInsAutoByBasis.insSpec) + double.Parse(WinQulInsAutoByBasis.SpecMax));
                                    }
                                    if (lib.IsNumOrAnother(WinQulInsAutoByBasis.insSpec) &&
                                        lib.IsNumOrAnother(WinQulInsAutoByBasis.SpecMin))
                                    {
                                        WinQulInsAutoByBasis.SpecMin = string.Format("{0:N2}",
                                            double.Parse(WinQulInsAutoByBasis.insSpec) - double.Parse(WinQulInsAutoByBasis.SpecMin));
                                    }
                                }
                                else
                                {
                                    WinQulInsAutoByBasis.Spec_CV = dr["insRaSpec"].ToString();
                                    WinQulInsAutoByBasis.insSpec = dr["insRaSpec"].ToString();
                                    WinQulInsAutoByBasis.SpecMax = lib.returnNumStringTwo(dr["insRASpecMax"].ToString());
                                    WinQulInsAutoByBasis.SpecMin = lib.returnNumStringTwo(dr["InsRaSpecMin"].ToString());
                                }


                                dgdSub2.Items.Add(WinQulInsAutoByBasis);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        #region 서브그리드 입력이벤트

        //
        private void DataGridSub1Cell_KeyDown(object sender, KeyEventArgs e)
        {
            WinInsAutoSub = dgdSub1.CurrentItem as Win_Qul_InspectAuto_U_Sub_CodeView;
            int rowCount = dgdSub1.Items.IndexOf(dgdSub1.CurrentItem);
            int colCount = dgdSub1.Columns.IndexOf(dgdSub1.CurrentCell.Column);

            int lastColcount = 0;
            switch (WinInsAutoSub.InsSampleQty)
            {
                case "1":
                    lastColcount = dgdSub1.Columns.IndexOf(dgdtpeText1);
                    break;
                case "2":
                    lastColcount = dgdSub1.Columns.IndexOf(dgdtpeText2);
                    break;
                case "3":
                    lastColcount = dgdSub1.Columns.IndexOf(dgdtpeText3);
                    break;
                case "4":
                    lastColcount = dgdSub1.Columns.IndexOf(dgdtpeText4);
                    break;
                case "5":
                    lastColcount = dgdSub1.Columns.IndexOf(dgdtpeText5);
                    break;
                case "6":
                    lastColcount = dgdSub1.Columns.IndexOf(dgdtpeText6);
                    break;
                case "7":
                    lastColcount = dgdSub1.Columns.IndexOf(dgdtpeText7);
                    break;
                case "8":
                    lastColcount = dgdSub1.Columns.IndexOf(dgdtpeText8);
                    break;
                case "9":
                    lastColcount = dgdSub1.Columns.IndexOf(dgdtpeText9);
                    break;
                case "10":
                    lastColcount = dgdSub1.Columns.IndexOf(dgdtpeText10);
                    break;
            }

            int startColcount = dgdSub1.Columns.IndexOf(dgdtpeText1);
            int sub2StartColunt = dgdSub2.Columns.IndexOf(dgdtpeValue1);

            //MessageBox.Show(e.Key.ToString());

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (lastColcount == colCount && dgdSub1.Items.Count - 1 > rowCount)
                {
                    dgdSub1.SelectedIndex = rowCount + 1;
                    dgdSub1.CurrentCell = new DataGridCellInfo(dgdSub1.Items[rowCount + 1], dgdSub1.Columns[startColcount]);
                }
                else if (lastColcount > colCount && dgdSub1.Items.Count - 1 > rowCount)
                {
                    dgdSub1.CurrentCell = new DataGridCellInfo(dgdSub1.Items[rowCount], dgdSub1.Columns[colCount + 1]);
                }
                else if (lastColcount == colCount && dgdSub1.Items.Count - 1 == rowCount)
                {
                    if (dgdSub2.Items.Count > 0)
                    {
                        dgdSub2.Focus();
                        dgdSub2.SelectedIndex = 0;
                        dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[0], dgdSub2.Columns[sub2StartColunt]);
                    }
                    else
                    {
                        btnSave.Focus();
                    }
                }
                else if (lastColcount > colCount && dgdSub1.Items.Count - 1 == rowCount)
                {
                    dgdSub1.CurrentCell = new DataGridCellInfo(dgdSub1.Items[rowCount], dgdSub1.Columns[colCount + 1]);
                }
                else
                {
                    MessageBox.Show("검사수량을 초과해서 입력하실 수 없습니다.");
                }
            }
            else if (e.Key == Key.Down)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (dgdSub1.Items.Count - 1 > rowCount)
                {
                    dgdSub1.SelectedIndex = rowCount + 1;
                    dgdSub1.CurrentCell = new DataGridCellInfo(dgdSub1.Items[rowCount + 1], dgdSub1.Columns[colCount]);
                }
                else if (dgdSub1.Items.Count - 1 == rowCount)
                {
                    if (lastColcount > colCount)
                    {
                        dgdSub1.SelectedIndex = 0;
                        dgdSub1.CurrentCell = new DataGridCellInfo(dgdSub1.Items[0], dgdSub1.Columns[colCount + 1]);
                    }
                    else
                    {
                        if (dgdSub2.Items.Count > 0)
                        {
                            dgdSub2.Focus();
                            dgdSub2.SelectedIndex = 0;
                            dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[0], dgdSub2.Columns[sub2StartColunt]);
                        }
                        else
                        {
                            btnSave.Focus();
                        }
                    }
                }
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (rowCount > 0)
                {
                    dgdSub1.SelectedIndex = rowCount - 1;
                    dgdSub1.CurrentCell = new DataGridCellInfo(dgdSub1.Items[rowCount - 1], dgdSub1.Columns[colCount]);
                }
            }
            else if (e.Key == Key.Left)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (colCount > 0)
                {
                    dgdSub1.CurrentCell = new DataGridCellInfo(dgdSub1.Items[rowCount], dgdSub1.Columns[colCount - 1]);
                }
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (lastColcount > colCount)
                {
                    dgdSub1.CurrentCell = new DataGridCellInfo(dgdSub1.Items[rowCount], dgdSub1.Columns[colCount + 1]);
                }
                else if (lastColcount == colCount)
                {
                    if (dgdSub1.Items.Count - 1 > rowCount)
                    {
                        dgdSub1.SelectedIndex = rowCount + 1;
                        dgdSub1.CurrentCell = new DataGridCellInfo(dgdSub1.Items[rowCount + 1], dgdSub1.Columns[startColcount]);
                    }
                    else
                    {
                        if (dgdSub2.Items.Count > 0)
                        {
                            dgdSub2.Focus();
                            dgdSub2.SelectedIndex = 0;
                            dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[0], dgdSub2.Columns[sub2StartColunt]);
                        }
                        else
                        {
                            btnSave.Focus();
                        }
                    }
                }
            }
        }

        //
        private void DataGridSub2Cell_KeyDown(object sender, KeyEventArgs e)
        {
            WinInsAutoSub = dgdSub2.CurrentItem as Win_Qul_InspectAuto_U_Sub_CodeView;
            int rowCount = dgdSub2.Items.IndexOf(dgdSub2.CurrentItem);
            int colCount = dgdSub2.Columns.IndexOf(dgdSub2.CurrentCell.Column);

            int lastColcount = 0;
            switch (WinInsAutoSub.InsSampleQty)
            {
                case "1":
                    lastColcount = dgdSub2.Columns.IndexOf(dgdtpeValue1);
                    break;
                case "2":
                    lastColcount = dgdSub2.Columns.IndexOf(dgdtpeValue2);
                    break;
                case "3":
                    lastColcount = dgdSub2.Columns.IndexOf(dgdtpeValue3);
                    break;
                case "4":
                    lastColcount = dgdSub2.Columns.IndexOf(dgdtpeValue4);
                    break;
                case "5":
                    lastColcount = dgdSub2.Columns.IndexOf(dgdtpeValue5);
                    break;
                case "6":
                    lastColcount = dgdSub2.Columns.IndexOf(dgdtpeValue6);
                    break;
                case "7":
                    lastColcount = dgdSub2.Columns.IndexOf(dgdtpeValue7);
                    break;
                case "8":
                    lastColcount = dgdSub2.Columns.IndexOf(dgdtpeValue8);
                    break;
                case "9":
                    lastColcount = dgdSub2.Columns.IndexOf(dgdtpeValue9);
                    break;
                case "10":
                    lastColcount = dgdSub2.Columns.IndexOf(dgdtpeValue10);
                    break;
            }


            int startColcount = dgdSub2.Columns.IndexOf(dgdtpeValue1);

            //MessageBox.Show(e.Key.ToString());

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                //WinInsAutoSub = dgdSub2.CurrentItem as Win_Qul_InspectAuto_U_Sub_CodeView;
                //ataRowView rowView = (DataRowView)dgdSub2.Items[rowCount];



                Double specMax = Convert.ToDouble(WinInsAutoSub.SpecMax);
                Double specMin = Convert.ToDouble(WinInsAutoSub.SpecMin);

                if (lastColcount == colCount && dgdSub2.Items.Count - 1 > rowCount)
                {
                    dgdSub2.SelectedIndex = rowCount + 1;
                    dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[rowCount + 1], dgdSub2.Columns[startColcount]);
                }
                else if (lastColcount > colCount && dgdSub2.Items.Count - 1 > rowCount)
                {
                    dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[rowCount], dgdSub2.Columns[colCount + 1]);
                }
                else if (lastColcount == colCount && dgdSub2.Items.Count - 1 == rowCount)
                {
                    btnSave.Focus();
                }
                else if (lastColcount > colCount && dgdSub2.Items.Count - 1 == rowCount)
                {
                    dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[rowCount], dgdSub2.Columns[colCount + 1]);
                }
                else
                {
                    MessageBox.Show("검사수량을 초과해서 입력하실 수 없습니다.");
                }
            }
            else if (e.Key == Key.Down)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (dgdSub2.Items.Count - 1 > rowCount)
                {
                    dgdSub2.SelectedIndex = rowCount + 1;
                    dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[rowCount + 1], dgdSub2.Columns[colCount]);
                }
                else if (dgdSub2.Items.Count - 1 == rowCount)
                {
                    if (lastColcount > colCount)
                    {
                        dgdSub2.SelectedIndex = 0;
                        dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[0], dgdSub2.Columns[colCount + 1]);
                    }
                    else
                    {
                        btnSave.Focus();
                    }
                }
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (rowCount > 0)
                {
                    dgdSub2.SelectedIndex = rowCount - 1;
                    dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[rowCount - 1], dgdSub2.Columns[colCount]);
                }
            }
            else if (e.Key == Key.Left)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (colCount > 0)
                {
                    dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[rowCount], dgdSub2.Columns[colCount - 1]);
                }
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (lastColcount > colCount)
                {
                    dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[rowCount], dgdSub2.Columns[colCount + 1]);
                }
                else if (lastColcount == colCount)
                {
                    if (dgdSub2.Items.Count - 1 > rowCount)
                    {
                        dgdSub2.SelectedIndex = rowCount + 1;
                        dgdSub2.CurrentCell = new DataGridCellInfo(dgdSub2.Items[rowCount + 1], dgdSub2.Columns[startColcount]);
                    }
                    else
                    {
                        btnSave.Focus();
                    }
                }
            }
        }

        private void DataGridSub1Cell_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                DataGridSub1Cell_KeyDown(sender, e);
            }
        }

        private void DataGridSub2Cell_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                DataGridSub2Cell_KeyDown(sender, e);
            }
        }

        //
        private void TextBoxFocusInDataGrid(object sender, KeyEventArgs e)
        {
            lib.DataGridINControlFocus(sender, e);
        }

        //
        private void TextBoxFocusInDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            lib.DataGridINBothByMouseUP(sender, e);
        }

        //
        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                DataGridCell cell = sender as DataGridCell;
                cell.IsEditing = true;
            }
        }

        private void InspectText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblMsg.Visibility != Visibility.Visible)
                return;

            WinInsAutoSub = dgdSub1.CurrentItem as Win_Qul_InspectAuto_U_Sub_CodeView;
            if (WinInsAutoSub != null)
            {
                TextBox tb1 = sender as TextBox;
                if (tb1 != null)
                {
                    int idx = int.Parse(tb1.Tag == null ? "0" : tb1.Tag.ToString());
                    if (idx > 1 && Convert.ToInt32(WinInsAutoSub.InsSampleQty.Trim()) < idx)
                    {
                        tb1.Text = "";
                        WinInsAutoSub.arrInspectText[idx - 1] = "";
                    }
                    else
                    {
                        WinInsAutoSub.arrInspectText[idx - 1] = tb1.Text.ToUpper();
                        WinInsAutoSub.RefreshTextBlock(1, WinInsAutoSub.arrInspectText, idx);
                        tb1.SelectionStart = tb1.Text.Length;
                    }
                }

                sender = tb1;
            }
        }

        private void NumValue_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            lib.CheckIsNumeric((TextBox)sender, e);
        }

        private void InspectValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblMsg.Visibility != Visibility.Visible)
                return;

            WinInsAutoSub = dgdSub2.CurrentItem as Win_Qul_InspectAuto_U_Sub_CodeView;
            if (WinInsAutoSub != null)
            {
                TextBox tb1 = sender as TextBox;
                if (tb1 != null)
                {
                    int idx = int.Parse(tb1.Tag == null ? "0" : tb1.Tag.ToString());
                    if (idx > 1 && Convert.ToInt32(WinInsAutoSub.InsSampleQty.Trim()) < idx)
                    {
                        tb1.Text = "";
                        WinInsAutoSub.arrInspectValue[idx - 1] = "";
                    }
                    else
                    {
                        WinInsAutoSub.arrInspectValue[idx - 1] = tb1.Text;
                        WinInsAutoSub.RefreshTextBlock(0, WinInsAutoSub.arrInspectValue, idx);
                    }
                }

                sender = tb1;
            }
        }
        #endregion

        #endregion

        //
        private void txtLotNO_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                LotNo_Click();
        }

        //
        private void btnPfLotNO_Click(object sender, RoutedEventArgs e)
        {
            LotNo_Click();
        }

        private void LotNo_Click()
        {
            int largeNum = strPoint.Equals("1") ? 101 : 100;
            MainWindow.pf.ReturnCode(txtLotNO, largeNum, txtLotNO.Text);

            if (!string.IsNullOrEmpty(txtLotNO.Text))
                GetLotID(txtLotNO.Text);
        }

        //
        private void GetLotID(string LotNo)
        {
            try
            {
                txtArticleName.Tag = null;
                txtArticleName.Text = "";
                txtBuyerArticle.Text = "";
                txtBuyerModel.Text = "";
                txtInOutCustom.Tag = null;
                txtInOutCustom.Text = "";
                dtpInOutDate.SelectedDate = DateTime.Today;

                string processID = strPoint == "3" || strPoint ==  "9" ? 
                    (cboProcess.SelectedValue != null ? cboProcess.SelectedValue.ToString() : "") : "";

                LotNo = LotNo.Replace(" ", "");

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("LotNo", LotNo);
                sqlParameter.Add("InspectPoint", strPoint);
                sqlParameter.Add("ArticleID", txtArticleName.Tag != null ? txtArticleName.Tag.ToString() : "");
                sqlParameter.Add("ProcessID", processID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Inspect_sLotNo", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];

                        var LotInfo = new GetLotInfo()
                        {
                            LOTID = dr["LabelID"].ToString(),
                            ArticleID = dr["ArticleID"].ToString(),
                            Article = dr["Article"].ToString(),
                            BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                            Model = dr["Model"].ToString(),
                            CustomID = dr["CustomID"].ToString(),
                            Custom = dr["KCustom"].ToString(),
                            InoutDate = dr["InoutDate"].ToString()
                        };

                        txtArticleName.Text = LotInfo.BuyerArticleNo;
                        txtArticleName.Tag = LotInfo.ArticleID;
                        txtBuyerArticle.Text = LotInfo.Article;
                        txtBuyerModel.Text = LotInfo.Model;
                        txtInOutCustom.Text = LotInfo.Custom;
                        txtInOutCustom.Tag = LotInfo.CustomID;

                        if (LotInfo.InoutDate.Replace(" ", "").Length > 0)
                            dtpInOutDate.SelectedDate = lib.strConvertDate(LotInfo.InoutDate);

                        if (txtArticleName.Tag != null && !txtArticleName.Tag.ToString().Equals(""))
                        {
                            SetEcoNoCombo(txtArticleName.Tag.ToString(), strPoint);

                            if (cboEcoNO.ItemsSource != null)
                                cboEcoNO.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        MessageBox.Show("더이상 등록할 수 없는 LabelID입니다.");

                        dgdSub1.Items.Clear();
                        dgdSub2.Items.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        //
        private int GetValueCount()
        {
            int totalCount = 0;
            int sub1Count = 0;
            int sub2Count = 0;
            int defectCount = 0;
            bool Flag = true;

            strTotalCount = string.Empty;
            strDefectYN = "N";

            for (int i = 0; i < dgdSub1.Items.Count; i++)
            {
                var WinSubAuto = dgdSub1.Items[i] as Win_Qul_InspectAuto_U_Sub_CodeView;

                if (WinSubAuto != null)
                {
                    WinSubAuto.ValueCount = 0;
                    string compareSpec = WinSubAuto.SpecMin.ToUpper();

                    for (int textIdx = 0; textIdx < WinSubAuto.arrInspectText.Length; textIdx++)
                    {
                        string inspectText = WinSubAuto.arrInspectText[textIdx];
                        if (inspectText != null && inspectText.Replace(" ", "").Length > 0)
                        {
                            sub1Count++;

                            if (!inspectText.Equals(compareSpec))
                            {
                                if (Flag)
                                {
                                    strDefectYN = "Y";
                                    Flag = false;
                                }

                                defectCount++;
                            }

                            WinSubAuto.ValueCount++;
                        }
                    }
                }
            }

            bool SpecFlag = false;
            double doubleSpecMin = 0.0;
            double doubleSpecMax = 0.0;
            for (int i = 0; i < dgdSub2.Items.Count; i++)
            {
                var WinSubAuto = dgdSub2.Items[i] as Win_Qul_InspectAuto_U_Sub_CodeView;

                SpecFlag = lib.IsNumOrAnother(WinSubAuto.SpecMin) && lib.IsNumOrAnother(WinSubAuto.SpecMax)
                            ? true : false;

                if (SpecFlag)
                {
                    doubleSpecMin = double.Parse(WinSubAuto.SpecMin);
                    doubleSpecMax = double.Parse(WinSubAuto.SpecMax);
                }

                if (WinSubAuto != null)
                {
                    WinSubAuto.ValueCount = 0;

                    for (int valueIdx = 0; valueIdx < WinSubAuto.arrInspectValue.Length; valueIdx++)
                    {
                        string inspectValue = WinSubAuto.arrInspectValue[valueIdx];
                        if (inspectValue != null && inspectValue.Replace(" ", "").Length > 0)
                        {
                            sub2Count++;

                            if (SpecFlag && lib.IsNumOrAnother(inspectValue))
                            {
                                if (doubleSpecMin <= double.Parse(inspectValue) && doubleSpecMax >= double.Parse(inspectValue))
                                {
                                    if (Flag)
                                        strDefectYN = "N";
                                }
                                else
                                {
                                    if (Flag)
                                    {
                                        strDefectYN = "Y";
                                        Flag = false;
                                    }

                                    defectCount++;
                                }

                                WinSubAuto.ValueCount++;
                            }
                        }
                    }
                }
            }

            totalCount = sub1Count + sub2Count;
            cboDefectYN.SelectedValue = strDefectYN;
            txtTotalDefectQty.Text = defectCount.ToString();

            return totalCount;
        }

        //
        private void ValueText_LostFocus(object sender, RoutedEventArgs e)
        {
            txtInspectQty.Text = GetValueCount().ToString();

        }

        private void dgdMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (btnUpdate.IsEnabled == true)
            //{
            //    if(e.ClickCount==2)
            //        btnUpdate_Click(btnUpdate, null);
            //}
        }

        private void DataGrid_SizeChange(object sender, SizeChangedEventArgs e)
        {
            DataGrid dgs = sender as DataGrid;
            if (dgs.ColumnHeaderHeight == 0)
            {
                dgs.ColumnHeaderHeight = 1;
            }
            double a = e.NewSize.Height / 100;
            double b = e.PreviousSize.Height / 100;
            double c = a / b;

            if (c != double.PositiveInfinity && c != 0 && double.IsNaN(c) == false)
            {
                dgs.ColumnHeaderHeight = dgs.ColumnHeaderHeight * c;
                dgs.FontSize = dgs.FontSize * c;
            }
        }

        #region FTP 따로 모음

        //
        private void btnSKetch_Click(object sender, RoutedEventArgs e)
        {
            OpenFileAndSetting(sender, e);
        }

        private void btnSKetchDel_Click(object sender, RoutedEventArgs e)
        {
            DeleteFileAndSetting(sender, e);
        }

        private void btnSKetchDown_Click(object sender, RoutedEventArgs e)
        {
            DownloadFileAndSetting(sender, e);
        }

        private void btnFileAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileAndSetting(sender, e);
        }

        private void btnFileDel_Click(object sender, RoutedEventArgs e)
        {
            DeleteFileAndSetting(sender, e);
        }

        private void btnFileDownload_Click(object sender, RoutedEventArgs e)
        {
            DownloadFileAndSetting(sender, e);
        }

        private void OpenFileAndSetting(object sender, RoutedEventArgs e)
        {
            // (버튼)sender 마다 tag를 달자.
            string ClickPoint = ((Button)sender).Tag.ToString();
            string[] strTemp = null;
            Microsoft.Win32.OpenFileDialog OFdlg = new Microsoft.Win32.OpenFileDialog();

            OFdlg.DefaultExt = ".jpg";
            OFdlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png | All Files|*.*";

            Nullable<bool> result = OFdlg.ShowDialog();
            if (result == true)
            {
                if (ClickPoint == "SKetch") { FullPath1 = OFdlg.FileName; }  //긴 경로(FULL 사이즈)
                if (ClickPoint == "File") { FullPath2 = OFdlg.FileName; }

                string AttachFileName = OFdlg.SafeFileName;  //명.
                string AttachFilePath = string.Empty;       // 경로

                if (ClickPoint == "SKetch") { AttachFilePath = FullPath1.Replace(AttachFileName, ""); }
                if (ClickPoint == "File") { AttachFilePath = FullPath2.Replace(AttachFileName, ""); }

                StreamReader sr = new StreamReader(OFdlg.FileName);
                long File_size = sr.BaseStream.Length;
                if (sr.BaseStream.Length > (2048 * 1000))
                {
                    // 업로드 파일 사이즈범위 초과
                    MessageBox.Show("이미지의 파일사이즈가 2M byte를 초과하였습니다.");
                    sr.Close();
                    return;
                }
                if (ClickPoint == "SKetch")
                {
                    txtSKetch.Text = AttachFileName;
                    txtSKetch.Tag = AttachFilePath.ToString();
                }
                else if (ClickPoint == "File")
                {
                    txtFile.Text = AttachFileName;
                    txtFile.Tag = AttachFilePath.ToString();
                }
                strTemp = new string[] { AttachFileName, AttachFilePath.ToString() };
                listFtpFile.Add(strTemp);
            }
        }

        // 파일 저장하기.
        private bool FTP_Save_File(List<string[]> listStrArrayFileInfo, string MakeFolderName)
        {
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

            List<string[]> UpdateFilesInfo = new List<string[]>();
            string[] fileListSimple;
            string[] fileListDetail = null;
            fileListSimple = _ftp.directoryListSimple("", Encoding.Default);

            // 기존 폴더 확인작업.
            bool MakeFolder = false;
            MakeFolder = FolderInfoAndFlag(fileListSimple, MakeFolderName.Trim());

            if (MakeFolder == false)        // 같은 아이를 찾지 못한경우,
            {
                //MIL 폴더에 InspectionID로 저장
                if (_ftp.createDirectory(MakeFolderName.Trim()) == false)
                {
                    MessageBox.Show("업로드를 위한 폴더를 생성할 수 없습니다.");
                    return false;
                }
            }
            else
            {
                fileListDetail = _ftp.directoryListSimple(MakeFolderName.Trim(), Encoding.Default);
            }
            for (int i = 0; i < listStrArrayFileInfo.Count; i++)
            {
                bool flag = true;

                if (fileListDetail != null)
                {
                    foreach (string compare in fileListDetail)
                    {
                        if (compare.Equals(listStrArrayFileInfo[i][0]))
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (flag)
                {
                    listStrArrayFileInfo[i][0] = MakeFolderName.Trim() + "/" + listStrArrayFileInfo[i][0];
                    UpdateFilesInfo.Add(listStrArrayFileInfo[i]);
                }
            }
            if (UpdateFilesInfo.Count > 0)
            {
                if (!_ftp.UploadTempFilesToFTP(UpdateFilesInfo))
                {
                    MessageBox.Show("파일업로드에 실패하였습니다.");
                    return false;
                }
            }
            return true;
        }
        // 다운받기
        private void DownloadFileAndSetting(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgresult = MessageBox.Show("파일을 보시겠습니까?", "보기 확인", MessageBoxButton.YesNo);
            if (msgresult == MessageBoxResult.Yes)
            {
                //버튼 태그값.
                string ClickPoint = ((Button)sender).Tag.ToString();

                if ((ClickPoint == "SKetch") && (txtSKetch.Tag.ToString() == string.Empty))
                {
                    MessageBox.Show("파일이 없습니다.");
                    return;
                }
                if ((ClickPoint == "File") && (txtFile.Tag.ToString() == string.Empty))
                {
                    MessageBox.Show("파일이 없습니다.");
                    return;
                }

                var ViewReceiver = dgdMain.SelectedItem as Win_Qul_InspectAuto_U_CodeView;
                if (ViewReceiver != null)
                {
                    string imgName = "";
                    if (ClickPoint == "SKetch")
                    {
                        imgName = ViewReceiver.SketchFile;
                        FTP_DownLoadFile(ViewReceiver.SketchPath, ViewReceiver.InspectID, ref imgName);
                    }
                    else if (ClickPoint == "File")
                    {
                        imgName = ViewReceiver.AttachedFile;
                        FTP_DownLoadFile(ViewReceiver.AttachedPath, ViewReceiver.InspectID, ref imgName);
                    }
                }
            }
        }

        //다운로드
        private void FTP_DownLoadFile(string Path, string FolderName, ref string ImageName, bool isArticleDown = false)
        {
            try
            {
                if (isArticleDown)
                    _ftp = new FTP_EX(FTP_ADDRESS_ARTICLE, FTP_ID, FTP_PASS);
                else
                    _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

                string[] fileListSimple;
                string[] fileListDetail;

                fileListSimple = _ftp.directoryListSimple("", Encoding.UTF8);

                bool ExistFile = false;

                ExistFile = FolderInfoAndFlag(fileListSimple, FolderName);

                if (ExistFile)
                {
                    ExistFile = false;
                    fileListDetail = _ftp.directoryListSimple(FolderName, Encoding.UTF8);

                    if (isArticleDown)
                    {
                        ImageName = ImageName + ".png";
                        ExistFile = FileInfoAndFlag(fileListDetail, ImageName);
                        if (!ExistFile)
                        {
                            ImageName = ImageName + ".jpg";
                            ExistFile = FileInfoAndFlag(fileListDetail, ImageName);
                        }
                    }
                    else
                        ExistFile = FileInfoAndFlag(fileListDetail, ImageName);

                    if (ExistFile)
                    {
                        string str_remotepath = string.Empty;
                        string str_localpath = string.Empty;

                        str_remotepath = FTP_ADDRESS + '/' + FolderName + '/' + ImageName;
                        str_localpath = LOCAL_DOWN_PATH + "\\" + ImageName;

                        DirectoryInfo DI = new DirectoryInfo(LOCAL_DOWN_PATH);
                        if (DI.Exists)
                            DI.Create();

                        FileInfo file = new FileInfo(str_localpath);
                        if (file.Exists)
                            file.Delete();

                        str_remotepath = str_remotepath.Substring(str_remotepath.Substring(0, str_remotepath.LastIndexOf("/")).LastIndexOf("/"));
                        _ftp.download(str_remotepath, str_localpath);

                        if (!isArticleDown)
                        {
                            ProcessStartInfo proc = new ProcessStartInfo(str_localpath);
                            proc.UseShellExecute = true;
                            Process.Start(proc);
                        }
                    }
                    else
                    {
                        MessageBox.Show("파일을 찾을 수 없습니다.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }



        private void DeleteFileAndSetting(object sender, RoutedEventArgs e)
        {
            MessageBoxResult msgresult = MessageBox.Show("파일을 삭제 하시겠습니까?", "삭제 확인", MessageBoxButton.YesNo);
            if (msgresult == MessageBoxResult.Yes)
            {
                string ClickPoint = ((Button)sender).Tag.ToString();

                if ((ClickPoint == "SKetch") && (txtSKetch.Tag.ToString() != string.Empty))
                {
                    //if (DetectFtpFile(txtDrawID.Text))
                    //{
                    //    FTP_UploadFile_File_Delete(txtDrawID.Text, txtAttFile1.Text);
                    //}

                    txtSKetch.Text = string.Empty;
                    txtSKetch.Tag = string.Empty;
                }
                if ((ClickPoint == "File") && (txtFile.Tag.ToString() != string.Empty))
                {
                    //if (DetectFtpFile(txtDrawID.Text))
                    //{
                    //    FTP_UploadFile_File_Delete(txtDrawID.Text, txtAttFile2.Text);
                    //}

                    txtFile.Text = string.Empty;
                    txtFile.Tag = string.Empty;
                }
            }
        }






        /// <summary>
        /// 해당영역에 폴더가 있는지 확인
        /// </summary>
        bool FolderInfoAndFlag(string[] strFolderList, string FolderName)
        {
            bool flag = false;
            foreach (string FolderList in strFolderList)
            {
                if (FolderList == FolderName)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        /// <summary>
        /// 해당영역에 파일 있는지 확인
        /// </summary>
        bool FileInfoAndFlag(string[] strFileList, string FileName)
        {
            bool flag = false;
            foreach (string FileList in strFileList)
            {
                if (FileList == FileName)
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }


        private void clear()
        {
            txtArticleName.Clear();
            txtinspectID.Clear();
            txtLotNO.Clear();
            txtBuyerArticle.Clear();
            txtBuyerModel.Clear();
            txtComments.Clear();
            txtFile.Clear();
            txtInspectQty.Clear();
            txtInOutCustom.Clear();
            txtInspectUserID.Clear();
            txtMilSheetNo.Clear();
            txtSKetch.Clear();
            txtSumDefectQty.Clear();
            txtSumInspectQty.Clear();
            txtTotalDefectQty.Clear();
            cboProcess.SelectedIndex = -1;
            cboMachine.SelectedIndex = -1;
            cboInspectClss.SelectedIndex = -1;
            cboInspectGbn.SelectedIndex = -1;
            cboIRELevel.SelectedIndex = -1;
            cboFML.SelectedIndex = -1;
            cboDefectYN.SelectedIndex = -1;
            cboEcoNO.SelectedIndex = -1;
        }



        #endregion

        private void dgdMain_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {

        }

        // 품명
        private void chkArticle_Checked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = true;
            btnPFArticleSrh.IsEnabled = true;
        }

        private void chkArticle_Unchecked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = false;
            btnPFArticleSrh.IsEnabled = false;
        }

        private void ValueText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                WinInsAutoSub = dgdSub2.CurrentItem as Win_Qul_InspectAuto_U_Sub_CodeView;
                if (WinInsAutoSub != null)
                {
                    TextBox tb1 = sender as TextBox;
                    if (tb1 != null)
                    {
                        double maxValue = Convert.ToDouble(WinInsAutoSub.SpecMax);
                        double minValue = Convert.ToDouble(WinInsAutoSub.SpecMin);

                        int idx = int.Parse(tb1.Tag == null ? "0" : tb1.Tag.ToString());
                        string inspectValue = WinInsAutoSub.arrInspectValue[idx - 1];
                        double value = string.IsNullOrEmpty(inspectValue) ? 0 : Convert.ToDouble(inspectValue);
                        WinInsAutoSub.arrValueDefect[idx - 1] = !(value >= minValue && value <= maxValue) ? "true" : "";
                        WinInsAutoSub.RefreshTextBlock(2, WinInsAutoSub.arrValueDefect, idx);
                    }
                }
            }
        }

        //품명
        private void chkArticle_Click(object sender, MouseButtonEventArgs e)
        {
            if (chkArticleSrh.IsChecked == true)
            {
                chkArticleSrh.IsChecked = false;
                txtArticleSrh.IsEnabled = false;
                btnPFArticleSrh.IsEnabled = false;
            }
            else
            {
                chkArticleSrh.IsChecked = true;
                txtArticleSrh.IsEnabled = true;
                btnPFArticleSrh.IsEnabled = true;
                txtArticleSrh.Focus();
            }
        }

        //품번
        private void chkArticleNo_Click(object sender, RoutedEventArgs e)
        {
            if (chkArticleNo.IsChecked == true)
            {
                txtArticleNo.IsEnabled = true;
                txtArticleNo.Focus();
                btnArticleNo.IsEnabled = true;
            }
            else
            {
                txtArticleNo.IsEnabled = false;
                btnArticleNo.IsEnabled = false;
            }
        }

        // 플러스파인더 _ 품번 찾기
        private void btnArticleNo_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleNo, 76, txtArticleNo.Text);
        }

        // 품번 키다운 
        private void TxtArticleNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleNo, 76, txtArticleNo.Text);
            }
        }

        private void chkInspect_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chkSender = sender as CheckBox;
            var view = chkSender.DataContext as Win_Qul_InspectAuto_U_CodeView;
            if (view != null)
            {
                if (chkSender.IsChecked == true)
                {
                    view.Chk = true;

                    if (listLotLabelPrint.Contains(view) == false)
                        listLotLabelPrint.Add(view);
                }
                else
                {
                    view.Chk = false;

                    if (listLotLabelPrint.Contains(view) == false)
                        listLotLabelPrint.Remove(view);
                }
            }
        }
    }

    class Win_Qul_InspectAuto_U_CodeView : BaseView
    {
        public bool Chk { get; set; }
        public int Num { get; set; }
        public string InspectID { get; set; }
        public string ArticleID { get; set; }
        public string Article { get; set; }
        public string InspectGubun { get; set; }

        public string InspectDate { get; set; }
        public string LotID { get; set; }
        public string InspectQty { get; set; }
        public string ECONo { get; set; }
        public string Comments { get; set; }

        public string InspectLevel { get; set; }
        public string SketchPath { get; set; }
        public string SketchFile { get; set; }
        public string AttachedPath { get; set; }
        public string AttachedFile { get; set; }

        public string InspectUserID { get; set; }
        public string InspectBasisID { get; set; }
        public string ProcessID { get; set; }
        public string DefectYN { get; set; }

        public string Process { get; set; }
        public string BuyerArticleNo { get; set; }
        public string InspectPoint { get; set; }
        public string ImportSecYN { get; set; }
        public string ImportlawYN { get; set; }

        public string ImportImpYN { get; set; }
        public string ImportNorYN { get; set; }
        public string IRELevel { get; set; }
        public string IRELevelName { get; set; }
        public string InpCustomID { get; set; }

        public string InpCustomName { get; set; }
        public string InpDate { get; set; }
        public string OutCustomID { get; set; }
        public string OutCustomName { get; set; }
        public string OutDate { get; set; }

        public string MachineID { get; set; }
        public string BuyerModelID { get; set; }
        public string BuyerModel { get; set; }
        public string FMLGubun { get; set; }
        public string TotalDefectQty { get; set; }

        public string MilSheetNo { get; set; }
        public string Name { get; set; }

        public string SumInspectQty { get; set; }
        public string SumDefectQty { get; set; }

        public string InspectDate_CV { get; set; }
        public string InpDate_CV { get; set; }
        public string OutDate_CV { get; set; }

        public string InOutCustom { get; set; }
        public string INOUTCustomID { get; set; }
        public string INOUTCustomDate { get; set; }
        public string FMLGubunName { get; set; }

        public string INOutDate { get; set; }
    }

    class Win_Qul_InspectAuto_U_Sub_CodeView : BaseView
    {
        public int Num { get; set; }
        public string InspectBasisID { get; set; }
        public string Seq { get; set; }
        public string SubSeq { get; set; }
        public string insType { get; set; }

        public string insItemName { get; set; }
        public string insSpec { get; set; }
        public string SpecMin { get; set; }
        public string SpecMax { get; set; }
        public string InsTPSpecMax { get; set; }
        public string InsTPSpecMin { get; set; }
        public string InsSampleQty { get; set; }

        public string[] arrInspectValue = new string[10];
        public string[] arrInspectText = new string[10];
        public string[] arrValueDefect = new string[10];

        private void SetTextBlock(int idx, byte gbn, string str)
        {
            switch (idx)
            {
                case 0:
                    if (gbn == 0)       InspectValue1 = str;
                    else if (gbn == 1)  InspectText1 = str;
                    else                ValueDefect1 = str;
                    break;
                case 1:
                    if (gbn == 0)       InspectValue2 = str;
                    else if (gbn == 1)  InspectText2 = str;
                    else                ValueDefect2 = str;
                    break;
                case 2:
                    if (gbn == 0)       InspectValue3 = str;
                    else if (gbn == 1)  InspectText3 = str;
                    else                ValueDefect3 = str;
                    break;
                case 3:
                    if (gbn == 0)       InspectValue4 = str;
                    else if (gbn == 1)  InspectText4 = str;
                    else                ValueDefect4 = str;
                    break;
                case 4:
                    if (gbn == 0)       InspectValue5 = str;
                    else if (gbn == 1)  InspectText5 = str;
                    else                ValueDefect5 = str;
                    break;
                case 5:
                    if (gbn == 0)       InspectValue6 = str;
                    else if (gbn == 1)  InspectText6 = str;
                    else                ValueDefect6 = str;
                    break;
                case 6:
                    if (gbn == 0)       InspectValue7 = str;
                    else if (gbn == 1)  InspectText7 = str;
                    else                ValueDefect7 = str;
                    break;
                case 7:
                    if (gbn == 0)       InspectValue8 = str;
                    else if (gbn == 1)  InspectText8 = str;
                    else                ValueDefect8 = str;
                    break;
                case 8:
                    if (gbn == 0)       InspectValue9 = str;
                    else if (gbn == 1)  InspectText9 = str;
                    else                ValueDefect9 = str;
                    break;
                case 9:
                    if (gbn == 0)       InspectValue10 = str;
                    else if (gbn == 1)  InspectText10 = str;
                    else                ValueDefect10 = str;
                    break;
            }
        }

        public void RefreshTextBlock(byte gbn, string[] arrBase, int idx)
        {
            for (int i = 0; i < arrBase.Length; i++)
            {
                if (i == idx - 1)
                {
                    SetTextBlock(i, gbn, arrBase[i]);
                    break;
                }
            }
        }

        public void RefreshTextBlock(byte gbn, string[] arrBase)
        {
            for (int i = 0; i < arrBase.Length; i++)
                SetTextBlock(i, gbn, arrBase[i]);
        }

        public string InspectValue1 { get; set; }
        public string InspectValue2 { get; set; }
        public string InspectValue3 { get; set; }
        public string InspectValue4 { get; set; }
        public string InspectValue5 { get; set; }

        public string InspectValue6 { get; set; }
        public string InspectValue7 { get; set; }
        public string InspectValue8 { get; set; }
        public string InspectValue9 { get; set; }
        public string InspectValue10 { get; set; }

        public string InspectText1 { get; set; }
        public string InspectText2 { get; set; }
        public string InspectText3 { get; set; }
        public string InspectText4 { get; set; }
        public string InspectText5 { get; set; }

        public string InspectText6 { get; set; }
        public string InspectText7 { get; set; }
        public string InspectText8 { get; set; }
        public string InspectText9 { get; set; }
        public string InspectText10 { get; set; }

        public string ValueDefect1 { get; set; }
        public string ValueDefect2 { get; set; }
        public string ValueDefect3 { get; set; }
        public string ValueDefect4 { get; set; }
        public string ValueDefect5 { get; set; }

        public string ValueDefect6 { get; set; }
        public string ValueDefect7 { get; set; }
        public string ValueDefect8 { get; set; }
        public string ValueDefect9 { get; set; }
        public string ValueDefect10 { get; set; }

        public string xBar { get; set; }
        public string R { get; set; }
        public string Sigma { get; set; }

        //public string CV_Spec { get; set; }
        public int ValueCount { get; set; }
        public string Spec_CV { get; set; }
    }

    class EcoNoAndBasisID : BaseView
    {
        public string EcoNo { get; set; }
        public string InspectBasisID { get; set; }
        public string Seq { get; set; }
    }

    class GetLotInfo : BaseView
    {
        public string InstID { get; set; }
        public string ArticleID { get; set; }
        public string Article { get; set; }
        public string CustomID { get; set; }
        public string Custom { get; set; }

        public string InoutDate { get; set; }
        public string InspectBasisID { get; set; }
        public string Seq { get; set; }
        public string EcoNo { get; set; }
        public string Model { get; set; }

        public string BuyerArticleNo { get; set; }
        public string MoldNo { get; set; }
        public string ProcessID { get; set; }
        public string LOTID { get; set; }
        public string InoutDate_CV { get; set; }
    }
}
