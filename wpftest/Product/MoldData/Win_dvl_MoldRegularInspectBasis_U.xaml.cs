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
using WizMes_ANT.PopUp;

namespace WizMes_ANT
{
    /// <summary>
    /// Win_dvl_MoldRegularInspectBasis_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_dvl_MoldRegularInspectBasis_U : UserControl
    {
        List<Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView> RData = new List<Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView>();
        string strFlag = string.Empty;
        bool strCopy = false;
        int rowNum = 0;
        /// <summary>
        /// 하위그리드 행넘버
        /// </summary>
        /// <summary>
        /// 하위그리드 현재 행넘버
        /// </summary>
        int currentSubRowNum = 0;
        Win_dvl_MoldRegularInspectBasis_U_CodeView MoldRegularInsBa = new Win_dvl_MoldRegularInspectBasis_U_CodeView();
        Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView MoldRegularInsBaSub = new Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView();

        // FTP 활용모음.
        string strImagePath = string.Empty;
        string strFullPath = string.Empty;

        List<string[]> listFtpFile = new List<string[]>();
        private FTP_EX _ftp = null;

        //string FTP_ADDRESS = "ftp://wizis.iptime.org/ImageData/MoldReqularInspect";
        string FTP_ADDRESS = "ftp://" + LoadINI.FileSvr + ":"
            + LoadINI.FTPPort + LoadINI.FtpImagePath + "/MoldReqularInspect";
        //string FTP_ADDRESS = "ftp://222.104.222.145:25000/ImageData/MoldReqularInspect";
        //string FTP_ADDRESS = "ftp://192.168.0.95/ImageData/MoldReqularInspect";
        private const string FTP_ID = "wizuser";
        private const string FTP_PASS = "wiz9999";
        private const string LOCAL_DOWN_PATH = "C:\\Temp";

        public Win_dvl_MoldRegularInspectBasis_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Lib.Instance.UiLoading(sender);
        }

