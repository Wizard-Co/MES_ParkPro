﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_ParkPro.PopUP;
using WizMes_ParkPro.PopUp;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Media;
using System.Linq;

/**************************************************************************************************
'** 프로그램명 : Win_ord_Order_U
'** 설명       : 수주등록
'** 작성일자   : 2023.04.03
'** 작성자     : 장시영
'**------------------------------------------------------------------------------------------------
'**************************************************************************************************
' 변경일자  , 변경자, 요청자    , 요구사항ID      , 요청 및 작업내용
'**************************************************************************************************

'**************************************************************************************************/

namespace WizMes_ParkPro
{
    /// <summary>
    /// Win_ord_Order_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_ord_Order_U : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;

        Lib lib = new Lib();
        PlusFinder pf = new PlusFinder();

        string strFlag = string.Empty;
        int rowNum = 0;

        Win_ord_Order_U_CodeView OrderView = new Win_ord_Order_U_CodeView();

        ArticleData articleData = new ArticleData();
        string PrimaryKey = string.Empty;

        //FTP 활용모음
        string strImagePath = string.Empty;
        string strFullPath = string.Empty;
        string strDelFileName = string.Empty;

        List<string[]> deleteListFtpFile = new List<string[]>(); // 삭제할 파일 리스트
        List<string[]> lstExistFtpFile = new List<string[]>();

        // 촤! FTP Server 에 있는 폴더 + 파일 경로를 저장해놓고 그걸로 다운 및 업로드하자 마!
        // 이미지 이름 : 폴더이름
        Dictionary<string, string> lstFtpFilePath = new Dictionary<string, string>();

        private FTP_EX _ftp = null;
        string SketchPath = null;


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

        string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":" + LoadINI.FTPPort + LoadINI.FtpImagePath + "/Order";
        string ForderName = "Order";
        private const string FTP_ID = "wizuser";
        private const string FTP_PASS = "wiz9999";
        private const string LOCAL_DOWN_PATH = "C:\\Temp";

        ////string FTP_ADDRESS = "ftp://222.104.222.145:25000/ImageData/McRegularInspect";
        //string FTP_ADDRESS = "ftp://192.168.0.95/ImageData/McRegularInspect";


        public Win_ord_Order_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(sender);
            btnToday_Click(null, null);
            SetComboBox();

            txtOrderCount.Text = "0 건";
            txtOrderYds.Text = "0";

