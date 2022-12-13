using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_ANT.PopUP;

namespace WizMes_ANT
{
    /**************************************************************************************************
    '** System 명 : WizMes_ANT
    '** Author    : Wizard
    '** 작성자    : 최준호
    '** 내용      : 금형 타발수 조회(AFT 최규환 과장 요청으로 생성)
    '** 생성일자  : 2019.06.25
    '** 변경일자  : 
    '**------------------------------------------------------------------------------------------------
    ''*************************************************************************************************
    ' 변경일자  , 변경자, 요청자    , 요구사항ID  , 요청 및 작업내용
    '**************************************************************************************************
    ' ex) 2015.11.09, 박진성, 오영      ,S_201510_AFT_03 , 월별집계(가로) 순서 변경 : 합계/10월/9월/8월 순으로
    ' 2019.06.25  최준호 , 최규환     ,품명 다중선택 가능하게 해달라 => (금형현황등록에서였지만 여기도 그냥 적용)
    '**************************************************************************************************/
    /// <summary>
    /// Win_dvl_MoldHitCount_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_dvl_MoldHitCount_Q : UserControl
    {
        int rowNum = 0;
        bool MultiArticle = false;

        string ArticleSrh1 = string.Empty;
        string ArticleSrh2 = string.Empty;
        string ArticleSrh3 = string.Empty;
        string ArticleSrh4 = string.Empty;
        string ArticleSrh5 = string.Empty;

        public Win_dvl_MoldHitCount_Q()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Lib.Instance.UiLoading(this);
            btnToday_Click(null, null);
        }