        //금형종류 라벨 클릭시
        private void lblMoldKindSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldKindSrh.IsChecked == true) { chkMoldKindSrh.IsChecked = false; }
            else { chkMoldKindSrh.IsChecked = true; }
        }

        //금형종류 라벨 in 체크박스 체크시
        private void chkMoldKindSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldKindSrh.IsEnabled = true;
        }

        //금형종류  라벨 in 체크박스 언체크시
        private void chkMoldKindSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldKindSrh.IsEnabled = false;
        }

        ///금형LotNo 라벨 클릭시
        private void lblMoldLotNoSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldLotNoSrh.IsChecked == true) { chkMoldLotNoSrh.IsChecked = false; }
            else { chkMoldLotNoSrh.IsChecked = true; }
        }

        //금형LotNo 라벨 in 체크박스 체크시
        private void chkMoldLotNoSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldLotNoSrh.IsEnabled = true;
        }

        //금형LotNo 라벨 in 체크박스 언체크시
        private void chkMoldLotNoSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldLotNoSrh.IsEnabled = false;
        }

        //품명 라벨 클릭시
        private void lblArticleSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticleSrh.IsChecked == true) { chkArticleSrh.IsChecked = false; }
            else { chkArticleSrh.IsChecked = true; }
        }

        //품명 라벨 in 체크박스 체크시
        private void chkArticleSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = true;
        }

        //품명 라벨 in 체크박스 언체크시
        private void chkArticleSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = false;
        }

        /// <summary>
        /// 수정,추가 저장 후
        /// </summary>
        private void CanBtnControl()
        {
            Lib.Instance.UiButtonEnableChange_IUControl(this);
            grdInput.IsEnabled = false;
            listFtpFile.Clear();
        }

        /// <summary>
        /// 수정,추가 진행 중
        /// </summary>
        private void CantBtnControl()
        {
            Lib.Instance.UiButtonEnableChange_SCControl(this);
            grdInput.IsEnabled = true;
        }

        //유지추가 클릭
        private void btnRemainAdd_Click(object sender, RoutedEventArgs e)
        {
            MoldRegularInsBa = dgdMain.SelectedItem as Win_dvl_MoldRegularInspectBasis_U_CodeView;

            if (MoldRegularInsBa != null)
            {
                rowNum = dgdMain.SelectedIndex;
                dgdMain.IsHitTestVisible = false;
                tbkMsg.Text = "자료 추가 중";
                CantBtnControl();

                txtStandardNumber.Clear();
                dtpRevision.SelectedDate = null;
                dtpRevision.Refresh();

                strCopy = true;
                FillGridSub(MoldRegularInsBa.MoldInspectBasisID);
                strFlag = "I";
            }
            else
            {
                MessageBox.Show("복사할 대상이 선택되지 않았습니다.");
            }
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CantBtnControl();
            strCopy = false;
            strFlag = "I";

            if (dgdSub.Items.Count > 0)
            {
                dgdSub.Items.Clear();
                dgdSub.Refresh();
            }
            
            dgdMain.IsHitTestVisible = false;
            lblMsg.Visibility = Visibility.Visible;
            tbkMsg.Text = "자료 입력 중";
            rowNum = dgdMain.SelectedIndex;
            this.DataContext = null;
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            MoldRegularInsBa = dgdMain.SelectedItem as Win_dvl_MoldRegularInspectBasis_U_CodeView;

            if (MoldRegularInsBa != null)
            {
                rowNum = dgdMain.SelectedIndex;
                dgdMain.IsHitTestVisible = false;
                tbkMsg.Text = "자료 수정 중";
                lblMsg.Visibility = Visibility.Visible;
                CantBtnControl();
                strCopy = false;
                strFlag = "U";
            }
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MoldRegularInsBa = dgdMain.SelectedItem as Win_dvl_MoldRegularInspectBasis_U_CodeView;

            if (MoldRegularInsBa == null)
            {
                MessageBox.Show("삭제할 데이터가 지정되지 않았습니다. 삭제데이터를 지정하고 눌러주세요");
            }
            else
            {
                if (MessageBox.Show("선택하신 항목을 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (dgdMain.Items.Count > 0 && dgdMain.SelectedItem != null)
                    {
                        rowNum = dgdMain.SelectedIndex;
                    }

                    if (DeleteData(MoldRegularInsBa.MoldInspectBasisID))
                    {
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
            //검색버튼 비활성화
            btnSearch.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>

            {
                try
                {
                    rowNum = 0;
                    using (Loading lw = new Loading(FillGrid))
                    {
                        lw.ShowDialog();
                        if (dgdMain.Items.Count <= 0)
                        {
                            this.DataContext = null;
                            MessageBox.Show("조회된 내용이 없습니다.");
                        }
                        else
                        {
                            dgdMain.SelectedIndex = rowNum;
                        }

                        btnSearch.IsEnabled = true;
                    }
                }
                catch (Exception ee)
                {
                    MessageBox.Show("예외처리 - " + ee.ToString());
                }

            }), System.Windows.Threading.DispatcherPriority.Background);

        }

        //저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (SaveData(strFlag, txtStandardNumber.Text))
            {
                CanBtnControl();
                lblMsg.Visibility = Visibility.Hidden;
                rowNum = 0;
                dgdMain.IsHitTestVisible = true;
                re_Search(rowNum);
                strCopy = false;
                strFlag = string.Empty;
            }
        }

        //취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CanBtnControl();
            if (dgdSub.Items.Count > 0)
            {
                dgdSub.Items.Clear();
                dgdSub.Refresh();
            }

            if (!strFlag.Equals(string.Empty))
            {
                re_Search(rowNum);
            }

            strFlag = string.Empty;
            dgdMain.IsHitTestVisible = true;
            strCopy = false;
            strFlag = string.Empty;
        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[4];
            lst[0] = "금형일상점검기준등록 목록조회";
            lst[1] = "금형일상점검기준등록 목록별 세부사항";
            lst[2] = dgdMain.Name;
            lst[3] = dgdSub.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);
            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdMain.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdMain);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdMain);

                    Name = dgdMain.Name;
                    Lib.Instance.GenerateExcel(dt, Name);
                    Lib.Instance.excel.Visible = true;
                }
                else if (ExpExc.choice.Equals(dgdSub.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdSub);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdSub);

                    Name = dgdSub.Name;
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

            if (dgdMain.Items.Count > 0)
            {
                dgdMain.SelectedIndex = selectedIndex;
            }
        }

        /// <summary>
        /// 실조회
        /// </summary>
        private void FillGrid()
        {
            if (dgdMain.Items.Count > 0)
            {
                dgdMain.Items.Clear();
            }

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("nchkMoldName", chkMoldKindSrh.IsChecked==true ? 1: 0);
                sqlParameter.Add("sMoldName", chkMoldKindSrh.IsChecked == true ? txtMoldKindSrh.Text : "");
                sqlParameter.Add("nchkMoldID", chkMoldLotNoSrh.IsChecked == true ? 1: 0);
                sqlParameter.Add("sMoldID", chkMoldLotNoSrh.IsChecked == true ? txtMoldLotNoSrh.Text : "" );
                sqlParameter.Add("chkArticle", chkArticleSrh.IsChecked == true ? 1:0);
                sqlParameter.Add("sArticle", chkArticleSrh.IsChecked == true ? txtArticleSrh.Text : "");

                ds = DataStore.Instance.ProcedureToDataSet("xp_DvlMold_sMoldRegularInspectBasis", sqlParameter, false);

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
                            var WinMoldRegul = new Win_dvl_MoldRegularInspectBasis_U_CodeView()
                            {
                                Num = i + 1,
                                MoldInspectBasisID = dr["MoldInspectBasisID"].ToString(),
                                MoldID  = dr["MoldID"].ToString(),
                                MoldNo = dr["MoldNo"].ToString(),
                                MoldInspectBasisDate = dr["MoldInspectBasisDate"].ToString(),
                                MoldInspectContent = dr["MoldInspectContent"].ToString(),
                                Comments = dr["Comments"].ToString(),
                                ArticleID = dr["ArticleID"].ToString(),
                                Article = dr["Article"].ToString(),
                                MoldKindName = dr["MoldKindName"].ToString()
                            };

                            if (WinMoldRegul.MoldInspectBasisDate != null && !WinMoldRegul.MoldInspectBasisDate.Replace(" ", "").Equals(""))
                            {
                                //WinMCRegul.McInsBasisDate_CV = Lib.Instance.strConvertDate(WinMCRegul.McInsBasisDate);
                                WinMoldRegul.MoldInspectBasisDate_CV = Lib.Instance.StrDateTimeBar(WinMoldRegul.MoldInspectBasisDate);
                            }

                            dgdMain.Items.Add(WinMoldRegul);
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

        private void dgdMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgdSub.Items.Count > 0)
            {
                dgdSub.Items.Clear();
                dgdSub.Refresh();
            }
            RData.Clear();

            MoldRegularInsBa = dgdMain.SelectedItem as Win_dvl_MoldRegularInspectBasis_U_CodeView;

            if (MoldRegularInsBa != null)
            {
                this.DataContext = MoldRegularInsBa;
                FillGridSub(MoldRegularInsBa.MoldInspectBasisID);
            }
        }

        /// <summary>
        /// 서브 조회
        /// </summary>
        /// <param name="strID"></param>
        private void FillGridSub(string strID)
        {
            if (dgdSub.Items.Count > 0)
            {
                dgdSub.Items.Clear();
            }

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("MoldInspectBasisID", strID);
                sqlParameter.Add("nMoldInspectSeq", 0);
                ds = DataStore.Instance.ProcedureToDataSet("xp_DvlMold_sMoldRegularInspectBasisSub", sqlParameter, false);

                ObservableCollection<CodeView> ovcMoldCheck = ComboBoxUtil.Instance.GetCMCode_SetComboBox("MLDCHECKGBN", "");
                ObservableCollection<CodeView> ovcMoldCycle = ComboBoxUtil.Instance.GetCMCode_SetComboBox("MLDCYCLEGBN", "");
                ObservableCollection<CodeView> ovcMoldRecord = ComboBoxUtil.Instance.GetCMCode_SetComboBox("MLDRECORDGBN", "");

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
                            var WinMoldRegul = new Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView()
                            {
                                Num = i + 1,
                                MoldInspectBasisID = dr["MoldInspectBasisID"].ToString(),
                                MoldSeq = dr["MoldSeq"].ToString(),
                                MoldInspectItemName = dr["MoldInspectItemName"].ToString(),
                                MoldInspectContent = dr["MoldInspectContent"].ToString(),
                                MoldInspectGbn = dr["MoldInspectGbn"].ToString(),
                                MoldInspectGbnName = dr["MoldInspectGbnName"].ToString(),
                                MoldInspectCheckGbn = dr["MoldInspectCheckGbn"].ToString(),
                                MoldInspectCheckGbnName = dr["MoldInspectCheckGbnName"].ToString(),
                                MoldInspectCycleGbn = dr["MoldInspectCycleGbn"].ToString(),
                                MoldInspectCycleGbnName = dr["MoldInspectCycleGbnName"].ToString(),
                                MoldInspectCycleDate = dr["MoldInspectCycleDate"].ToString(),
                                MoldInspectRecordGbn = dr["MoldInspectRecordGbn"].ToString(),
                                MoldInspectRecordGbnName = dr["MoldInspectRecordGbnName"].ToString(),
                                MoldInspectImagePath = dr["MoldInspectImagePath"].ToString(),
                                MoldInspectImageFile = dr["MoldInspectImageFile"].ToString(),
                                MoldInspectComments = dr["MoldInspectComments"].ToString(),
                                ovcMoldInspectCheck = ovcMoldCheck,
                                ovcMoldInspectCycle = ovcMoldCycle,
                                ovcMoldInspectRecord = ovcMoldRecord
                            };

                            if (strCopy)
                            {
                                WinMoldRegul.MoldInspectImagePath = "";
                                WinMoldRegul.MoldInspectImageFile = "";
                            }
                            else
                            {
                                if (!WinMoldRegul.MoldInspectImageFile.Replace(" ", "").Equals(""))
                                {
                                    if (Lib.Instance.Right(WinMoldRegul.MoldInspectImageFile, 3).Equals("pdf"))
                                    {
                                        WinMoldRegul.imageFlag = true;
                                    }
                                    else
                                    {
                                        WinMoldRegul.imageFlag = true;
                                        //string strImage = "/" + WinMoldRegul.MoldInspectBasisID + "/" + WinMoldRegul.MoldInspectImageFile;
                                        WinMoldRegul.ImageView = SetImage(WinMoldRegul.MoldInspectImageFile, WinMoldRegul.MoldInspectBasisID);
                                        if (WinMoldRegul.ImageView == null) { WinMoldRegul.imageFlag = false; }
                                    }
                                }
                                else
                                {
                                    WinMoldRegul.imageFlag = false;
                                }
                            }

                            dgdSub.Items.Add(WinMoldRegul);
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
                sqlParameter.Add("MoldInspectBasisID", strID);

                string[] result = DataStore.Instance.ExecuteProcedure("xp_DvlMold_dMolRegularInspectBasis", sqlParameter, false);

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
        /// <param name="strID"></param>
        /// <returns></returns>
        private bool SaveData(string strFlag, string strID)
        {
            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                if (CheckData())
                {
                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                    sqlParameter.Add("MoldInspectBasisID", strID);
                    sqlParameter.Add("MoldID", txtMoldLotNo.Tag.ToString());
                    sqlParameter.Add("MoldInspectBasisDate", dtpRevision.SelectedDate.Value.ToString("yyyyMMdd"));
                    sqlParameter.Add("MoldInspectContent", txtRevisionContent.Text);
                    sqlParameter.Add("Comments", txtComments.Text);

                    #region 추가

                    if (strFlag.Equals("I"))
                    {
                        sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_DvlMold_iMoldRegularInspectBasis";
                        pro1.OutputUseYN = "Y";
                        pro1.OutputName = "MoldInspectBasisID";
                        pro1.OutputLength = "10";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        for (int i = 0; i < dgdSub.Items.Count; i++)
                        {
                            MoldRegularInsBaSub = dgdSub.Items[i] as Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView;
                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldInspectBasisID", strID);
                            sqlParameter.Add("nMoldSeq", MoldRegularInsBaSub.MoldSeq);
                            sqlParameter.Add("MoldInspectItemName", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectItemName));
                            sqlParameter.Add("MoldInspectContent", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectContent));
                            sqlParameter.Add("MoldInspectGbn", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectGbn));
                            sqlParameter.Add("MoldInspectCheckGbn", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectCheckGbn));
                            sqlParameter.Add("MoldInspectCycleGbn", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectCycleGbn));
                            sqlParameter.Add("nMoldInspectCycleDate", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectCycleDate));
                            sqlParameter.Add("MoldInspectRecordGbn", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectRecordGbn));
                            sqlParameter.Add("MoldImagePath", Lib.Instance.CheckNull(Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectImagePath)));
                            sqlParameter.Add("MoldImageFile", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectImageFile));
                            sqlParameter.Add("Comments", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectComments));
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);
                            
                            Procedure pro2 = new Procedure();
                            pro2.Name = "xp_DvlMold_iMoldRegularInspectBasisSub";
                            pro2.OutputUseYN = "N";
                            pro2.OutputName = "MoldInspectBasisID";
                            pro2.OutputLength = "10";

                            Prolist.Add(pro2);
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
                                if (kv.key == "MoldInspectBasisID")
                                {
                                    sGetID = kv.value;
                                    flag = true;
                                }
                            }

                            FTP_Save_File(listFtpFile, sGetID);
                            UpdateFTP_PathAndFile(sGetID,true);
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
                        sqlParameter.Add("UserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_DvlMold_uMoldRegularInspectBasis";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "MoldInspectBasisID";
                        pro1.OutputLength = "10";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);

                        for (int i = 0; i < RData.Count; i++)
                        {
                            MoldRegularInsBaSub = RData[i] as Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView;
                            //Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldInspectBasisID", strID);
                            sqlParameter.Add("MoldSeq", MoldRegularInsBaSub.MoldSeq);

                            Procedure pro3 = new Procedure();
                            pro3.Name = "xp_DvlMold_dMolRegularInspectBasisSub";
                            pro3.OutputUseYN = "N";
                            pro3.OutputName = "McInspectBasisID";
                            pro3.OutputLength = "10";

                            Prolist.Add(pro3);
                            ListParameter.Add(sqlParameter);
                        }

                        for (int i = 0; i < dgdSub.Items.Count; i++)
                        {
                            MoldRegularInsBaSub = dgdSub.Items[i] as Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView;
                            sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldInspectBasisID", strID);
                            sqlParameter.Add("nMoldSeq", MoldRegularInsBaSub.MoldSeq);
                            sqlParameter.Add("MoldInspectItemName", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectItemName));
                            sqlParameter.Add("MoldInspectContent", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectContent));
                            sqlParameter.Add("MoldInspectGbn", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectGbn));
                            sqlParameter.Add("MoldInspectCheckGbn", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectCheckGbn));

                            sqlParameter.Add("MoldInspectCycleGbn", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectCycleGbn));
                            sqlParameter.Add("nMoldInspectCycleDate", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectCycleDate));
                            sqlParameter.Add("MoldInspectRecordGbn", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectRecordGbn));
                            //sqlParameter.Add("MoldImagePath", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectImagePath)+"/"+strID);
                            sqlParameter.Add("MoldImagePath", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectImagePath));
                            sqlParameter.Add("MoldImageFile", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectImageFile));

                            sqlParameter.Add("Comments", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectComments));
                            

                            if (MoldRegularInsBaSub.RowItemCount != -1)
                            {
                                sqlParameter.Add("UserID", MainWindow.CurrentUser);

                                Procedure pro2 = new Procedure();
                                pro2.Name = "xp_DvlMold_uMoldRegularInspectBasisSub";
                                pro2.OutputUseYN = "N";
                                pro2.OutputName = "MoldInspectBasisID";
                                pro2.OutputLength = "10";

                                Prolist.Add(pro2);
                                ListParameter.Add(sqlParameter);
                            }
                            else
                            {
                                sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                                Procedure pro2 = new Procedure();
                                pro2.Name = "xp_DvlMold_iMoldRegularInspectBasisSub";
                                pro2.OutputUseYN = "N";
                                pro2.OutputName = "MoldInspectBasisID";
                                pro2.OutputLength = "10";

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
                            //return false;
                        }
                        else
                        {
                            flag = true;
                            FTP_Save_File(listFtpFile, strID);
                            UpdateFTP_PathAndFile(strID,false);
                        }
                    }

                    #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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

            if (txtMoldLotNo.Text.Length <= 0 || txtMoldLotNo.Text.Equals(""))
            {
                MessageBox.Show("금형LotNo가 입력되지 않았습니다.");
                flag = false;
                return flag;
            }

            if (dtpRevision.SelectedDate == null)
            {
                MessageBox.Show("개정일자가 선택되지 않았습니다.");
                flag = false;
                return flag;
            }

            return flag;
        }

        //금형LotNo 엔터키 이벤트용(입력)
        private void txtMoldLotNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtMoldLotNo, (int)Defind_CodeFind.DCF_MOLD, "");
                GetMoldInfo(txtMoldLotNo.Tag);
            }
        }

        //금형LotNo 버튼 클릭 이벤트용(입력)
        private void btnMoldLotNoPf_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtMoldLotNo, (int)Defind_CodeFind.DCF_MOLD, "");
            GetMoldInfo(txtMoldLotNo.Tag);
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

        private void UpdateFTP_PathAndFile(string ID, bool Flag)
        {
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();

                for (int i = 0; i < dgdSub.Items.Count; i++)
                {
                    MoldRegularInsBaSub = dgdSub.Items[i] as Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView;

                    if (MoldRegularInsBaSub.MoldInspectImageFile != null && !MoldRegularInsBaSub.MoldInspectImageFile.Replace(" ", "").Equals(""))
                    {
                        sqlParameter = new Dictionary<string, object>();
                        sqlParameter.Clear();
                        sqlParameter.Add("MoldInspectBasisID", ID);
                        sqlParameter.Add("MoldInspectImageFile", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectImageFile));

                        //if (Flag) //추가시
                        //{
                        //    sqlParameter.Add("MoldInspectImagePath", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectImagePath) + "/" + ID);
                        //}
                        //else //수정시
                        //{
                        //    sqlParameter.Add("MoldInspectImagePath", Lib.Instance.CheckNull(MoldRegularInsBaSub.MoldInspectImagePath));
                        //}
                        sqlParameter.Add("MoldInspectImagePath", "/ImageData/MoldReqularInspect/" + ID); //=>자꾸 문제가 되서 일단 요렇게...
                        sqlParameter.Add("MoldSeq", MoldRegularInsBaSub.MoldSeq);
                        sqlParameter.Add("UpdateUserID", MainWindow.CurrentUser);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_MoldRegularInspectBasis_uMoldRegularInspectBasisSub_FTP";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "McInspectBasisID";
                        pro1.OutputLength = "10";

                        Prolist.Add(pro1);
                        ListParameter.Add(sqlParameter);
                    }
                }

                string[] Confirm = new string[2];
                Confirm = DataStore.Instance.ExecuteAllProcedureOutputNew(Prolist, ListParameter);
                if (Confirm[0] != "success")
                {
                    MessageBox.Show("[파일경로 저장실패]\r\n" + Confirm[1].ToString());
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

        //추가
        private void btnSubAdd_Click(object sender, RoutedEventArgs e)
        {
            SubPlus();
            int count = dgdSub.Columns.IndexOf(dgdtpeMoldInspectItemName);
            dgdSub.Focus();
            dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[dgdSub.Items.Count - 1], dgdSub.Columns[count]);
        }

        private void SubPlus()
        {
            int i = dgdSub.Items.Count + 1;
            ObservableCollection<CodeView> ovcMoldCheck = ComboBoxUtil.Instance.GetCMCode_SetComboBox("MLDCHECKGBN", "");
            ObservableCollection<CodeView> ovcMoldCycle = ComboBoxUtil.Instance.GetCMCode_SetComboBox("MLDCYCLEGBN", "");
            ObservableCollection<CodeView> ovcMoldRecord = ComboBoxUtil.Instance.GetCMCode_SetComboBox("MLDRECORDGBN", "");
            ObservableCollection<CodeView> ovcMoldInspect = ComboBoxUtil.Instance.GetCMCode_SetComboBox("instype", "");

            var SubGrid = new Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView()
            {
                Num= i,
                MoldInspectBasisID="",
                MoldSeq=i.ToString(),
                MoldInspectItemName="",
                MoldInspectContent="",
                MoldInspectCheckGbn="",
                MoldInspectCheckGbnName="",
                MoldInspectCycleGbn="",
                MoldInspectCycleGbnName="",
                MoldInspectCycleDate="",
                MoldInspectRecordGbn="",
                MoldInspectRecordGbnName="",
                MoldInspectImagePath="",
                MoldInspectImageFile="",
                MoldInspectComments="",
                RowItemCount = -1,
                ovcMoldInspectCheck = ovcMoldCheck,
                ovcMoldInspectCycle = ovcMoldCycle,
                ovcMoldInspectRecord = ovcMoldRecord,
                ovcMoldInspect = ovcMoldInspect
            };
            dgdSub.Items.Add(SubGrid);
        }

        //삭제
        private void btnSubDel_Click(object sender, RoutedEventArgs e)
        {
            SubRemove();
        }

        private void SubRemove()
        {
            if (dgdSub.Items.Count > 0)
            {
                if (dgdSub.CurrentItem != null)
                {
                    RData.Add(dgdSub.CurrentItem as Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView);
                    dgdSub.Items.Remove(dgdSub.CurrentItem as Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView);
                }
                else
                {
                    RData.Add((dgdSub.Items[dgdSub.Items.Count - 1]) as Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView);
                    dgdSub.Items.Remove((dgdSub.Items[dgdSub.Items.Count - 1]) as Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView);
                }

                dgdSub.Refresh();
            }
        }

        private void DataGridCell_KeyDown(object sender, KeyEventArgs e)
        {
            MoldRegularInsBaSub = dgdSub.CurrentItem as Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView;
            int rowCount = dgdSub.Items.IndexOf(dgdSub.CurrentItem);
            int colCount = dgdSub.Columns.IndexOf(dgdSub.CurrentCell.Column);
            currentSubRowNum = rowCount;

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (dgdSub.Items.Count - 1 > rowCount && dgdSub.Columns.Count - 1 > colCount)
                {
                    dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount], dgdSub.Columns[colCount+1]);
                }
                else if (dgdSub.Items.Count - 1 > rowCount && dgdSub.Columns.Count - 1 == colCount)
                {
                    dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount + 1], dgdSub.Columns[1]);
                }
                else if (dgdSub.Items.Count - 1 == rowCount && dgdSub.Columns.Count - 1 > colCount)
                {
                    dgdSub.CurrentCell = new DataGridCellInfo(dgdSub.Items[rowCount], dgdSub.Columns[colCount+1]);
                }
                else if (dgdSub.Items.Count - 1 == rowCount && dgdSub.Columns.Count - 1 == colCount)
                {
                    btnSave.Focus();
                }
                else
                {
                    MessageBox.Show("오류 방지");
                }
            }
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
        
        //확인방법
        private void dgdtpecboMoldInspectCheck_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DataGridCell cell = Lib.Instance.GetParent<DataGridCell>(sender as ComboBox);
                cell.Focus();
            }
        }

        //주기
        private void dgdtpecboMoldInspectCycle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DataGridCell cell = Lib.Instance.GetParent<DataGridCell>(sender as ComboBox);
                cell.Focus();
            }
        }

        //검사구분
        private void dgdtpecboMoldInspect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DataGridCell cell = Lib.Instance.GetParent<DataGridCell>(sender as ComboBox);
                cell.Focus();
            }
        }
        
        //기록구분
        private void dgdtpecboovcMoldInspectRecord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DataGridCell cell = Lib.Instance.GetParent<DataGridCell>(sender as ComboBox);
                cell.Focus();
            }
        }

        //이미지 파일
        private void dgdtpetxtMoldInspectImageFile_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (lblMsg.Visibility == Visibility.Visible)
                {
                    MoldRegularInsBaSub = dgdSub.CurrentItem as Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView;

                    if (MoldRegularInsBaSub != null)
                    {
                        TextBox tb1 = sender as TextBox;

                        tb1 = Ftp_Upload_TextBox();

                        if (tb1.Text.Equals("파일사이즈초과"))
                        {
                            MessageBox.Show("이미지의 파일사이즈가 2M byte를 초과하였습니다.");
                            return;
                        }
                        else
                        {
                            if (tb1.Tag == null)
                            {
                                MessageBox.Show("선택된 파일이 없습니다.");
                            }
                            else
                            {
                                if (tb1.Text.Equals(string.Empty))
                                {
                                    MoldRegularInsBaSub.MoldInspectImageFile = "";
                                    MoldRegularInsBaSub.LocalImagePath = "";
                                    MoldRegularInsBaSub.MoldInspectImagePath = "";
                                }
                                else
                                {
                                    MoldRegularInsBaSub.MoldInspectImageFile = tb1.Text;
                                    MoldRegularInsBaSub.LocalImagePath = tb1.Tag.ToString();
                                    MoldRegularInsBaSub.MoldInspectImagePath = "/ImageData/MoldReqularInspect";
                                }
                            }
                        }

                        sender = tb1;
                    }
                }
            }
        }

        //보기
        private void btnSeeImage_Click(object sender, RoutedEventArgs e)
        {
            MoldRegularInsBaSub = dgdSub.CurrentItem as Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView;

            if (MoldRegularInsBaSub != null && !MoldRegularInsBaSub.MoldInspectImageFile.Equals(""))
            {
                FTP_DownLoadFile(MoldRegularInsBaSub.MoldInspectImagePath, MoldRegularInsBaSub.MoldInspectBasisID, MoldRegularInsBaSub.MoldInspectImageFile);
            }
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

        //FTP 업로드시 파일체크 및 경로,파일이름 표시
        private TextBox Ftp_Upload_TextBox()
        {
            TextBox tb = new TextBox();
            string[] strTemp = null;
            Microsoft.Win32.OpenFileDialog OFdlg = new Microsoft.Win32.OpenFileDialog();
            OFdlg.Filter =
                "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.pcx, *.pdf) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.pcx; *.pdf | All Files|*.*";

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
                    //MessageBox.Show("이미지의 파일사이즈가 2M byte를 초과하였습니다.");
                    sr.Close();
                    tb.Text = "파일사이즈초과";
                    //return;
                }
                else
                {
                    tb.Text = ImageFileName;
                    tb.Tag = ImageFilePath;
                }

                strTemp = new string[] { ImageFileName, ImageFilePath.ToString() };
                listFtpFile.Add(strTemp);
            }

            return tb;
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
                MessageBox.Show("파일업로드에 실패하였습니다.");
                return;
            }
        }
        
        //파일 다운로드
        private void FTP_DownLoadFile(string strMcImagePath, string FolderName, string ImageName)
        {
            try
            {
                _ftp = new FTP_EX(FTP_ADDRESS, FTP_ID, FTP_PASS);

                string[] fileListSimple;
                string[] fileListDetail;

                fileListSimple = _ftp.directoryListSimple("", Encoding.Default);

                bool ExistFile = false;

                ExistFile = FolderInfoAndFlag(fileListSimple, FolderName);

                if (ExistFile)
                {
                    ExistFile = false;
                    fileListDetail = _ftp.directoryListSimple(FolderName, Encoding.Default);

                    ExistFile = FileInfoAndFlag(fileListDetail, ImageName);

                    if (ExistFile)
                    {
                        string str_remotepath = string.Empty;
                        string str_localpath = string.Empty;

                        str_remotepath = FTP_ADDRESS + '/' + FolderName + '/' + ImageName;
                        str_localpath = LOCAL_DOWN_PATH + "\\" + ImageName;

                        DirectoryInfo DI = new DirectoryInfo(LOCAL_DOWN_PATH);
                        if (DI.Exists)
                        {
                            DI.Create();
                        }

                        FileInfo file = new FileInfo(str_localpath);
                        if (file.Exists)
                        {
                            file.Delete();
                        }

                        _ftp.download(str_remotepath.Substring(str_remotepath.Substring
                            (0, str_remotepath.LastIndexOf("/")).LastIndexOf("/")), str_localpath);

                        ProcessStartInfo proc = new ProcessStartInfo(str_localpath);
                        proc.UseShellExecute = true;
                        Process.Start(proc);
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
    }

    class Win_dvl_MoldRegularInspectBasis_U_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }
        public string MoldInspectBasisID { get; set; }
        public string MoldID { get; set; }
        public string MoldNo { get; set; }
        public string MoldInspectBasisDate { get; set; }
        public string MoldInspectContent { get; set; }
        public string Comments { get; set; }
        public string ArticleID { get; set; }
        public string Article { get; set; }

        public string MoldInspectBasisDate_CV { get; set; }
        public string MoldKindName { get; set; }
    }

    class Win_dvl_MoldRegularInspectBasis_U_Sub_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }
        public string MoldInspectBasisID { get; set; }
        public string MoldSeq { get; set; }
        public string MoldInspectItemName { get; set; }
        public string MoldInspectContent { get; set; }
        public string MoldInspectGbn { get; set; }
        public string MoldInspectGbnName { get; set; }
        public string MoldInspectCheckGbn { get; set; }
        public string MoldInspectCheckGbnName { get; set; }
        public string MoldInspectCycleGbn { get; set; }
        public string MoldInspectCycleGbnName { get; set; }
        public string MoldInspectCycleDate { get; set; }
        public string MoldInspectRecordGbn { get; set; }
        public string MoldInspectRecordGbnName { get; set; }
        public string MoldInspectImagePath { get; set; }
        public string MoldInspectImageFile { get; set; }
        public string MoldInspectComments { get; set; }

        public string MoldInspectCycleDate_CV { get; set; }
        public string LocalImagePath { get; set; }

        public ObservableCollection<CodeView> ovcMoldInspectCheck { get; set; }
        public ObservableCollection<CodeView> ovcMoldInspectCycle { get; set; }
        public ObservableCollection<CodeView> ovcMoldInspectRecord { get; set; }
        public ObservableCollection<CodeView> ovcMoldInspect { get; set; }

        public BitmapImage ImageView { get; set; }
        public bool imageFlag { get; set; }
        public int RowItemCount { get; set; }
    }
}
