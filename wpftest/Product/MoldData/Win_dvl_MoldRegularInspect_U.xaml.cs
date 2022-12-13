using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WizMes_ANT.PopUP;
using WPF.MDI;

namespace WizMes_ANT
{
    /// <summary>
    /// Win_dvl_MoldRegularInspect_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_dvl_MoldRegularInspect_U : UserControl
    {
        string strFlag = string.Empty;
        string strMoldBasisID = string.Empty;
        int rowNum = 0;
        Win_dvl_MoldRegularInspect_U_CodeView WinMoldRegulIns = new Win_dvl_MoldRegularInspect_U_CodeView();
        Win_dvl_MoldRegularInspect_U_Sub_CodeView WinMoldRegulInsSub = new Win_dvl_MoldRegularInspect_U_Sub_CodeView();
        //Win_dvl_MoldRegularInspect_U_Sub2_CodeView WinMoldRegulInsSub2 = new Win_dvl_MoldRegularInspect_U_Sub2_CodeView();

        ObservableCollection<CodeView> ovcLegend = new ObservableCollection<CodeView>();

        // FTP 활용모음.
        string strImagePath = string.Empty;
        string strFullPath = string.Empty;

        private FTP_EX _ftp = null;
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

        //string FTP_ADDRESS = "ftp://wizis.iptime.org/ImageData/MoldReqularInspect";
        string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":"
            + LoadINI.FTPPort + LoadINI.FtpImagePath + "/MoldReqularInspect";
        //string FTP_ADDRESS = "ftp://222.104.222.145:25000/ImageData/MoldReqularInspect";
        //string FTP_ADDRESS = "ftp://192.168.0.95/ImageData/MoldReqularInspect";
        private const string FTP_ID = "wizuser";
        private const string FTP_PASS = "wiz9999";
        private const string LOCAL_DOWN_PATH = "C:\\Temp";

        public Win_dvl_MoldRegularInspect_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Lib.Instance.UiLoading(sender);
            SetComboBox();
            SetComboBoxSearch();
        }