        //금형발주일
        private void lblDate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkDate.IsChecked == true) { chkDate.IsChecked = false; }
            else { chkDate.IsChecked = true; }
        }

        //금형발주일
        private void chkDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;
        }

        //금형발주일
        private void chkDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }

        //금월
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = Lib.Instance.BringThisMonthDatetimeList()[1];
        }

        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        //폐기건 포함
        private void lblDisCardSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkDisCardSrh.IsChecked == true) { chkDisCardSrh.IsChecked = false; }
            else { chkDisCardSrh.IsChecked = true; }
        }

        //폐기건 포함
        private void chkDisCardSrh_Checked(object sender, RoutedEventArgs e)
        {

        }

        //폐기건 포함
        private void chkDisCardSrh_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        //타발 수 점검필요
        private void lblCheckNeedMoldSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCheckNeedMoldSrh.IsChecked == true) { chkCheckNeedMoldSrh.IsChecked = false; }
            else { chkCheckNeedMoldSrh.IsChecked = true; }
        }

        //타발 수 점검필요
        private void chkCheckNeedMoldSrh_Checked(object sender, RoutedEventArgs e)
        {

        }

        //타발 수 점검필요
        private void chkCheckNeedMoldSrh_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        //금형LotNo(%)
        private void lblMoldNoSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldNoSrh.IsChecked == true) { chkMoldNoSrh.IsChecked = false; }
            else { chkMoldNoSrh.IsChecked = true; }
        }

        //금형LotNo(%)
        private void chkMoldNoSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldNoSrh.IsEnabled = true;
        }

        //금형LotNo(%)
        private void chkMoldNoSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldNoSrh.IsEnabled = false;
        }

        //품명
        private void lblArticleSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticleSrh.IsChecked == true) { chkArticleSrh.IsChecked = false; }
            else { chkArticleSrh.IsChecked = true; }
        }

        //품명
        private void chkArticleSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = true;
            btnPfArticleSrh.IsEnabled = true;

            CheckBoxMultiArticle.IsChecked = false;
            MultiArticle = false;
            MessageBox.Show("단일 품명 조회를 선택하셨습니다. (다중 품명 조회가 해제됩니다.)");
        }

        //품명
        private void chkArticleSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtArticleSrh.IsEnabled = false;
            btnPfArticleSrh.IsEnabled = false;
        }

        //품명
        private void txtArticleSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh, 68, "");
            }
        }

        //품명
        private void btnPfArticleSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh, 68, "");
        }

        //거래처
        private void lblCustomSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCustomSrh.IsChecked == true) { chkCustomSrh.IsChecked = false; }
            else { chkCustomSrh.IsChecked = true; }
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
            {
                MainWindow.pf.ReturnCode(txtCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
            }
        }

        //거래처
        private void btnPfCustomSrh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtCustomSrh, (int)Defind_CodeFind.DCF_CUSTOM, "");
        }

        //검색
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            rowNum = 0; 
            re_Search(rowNum);
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[2];
            lst[0] = "금형타발수";
            lst[1] = dgdMain.Name;

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
                string sql = string.Empty;

                if (ArticleSrh1 != string.Empty)
                {
                    sql = "ProductionArticleID = " + ArticleSrh1 + " ";
                }

                if (ArticleSrh2 != string.Empty)
                {
                    if (sql == string.Empty)
                        sql = "ProductionArticleID = " + ArticleSrh2 + " ";
                    else
                        sql += "or ProductionArticleID = " + ArticleSrh2 + " ";
                }

                if (ArticleSrh3 != string.Empty)
                {
                    if (sql == string.Empty)
                        sql = "ProductionArticleID = " + ArticleSrh3 + " ";
                    else
                        sql += "or ProductionArticleID = " + ArticleSrh3 + " ";
                }

                if (ArticleSrh4 != string.Empty)
                {
                    if (sql == string.Empty)
                        sql = "ProductionArticleID = " + ArticleSrh4 + " ";
                    else
                        sql += "or ProductionArticleID = " + ArticleSrh4 + " ";
                }

                if (ArticleSrh5 != string.Empty)
                {
                    if (sql == string.Empty)
                        sql = "ProductionArticleID = " + ArticleSrh5 + " ";
                    else
                        sql += "or ProductionArticleID = " + ArticleSrh5 + " ";
                }

                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("chkDate", chkDate.IsChecked == true ? 1 : 0);
                sqlParameter.Add("FromDate", chkDate.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("ToDate", chkDate.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("nchkArticle", chkArticleSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("ArticleID", chkArticleSrh.IsChecked == true ?
                    (txtArticleSrh.Tag != null ? txtArticleSrh.Tag.ToString() : "") : "");

                sqlParameter.Add("nchkMold", chkMoldNoSrh.IsChecked == true ? 1 : 0);
                sqlParameter.Add("MoldNo", chkMoldNoSrh.IsChecked == true ? txtMoldNoSrh.Text : "");
                sqlParameter.Add("ndvlYN", 0);
                sqlParameter.Add("dvlYN", "");
                sqlParameter.Add("nChkCustom", chkCustomSrh.IsChecked == true ? 1 : 0);

                sqlParameter.Add("CustomID", chkCustomSrh.IsChecked == true ?
                    (txtCustomSrh.Tag != null ? txtCustomSrh.Tag.ToString() : "") : "");
                sqlParameter.Add("nChkDisCardYN", chkDisCardSrh.IsChecked == true ? "Y" : "N");
                sqlParameter.Add("nNeedCheckMold", chkCheckNeedMoldSrh.IsChecked == true ? 1 : 0);

                ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldHit", sqlParameter, false);

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
                        if (!MultiArticle)
                        {
                            DataRowCollection drc = dt.Rows;

                            foreach (DataRow dr in drc)
                            {
                                var WinMolding = new Win_dvl_Molding_U_CodeView()
                                {
                                    Num = i + 1,
                                    MoldNo = dr["MoldNo"].ToString(),
                                    Article = dr["Article"].ToString(),
                                    MoldID = dr["MoldID"].ToString(),
                                    AfterRepairHitcount = dr["AfterRepairHitcount"].ToString(),
                                    AfterWashHitcount = dr["AfterWashHitcount"].ToString(),
                                    BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                    ProductionArticleID = dr["ProductionArticleID"].ToString(),
                                    DisCardYN = dr["DisCardYN"].ToString(),
                                    dvlYN = dr["dvlYN"].ToString(),
                                    Hitcount = dr["Hitcount"].ToString(),
                                    MoldKind = dr["MoldKind"].ToString(),
                                    MoldName = dr["MoldName"].ToString(),
                                    SetCheckProdQty = dr["SetCheckProdQty"].ToString(),
                                    SetinitHitCount = dr["SetinitHitCount"].ToString(),
                                    SetInitHitCountDate = dr["SetInitHitCountDate"].ToString(),
                                    SetProdQty = dr["SetProdQty"].ToString(),
                                    SetWashingProdQty = dr["SetWashingProdQty"].ToString(),
                                    MoldKindName = dr["MoldKindName"].ToString(),
                                    PeriodHitCount = dr["PeriodHitCount"].ToString(),
                                    AfterinitHitCount = dr["AfterinitHitCount"].ToString()
                                };

                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetCheckProdQty))
                                {
                                    WinMolding.SetCheckProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetCheckProdQty);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.AfterRepairHitcount))
                                {
                                    WinMolding.AfterRepairHitcount = Lib.Instance.returnNumStringZero(WinMolding.AfterRepairHitcount);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.Hitcount))
                                {
                                    WinMolding.Hitcount = Lib.Instance.returnNumStringZero(WinMolding.Hitcount);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetProdQty))
                                {
                                    WinMolding.SetProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetProdQty);
                                }

                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetWashingProdQty))
                                {
                                    WinMolding.SetWashingProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetWashingProdQty);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetinitHitCount))
                                {
                                    WinMolding.SetinitHitCount = Lib.Instance.returnNumStringZero(WinMolding.SetinitHitCount);
                                }

                                if (Lib.Instance.IsNumOrAnother(WinMolding.PeriodHitCount))
                                {
                                    WinMolding.PeriodHitCount = Lib.Instance.returnNumStringZero(WinMolding.PeriodHitCount);
                                }
                                else
                                {
                                    WinMolding.PeriodHitCount = "0";
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.AfterinitHitCount))
                                {
                                    WinMolding.AfterinitHitCount = Lib.Instance.returnNumStringZero(WinMolding.AfterinitHitCount);
                                }
                                else
                                {
                                    WinMolding.AfterinitHitCount = "0";
                                }

                                dgdMain.Items.Add(WinMolding);
                                i++;
                            }
                        }
                        else
                        {
                            foreach (DataRow dr in dt.Select(sql))
                            {
                                var WinMolding = new Win_dvl_Molding_U_CodeView()
                                {
                                    Num = i + 1,
                                    MoldNo = dr["MoldNo"].ToString(),
                                    Article = dr["Article"].ToString(),
                                    MoldID = dr["MoldID"].ToString(),
                                    AfterRepairHitcount = dr["AfterRepairHitcount"].ToString(),
                                    AfterWashHitcount = dr["AfterWashHitcount"].ToString(),
                                    BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                    ProductionArticleID = dr["ProductionArticleID"].ToString(),
                                    DisCardYN = dr["DisCardYN"].ToString(),
                                    dvlYN = dr["dvlYN"].ToString(),
                                    Hitcount = dr["Hitcount"].ToString(),
                                    MoldKind = dr["MoldKind"].ToString(),
                                    MoldName = dr["MoldName"].ToString(),
                                    SetCheckProdQty = dr["SetCheckProdQty"].ToString(),
                                    SetinitHitCount = dr["SetinitHitCount"].ToString(),
                                    SetInitHitCountDate = dr["SetInitHitCountDate"].ToString(),
                                    SetProdQty = dr["SetProdQty"].ToString(),
                                    SetWashingProdQty = dr["SetWashingProdQty"].ToString(),
                                    MoldKindName = dr["MoldKindName"].ToString(),
                                    PeriodHitCount = dr["PeriodHitCount"].ToString(),
                                    AfterinitHitCount = dr["AfterinitHitCount"].ToString()
                                };

                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetCheckProdQty))
                                {
                                    WinMolding.SetCheckProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetCheckProdQty);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.AfterRepairHitcount))
                                {
                                    WinMolding.AfterRepairHitcount = Lib.Instance.returnNumStringZero(WinMolding.AfterRepairHitcount);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.Hitcount))
                                {
                                    WinMolding.Hitcount = Lib.Instance.returnNumStringZero(WinMolding.Hitcount);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetProdQty))
                                {
                                    WinMolding.SetProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetProdQty);
                                }

                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetWashingProdQty))
                                {
                                    WinMolding.SetWashingProdQty = Lib.Instance.returnNumStringZero(WinMolding.SetWashingProdQty);
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.SetinitHitCount))
                                {
                                    WinMolding.SetinitHitCount = Lib.Instance.returnNumStringZero(WinMolding.SetinitHitCount);
                                }
                                else
                                {
                                    WinMolding.PeriodHitCount = "0";
                                }
                                if (Lib.Instance.IsNumOrAnother(WinMolding.AfterinitHitCount))
                                {
                                    WinMolding.AfterinitHitCount = Lib.Instance.returnNumStringZero(WinMolding.AfterinitHitCount);
                                }
                                else
                                {
                                    WinMolding.AfterinitHitCount = "0";
                                }

                                dgdMain.Items.Add(WinMolding);
                                i++;
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

        private void BtnMultiArticle_Click(object sender, RoutedEventArgs e)
        {
            MultiArticle = true;

            if (popMultiArticle.IsOpen == false)
                popMultiArticle.IsOpen = true;
        }

        private void TxtArticleSrh1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh1, 68, "");
            }
        }

        private void TxtArticleSrh2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh2, 68, "");
            }
        }

        private void TxtArticleSrh3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh3, 68, "");
            }
        }

        private void TxtArticleSrh4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh4, 68, "");
            }
        }

        private void TxtArticleSrh5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticleSrh5, 68, "");
            }
        }

        private void BtnPfArticleSrh1_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh1, 68, "");
        }

        private void BtnPfArticleSrh2_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh2, 68, "");
        }

        private void BtnPfArticleSrh3_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh3, 68, "");
        }

        private void BtnPfArticleSrh4_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh4, 68, "");
        }

        private void BtnPfArticleSrh5_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticleSrh5, 68, "");
        }

        private void BtnMultiArticleOK_Click(object sender, RoutedEventArgs e)
        {
            if (txtArticleSrh1.Tag != null && !txtArticleSrh1.Text.Equals(string.Empty))
                ArticleSrh1 = txtArticleSrh1.Tag.ToString().Trim();
            else
                ArticleSrh1 = string.Empty;

            if (txtArticleSrh2.Tag != null && !txtArticleSrh2.Text.Equals(string.Empty))
                ArticleSrh2 = txtArticleSrh2.Tag.ToString().Trim();
            else
                ArticleSrh2 = string.Empty;

            if (txtArticleSrh3.Tag != null && !txtArticleSrh3.Text.Equals(string.Empty))
                ArticleSrh3 = txtArticleSrh3.Tag.ToString().Trim();
            else
                ArticleSrh3 = string.Empty;

            if (txtArticleSrh4.Tag != null && !txtArticleSrh4.Text.Equals(string.Empty))
                ArticleSrh4 = txtArticleSrh4.Tag.ToString().Trim();
            else
                ArticleSrh4 = string.Empty;

            if (txtArticleSrh5.Tag != null && !txtArticleSrh5.Text.Equals(string.Empty))
                ArticleSrh5 = txtArticleSrh5.Tag.ToString().Trim();
            else
                ArticleSrh5 = string.Empty;

            if (popMultiArticle.IsOpen == true)
                popMultiArticle.IsOpen = false;
        }

        private void BtnMultiArticleCC_Click(object sender, RoutedEventArgs e)
        {
            if ((txtArticleSrh1.Tag != null && !txtArticleSrh1.Text.Equals(string.Empty)) ||
                 (txtArticleSrh2.Tag != null && !txtArticleSrh2.Text.Equals(string.Empty)) ||
                 (txtArticleSrh3.Tag != null && !txtArticleSrh3.Text.Equals(string.Empty)) ||
                 (txtArticleSrh4.Tag != null && !txtArticleSrh4.Text.Equals(string.Empty)) ||
                 (txtArticleSrh5.Tag != null && !txtArticleSrh5.Text.Equals(string.Empty)))
                MultiArticle = true;
            else
                MultiArticle = false;

            if (popMultiArticle.IsOpen == true)
                popMultiArticle.IsOpen = false;
        }

        private void BtnMultiArticleClear_Click(object sender, RoutedEventArgs e)
        {
            MultiArticle = false;
            txtArticleSrh1.Clear();
            txtArticleSrh2.Clear();
            txtArticleSrh3.Clear();
            txtArticleSrh4.Clear();
            txtArticleSrh5.Clear();

            ArticleSrh1 = string.Empty;
            ArticleSrh2 = string.Empty;
            ArticleSrh3 = string.Empty;
            ArticleSrh4 = string.Empty;
            ArticleSrh5 = string.Empty;
        }

        private void BtnPfArticleSrh1Clear_Click(object sender, RoutedEventArgs e)
        {
            txtArticleSrh1.Clear();
        }

        private void BtnPfArticleSrh2Clear_Click(object sender, RoutedEventArgs e)
        {
            txtArticleSrh2.Clear();
        }

        private void BtnPfArticleSrh3Clear_Click(object sender, RoutedEventArgs e)
        {
            txtArticleSrh3.Clear();
        }

        private void BtnPfArticleSrh4Clear_Click(object sender, RoutedEventArgs e)
        {
            txtArticleSrh4.Clear();
        }

        private void BtnPfArticleSrh5Clear_Click(object sender, RoutedEventArgs e)
        {
            txtArticleSrh5.Clear();
        }

        /// <summary>
        /// 품명 다중 선택 체크
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxMultiArticle_Checked(object sender, RoutedEventArgs e)
        {
            btnMultiArticle.IsEnabled = true;
            btnMultiArticleClear.IsEnabled = true;

            chkArticleSrh.IsChecked = false;

            MessageBox.Show("다중 품명 선택을 선택하셨습니다. (단일 품명 조건이 초기화 됩니다.)");
        }

        /// <summary>
        /// 품명 다중 선택 체크 해제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBoxMultiArticle_UnChecked(object sender, RoutedEventArgs e)
        {
            btnMultiArticle.IsEnabled = false;
            btnMultiArticleClear.IsEnabled = false;

            //MultiArticle = false;
            //txtArticleSrh1.Clear();
            //txtArticleSrh2.Clear();
            //txtArticleSrh3.Clear();
            //txtArticleSrh4.Clear();
            //txtArticleSrh5.Clear();

            //ArticleSrh1 = string.Empty;
            //ArticleSrh2 = string.Empty;
            //ArticleSrh3 = string.Empty;
            //ArticleSrh4 = string.Empty;
            //ArticleSrh5 = string.Empty;
        }
    }
}
