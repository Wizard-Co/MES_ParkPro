﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WizMes_ParkPro.PopUP;
using WizMes_ParkPro.PopUp;


namespace WizMes_ParkPro
{
    /**************************************************************************************************
    '** System 명 : WizMes_GLS
    '** Author    : Wizard
    '** 작성자    : 최준호
    '** 내용      : 검사기준등록
    '** 생성일자  : 2019.04.11
    '** 변경일자  : 
    '**------------------------------------------------------------------------------------------------
    ''*************************************************************************************************
    ' 변경일자  , 변경자, 요청자    , 요구사항ID  , 요청 및 작업내용
    '**************************************************************************************************
    ' ex) 2015.11.09, 박진성, 오영      ,S_201510_AFT_03 , 월별집계(가로) 순서 변경 : 합계/10월/9월/8월 순으로
    '**************************************************************************************************/

    /// <summary>
    /// Win_Qul_InspectAutoBasis_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_Qul_InspectAutoBasis_U : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        string sInspectPoint = string.Empty;
        string strFlag = string.Empty;
        string ButtonEOClickCheck = string.Empty;
        int Wh_Ar_SelectedLastIndex = 0;        // 그리드 마지막 선택 줄 임시저장 그릇
        int rowNum = 0;

        string SstrID = string.Empty;
        Lib lib = new Lib();

        bool FtpFirstFlag = false; // 데이터 그리드 선택 → 서브 그리드 조회 시 FTP 문제시에 서브 그리드 갯수만큼 경고메시지를 보게 되는걸 방지하기 위해 추가. 2020.02.12

        //FTP 활용모음
        string strImagePath = string.Empty;
        string strFullPath = string.Empty;
        string strDelFileName = string.Empty;

        List<string> deleteListFtpFile = new List<string>(); // 삭제할 파일 리스트
        List<string> lstExistFtpFile = new List<string>();

        // 촤! FTP Server 에 있는 폴더 + 파일 경로를 저장해놓고 그걸로 다운 및 업로드하자 마!
        // 이미지 이름 : 폴더이름
        Dictionary<string, string> lstFtpFilePath = new Dictionary<string, string>();

        private FTP_EX _ftp = null;

        List<string[]> listFtpFile = new List<string[]>();
        private List<UploadFileInfo> _listFileInfo = new List<UploadFileInfo>();

        internal struct UploadFileInfo          //FTP.
        {
            public string Filename { get; set; }
            public FtpFileType Type { get; set; }
            public DateTime LastModifiedTime { get; set; }
            public long Size { get; set; }
            public string Filepath { get; set; }
        }

        internal enum FtpFileType
        {
            None,
            DIR,
            File
        }

        //string FTP_ADDRESS = "ftp://wizis.iptime.org/ImageData/InspectAuto";

        string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":" + LoadINI.FTPPort + LoadINI.FtpImagePath + "/InspectAutoBasis";
        //원본 
        //string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":" + LoadINI.FTPPort + LoadINI.FtpImagePath + "/InspectAutoBasis";
        string ForderName = "InspectAutoBasis";
        private const string FTP_ID = "wizuser";
        private const string FTP_PASS = "wiz9999";

        ////string FTP_ADDRESS = "ftp://222.104.222.145:25000/ImageData/McRegularInspect";
        //string FTP_ADDRESS = "ftp://192.168.0.95/ImageData/McRegularInspect";

        private const string LOCAL_DOWN_PATH = "C:\\Temp";



        /// <summary>
        /// Main 그리드용
        /// </summary>
        ObservableCollection<Win_Qul_InspectAutoBasis_U_CodeView> ovcInspectAutoBasis
            = new ObservableCollection<Win_Qul_InspectAutoBasis_U_CodeView>();

        /// <summary>
        /// 서브 그리드용
        /// </summary>
        ObservableCollection<Win_Qul_InspectAutoBasis_U_Sub_CodeView> ovcInspectAutoBasisSub
           = new ObservableCollection<Win_Qul_InspectAutoBasis_U_Sub_CodeView>();

        /// <summary>
        /// 서브 그리드 삭제용
        /// </summary>
        ObservableCollection<Win_Qul_InspectAutoBasis_U_Sub_CodeView> ovcInspectAutoBasisSub_Delete
            = new ObservableCollection<Win_Qul_InspectAutoBasis_U_Sub_CodeView>();

        ObservableCollection<CodeView> ovcTypeView = null;
        ObservableCollection<CodeView> ovcManageView = null;
        ObservableCollection<CodeView> ovcCycleView = null;