        private void SetComboBox()
        {
            ObservableCollection<CodeView> ovcMoldCycle = ComboBoxUtil.Instance.GetCMCode_SetComboBox("MLDCYCLEGBN", "");
            this.cboRegularGubun.ItemsSource = ovcMoldCycle;
            this.cboRegularGubun.DisplayMemberPath = "code_name";
            this.cboRegularGubun.SelectedValuePath = "code_id";

            ovcLegend = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "MLDLEGEND", "Y", "", "");
        }
        
        private void SetComboBoxSearch()
        {
            ObservableCollection<CodeView> ovcMoldCycleSearch = ComboBoxUtil.Instance.GetCMCode_SetComboBox("MLDCYCLEGBN", "");
            this.cboRegularGubunSearch.ItemsSource = ovcMoldCycleSearch;
            this.cboRegularGubunSearch.DisplayMemberPath = "code_name";
            this.cboRegularGubunSearch.SelectedValuePath = "code_id";

            this.cboRegularGubunSearch.SelectedIndex = 0;
            //ovcLegend = ComboBoxUtil.Instance.Gf_DB_CM_GetComCodeDataset(null, "MLDLEGEND", "Y", "", "");
        }

        //점검일자
        private void lblInspectDaySrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkInspectDaySrh.IsChecked == true) { chkInspectDaySrh.IsChecked = false; }
            else { chkInspectDaySrh.IsChecked = true; }
        }

        //점검일자
        private void chkInspectDaySrh_Checked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;
            btnLastMonth.IsEnabled = true;
            btnThisMonth.IsEnabled = true;
            btnYesterDay.IsEnabled = true;
            btnToday.IsEnabled = true;
        }

        //점검일자
        private void chkInspectDaySrh_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
            btnLastMonth.IsEnabled = false;
            btnThisMonth.IsEnabled = false;
            btnYesterDay.IsEnabled = false;
            btnToday.IsEnabled = false;
        }

        //전월
        private void btnLastMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringLastMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringLastMonthDatetimeList()[1];
        }

        //금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
        }

        //전일
        private void btnYesterDay_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today.AddDays(-1);
            dtpEDate.SelectedDate = DateTime.Today.AddDays(-1);
        }

        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        //금형
        private void lblMoldSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldSrh.IsChecked == true) { chkMoldSrh.IsChecked = false; }
            else { chkMoldSrh.IsChecked = true; }
        }

        //금형
        private void chkMoldSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldSrh.IsEnabled = true;
            btnPfMoldSrh.IsEnabled = true;
        }

        //금형
        private void chkMoldSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldSrh.IsEnabled = false;
            btnPfMoldSrh.IsEnabled = false;
        }

        //정기검사구분
        private void lblRegularGubunSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkRegularGubunSrh.IsChecked == true) { chkRegularGubunSrh.IsChecked = false; }
            else { chkRegularGubunSrh.IsChecked = true; }
        }

        //정기검사구분
        private void chkRegularGubunSrh_Checked(object sender, RoutedEventArgs e)
        {
            cboRegularGubunSearch.IsEnabled = true;           
        }

        //정기검사구분
        private void chkRegularGubunSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            cboRegularGubunSearch.IsEnabled = false;
        }

        private void lblMoldLotNoSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldLotNoSrh.IsChecked == true) { chkMoldLotNoSrh.IsChecked = false; }
            else { chkMoldLotNoSrh.IsChecked = true; }
        }

        private void chkMoldLotNoSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldLotNoSrh.IsEnabled = true;
            btnMoldLotNoSrh.IsEnabled = true;
        }

        private void chkMoldLotNoSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldLotNoSrh.IsEnabled = false;
            btnMoldLotNoSrh.IsEnabled = false;
        }

        /// <summary>
        /// 수정,추가 저장 후
        /// </summary>
        private void CanBtnControl()
        {
            Lib.Instance.UiButtonEnableChange_IUControl(this);
            gbxMold.IsEnabled = false;
        }

        /// <summary>
        /// 수정,추가 진행 중
        /// </summary>
        private void CantBtnControl()
        {
            Lib.Instance.UiButtonEnableChange_SCControl(this);
            gbxMold.IsEnabled = true;
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CantBtnControl();
            strFlag = "I";
            
            tbkMsg.Text = "자료 입력 중";
            rowNum = dgdMoldInspect.SelectedIndex;
            dgdMoldInspect.IsHitTestVisible = false;
            this.DataContext = null;
            dtpMoldInspectDate.SelectedDate = DateTime.Today;
            cboRegularGubun.SelectedIndex = 0;

            if (dgdMold_InspectSub1.Items.Count > 0)
            {
                dgdMold_InspectSub1.Items.Clear();
                dgdMold_InspectSub1.Refresh();
            }
            if (dgdMold_InspectSub2.Items.Count > 0)
            {
                dgdMold_InspectSub2.Items.Clear();
                dgdMold_InspectSub2.Refresh();
            }
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            WinMoldRegulIns = dgdMoldInspect.SelectedItem as Win_dvl_MoldRegularInspect_U_CodeView;

            if (WinMoldRegulIns != null)
            {
                rowNum = dgdMoldInspect.SelectedIndex;
                //dgdMoldInspect.IsEnabled = false;
                dgdMoldInspect.IsHitTestVisible = false;
                tbkMsg.Text = "자료 수정 중";
                lblMsg.Visibility = Visibility.Visible;
                CantBtnControl();
                strFlag = "U";
            }
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            WinMoldRegulIns = dgdMoldInspect.SelectedItem as Win_dvl_MoldRegularInspect_U_CodeView;
            List<string> lstArrayFileName = new List<string>();

            if (WinMoldRegulIns == null)
            {
                MessageBox.Show("삭제할 데이터가 지정되지 않았습니다. 삭제데이터를 지정하고 눌러주세요");
            }
            else
            {
                if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (dgdMoldInspect.Items.Count > 0 && dgdMoldInspect.SelectedItem != null)
                    {
                        rowNum = dgdMoldInspect.SelectedIndex;
                    }

                    if (DeleteData(WinMoldRegulIns.MoldInspectID))
                    {
                        this.DataContext = null;

                        if (dgdMold_InspectSub1.Items.Count > 0)
                        {
                            dgdMold_InspectSub1.Items.Clear();
                        }
                        if (dgdMold_InspectSub2.Items.Count > 0)
                        {
                            dgdMold_InspectSub2.Items.Clear();
                        }

                        rowNum -= 1;
                        re_Search(rowNum);
                    }
                }
            }
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        //검색
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMold_InspectSub1.Items.Count > 0)
            {
                dgdMold_InspectSub1.Items.Clear();
            }
            if (dgdMold_InspectSub2.Items.Count > 0)
            {
                dgdMold_InspectSub2.Items.Clear();
            }

            rowNum = 0;
            re_Search(rowNum);
        }

        //저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveData(strFlag, txtMoldRInspectID.Text))
            {
                CanBtnControl();
                
                lblMsg.Visibility = Visibility.Hidden;
                rowNum = 0;
                //dgdMoldInspect.IsEnabled = true;
                dgdMoldInspect.IsHitTestVisible = true;

                if (dgdMold_InspectSub1.Items.Count > 0)
                {
                    dgdMold_InspectSub1.Items.Clear();
                }
                if (dgdMold_InspectSub2.Items.Count > 0)
                {
                    dgdMold_InspectSub2.Items.Clear();
                }
                re_Search(rowNum);
            }
        }

        //취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CanBtnControl();
            if (dgdMold_InspectSub1.Items.Count > 0)
            {
                dgdMold_InspectSub1.Items.Clear();
            }
            if (dgdMold_InspectSub2.Items.Count > 0)
            {
                dgdMold_InspectSub2.Items.Clear();
            }

            if (!strFlag.Equals(string.Empty))
            {
                re_Search(rowNum);
            }

            strFlag = string.Empty;
            //dgdMoldInspect.IsEnabled = true;
            dgdMoldInspect.IsHitTestVisible = true;
        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[6];
            lst[0] = "금형점검";
            lst[1] = "금형점검 검사 범례";
            lst[2] = "금형점검 검사 수치";
            lst[3] = dgdMoldInspect.Name;
            lst[4] = dgdMold_InspectSub1.Name;
            lst[5] = dgdMold_InspectSub2.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);
            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdMoldInspect.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdMoldInspect);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdMoldInspect);

                    Name = dgdMoldInspect.Name;
                    Lib.Instance.GenerateExcel(dt, Name);
                    Lib.Instance.excel.Visible = true;
                }
                else if (ExpExc.choice.Equals(dgdMold_InspectSub1.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdMold_InspectSub1);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdMold_InspectSub1);

                    Name = dgdMold_InspectSub1.Name;
                    Lib.Instance.GenerateExcel(dt, Name);
                    Lib.Instance.excel.Visible = true;
                }
                else if (ExpExc.choice.Equals(dgdMold_InspectSub2.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdMold_InspectSub2);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdMold_InspectSub2);

                    Name = dgdMold_InspectSub2.Name;
                    Lib.Instance.GenerateExcel(dt, Name);
                    Lib.Instance.excel.Visible = true;
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

        /// <summary>
        /// 재검색(수정,삭제,추가 저장후에 자동 재검색)
        /// </summary>
        /// <param name="selectedIndex"></param>
        private void re_Search(int selectedIndex)
        {
            FillGrid();

            if (dgdMoldInspect.Items.Count > 0)
            {
                dgdMoldInspect.SelectedIndex = selectedIndex;
            }
        }

        /// <summary>
        /// 실조회
        /// </summary>
        private void FillGrid()
        {
            if (dgdMoldInspect.Items.Count > 0)
            {
                dgdMoldInspect.Items.Clear();
            }

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("nChkDate", chkInspectDaySrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("SDate", chkInspectDaySrh.IsChecked == true ?
                    (dtpSDate.SelectedDate != null ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "") : "");
                sqlParameter.Add("EDate", chkInspectDaySrh.IsChecked == true ?
                    (dtpEDate.SelectedDate != null ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "") : "");
                //2022-04-11 검색조건 추가
                sqlParameter.Add("chkMold", chkMoldSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("txtMold", chkMoldSrh.IsChecked == true ? txtMoldSrh.Text : "");
                sqlParameter.Add("chkRegularGubun", chkRegularGubunSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("cboRegularGubun", chkRegularGubunSrh.IsChecked == true && cboRegularGubunSearch.SelectedItem != null ? cboRegularGubunSearch.SelectedValue.ToString() : "");
                sqlParameter.Add("chkMoldLotNo", chkMoldLotNoSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("txtMoldLotNo", chkMoldLotNoSrh.IsChecked == true ? txtMoldLotNoSrh.Text : "");


                ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMoldIns_sRegularInspect", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;

                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("조회된 데이터가 없습니다.");
                    }
                    else
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinMoldRegulInspect = new Win_dvl_MoldRegularInspect_U_CodeView()
                            {
                                Num = i + 1,
                                MoldInspectID = dr["MoldInspectID"].ToString(),
                                MoldInspectCycleGbn = dr["MoldInspectCycleGbn"].ToString(),
                                MoldInspectCycleName = dr["MoldInspectCycleName"].ToString(),
                                MoldNo = dr["MoldNo"].ToString(),
                                MoldInspectDate = dr["MoldInspectDate"].ToString(),
                                ArticleID = dr["ArticleID"].ToString(),
                                Article = dr["Article"].ToString(),
                                MoldInspectBasisDate = dr["MoldInspectBasisDate"].ToString(),
                                MoldInspectBasisID = dr["MoldInspectBasisID"].ToString(),
                                MoldInspectUserID = dr["MoldInspectUserID"].ToString(),
                                Person = dr["Person"].ToString(),
                                MoldID = dr["MoldID"].ToString(),
                                HitCount = dr["HitCount"].ToString(),
                                DefectContents = dr["DefectContents"].ToString(),
                                DefectRespectContents = dr["DefectRespectContents"].ToString(),
                                Comments = dr["Comments"].ToString(),
                                MoldInspectContent = dr["MoldInspectContent"].ToString(),
                                DefectReason = dr["DefectReason"].ToString()
                            };

                            if (WinMoldRegulInspect.MoldInspectDate.Length == 8)
                            {
                                WinMoldRegulInspect.MoldInspectDate_CV = Lib.Instance.StrDateTimeBar(WinMoldRegulInspect.MoldInspectDate);
                            }

                            if (WinMoldRegulInspect.MoldInspectBasisDate.Length == 8)
                            {
                                WinMoldRegulInspect.MoldInspectBasisDate_CV = Lib.Instance.StrDateTimeBar(WinMoldRegulInspect.MoldInspectBasisDate);
                            }

                            dgdMoldInspect.Items.Add(WinMoldRegulInspect);
                            i++;
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

        private void dgdMoldInspect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WinMoldRegulIns = dgdMoldInspect.SelectedItem as Win_dvl_MoldRegularInspect_U_CodeView;

            if (WinMoldRegulIns != null)
            {
                this.DataContext = WinMoldRegulIns;
                FillGridSub(WinMoldRegulIns.MoldInspectID, WinMoldRegulIns.MoldID);
            }
        }

        //private BitmapImage SetImage(string strAttachPath)
        //{
        //    _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);
        //    BitmapImage bit = _ftp.DrawingImageByByte(FTP_ADDRESS + strAttachPath + "");
        //    //image.Source = bit;
        //    return bit;
        //}

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

        private BitmapImage SetImage(string ImageName, string FolderName)
        {
            bool ExistFile = false;
            BitmapImage bit = null;
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);
            if (_ftp == null) { return null; }

            string[] fileListDetail;
            fileListDetail = _ftp.directoryListSimple(FolderName, Encoding.Default);

            ExistFile = FileInfoAndFlag(fileListDetail, ImageName);
            if (ExistFile)
            {
                bit = _ftp.DrawingImageByByte(FTP_ADDRESS + '/' + FolderName + '/' + ImageName + "");
            }

            return bit;
        }

        /// <summary>
        /// 서브 조회
        /// </summary>
        /// <param name="strID"></param>
        private void FillGridSub(string InspectID, string MoldID)
        {
            if (dgdMold_InspectSub1.Items.Count > 0)
            {
                dgdMold_InspectSub1.Items.Clear();
            }

            if (dgdMold_InspectSub2.Items.Count > 0)
            {
                dgdMold_InspectSub2.Items.Clear();
            }

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("MoldID", MoldID);
                sqlParameter.Add("MoldInspectID", InspectID);
                ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMoldIns_sRegularInspectSub", sqlParameter, false);

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
                            var WinMoldRegulInsSub = new Win_dvl_MoldRegularInspect_U_Sub_CodeView()
                            {
                                MoldInspectID = dr["MoldInspectID"].ToString(),
                                MoldInspectBasisID = dr["MoldInspectBasisID"].ToString(),
                                MoldID = dr["MoldID"].ToString(),
                                MoldInspectBasisDate = dr["MoldInspectBasisDate"].ToString(),
                                MoldSeq = dr["MoldSeq"].ToString(),
                                MoldInsSeq = dr["MoldInsSeq"].ToString(),
                                MoldInspectItemName = dr["MoldInspectItemName"].ToString(),
                                MoldInspectContent = dr["MoldInspectContent"].ToString(),
                                MoldInspectGbn = dr["MoldInspectGbn"].ToString(),
                                MoldInspectGbnName = dr["MoldInspectGbnName"].ToString(),
                                MoldInspectCheckGbn = dr["MoldInspectCheckGbn"].ToString(),
                                MoldInspectCheckName = dr["MoldInspectCheckName"].ToString(),
                                MoldInspectCycleGbn = dr["MoldInspectCycleGbn"].ToString(),
                                MoldInspectCycleName = dr["MoldInspectCycleName"].ToString(),
                                MoldInspectCycleDate = dr["MoldInspectCycleDate"].ToString(),
                                MoldInspectRecordGbn = dr["MoldInspectRecordGbn"].ToString(),
                                MoldInspectRecordGbnName = dr["MoldInspectRecordGbnName"].ToString(),
                                MldInspectLegend = dr["MldInspectLegend"].ToString(),
                                MldValue = dr["MldValue"].ToString(),
                                Comments = dr["Comments"].ToString(),
                                MoldInspectImagePath = dr["MoldInspectImagePath"].ToString(),
                                MoldInspectImageFile = dr["MoldInspectImageFile"].ToString(),
                                ovcLegend = this.ovcLegend
                            };

                            if (WinMoldRegulInsSub.MldInspectLegend != null && !WinMoldRegulInsSub.MldInspectLegend.Equals(string.Empty))
                            {
                                foreach (CodeView codeView in ovcLegend)
                                {
                                    if (codeView.code_id.Equals(WinMoldRegulInsSub.MldInspectLegend))
                                    {
                                        WinMoldRegulInsSub.LegendShape = codeView.code_name;
                                        break;
                                    }
                                }
                            }

                            if (WinMoldRegulInsSub.MoldInspectImageFile != null && !WinMoldRegulInsSub.MoldInspectImageFile.Replace(" ", "").Equals(""))
                            {
                                WinMoldRegulInsSub.imageFlag = true;
                                if (!Lib.Instance.Right(WinMoldRegulInsSub.MoldInspectImageFile, 3).Equals("pdf"))
                                {
                                    //string strImage = "/" + WinMoldRegulInsSub.MoldInspectBasisID + "/" + WinMoldRegulInsSub.MoldInspectImageFile;
                                    WinMoldRegulInsSub.ImageView = SetImage(WinMoldRegulInsSub.MoldInspectImageFile, WinMoldRegulInsSub.MoldInspectBasisID);
                                }
                            }
                            else
                            {
                                WinMoldRegulInsSub.imageFlag = false;
                            }

                            //if (WinMoldRegulInsSub.MoldInspectGbn.Equals("1"))
                            //{
                            //    WinMoldRegulInsSub.Num = i + 1;
                            //    dgdMold_InspectSub1.Items.Add(WinMoldRegulInsSub);
                            //    i++;
                            //}
                            //else if (WinMoldRegulInsSub.MoldInspectGbn.Equals("2"))
                            //{
                            //    WinMoldRegulInsSub.Num = j + 1;
                            //    WinMoldRegulInsSub.MldValue = Lib.Instance.returnNumStringOne(WinMoldRegulInsSub.MldValue);
                            //    dgdMold_InspectSub2.Items.Add(WinMoldRegulInsSub);
                            //    j++;
                            //}
                            if (WinMoldRegulInsSub.MoldInspectRecordGbn.Equals("01"))
                            {
                                WinMoldRegulInsSub.Num = i + 1;
                                dgdMold_InspectSub1.Items.Add(WinMoldRegulInsSub);
                                i++;
                            }
                            else if (WinMoldRegulInsSub.MoldInspectRecordGbn.Equals("02"))
                            {
                                WinMoldRegulInsSub.Num = j + 1;
                                WinMoldRegulInsSub.MldValue = Lib.Instance.returnNumStringOne(WinMoldRegulInsSub.MldValue);
                                dgdMold_InspectSub2.Items.Add(WinMoldRegulInsSub);
                                j++;
                            }

                            //i++;
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
                sqlParameter.Add("MoldInspectID", strID);

                string[] result = DataStore.Instance.ExecuteProcedure("xp_dvlMoldIns_dRegularInspect", sqlParameter, false);

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
        /// <param name="strYYYY"></param>
        /// <returns></returns>
        private bool SaveData(string strFlag, string strInspectID)
        {
            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                if (CheckData())
                {
                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Add("MoldInspectID", strInspectID);
                    sqlParameter.Add("MoldInspectDate", dtpMoldInspectDate.SelectedDate.Value.ToString("yyyyMMdd"));
                    sqlParameter.Add("MoldInspectUserID", MainWindow.CurrentPersonID);
                    sqlParameter.Add("MoldInspectBasisID", txtMoldRInspectID.Tag.ToString());
                    sqlParameter.Add("MoldInspectBasisDate", dtpBasisDate.SelectedDate.Value.ToString("yyyyMMdd"));
                    sqlParameter.Add("MoldICycleGbn", cboRegularGubun.SelectedValue.ToString());
                    sqlParameter.Add("DefectContents", txtDefectContents.Text);
                    sqlParameter.Add("DefectReason", txtDefectReason.Text);
                    sqlParameter.Add("DefectRespectContents", txtDefectRespectContents.Text);
                    sqlParameter.Add("Comments", txtComments.Text);
                    sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                    #region 추가

                    if (strFlag.Equals("I"))
                    {
                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_dvlMoldIns_iRegularInspect";
                        pro1.OutputUseYN = "Y";
                        pro1.OutputName = "MoldInspectID";
                        pro1.OutputLength = "10";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        for (int i = 0; i < dgdMold_InspectSub1.Items.Count; i++)
                        {
                            WinMoldRegulInsSub = dgdMold_InspectSub1.Items[i] as Win_dvl_MoldRegularInspect_U_Sub_CodeView;
                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldInspectID", strInspectID);
                            sqlParameter.Add("MoldSeq", WinMoldRegulInsSub.MoldSeq);
                            sqlParameter.Add("MoldInsBasisID", WinMoldRegulInsSub.MoldInspectBasisID);
                            sqlParameter.Add("MoldInsSeq", WinMoldRegulInsSub.MoldSeq);
                            sqlParameter.Add("MldValue", WinMoldRegulInsSub.MldValue ==null ? 
                                0 : double.Parse(WinMoldRegulInsSub.MldValue.Replace(",", "")));
                            sqlParameter.Add("MldInspectLegend", WinMoldRegulInsSub.MldInspectLegend == null ? 
                                "" : WinMoldRegulInsSub.MldInspectLegend);
                            sqlParameter.Add("Comments", Lib.Instance.CheckNull(WinMoldRegulInsSub.Comments));
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_dvlMoldIns_iRegularInspectSub";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "MoldInspectID";
                            pro2.OutputLength = "10";

                            Prolist.Add(pro2);
                            ListParameter.Add(sqlParameter);
                        }

                        for (int i = 0; i < dgdMold_InspectSub2.Items.Count; i++)
                        {
                            WinMoldRegulInsSub = dgdMold_InspectSub2.Items[i] as Win_dvl_MoldRegularInspect_U_Sub_CodeView;
                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldInspectID", strInspectID);
                            sqlParameter.Add("MoldSeq", WinMoldRegulInsSub.MoldSeq);
                            sqlParameter.Add("MoldInsBasisID", WinMoldRegulInsSub.MoldInspectBasisID);
                            sqlParameter.Add("MoldInsSeq", WinMoldRegulInsSub.MoldSeq);
                            sqlParameter.Add("MldValue", WinMoldRegulInsSub.MldValue == null ?
                                0 : double.Parse(WinMoldRegulInsSub.MldValue.Replace(",", "")));
                            sqlParameter.Add("MldInspectLegend", WinMoldRegulInsSub.MldInspectLegend == null ?
                                "" : WinMoldRegulInsSub.MldInspectLegend);
                            sqlParameter.Add("Comments", Lib.Instance.CheckNull(WinMoldRegulInsSub.Comments));
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro3 = new Procedure();
                            pro3.Name = "xp_dvlMoldIns_iRegularInspectSub";
                            pro3.OutputUseYN = "N";
                            pro3.OutputName = "MoldInspectID";
                            pro3.OutputLength = "10";

                            Prolist.Add(pro3);
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
                                if (kv.key == "MoldInspectID")
                                {
                                    sGetID = kv.value;
                                    flag = true;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("[저장실패]\r\n" + list_Result[0].value.ToString());
                            flag = false;
                            //return false;
                        }
                    }

                    #endregion

                    #region 수정

                    else if (strFlag.Equals("U"))
                    {
                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_dvlMoldIns_uRegularInspect";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "MoldInspectID";
                        pro1.OutputLength = "10";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        for (int i = 0; i < dgdMold_InspectSub1.Items.Count; i++)
                        {
                            WinMoldRegulInsSub = dgdMold_InspectSub1.Items[i] as Win_dvl_MoldRegularInspect_U_Sub_CodeView;
                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldInspectID", strInspectID);
                            sqlParameter.Add("MoldSeq", WinMoldRegulInsSub.MoldSeq);
                            sqlParameter.Add("MoldInsBasisID", WinMoldRegulInsSub.MoldInspectBasisID);
                            sqlParameter.Add("MoldInsSeq", WinMoldRegulInsSub.MoldSeq);
                            sqlParameter.Add("MldValue", WinMoldRegulInsSub.MldValue == null ?
                                0 : double.Parse(WinMoldRegulInsSub.MldValue.Replace(",", "")));
                            sqlParameter.Add("MldInspectLegend", WinMoldRegulInsSub.MldInspectLegend == null ?
                                "" : WinMoldRegulInsSub.MldInspectLegend);
                            sqlParameter.Add("Comments", Lib.Instance.CheckNull(WinMoldRegulInsSub.Comments));
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_dvlMoldIns_iRegularInspectSub";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "MoldInspectID";
                            pro2.OutputLength = "10";

                            Prolist.Add(pro2);
                            ListParameter.Add(sqlParameter);
                        }

                        for (int i = 0; i < dgdMold_InspectSub2.Items.Count; i++)
                        {
                            WinMoldRegulInsSub = dgdMold_InspectSub2.Items[i] as Win_dvl_MoldRegularInspect_U_Sub_CodeView;
                            sqlParameter.Add("MoldInspectID", strInspectID);
                            sqlParameter.Add("MoldSeq", WinMoldRegulInsSub.MoldSeq);
                            sqlParameter.Add("MoldInsBasisID", WinMoldRegulInsSub.MoldInspectBasisID);
                            sqlParameter.Add("MoldInsSeq", WinMoldRegulInsSub.MoldSeq);
                            sqlParameter.Add("MldValue", WinMoldRegulInsSub.MldValue == null ?
                                0 : double.Parse(WinMoldRegulInsSub.MldValue.Replace(",", "")));
                            sqlParameter.Add("MldInspectLegend", WinMoldRegulInsSub.MldInspectLegend == null ?
                                "" : WinMoldRegulInsSub.MldInspectLegend);
                            sqlParameter.Add("Comments", Lib.Instance.CheckNull(WinMoldRegulInsSub.Comments));
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro3 = new Procedure();
                            pro3.Name = "xp_dvlMoldIns_iRegularInspectSub";
                            pro3.OutputUseYN = "N";
                            pro3.OutputName = "MoldInspectID";
                            pro3.OutputLength = "10";

                            Prolist.Add(pro3);
                            ListParameter.Add(sqlParameter);
                        }

                        string[] Confirm = new string[2];
                        Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew(Prolist, ListParameter);
                        if (Confirm[0] != "success")
                        {
                            MessageBox.Show("[저장실패]\r\n" + Confirm[1].ToString());
                            flag = false;
                            //return false;
                        }
                        else
                        {
                            flag = true;
                        }
                    }

                    #endregion
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

        /// <summary>
        /// 입력사항 체크
        /// </summary>
        /// <returns></returns>
        private bool CheckData()
        {
            bool flag = true;

            //if (txtMoldRInspectID.Text.Length <= 0 || txtMoldRInspectID.Text.Equals(""))
            //{
            //    MessageBox.Show("검사번호가 입력되지 않았습니다.");
            //    flag = false;
            //    return flag;
            //}

            if (cboRegularGubun.SelectedValue == null)
            {
                MessageBox.Show("정기검사구분이 선택되지 않았습니다.");
                flag = false;
                return flag;
            }

            if (txtMoldLotNo.Text.Length <= 0 || txtMoldLotNo.Text.Equals(""))
            {
                MessageBox.Show("금형LotNo이 입력되지 않았습니다.");
                flag = false;
                return flag;
            }

            if (dtpMoldInspectDate.SelectedDate == null)
            {
                MessageBox.Show("점검일자가 선택되지 않았습니다.");
                flag = false;
                return flag;
            }

            //if (txtArticle.Text.Length <= 0 || txtArticle.Text.Equals(""))
            //{
            //    MessageBox.Show("품명이 입력되지 않았습니다.");
            //    flag = false;
            //    return flag;
            //}

            //if (dtpRevision.SelectedDate == null)
            //{
            //    MessageBox.Show("개정일자가 선택되지 않았습니다.");
            //    flag = false;
            //    return flag;
            //}

            return flag;
        }

        private void cboRegularGubun_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                if (txtMoldLotNo.Tag != null)
                {
                    if (cboRegularGubun.SelectedValue != null)
                    {
                        GetMoldInspectInfo(txtMoldLotNo.Tag.ToString(), cboRegularGubun.SelectedValue.ToString());
                    }
                }
            }
        }

        //금형정보 가져가기
        private void GetMoldInfo(object obj)
        {
            try
            {
                if (obj != null)
                {
                    string sql = " select dm.ProductionArticleID, ma.Article from dvl_Mold dm, mt_Article ma ";
                    sql += " where ma.ArticleID = dm.ProductionArticleID    ";
                    sql += " and dm.MoldID = '" + obj.ToString() + "'       ";

                    DataSet ds = DataStore.Instance.QueryToDataSet(sql);
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        DataTable dt = ds.Tables[0];
                        if (dt.Rows.Count > 0)
                        {
                            txtArticle.Text = Lib.Instance.CheckNull(dt.Rows[0].ItemArray[1]);
                            Tag = Lib.Instance.CheckNull(dt.Rows[0].ItemArray[0]);
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

        private void txtMoldLotNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtMoldLotNo, (int)Defind_CodeFind.DCF_MOLD, "");

                if (cboRegularGubun.SelectedValue != null)
                {
                    if (txtMoldLotNo.Tag != null)
                    {
                        GetMoldInspectInfo(txtMoldLotNo.Tag.ToString(), cboRegularGubun.SelectedValue.ToString());
                    }
                    GetMoldInfo(txtMoldLotNo.Tag);
                }

                dtpMoldInspectDate.Focus();
            }
        }

        private void btnMoldLotNoPf_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtMoldLotNo, (int)Defind_CodeFind.DCF_MOLD, "");

            if (cboRegularGubun.SelectedValue != null)
            {
                if (txtMoldLotNo.Tag != null)
                {
                    GetMoldInspectInfo(txtMoldLotNo.Tag.ToString(), cboRegularGubun.SelectedValue.ToString());
                }
                GetMoldInfo(txtMoldLotNo.Tag);
            }

            dtpMoldInspectDate.Focus();
        }

        //금형번호 선택시, 선택된 금형의 정보를 가져온다.
        private void GetMoldInspectInfo(string strMoldID, string strCycleGbn)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("MoldID", strMoldID);
                sqlParameter.Add("CycleGbn", strCycleGbn);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMoldIns_sRegularInspectSubByMoldID", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    int i = 0;
                    int j = 0;
                    int k = 0;

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinMoldRIns = new Win_dvl_MoldRegularInspect_U_Sub_CodeView()
                            {
                                Num = k+1,
                                MoldID = dr["MoldID"].ToString(),
                                MoldInspectBasisID = dr["MoldInspectBasisID"].ToString(),
                                MoldSeq = dr["MoldSeq"].ToString(),
                                MoldInspectBasisDate = dr["MoldInspectBasisDate"].ToString(),
                                MoldInspectItemName = dr["MoldInspectItemName"].ToString(),
                                MoldInspectContent = dr["MoldInspectContent"].ToString(),
                                MoldInspectGbn = dr["MoldInspectGbn"].ToString(),
                                MoldInspectGbnName = dr["MoldInspectGbnName"].ToString(),
                                MoldInspectCheckGbn = dr["MoldInspectCheckGbn"].ToString(),
                                MoldInspectCheckName = dr["MoldInspectCheckName"].ToString(),
                                MoldInspectCycleGbn = dr["MoldInspectCycleGbn"].ToString(),
                                MoldInspectCycleName = dr["MoldInspectCycleName"].ToString(),
                                MoldInspectCycleDate = dr["MoldInspectCycleDate"].ToString(),
                                MoldInspectRecordGbn = dr["MoldInspectRecordGbn"].ToString(),
                                MoldInspectRecordGbnName = dr["MoldInspectRecordGbnName"].ToString(),
                                MoldInspectImagePath = dr["MoldInspectImagePath"].ToString(),
                                MoldInspectImageFile = dr["MoldInspectImageFile"].ToString(),
                                ovcLegend = this.ovcLegend
                            };

                            if (WinMoldRIns.MoldInspectImageFile != null && !WinMoldRIns.MoldInspectImageFile.Replace(" ", "").Equals(""))
                            {
                                WinMoldRIns.imageFlag = true;
                                if (!Lib.Instance.Right(WinMoldRIns.MoldInspectImageFile, 3).Equals("pdf"))
                                {
                                    string strImage = "/" + WinMoldRIns.MoldInspectBasisID + "/" + WinMoldRIns.MoldInspectImageFile;
                                    //WinMoldRIns.ImageView = SetImage(strImage);
                                }
                            }
                            else
                            {
                                WinMoldRIns.imageFlag = false;
                            }

                            //if (WinMoldRIns.MoldInspectGbn.Equals("1"))
                            //{
                            //    WinMoldRIns.Num = i + 1;
                            //    dgdMold_InspectSub1.Items.Add(WinMoldRIns);
                            //    i++;
                            //}
                            //else if(WinMoldRIns.MoldInspectGbn.Equals("2"))
                            //{
                            //    WinMoldRIns.Num = j + 1;
                            //    dgdMold_InspectSub2.Items.Add(WinMoldRIns);
                            //    j++;
                            //}
                            if (WinMoldRIns.MoldInspectRecordGbn.Equals("01"))
                            {
                                WinMoldRIns.Num = i + 1;
                                dgdMold_InspectSub1.Items.Add(WinMoldRIns);
                                i++;
                            }
                            else if (WinMoldRIns.MoldInspectRecordGbn.Equals("02"))
                            {
                                WinMoldRIns.Num = j + 1;
                                dgdMold_InspectSub2.Items.Add(WinMoldRIns);
                                j++;
                            }
                            k++;
                        }

                        strMoldBasisID = drc[0]["MoldInspectBasisID"].ToString();
                        txtMoldRInspectID.Tag = strMoldBasisID;

                        if (drc[0]["MoldInspectBasisDate"].ToString().Length == 8)
                        {
                            dtpBasisDate.SelectedDate = Lib.Instance.strConvertDate(drc[0]["MoldInspectBasisDate"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생,오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void btnSeeImage_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellInfo dgdinfoOne = dgdMold_InspectSub1.CurrentCell;
            DataGridCellInfo dgdinfoTwo = dgdMold_InspectSub2.CurrentCell;

            if (dgdinfoOne.Column != null)
            {
                WinMoldRegulInsSub = dgdMold_InspectSub1.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;
            }
            if (dgdinfoTwo.Column != null)
            {
                WinMoldRegulInsSub = dgdMold_InspectSub2.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;
            }

            if (WinMoldRegulInsSub != null && !WinMoldRegulInsSub.MoldInspectImageFile.Equals(""))
            {
                FTP_DownLoadFile(WinMoldRegulInsSub.MoldInspectImagePath + "/" + WinMoldRegulInsSub.MoldInspectImageFile);
            }
        }

        //
        private void FTP_DownLoadFile(string strFilePath)
        {
            _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

            string[] fileListSimple;
            string[] fileListDetail;

            fileListSimple = _ftp.directoryListSimple("", Encoding.Default);
            fileListDetail = _ftp.directoryListDetailed("", Encoding.Default);

            bool ExistFile = false;
            ExistFile = MakeFileInfoList(fileListSimple, fileListDetail, strFilePath.Split('/')[3].Trim());

            int fileLength = _listFileInfo.Count;

            if (ExistFile)
            {
                string str_remotepath = string.Empty;
                string str_localpath = string.Empty;

                str_remotepath = strFilePath.ToString();
                str_localpath = LOCAL_DOWN_PATH + "\\" + strFilePath.Substring(strFilePath.LastIndexOf("/")).ToString();

                DirectoryInfo DI = new DirectoryInfo(LOCAL_DOWN_PATH);
                if (DI.Exists)
                {
                    DI.Create();
                }

                FileInfo file = new FileInfo(str_localpath);
                if (file.Exists)
                {
                    //if (MessageBox.Show("같은 이름의 파일이 존재하여" +
                    //    "진행합니다. 계속 하시겠습니까?", "", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    //{
                    //    file.Delete();
                    //}
                    //else
                    //{
                    //    MessageBox.Show("C:Temp 폴더를 확인하세요.");
                    //    return;
                    //}
                    file.Delete();
                }

                _ftp.download(str_remotepath.Substring(str_remotepath.Substring
                    (0, str_remotepath.LastIndexOf("/")).LastIndexOf("/")), str_localpath);

                ProcessStartInfo proc = new ProcessStartInfo(str_localpath);
                proc.UseShellExecute = true;
                Process.Start(proc);
            }
        }

        //업로드 폴더 존재유무 확인
        private bool MakeFileInfoList(string[] simple, string[] detail, string str_InspectID)
        {
            bool tf_return = false;
            foreach (string filename in simple)
            {
                foreach (string info in detail)
                {
                    if (info.Contains(filename) == true)
                    {
                        if (MakeFileInfoList(filename, info, str_InspectID) == true)
                        {
                            tf_return = true;
                        }
                    }
                }
            }
            return tf_return;
        }

        //업로드 폴더 존재유무 확인
        private bool MakeFileInfoList(string simple, string detail, string strCompare)
        {
            UploadFileInfo info = new UploadFileInfo();
            info.Filename = simple;
            info.Filepath = detail;

            if (simple.Length > 0)
            {
                string[] tokens = detail.Split(new[] { ' ' }, 9, StringSplitOptions.RemoveEmptyEntries);
                string name = tokens[3].ToString();
                string permissions = tokens[2].ToString();

                if (permissions.Contains("D") == true)
                {
                    info.Type = FtpFileType.DIR;
                }
                else
                {
                    info.Type = FtpFileType.File;
                }

                if (info.Type == FtpFileType.File)
                {
                    info.Size = Convert.ToInt64(detail.Substring(17, detail.LastIndexOf(simple) - 17).Trim());
                }

                _listFileInfo.Add(info);

                if (string.Compare(simple, strCompare, false) == 0)
                    return true;
            }

            return false;
        }

        private void TextBoxFocusInDataGrid(object sender, KeyEventArgs e)
        {
            Lib.Instance.DataGridINControlFocus(sender, e);
        }

        private void TextBoxFocusInDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Lib.Instance.DataGridINBothByMouseUP(sender, e);
        }

        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                DataGridCell cell = sender as DataGridCell;
                cell.IsEditing = true;
            }
        }

        private void DataGridCell_Sub1_KeyDown(object sender, KeyEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldRegulInsSub = dgdMold_InspectSub1.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;
                int rowCount = dgdMold_InspectSub1.Items.IndexOf(dgdMold_InspectSub1.CurrentItem);
                int colCount = dgdMold_InspectSub1.Columns.IndexOf(dgdtpetxtLegend);
                //dgdInComBoNum = rowCount;

                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (dgdMold_InspectSub1.Items.Count > rowCount + 1 && 
                        dgdMold_InspectSub1.Columns.Count - 1 == 
                        dgdMold_InspectSub1.Columns.IndexOf(dgdMold_InspectSub1.CurrentCell.Column))
                    {
                        //dgdInspectSub1.SelectedIndex = rowCount+1;
                        dgdMold_InspectSub1.CurrentCell = new DataGridCellInfo
                            (dgdMold_InspectSub1.Items[rowCount + 1], dgdMold_InspectSub1.Columns[colCount]);
                    }
                    else if (dgdMold_InspectSub1.Items.Count == rowCount + 1 && 
                        dgdMold_InspectSub1.Columns.Count - 1 ==
                        dgdMold_InspectSub1.Columns.IndexOf(dgdMold_InspectSub1.CurrentCell.Column))
                    {
                        if (dgdMold_InspectSub2.Items.Count > 0)
                        {
                            dgdMold_InspectSub2.Focus();
                            //dgdInspectSub2.SelectedIndex = 0;
                            dgdMold_InspectSub2.CurrentCell = new DataGridCellInfo(dgdMold_InspectSub2.Items[0],
                                dgdMold_InspectSub2.Columns[dgdMold_InspectSub2.Columns.IndexOf(dgdtpetxtValue)]);
                        }
                        else
                        {
                            btnSave.Focus();
                        }
                    }
                    else
                    {
                        dgdMold_InspectSub1.CurrentCell = new DataGridCellInfo
                            (dgdMold_InspectSub1.Items[rowCount], dgdMold_InspectSub1.Columns[colCount + 1]);
                    }                    
                }
            }
        }

        private void DataGridCell_Sub2_KeyDown(object sender, KeyEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldRegulInsSub = dgdMold_InspectSub2.CurrentItem as Win_dvl_MoldRegularInspect_U_Sub_CodeView;
                int rowCount = dgdMold_InspectSub2.Items.IndexOf(dgdMold_InspectSub2.CurrentItem);
                int colCount = dgdMold_InspectSub2.Columns.IndexOf(dgdtpetxtValue);

                if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    (sender as DataGridCell).IsEditing = false;

                    if (dgdMold_InspectSub2.Items.Count > rowCount + 1 && 
                        dgdMold_InspectSub1.Columns.Count - 1 ==
                        dgdMold_InspectSub1.Columns.IndexOf(dgdMold_InspectSub2.CurrentCell.Column))
                    {
                        //dgdInspectSub2.SelectedIndex = rowCount + 1;
                        dgdMold_InspectSub2.CurrentCell = new DataGridCellInfo
                            (dgdMold_InspectSub2.Items[rowCount + 1], dgdMold_InspectSub2.Columns[colCount]);
                    }
                    else if (dgdMold_InspectSub2.Items.Count == rowCount + 1 &&
                        dgdMold_InspectSub1.Columns.Count - 1 ==
                        dgdMold_InspectSub1.Columns.IndexOf(dgdMold_InspectSub2.CurrentCell.Column))
                    {
                        btnSave.Focus();
                    }
                    else
                    {
                        dgdMold_InspectSub2.CurrentCell = new DataGridCellInfo
                            (dgdMold_InspectSub2.Items[rowCount], dgdMold_InspectSub2.Columns[colCount+1]);
                    }
                }
            }
        }

        //
        private void btnGoBasis_Click(object sender, RoutedEventArgs e)
        {
            int k = 0;
            foreach (MenuViewModel mvm in MainWindow.mMenulist)
            {
                if (mvm.Menu.Equals("★금형 점검기준등록"))
                {
                    break;
                }
                k++;
            }

            if (MainWindow.MainMdiContainer.Children.Contains(MainWindow.mMenulist[k].subProgramID as MdiChild))
            {
                (MainWindow.mMenulist[k].subProgramID as MdiChild).Focus();
            }
            else
            {
                Type type = Type.GetType("WizMes_ANT." + MainWindow.mMenulist[k].ProgramID.Trim(), true);
                object uie = Activator.CreateInstance(type);

                MainWindow.mMenulist[k].subProgramID = new MdiChild()
                {
                    Title = "AFT [" + MainWindow.mMenulist[k].MenuID.Trim() + "] " + MainWindow.mMenulist[k].Menu.Trim() +
                            " (→" + MainWindow.mMenulist[k].ProgramID + ")",
                    Height = SystemParameters.PrimaryScreenHeight * 0.8,
                    MaxHeight = SystemParameters.PrimaryScreenHeight * 0.85,
                    Width = SystemParameters.WorkArea.Width * 0.85,
                    MaxWidth = SystemParameters.WorkArea.Width,
                    Content = uie as UIElement,
                    Tag = MainWindow.mMenulist[k]
                };
                Lib.Instance.AllMenuLogInsert(MainWindow.mMenulist[k].MenuID, MainWindow.mMenulist[k].Menu, MainWindow.mMenulist[k].subProgramID);
                MainWindow.MainMdiContainer.Children.Add(MainWindow.mMenulist[k].subProgramID as MdiChild);
            }
        }

        #region 2022-04-11 검색조건 플러스파인더 추가
        private void btnMoldLotNoSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtMoldLotNoSrh, (int)Defind_CodeFind.DCF_MOLD, "");
        }

        private void btnPfMoldSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtMoldSrh, (int)Defind_CodeFind.DCF_Article, "");
        }

        private void txtMoldSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtMoldSrh, (int)Defind_CodeFind.DCF_Article, "");
            }
        }

        private void txtMoldLotNoSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtMoldLotNoSrh, (int)Defind_CodeFind.DCF_MOLD, "");
            }
        }
        #endregion

    }

    class Win_dvl_MoldRegularInspect_U_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }
        public string MoldInspectID { get; set; }
        public string MoldInspectCycleGbn { get; set; }
        public string MoldInspectCycleName { get; set; }
        public string MoldNo { get; set; }
        public string MoldInspectDate { get; set; }
        public string ArticleID { get; set; }
        public string Article { get; set; }
        public string MoldInspectBasisDate { get; set; }
        public string MoldInspectBasisID { get; set; }
        public string MoldInspectUserID { get; set; }
        public string Person { get; set; }
        public string MoldID { get; set; }
        public string HitCount { get; set; }
        public string DefectContents { get; set; }
        public string DefectReason { get; set; }
        public string DefectRespectContents { get; set; }
        public string Comments { get; set; }
        public string MoldInspectContent { get; set; }        

        public string MoldInspectDate_CV { get; set; }
        public string MoldInspectBasisDate_CV { get; set; }
    }

    class Win_dvl_MoldRegularInspect_U_Sub_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }
        public string MoldInspectID { get; set; }
        public string MoldInspectBasisID { get; set; }
        public string MoldID { get; set; }
        public string MoldInspectBasisDate { get; set; }
        public string MoldSeq { get; set; }
        public string MoldInsSeq { get; set; }
        public string MoldInspectItemName { get; set; }
        public string MoldInspectContent { get; set; }
        public string MoldInspectGbn { get; set; }
        public string MoldInspectGbnName { get; set; }
        public string MoldInspectCheckGbn { get; set; }
        public string MoldInspectCheckName { get; set; }
        public string MoldInspectCycleGbn { get; set; }
        public string MoldInspectCycleName { get; set; }
        public string MoldInspectCycleDate { get; set; }
        public string MoldInspectRecordGbn { get; set; }
        public string MoldInspectRecordGbnName { get; set; }
        public string MldInspectLegend { get; set; }
        public string MldValue { get; set; }
        public string Comments { get; set; }
        public string MoldInspectImagePath { get; set; }
        public string MoldInspectImageFile { get; set; }

        public BitmapImage ImageView { get; set; }
        public bool imageFlag { get; set; }
        public string LegendShape { get; set; }
        public ObservableCollection<CodeView> ovcLegend { get; set; }
    }

    //class Win_dvl_MoldRegularInspect_U_Sub2_CodeView : BaseView
    //{
    //    public int Num { get; set; }
    //    public string MoldInspectID { get; set; }
    //    public string MoldInspectBasisID { get; set; }
    //    public string MoldID { get; set; }
    //    public string MoldInspectBasisDate { get; set; }
    //    public string MoldSeq { get; set; }
    //    public string MoldInsSeq { get; set; }
    //    public string MoldInspectItemName { get; set; }
    //    public string MoldInspectContent { get; set; }
    //    public string MoldInspectGbn { get; set; }
    //    public string MoldInspectGbnName { get; set; }
    //    public string MoldInspectCheckGbn { get; set; }
    //    public string MoldInspectCheckName { get; set; }
    //    public string MoldInspectCycleGbn { get; set; }
    //    public string MoldInspectCycleName { get; set; }
    //    public string MoldInspectCycleDate { get; set; }
    //    public string MoldInspectRecordGbn { get; set; }
    //    public string MoldInspectRecordGbnName { get; set; }
    //    public string MldInspectLegend { get; set; }
    //    public string MldValue { get; set; }
    //    public string Comments { get; set; }
    //    public string MoldInspectImagePath { get; set; }
    //    public string MoldInspectImageFile { get; set; }

    //    public BitmapImage ImageView { get; set; }
    //    public bool imageFlag { get; set; }
    //}
}
