using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_ANT.PopUP;

namespace WizMes_ANT
{
    /// <summary>
    /// Win_dvl_MoldTracking_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_dvl_MoldTracking_Q : UserControl
    {
        #region 변수선언 및 로드
        
        Win_dvl_MoldTracking_Q_CodeView WinMoldTracking = new Win_dvl_MoldTracking_Q_CodeView();
        string strSDate = string.Empty;
        string strEDate = string.Empty;

        public Win_dvl_MoldTracking_Q()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Lib.Instance.UiLoading(sender);
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;

            rbnRecentSixMonth.IsChecked = true;

            rbnRecentSixMonth_Click(null, null);
            //strSDate = DateTime.Now.AddMonths(-6).ToString("yyyyMMdd");
            //strEDate = DateTime.Now.ToString("yyyyMMdd");
        }

        #endregion

        #region 체크 및 날짜 클릭 이벤트

        //입출고일 라벨
        private void lblInOutDay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkInOutDay.IsChecked == true) { chkInOutDay.IsChecked = false; }
            else { chkInOutDay.IsChecked = true; }
        }

        //입출고일 체크박스
        private void chkInOutDay_Checked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;
        }

        //입출고일 체크박스
        private void chkInOutDay_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }

        //금형번호 라벨
        private void lblMoldID_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldID.IsChecked == true) { chkMoldID.IsChecked = false; }
            else { chkMoldID.IsChecked = true; }
        }

        //금형번호 체크박스
        private void chkMoldID_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldID.IsEnabled = true;
            btnPfMoldID.IsEnabled = true;
            txtMoldID.Focus();
        }

        //금형번호 체크박스
        private void chkMoldID_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldID.IsEnabled = false;
            btnPfMoldID.IsEnabled = false;
        }

        //금형번호 텍스트박스
        private void txtMoldID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtMoldID, 51, "");
            }
        }

        //금형번호 버튼
        private void btnPfMoldID_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtMoldID, 51, "");
        }

        //금형LotNo 라벨
        private void lblMoldLotNo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldLotNo.IsChecked == true) { chkMoldLotNo.IsChecked = false; }
            else { chkMoldLotNo.IsChecked = true; }
        }

        //금형LotNo 체크박스
        private void chkMoldLotNo_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldLotNo.IsEnabled = true;
            txtMoldLotNo.Focus();
        }

        //금형LotNo 체크박스
        private void chkMoldLotNo_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldLotNo.IsEnabled = false;
        }

        //품명 라벨
        private void lblArticle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticle.IsChecked == true) { chkArticle.IsChecked = false; }
            else { chkArticle.IsChecked = true; }
        }

        //품명 체크박스
        private void chkArticle_Checked(object sender, RoutedEventArgs e)
        {
            txtArticle.IsEnabled = true;
            btnPfArticle.IsEnabled = true;
            txtArticle.Focus();
        }

        //품명 체크박스
        private void chkArticle_Unchecked(object sender, RoutedEventArgs e)
        {
            txtArticle.IsEnabled = false;
            btnPfArticle.IsEnabled = false;
        }

        //품명 텍스트박스
        private void txtArticle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticle, 1, "");
            }
        }

        //품명 버튼
        private void btnPfArticle_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticle, 1, "");
        }

        #endregion

        #region 우측 상단 버튼 클릭 이벤트

        //검색
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMoldTracking.Items.Count > 0)
            {
                dgdMoldTracking.Items.Clear();
            }

            FillGrid();

            if (dgdMoldTracking.Items.Count > 0)
            {
                dgdMoldTracking.SelectedIndex = 0;
            }
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

            string[] lst = new string[8];
            lst[0] = "금형목록";
            lst[1] = "금형이력상세";
            lst[2] = "금형Lot별 이력";
            lst[3] = "평균수명";
            lst[4] = dgdMoldTracking.Name;
            lst[5] = dgdTrackingSub1.Name;
            lst[6] = dgdTrackingSub2.Name;
            lst[7] = dgdAverAge.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdMoldTracking.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdMoldTracking);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdMoldTracking);

                    Name = dgdMoldTracking.Name;

                    if (Lib.Instance.GenerateExcel(dt, Name))
                        Lib.Instance.excel.Visible = true;
                    else
                        return;
                }
                else if (ExpExc.choice.Equals(dgdTrackingSub1.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdTrackingSub1);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdTrackingSub1);
                    Name = dgdTrackingSub1.Name;

                    if (Lib.Instance.GenerateExcel(dt, Name))
                        Lib.Instance.excel.Visible = true;
                    else
                        return;
                }
                else if (ExpExc.choice.Equals(dgdTrackingSub2.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdTrackingSub2);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdTrackingSub2);
                    Name = dgdTrackingSub2.Name;

                    if (Lib.Instance.GenerateExcel(dt, Name))
                        Lib.Instance.excel.Visible = true;
                    else
                        return;
                }
                else if (ExpExc.choice.Equals(dgdAverAge.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdAverAge);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdAverAge);
                    Name = dgdAverAge.Name;

                    if (Lib.Instance.GenerateExcel(dt, Name))
                        Lib.Instance.excel.Visible = true;
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
        }

        #endregion

        #region 그리드 조회 및 이벤트

        //중단 중 좌측(메인)
        private void FillGrid()
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("chkDate", chkInOutDay.IsChecked == true ? 1 : 0);
                sqlParameter.Add("FromDate", chkInOutDay.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("ToDate", chkInOutDay.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("nchkMoldKind", chkMoldID.IsChecked == true ? (txtMoldID.Tag != null ? 1 : 2) : 0);
                sqlParameter.Add("MoldKind", chkMoldID.IsChecked == true ? (txtMoldID.Tag != null ? txtMoldID.Tag.ToString() : txtMoldID.Text) : "");
                sqlParameter.Add("nchkMold", chkMoldLotNo.IsChecked == true ? 1 : 0);
                sqlParameter.Add("MoldNo", chkMoldLotNo.IsChecked == true ? txtMoldLotNo.Text : "");
                sqlParameter.Add("nchkArticle", chkArticle.IsChecked == true ? (txtArticle.Tag != null ? 1 : 2) : 0);
                sqlParameter.Add("sArticleID", chkArticle.IsChecked == true ? (txtArticle.Tag != null ? txtArticle.Tag.ToString() : "") : "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldTrackList", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinTracking = new Win_dvl_MoldTracking_Q_CodeView()
                            {
                                MoldKind = dr["MoldKind"].ToString(),
                                MoldName = dr["MoldName"].ToString(),
                                InQty = dr["InQty"].ToString(),
                                OutQty = dr["OutQty"].ToString(),
                                RepairQty = dr["RepairQty"].ToString(),
                                MoldHitLimitCount = dr["MoldHitLimitCount"].ToString(),
                                SafyStockQty = dr["SafyStockQty"].ToString(),
                                OrderQty = dr["OrderQty"].ToString(),
                                ProdCustomName = dr["ProdCustomName"].ToString(),
                                ProdOrderDate = dr["ProdOrderDate"].ToString(),
                                ProdDueDate = dr["ProdOrderDate"].ToString(),
                                Article = dr["Article"].ToString(),
                                ArticleID = dr["ArticleID"].ToString(),
                                MoldCount = dr["MoldCount"].ToString(),
                                //Spec = dr["Spec"].ToString(),
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                MoldID = dr["MoldID"].ToString()
                            };

                            if (WinTracking.SafyStockQty != null && WinTracking.MoldCount != null)
                            {
                                if (Lib.Instance.IsNumOrAnother(WinTracking.SafyStockQty))
                                {
                                    if (Lib.Instance.IsNumOrAnother(WinTracking.MoldCount))
                                    {
                                        WinTracking.LackQty = (int.Parse(WinTracking.SafyStockQty) - int.Parse(WinTracking.MoldCount)).ToString();
                                    }
                                    else
                                    {
                                        WinTracking.LackQty = WinTracking.SafyStockQty;
                                    }
                                }
                                else
                                {
                                    WinTracking.LackQty = "0";
                                }
                            }

                            WinTracking.InQty = Lib.Instance.returnNumStringZero(WinTracking.InQty);
                            WinTracking.OutQty = Lib.Instance.returnNumStringZero(WinTracking.OutQty);
                            WinTracking.RepairQty = Lib.Instance.returnNumStringZero(WinTracking.RepairQty);
                            WinTracking.MoldHitLimitCount = Lib.Instance.returnNumStringZero(WinTracking.MoldHitLimitCount);
                            WinTracking.SafyStockQty = Lib.Instance.returnNumStringZero(WinTracking.SafyStockQty);
                            WinTracking.OrderQty = Lib.Instance.returnNumStringZero(WinTracking.OrderQty);
                            WinTracking.MoldCount = Lib.Instance.returnNumStringZero(WinTracking.MoldCount);

                            dgdMoldTracking.Items.Add(WinTracking);
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

        //메인에서 한 줄 선택 시
        private void dgdMoldTracking_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WinMoldTracking = dgdMoldTracking.SelectedItem as Win_dvl_MoldTracking_Q_CodeView;

            if (WinMoldTracking != null)
            {
                this.DataContext = WinMoldTracking;

                if (WinMoldTracking.MoldKind != null)
                {
                    FillGridSub1(WinMoldTracking.MoldKind);
                    FillGridSub2(WinMoldTracking.MoldKind, WinMoldTracking.MoldID);
                    FillGridAverAge(WinMoldTracking.MoldKind);
                }
            }
        }

        //중단 중 우측 상단 
        private void FillGridSub1(string strMoldKind)
        {
            if (dgdTrackingSub1.Items.Count > 0)
            {
                dgdTrackingSub1.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("chkDate", chkInOutDay.IsChecked == true ? 1 : 0);
                sqlParameter.Add("FromDate", chkInOutDay.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("ToDate", chkInOutDay.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("nchkMoldKind", 1);
                sqlParameter.Add("MoldKind", strMoldKind);
                sqlParameter.Add("nchkMold", 0);
                sqlParameter.Add("MoldNo", "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldTrackLotNoSum", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinMoldTrackingSub1 = new Win_dvl_MoldTracking_Q_SubView1()
                            {
                                MoldNo = dr["MoldNo"].ToString(),
                                OutQty = dr["OutQty"].ToString(),
                                StockQty = dr["StockQty"].ToString(),
                                WorkQty = dr["WorkQty"].ToString(),
                                MoldID = dr["MoldID"].ToString()
                            };

                            WinMoldTrackingSub1.OutQty = Lib.Instance.returnNumStringZero(WinMoldTrackingSub1.OutQty);
                            WinMoldTrackingSub1.StockQty = Lib.Instance.returnNumStringZero(WinMoldTrackingSub1.StockQty);
                            WinMoldTrackingSub1.WorkQty = Lib.Instance.returnNumStringZero(WinMoldTrackingSub1.WorkQty);

                            dgdTrackingSub1.Items.Add(WinMoldTrackingSub1);
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

        //중단 중 우측 하단
        private void FillGridSub2(string strMoldKind, string strMoldID)
        {
            if (dgdTrackingSub2.Items.Count > 0)
            {
                dgdTrackingSub2.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("chkDate", chkInOutDay.IsChecked == true ? 1 : 0);
                sqlParameter.Add("FromDate", chkInOutDay.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("ToDate", chkInOutDay.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("nchkMoldKind", 1);
                sqlParameter.Add("MoldKind", strMoldKind);

                sqlParameter.Add("nchkMold", 1);
                sqlParameter.Add("MoldNo", strMoldID);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldTrackLotNoDetailList", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        foreach (DataRow dr in drc)
                        {
                            var WinMoldTrackingSub2 = new Win_dvl_MoldTracking_Q_SubView2()
                            {
                                IODate = dr["IODate"].ToString(),
                                gbnName = dr["gbnName"].ToString(),
                                InQty = dr["InQty"].ToString(),
                                OutQty = dr["OutQty"].ToString(),
                                StockQty = dr["StockQty"].ToString(),
                                ProdQty = dr["ProdQty"].ToString(),
                                MachineName = dr["MachineName"].ToString(),
                                MachineNo = dr["MachineNo"].ToString(),
                                ProdArticleName = dr["ProdArticleName"].ToString(),
                                MoldID = dr["MoldID"].ToString(),
                                Process = dr["Process"].ToString(),
                                gbn = dr["gbn"].ToString()
                            };

                            WinMoldTrackingSub2.InQty = Lib.Instance.returnNumStringZero(WinMoldTrackingSub2.InQty);
                            WinMoldTrackingSub2.OutQty = Lib.Instance.returnNumStringZero(WinMoldTrackingSub2.OutQty);
                            WinMoldTrackingSub2.StockQty = Lib.Instance.returnNumStringZero(WinMoldTrackingSub2.StockQty);
                            WinMoldTrackingSub2.ProdQty = Lib.Instance.returnNumStringZero(WinMoldTrackingSub2.ProdQty);

                            dgdTrackingSub2.Items.Add(WinMoldTrackingSub2);
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

        //하단 그리드
        private void FillGridAverAge(string strMoldKind)
        {
            if (dgdAverAge.Items.Count > 0)
            {
                dgdAverAge.Items.Clear();
            }

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("sStartDate", strSDate);
                sqlParameter.Add("sEndDate", strEDate);
                sqlParameter.Add("nchkMoldKind", 1);
                sqlParameter.Add("sMoldKindID", strMoldKind);

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldTrackMontSpread", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow dr = dt.Rows[i];
                            for (int j = 0; j < 3; j++)
                            {
                                if (j == 0)
                                {
                                    dgdtxtYM1.Header = (dr["M1YYMM"] != null ? dr["M1YYMM"].ToString().Substring(0, 4) + "-" + dr["M1YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtYM2.Header = (dr["M2YYMM"] != null ? dr["M2YYMM"].ToString().Substring(0, 4) + "-" + dr["M2YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtYM3.Header = (dr["M3YYMM"] != null ? dr["M3YYMM"].ToString().Substring(0, 4) + "-" + dr["M3YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtYM4.Header = (dr["M4YYMM"] != null ? dr["M4YYMM"].ToString().Substring(0, 4) + "-" + dr["M4YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtYM5.Header = (dr["M5YYMM"] != null ? dr["M5YYMM"].ToString().Substring(0, 4) + "-" + dr["M5YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtYM6.Header = (dr["M6YYMM"] != null ? dr["M6YYMM"].ToString().Substring(0, 4) + "-" + dr["M6YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtYM7.Header = (dr["M7YYMM"] != null ? dr["M7YYMM"].ToString().Substring(0, 4) + "-" + dr["M7YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtYM8.Header = (dr["M8YYMM"] != null ? dr["M8YYMM"].ToString().Substring(0, 4) + "-" + dr["M8YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtYM9.Header = (dr["M9YYMM"] != null ? dr["M9YYMM"].ToString().Substring(0, 4) + "-" + dr["M9YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtYM10.Header = (dr["M10YYMM"] != null ? dr["M10YYMM"].ToString().Substring(0, 4) + "-" + dr["M10YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtYM11.Header = (dr["M11YYMM"] != null ? dr["M11YYMM"].ToString().Substring(0, 4) + "-" + dr["M11YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtYM12.Header = (dr["M12YYMM"] != null ? dr["M12YYMM"].ToString().Substring(0, 4) + "-" + dr["M12YYMM"].ToString().Substring(4, 2) : "");
                                    dgdtxtSum.Header = "합계";
                                }
                                if (j == 1)
                                {
                                    var WinMonth = new Win_dvl_MoldTracking_Q_Month_Value()
                                    {
                                        M1Qty = dr["M1InQty"].ToString(),
                                        M2Qty = dr["M2InQty"].ToString(),
                                        M3Qty = dr["M3InQty"].ToString(),
                                        M4Qty = dr["M4InQty"].ToString(),
                                        M5Qty = dr["M5InQty"].ToString(),
                                        M6Qty = dr["M6InQty"].ToString(),
                                        M7Qty = dr["M7InQty"].ToString(),
                                        M8Qty = dr["M8InQty"].ToString(),
                                        M9Qty = dr["M9InQty"].ToString(),
                                        M10Qty = dr["M10InQty"].ToString(),
                                        M11Qty = dr["M11InQty"].ToString(),
                                        M12Qty = dr["M12InQty"].ToString(),
                                        QTY = dr["InQty"].ToString()
                                    };

                                    WinMonth.QTY = Lib.Instance.returnNumStringZero(WinMonth.QTY);
                                    WinMonth.M1Qty = Lib.Instance.returnNumStringZero(WinMonth.M1Qty);
                                    WinMonth.M2Qty = Lib.Instance.returnNumStringZero(WinMonth.M2Qty);
                                    WinMonth.M3Qty = Lib.Instance.returnNumStringZero(WinMonth.M3Qty);
                                    WinMonth.M4Qty = Lib.Instance.returnNumStringZero(WinMonth.M4Qty);
                                    WinMonth.M5Qty = Lib.Instance.returnNumStringZero(WinMonth.M5Qty);
                                    WinMonth.M6Qty = Lib.Instance.returnNumStringZero(WinMonth.M6Qty);
                                    WinMonth.M7Qty = Lib.Instance.returnNumStringZero(WinMonth.M7Qty);
                                    WinMonth.M8Qty = Lib.Instance.returnNumStringZero(WinMonth.M8Qty);
                                    WinMonth.M9Qty = Lib.Instance.returnNumStringZero(WinMonth.M9Qty);
                                    WinMonth.M10Qty = Lib.Instance.returnNumStringZero(WinMonth.M10Qty);
                                    WinMonth.M11Qty = Lib.Instance.returnNumStringZero(WinMonth.M11Qty);
                                    WinMonth.M12Qty = Lib.Instance.returnNumStringZero(WinMonth.M12Qty);

                                    dgdAverAge.Items.Add(WinMonth);
                                }
                                if (j == 2)
                                {
                                    var WinMonth = new Win_dvl_MoldTracking_Q_Month_Value()
                                    {
                                        M1Qty = dr["M1OutQty"].ToString(),
                                        M2Qty = dr["M2OutQty"].ToString(),
                                        M3Qty = dr["M3OutQty"].ToString(),
                                        M4Qty = dr["M4OutQty"].ToString(),
                                        M5Qty = dr["M5OutQty"].ToString(),
                                        M6Qty = dr["M6OutQty"].ToString(),
                                        M7Qty = dr["M7OutQty"].ToString(),
                                        M8Qty = dr["M8OutQty"].ToString(),
                                        M9Qty = dr["M9OutQty"].ToString(),
                                        M10Qty = dr["M10OutQty"].ToString(),
                                        M11Qty = dr["M11OutQty"].ToString(),
                                        M12Qty = dr["M12OutQty"].ToString(),
                                        QTY = dr["OutQty"].ToString()
                                    };

                                    WinMonth.QTY = Lib.Instance.returnNumStringZero(WinMonth.QTY);
                                    WinMonth.M1Qty = Lib.Instance.returnNumStringZero(WinMonth.M1Qty);
                                    WinMonth.M2Qty = Lib.Instance.returnNumStringZero(WinMonth.M2Qty);
                                    WinMonth.M3Qty = Lib.Instance.returnNumStringZero(WinMonth.M3Qty);
                                    WinMonth.M4Qty = Lib.Instance.returnNumStringZero(WinMonth.M4Qty);
                                    WinMonth.M5Qty = Lib.Instance.returnNumStringZero(WinMonth.M5Qty);
                                    WinMonth.M6Qty = Lib.Instance.returnNumStringZero(WinMonth.M6Qty);
                                    WinMonth.M7Qty = Lib.Instance.returnNumStringZero(WinMonth.M7Qty);
                                    WinMonth.M8Qty = Lib.Instance.returnNumStringZero(WinMonth.M8Qty);
                                    WinMonth.M9Qty = Lib.Instance.returnNumStringZero(WinMonth.M9Qty);
                                    WinMonth.M10Qty = Lib.Instance.returnNumStringZero(WinMonth.M10Qty);
                                    WinMonth.M11Qty = Lib.Instance.returnNumStringZero(WinMonth.M11Qty);
                                    WinMonth.M12Qty = Lib.Instance.returnNumStringZero(WinMonth.M12Qty);

                                    dgdAverAge.Items.Add(WinMonth);
                                }
                            }
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

        private void dgdTrackingSub1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Win_dvl_MoldTracking_Q_SubView1 WinTrackinSub1 = dgdTrackingSub1.SelectedItem as Win_dvl_MoldTracking_Q_SubView1;

            if (WinTrackinSub1 != null)
            {
                if (WinTrackinSub1.MoldID != null)
                {
                    FillGridSub2(WinMoldTracking.MoldKind, WinTrackinSub1.MoldID);
                }
            }
        }

        #endregion

        #region  하단 라디오 버튼

        //최근 6개월
        private void rbnRecentSixMonth_Click(object sender, RoutedEventArgs e)
        {
            if (rbnRecentSixMonth.IsChecked == true)
            {
                strSDate = DateTime.Now.AddMonths(-6).ToString("yyyyMMdd");
                strEDate = DateTime.Now.ToString("yyyyMMdd");
                dgdtxtYM1.Visibility = Visibility.Collapsed;
                dgdtxtYM2.Visibility = Visibility.Collapsed;
                dgdtxtYM3.Visibility = Visibility.Collapsed;
                dgdtxtYM4.Visibility = Visibility.Collapsed;
                dgdtxtYM5.Visibility = Visibility.Collapsed;
                dgdtxtYM6.Visibility = Visibility.Collapsed;
            }
        }

        //최근 1년
        private void rbnRecentYear_Click(object sender, RoutedEventArgs e)
        {
            if (rbnRecentYear.IsChecked == true)
            {
                strSDate = DateTime.Now.AddYears(-1).ToString("yyyyMMdd");
                strEDate = DateTime.Now.ToString("yyyyMMdd");
                dgdtxtYM1.Visibility = Visibility.Visible;
                dgdtxtYM2.Visibility = Visibility.Visible;
                dgdtxtYM3.Visibility = Visibility.Visible;
                dgdtxtYM4.Visibility = Visibility.Visible;
                dgdtxtYM5.Visibility = Visibility.Visible;
                dgdtxtYM6.Visibility = Visibility.Visible;
            }
        }

        //검색조건 종료일로부터 6개월 전까지
        private void rbnEndSixMonth_Click(object sender, RoutedEventArgs e)
        {
            if (rbnEndSixMonth.IsChecked == true)
            {
                strSDate = dtpEDate.SelectedDate.Value.AddMonths(-6).ToString("yyyyMMdd");
                strEDate = dtpEDate.SelectedDate.Value.ToString("yyyyMMdd");
                dgdtxtYM1.Visibility = Visibility.Collapsed;
                dgdtxtYM2.Visibility = Visibility.Collapsed;
                dgdtxtYM3.Visibility = Visibility.Collapsed;
                dgdtxtYM4.Visibility = Visibility.Collapsed;
                dgdtxtYM5.Visibility = Visibility.Collapsed;
                dgdtxtYM6.Visibility = Visibility.Collapsed;
            }
        }

        //검색조건 종료일로부터 1년전까지
        private void rbnEndYear_Click(object sender, RoutedEventArgs e)
        {
            if (rbnEndYear.IsChecked == true)
            {
                strSDate = dtpEDate.SelectedDate.Value.AddYears(-1).ToString("yyyyMMdd");
                strEDate = dtpEDate.SelectedDate.Value.ToString("yyyyMMdd");
                dgdtxtYM1.Visibility = Visibility.Visible;
                dgdtxtYM2.Visibility = Visibility.Visible;
                dgdtxtYM3.Visibility = Visibility.Visible;
                dgdtxtYM4.Visibility = Visibility.Visible;
                dgdtxtYM5.Visibility = Visibility.Visible;
                dgdtxtYM6.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }

    class Win_dvl_MoldTracking_Q_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public string MoldKind { get; set; }
        public string MoldName { get; set; }
        public string InQty { get; set; }
        public string OutQty { get; set; }
        public string RepairQty { get; set; }
        public string MoldHitLimitCount { get; set; }
        public string SafyStockQty { get; set; }
        public string OrderQty { get; set; }
        public string LackQty { get; set; }
        public string ProdCustomName { get; set; }
        public string ProdOrderDate { get; set; }
        public string ProdDueDate { get; set; }
        public string MoldCount { get; set; }
        public string Article { get; set; }
        public string ArticleID { get; set; }
        //public string Spec { get; set; }
        public string BuyerArticleNo { get; set; }
        public string MoldID { get; set; }
    }

    class Win_dvl_MoldTracking_Q_SubView1 : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public string MoldNo { get; set; }
        public string OutQty { get; set; }
        public string StockQty { get; set; }
        public string WorkQty { get; set; }
        public string MoldID { get; set; }
    }

    class Win_dvl_MoldTracking_Q_SubView2 : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public string gbn { get; set; }
        public string MoldID { get; set; }
        public string IODate { get; set; }
        public string InQty { get; set; }
        public string OutQty { get; set; }
        public string StockQty { get; set; }
        public string ProdQty { get; set; }
        public string MachineName { get; set; }
        public string MachineNo { get; set; }
        public string ProdArticleName { get; set; }
        public string gbnName { get; set; }
        public string Process { get; set; }
    }

    class Win_dvl_MoldTracking_Q_Month : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public string AvgAge { get; set; }
        public string M1YYMM { get; set; }
        public string M2YYMM { get; set; }
        public string M3YYMM { get; set; }
        public string M4YYMM { get; set; }
        public string M5YYMM { get; set; }
        public string M6YYMM { get; set; }
        public string M7YYMM { get; set; }
        public string M8YYMM { get; set; }
        public string M9YYMM { get; set; }
        public string M10YYMM { get; set; }
        public string M11YYMM { get; set; }
        public string M12YYMM { get; set; }
        public string Sum { get; set; }
        public string M1InQty { get; set; }
        public string M2InQty { get; set; }
        public string M3InQty { get; set; }
        public string M4InQty { get; set; }
        public string M5InQty { get; set; }
        public string M6InQty { get; set; }
        public string M7InQty { get; set; }
        public string M8InQty { get; set; }
        public string M9InQty { get; set; }
        public string M10InQty { get; set; }
        public string M11InQty { get; set; }
        public string M12InQty { get; set; }
        public string INQTY { get; set; }
        public string M1OutQty { get; set; }
        public string M2OutQty { get; set; }
        public string M3OutQty { get; set; }
        public string M4OutQty { get; set; }
        public string M5OutQty { get; set; }
        public string M6OutQty { get; set; }
        public string M7OutQty { get; set; }
        public string M8OutQty { get; set; }
        public string M9OutQty { get; set; }
        public string M10OutQty { get; set; }
        public string M11OutQty { get; set; }
        public string M12OutQty { get; set; }
        public string OutQty { get; set; }
    }

    class Win_dvl_MoldTracking_Q_Month_Value : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public string M1Qty { get; set; }
        public string M2Qty { get; set; }
        public string M3Qty { get; set; }
        public string M4Qty { get; set; }
        public string M5Qty { get; set; }
        public string M6Qty { get; set; }
        public string M7Qty { get; set; }
        public string M8Qty { get; set; }
        public string M9Qty { get; set; }
        public string M10Qty { get; set; }
        public string M11Qty { get; set; }
        public string M12Qty { get; set; }
        public string QTY { get; set; }
    }
}