        public Win_Qul_InspectAutoBasis_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(this);
            TbnOutCome_Click(tbnOutCome, null);
            SetComboBox();
            //dtpMoldNo.SelectedDate = DateTime.Today;
        }

        void SetComboBox()
        {
            List<string[]> listTypeArray = new List<string[]>();
            string[] typeOne = new string[] { "1", "외관" };
            string[] typeTwo = new string[] { "2", "DIM's" };
            listTypeArray.Add(typeOne);
            listTypeArray.Add(typeTwo);

            ovcTypeView = ComboBoxUtil.Instance.Direct_SetComboBox(listTypeArray);


            ovcManageView = ComboBoxUtil.Instance.GetCMCode_SetComboBox("INSITEMGBN", "");
            ovcCycleView = ComboBoxUtil.Instance.GetCMCode_SetComboBox("INSCYCLEGBN", "");
        }

        void SetBuyerArticleNo(string strID)  //품명을 뿌려야하니까 수정 2020.03.19, 장가빈
                                              //공정도 같이 뿌려주기로 함 2020.04.17, 장가빈
        {
            DataTable dt = Procedure.Instance.GetArticleData(strID);

            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["Article"] != null &&
                    !dt.Rows[0]["Article"].ToString().Trim().Equals(string.Empty))
                {
                    txtBuyerArticle.Text = dt.Rows[0]["Article"].ToString();
                    //txtProcess.Tag = dt.Rows[0]["ProcessID"].ToString();
                    //txtProcess.Text = dt.Rows[0]["Process"].ToString();
                }
            }
        }

        // 기준번호
        private void LblInspectBasisSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkInspectBasisSrh.IsChecked == true) { chkInspectBasisSrh.IsChecked = false; }
            else { chkInspectBasisSrh.IsChecked = true; }
        }

        // 기준번호
        private void ChkInspectBasisSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtInspectBasisSrh.IsEnabled = true;
            txtInspectBasisSrh.Focus();
        }

        // 기준번호
        private void ChkInspectBasisSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtInspectBasisSrh.IsEnabled = false;
        }
        // 품명
        private void LblArticleSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticleSrh.IsChecked == true) { chkArticleSrh.IsChecked = false; }
            else { chkArticleSrh.IsChecked = true; }
        }

        // 품명
        private void ChkArticleSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = true;
            btnPfArticleSrh.IsEnabled = true;
            txtArticleSrh.Focus();
        }

        // 품명
        private void ChkArticleSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = false;
            btnPfArticleSrh.IsEnabled = false;
        }

        // 품명(품번으로 수정요청, 2020.03.19, 장가빈)
        private void TxtArticleSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh, 77, txtArticleSrh.Text);
            }
        }

        // 품명(품번으로 수정요청, 2020.03.19, 장가빈)
        private void BtnPfArticleSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh, 77, txtArticleSrh.Text);
        }

        // EONO
        private void LblECONOSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkECONOSrh.IsChecked == true) { chkECONOSrh.IsChecked = false; }
            else { chkECONOSrh.IsChecked = true; }
        }

        // EONO
        private void ChkECONOSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtECONOSrh.IsEnabled = true;
            txtECONOSrh.Focus();
        }

        // EONO
        private void ChkECONOSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtECONOSrh.IsEnabled = false;
        }

        // 기준일자 ? 
        private void LblMoldNoSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldNoSrh.IsChecked == true) { chkMoldNoSrh.IsChecked = false; }
            else { chkMoldNoSrh.IsChecked = true; }
        }

        // 기준일자 ? 
        private void ChkMoldNoSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldNoSrh.IsEnabled = true;
            txtMoldNoSrh.Focus();
        }

        // 기준일자 ? 
        private void ChkMoldNoSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldNoSrh.IsEnabled = false;
        }

        // 수입
        private void TbnInCome_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton.IsChecked == true)
            {
                tbnOutCome.IsChecked = false;
                tbnProcessCycle.IsChecked = false;
                tbnJaju.IsChecked = false;
                if (!sInspectPoint.Equals("1"))
                {
                    dgdSub.ItemsSource = null;
                    dgdMain.ItemsSource = null;
                    dgdMain.Refresh();
                    dgdSub.Refresh();

                }

                txtInspectBasisID.Text = "";
                txtArticle.Text = "";
                txtProcess.Text = "";
                txtCarModel.Text = "";
                txtBuyerArticle.Text = "";
                txtECONO.Text = "";
                //dtpMoldNo.SelectedDate = DateTime.Today;
                //txtMoldNo.Text = "";
                txtComments.Text = "";

                sInspectPoint = "1";
                txtProcess.Visibility = Visibility.Hidden;
                lblProcess.Visibility = Visibility.Hidden;
            }
            else
            {
                toggleButton.IsChecked = true;
            }
        }

        // 공정순회
        private void TbnProcessCycle_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton.IsChecked == true)
            {
                tbnInCome.IsChecked = false;
                tbnOutCome.IsChecked = false;
                tbnJaju.IsChecked = false;
                if (!sInspectPoint.Equals("3"))
                {
                    dgdSub.ItemsSource = null;
                    dgdMain.ItemsSource = null;
                    dgdMain.Refresh();
                    dgdSub.Refresh();
                }

                txtInspectBasisID.Text = "";
                txtArticle.Text = "";
                txtProcess.Text = "";
                txtCarModel.Text = "";
                txtBuyerArticle.Text = "";
                txtECONO.Text = "";
                //dtpMoldNo.SelectedDate = DateTime.Today;
                //txtMoldNo.Text = "";
                txtComments.Text = "";

                sInspectPoint = "3";
                txtProcess.Visibility = 0;
                lblProcess.Visibility = 0;
            }
            else
            {
                toggleButton.IsChecked = true;
            }
        }

        // 자주
        private void TbnJaju_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton.IsChecked == true)
            {
                tbnInCome.IsChecked = false;
                tbnOutCome.IsChecked = false;
                tbnProcessCycle.IsChecked = false;
                if (!sInspectPoint.Equals("9"))
                {
                    dgdSub.ItemsSource = null;
                    dgdMain.ItemsSource = null;
                    dgdMain.Refresh();
                    dgdSub.Refresh();

                }
                txtInspectBasisID.Text = "";
                txtArticle.Text = "";
                txtProcess.Text = "";
                txtCarModel.Text = "";
                txtBuyerArticle.Text = "";
                txtECONO.Text = "";
                //dtpMoldNo.SelectedDate = DateTime.Today;
                //txtMoldNo.Text = "";
                txtComments.Text = "";

                sInspectPoint = "9";
                txtProcess.Visibility = 0;
                lblProcess.Visibility = 0;
            }
            else
            {
                toggleButton.IsChecked = true;
            }
        }

        // 출하
        private void TbnOutCome_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton toggleButton = sender as ToggleButton;
            if (toggleButton.IsChecked == true)
            {
                tbnInCome.IsChecked = false;
                tbnProcessCycle.IsChecked = false;
                tbnJaju.IsChecked = false;
                if (!sInspectPoint.Equals("5"))
                {
                    dgdSub.ItemsSource = null;
                    dgdMain.ItemsSource = null;
                    dgdMain.Refresh();
                    dgdSub.Refresh();
                }

                txtInspectBasisID.Text = "";
                txtArticle.Text = "";
                txtProcess.Text = "";
                txtCarModel.Text = "";
                txtBuyerArticle.Text = "";
                txtECONO.Text = "";
                //dtpMoldNo.SelectedDate = DateTime.Today;
                //txtMoldNo.Text = "";
                txtComments.Text = "";

                sInspectPoint = "5";
                txtProcess.Visibility = Visibility.Hidden;
                lblProcess.Visibility = Visibility.Hidden;
            }
            else
            {
                toggleButton.IsChecked = true;
            }
        }

        /// <summary>
        /// 추가,수정 시 동작 모음
        /// </summary>
        private void ControlVisibleAndEnable_AU()
        {
            Lib.Instance.UiButtonEnableChange_SCControl(this);
            dgdMain.IsHitTestVisible = false;
            gbxInput.IsHitTestVisible = true;
            subAdd.IsEnabled = true;
            subDel.IsEnabled = true;
        }

        /// <summary>
        /// 저장,취소 시 동작 모음
        /// </summary>
        private void ControlVisibleAndEnable_SC()
        {
            Lib.Instance.UiButtonEnableChange_IUControl(this);
            dgdMain.IsHitTestVisible = true;
            gbxInput.IsHitTestVisible = false;
            subAdd.IsEnabled = false;
            subDel.IsEnabled = false;
        }

        //EO추가
        private void btnEOAdd_Click(object sender, RoutedEventArgs e)
        {

            var WinInspect = dgdMain.SelectedItem as Win_Qul_InspectAutoBasis_U_CodeView;

            if (WinInspect != null)
            {
                ControlVisibleAndEnable_AU();
                tbkMsg.Text = "EO자료 추가 중";
                strFlag = "I";

                ButtonEOClickCheck = "EoCheck";

                lstFtpFilePath.Clear();


                //dtpMoldNo.SelectedDate = DateTime.Today;

                // 폴더이름 → 대개 객체 PK 값
                string Key = WinInspect.InspectBasisID;

                for (int i = 0; i < dgdSub.Items.Count; i++)
                {
                    var sub = dgdSub.Items[i] as Win_Qul_InspectAutoBasis_U_Sub_CodeView;


                    //이미지 파일이 있을 경우에만 lstFtpFilePath에 정보를 넣도록 하고싶다 ㅠㅠ
                    if (sub.InsImageFile != null)
                    {
                        if (!sub.InsImageFile.ToString().Trim().Equals(""))
                        {
                            if (!lstFtpFilePath.ContainsKey(sub.InsImageFile))
                            {
                                lstFtpFilePath.Add(sub.InsImageFile, Key);
                            }
                        }
                    }
                }
            }
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMain.SelectedItem != null)
            {
                if (dgdMain.Items.Count > 0)
                {
                    Wh_Ar_SelectedLastIndex = dgdMain.SelectedIndex;
                }
                else
                {
                    Wh_Ar_SelectedLastIndex = 0;
                }
            }
            this.DataContext = null;
            strFlag = "I";
            ControlVisibleAndEnable_AU();
            tbkMsg.Text = "자료 추가 중";
            ovcInspectAutoBasisSub.Clear();

            AddSubItem();

            //추가 시작 포커스. (품 명)
            txtArticle.Focus();

            strImagePath = string.Empty;
            strDelFileName = string.Empty;
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            var InsAutoBasis = dgdMain.SelectedItem as Win_Qul_InspectAutoBasis_U_CodeView;
            if (InsAutoBasis == null)
            {
                MessageBox.Show("수정할 데이터가 지정되지 않았습니다. 수정데이터를 지정하고 눌러주세요");
                return;
            }
            else
            {
                Wh_Ar_SelectedLastIndex = dgdMain.SelectedIndex;
                strFlag = "U";
                ControlVisibleAndEnable_AU();
                tbkMsg.Text = "자료 수정 중";
            }
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var InsAutoBasis = dgdMain.SelectedItem as Win_Qul_InspectAutoBasis_U_CodeView;
                if (InsAutoBasis == null)
                {
                    MessageBox.Show("삭제할 데이터가 지정되지 않았습니다. 삭제데이터를 지정하고 눌러주세요");
                    return;
                }
                else
                {
                    if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        using (Loading ld = new Loading(beDelete))
                        {
                            ld.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void beDelete()
        {
            //삭제버튼 비활성화
            btnDelete.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                var InsAutoBasis = dgdMain.SelectedItem as Win_Qul_InspectAutoBasis_U_CodeView;

                DataStore.Instance.InsertLogByForm(this.GetType().Name, "D");
                if (Procedure.Instance.DeleteData(InsAutoBasis.InspectBasisID, InsAutoBasis.Seq
                    , "InspectBasisID", "Seq", "xp_Code_dInspectAutoBasis"))
                {
                    rowNum = dgdMain.SelectedIndex - 1;
                    re_Search(rowNum);
                }
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnDelete.IsEnabled = true;
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        // 검색 클릭
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
                rowNum = 0;
                re_Search(rowNum);

            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSearch.IsEnabled = true;
        }

        // 저장 클릭
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
                if (SaveData(txtInspectBasisID.Text, 1))
                {
                    ControlVisibleAndEnable_SC();

                    //저장,취소하면 삭제를 위해 담아둔 뷰를 모두 삭제
                    ovcInspectAutoBasisSub_Delete.Clear();

                    rowNum = dgdMain.Items.Count + 1;
                    re_Search(rowNum);
                    //FillGrid();
                    dgdMain.ItemsSource = null;
                    dgdMain.ItemsSource = ovcInspectAutoBasis;
                    dgdMain.Items.Refresh();

                    if (strFlag == "I")     //1. 추가 > 저장했다면,
                    {
                        if (dgdMain.Items.Count > 0)
                        {
                            dgdMain.SelectedIndex = dgdMain.Items.Count - 1;
                            dgdMain.Focus();
                        }
                    }
                    else        //2. 수정 > 저장했다면,
                    {
                        dgdMain.SelectedIndex = Wh_Ar_SelectedLastIndex;
                        dgdMain.Focus();
                    }
                    strFlag = string.Empty; // 추가했는지, 수정했는지 알려면 맨 마지막에 flag 값을 비어야 한다.
                    ButtonEOClickCheck = string.Empty;
                }
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSave.IsEnabled = true;
        }

        // 취소 클릭
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ControlVisibleAndEnable_SC();
            //저장,취소하면 삭제를 위해 담아둔 뷰를 모두 삭제
            ovcInspectAutoBasisSub_Delete.Clear();

            rowNum = dgdMain.SelectedIndex;
            re_Search(rowNum);
            //FillGrid();

            if (strFlag == "I") // 1. 추가하다가 취소했다면,
            {
                if (dgdMain.Items.Count > 0)
                {
                    dgdMain.SelectedIndex = Wh_Ar_SelectedLastIndex;
                    dgdMain.Focus();
                }
            }
            else        //2. 수정하다가 취소했다면
            {
                dgdMain.SelectedIndex = Wh_Ar_SelectedLastIndex;
                dgdMain.Focus();
            }

            strFlag = string.Empty; // 추가했는지, 수정했는지 알려면 맨 마지막에 flag 값을 비어야 한다.
        }

        // 엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;
            Lib lib = new Lib();

            string[] dgdStr = new string[4];
            dgdStr[0] = "검사기준 목록";
            dgdStr[1] = "검사기준 세부목록";
            dgdStr[2] = dgdMain.Name;
            dgdStr[3] = dgdSub.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(dgdStr);
            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");
                if (ExpExc.choice.Equals(dgdMain.Name))
                {
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
                    else
                        return;
                }
                else if (ExpExc.choice.Equals(dgdSub.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = lib.DataGridToDTinHidden(dgdSub);
                    else
                        dt = lib.DataGirdToDataTable(dgdSub);

                    Name = dgdSub.Name;

                    if (lib.GenerateExcel(dt, Name))
                    {
                        lib.excel.Visible = true;
                        lib.ReleaseExcelObject(lib.excel);
                    }
                    else
                        return;
                }
                else
                {
                    if (dt != null)
                    {
                        dt.Clear();
                    }
                }
            }
            lib = null;
        }

        private void re_Search(int rowNum)
        {

            dgdMain.SelectedIndex = rowNum;
            FillGrid();
            //FillGrid();
        }

        // 조회
        private void FillGrid()
        {
            try
            {
                ovcInspectAutoBasis.Clear();

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("chkInspectBasisID", chkInspectBasisSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("InspectBasisID", chkInspectBasisSrh.IsChecked == true ? txtInspectBasisSrh.Text : "");
                sqlParameter.Add("chkArticleID", chkArticleSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleID", chkArticleSrh.IsChecked == true ? txtArticleSrh.Tag : "");
                sqlParameter.Add("chkEcoNo", chkECONOSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("EcoNo", chkECONOSrh.IsChecked == true ? txtECONOSrh.Text : "");
                sqlParameter.Add("chkMoldNo", chkMoldNoSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("MoldNo", chkMoldNoSrh.IsChecked == true ? txtMoldNoSrh.SelectedDate.Value.ToString("yyyyMMdd") : "");
                //sqlParameter.Add("MoldNo", chkMoldNoSrh.IsChecked == true ? txtMoldNoSrh.Text : ""); //dtpMoldNo.SelectedDate.Value.ToString("yyyyMMdd")
                sqlParameter.Add("InspectPoint", sInspectPoint);
                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_Code_sInspectAutoBasis", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("조회결과가 없습니다.");
                        return;
                    }
                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;
                            var InsAutoBasis = new Win_Qul_InspectAutoBasis_U_CodeView
                            {
                                Num = i,
                                InspectBasisID = dr["InspectBasisID"].ToString(),
                                Seq = dr["Seq"].ToString(),
                                ArticleID = dr["ArticleID"].ToString(),
                                Article = dr["Article"].ToString(),

                                ProcessID = dr["ProcessID"].ToString(),
                                Process = dr["Process"].ToString(),
                                EcoNo = dr["EcoNo"].ToString(),
                                Comments = dr["Comments"].ToString(),
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),

                                BuyerModelID = dr["BuyerModelID"].ToString(),
                                Model = dr["Model"].ToString(),
                                InspectPoint = dr["InspectPoint"].ToString(),
                                MoldNo = Lib.Instance.StrDateTimeBar(dr["MoldNo"].ToString()),
                                CreateDate = dr["CreateDate"].ToString()
                            };
                            ovcInspectAutoBasis.Add(InsAutoBasis);
                        }
                        tbkIndexCount.Text = "▶검색결과 : " + i + " 건";
                        dgdMain.ItemsSource = ovcInspectAutoBasis;
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

        private void DgdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var InsAutoBasis = dgdMain.SelectedItem as Win_Qul_InspectAutoBasis_U_CodeView;

            if (InsAutoBasis != null)
            {
                this.DataContext = InsAutoBasis;
                FillGridSub(InsAutoBasis.InspectBasisID, InsAutoBasis.Seq, 0);
            }
        }

        // 서브그리드 조회
        private void FillGridSub(string strInspectBasisID, string Seq, int subSeq)
        {
            try
            {
                ovcInspectAutoBasisSub.Clear();

                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("InspectBasisID", strInspectBasisID);
                sqlParameter.Add("Seq", Seq);
                sqlParameter.Add("SubSeq", subSeq);
                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Code_sInspectAutoBasisSub", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        int i = 0;

                        foreach (DataRow dr in drc)
                        {
                            i++;
                            var InsAutoBasisSub = new Win_Qul_InspectAutoBasis_U_Sub_CodeView
                            {
                                Num = i,
                                InspectBasisID = dr["InspectBasisID"].ToString(),
                                Seq = dr["Seq"].ToString(),
                                SubSeq = dr["SubSeq"].ToString(),
                                insType = dr["insType"].ToString(),
                                insItemName = dr["insItemName"].ToString(),
                                insRaSpec = dr["insRaSpec"].ToString(),
                                insRASpecMax = dr["insRASpecMax"].ToString(),
                                InsRaSpecMin = dr["InsRaSpecMin"].ToString(),
                                InsTPSpec = dr["InsTPSpec"].ToString(),
                                InsTPSpecMax = dr["InsTPSpecMax"].ToString(),
                                InsTPSpecMin = dr["InsTPSpecMin"].ToString(),
                                InsSampleQty = dr["InsSampleQty"].ToString(),
                                ManageGubun = dr["ManageGubun"].ToString(),
                                ManageGubunname = dr["ManageGubunname"].ToString(),
                                InspectGage = dr["InspectGage"].ToString(),
                                InspectGageName = dr["InspectGageName"].ToString(),
                                InspectCycleGubun = dr["InspectCycleGubun"].ToString(),
                                InspectCycleGubunName = dr["InspectCycleGubunName"].ToString(),
                                InspectCycle = dr["InspectCycle"].ToString(),
                                Comments = dr["Comments"].ToString(),
                                InsImageFile = dr["InsImageFile"].ToString(),
                                InsImagePath = dr["InsImagePath"].ToString(),

                                ovcCycle = ovcCycleView,
                                ovcManage = ovcManageView,
                                ovcType = ovcTypeView,
                                stringFlag = "U" //필그리드 할때는 기본적으로 수정 Flag

                            };

                            if (!InsAutoBasisSub.InsImageFile.Replace(" ", "").Equals(""))
                            {
                                if (Lib.Instance.Right(InsAutoBasisSub.InsImageFile, 3).Equals("pdf"))
                                {
                                    InsAutoBasisSub.imageFlag = true;
                                }
                                else
                                {
                                    lstExistFtpFile.Add(InsAutoBasisSub.InsImageFile);
                                    InsAutoBasisSub.imageFlag = true;
                                    string strImage = "/" + InsAutoBasisSub.InsImageFile;
                                    // 테스트 곽동운 20190919 : 이미지가 없어서 오류발생 → 주석처리 
                                    InsAutoBasisSub.ImageView = SetImage(strImage, InsAutoBasisSub.InspectBasisID);
                                }
                            }

                            if (InsAutoBasisSub.insType.Trim().Equals("1"))
                            {
                                InsAutoBasisSub.Spec = InsAutoBasisSub.InsTPSpec;
                                InsAutoBasisSub.SpecMax = InsAutoBasisSub.InsTPSpecMax;
                                InsAutoBasisSub.SpecMin = InsAutoBasisSub.InsTPSpecMin;
                                InsAutoBasisSub.insTypeText = "외관";
                            }
                            else
                            {
                                InsAutoBasisSub.Spec = InsAutoBasisSub.insRaSpec;
                                //InsAutoBasisSub.SpecMax = Lib.Instance.returnNumString(InsAutoBasisSub.insRASpecMax); 최대최소 입력시 빈칸 입력할수 있다고 텍스트로 받음
                                //InsAutoBasisSub.SpecMin = Lib.Instance.returnNumString(InsAutoBasisSub.InsRaSpecMin); 최대최소 입력시 빈칸 입력할수 있다고 텍스트로 받음  
                                InsAutoBasisSub.SpecMax = InsAutoBasisSub.InsTPSpecMax;
                                InsAutoBasisSub.SpecMin = InsAutoBasisSub.InsTPSpecMin;
                                InsAutoBasisSub.insTypeText = "DIM's";
                            }

                            ovcInspectAutoBasisSub.Add(InsAutoBasisSub);
                        }

                        dgdSub.ItemsSource = ovcInspectAutoBasisSub;
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

        // 품명 플러스파인더(품번으로 수정 요청, 2020.03.19, 장가빈)
        private void TxtArticle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (tbnJaju.IsChecked == true || tbnProcessCycle.IsChecked == true)
                {
                    MainWindow.pf.ReturnCode(txtArticle, txtProcess, 831, txtArticle.Text);
                }
                else
                {
                    MainWindow.pf.ReturnCode(txtArticle, 76, txtArticle.Text);
                }


                if (txtArticle.Tag != null)
                {
                    SetBuyerArticleNo(txtArticle.Tag.ToString());
                    //txtProcess.Focus();
                }


                //MainWindow.pf.ReturnCode(txtArticle, (int)Defind_CodeFind.DCF_Article, "");
                //MainWindow.pf.ReturnCode(txtArticle, 83, txtArticle.Text);

            }
        }

        // 품명 플러스파인더
        private void BtnPfArticle_Click(object sender, RoutedEventArgs e)

        {
            //MainWindow.pf.ReturnCode(txtArticle, (int)Defind_CodeFind.DCF_Article, "");
            //MainWindow.pf.ReturnCode(txtArticle, 83, txtArticle.Text);

            if (tbnJaju.IsChecked == true || tbnProcessCycle.IsChecked == true)
            {
                MainWindow.pf.ReturnCode(txtArticle, txtProcess, txtBuyerArticle, 831, txtArticle.Text);
            }
            else
            {
                MainWindow.pf.ReturnCode(txtArticle, 76, txtArticleSrh.Text);
            }

            if (txtArticle.Tag != null)
            {
                SetBuyerArticleNo(txtArticle.Tag.ToString());
                //txtProcess.Focus();
            }
        }

        // 공정 플러스파인더
        private void txtProcess_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtProcess, (int)Defind_CodeFind.DCF_PROCESS, "");

                if (txtProcess.Tag != null)
                {
                    txtCarModel.Focus();
                }
            }
        }
        // 공정 플러스파인더
        private void btnPfProcess_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtProcess, (int)Defind_CodeFind.DCF_PROCESS, "");

            if (txtProcess.Tag != null)
            {
                txtCarModel.Focus();
            }
        }


        // 차종 플러스파인더
        private void TxtCarModel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtCarModel, (int)Defind_CodeFind.DCF_BUYERMODEL, "");
                txtECONO.Focus();
            }
        }

        // 차종 플러스파인더
        private void BtnPfCarModel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtCarModel, (int)Defind_CodeFind.DCF_BUYERMODEL, "");
        }

        // 저장 실동작
        private bool SaveData(string strID, int Seq)
        {
            //받아 온 ID값 전역변수에 담기.
            SstrID = strID;

            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            //string GetKey = "";

            try
            {
                if (CheckData())
                {
                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Clear();
                    sqlParameter.Add("InspectBasisID", strID);
                    sqlParameter.Add("Seq", Seq);
                    sqlParameter.Add("ArticleID", txtArticle.Tag.ToString());
                    sqlParameter.Add("EcoNo", txtECONO.Text);
                    sqlParameter.Add("Comments", txtComments.Text);

                    sqlParameter.Add("BuyerModelID", txtCarModel.Tag != null ? txtCarModel.Tag.ToString() : "");
                    sqlParameter.Add("InspectPoint", sInspectPoint);
                    sqlParameter.Add("MoldNo", dtpMoldNo.SelectedDate.Value.ToString("yyyyMMdd"));
                    sqlParameter.Add("ProcessID", txtProcess.Tag != null ? txtProcess.Tag.ToString() : "");




                    if (strFlag.Equals("I"))   //추가일 때 
                    {
                        sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Code_iInspectAutoBasis";
                        pro1.OutputUseYN = "Y";
                        pro1.OutputName = "InspectBasisID";
                        pro1.OutputLength = "30";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);


                        List<KeyValue> list_Result = new List<KeyValue>();
                        list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS_NewLog(Prolist, ListParameter,"C");
                        string sGetID = string.Empty;

                        if (list_Result[0].key.ToLower() == "success")
                        {
                            list_Result.RemoveAt(0);
                            for (int i = 0; i < list_Result.Count; i++)
                            {
                                KeyValue kv = list_Result[i];
                                if (kv.key == "InspectBasisID")
                                {
                                    sGetID = kv.value;
                                    flag = true;

                                    strID = kv.value;

                                    //새로 생성시 전역변수에 담기.
                                    SstrID = strID;

                                    Prolist.RemoveAt(0);
                                    ListParameter.Clear();
                                }

                            }

                        }
                        else
                        {
                            MessageBox.Show("[저장실패]\r\n" + list_Result[0].value.ToString());
                            flag = false;
                        }

                        //Sub 저장 프로시저 돌리기
                        for (int i = 0; i < dgdSub.Items.Count; i++)
                        {
                            var SubInsAutoBasis = dgdSub.Items[i] as Win_Qul_InspectAutoBasis_U_Sub_CodeView;

                            if (ButtonEOClickCheck == "EoCheck")
                            {
                                SubInsAutoBasis.stringFlag = "I";
                            }


                            if (SubInsAutoBasis != null)
                            {
                                sqlParameter = new Dictionary<string, object>();
                                sqlParameter.Clear();
                                sqlParameter = ReturnSubListParameter(SubInsAutoBasis, strID, Seq, SubInsAutoBasis.stringFlag);

                                Procedure pro2 = new Procedure();
                                pro2 = ReturSubProcedure(SubInsAutoBasis.stringFlag);
                                if (SubInsAutoBasis.insType.ToString() == "1")
                                {
                                    IsNotNumberic(SubInsAutoBasis.SpecMax);
                                    IsNotNumberic(SubInsAutoBasis.SpecMin);

                                }
                                else if (SubInsAutoBasis.insType.ToString() == "2")
                                {
                                    if (IsNumberic(SubInsAutoBasis.SpecMin) && IsNumberic(SubInsAutoBasis.SpecMax))
                                    {
                                        IsNumberic(SubInsAutoBasis.SpecMax);
                                        IsNumberic(SubInsAutoBasis.SpecMin);

                                    }

                                }
                                Prolist.Add(pro2);
                                ListParameter.Add(sqlParameter);

                            }
                        }

                        string[] Confirm = new string[2];
                        Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew(Prolist, ListParameter);
                        if (Confirm[0] != "success")
                        {
                            MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                            flag = false;
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    else //수정일 때 
                    {
                        sqlParameter.Add("UpdateUserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Code_uInspectAutoBasis";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "InspectBasisID";
                        pro1.OutputLength = "30";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        for (int i = 0; i < dgdSub.Items.Count; i++)
                        {
                            var SubInsAutoBasis = dgdSub.Items[i] as Win_Qul_InspectAutoBasis_U_Sub_CodeView;

                            if (SubInsAutoBasis != null)
                            {
                                sqlParameter = new Dictionary<string, object>();
                                sqlParameter.Clear();
                                sqlParameter = ReturnSubListParameter(SubInsAutoBasis, strID, Seq, SubInsAutoBasis.stringFlag);
                                Procedure pro2 = new Procedure();
                                pro2 = ReturSubProcedure(SubInsAutoBasis.stringFlag);

                                if (SubInsAutoBasis.insTypeText.ToString() == "DIM's")
                                {
                                    if (!IsNumberic(SubInsAutoBasis.SpecMax) && !(IsNumberic(SubInsAutoBasis.SpecMin)))
                                    {
                                        flag = false;
                                        return flag;
                                    }

                                    if (SubInsAutoBasis.InsSampleQty == "" || SubInsAutoBasis.InsSampleQty == "0")
                                    {
                                        MessageBox.Show("샘플수량을 입력해주세요.");
                                        flag = false;
                                        return flag;
                                    }
                                    else if (SubInsAutoBasis.InsSampleQty != "" && !IsNumberic(SubInsAutoBasis.InsSampleQty))
                                    {
                                        MessageBox.Show("샘플수량은 숫자만 입력이 가능합니다.");
                                        flag = false;
                                        return flag;
                                    }
                                }
                                Prolist.Add(pro2);
                                ListParameter.Add(sqlParameter);

                            }

                        }


                        for (int i = 0; i < ovcInspectAutoBasisSub_Delete.Count; i++)
                        {
                            var DelInsAutoBasis = ovcInspectAutoBasisSub_Delete[i] as Win_Qul_InspectAutoBasis_U_Sub_CodeView;

                            if (DelInsAutoBasis != null)
                            {
                                sqlParameter = new Dictionary<string, object>();
                                sqlParameter.Clear();
                                sqlParameter = ReturnSubListParameter(DelInsAutoBasis, strID, Seq, DelInsAutoBasis.stringFlag);

                                Procedure pro3 = new Procedure();
                                pro3 = ReturSubProcedure(DelInsAutoBasis.stringFlag);

                                Prolist.Add(pro3);
                                ListParameter.Add(sqlParameter);
                            }
                        }

                        string[] Confirm = new string[2];
                        Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew_NewLog(Prolist, ListParameter,"U");
                        if (Confirm[0] != "success")
                        {
                            MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                            flag = false;
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                }

                // 파일을 올리자 : GetKey != "" 라면 파일을 올려보자
                if (!strID.Trim().Equals(""))
                {

                    //삭제할 사진이 있을 경우
                    //if (deleteListFtpFile.Count > 0)
                    //{
                    //    foreach (string[] str in deleteListFtpFile)
                    //    {
                    //        FTP_RemoveFile(GetKey + "/" + str[0]);
                    //    }
                    //}

                    //추가한 사진이 있을 때
                    if (listFtpFile.Count > 0 && ButtonEOClickCheck != "EoCheck")
                    {
                        FTP_Save_File(listFtpFile, strID);
                        //AttachFileUpdate(GetKey);
                    }

                    //복사추가 했을 떄 
                    if (ButtonEOClickCheck == "EoCheck" && lstFtpFilePath.Count > 0)
                    {
                        FTP_Save_FileByFtpServerFilePath(lstFtpFilePath, strID);
                    }
                }

                // 파일 List 비워주기
                listFtpFile.Clear();
                //deleteListFtpFile.Clear();
                lstFtpFilePath.Clear();

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


        // FTP Byte 로 저장하기 //설비 쪽에서 가져옴 2020.05.21
        private void FTP_Save_FileByFtpServerFilePath(Dictionary<string, string> lstFtpFilePath, string Key)
        {
            try
            {
                // 폴더 경로 포함해서 다시 생성 후 스따뜨
                _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

                List<string> imgs = new List<string>();

                string[] fileListSimple;
                string[] fileListDetail = null;
                fileListSimple = _ftp.directoryListSimple("", Encoding.Default);

                string MakeFolderName = Key;

                // 기존 폴더 확인작업.
                bool MakeFolder = false;
                MakeFolder = FolderInfoAndFlag(fileListSimple, MakeFolderName);

                if (MakeFolder == false)        // 같은 아이를 찾지 못한경우,
                {
                    //MIL 폴더에 InspectionID로 저장
                    if (_ftp.createDirectory(MakeFolderName) == false)
                    {
                        MessageBox.Show("업로드를 위한 폴더를 생성할 수 없습니다.");
                        return;
                    }
                }
                else
                {
                    fileListDetail = _ftp.directoryListSimple(MakeFolderName, Encoding.Default);
                }

                _ftp.UploadUsingFtpServerFilePath(lstFtpFilePath, Key);
            }
            catch (Exception ep1)
            {
                MessageBox.Show(ep1.Message);
            }
        }




        #region 저장을 위한 동작들

        /// <summary>
        /// 서브 프로시저 추가
        /// </summary>
        private Procedure ReturSubProcedure(string strFlag)
        {
            Procedure pro = new Procedure();

            if (strFlag.Equals("I"))
            {
                pro.Name = "xp_Code_iInspectAutoBasisSub";

            }
            else if (strFlag.Equals("U"))
            {
                pro.Name = "xp_Code_uInspectAutoBasisSub";
            }
            else if (strFlag.Equals("D"))
            {
                pro.Name = "xp_Code_dInspectAutoBasisSub";
            }

            pro.OutputUseYN = "N";
            pro.OutputName = "InspectBasisID";
            pro.OutputLength = "30";

            return pro;
        }

        /// <summary>
        /// 서브 그리드 파라미터 모음
        /// </summary>
        private Dictionary<string, object> ReturnSubListParameter
            (Win_Qul_InspectAutoBasis_U_Sub_CodeView InsAutoBasisSub, string strID, int Seq, string strFlag)
        {
            Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
            sqlParameter.Clear();

            if (!strFlag.Equals("D")) //삭제가 아니라면
            {
                sqlParameter.Add("InspectBasisID", strID);
                sqlParameter.Add("Seq", Seq);
                sqlParameter.Add("SubSeq", InsAutoBasisSub.SubSeq);
                sqlParameter.Add("InsType", InsAutoBasisSub.insType);
                sqlParameter.Add("InsItemName", InsAutoBasisSub.insItemName);

                if (InsAutoBasisSub.insType.Trim().Equals("1"))
                {
                    sqlParameter.Add("InsTPSpec", InsAutoBasisSub.Spec);
                    sqlParameter.Add("InsTPSpecMin", InsAutoBasisSub.SpecMin);
                    sqlParameter.Add("InsTPSpecMax", InsAutoBasisSub.SpecMax);
                    sqlParameter.Add("InsRASpec", "");
                    sqlParameter.Add("InsRASpecMin", 0);      //프로시저 자체에서 추가된다.
                    sqlParameter.Add("InsRASpecMax", 0);      //프로시저 자체에서 추가된다.
                }
                else
                {
                    sqlParameter.Add("InsTPSpec", InsAutoBasisSub.Spec);
                    sqlParameter.Add("InsTPSpecMin", InsAutoBasisSub.SpecMin);
                    sqlParameter.Add("InsTPSpecMax", InsAutoBasisSub.SpecMax);
                    sqlParameter.Add("InsRASpec", InsAutoBasisSub.Spec);

                    if (InsAutoBasisSub.SpecMin.Replace(",", "") == "")
                    {
                        sqlParameter.Add("InsRASpecMin", -99999);
                    }
                    else
                    {
                        sqlParameter.Add("InsRASpecMin", InsAutoBasisSub.SpecMin.Replace(",", ""));
                    }

                    if (InsAutoBasisSub.SpecMax.Replace(",", "") == "")
                    {
                        sqlParameter.Add("InsRASpecMax", 99999);
                    }
                    else
                    {
                        sqlParameter.Add("InsRASpecMax", InsAutoBasisSub.SpecMax.Replace(",", ""));
                    }
                }

                //샘플수량은 빈값 들어가면 안돼, 0 이거나 숫자가 들어가도록.
                sqlParameter.Add("InsSampleQty", InsAutoBasisSub.InsSampleQty == "" ? "0" : InsAutoBasisSub.InsSampleQty);
                sqlParameter.Add("ManageGubun", InsAutoBasisSub.ManageGubun);
                sqlParameter.Add("InspectGage", InsAutoBasisSub.InspectGage);
                sqlParameter.Add("InspectCycleGubun", InsAutoBasisSub.InspectCycleGubun);

                sqlParameter.Add("InspectCycle", InsAutoBasisSub.InspectCycle);
                sqlParameter.Add("Comments", InsAutoBasisSub.Comments);

                sqlParameter.Add("InsImageFile", InsAutoBasisSub.InsImageFile != null ? InsAutoBasisSub.InsImageFile : "");
                sqlParameter.Add("InsImagePath", "/ImageData/" + ForderName + "/" + strID);


                if (strFlag.Equals("I"))
                    sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);
                else if (strFlag.Equals("U"))
                    sqlParameter.Add("UpdateUserID", MainWindow.CurrentUser);
            }
            else   //삭제라면
            {
                //strFlag =D (서브 그리드행 삭제)
                sqlParameter.Add("InspectBasisID", strID);
                sqlParameter.Add("Seq", Seq);
                sqlParameter.Add("SubSeq", InsAutoBasisSub.SubSeq);

            }

            return sqlParameter;
        }

        /// <summary>
        /// 필수 입력 등 체크
        /// </summary>
        private bool CheckData()
        {
            bool flag = true;

            if (txtArticle.Text.Length <= 0 || txtArticle.Text.Equals(""))
            {
                MessageBox.Show("품번이 입력되지 않았습니다.");
                flag = false;
                return flag;
            }
            if (tbnJaju.IsChecked == true || tbnProcessCycle.IsChecked == true)
            {
                if (txtProcess.Text.Length <= 0 || txtProcess.Text.Equals(""))
                {
                    MessageBox.Show("공정이 입력되지 않았습니다.");
                    flag = false;
                    return flag;
                }
            }
            else if (tbnInCome.IsChecked == true || tbnOutCome.IsChecked == true)
            {
                if (txtProcess.Text.Length > 0 || !txtProcess.Text.Equals(""))
                {
                    MessageBox.Show("공정 입력하지 않아도 됩니다.");
                    flag = false;
                    return flag;
                }
            }

            if (dtpMoldNo.SelectedDate == null)
            {
                MessageBox.Show("기준일자를 등록해주세요.");
                flag = false;
                return flag;
            }

            int i = 0;
            for (i = 0; i < dgdSub.Items.Count; i++)
            {
                var dgdSubInput = dgdSub.Items[i] as Win_Qul_InspectAutoBasis_U_Sub_CodeView;

                if (dgdSubInput.insType.ToString().Trim() == "2")
                {
                    if (dgdSubInput.SpecMin != "" && !IsNumberic(dgdSubInput.SpecMin))
                    {
                        MessageBox.Show("수치 하한 값에는 숫자만 입력 가능합니다.\n다시 입력해주세요.", "주의");
                        flag = false;
                        return flag;
                    }

                    if (dgdSubInput.SpecMax != "" && !IsNumberic(dgdSubInput.SpecMax))
                    {
                        MessageBox.Show("수치 상한 값에는 숫자만 입력 가능합니다.\n다시 입력해주세요.", "주의");
                        flag = false;
                        return flag;
                    }

                    //JDJ 추가
                    if (dgdSubInput.SpecMin != "" && IsNumberic(dgdSubInput.SpecMin) && dgdSubInput.SpecMax != "" && IsNumberic(dgdSubInput.SpecMax))
                    {
                        if (Convert.ToDouble(dgdSubInput.SpecMax) < Convert.ToDouble(dgdSubInput.SpecMin))
                        {
                            MessageBox.Show("수치 하한 값이 상한 값보다 큽니다.\n다시 입력해주세요.", "주의");
                            flag = false;
                            return flag;
                        }
                    }
                }
            }

            //내가 살짝 수정한 부분
            int p = 0;
            for (p = 0; p < dgdSub.Items.Count; p++)
            {
                var dgdSubInput = dgdSub.Items[p] as Win_Qul_InspectAutoBasis_U_Sub_CodeView;

                if (dgdSubInput.insType == null || dgdSubInput.insType == "")
                {
                    MessageBox.Show("외관 혹은 DiM's를 선택해주세요.");
                    flag = false;
                    return flag;
                }
                //JDJ 추가
                else if (dgdSubInput.InsSampleQty == "" || dgdSubInput.InsSampleQty == "0")
                {
                    MessageBox.Show("샘플수량을 입력해주세요.");
                    flag = false;
                    return flag;
                }
                else if (dgdSubInput.InsSampleQty != "" && !IsNumberic(dgdSubInput.InsSampleQty))
                {
                    MessageBox.Show("샘플수량은 숫자만 입력이 가능합니다.");
                    flag = false;
                    return flag;
                }

                else if (!IsNumberic(dgdSubInput.InspectCycle))
                {
                    MessageBox.Show("주기는 숫자만 입력이 가능합니다.");
                    flag = false;
                    return flag;
                }
            }
            return flag;
        }

        #endregion

        #region 데이터그리드 내부 입력 동작 모음

        //방향키로 셀 움직일수 있게 이벤트
        private void DataGridSubCell_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                DataGridSubCell_KeyDown(sender, e);
            }
        }

        //내부 핵심 이벤트
        private void DataGridSubCell_KeyDown(object sender, KeyEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                var InsAutoSub = dgdSub.CurrentItem as Win_Qul_InspectAutoBasis_U_Sub_CodeView;
                int rowCount = dgdSub.Items.IndexOf(dgdSub.CurrentItem);
                int colCount = dgdSub.Columns.IndexOf(dgdSub.CurrentCell.Column);
                int lastColcount = dgdSub.Columns.Count - 1;

                //MessageBox.Show(e.Key.ToString());

                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (lastColcount == colCount && dgdSub.Items.Count - 1 > rowCount)
                    {
                        dgdSub.SelectedIndex = rowCount + 1;
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount + 1], dgdSub.Columns[1]);
                    }
                    else if (lastColcount > colCount && dgdSub.Items.Count - 1 > rowCount)
                    {
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount], dgdSub.Columns[colCount + 1]);
                    }
                    else if (lastColcount == colCount && dgdSub.Items.Count - 1 == rowCount)
                    {
                        if (MessageBox.Show("추가하시겠습니까?", "추가 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            AddSubItem();
                            dgdSub.SelectedIndex = rowCount + 1;
                            dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount + 1], dgdSub.Columns[1]);
                        }
                        else
                        {
                            btnSave.Focus();
                        }
                    }
                    else if (lastColcount > colCount && dgdSub.Items.Count - 1 == rowCount)
                    {
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount], dgdSub.Columns[colCount + 1]);
                    }
                    else
                    {
                        //MessageBox.Show("있으면 찾아보자...");
                    }
                }
                else if (e.Key == Key.Delete)
                {
                    e.Handled = true;
                    //selectedIndex_Sub = rowCount;

                    if (strFlag.Equals("U"))
                    {
                        //추가시에는 굳이 delete 프로시저가 필요없다.
                        InsAutoSub.stringFlag = "D";
                        ovcInspectAutoBasisSub_Delete.Add(InsAutoSub);
                    }

                    ovcInspectAutoBasisSub.RemoveAt(rowCount);
                    dgdSub.Refresh();
                    if (dgdSub.Items.Count > 0)
                    {
                        if (dgdSub.Items.Count - 1 > rowCount)
                        {
                            dgdSub.SelectedIndex = rowCount;
                        }
                        else
                        {
                            dgdSub.SelectedIndex = 0;
                        }
                    }
                }
                else if (e.Key == Key.Down)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (dgdSub.Items.Count - 1 > rowCount)
                    {
                        dgdSub.SelectedIndex = rowCount + 1;
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount + 1], dgdSub.Columns[colCount]);
                    }
                    else if (dgdSub.Items.Count - 1 == rowCount)
                    {
                        if (lastColcount > colCount)
                        {
                            dgdSub.SelectedIndex = 0;
                            dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[0], dgdSub.Columns[colCount + 1]);
                        }
                    }
                }
                else if (e.Key == Key.Up)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (rowCount > 0)
                    {
                        dgdSub.SelectedIndex = rowCount - 1;
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount - 1], dgdSub.Columns[colCount]);
                    }
                }
                else if (e.Key == Key.Left)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (colCount > 0)
                    {
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount], dgdSub.Columns[colCount - 1]);
                    }
                }
                else if (e.Key == Key.Right)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (lastColcount > colCount)
                    {
                        dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount], dgdSub.Columns[colCount + 1]);
                    }
                    else if (lastColcount == colCount)
                    {
                        if (dgdSub.Items.Count - 1 > rowCount)
                        {
                            dgdSub.SelectedIndex = rowCount + 1;
                            dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount + 1], dgdSub.Columns[1]);
                        }
                    }
                }
            }
        }

        //셀의 내부 컨트롤에 포커싱
        private void TextBoxFocusInDataGrid(object sender, KeyEventArgs e)
        {
            Lib.Instance.DataGridINControlFocus(sender, e);
        }

        //셀의 내부 컨트롤에 포커싱
        private void TextBoxFocusInDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Lib.Instance.DataGridINBothByMouseUP(sender, e);
        }

        //포커스 오면 셀 EditingMode 전화
        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                DataGridCell cell = sender as DataGridCell;
                cell.IsEditing = true;
            }
        }

        // 측정게이지 셀 클릭 이벤트(PLUS FINDER)
        private void dgdtpeInspectGage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var ViewReceiver = dgdSub.CurrentItem as Win_Qul_InspectAutoBasis_U_Sub_CodeView;

                TextBox textBox = sender as TextBox;
                MainWindow.pf.ReturnCode(textBox, (int)Defind_CodeFind.DCF_InspectGage, "");
            }
        }
        // 측정게이지 셀 클릭 이벤트(PLUS FINDER)
        private void dgdtpeInspectGage_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var ViewReceiver = dgdSub.CurrentItem as Win_Qul_InspectAutoBasis_U_Sub_CodeView;

            TextBox textBox = sender as TextBox;
            MainWindow.pf.ReturnCode(textBox, (int)Defind_CodeFind.DCF_InspectGage, "");
        }


        #endregion

        /// <summary>
        /// 행 추가 동작
        /// </summary>
        void AddSubItem()
        {
            int count = dgdSub.Items.Count + 1;
            var InsAuto = new Win_Qul_InspectAutoBasis_U_Sub_CodeView()
            {
                Num = count,
                InspectBasisID = txtInspectBasisID.Text,
                Seq = txtSeq.Text,
                SubSeq = "0",
                insType = "1",
                insItemName = "",
                Spec = "",
                SpecMax = "",
                SpecMin = "",
                InsSampleQty = "",
                ManageGubun = "",
                ManageGubunname = "",
                InspectGage = "",
                InspectCycleGubun = "",
                InspectCycleGubunName = "",
                InspectCycle = "",
                Comments = "",
                ovcCycle = ovcCycleView,
                ovcManage = ovcManageView,
                ovcType = ovcTypeView,
                stringFlag = "I"    //행을 추가할때는 무조건 I (추가<insert>)Flag
            };

            InsAuto.insTypeText = "외관";
            ovcInspectAutoBasisSub.Add(InsAuto);
            dgdSub.ItemsSource = ovcInspectAutoBasisSub;
            dgdSub.Refresh();
            //dgdSub.Items.Add(InsAuto);
        }

        private void dgdMain_LeftDoubleDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                btnUpdate_Click(btnUpdate, null);
            }

            //btnUpdate_Click(btnUpdate, null);
        }




        #region 포커스 이동용 키 다운 이벤트 모음
        private void txtECONO_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dtpMoldNo.Focus();
            }
        }
        private void txtMoldNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtComments.Focus();
            }
        }
        private void txtComments_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtSeq.Focus();
            }
        }
        private void txtSeq_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtArticle.Focus();
            }
        }

        #endregion


        #region FTP

        //이미지 키다운
        private void dgdtpetxtImage_KeyDown(object sender, KeyEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {

                if (e.Key == Key.Enter)
                {
                    var InspectAutoSub = dgdSub.CurrentItem as Win_Qul_InspectAutoBasis_U_Sub_CodeView;

                    if (InspectAutoSub != null)
                    {
                        if (InspectAutoSub.InsImageFile != null
                                && !InspectAutoSub.InsImageFile.Trim().Equals(string.Empty) && strFlag.Equals("U"))
                        {
                            MessageBox.Show("먼저 해당파일의 삭제를 진행 후 진행해주세요.");
                            return;
                        }
                        else
                        {
                            FTP_Upload_TextBox(sender as TextBox);
                        }
                    }
                }
            }
        }

        //이미지 보기 클릭..
        private void btnSeeImage_Click(object sender, RoutedEventArgs e)
        {
            var InspectAutoSub = dgdSub.CurrentItem as Win_Qul_InspectAutoBasis_U_Sub_CodeView;

            if (InspectAutoSub != null && !InspectAutoSub.InsImageFile.Equals(""))
            {
                //FTP_DownLoadFile(WinMcRegularSub.McImagePath + "/" + WinMcRegularSub.McInspectBasisID + "/" + WinMcRegularSub.McImageFile);
                FTP_DownLoadFile2(InspectAutoSub.InspectBasisID, InspectAutoSub.InsImageFile);
            }
        }

        private void FTP_Upload_TextBox(TextBox textBox)
        {
            if (!textBox.Text.Equals(string.Empty) && strFlag.Equals("U"))
            {
                MessageBox.Show("먼저 해당파일의 삭제를 진행 후 진행해주세요.");
                return;
            }
            else
            {
                Microsoft.Win32.OpenFileDialog OFdlg = new Microsoft.Win32.OpenFileDialog();
                //OFdlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.pcx, *.pdf) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.pcx; *.pdf | All Files|*.*";

                //현장 자주검사시 이미지가 아닌 경우 프로그램 꺼짐현상을 예방하기 위해 이미지 파일 확장자만 보이도록 2020.04.16
                OFdlg.Filter = MainWindow.OFdlg_Filter;

                Nullable<bool> result = OFdlg.ShowDialog();
                if (result == true)
                {
                    strFullPath = OFdlg.FileName;

                    string ImageFileName = OFdlg.SafeFileName;  //명.
                    string ImageFilePath = string.Empty;       // 경로

                    ImageFilePath = strFullPath.Replace(ImageFileName, "");

                    StreamReader sr = new StreamReader(OFdlg.FileName);
                    long FileSize = sr.BaseStream.Length;
                    if (sr.BaseStream.Length > (2048 * 1000))
                    {
                        //업로드 파일 사이즈범위 초과
                        MessageBox.Show("이미지의 파일사이즈가 2M byte를 초과하였습니다.");
                        sr.Close();
                        return;
                    }
                    else
                    {
                        textBox.Text = ImageFileName;
                        textBox.Tag = ImageFilePath;

                        Bitmap image = new Bitmap(ImageFilePath + ImageFileName);

                        var Hoit = textBox.DataContext as Win_Qul_InspectAutoBasis_U_Sub_CodeView;
                        Hoit.ImageView = BitmapToImageSource(image);
                        Hoit.imageFlag = true;
                        //MessageBox.Show(Hoit.McInspectBasisID);

                        //imgSetting.Source = BitmapToImageSource(image);

                        string[] strTemp = new string[] { ImageFileName, ImageFilePath.ToString() };
                        listFtpFile.Add(strTemp);
                    }
                }
            }
        }

        // 비트맵을 비트맵 이미지로 형태변환시키기.<0823 허윤구> 
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private void FTP_DownLoadFile2(string FolderName, string ImageName)
        {
            try
            {
                // 접속 경로
                _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

                string str_path = string.Empty;
                str_path = FTP_ADDRESS + '/' + FolderName;
                _ftp = new FTP_EX(str_path, FTP_ID, FTP_PASS);

                string str_remotepath = ImageName;
                string str_localpath = LOCAL_DOWN_PATH + "\\" + ImageName;

                DirectoryInfo DI = new DirectoryInfo(LOCAL_DOWN_PATH);      // Temp 폴더가 없는 컴터라면, 만들어 줘야지.
                if (DI.Exists == false)
                {
                    DI.Create();
                }

                FileInfo file = new FileInfo(str_localpath);
                if (file.Exists)
                {
                    file.Delete();
                }

                _ftp.download(str_remotepath, str_localpath);

                ProcessStartInfo proc = new ProcessStartInfo(str_localpath);
                proc.UseShellExecute = true;
                Process.Start(proc);
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


        // 파일 저장하기.
        private void FTP_Save_File(List<string[]> listStrArrayFileInfo, string MakeFolderName)
        {
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

            List<string[]> UpdateFilesInfo = new List<string[]>();
            string[] fileListSimple;
            string[] fileListDetail = null;
            fileListSimple = _ftp.directoryListSimple("", Encoding.Default);

            // 기존 폴더 확인작업.
            bool MakeFolder = false;
            MakeFolder = FolderInfoAndFlag(fileListSimple, MakeFolderName);

            if (MakeFolder == false)        // 같은 아이를 찾지 못한경우,
            {
                //MIL 폴더에 InspectionID로 저장
                if (_ftp.createDirectory(MakeFolderName) == false)
                {
                    MessageBox.Show("업로드를 위한 폴더를 생성할 수 없습니다.");

                    //업로드에 실패할 경우 이미지 네임, 패스를 비워주자.
                    List<Procedure> Prolist = new List<Procedure>();
                    List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();


                    for (int i = 0; i < dgdSub.Items.Count; i++)
                    {
                        var SubInsAutoBasis = dgdSub.Items[i] as Win_Qul_InspectAutoBasis_U_Sub_CodeView;

                        Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                        sqlParameter.Clear();
                        sqlParameter.Add("InspectBasisID", SstrID);
                        sqlParameter.Add("SubSeq", SubInsAutoBasis.SubSeq);


                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Code_iuInspectAutoBasisSub_ftp";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "InspectBasisID";
                        pro1.OutputLength = "30";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);
                    }


                    List<KeyValue> list_Result = new List<KeyValue>();
                    list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS(Prolist, ListParameter);
                    string sGetID = string.Empty;

                    if (list_Result[0].key.ToLower() == "success")
                    {
                        list_Result.RemoveAt(0);
                        for (int i = 0; i < list_Result.Count; i++)
                        {
                            KeyValue kv = list_Result[i];
                            if (kv.key == "InspectBasisID")
                            {
                                sGetID = kv.value;
                                //flag = true;

                                //strID = kv.value;

                                Prolist.RemoveAt(0);
                                ListParameter.Clear();
                            }

                        }

                    }
                    else
                    {
                        MessageBox.Show("[저장실패]\r\n" + list_Result[0].value.ToString());
                        //flag = false;
                    }





                    return;
                }
            }
            else
            {
                fileListDetail = _ftp.directoryListSimple(MakeFolderName, Encoding.Default);
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
                    listStrArrayFileInfo[i][0] = MakeFolderName + "/" + listStrArrayFileInfo[i][0];
                    UpdateFilesInfo.Add(listStrArrayFileInfo[i]);
                }
            }

            if (!_ftp.UploadTempFilesToFTP(UpdateFilesInfo))
            {
                //원본
                //MessageBox.Show("파일업로드에 실패하였습니다.");
                //return;

                MessageBox.Show("파일업로드에 실패하였습니다.");

                //업로드에 실패할 경우 이미지 네임, 패스를 비워주자.
                List<Procedure> Prolist = new List<Procedure>();
                List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();


                for (int i = 0; i < dgdSub.Items.Count; i++)
                {
                    var SubInsAutoBasis = dgdSub.Items[i] as Win_Qul_InspectAutoBasis_U_Sub_CodeView;

                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Clear();
                    sqlParameter.Add("InspectBasisID", SstrID);
                    sqlParameter.Add("Seq", SubInsAutoBasis.SubSeq);


                    Procedure pro1 = new Procedure();
                    pro1.Name = "xp_Code_iuInspectAutoBasisSub_ftp";
                    pro1.OutputUseYN = "N";
                    pro1.OutputName = "InspectBasisID";
                    pro1.OutputLength = "30";

                    Prolist.Add(pro1);
                    ListParameter.Add(sqlParameter);
                }


                List<KeyValue> list_Result = new List<KeyValue>();
                list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS(Prolist, ListParameter);
                string sGetID = string.Empty;

                if (list_Result[0].key.ToLower() == "success")
                {
                    list_Result.RemoveAt(0);
                    for (int i = 0; i < list_Result.Count; i++)
                    {
                        KeyValue kv = list_Result[i];
                        if (kv.key == "InspectBasisID")
                        {
                            sGetID = kv.value;
                            //flag = true;

                            //strID = kv.value;

                            Prolist.RemoveAt(0);
                            ListParameter.Clear();
                        }

                    }

                }
                else
                {
                    MessageBox.Show("[저장실패]\r\n" + list_Result[0].value.ToString());
                    //flag = false;
                }

                return;

            }
        }

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


        private BitmapImage SetImage(string ImageName, string FolderName)
        {
            BitmapImage bit = null;
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);
            if (_ftp == null) { return null; }

            bit = DrawingImageByByte(FTP_ADDRESS + '/' + FolderName + '/' + ImageName + "");

            return bit;
        }

        /// <summary>
        /// ftp경로를 가지고 Bitmap 정보 리턴한다
        /// </summary>
        /// <param name="ftpFilePath"></param>
        /// <returns></returns>
        public BitmapImage DrawingImageByByte(string ftpFilePath)
        {
            BitmapImage image = new BitmapImage();

            try
            {
                WebClient ftpClient = new WebClient();
                ftpClient.Credentials = new NetworkCredential(FTP_ID, FTP_PASS);
                byte[] imageByte = ftpClient.DownloadData(ftpFilePath);

                //MemoryStream mStream = new MemoryStream();
                //mStream.Write(imageByte, 0, Convert.ToInt32(imageByte.Length));

                using (MemoryStream stream = new MemoryStream(imageByte))
                {
                    image.BeginInit();
                    image.StreamSource = stream;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze();
                }

            }
            catch (Exception ex)
            {
                if (FtpFirstFlag == false)
                {
                    System.Windows.MessageBox.Show("1" + ex.Message + " / " + ex.Source);
                    //throw ex;
                    FtpFirstFlag = true;
                }
            }

            return image;
        }

        private void txtComments_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Lib.Instance.CheckIsNumeric((TextBox)sender, e);
        }

        private bool IsNumberic(string str)
        {
            Regex regexnumber = new Regex(@"[0-9]");
            return regexnumber.IsMatch(str);

            //bool flag = false;
            //Regex regexnumber = new Regex(@"^\d+");

            //if (regexnumber.IsMatch(str))
            //{
            //    flag = true;
            //    return flag;
            //}

            //return flag;

        }
        private bool IsNotNumberic(string str)
        {
            Regex regexString = new Regex(@"[^가-힣]");
            return regexString.IsMatch(str);
        }

        private bool IsNumberictwoPoint(string str)
        {
            string result = string.Empty;
            Regex regexnumber = new Regex(@"^\d{1,3}([.]\d{1,2})?$");

            Match match = regexnumber.Match(str);
            if (match.Success)
            {
                result = match.Value.ToString();
                if (result.StartsWith("."))
                {
                    result = string.Format("0{0}", result);
                }
            }

            return true;
        }



        private void SubAdd_Click(object sender, RoutedEventArgs e)
        {
            int count = dgdSub.Items.Count + 1;
            var SubInspectAutoBasis = new Win_Qul_InspectAutoBasis_U_Sub_CodeView
            {
                Num = count,
                InspectBasisID = txtInspectBasisID.Text,
                Seq = txtSeq.Text,
                SubSeq = "0",
                insType = "1",          //뭐라도 값이 들어가 있어야 ... instype는 null 허용 안되고, 필수값이다. 1은 외관, 2는 수치
                Spec = "",
                SpecMax = "",
                SpecMin = "",
                InsSampleQty = "",
                ManageGubun = "",
                ManageGubunname = "",
                InspectGage = "",
                InspectCycleGubun = "",
                InspectCycleGubunName = "",
                InspectCycle = "",
                Comments = "",
                ovcCycle = ovcCycleView,
                ovcManage = ovcManageView,
                ovcType = ovcTypeView,
                stringFlag = "I"
            };

            SubInspectAutoBasis.insTypeText = "외관";
            ovcInspectAutoBasisSub.Add(SubInspectAutoBasis);
            dgdSub.ItemsSource = ovcInspectAutoBasisSub;

            dgdSub.Refresh();
        }

        private void SubDel_Click(object sender, RoutedEventArgs e)
        {
            var SubInsAuto = dgdSub.SelectedItem as Win_Qul_InspectAutoBasis_U_Sub_CodeView;
            int rowcount = dgdSub.Items.IndexOf(dgdSub.SelectedItem);
            int colcount = dgdSub.Columns.IndexOf(dgdSub.CurrentCell.Column);
            int lastCount = dgdSub.Columns.Count - 1;

            if (SubInsAuto != null)
            {
                if (strFlag.Equals("U"))
                {
                    SubInsAuto.stringFlag = "D";
                    ovcInspectAutoBasisSub_Delete.Add(SubInsAuto);
                }

                ovcInspectAutoBasisSub.RemoveAt(rowcount);
                dgdSub.Refresh();
                if (dgdSub.Items.Count > 0)
                {
                    if (dgdSub.Items.Count - 1 > rowcount)
                    {
                        dgdSub.SelectedIndex = rowcount;
                    }
                    else
                    {
                        dgdSub.SelectedIndex = 0;
                    }
                }
            }
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
    }
    #endregion FTP 이미지

}
