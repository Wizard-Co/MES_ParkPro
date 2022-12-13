using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_ANT.PopUP;

namespace WizMes_ANT
{
    /// <summary>
    /// Win_dvl_MoldRepari_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_dvl_MoldRepair_Q : UserControl
    {
        ObservableCollection<Win_dvl_MoldRepair_Q_CodeView> ovcMoldRepairQ = 
            new ObservableCollection<Win_dvl_MoldRepair_Q_CodeView>();

        public Win_dvl_MoldRepair_Q()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            btnToday_Click(null, null);
        }

        //검색기간
        private void lblDate_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkDate.IsChecked == true) { chkDate.IsChecked = false; }
            else { chkDate.IsChecked = true; }
        }

        //검색기간
        private void chkDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;
        }

        //검색기간
        private void chkDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
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
        private void btnYesterday_Click(object sender, RoutedEventArgs e)
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

        //금형LotNo
        private void lblMoldLotNo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldLotNo.IsChecked == true) { chkMoldLotNo.IsChecked = false; }
            else { chkMoldLotNo.IsChecked = true; }
        }

        //금형LotNo
        private void chkMoldLotNo_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldLotNo.IsEnabled = true;
            btnPfMoldLotNo.IsEnabled = true;
            txtMoldLotNo.Focus();
        }

        //금형LotNo
        private void chkMoldLotNo_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldLotNo.IsEnabled = false;
            btnPfMoldLotNo.IsEnabled = false;
        }

        //금형LotNo
        private void txtMoldLotNo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtMoldLotNo, 51, "");
            }
        }

        //금형LotNo
        private void btnPfMoldLotNo_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtMoldLotNo, 51, "");
        }

        //검색
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //if (dgdRepairQ.Items.Count > 0)
            //{
            //    dgdRepairQ.Items.Clear();
            //}

            FillGrid();

            if (dgdRepairQ.Items.Count > 0)
            {
                dgdRepairQ.SelectedIndex = 0;
                this.DataContext = dgdRepairQ.SelectedItem as Win_dvl_MoldRepair_Q_CodeView;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[2];
            lst[0] = "금형 수리 조회";
            lst[1] = dgdRepairQ.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdRepairQ.Name))
                {
                    if (ExpExc.Check.Equals("Y"))
                        dt = Lib.Instance.DataGridToDTinHidden(dgdRepairQ);
                    else
                        dt = Lib.Instance.DataGirdToDataTable(dgdRepairQ);

                    Name = dgdRepairQ.Name;

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

        //실질 조회
        private void FillGrid()
        {
            dgdRepairQ.ItemsSource = null;
            ovcMoldRepairQ.Clear();

            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Add("nChkDate", chkDate.IsChecked == true ? 1 : 0);
                sqlParameter.Add("StartDate", chkDate.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EndDate", chkDate.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("nMoldID", chkMoldLotNo.IsChecked == true ? (txtMoldLotNo.Tag != null ? 1 : 2) : 0);
                sqlParameter.Add("sMoldID", chkMoldLotNo.IsChecked == true ? (txtMoldLotNo.Tag != null ? txtMoldLotNo.Tag.ToString() : txtMoldLotNo.Text) : "");

                DataSet ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldRepairQ", sqlParameter, false);

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
                            var WinRepairQ = new Win_dvl_MoldRepair_Q_CodeView()
                            {
                                Num = i,
                                RepairGubun = dr["RepairGubun"].ToString(),
                                RepairGubunName = dr["RepairGubunName"].ToString(),
                                MoldID = dr["MoldID"].ToString(),
                                MoldLotNo = dr["MoldLotNo"].ToString(),
                                repairdate = dr["repairdate"].ToString(),
                                repairremark = dr["repairremark"].ToString(),
                                McPartid = dr["McPartid"].ToString(),
                                MCPartName = dr["MCPartName"].ToString(),
                                partcnt = dr["partcnt"].ToString(),
                                partremark = dr["partremark"].ToString(),
                                RepairID = dr["RepairID"].ToString(),
                                RepairSubSeq = dr["RepairSubSeq"].ToString()
                            };

                            WinRepairQ.partcnt = Lib.Instance.returnNumStringZero(WinRepairQ.partcnt);

                            if (WinRepairQ.repairdate != null && WinRepairQ.repairdate.Length == 8)
                            {
                                WinRepairQ.repairdate_CV = Lib.Instance.StrDateTimeBar(WinRepairQ.repairdate);
                            }

                            ovcMoldRepairQ.Add(WinRepairQ);
                            //dgdRepairQ.Items.Add(WinRepairQ);
                        }

                        dgdRepairQ.ItemsSource = ovcMoldRepairQ;
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
    }

    class Win_dvl_MoldRepair_Q_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }

        public string RepairGubun { get; set; }
        public string RepairGubunName { get; set; }
        public string MoldID { get; set; }
        public string MoldLotNo { get; set; }
        public string repairdate { get; set; }

        public string repairdate_CV { get; set; }
        public string repairremark { get; set; }
        public string McPartid { get; set; }
        public string MCPartName { get; set; }
        public string partcnt { get; set; }

        public string partremark { get; set; }
        public string RepairID { get; set; }
        public string RepairSubSeq { get; set; }
    }
}