            if (MainWindow.tempContent != null
                && MainWindow.tempContent.Count > 0)
            {
                string OrderId = MainWindow.tempContent[0];
                string sDate = MainWindow.tempContent[1];
                string eDate = MainWindow.tempContent[2];
                string chkYN = MainWindow.tempContent[3];


                if (chkYN.Equals("Y"))
                {
                    ChkDateSrh.IsChecked = true;
                }
                else
                {
                    ChkDateSrh.IsChecked = false;
                }

                dtpSDate.SelectedDate = DateTime.Parse(sDate.Substring(0, 4) + "-" + sDate.Substring(4, 2) + "-" + sDate.Substring(6, 2));
                dtpEDate.SelectedDate = DateTime.Parse(eDate.Substring(0, 4) + "-" + eDate.Substring(4, 2) + "-" + eDate.Substring(6, 2));

                chkOrderIDSrh.IsChecked = true;
                txtOrderIDSrh.Text = OrderId;

                //;

                rowNum = 0;
                re_Search(rowNum);

                MainWindow.tempContent.Clear();
            }
        }

        //콤보박스 만들기
        private void SetComboBox()
        {
            // 가공 구분
            ObservableCollection<CodeView> ovcWork = ComboBoxUtil.Instance.GetCode_SetComboBox("Work", null);
            cboWork.ItemsSource = ovcWork;
            cboWork.DisplayMemberPath = "code_name";
            cboWork.SelectedValuePath = "code_id";

            // 주문 형태
            ObservableCollection<CodeView> oveOrderForm = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ORDFRM", "Y", "", "");
            cboOrderForm.ItemsSource = oveOrderForm;
            cboOrderForm.DisplayMemberPath = "code_name";
            cboOrderForm.SelectedValuePath = "code_id";

            // 주문 구분
            ObservableCollection<CodeView> ovcOrderClss = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ORDGBN", "Y", "", "");
            cboOrderClss.ItemsSource = ovcOrderClss;
            cboOrderClss.DisplayMemberPath = "code_name";
            cboOrderClss.SelectedValuePath = "code_id";

            // 주문 구분 (검색)
            cboOrderClassSrh.ItemsSource = ovcOrderClss;
            cboOrderClassSrh.DisplayMemberPath = "code_name";
            cboOrderClassSrh.SelectedValuePath = "code_id";
            cboOrderClassSrh.SelectedIndex = 0;

            // 주문 기준
            ObservableCollection<CodeView> ovcWorkUnitClss = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "CMMUNIT", "Y", "", "");
            cboUnitClss.ItemsSource = ovcWorkUnitClss;
            cboUnitClss.DisplayMemberPath = "code_name";
            cboUnitClss.SelectedValuePath = "code_id";

            // 품명 종류
            ObservableCollection<CodeView> ovcArticleGrpID = ComboBoxUtil.Instance.GetArticleCode_SetComboBox("", 0);
            cboArticleGroup.ItemsSource = ovcArticleGrpID;
            cboArticleGroup.DisplayMemberPath = "code_name";
            cboArticleGroup.SelectedValuePath = "code_id";

            // 부가세 별도
            /*List<string> strVAT_Value = new List<string>();
            strVAT_Value.Add("Y");
            strVAT_Value.Add("N");
            strVAT_Value.Add("0");

            ObservableCollection<CodeView> cboVAT_YN = ComboBoxUtil.Instance.Direct_SetComboBox(strVAT_Value);
            cboVAT_YN.ItemsSource = cboVAT_YN;
            cboVAT_YN.DisplayMemberPath = "code_name";
            cboVAT_YN.SelectedValuePath = "code_name";*/

            List<string[]> strArray = new List<string[]>();
            string[] strOne = { "", "진행" };
            string[] strTwo = { "1", "완료" };
            strArray.Add(strOne);
            strArray.Add(strTwo);

            // 완료 구분
            ObservableCollection<CodeView> ovcCloseClssSrh = ComboBoxUtil.Instance.Direct_SetComboBox(strArray);
            cboCloseClssSrh.ItemsSource = ovcCloseClssSrh;
            cboCloseClssSrh.DisplayMemberPath = "code_name";
            cboCloseClssSrh.SelectedValuePath = "code_id";
            cboCloseClssSrh.SelectedIndex = 0;

            // 수주 구분
            ObservableCollection<CodeView> oveOrderFlag = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "ORDFLG", "Y", "", "");
            //영업, 생산오더만 보여주기 위해.            
            oveOrderFlag.RemoveAt(2);
            //카운트 4에서 하나 지우고 나면 카운트 3돼서 또 2번 지움
            oveOrderFlag.RemoveAt(2);

            cboOrderFlag.ItemsSource = oveOrderFlag;
            cboOrderFlag.DisplayMemberPath = "code_name";
            cboOrderFlag.SelectedValuePath = "code_id";
            cboOrderFlag.SelectedIndex = 1;

            cboOrderNO.ItemsSource = oveOrderFlag;
            cboOrderNO.DisplayMemberPath = "code_name";
            cboOrderNO.SelectedValuePath = "code_id";
            cboOrderNO.SelectedIndex = 1;
        }

        #region 체크박스 연동동작(상단)

        //수주일자
        private void lblDateSrh_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChkDateSrh.IsChecked = ChkDateSrh.IsChecked == true ? false : true;
        }

        //수주일자
        private void ChkDateSrh_Checked(object sender, RoutedEventArgs e)
        {
            if (dtpSDate != null && dtpEDate != null)
            {
                dtpSDate.IsEnabled = true;
                dtpEDate.IsEnabled = true;
            }
        }

        //수주일자
        private void ChkDateSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }

        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        //금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
        }

        //거래처
        private void lblCustomSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chkCustomSrh.IsChecked = chkCustomSrh.IsChecked == true ? false : true;
        }

        //거래처
        private void chkCustomSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtCustomSrh.IsEnabled = true;
            btnPfCustomSrh.IsEnabled = true;
        }

        //거래처
        private void chkCustomSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtCustomSrh.IsEnabled = false;
            btnPfCustomSrh.IsEnabled = false;
        }

        //거래처
        private void txtCustomSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //거래처
        private void btnPfCustomSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //최종고객사
        private void lblInCustomSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chkInCustomSrh.IsChecked = chkInCustomSrh.IsChecked == true ? false : true;
        }

        //최종고객사
        private void chkInCustomSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtInCustomSrh.IsEnabled = true;
            btnPfInCustomSrh.IsEnabled = true;
        }

        //최종고객사
        private void chkInCustomSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtInCustomSrh.IsEnabled = false;
            btnPfInCustomSrh.IsEnabled = false;
        }

        //최종고객사
        private void txtInCustomSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtInCustomSrh, (int)Defind_CodeFind.DCF_BUYERMODEL, "");
        }

        //최종고객사
        private void btnPfInCustomSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtInCustomSrh, (int)Defind_CodeFind.DCF_BUYERMODEL, "");
        }

        //검색조건 - 품번 라벨 클릭
        private void LabelBuyerArticleNoSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (CheckBoxBuyerArticleNoSearch.IsChecked == true)
                {
                    CheckBoxBuyerArticleNoSearch.IsChecked = false;
                }
                else
                {
                    CheckBoxBuyerArticleNoSearch.IsChecked = true;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //검색조건 - 품번 체크박스 체크
        private void CheckBoxBuyerArticleNoSearch_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxBuyerArticleNoSearch.IsEnabled = true;
                ButtonBuyerArticleNoSearch.IsEnabled = true;
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //검색조건 - 품번 체크박스 체크해제
        private void CheckBoxBuyerArticleNoSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxBuyerArticleNoSearch.IsEnabled = false;
                ButtonBuyerArticleNoSearch.IsEnabled = false;
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //검색조건 - 품번 텍스트박스 키다운 이벤트
        private void TextBoxBuyerArticleNoSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    pf.ReturnCode(TextBoxBuyerArticleNoSearch, 76, TextBoxBuyerArticleNoSearch.Text);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //검색조건 - 품번 플러스파인더 버튼
        private void ButtonBuyerArticleNoSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pf.ReturnCode(TextBoxBuyerArticleNoSearch, 76, TextBoxBuyerArticleNoSearch.Text);
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //품명 라벨 클릭
        private void LabelArticleSearch_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (CheckBoxArticleSearch.IsChecked == true)
                {
                    CheckBoxArticleSearch.IsChecked = false;
                }
                else
                {
                    CheckBoxArticleSearch.IsChecked = true;
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //품명 체크박스 체크
        private void CheckBoxArticleSearch_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxArticleSearch.IsEnabled = true;
                ButtonArticleSearch.IsEnabled = true;
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //품명 체크박스 체크해제
        private void CheckBoxArticleSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxArticleSearch.IsEnabled = false;
                ButtonArticleSearch.IsEnabled = false;
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //품명 텍스트박스 키다운 이벤트
        private void TextBoxArticleSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    pf.ReturnCode(TextBoxArticleSearch, 77, TextBoxArticleSearch.Text);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //품명 플러스파인더 버튼
        private void ButtonArticleSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pf.ReturnCode(TextBoxArticleSearch, 77, TextBoxArticleSearch.Text);
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
        }

        //수주번호
        private void lblOrderIDSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chkOrderIDSrh.IsChecked = chkOrderIDSrh.IsChecked == true ? false : true;
        }

        //수주번호
        private void chkOrderIDSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtOrderIDSrh.IsEnabled = true;
        }

        //수주번호
        private void chkOrderIDSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtOrderIDSrh.IsEnabled = false;
        }

        //완료구분
        private void lblCloseClssSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCloseClssSrh.IsChecked == true) { chkCloseClssSrh.IsChecked = false; }
            else { chkCloseClssSrh.IsChecked = true; }
        }

        //완료구분
        private void chkCloseClssSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboCloseClssSrh.IsEnabled = true;
        }

        //완료구분
        private void chkCloseClssSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboCloseClssSrh.IsEnabled = false;
        }

        //가공구분
        private void lblWorkSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chkWorkSrh.IsChecked = chkWorkSrh.IsChecked == true ? false : true;
        }

        //가공구분
        private void chkWorkSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboWorkSrh.IsEnabled = true;
        }

        //가공구분
        private void chkWorkSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboWorkSrh.IsEnabled = false;
        }

        //주문구분
        private void lblOrderClassSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chkOrderClassSrh.IsChecked = chkOrderClassSrh.IsChecked == true ? false : true;
        }

        //주문구분
        private void chkOrderClassSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboOrderClassSrh.IsEnabled = true;
        }

        //주문구분
        private void chkOrderClassSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboOrderClassSrh.IsEnabled = false;
        }

        #endregion

        #region 수주일괄등록복사

        //수주일괄등록복사
        private void btnMassEnrollment_Click(object sender, RoutedEventArgs e)
        {
            popPreviousOrder.IsOpen = true;
        }

        private void popPreviousOrder_Opened(object sender, EventArgs e)
        {
            dtpPreviousMonth.SelectedDate = DateTime.Today.AddMonths(-1);
            dtpThisMonth.SelectedDate = DateTime.Today;
        }

        private void btnPreOrderOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("ChkDate", 1);
                sqlParameter.Add("SDate", dtpThisMonth.SelectedDate.Value.ToString("yyyyMM") + "1");
                sqlParameter.Add("EDate", dtpThisMonth.SelectedDate.Value.ToString("yyyyMM") + "31");
                sqlParameter.Add("ChkCustom", 0);
                sqlParameter.Add("CustomID", "");

                sqlParameter.Add("ChkArticleID", 0);
                sqlParameter.Add("ArticleID", "");
                sqlParameter.Add("ChkBuyerModelID", "");
                sqlParameter.Add("BuyerModelID", "");
                sqlParameter.Add("ChkOrderID", 0);

                sqlParameter.Add("OrderID", "");
                sqlParameter.Add("ChkCloseClss", "");
                sqlParameter.Add("CloseClss", "");
                sqlParameter.Add("ChkWorkID", 0);
                sqlParameter.Add("WorkID", "");

                sqlParameter.Add("ChkOrderClss", 0);
                sqlParameter.Add("OrderClss", "");
                sqlParameter.Add("ChkOrderFlag", 0);
                sqlParameter.Add("OrderFlag", "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_ord_sOrder", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        if (MessageBox.Show(dtpThisMonth.SelectedDate.Value.ToString("yyyyMM") + " 월의 수주가 " + dt.Rows.Count.ToString() + " 건이 있습니다. " +
                            "무시하고 진행하시겠습니까?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            OrderCopy();
                        }
                    }
                    else
                    {
                        if (MessageBox.Show(dtpPreviousMonth.SelectedDate.Value.ToString("yyyyMM") + " 월의 수주가 " + dtpThisMonth.SelectedDate.Value.ToString("yyyyMM") + "월의 수주로 복사됩니다." +
                            "진행하시겠습니까?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                        {
                            OrderCopy();
                        }
                    }
                }
                else
                {
                    if (MessageBox.Show(dtpPreviousMonth.SelectedDate.Value.ToString("yyyyMM") + " 월의 수주가 " + dtpThisMonth.SelectedDate.Value.ToString("yyyyMM") + "월의 수주로 복사됩니다." +
                        "진행하시겠습니까?", "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        OrderCopy();
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

            popPreviousOrder.IsOpen = false;
        }

        private void btnPreOrderCC_Click(object sender, RoutedEventArgs e)
        {
            popPreviousOrder.IsOpen = false;
        }

        private void OrderCopy()
        {
            bool Inresult = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("FromYYYYMM", dtpPreviousMonth.SelectedDate.Value.ToString("yyyyMM"));
                sqlParameter.Add("ToYYYYMM", dtpThisMonth.SelectedDate.Value.ToString("yyyyMM"));  //후에 Tag.Text 로 바꿔야 한다
                sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                Procedure pro1 = new Procedure();
                pro1.Name = "xp_Order_iOrderCopy";
                pro1.OutputUseYN = "N";
                pro1.OutputName = "OrderID";
                pro1.OutputLength = "10";

                Prolist.Add(pro1);
                ListParameter.Add(sqlParameter);

                string[] Confirm = new string[2];
                Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew(Prolist, ListParameter);
                if (Confirm[0] != "success")
                {
                    MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                }
                else
                {
                    MessageBox.Show("수주 복사가 완료 되었습니다.");
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

        #endregion

        /// <summary>
        /// 수정,추가 저장 후
        /// </summary>
        private void CanBtnControl()
        {
            btnAdd.IsEnabled = true;
            btnUpdate.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnSearch.IsEnabled = true;
            btnSave.Visibility = Visibility.Hidden;
            btnCancel.Visibility = Visibility.Hidden;
            btnExcel.Visibility = Visibility.Visible;
            btnUpload.IsEnabled = true;

            grdInput.IsHitTestVisible = false;
            lblMsg.Visibility = Visibility.Hidden;
            dgdMain.IsHitTestVisible = true;
        }

        /// <summary>
        /// 수정,추가 진행 중
        /// </summary>
        private void CantBtnControl()
        {
            btnAdd.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnSearch.IsEnabled = false;
            btnSave.Visibility = Visibility.Visible;
            btnCancel.Visibility = Visibility.Visible;
            btnExcel.Visibility = Visibility.Hidden;
            btnUpload.IsEnabled = false;

            grdInput.IsHitTestVisible = true;
            lblMsg.Visibility = Visibility.Visible;
            dgdMain.IsHitTestVisible = false;
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            strFlag = "I";

            DateTime today = DateTime.Today;

            dtpAcptDate.SelectedDate = today;
            dtpDvlyDate.SelectedDate = today;

            this.DataContext = new object();

            txtAmount.Text = "0";

            //cboVAT_YN 부가세별도 값은 Y로 기본 값 셋팅
            cboVAT_YN.SelectedIndex = 0;

            //자동검사여부는 기본값 N으로
            cboAutoInspect.SelectedItem = cboN;

            //혹시 모르니까 납기일자의 체크박스가 체크되어 있을 수도 있으니까 해제
            chkDvlyDate.IsChecked = false;

            CantBtnControl();

            cboOrderNO.SelectedIndex = 1;
            cboOrderForm.SelectedIndex = 1;
            cboArticleGroup.SelectedIndex = 0;
            cboOrderClss.SelectedIndex = 0;
            cboUnitClss.SelectedIndex = 0;
            cboWork.SelectedIndex = 0;


                     
            btnNeedStuff.IsEnabled = true;
            tbkMsg.Text = "자료 입력 중";
            rowNum = Math.Max(0, dgdMain.SelectedIndex);

            //dtpAcptDate.GetBindingExpression(DatePicker.SelectedDateProperty)?.UpdateTarget();
            //dtpDvlyDate.GetBindingExpression(DatePicker.SelectedDateProperty)?.UpdateTarget();

            if (dgdNeedStuff.Items.Count > 0)
                dgdNeedStuff.Items.Clear();
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            OrderView = dgdMain.SelectedItem as Win_ord_Order_U_CodeView;

            if (OrderView != null)
            {
                //rowNum = dgdMain.SelectedIndex;
                dgdMain.IsHitTestVisible = false;
                btnNeedStuff.IsEnabled = true;
                tbkMsg.Text = "자료 수정 중";
                strFlag = "U";
                CantBtnControl();
                PrimaryKey = OrderView.OrderID;
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
            #region ...
            //한건씩 삭제
            ////btnDelete.IsEnabled = false;

            ////Dispatcher.BeginInvoke(new Action(() =>
            ////{
            ////    OrderView = dgdMain.SelectedItem as Win_ord_Order_U_CodeView;

            ////    if (OrderView == null)
            ////    {
            ////        MessageBox.Show("삭제할 데이터가 지정되지 않았습니다. 삭제데이터를 지정하고 눌러주세요");
            ////    }
            ////    else
            ////    {
            ////        string sql = "select OrderID from pl_Input where OrderID = " + OrderView.OrderID;

            ////        DataSet ds = DataStore.Instance.QueryToDataSet(sql);
            ////        if (ds != null && ds.Tables.Count > 0)
            ////        {
            ////            DataTable dt = ds.Tables[0];
            ////            if (dt.Rows.Count > 0)
            ////            {
            ////                sql = "select OrderID from OutWare where OrderID = " + OrderView.OrderID;

            ////                ds = DataStore.Instance.QueryToDataSet(sql);
            ////                if (ds != null && ds.Tables.Count > 0)
            ////                {
            ////                    dt = ds.Tables[0];
            ////                    string msg = dt.Rows.Count > 0 ?
            ////                        "해당 수주 건은 생산 진행중이오니, 삭제하시려면 생산부터 작업지시까지 먼저 삭제해주세요" :
            ////                        "해당 수주 건은 작업지시 진행중이오니, 삭제하시려면 작업지시 먼저 삭제해주세요";
            ////                    MessageBox.Show(msg);
            ////                }
            ////            }
            ////            else
            ////            {
            ////                if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            ////                {
            ////                    if (dgdMain.Items.Count > 0 && dgdMain.SelectedItem != null)
            ////                        rowNum = dgdMain.SelectedIndex;

            ////                    if (DeleteData(OrderView.OrderID))
            ////                    {
            ////                        rowNum = Math.Max(0, rowNum - 1);
            ////                        re_Search(rowNum);
            ////                    }
            ////                }
            ////            }
            ////        }
            ////    }
            ////}), System.Windows.Threading.DispatcherPriority.Background);

            ////btnDelete.IsEnabled = true;
            ///
            #endregion


            btnDelete.IsEnabled = false;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (dgdMain.SelectedItems.Count == 0)
                {
                    MessageBox.Show("삭제할 데이터가 지정되지 않았습니다. 삭제할 데이터를 선택해주세요.");
                    return;
                }

                var selectedOrders = dgdMain.SelectedItems.Cast<Win_ord_Order_U_CodeView>().ToList();
                var orderIdsToDelete = new List<string>();

                foreach (var order in selectedOrders)
                {
                    if (!CanDeleteOrder(order.OrderID))
                    {
                        return; // 삭제할 수 없는 수주가 있으면 중단
                    }
                    orderIdsToDelete.Add(order.OrderID);
                }

                if (MessageBox.Show($"선택하신 {orderIdsToDelete.Count}개의 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    bool allDeleted = true;
                    foreach (var orderId in orderIdsToDelete)
                    {
                        if (!DeleteData(orderId))
                        {
                            allDeleted = false;
                            break;
                        }
                    }

                    if (allDeleted)
                    {
                        MessageBox.Show("선택한 모든 항목이 성공적으로 삭제되었습니다.");
                        re_Search(0); // 첫 번째 행부터 다시 검색
                    }
                    else
                    {
                        MessageBox.Show("일부 항목 삭제 중 오류가 발생했습니다.");
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
            btnDelete.IsEnabled = true;
        }


        private bool CanDeleteOrder(string orderId)
        {
            string sql = $"select OrderID from pl_Input where OrderID = {orderId}";
            DataSet ds = DataStore.Instance.QueryToDataSet(sql);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                sql = $"select OrderID from OutWare where OrderID = {orderId}";
                ds = DataStore.Instance.QueryToDataSet(sql);
                if (ds != null && ds.Tables.Count > 0)
                {
                    string msg = ds.Tables[0].Rows.Count > 0 ?
                        $"해당 수주번호: {orderId} 건은 생산 진행중이오니, 삭제하시려면 생산부터 작업지시까지 먼저 삭제해주세요" :
                        $"해당 수주번호: {orderId} 작업지시 진행중이오니, 삭제하시려면 작업지시 먼저 삭제해주세요";
                    MessageBox.Show(msg);
                    return false;
                }
            }
            return true;
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            Lib.Instance.ChildMenuClose(this.ToString());
        }

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
            rowNum = 0;
            btnSearch.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //로직
                re_Search(rowNum);
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
            btnSave.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //로직
                if (SaveData(strFlag))
                {
                    CanBtnControl();
                    lblMsg.Visibility = Visibility.Hidden;
                    dgdMain.IsHitTestVisible = true;
                    btnNeedStuff.IsEnabled = false;
                    re_Search(rowNum);
                    PrimaryKey = string.Empty;
                    rowNum = 0;
                    MessageBox.Show("저장이 완료되었습니다.");
                }
            }), System.Windows.Threading.DispatcherPriority.Background);

            btnSave.IsEnabled = true;
        }

        //취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CanBtnControl();

            //혹시 모르니까 납기일자의 체크박스가 체크되어 있을 수도 있으니까 해제
            chkDvlyDate.IsChecked = false;

            dgdMain.IsHitTestVisible = true;
            btnNeedStuff.IsEnabled = false;

            if (strFlag.Equals("U"))
            {
                re_Search(rowNum);
            }
            else
            {
                rowNum = 0;
                re_Search(rowNum);
            }

            strFlag = string.Empty;

        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;
            Lib lib = new Lib();

            string[] lst = new string[2];
            lst[0] = "수주 조회 목록";
            lst[1] = dgdMain.Name;

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

        // 주문일괄 업로드
        string upload_fileName = "";

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog file = new Microsoft.Win32.OpenFileDialog();
            file.Filter = "Excel files (*.xls,*xlsx)|*.xls;*xlsx|All files (*.*)|*.*";
            file.InitialDirectory = "C:\\";

            if (file.ShowDialog() == true)
            {
                upload_fileName = file.FileName;

                btnUpload.IsEnabled = false;

                using (Loading ld = new Loading("excel", beUpload))
                {
                    ld.ShowDialog();
                }

                re_Search(0);

                btnUpload.IsEnabled = true;
            }
        }

        private void beUpload()
        {
            Lib lib2 = new Lib();

            Excel.Application excelapp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;
            Excel.Range workrange = null;

            List<OrderExcel> listExcel = new List<OrderExcel>();

            try
            {
                excelapp = new Excel.Application();
                workbook = excelapp.Workbooks.Add(upload_fileName);
                worksheet = workbook.Sheets["Sheet"];
                workrange = worksheet.UsedRange;

                for (int row = 3; row <= workrange.Rows.Count; row++)
                {
                    OrderExcel excel = new OrderExcel();
                    excel.CustomID = workrange.get_Range("A" + row.ToString()).Value2;
                    excel.Model = workrange.get_Range("B" + row.ToString()).Value2;
                    excel.BuyerArticleNo = workrange.get_Range("C" + row.ToString()).Value2;
                    excel.Article = workrange.get_Range("D" + row.ToString()).Value2;
                    excel.UnitClss = workrange.get_Range("E" + row.ToString()).Value2;

                    object objOrderQty = workrange.get_Range("H" + row.ToString()).Value2;
                    if (objOrderQty != null)
                        excel.OrderQty = objOrderQty.ToString();

                    if (!string.IsNullOrEmpty(excel.CustomID)
                        && !string.IsNullOrEmpty(excel.BuyerArticleNo) && !string.IsNullOrEmpty(excel.Article)
                        && !string.IsNullOrEmpty(excel.UnitClss) && !string.IsNullOrEmpty(excel.OrderQty))
                    {
                        listExcel.Add(excel);
                    }

                    if (string.IsNullOrEmpty(excel.CustomID) && string.IsNullOrEmpty(excel.Model)
                        && string.IsNullOrEmpty(excel.BuyerArticleNo) && string.IsNullOrEmpty(excel.Article)
                        && string.IsNullOrEmpty(excel.UnitClss) && string.IsNullOrEmpty(excel.OrderQty))
                    {
                        break;
                    }
                }

                if (listExcel.Count > 0)
                {
                    List<Procedure> Prolist = new List<Procedure>();
                    List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();
                    for (int i = 0; i < listExcel.Count; i++)
                    {
                        Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                        sqlParameter.Add("CustomID", string.IsNullOrEmpty(listExcel[i].CustomID) ? "" : listExcel[i].CustomID);
                        sqlParameter.Add("Model", string.IsNullOrEmpty(listExcel[i].Model) ? "" : listExcel[i].Model);
                        sqlParameter.Add("BuyerArticleNo", string.IsNullOrEmpty(listExcel[i].BuyerArticleNo) ? "" : listExcel[i].BuyerArticleNo);
                        sqlParameter.Add("Article", string.IsNullOrEmpty(listExcel[i].Article) ? "" : listExcel[i].Article);
                        sqlParameter.Add("UnitClss", string.IsNullOrEmpty(listExcel[i].UnitClss) ? "" : listExcel[i].UnitClss);
                        sqlParameter.Add("OrderQty", string.IsNullOrEmpty(listExcel[i].OrderQty) ? "" : listExcel[i].OrderQty);

                        Procedure pro2 = new Procedure();
                        pro2.Name = "xp_Order_iOrderExcel";
                        pro2.OutputUseYN = "N";
                        pro2.OutputName = "";
                        pro2.OutputLength = "10";

                        Prolist.Add(pro2);
                        ListParameter.Add(sqlParameter);
                    }

                    string[] Confirm = new string[2];
                    Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew_NewLog(Prolist, ListParameter, "C");
                    if (Confirm[0] != "success")
                        MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                    else
                        MessageBox.Show("업로드가 완료되었습니다.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            finally
            {
                excelapp.Visible = true;
                workbook.Close(true);
                excelapp.Quit();

                lib2.ReleaseExcelObject(workbook);
                lib2.ReleaseExcelObject(worksheet);
                lib2.ReleaseExcelObject(excelapp);
                lib2 = null;

                upload_fileName = "";
                listExcel.Clear();
            }
        }

        private int SelectItem(string strPrimary, DataGrid dataGrid)
        {
            int index = 0;

            try
            {
                for (int i = 0; i < dataGrid.Items.Count; i++)
                {
                    var Item = dataGrid.Items[i] as Win_ord_Order_U_CodeView;

                    if (strPrimary.Equals(Item.OrderID))
                    {
                        index = i;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return index;
        }

        private void re_Search(int selectedIndex)
        {
            FillGrid();

            if (dgdMain.Items.Count > 0)
            {
                dgdMain.SelectedIndex = PrimaryKey.Equals(string.Empty) ?
                    selectedIndex : SelectItem(PrimaryKey, dgdMain);
            }
            else
                DataContext = new object();

            CalculGridSum();
        }

        //실조회
        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
                dgdMain.Items.Clear();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("ChkDate", ChkDateSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SDate", ChkDateSrh.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EDate", ChkDateSrh.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");

                // 거래처
                sqlParameter.Add("ChkCustom", chkCustomSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("CustomID", chkCustomSrh.IsChecked == true ? (txtCustomSrh.Tag != null ? txtCustomSrh.Tag.ToString() : "") : "");
                // 최종고객사
                sqlParameter.Add("ChkInCustom", chkInCustomSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("InCustomID", chkInCustomSrh.IsChecked == true ? (txtInCustomSrh.Tag != null ? txtInCustomSrh.Tag.ToString() : "") : "");


                // 품번
                sqlParameter.Add("ChkArticleID", CheckBoxBuyerArticleNoSearch.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleID", CheckBoxBuyerArticleNoSearch.IsChecked == true ? (TextBoxBuyerArticleNoSearch.Tag == null ? "" : TextBoxBuyerArticleNoSearch.Tag.ToString()) : "");
                // 품명
                sqlParameter.Add("ChkArticle", CheckBoxArticleSearch.IsChecked == true ? 1 : 0);
                sqlParameter.Add("Article", CheckBoxArticleSearch.IsChecked == true ? (TextBoxArticleSearch.Text == string.Empty ? "" : TextBoxArticleSearch.Text) : "");


                // 관리번호
                sqlParameter.Add("ChkOrderID", chkOrderIDSrh.IsChecked == true ? (rbnOrderID.IsChecked == true ? 1 : 2) : 0);
                sqlParameter.Add("OrderID", chkOrderIDSrh.IsChecked == true ? (txtOrderIDSrh.Text == string.Empty ? "" : txtOrderIDSrh.Text) : "");
                // 완료구분
                sqlParameter.Add("ChkCloseClss", chkCloseClssSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("CloseClss", chkCloseClssSrh.IsChecked == true ? (cboCloseClssSrh.SelectedValue == null ? "" : cboCloseClssSrh.SelectedValue.ToString()) : "");                


                // 주문구분
                sqlParameter.Add("ChkOrderClss", chkOrderClassSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("OrderClss", chkOrderClassSrh.IsChecked == true ? (cboOrderClassSrh.SelectedValue == null ? "" : cboOrderClassSrh.SelectedValue.ToString()) : "");
                // 수주구분
                sqlParameter.Add("ChkOrderFlag", chkOrderFlag.IsChecked == true ? 1 : 0);
                sqlParameter.Add("OrderFlag", chkOrderFlag.IsChecked == true ? (cboOrderFlag.SelectedValue == null ? "" : cboOrderFlag.SelectedValue.ToString()) : "");


                DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_ord_sOrder", sqlParameter, true, "R");

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count == 0)
                    {
                        ClearInputGrid();
                        MessageBox.Show("조회된 데이터가 없습니다.");                        
                    }
                    else
                    {
                        int i = 0;
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            i++;
                            var OrderCodeView = new Win_ord_Order_U_CodeView
                            {
                                Num = i,

                                OrderID = dr["OrderID"].ToString(),
                                CustomID = dr["CustomID"].ToString(),
                                KCustom = dr["KCustom"].ToString(),
                                CompanyID = dr["CompanyID"].ToString(),
                                BuyerID = dr["BuyerID"].ToString(),

                                InCustomID = dr["InCustomID"].ToString(),
                                OrderNo = dr["OrderNo"].ToString(),
                                PoNo = dr["PoNo"].ToString(),
                                BuyerModelID = dr["BuyerModelID"].ToString(),
                                BuyerModel = dr["BuyerModel"].ToString(),

                                OrderForm = dr["OrderForm"].ToString(),
                                OrderFormName = dr["OrderFormName"].ToString(),
                                OrderClss = dr["OrderClss"].ToString(),
                                OrderClssName = dr["OrderClssName"].ToString(),
                                BrandClss = dr["BrandClss"].ToString(),
                                AcptDate = dr["AcptDate"].ToString(),
                                DvlyDate = dr["DvlyDate"].ToString(),

                                DvlyPlace = dr["DvlyPlace"].ToString(),
                                WorkID = dr["WorkID"].ToString(),
                                WorkName = dr["WorkName"].ToString(),
                                ExchRate = dr["ExchRate"].ToString(),
                                OrderQty = dr["OrderQty"].ToString(),
                                UnitClss = dr["UnitClss"].ToString(),

                                Vat_IND_YN = dr["Vat_IND_YN"].ToString(),
                                ModifyClss = dr["ModifyClss"].ToString(),
                                ModifyRemark = dr["ModifyRemark"].ToString(),
                                CancelRemark = dr["CancelRemark"].ToString(),

                                Remark = dr["Remark"].ToString(),
                                CloseClss = dr["CloseClss"].ToString(),
                                //CloseDate = dr["CloseDate"].ToString(),
                                OrderFlag = dr["OrderFlag"].ToString(),
                                //OrderEnd = dr["OrderEnd"].ToString(),

                                OrderSpec = dr["OrderSpec"].ToString(),
                                KInCustom = dr["KInCustom"].ToString(),

                                ProductAutoInspectYN = dr["ProductAutoInspectYN"].ToString(),
                                ArticleGrpID = dr["ArticleGrpID"].ToString(),
                                ArticleID = dr["ArticleID"].ToString(),
                                Article = dr["Article"].ToString(),
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),

                                UnitPrice = stringFormatN0(dr["UnitPrice"]),
                            };

                            if (Lib.Instance.IsNumOrAnother(OrderCodeView.Amount))
                            {
                                OrderCodeView.Amount_CV = Lib.Instance.returnNumStringZero(OrderCodeView.Amount);
                            }

                            if (OrderCodeView.DvlyDate != null && !OrderCodeView.DvlyDate.Equals(string.Empty))
                            {
                                OrderCodeView.DvlyDate_CV = Lib.Instance.StrDateTimeBar(OrderCodeView.DvlyDate);
                            }

                            if (OrderCodeView.AcptDate != null && !OrderCodeView.AcptDate.Equals(string.Empty))
                            {
                                OrderCodeView.AcptDate_CV = Lib.Instance.StrDateTimeBar(OrderCodeView.AcptDate);
                            }

                            OrderCodeView.OrderQty = Lib.Instance.returnNumStringZero(OrderCodeView.OrderQty);
                            //OrderCodeView.NewArticleQty = Lib.Instance.returnNumStringZero(OrderCodeView.NewArticleQty);
                            //OrderCodeView.RePolishingQty = Lib.Instance.returnNumStringZero(OrderCodeView.RePolishingQty);
                            //OrderCodeView.UnitPrice = Lib.Instance.returnNumStringZero(OrderCodeView.UnitPrice);

                            dgdMain.Items.Add(OrderCodeView);
                        }
                    }
                }

                if (dgdMain.Items.Count > 0)
                {
                    dgdMain.Focus();
                    dgdMain.SelectedIndex = rowNum;
                    dgdMain.CurrentCell = dgdMain.SelectedCells[0];
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

        private DataRow FillOneOrderData(string strOrderID)
        {
            DataRow dr = null;

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("OrderID", strOrderID);
                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Order_sOrderOne", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;
                        dr = drc[0];
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

            return dr;
        }

        //그리드 하단 합계 표시
        private void CalculGridSum()
        {
            Int64 numYDS = 0;
            double numTotal = 0;

            txtOrderCount.Text = string.Format("{0:N0}", dgdMain.Items.Count) + " 건";
            if (dgdMain.Items.Count > 0)
            {
                Win_ord_Order_U_CodeView WinOrder = new Win_ord_Order_U_CodeView();

                for (int i = 0; i < dgdMain.Items.Count; i++)
                {
                    WinOrder = dgdMain.Items[i] as Win_ord_Order_U_CodeView;

                    if (WinOrder.UnitClss.Equals("0"))
                    {
                        numYDS += long.Parse(lib.CheckNullZero(WinOrder.OrderQty.Replace(",", "")));
                        numTotal += double.Parse(lib.CheckNullZero(WinOrder.UnitPrice.Replace(",", "")));
                    }
                    else
                    {
                        numYDS += long.Parse(lib.CheckNullZero(WinOrder.OrderQty.Replace(",", "")));
                        numTotal += double.Parse(lib.CheckNullZero(WinOrder.UnitPrice.Replace(",", "")));
                    }
                }
            }

            txtOrderYds.Text = string.Format("{0:N0}", numYDS) + " EA";
            txtOrderAmount.Text = string.Format("{0:N0}", numTotal) + " 원";
        }

        /// <summary>
        /// 실삭제
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        private bool DeleteData(string strID)
        {
            #region ...
            //한건씩 삭제
            //bool flag = false;

            //try
            //{
            //    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
            //    sqlParameter.Clear();
            //    sqlParameter.Add("OrderID", strID);

            //    string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_Order_dOrder", sqlParameter, "D");

            //    if (result[0].Equals("success"))
            //    {
            //        //MessageBox.Show("성공 *^^*");
            //        flag = true;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            //}
            //finally
            //{
            //    DataStore.Instance.CloseConnection();
            //}

            //return flag;
            #endregion

            bool flag = false;
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("OrderID", strID);
                string[] result = DataStore.Instance.ExecuteProcedure_NewLog("xp_Order_dOrder", sqlParameter, "D");
                if (result[0].Equals("success"))
                {
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
        /// 실저장
        /// </summary>
        /// <param name="strID"></param>
        /// <returns></returns>
        private bool SaveData(string strFlag)
        {
            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                if (CheckData())
                {
                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Clear();

                    sqlParameter.Add("OrderID", string.IsNullOrEmpty(txtOrderID.Text) ? "" : txtOrderID.Text);
                    sqlParameter.Add("OrderFlag", cboOrderNO.SelectedValue != null ? cboOrderNO.SelectedValue.ToString() : "");
                    sqlParameter.Add("OrderForm", cboOrderForm.SelectedValue != null ? cboOrderForm.SelectedValue.ToString() : "");
                    sqlParameter.Add("OrderNO", string.IsNullOrEmpty(TextBoxOrderNo.Text) ? "" : TextBoxOrderNo.Text);

                    sqlParameter.Add("OrderClss", cboOrderClss.SelectedValue != null ? cboOrderClss.SelectedValue.ToString() : "");
                    sqlParameter.Add("CustomID", txtCustom.Tag != null ? txtCustom.Tag.ToString() : "");
                    sqlParameter.Add("OrderQty", int.Parse(txtAmount.Text.Replace(",", "")));
                    sqlParameter.Add("DvlyPlace", string.IsNullOrEmpty(txtDylvLoc.Text) ? "" : txtDylvLoc.Text);

                    sqlParameter.Add("UnitClss", cboUnitClss.SelectedValue != null ? cboUnitClss.SelectedValue.ToString() : "");
                    sqlParameter.Add("InCustomID", txtInCustom.Tag != null ? txtInCustom.Tag.ToString() : "");
                    sqlParameter.Add("ArticleGrpID", cboArticleGroup.SelectedValue != null ? cboArticleGroup.SelectedValue.ToString() : "");
                    sqlParameter.Add("AcptDate", dtpAcptDate.SelectedDate.Value.ToString("yyyyMMdd"));

                    sqlParameter.Add("DvlyDate", chkDvlyDate.IsChecked == true ? dtpDvlyDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                    sqlParameter.Add("UnitPrice", ConvertDouble(txtUnitPrice.Text));
                    sqlParameter.Add("WorkID", cboWork.SelectedValue != null ? cboWork.SelectedValue.ToString() : "");
                    sqlParameter.Add("Remark", string.IsNullOrEmpty(txtComments.Text) ? "" : txtComments.Text);

                    sqlParameter.Add("PoNo", string.IsNullOrEmpty(txtPONO.Text) ? "" : txtPONO.Text);
                    sqlParameter.Add("BuyerModelID", txtModel.Tag != null ? txtModel.Tag.ToString() : "");
                    sqlParameter.Add("ExchRate", 0.00);
                    sqlParameter.Add("UnitPriceClss", "0");
                    sqlParameter.Add("OrderSpec", "");

                    sqlParameter.Add("Vat_IND_YN", "Y");
                    sqlParameter.Add("ProductAutoInspectYN", "N");


                    string sGetID = strFlag.Equals("I") ? string.Empty : txtOrderID.Text;
                    #region 추가

                    if (strFlag.Equals("I"))
                    {
                        sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_Order_iOrder";
                        pro1.OutputUseYN = "Y";
                        pro1.OutputName = "OrderID";
                        pro1.OutputLength = "10";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        List<KeyValue> list_Result = new List<KeyValue>();
                        list_Result = DataStore.Instance.ExecuteAllProcedureOutputGetCS_NewLog(Prolist, ListParameter,"C");

                        if (list_Result[0].key.ToLower() == "success")
                        {
                            list_Result.RemoveAt(0);
                            for (int i = 0; i < list_Result.Count; i++)
                            {
                                KeyValue kv = list_Result[i];
                                if (kv.key == "OrderID")
                                {
                                    sGetID = kv.value;
                                    PrimaryKey = sGetID;
                                    flag = true;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("[저장실패]\r\n" + list_Result[1].value.ToString());
                            //flag = false;
                            return false;
                        }

                        Prolist.Clear();
                        ListParameter.Clear();
                    }
                    #endregion

                    #region 수정
                    else if (strFlag.Equals("U"))
                    {
                        sqlParameter.Add("LastUpdateUserID", MainWindow.CurrentUser);

                        Procedure pro3 = new Procedure();
                        pro3.Name = "xp_Order_uOrder";
                        pro3.OutputUseYN = "N";
                        pro3.OutputName = "OrderID";
                        pro3.OutputLength = "10";

                        Prolist.Add(pro3);
                        ListParameter.Add(sqlParameter);
                    }
                    #endregion

                    //Sub 그리드 추가
                    sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Clear();

                    sqlParameter.Add("OrderID", sGetID);
                    sqlParameter.Add("OrderSeq", 1);
                    sqlParameter.Add("ArticleID", txtBuyerArticleNO.Tag != null ? txtBuyerArticleNO.Tag.ToString() : "");
                    sqlParameter.Add("ArticleGrpID", cboArticleGroup.SelectedValue != null ? cboArticleGroup.SelectedValue.ToString() : "");
                    sqlParameter.Add("UnitPrice", ConvertDouble(txtUnitPrice.Text));
                    sqlParameter.Add("ColorQty", int.Parse(txtAmount.Text.Replace(",", "")));
                    sqlParameter.Add("NewProductYN", "");
                    sqlParameter.Add("UnitPriceClss", "1");
                    sqlParameter.Add("UnitClss", cboUnitClss.SelectedValue.ToString());

                    sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                    Procedure pro2 = new Procedure();
                    pro2.Name = "xp_ord_iOrderSub";
                    pro2.OutputUseYN = "N";
                    pro2.OutputName = "OrderID";
                    pro2.OutputLength = "10";

                    Prolist.Add(pro2);
                    ListParameter.Add(sqlParameter);

                    string[] Confirm = new string[2];
                    Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew_NewLog(Prolist, ListParameter,"U");
                    if (Confirm[0] != "success")
                    {
                        MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                        flag = false;
                    }
                    else
                        flag = true;
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

            return flag;
        }

        private bool CheckData()
        {
            string msg = "";

            if (txtCustom.Text.Length <= 0 || txtCustom.Tag == null)
                msg = "거래처가 입력되지 않았습니다. 먼저 거래처를 입력해주세요";
            else if (txtAmount.Text.Length <= 0)
                msg = "총 주문량이 입력되지 않았습니다. 먼저 총 주문량을 입력해주세요";
            else if (cboOrderForm.SelectedValue == null)
                msg = "주문형태가 선택되지 않았습니다. 먼저 주문형태를 선택해주세요";
            else if (cboOrderClss.SelectedValue == null)
                msg = "주문구분이 선택되지 않았습니다. 먼저 주문구분을 선택해주세요";
            else if (cboUnitClss.SelectedValue == null)
                msg = "주문기준이 선택되지 않았습니다. 먼저 주문기준을 선택해주세요";
            else if (cboArticleGroup.SelectedValue == null)
                msg = "품명종류가 선택되지 않았습니다. 먼저 품명종류를 선택해주세요";
            else if (string.IsNullOrEmpty(txtBuyerArticleNO.Text) || txtBuyerArticleNO.Tag == null)
                msg = "품번이 선택되지 않았습니다. 먼저 품번을 선택해주세요";
            else if (cboWork.SelectedValue == null)
                msg = "가공구분이 선택되지 않았습니다. 먼저 가공구분을 선택해주세요";
            /*else if (cboVAT_YN.SelectedValue == null)
                msg = "부가세별도여부가 선택되지 않았습니다. 먼저 부가세별도여부를 선택해주세요");*/


            //작지, 출고 이력 있으면 변경 안 됨
            if (OrderView.OrderID != null)
            {
                string sql = "select OrderID from pl_Input where OrderID = " + OrderView.OrderID;
                DataSet ds = DataStore.Instance.QueryToDataSet(sql);
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        sql = "select OrderID from OutWare where OrderID = " + OrderView.OrderID;

                        ds = DataStore.Instance.QueryToDataSet(sql);
                        if (ds != null && ds.Tables.Count > 0)
                        {
                            dt = ds.Tables[0];
                            msg = dt.Rows.Count > 0 ?
                                "해당 수주 건은 생산 진행중이오니, 변경하시려면 생산부터 작업지시까지 먼저 삭제해주세요" :
                                "해당 수주 건은 작업지시 진행중이오니, 변경 하시려면 작업지시 진행 관리에서 먼저 삭제해주세요.";
                     
                        }
                    }
                }
            }


            bool flag = true;
            if (!string.IsNullOrEmpty(msg))
            {
                MessageBox.Show(msg);
                flag = false;
            }

            return flag;
        }

        #region 입력시 Event
        //거래처
        private void txtCustom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");

                if (txtCustom.Tag != null)
                {
                    //CallCustomData(txtCustom.Tag.ToString());
                    txtDylvLoc.Text = txtCustom.Text;
                    txtInCustom.Text = txtCustom.Text;
                    txtInCustom.Tag = txtCustom.Tag;
                }

                e.Handled = true;
            }
        }

        //거래처
        private void btnPfCustom_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtCustom, (int)Defind_CodeFind.DCF_CUSTOM, "");

            if (txtCustom.Tag != null)
            {
                //CallCustomData(txtCustom.Tag.ToString());
                txtDylvLoc.Text = txtCustom.Text;
                txtInCustom.Text = txtCustom.Text;
                txtInCustom.Tag = txtCustom.Tag;
            }
        }

        //품명
        private void txtArticle_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    if (txtCustom != null && txtCustom.Text != "")
                    {   //선택된 납품거래처에 따른 품명만 보여주게
                        //MainWindow.pf.ReturnCode(txtArticle, 57, txtCustom.Tag.ToString().Trim());

                        //품번을 품명처럼 쓴다고 해서 품번을 조회하도록 2020.03.17, 장가빈
                        MainWindow.pf.ReturnCodeGLS(txtArticle, 7070, txtCustom.Tag.ToString().Trim());

                    }
                    else
                    {   //선택된 납품거래처가 없다면 전체 품명 다 보여주게
                        //MainWindow.pf.ReturnCode(txtArticle, (int)Defind_CodeFind.DCF_Article, "");

                        //품번을 품명처럼 쓴다고 해서 품번을 조회하도록 2020.03.17, 장가빈
                        MainWindow.pf.ReturnCodeGLS(txtArticle, 7071, "");
                    }


                    if (txtArticle.Tag != null)
                    {
                        CallArticleData(txtArticle.Tag.ToString());
                        //품명종류 대입(ex.제품 등)
                        cboArticleGroup.SelectedValue = articleData.ArticleGrpID;
                        //품번 대입
                        //txtBuyerArticleNO.Text = articleData.BuyerArticleNo;
                        //품명 대입
                        txtBuyerArticleNO.Text = articleData.BuyerArticleNo;
                        //단가 대입
                        txtUnitPrice.Text = articleData.OutUnitPrice;

                    }

                    //플러스 파인더 작동 후 규격으로 커서 이동
                    txtSpec.Focus();
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
        private void btnPfArticle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCustom != null && txtCustom.Text != "")
                {   //선택된 납품거래처에 따른 품명만 보여주게
                    MainWindow.pf.ReturnCodeGLS(txtArticle, 7070, txtCustom.Tag.ToString().Trim());
                }
                else
                {   //선택된 납품거래처가 없다면 전체 품명 다 보여주게
                    MainWindow.pf.ReturnCodeGLS(txtArticle, 7071, "");
                }

                if (txtArticle.Tag != null)
                {
                    CallArticleData(txtArticle.Tag.ToString());
                    //품명종류 대입(ex.제품 등)
                    cboArticleGroup.SelectedValue = articleData.ArticleGrpID;
                    //품번 대입
                    //txtBuyerArticleNO.Text = articleData.BuyerArticleNo;
                    //품명 대입
                    //txtBuyerArticleNO.Text = articleData.Article;
                    //단가 대입
                    txtUnitPrice.Text = articleData.OutUnitPrice;
                }

                //플러스 파인더 작동 후 규격으로 커서 이동
                txtSpec.Focus();
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

        //차종 키다운 
        private void txtModel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtModel, (int)Defind_CodeFind.DCF_BUYERMODEL, "");

                //주문 형태 콤보박스 열기
                //cboOrderForm.IsDropDownOpen = true; //2020.02.14 장가빈, 수정시 콤보박스 자동 열리는 것 불편하대서 주석처리 함
                cboOrderForm.Focus();

            }
        }

        //차종
        private void btnPfModel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtModel, (int)Defind_CodeFind.DCF_BUYERMODEL, "");

            //주문 형태 콤보박스 열기
            //cboOrderForm.IsDropDownOpen = true; //2020.02.14 장가빈, 수정시 콤보박스 자동 열리는 것 불편하대서 주석처리 함
        }

        private void CallArticleData(string strArticleID)
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

                        articleData = new ArticleData
                        {
                            ArticleID = dr["ArticleID"].ToString(),
                            Article = dr["Article"].ToString(),
                            ThreadID = dr["ThreadID"].ToString(),
                            thread = dr["thread"].ToString(),
                            StuffWidth = dr["StuffWidth"].ToString(),
                            DyeingID = dr["DyeingID"].ToString(),
                            Weight = dr["Weight"].ToString(),
                            Spec = dr["Spec"].ToString(),
                            ArticleGrpID = dr["ArticleGrpID"].ToString(),
                            BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                            UnitPrice = dr["UnitPrice"].ToString(),
                            UnitPriceClss = dr["UnitPriceClss"].ToString(),
                            UnitClss = dr["UnitClss"].ToString(),
                            Code_Name = dr["Code_Name"].ToString(),
                            //ProcessName = dr["ProcessName"].ToString(),
                            //HSCode = dr["HSCode"].ToString(),
                            OutUnitPrice = dr["OutUnitPrice"].ToString(),
                            BuyerModelID = dr["BuyerModelID"].ToString(),
                            BuyerModel = dr["BuyerModel"].ToString(),
                        };
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

        private void chkDvlyDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpDvlyDate.IsEnabled = true;
        }

        private void chkDvlyDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpDvlyDate.IsEnabled = false;
        }

        private void FillNeedStockQty(string strArticleID, string strQty)
        {
            if (dgdNeedStuff.Items.Count > 0)
                dgdNeedStuff.Items.Clear();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("ArticleID", strArticleID);
                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_Order_sArticleNeedStockQty", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var NeedStockQty = new ArticleNeedStockQty()
                            {
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                Article = dr["Article"].ToString(),
                                NeedQty = dr["NeedQty"].ToString(),
                                UnitClss = dr["UnitClss"].ToString(),
                                UnitClssName = dr["UnitClssName"].ToString()
                            };

                            if (Lib.Instance.IsNumOrAnother(NeedStockQty.NeedQty))
                            {
                                if (Lib.Instance.IsNumOrAnother(strQty))
                                {
                                    double doubleTemp = double.Parse(NeedStockQty.NeedQty) * double.Parse(strQty);
                                    NeedStockQty.NeedQty = string.Format("{0:N0}", doubleTemp);
                                }
                            }

                            dgdNeedStuff.Items.Add(NeedStockQty);
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

        private void dgdMain_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (btnUpdate.IsEnabled == true)
            {
                if (e.ClickCount == 2)
                {
                    btnUpdate_Click(null, null);
                }
            }
        }

        #region 기타 메서드 모음

        // 천마리 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        // 천마리 콤마, 소수점 한자리
        private string stringFormatN1(object obj)
        {
            return string.Format("{0:N1}", obj);
        }

        // 천마리 콤마, 소수점 두자리
        private string stringFormatN2(object obj)
        {
            return string.Format("{0:N2}", obj);
        }

        // 데이터피커 포맷으로 변경
        private string DatePickerFormat(string str)
        {
            string result = "";

            if (str.Length == 8)
            {
                if (!str.Trim().Equals(""))
                    result = str.Substring(0, 4) + "-" + str.Substring(4, 2) + "-" + str.Substring(6, 2);
            }

            return result;
        }

        // Int로 변환
        private int ConvertInt(string str)
        {
            int result = 0;
            int chkInt = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Replace(",", "");

                if (int.TryParse(str, out chkInt) == true)
                    result = int.Parse(str);
            }

            return result;
        }

        // 소수로 변환 가능한지 체크 이벤트
        private bool CheckConvertDouble(string str)
        {
            bool flag = false;
            double chkDouble = 0;

            if (!str.Trim().Equals(""))
            {
                if (double.TryParse(str, out chkDouble) == true)
                    flag = true;
            }

            return flag;
        }

        // 숫자로 변환 가능한지 체크 이벤트
        private bool CheckConvertInt(string str)
        {
            bool flag = false;
            int chkInt = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Trim().Replace(",", "");

                if (int.TryParse(str, out chkInt) == true)
                    flag = true;
            }

            return flag;
        }

        // 소수로 변환
        private double ConvertDouble(string str)
        {
            double result = 0;
            double chkDouble = 0;

            if (!str.Trim().Equals(""))
            {
                str = str.Replace(",", "");

                if (double.TryParse(str, out chkDouble) == true)
                    result = double.Parse(str);
            }

            return result;
        }

        //남아있는 데이터로 오류 방지 입력칸 비우기
        private void ClearInputGrid()
        {
            //여기에 비우고자 하는 그리드를 파라미터로 적어주세요
            ClearTextLabel(grdInput);
        }

        //UI컨트롤을 찾아 해당하는 요소가 있으면 내용을 비움
        private void ClearTextLabel(DependencyObject parent)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is TextBox textBox)
                {
                    // TextBox를 찾으면 Text 속성을 빈 문자열로 설정
                    textBox.Text = string.Empty;
                    textBox.Tag = null;
                }
                if (child is ComboBox comboBox)
                {
                    //콤보박스 선택값 비워줌
                    comboBox.SelectedValue = "";
                }
                else
                {
                    // 자식이 TextBox가 아니면 재귀적으로 그 자식의 자식들을 탐색
                    ClearTextLabel(child);
                }
            }
        }

        #endregion

        #region keyDown 이벤트(커서이동)

        //숫자 외에 다른 문자열 못들어오도록
        public bool IsNumeric(string source)
        {

            Regex regex = new Regex("[^0-9.-]+");
            return !regex.IsMatch(source);
        }

        //총주문량 숫자 외에 못들어가게 
        private void TxtAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(e.Text);
        }

        //단가 숫자 외에 못들어가게
        private void TxtUnitPrice_TextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(e.Text);
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

        //수주구분 라벨
        private void LblOrderFlag_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            chkOrderFlag.IsChecked = chkOrderFlag.IsChecked == true ? false : true;
        }

        //수주구분 체크박스 체크
        private void ChkOrderFlag_Checked(object sender, RoutedEventArgs e)
        {
            cboOrderFlag.IsEnabled = true;
        }

        //수주구분 체크박스 체크 해제
        private void ChkOrderFlag_Unchecked(object sender, RoutedEventArgs e)
        {
            cboOrderFlag.IsEnabled = false;
        }

        //매출거래처 
        private void txtInCustom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                MainWindow.pf.ReturnCode(txtInCustom, 72, "");
        }

        //매출거래처
        private void btnPfInCustom_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtInCustom, 72, "");
        }
        #endregion keydown 이벤트

        //자재필요량조회
        private void btnNeedStuff_Click(object sender, RoutedEventArgs e)
        {
            if (txtArticle.Tag == null)
            {
                MessageBox.Show("먼저 품명을 선택해주세요");
                return;
            }

            if (txtAmount.Text.Replace(" ", "").Equals(""))
            {
                MessageBox.Show("먼저 총 주문량을 입력해주세요");
                return;
            }

            //자재필요량조회에 필요한 파라미터 값을 넘겨주자, 품명이랑 주문량
            FillNeedStockQty(txtArticle.Tag.ToString(), txtAmount.Text.Replace(",", ""));
        }

        //메인 데이터그리드 선택 이벤트
        private void DataGridMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (dgdMain.SelectedItems.Count == 1)
                {
                    dgdNeedStuff.Items.Clear();
                    var OrderInfo = dgdMain.SelectedItem as Win_ord_Order_U_CodeView;
                    if (OrderInfo != null)
                    {
                        DataContext = OrderInfo;
                        FillNeedStockQty(OrderInfo.ArticleID, OrderInfo.OrderQty);
                    }
                     
                }
                else if (dgdMain.SelectedItems.Count > 1)
                {
                    dgdNeedStuff.Items.Clear();
                    DataContext = null; 
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - DataGridMain_SelectionChanged : " + ee.ToString());
            }
        }

        private void txtBuyerArticle_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtCustom != null && txtCustom.Text != "")
                    {   
                        //품번을 품명처럼 쓴다고 해서 품번을 조회하도록 2020.03.17, 장가빈
                        MainWindow.pf.ReturnCodeGLS(txtBuyerArticleNO, 7070, txtCustom.Tag.ToString().Trim());
                    }
                    else
                    {
                        //품번을 품명처럼 쓴다고 해서 품번을 조회하도록 2020.03.17, 장가빈
                        MainWindow.pf.ReturnCodeGLS(txtBuyerArticleNO, 7071, "");
                    }

                    if (txtBuyerArticleNO.Tag != null)
                    {
                        CallArticleData(txtBuyerArticleNO.Tag.ToString());

                        //품명종류 대입(ex.제품 등)
                        cboArticleGroup.SelectedValue = articleData.ArticleGrpID;
                        //품명 대입
                        txtArticle.Text = articleData.Article;
                        txtArticle.Tag = articleData.ArticleID;                                             
                        //단가 대입                        
                        txtUnitPrice.Text = articleData.OutUnitPrice;
                        //차종 대입
                        txtModel.Tag = articleData.BuyerModelID;
                        txtModel.Text = articleData.BuyerModel;
                    }

                    //플러스 파인더 작동 후 규격으로 커서 이동
                    txtSpec.Focus();
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

        private void btnPfBuyerArticle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCustom != null && txtCustom.Text != "")
                {   
                    //선택된 납품거래처에 따른 품명만 보여주게
                    MainWindow.pf.ReturnCodeGLS(txtBuyerArticleNO, 7070, txtCustom.Tag.ToString().Trim());
                }
                else
                {   //선택된 납품거래처가 없다면 전체 품명 다 보여주게
                    MainWindow.pf.ReturnCodeGLS(txtBuyerArticleNO, 7071, "");
                }

                if (txtBuyerArticleNO.Tag != null)
                {
                    CallArticleData(txtBuyerArticleNO.Tag.ToString());

                    //품명종류 대입(ex.제품 등)
                    cboArticleGroup.SelectedValue = articleData.ArticleGrpID;
                    //품명 대입
                    txtArticle.Text = articleData.Article;
                    //단가 대입
                    txtUnitPrice.Text = articleData.OutUnitPrice;
                    //차종 대입
                    txtModel.Tag = articleData.BuyerModelID;
                    txtModel.Text = articleData.BuyerModel;
                }

                //플러스 파인더 작동 후 규격으로 커서 이동
                txtSpec.Focus();
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

        private void rbnOrderNo_Click(object sender, RoutedEventArgs e)
        {
            tbkOrderSrh.Text = " 발주번호";
            dgdtxtOrderID.Visibility = Visibility.Hidden;
            dgdtxtOrderNo.Visibility = Visibility.Visible;
        }

        private void rbnOrderID_Click(object sender, RoutedEventArgs e)
        {
            tbkOrderSrh.Text = " 관리번호";
            dgdtxtOrderID.Visibility = Visibility.Visible;
            dgdtxtOrderNo.Visibility = Visibility.Hidden;
        }
    }


    public class Win_ord_Order_U_CodeView : BaseView
    {
        public string OrderID { get; set; }
        public string OrderNO { get; set; }
        public string CustomID { get; set; }
        public string KCustom { get; set; }
        public string CloseClss { get; set; }

        public string OrderQty { get; set; }
        public string UnitClss { get; set; }
        public string Article { get; set; }
        public string ChunkRate { get; set; }
        public string PatternID { get; set; }

        public string Amount { get; set; }
        public string BuyerModelID { get; set; }
        public string BuyerModel { get; set; }
        public string BuyerArticleNo { get; set; }
        public string PONO { get; set; }

        public string OrderForm { get; set; }
        public string OrderClss { get; set; }
        public string InCustomID { get; set; }
        public string AcptDate { get; set; }
        public string DvlyDate { get; set; }

        public string ArticleID { get; set; }
        public string DvlyPlace { get; set; }
        public string WorkID { get; set; }
        public string PriceClss { get; set; }
        public string ExchRate { get; set; }

        public string Vat_IND_YN { get; set; }
        public string ColorCnt { get; set; }
        public string StuffWidth { get; set; }
        public string StuffWeight { get; set; }
        public string CutQty { get; set; }

        public string WorkWidth { get; set; }
        public string WorkWeight { get; set; }
        public string WorkDensity { get; set; }
        public string LossRate { get; set; }
        public string ReduceRate { get; set; }

        public string TagClss { get; set; }
        public string LabelID { get; set; }
        public string BandID { get; set; }
        public string EndClss { get; set; }
        public string MadeClss { get; set; }

        public string SurfaceClss { get; set; }
        public string ShipClss { get; set; }
        public string AdvnClss { get; set; }
        public string LotClss { get; set; }
        public string EndMark { get; set; }

        public string TagArticle { get; set; }
        public string TagArticle2 { get; set; }
        public string TagOrderNo { get; set; }
        public string TagRemark { get; set; }
        public string Tag { get; set; }

        public string BasisID { get; set; }
        public string BasisUnit { get; set; }
        public string SpendingClss { get; set; }
        public string DyeingID { get; set; }
        public string WorkingClss { get; set; }

        public string BTID { get; set; }
        public string BTIDSeq { get; set; }
        public string ChemClss { get; set; }
        public string AccountClss { get; set; }
        public string ModifyClss { get; set; }

        public string ModifyRemark { get; set; }
        public string CancelRemark { get; set; }
        public string Remark { get; set; }
        public string ActiveClss { get; set; }
        public string ModifyDate { get; set; }

        public string OrderFlag { get; set; }
        public string TagRemark2 { get; set; }
        public string TagRemark3 { get; set; }
        public string TagRemark4 { get; set; }
        public string UnitPriceClss { get; set; }

        public string WeightPerYard { get; set; }
        public string WorkUnitClss { get; set; }
        public string ArticleGrpID { get; set; }
        public string OrderSpec { get; set; }
        public string UnitPrice { get; set; }

        public string CompleteArticleFile { get; set; }
        public string CompleteArticlePath { get; set; }
        public string FirstArticleFile { get; set; }
        public string FirstArticlePath { get; set; }
        public string MediumArticleFIle { get; set; }

        public string MediumArticlePath { get; set; }
        public string sketch1Path { get; set; }
        public string sketch1file { get; set; }
        public string sketch2Path { get; set; }
        public string sketch2file { get; set; }

        public string sketch3Path { get; set; }
        public string sketch3file { get; set; }
        public string sketch4Path { get; set; }
        public string sketch4file { get; set; }
        public string sketch5Path { get; set; }

        public string sketch5file { get; set; }
        public string sketch6Path { get; set; }
        public string sketch6file { get; set; }
        public string ProductAutoInspectYN { get; set; }
        public string kBuyer { get; set; }

        public string BuyerID { get; set; }
        public int Num { get; set; }
        public string AcptDate_CV { get; set; }
        public string DvlyDate_CV { get; set; }
        public string Amount_CV { get; set; }

        public string KInCustom { get; set; }
        public string SketchFile { get; set; }
        public string SketchPath { get; set; }
        public string ImageName { get; set; }

        public string CompanyID { get; set; }
        public string OrderNo { get; set; }
        public string PoNo { get; set; }
        public string OrderFormName { get; set; }
        public string BrandClss { get; set; }
        public string WorkName { get; set; }
        public string OrderClssName { get; set; }

        public string NewArticleQty { get; set; }
        public string RePolishingQty { get; set; }
    }

    public class ArticleData : BaseView
    {
        public string ArticleID { get; set; }
        public string Article { get; set; }
        public string ThreadID { get; set; }
        public string thread { get; set; }
        public string StuffWidth { get; set; }
        public string DyeingID { get; set; }
        public string Weight { get; set; }
        public string Spec { get; set; }
        public string ArticleGrpID { get; set; }
        public string BuyerArticleNo { get; set; }
        public string UnitPrice { get; set; }
        public string UnitPriceClss { get; set; }
        public string UnitClss { get; set; }
        public string ProcessName { get; set; }
        public string HSCode { get; set; }
        public string OutUnitPrice { get; set; }
        public string Code_Name { get; set; }
        public string BuyerModelID { get; set; }
        public string BuyerModel { get; set; }
    }

    public class ArticleNeedStockQty : BaseView
    {
        public string BuyerArticleNo { get; set; }
        public string Article { get; set; }
        public string NeedQty { get; set; }
        public string UnitClss { get; set; }
        public string UnitClssName { get; set; }
    }

    public class OrderExcel : BaseView
    {
        public string CustomID { get; set; }
        public string Model { get; set; }
        public string BuyerArticleNo { get; set; }
        public string Article { get; set; }
        public string UnitClss { get; set; }
        public string OrderQty { get; set; }
    }
}

