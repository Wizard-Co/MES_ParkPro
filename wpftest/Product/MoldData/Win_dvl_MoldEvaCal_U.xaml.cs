using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WizMes_ANT.PopUP;
using WPF.MDI;

namespace WizMes_ANT
{
    /// <summary>
    /// Win_dvl_MoldEvaCal_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_dvl_MoldEvaCal_U : UserControl
    {
        string strFlag = string.Empty;
        int rowNum = 0;
        Win_dvl_MoldEvaCal_U_CodeView MoldEvalCal = new Win_dvl_MoldEvaCal_U_CodeView();

        ObservableCollection<Win_dvl_MoldEvaCal_U_CodeView> ovcMold = new ObservableCollection<Win_dvl_MoldEvaCal_U_CodeView>();

        DataTable thisDT = new DataTable();
        DataTable thisDTClone = new DataTable();

        int MaxScore1 = 0;
        int MaxScore2 = 0;
        int MaxScore3 = 0;
        int MaxScore4 = 0;
        int MaxScore5 = 0;

        int MinScore1 = 0;
        int MinScore2 = 0;
        int MinScore3 = 0;
        int MinScore4 = 0;
        int MinScore5 = 0;

        public Win_dvl_MoldEvaCal_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Lib.Instance.UiLoading(sender);
            btnThisMonth_Click(null, null);
            dtpEvalDate.SelectedDate = DateTime.Today;
            SetPerson();
            FillDataTable();

            chkMoldEvalDaySrh.IsChecked = true;
        }

        private void FillDataTable()
        {
            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldEvalBasis", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        thisDT = dt;
                        thisDTClone = thisDT.Clone();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
        }

        //금형평가일
        private void lblMoldEvalDaySrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldEvalDaySrh.IsChecked == true) { chkMoldEvalDaySrh.IsChecked = false; }
            else { chkMoldEvalDaySrh.IsChecked = true; }
        }

        //금형평가일
        private void chkMoldEvalDaySrh_Checked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;
            btnLastMonth.IsEnabled = true;
            btnThisMonth.IsEnabled = true;
            btnYesterDay.IsEnabled = true;
            btnToday.IsEnabled = true;
        }

        //금형평가일
        private void chkMoldEvalDaySrh_Unchecked(object sender, RoutedEventArgs e)
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

        //금형명
        private void lblMoldEvalNameSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkMoldEvalDaySrh.IsChecked == true) { chkMoldEvalDaySrh.IsChecked = false; }
            else { chkMoldEvalDaySrh.IsChecked = true; }
        }

        //금형명
        private void chkMoldEvalNameSrh_Checked(object sender, RoutedEventArgs e)
        {
            txtMoldEvalNameSrh.IsEnabled = true;
        }

        //금형명
        private void chkMoldEvalNameSrh_Unchecked(object sender, RoutedEventArgs e)
        {
            txtMoldEvalNameSrh.IsEnabled = false;
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            int delCount = 0;
            for (int i = 0; i < dgdMain.Items.Count; i++)
            {
                MoldEvalCal = dgdMain.Items[i] as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEvalCal.Flag)
                {
                    delCount++;
                }
            }

            if (delCount <= 0)
            {
                MessageBox.Show("삭제할 데이터가 지정되지 않았습니다. 삭제데이터를 지정하고 눌러주세요");
            }
            else
            {
                if (MessageBox.Show("선택하신 항목을 모두 삭제하시겠습니까?", "삭제 전 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    if (dgdMain.Items.Count > 0 && dgdMain.SelectedItem != null)
                    {
                        rowNum = dgdMain.SelectedIndex;
                    }

                    if (DeleteData())
                    {
                        FillGrid();
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
            strFlag = string.Empty;
            btnNewEval.IsEnabled = true;
            btnUpdate.IsEnabled = true;
            btnEval.IsEnabled = false;
            btnRowAdd.IsEnabled = false;
            btnRowDel.IsEnabled = false;
            chkAllCheck.IsEnabled = true;
            chkAllCheck.IsChecked = false;
            rowNum = 0;
            re_Search(rowNum);
        }

        //엑셀
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[2];
            lst[0] = "금형등급평가처리";
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
            strFlag = string.Empty;

            try
            {
                ovcMold.Clear();

                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("chkMoldID", chkMoldEvalNameSrh.IsChecked==true ? 1:0);
                sqlParameter.Add("MoldID", chkMoldEvalNameSrh.IsChecked == true ?
                    (txtMoldEvalNameSrh.Tag != null ? txtMoldEvalNameSrh.Tag.ToString() : "") : "");
                sqlParameter.Add("chkDate", chkMoldEvalDaySrh.IsChecked == true ? 1:0 );
                sqlParameter.Add("EvalStartDate", chkMoldEvalDaySrh.IsChecked == true ? 
                    dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                sqlParameter.Add("EvalEndDate", chkMoldEvalDaySrh.IsChecked == true ? 
                    dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldEval", sqlParameter, false);

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
                            var WinMoldEvalBasis = new Win_dvl_MoldEvaCal_U_CodeView()
                            {
                                Num = i + 1,
                                MoldEvalID = dr["MoldEvalID"].ToString(),
                                EvalDate = dr["EvalDate"].ToString(),
                                MoldID = dr["MoldID"].ToString(),
                                MoldNo = dr["MoldNo"].ToString(),
                                Article = dr["Article"].ToString(),

                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                AvgWorkHour = dr["AvgWorkHour"].ToString(),
                                HitCount = dr["HitCount"].ToString(),
                                QualPartEasyChangeRate = dr["QualPartEasyChangeRate"].ToString(),
                                QualOccurRate = dr["QualOccurRate"].ToString(),

                                QualAvgRepairHour = dr["QualAvgRepairHour"].ToString(),
                                AvgWorkHourScore = dr["AvgWorkHourScore"].ToString(),
                                HitCountScore = dr["HitCountScore"].ToString(),
                                QualPartEasyChangeRateScore = dr["QualPartEasyChangeRateScore"].ToString(),
                                QualOccurRateScore = dr["QualOccurRateScore"].ToString(),

                                QualAvgRepairHourScore = dr["QualAvgRepairHourScore"].ToString(),
                                Score = dr["Score"].ToString(),
                                EvalGrade = dr["EvalGrade"].ToString(),
                                EvalPersonName = dr["EvalPersonName"].ToString(),
                                Comments = dr["Comments"].ToString(),
                                OvcMold = ComboBoxUtil.Instance.Get_MOLDID(),

                                Flag = false
                            };

                            if (WinMoldEvalBasis.AvgWorkHourScore.Contains("."))
                            {
                                if (int.Parse(WinMoldEvalBasis.AvgWorkHourScore.Substring(WinMoldEvalBasis.AvgWorkHourScore.IndexOf(".") + 1)) > 0)
                                {
                                    WinMoldEvalBasis.AvgWorkHourScore = Lib.Instance.returnNumStringTwo(WinMoldEvalBasis.AvgWorkHourScore);
                                }
                                else
                                {
                                    WinMoldEvalBasis.AvgWorkHourScore = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.AvgWorkHourScore);
                                }
                            }
                            else
                            {
                                WinMoldEvalBasis.AvgWorkHourScore = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.AvgWorkHourScore);
                            }

                            if (WinMoldEvalBasis.QualPartEasyChangeRateScore.Contains("."))
                            {
                                if (int.Parse(WinMoldEvalBasis.QualPartEasyChangeRateScore.Substring(WinMoldEvalBasis.QualPartEasyChangeRateScore.IndexOf(".") + 1)) > 0)
                                {
                                    WinMoldEvalBasis.QualPartEasyChangeRateScore = Lib.Instance.returnNumStringTwo(WinMoldEvalBasis.QualPartEasyChangeRateScore);
                                }
                                else
                                {
                                    WinMoldEvalBasis.QualPartEasyChangeRateScore = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.QualPartEasyChangeRateScore);
                                }
                            }
                            else
                            {
                                WinMoldEvalBasis.QualPartEasyChangeRateScore = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.QualPartEasyChangeRateScore);
                            }

                            if (WinMoldEvalBasis.QualAvgRepairHour.Contains("."))
                            {
                                if (int.Parse(WinMoldEvalBasis.QualAvgRepairHour.Substring(WinMoldEvalBasis.QualAvgRepairHour.IndexOf(".") + 1)) > 0)
                                {
                                    WinMoldEvalBasis.QualAvgRepairHour = Lib.Instance.returnNumStringTwo(WinMoldEvalBasis.QualAvgRepairHour);
                                }
                                else
                                {
                                    WinMoldEvalBasis.QualAvgRepairHour = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.QualAvgRepairHour);
                                }
                            }
                            else
                            {
                                WinMoldEvalBasis.QualAvgRepairHour = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.QualAvgRepairHour);
                            }


                            //WinMoldEvalBasis.QualPartEasyChangeRateScore = Lib.Instance.returnNumStringTwo(WinMoldEvalBasis.QualPartEasyChangeRateScore);
                            //WinMoldEvalBasis.QualAvgRepairHour = Lib.Instance.returnNumStringTwo(WinMoldEvalBasis.QualAvgRepairHour);

                            WinMoldEvalBasis.Score = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.Score);
                            WinMoldEvalBasis.HitCount = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.HitCount);
                            WinMoldEvalBasis.QualOccurRate = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.QualOccurRate);

                            //dgdMain.Items.Add(WinMoldEvalBasis);
                            ovcMold.Add(WinMoldEvalBasis);
                            i++;
                        }

                        dgdMain.ItemsSource = ovcMold;
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

        /// <summary>
        /// 실삭제
        /// </summary>
        /// <returns></returns>
        private bool DeleteData()
        {
            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();

            try
            {
                for (int i = 0; i < dgdMain.Items.Count; i++)
                {
                    MoldEvalCal = dgdMain.Items[i] as Win_dvl_MoldEvaCal_U_CodeView;

                    if (MoldEvalCal.Flag)
                    {
                        Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                        sqlParameter.Clear();
                        sqlParameter.Add("MoldEvalID", MoldEvalCal.MoldEvalID);

                        Procedure pro1 = new Procedure();
                        pro1.Name = "xp_dvlMold_dMoldEval";
                        pro1.OutputUseYN = "N";
                        pro1.OutputName = "MoldEvalID";
                        pro1.OutputLength = "10";

                        Prolist.Add(pro1);
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

            return flag;
        }

        //
        private bool SaveData()
        {
            bool flag = false;
            List<Procedure> Prolist = new List<Procedure>();
            List<Dictionary<string, object>> ListParameter = new List<Dictionary<string, object>>();
            bool DataCheck = true;

            try
            {
                #region 추가

                if (strFlag.Equals("I"))
                {
                    for (int i = 0; i < dgdMain.Items.Count; i++)
                    {
                        var WinMoldEval = dgdMain.Items[i] as Win_dvl_MoldEvaCal_U_CodeView;
                        if (CheckData(WinMoldEval, i + 1))
                        {
                            Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldEvalID", "");
                            sqlParameter.Add("MoldID", WinMoldEval.MoldID);
                            sqlParameter.Add("EvalDate", dtpEvalDate.SelectedDate.Value.ToString("yyyyMMdd"));
                            sqlParameter.Add("AvgWorkHourScore", WinMoldEval.AvgWorkHourScore.Replace(",", ""));
                            sqlParameter.Add("QualPartEasyChangeRateScore",
                                WinMoldEval.QualPartEasyChangeRateScore == null ? "0" : WinMoldEval.QualPartEasyChangeRateScore.Replace(",", ""));

                            sqlParameter.Add("HitCout", WinMoldEval.HitCount.Replace(",", ""));
                            sqlParameter.Add("QualPartEasyChangeRate", WinMoldEval.QualPartEasyChangeRate.Replace(",", ""));
                            sqlParameter.Add("QualOccurRate", WinMoldEval.QualOccurRate.Replace(",", ""));
                            sqlParameter.Add("AvgWorkHour", WinMoldEval.AvgWorkHour.Replace(",", ""));
                            sqlParameter.Add("QualAvgRepairHour", WinMoldEval.QualAvgRepairHour.Replace(",", ""));

                            sqlParameter.Add("HitCountScore", WinMoldEval.HitCountScore == null ? "0" : WinMoldEval.HitCountScore.Replace(",", ""));
                            sqlParameter.Add("QualOccurRateScore", WinMoldEval.QualOccurRateScore.Replace(",", ""));
                            sqlParameter.Add("QualAvgRepairHourScore", WinMoldEval.QualAvgRepairHourScore.Replace(",", ""));

                            sqlParameter.Add("Comments", WinMoldEval.Comments == null ? "" : WinMoldEval.Comments);
                            sqlParameter.Add("EvalPersonName", WinMoldEval.EvalPersonName);
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro1 = new Procedure();
                            pro1.Name = "xp_dvlMold_iMoldEval";
                            pro1.OutputUseYN = "N";
                            pro1.OutputName = "MoldEvalID";
                            pro1.OutputLength = "10";

                            Prolist.Add(pro1);
                            ListParameter.Add(sqlParameter);


                            System.Diagnostics.Debug.WriteLine("확인1 : " + WinMoldEval.EvalGrade);
                            System.Diagnostics.Debug.WriteLine("확인2 : " + WinMoldEval.MoldID);
                            System.Diagnostics.Debug.WriteLine("확인3 : " + WinMoldEval.MoldInspectCycleDate);

                            //점검 주기 변경
                            Update_MoldEvalBasis_InspectCycle(WinMoldEval.EvalGrade, WinMoldEval.MoldID, WinMoldEval.MoldInspectCycleDate);

                        }
                        else
                        {
                            DataCheck = false;
                            break;
                        }
                    }

                    if (DataCheck)
                    {
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
                }

                #endregion

                #region 수정

                if (strFlag.Equals("U"))
                {
                    for (int i = 0; i < dgdMain.Items.Count; i++)
                    {
                        var WinMoldEval = dgdMain.Items[i] as Win_dvl_MoldEvaCal_U_CodeView;
                        if (CheckData(WinMoldEval, i + 1))
                        {
                            Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("MoldEvalID", WinMoldEval.MoldEvalID);
                            sqlParameter.Add("MoldID", WinMoldEval.MoldID);
                            sqlParameter.Add("EvalDate", dtpEvalDate.SelectedDate.Value.ToString("yyyyMMdd"));
                            sqlParameter.Add("AvgWorkHourScore", WinMoldEval.AvgWorkHourScore.Replace(",", ""));
                            sqlParameter.Add("QualPartEasyChangeRateScore",
                                WinMoldEval.QualPartEasyChangeRateScore == null ? "0" : WinMoldEval.QualPartEasyChangeRateScore.Replace(",", ""));

                            sqlParameter.Add("HitCout", WinMoldEval.HitCount.Replace(",", ""));
                            sqlParameter.Add("QualPartEasyChangeRate", WinMoldEval.QualPartEasyChangeRate.Replace(",", ""));
                            sqlParameter.Add("QualOccurRate", WinMoldEval.QualOccurRate.Replace(",", ""));
                            sqlParameter.Add("AvgWorkHour", WinMoldEval.AvgWorkHour.Replace(",", ""));
                            sqlParameter.Add("QualAvgRepairHour", WinMoldEval.QualAvgRepairHour.Replace(",", ""));

                            sqlParameter.Add("HitCountScore", WinMoldEval.HitCountScore == null ? "0" : WinMoldEval.HitCountScore.Replace(",", ""));
                            sqlParameter.Add("QualOccurRateScore", WinMoldEval.QualOccurRateScore.Replace(",", ""));
                            sqlParameter.Add("QualAvgRepairHourScore", WinMoldEval.QualAvgRepairHourScore.Replace(",", ""));

                            sqlParameter.Add("Comments", WinMoldEval.Comments == null ? "" : WinMoldEval.Comments);
                            sqlParameter.Add("EvalPersonName", WinMoldEval.EvalPersonName);
                            sqlParameter.Add("UpdateUserID", MainWindow.CurrentUser);

                            Procedure pro1 = new Procedure();
                            pro1.Name = "xp_dvlMold_uMoldEval";
                            pro1.OutputUseYN = "N";
                            pro1.OutputName = "MoldEvalID";
                            pro1.OutputLength = "10";

                            Prolist.Add(pro1);
                            ListParameter.Add(sqlParameter);
                        }
                        else
                        {
                            DataCheck = false;
                            break;
                        }
                    }

                    if (DataCheck)
                    {
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
                }

                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생,오류 내용 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }

            return flag;
        }

        private bool CheckData(Win_dvl_MoldEvaCal_U_CodeView MoldEvalCodeView, int Index)
        {
            bool flag = true;

            if (MoldEvalCodeView.AvgWorkHourScore.Equals(string.Empty))
            {
                MessageBox.Show("일평균 작업시간 평가가 입력되지 않았습니다. " + Index + "번째 줄");
                flag = false;
                return flag;
            }

            if (MoldEvalCodeView.HitCountScore.Equals(string.Empty))
            {
                MessageBox.Show("금형 타발수 평가가 입력되지 않았습니다. " + Index + "번째 줄");
                flag = false;
                return flag;
            }

            if (MoldEvalCodeView.QualAvgRepairHourScore.Equals(string.Empty))
            {
                MessageBox.Show("평균수리 소요시간 평가가 입력되지 않았습니다. " + Index + "번째 줄");
                flag = false;
                return flag;
            }

            if (MoldEvalCodeView.QualOccurRateScore.Equals(string.Empty))
            {
                MessageBox.Show("품질문제 발생빈도 평가가 입력되지 않았습니다. " + Index + "번째 줄");
                flag = false;
                return flag;
            }

            if (MoldEvalCodeView.QualPartEasyChangeRateScore.Equals(string.Empty))
            {
                MessageBox.Show("이상발생시 부품교환용이성 평가가 입력되지 않았습니다. " + Index + "번째 줄");
                flag = false;
                return flag;
            }

            return flag;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMain.Items.Count == 0)
            {
                MessageBox.Show("수정할 자료가 없습니다.");
            }
            else
            {
                strFlag = "U";
                chkAllCheck.IsEnabled = false;
                chkAllCheck.IsChecked = false;
                btnNewEval.IsEnabled = false;
                btnUpdate.IsEnabled = false;
                btnEval.IsEnabled = true;
                btnRowAdd.IsEnabled = true;
                btnRowDel.IsEnabled = true;
            }
        }

        private void btnNewEval_Click(object sender, RoutedEventArgs e)
        {
            strFlag = "I";
            chkAllCheck.IsEnabled = false;
            chkAllCheck.IsChecked = false;
            btnNewEval.IsEnabled = false;
            btnUpdate.IsEnabled = false;
            btnEval.IsEnabled = true;
            btnRowAdd.IsEnabled = true;
            btnRowDel.IsEnabled = true;
            FillData();
        }

        private void btnEval_Click(object sender, RoutedEventArgs e)
        {
            if (SaveData())
            {
                strFlag = string.Empty;
                FillGrid();
                btnNewEval.IsEnabled = true;
                btnUpdate.IsEnabled = true;
                btnEval.IsEnabled = false;
                btnRowAdd.IsEnabled = false;
                btnRowDel.IsEnabled = false;
                chkAllCheck.IsEnabled = true;
                chkAllCheck.IsChecked = false;
            }
        }

        private NewMoldEval GetMoldInfo(string MoldID)
        {
            NewMoldEval newMoldEval = null;

            try
            {
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("MoldID", MoldID);
                ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sNewMoldEval_byMoldID", sqlParameter, false);

                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    if (dt.Rows.Count > 0)
                    {
                        DataRow dr = dt.Rows[0];
                        newMoldEval = new NewMoldEval()
                        {
                            Article = dr["Article"].ToString(),
                            AvgRepairHour = dr["AvgRepairHour"].ToString(),
                            AvgWorkHour = dr["AvgWorkHour"].ToString(),
                            BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                            EvalDate = dr["EvalDate"].ToString(),
                            HitCount = dr["HitCount"].ToString(),
                            DefectOccur12Month = dr["DefectOccur12Month"].ToString() //2022-02-17 최근 12개월 내 불량발생 수
                        };
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

            return newMoldEval;
        }

        //행 추가
        private void BtnRowAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newMoldinfo = GetMoldInfo("00088");

                var WinMoldEval = new Win_dvl_MoldEvaCal_U_CodeView()
                {
                    Num = dgdMain.Items.Count + 1,
                    MoldID = "00088",
                    MoldNo = "AFT-M-00",
                    Article = newMoldinfo.Article,
                    HitCount = newMoldinfo.HitCount,
                    BuyerArticleNo = newMoldinfo.BuyerArticleNo,
                    EvalDate = newMoldinfo.EvalDate,
                    QualAvgRepairHour = newMoldinfo.AvgRepairHour,
                    AvgWorkHour = newMoldinfo.AvgWorkHour,
                    EvalPersonName = txtEvalPerson.Text,
                    QualOccurRate = newMoldinfo.DefectOccur12Month, //2022-02-17 최근 12개월 내 불량발생 수
                    QualPartEasyChangeRate = "0",
                    OvcMold = ComboBoxUtil.Instance.Get_MOLDID()
                };

                if (Lib.Instance.IsNumOrAnother(WinMoldEval.HitCount))
                {
                    WinMoldEval.HitCount = Lib.Instance.returnNumStringZero(WinMoldEval.HitCount);
                }

                if (Lib.Instance.IsNumOrAnother(WinMoldEval.QualAvgRepairHour))
                {
                    WinMoldEval.QualAvgRepairHour = Lib.Instance.returnNumStringTwo(WinMoldEval.QualAvgRepairHour);
                }

                if (Lib.Instance.IsNumOrAnother(WinMoldEval.AvgWorkHourScore))
                {
                    WinMoldEval.AvgWorkHour = Lib.Instance.returnNumStringZero(WinMoldEval.AvgWorkHour);
                }

                //WinMoldEval.AvgWorkHourScore = Lib.Instance.CheckNullZero(GetScore("일일 평균 작업시간", WinMoldEval.AvgWorkHour, 1));
                WinMoldEval.AvgWorkHourScore = Lib.Instance.CheckNullZero(GetScore("일일 평균 가동시간", WinMoldEval.AvgWorkHour, 1));
                //WinMoldEval.HitCountScore = Lib.Instance.CheckNullZero(GetScore("누적 타발수", WinMoldEval.HitCount, 2));
                WinMoldEval.HitCountScore = Lib.Instance.CheckNullZero(GetScore("누적 타발수/월", WinMoldEval.HitCount, 2));
                //WinMoldEval.QualPartEasyChangeRateScore = Lib.Instance.CheckNullZero(GetScore("이상발생시 부품 교환 용이성", WinMoldEval.QualPartEasyChangeRate, 3));
                WinMoldEval.QualPartEasyChangeRateScore = Lib.Instance.CheckNullZero(GetScore("이상 발생시 부품 수급 용이성", WinMoldEval.QualPartEasyChangeRate, 3));
                WinMoldEval.QualOccurRateScore = Lib.Instance.CheckNullZero(GetScore("품질 문제 발생 횟 수/월", WinMoldEval.QualOccurRate, 4)); //2022-02-17 최근 12개월 내 불량발생 수
                //WinMoldEval.QualAvgRepairHourScore = Lib.Instance.CheckNullZero(GetScore("평균 수리 소요 시간", WinMoldEval.QualAvgRepairHour, 5));
                WinMoldEval.QualAvgRepairHourScore = Lib.Instance.CheckNullZero(GetScore("수리 횟 수/월", WinMoldEval.QualAvgRepairHour, 5));

                WinMoldEval.Score = TotalScore(WinMoldEval.AvgWorkHourScore, WinMoldEval.HitCountScore,
                    WinMoldEval.QualPartEasyChangeRateScore, WinMoldEval.QualOccurRateScore,
                    WinMoldEval.QualAvgRepairHourScore);
                WinMoldEval.EvalGrade = Lib.Instance.ReturnGrade(WinMoldEval.Score);

                //dgdMain.Items.Add(WinMoldEval);
                ovcMold.Add(WinMoldEval);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        //행 삭제
        private void BtnRowDel_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMain.Items.Count > 0)
            {
                if (dgdMain.SelectedItem != null)
                {
                    ovcMold.Remove(dgdMain.SelectedItem as Win_dvl_MoldEvaCal_U_CodeView);
                }
                else
                {
                    ovcMold.Remove(dgdMain.Items[dgdMain.Items.Count - 1] as Win_dvl_MoldEvaCal_U_CodeView);
                }
            }
        }

        private void btnSEvalBasis_Click(object sender, RoutedEventArgs e)
        {
            int k = 0;
            foreach (MenuViewModel mvm in MainWindow.mMenulist)
            {
                if (mvm.Menu.Equals("★금형평가기준등록"))
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

        private void FillData()
        {
            try
            {
                ovcMold.Clear();

                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sNewMoldEval", sqlParameter, false);

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
                        for (int k = 0; k < drc.Count; k++)
                        {
                            DataRow dr = drc[k];
                            var WinNewMold = new NewMoldEval()
                            {
                                Article = dr["Article"].ToString(),
                                AvgRepairHour = dr["AvgRepairHour"].ToString(),
                                AvgWorkHour = dr["AvgWorkHour"].ToString(),                 //일일 평균 가동 시간
                                BuyerArticleNo = dr["BuyerArticleNo"].ToString(),          
                                EvalDate = dr["EvalDate"].ToString(),
                                HitCount = dr["HitCount"].ToString(),                       //누적 타발수
                                MoldID = dr["MoldID"].ToString(),
                                MoldNo = dr["MoldNo"].ToString(),
                                DefectOccur12Month = dr["DefectOccur12Month"].ToString(), //2022-02-17 최근 12개월 내 불량발생 수
                                RepairCount12Month = dr["RepairCount12Month"].ToString()  //2022-02-17 최근 12개월 내 수리횟수
                            };

                            var WinMoldEval = new Win_dvl_MoldEvaCal_U_CodeView()
                            {
                                Num = i + 1,
                                Article = WinNewMold.Article,
                                HitCount = WinNewMold.HitCount,
                                MoldID = WinNewMold.MoldID,
                                MoldNo = WinNewMold.MoldNo,
                                BuyerArticleNo = WinNewMold.BuyerArticleNo,
                                EvalDate = WinNewMold.EvalDate,
                                QualAvgRepairHour = WinNewMold.AvgRepairHour,
                                AvgWorkHour = WinNewMold.AvgWorkHour,
                                EvalPersonName = txtEvalPerson.Text,
                                QualOccurRate = WinNewMold.DefectOccur12Month, //2022-02-17 최근 12개월 내 불량발생 수
                                RepairCount12Month = WinNewMold.RepairCount12Month, //2022-02-17 최근 12개월 내 수리횟수
                                QualPartEasyChangeRate = "0",
                                OvcMold = ComboBoxUtil.Instance.Get_MOLDID()
                            };

                            if (Lib.Instance.IsNumOrAnother(WinMoldEval.HitCount))
                            {
                                WinMoldEval.HitCount = Lib.Instance.returnNumStringZero(WinMoldEval.HitCount);
                            }

                            if (Lib.Instance.IsNumOrAnother(WinMoldEval.QualAvgRepairHour))
                            {
                                WinMoldEval.QualAvgRepairHour = Lib.Instance.returnNumStringTwo(WinMoldEval.QualAvgRepairHour);
                            }

                            if (Lib.Instance.IsNumOrAnother(WinMoldEval.AvgWorkHourScore))
                            {
                                WinMoldEval.AvgWorkHour = Lib.Instance.returnNumStringZero(WinMoldEval.AvgWorkHour);
                            }

                            //WinMoldEval.AvgWorkHourScore = Lib.Instance.CheckNullZero(GetScore("일일 평균 작업시간", WinMoldEval.AvgWorkHour, 1));
                            WinMoldEval.AvgWorkHourScore = Lib.Instance.CheckNullZero(GetScore("일일 평균 가동시간", WinMoldEval.AvgWorkHour, 1));
                            //WinMoldEval.HitCountScore = Lib.Instance.CheckNullZero(GetScore("누적 타발수", WinMoldEval.HitCount, 2));
                            WinMoldEval.HitCountScore = Lib.Instance.CheckNullZero(GetScore("누적 타발수/월", WinMoldEval.HitCount, 2));
                            //WinMoldEval.QualPartEasyChangeRateScore = Lib.Instance.CheckNullZero(GetScore("이상발생시 부품 교환 용이성", WinMoldEval.QualPartEasyChangeRate, 3));
                            WinMoldEval.QualPartEasyChangeRateScore = Lib.Instance.CheckNullZero(GetScore("이상 발생시 부품 수급 용이성", WinMoldEval.QualPartEasyChangeRate, 3));                            
                            WinMoldEval.QualOccurRateScore = Lib.Instance.CheckNullZero(GetScore("품질 문제 발생 횟 수/월", WinMoldEval.QualOccurRate, 4));   //2022-02-17 최근 12개월 내 불량발생 수                     
                            //WinMoldEval.QualAvgRepairHourScore = Lib.Instance.CheckNullZero(GetScore("평균 수리 소요 시간", WinMoldEval.QualAvgRepairHour, 5));
                            WinMoldEval.QualAvgRepairHourScore = Lib.Instance.CheckNullZero(GetScore("수리 횟 수/월", WinMoldEval.RepairCount12Month, 5)); //수리횟수

                            WinMoldEval.Score = TotalScore(WinMoldEval.AvgWorkHourScore, WinMoldEval.HitCountScore, 
                                WinMoldEval.QualPartEasyChangeRateScore, WinMoldEval.QualOccurRateScore, 
                                WinMoldEval.QualAvgRepairHourScore);
                            WinMoldEval.EvalGrade = Lib.Instance.ReturnGrade(WinMoldEval.Score);

                            //dgdMain.Items.Add(WinMoldEval);
                            ovcMold.Add(WinMoldEval);
                            i++;
                        }

                        dgdMain.ItemsSource = ovcMold;
                    }

                    int colCount = dgdMain.Columns.IndexOf(dgdtpeAvgWorkHour);
                    dgdMain.Focus();
                    dgdMain.CurrentCell = new DataGridCellInfo
                        (dgdMain.Items[0], dgdMain.Columns[colCount]);
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

        private string TotalScore(string AvgWorkHourScore, string HitCountScore, string QualPartEasyChangeRateScore
            , string QualOccurRateScore, string QualAvgRepairHourScore)
        {
            string strTotalScore = string.Empty;
            if (AvgWorkHourScore.Equals(string.Empty))
            {
                AvgWorkHourScore = "0";
            }
            if (HitCountScore.Equals(string.Empty))
            {
                HitCountScore = "0";
            }
            if (QualPartEasyChangeRateScore.Equals(string.Empty))
            {
                QualPartEasyChangeRateScore = "0";
            }
            if (QualOccurRateScore.Equals(string.Empty))
            {
                QualOccurRateScore = "0";
            }
            if (QualAvgRepairHourScore.Equals(string.Empty))
            {
                QualAvgRepairHourScore = "0";
            }

            strTotalScore = (int.Parse(AvgWorkHourScore) + int.Parse(HitCountScore)
                                + int.Parse(QualPartEasyChangeRateScore) + int.Parse(QualOccurRateScore)
                                + int.Parse(QualAvgRepairHourScore)).ToString();

            return strTotalScore;
        }

        private void SetPerson()
        {
            string sql = "SELECT PersonID , Name    from  mt_Person    ";           
            sql += "   WHERE 1          = 1                            ";
            sql += "   AND UserID     =   '" + MainWindow.CurrentUser + "'    ";

            try
            {
                DataSet ds = DataStore.Instance.QueryToDataSet(sql);
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt.Rows.Count > 0)
                    {
                        DataRowCollection drc = dt.Rows;

                        txtEvalPerson.Text = drc[0]["Name"].ToString();
                        txtEvalPerson.Tag = drc[0]["PersonID"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생 : " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        private void DataGridMainCell_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.Left || e.Key == Key.Right)
            {
                DataGridMainCell_KeyDown(sender, e);
            }
        }

        private void DataGridMainCell_KeyDown(object sender, KeyEventArgs e)
        {
            MoldEvalCal = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;
            int rowCount = dgdMain.Items.IndexOf(dgdMain.CurrentItem);
            int colCount = dgdMain.Columns.IndexOf(dgdMain.CurrentCell.Column);
            int lastColcount = dgdMain.Columns.IndexOf(dgdtpeComments);
            int startColcount = dgdMain.Columns.IndexOf(dgdtpeAvgWorkHour);

            //MessageBox.Show(e.Key.ToString());

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (lastColcount == colCount && dgdMain.Items.Count - 1 > rowCount)
                {
                    dgdMain.SelectedIndex = rowCount + 1;
                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[rowCount + 1], dgdMain.Columns[startColcount]);
                }
                else if (lastColcount > colCount && dgdMain.Items.Count - 1 > rowCount)
                {
                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[rowCount], dgdMain.Columns[colCount + 1]);
                }
                else if (lastColcount == colCount && dgdMain.Items.Count - 1 == rowCount)
                {
                    btnEval.Focus();
                }
                else if (lastColcount > colCount && dgdMain.Items.Count - 1 == rowCount)
                {
                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[rowCount], dgdMain.Columns[colCount + 1]);
                }
                else
                {
                    MessageBox.Show("있으면 찾아보자...");
                }
            }
            else if (e.Key == Key.Down)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (dgdMain.Items.Count - 1 > rowCount)
                {
                    dgdMain.SelectedIndex = rowCount + 1;
                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[rowCount + 1], dgdMain.Columns[colCount]);
                }
                else if (dgdMain.Items.Count - 1 == rowCount)
                {
                    if (lastColcount > colCount)
                    {
                        dgdMain.SelectedIndex = 0;
                        dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[0], dgdMain.Columns[colCount + 1]);
                    }
                    else
                    {
                        btnEval.Focus();
                    }
                }
            }
            else if (e.Key == Key.Up)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (rowCount > 0)
                {
                    dgdMain.SelectedIndex = rowCount - 1;
                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[rowCount - 1], dgdMain.Columns[colCount]);
                }
            }
            else if (e.Key == Key.Left)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (colCount > 0)
                {
                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[rowCount], dgdMain.Columns[colCount - 1]);
                }
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (lastColcount > colCount)
                {
                    dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[rowCount], dgdMain.Columns[colCount + 1]);
                }
                else if (lastColcount == colCount)
                {
                    if (dgdMain.Items.Count - 1 > rowCount)
                    {
                        dgdMain.SelectedIndex = rowCount + 1;
                        dgdMain.CurrentCell = new DataGridCellInfo(dgdMain.Items[rowCount + 1], dgdMain.Columns[startColcount]);
                    }
                    else
                    {
                        btnEval.Focus();
                    }
                }
            }
        }

        //
        private void TextBoxFocusInDataGrid(object sender, KeyEventArgs e)
        {
            Lib.Instance.DataGridINControlFocus(sender, e);
        }

        //
        private void TextBoxFocusInDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Lib.Instance.DataGridINBothByMouseUP(sender, e);
        }

        //
        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                DataGridCell cell = sender as DataGridCell;
                cell.IsEditing = true;
            }
        }

        //
        private string GetScore(string strName, string strValue, int number)
        {
            string retunString = string.Empty;
            string sql = string.Empty;
            if (thisDTClone.Rows.Count > 0)
            {
                thisDTClone.Rows.Clear();
            }

            if ((strFlag.Equals("I") || strFlag.Equals("U")) && strName.Equals("일일 평균 가동시간")) //일일 평균 작업시간
            {
                string ColName = string.Empty;
                ColName = thisDT.Columns[2].Caption;

                if (ColName != null && !ColName.Equals(string.Empty))
                {
                    sql = ColName + " = '" + strName + "' ";

                    foreach (DataRow dr in thisDT.Select(sql))
                    {
                        thisDTClone.Rows.Add(dr.ItemArray);

                        if (Lib.Instance.IsNumOrAnother(strValue) &&  Lib.Instance.IsNumOrAnother(dr["EvalSpecMin"].ToString())
                            && Lib.Instance.IsNumOrAnother(dr["EvalSpecMax"].ToString()))
                        {
                            if ((int)(double.Parse(strValue.Replace(",", ""))) >= (int)(double.Parse(dr["EvalSpecMin"].ToString())) &&
                               (int)(double.Parse(strValue.Replace(",", ""))) <= (int)(double.Parse(dr["EvalSpecMax"].ToString())))
                            {
                                retunString = dr["EvalScore"].ToString();
                                break;
                            }
                        }
                        else
                        {
                            if (strValue.Equals(dr["EvalSpecMin"].ToString()))
                            {
                                retunString = dr["EvalScore"].ToString();
                                break;
                            }
                        }
                    }

                    foreach (DataRow dr in thisDTClone.Rows)
                    {
                        int Level = dr.Field<int>("EvalScore");
                        if (number == 1)
                        {
                            MaxScore1 = Math.Max(MaxScore1, Level);
                            MinScore1 = Math.Min(MinScore1, Level);
                        }
                        else if (number == 2)
                        {
                            MaxScore2 = Math.Max(MaxScore2, Level);
                            MinScore2 = Math.Min(MinScore2, Level);
                        }
                        else if (number == 3)
                        {
                            MaxScore3 = Math.Max(MaxScore3, Level);
                            MinScore3 = Math.Min(MinScore3, Level);
                        }
                        else if (number == 4)
                        {
                            MaxScore4 = Math.Max(MaxScore4, Level);
                            MinScore4 = Math.Min(MinScore4, Level);
                        }
                        else if (number == 5)
                        {
                            MaxScore5 = Math.Max(MaxScore5, Level);
                            MinScore5 = Math.Min(MinScore5, Level);
                        }
                    }
                }
            }
            else if ((strFlag.Equals("I") || strFlag.Equals("U")) && Lib.Instance.IsNumOrAnother(strValue.Replace(",", "")))
            {
                string ColName = string.Empty;
                ColName = thisDT.Columns[2].Caption;

                if (ColName != null && !ColName.Equals(string.Empty))
                {
                    sql = ColName + " = '" + strName + "' ";

                    foreach (DataRow dr in thisDT.Select(sql))
                    {
                        thisDTClone.Rows.Add(dr.ItemArray);
                        if ((int)(double.Parse(strValue.Replace(",", ""))) >= (int)(double.Parse(dr["EvalSpecMin"].ToString())) &&
                            (int)(double.Parse(strValue.Replace(",", ""))) <= (int)(double.Parse(dr["EvalSpecMax"].ToString())))
                        {
                            retunString = dr["EvalScore"].ToString();
                            break;
                        }
                    }

                    foreach (DataRow dr in thisDTClone.Rows)
                    {
                        int Level = dr.Field<int>("EvalScore");
                        if (number == 1)
                        {
                            MaxScore1 = Math.Max(MaxScore1, Level);
                            MinScore1 = Math.Min(MinScore1, Level);
                        }
                        else if (number == 2)
                        {
                            MaxScore2 = Math.Max(MaxScore2, Level);
                            MinScore2 = Math.Min(MinScore2, Level);
                        }
                        else if (number == 3)
                        {
                            MaxScore3 = Math.Max(MaxScore3, Level);
                            MinScore3 = Math.Min(MinScore3, Level);
                        }
                        else if (number == 4)
                        {
                            MaxScore4 = Math.Max(MaxScore4, Level);
                            MinScore4 = Math.Min(MinScore4, Level);
                        }
                        else if (number == 5)
                        {
                            MaxScore5 = Math.Max(MaxScore5, Level);
                            MinScore5 = Math.Min(MinScore5, Level);
                        }
                    }
                }
            }

            return retunString;
        }

        //
        private void MaxAndMinLimit(string strValue, int number)
        {
            if (strValue.Equals(string.Empty))
            {
                return;
            }

            if (number == 1)
            {
                if (int.Parse(strValue) > MaxScore1)
                {
                    MessageBox.Show("해당 평가의 최고점수는 " + MaxScore1 + " 입니다.");
                    return;
                }
                if (int.Parse(strValue) < MinScore1)
                {
                    MessageBox.Show("해당 평가의 최저점수는 " + MinScore1 + " 입니다.");
                    return;
                }
            }
            else if (number == 2)
            {
                if (int.Parse(strValue) > MaxScore2)
                {
                    MessageBox.Show("해당 평가의 최고점수는 " + MaxScore2 + " 입니다.");
                    return;
                }
                if (int.Parse(strValue) < MinScore2)
                {
                    MessageBox.Show("해당 평가의 최저점수는 " + MinScore2 + " 입니다.");
                    return;
                }
            }
            else if (number == 3)
            {
                if (int.Parse(strValue) > MaxScore3)
                {
                    MessageBox.Show("해당 평가의 최고점수는 " + MaxScore3 + " 입니다.");
                    return;
                }
                if (int.Parse(strValue) < MinScore3)
                {
                    MessageBox.Show("해당 평가의 최저점수는 " + MinScore3 + " 입니다.");
                    return;
                }
            }
            else if (number == 4)
            {
                if (int.Parse(strValue) > MaxScore4)
                {
                    MessageBox.Show("해당 평가의 최고점수는 " + MaxScore4 + " 입니다.");
                    return;
                }
                if (int.Parse(strValue) < MinScore4)
                {
                    MessageBox.Show("해당 평가의 최저점수는 " + MinScore4 + " 입니다.");
                    return;
                }
            }
            else if (number == 5)
            {
                if (int.Parse(strValue) > MaxScore5)
                {
                    MessageBox.Show("해당 평가의 최고점수는 " + MaxScore5 + " 입니다.");
                    return;
                }
                if (int.Parse(strValue) < MinScore5)
                {
                    MessageBox.Show("해당 평가의 최저점수는 " + MinScore5 + " 입니다.");
                    return;
                }
            }
        }

        //금형번호 선택변경
        private void cboMold_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (strFlag.Equals("I") || strFlag.Equals("U"))
                {
                    var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                    if (MoldEval != null)
                    {
                        ComboBox comboBox = sender as ComboBox;

                        if (comboBox.SelectedValue != null && !comboBox.SelectedValue.ToString().Equals(string.Empty))
                        {
                            var moldInfoUpdate = GetMoldInfo(comboBox.SelectedValue.ToString());

                            MoldEval.Article = moldInfoUpdate.Article;
                            MoldEval.HitCount = moldInfoUpdate.HitCount;
                            MoldEval.BuyerArticleNo = moldInfoUpdate.BuyerArticleNo;
                            MoldEval.EvalDate = moldInfoUpdate.EvalDate;
                            MoldEval.QualAvgRepairHour = moldInfoUpdate.AvgRepairHour;
                            MoldEval.AvgWorkHour = moldInfoUpdate.AvgWorkHour;
                            MoldEval.QualOccurRate = moldInfoUpdate.DefectOccur12Month; //2022-02-17 최근 12개월 내 불량발생 수

                            if (Lib.Instance.IsNumOrAnother(MoldEval.HitCount))
                            {
                                MoldEval.HitCount = Lib.Instance.returnNumStringZero(MoldEval.HitCount);
                            }

                            if (Lib.Instance.IsNumOrAnother(MoldEval.QualAvgRepairHour))
                            {
                                MoldEval.QualAvgRepairHour = Lib.Instance.returnNumStringTwo(MoldEval.QualAvgRepairHour);
                            }

                            if (Lib.Instance.IsNumOrAnother(MoldEval.AvgWorkHourScore))
                            {
                                MoldEval.AvgWorkHour = Lib.Instance.returnNumStringZero(MoldEval.AvgWorkHour);
                            }

                            //MoldEval.AvgWorkHourScore = Lib.Instance.CheckNullZero(GetScore("일일 평균 작업시간", MoldEval.AvgWorkHour, 1));
                            //MoldEval.HitCountScore = Lib.Instance.CheckNullZero(GetScore("누적 타발수", MoldEval.HitCount, 2));
                            //MoldEval.QualPartEasyChangeRateScore = Lib.Instance.CheckNullZero(GetScore("이상발생시 부품 교환 용이성", MoldEval.QualPartEasyChangeRate, 3));
                            ////MoldEval.QualOccurRateScore = Lib.Instance.CheckNullZero(GetScore("품질 문제 발생 빈도", MoldEval.QualOccurRate, 4)); //2022-02-17 
                            //MoldEval.QualOccurRateScore = MoldEval.QualOccurRate; //2022-02-17 최근 12개월 내 불량발생 수
                            //MoldEval.QualAvgRepairHourScore = Lib.Instance.CheckNullZero(GetScore("평균 수리 소요 시간", MoldEval.QualAvgRepairHour, 5));

                            //WinMoldEval.AvgWorkHourScore = Lib.Instance.CheckNullZero(GetScore("일일 평균 작업시간", WinMoldEval.AvgWorkHour, 1));
                            MoldEval.AvgWorkHourScore = Lib.Instance.CheckNullZero(GetScore("일일 평균 가동시간", MoldEval.AvgWorkHour, 1));
                            //WinMoldEval.HitCountScore = Lib.Instance.CheckNullZero(GetScore("누적 타발수", WinMoldEval.HitCount, 2));
                            MoldEval.HitCountScore = Lib.Instance.CheckNullZero(GetScore("누적 타발수/월", MoldEval.HitCount, 2));
                            //WinMoldEval.QualPartEasyChangeRateScore = Lib.Instance.CheckNullZero(GetScore("이상발생시 부품 교환 용이성", WinMoldEval.QualPartEasyChangeRate, 3));
                            MoldEval.QualPartEasyChangeRateScore = Lib.Instance.CheckNullZero(GetScore("이상 발생시 부품 수급 용이성", MoldEval.QualPartEasyChangeRate, 3));
                            MoldEval.QualOccurRateScore = Lib.Instance.CheckNullZero(GetScore("품질 문제 발생 횟 수/월", MoldEval.QualOccurRate, 4)); //2022-02-17 최근 12개월 내 불량발생 수
                            //WinMoldEval.QualAvgRepairHourScore = Lib.Instance.CheckNullZero(GetScore("평균 수리 소요 시간", WinMoldEval.QualAvgRepairHour, 5));
                            MoldEval.QualAvgRepairHourScore = Lib.Instance.CheckNullZero(GetScore("수리 횟 수/월", MoldEval.QualAvgRepairHour, 5));



                            MoldEval.Score = TotalScore(MoldEval.AvgWorkHourScore, MoldEval.HitCountScore,
                                MoldEval.QualPartEasyChangeRateScore, MoldEval.QualOccurRateScore,
                                MoldEval.QualAvgRepairHourScore);
                            MoldEval.EvalGrade = Lib.Instance.ReturnGrade(MoldEval.Score);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
            }
        }

        //일평균 작업시간
        private void AvgWorkHour_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEval != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MoldEval.AvgWorkHour = Lib.Instance.returnNumStringZero(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    MoldEval.AvgWorkHourScore =
                       GetScore("일일 평균 가동시간", MoldEval.AvgWorkHour, 1);
                    MoldEval.Score = TotalScore(MoldEval.AvgWorkHourScore, MoldEval.HitCountScore,
                                MoldEval.QualPartEasyChangeRateScore, MoldEval.QualOccurRateScore,
                                MoldEval.QualAvgRepairHourScore);
                    MoldEval.EvalGrade = Lib.Instance.ReturnGrade(MoldEval.Score);
                    sender = tb1;
                }
            }
        }

        //일평균 작업시간 평가
        private void AvgWorkHourScore_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEval != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MoldEval.AvgWorkHourScore = Lib.Instance.returnNumStringZero(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    //MaxAndMinLimit(tb1.Text.Replace(",", ""), 1);
                    MoldEval.Score = TotalScore(MoldEval.AvgWorkHourScore, MoldEval.HitCountScore,
                                MoldEval.QualPartEasyChangeRateScore, MoldEval.QualOccurRateScore,
                                MoldEval.QualAvgRepairHourScore);
                    MoldEval.EvalGrade = Lib.Instance.ReturnGrade(MoldEval.Score);
                    sender = tb1;
                }
            }
        }

        //누적 타발수
        private void dgdtpetxtHitCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEval != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MoldEval.HitCount = Lib.Instance.returnNumStringTwoExceptDot(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    MoldEval.HitCountScore =
                        GetScore("누적 타발수/월", MoldEval.HitCount, 2);
                    MoldEval.Score = TotalScore(MoldEval.AvgWorkHourScore, MoldEval.HitCountScore,
                                MoldEval.QualPartEasyChangeRateScore, MoldEval.QualOccurRateScore,
                                MoldEval.QualAvgRepairHourScore);
                    MoldEval.EvalGrade = Lib.Instance.ReturnGrade(MoldEval.Score);
                    sender = tb1;
                }
            }
        }

        //누적 타발수 평가
        private void dgdtpetxtHitCountScore_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEval != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MoldEval.HitCountScore = Lib.Instance.returnNumStringZero(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    //MaxAndMinLimit(tb1.Text.Replace(",", ""), 2);
                    MoldEval.Score = TotalScore(MoldEval.AvgWorkHourScore, MoldEval.HitCountScore,
                                MoldEval.QualPartEasyChangeRateScore, MoldEval.QualOccurRateScore,
                                MoldEval.QualAvgRepairHourScore);
                    MoldEval.EvalGrade = Lib.Instance.ReturnGrade(MoldEval.Score);
                    sender = tb1;
                }
            }
        }

        //이상발생시 부품교환 용이성
        private void QualPartEasyChangeRate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEval != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MoldEval.QualPartEasyChangeRate = Lib.Instance.returnNumStringTwoExceptDot(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    MoldEval.QualPartEasyChangeRateScore =
                        Lib.Instance.CheckNullZero(GetScore("이상 발생시 부품 수급 용이성", MoldEval.QualPartEasyChangeRate, 3));
                    MoldEval.Score = TotalScore(MoldEval.AvgWorkHourScore, MoldEval.HitCountScore,
                                MoldEval.QualPartEasyChangeRateScore, MoldEval.QualOccurRateScore,
                                MoldEval.QualAvgRepairHourScore);
                    MoldEval.EvalGrade = Lib.Instance.ReturnGrade(MoldEval.Score);
                    sender = tb1;
                }
            }
        }

        //이상발생시 부품교환 용이성 평가
        private void QualPartEasyChangeRateScore_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEval != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MoldEval.QualPartEasyChangeRateScore = Lib.Instance.returnNumStringTwoExceptDot(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    //MaxAndMinLimit(tb1.Text.Replace(",", ""), 3);
                    MoldEval.Score = TotalScore(MoldEval.AvgWorkHourScore, MoldEval.HitCountScore,
                                MoldEval.QualPartEasyChangeRateScore, MoldEval.QualOccurRateScore,
                                MoldEval.QualAvgRepairHourScore);
                    MoldEval.EvalGrade = Lib.Instance.ReturnGrade(MoldEval.Score);
                    sender = tb1;
                }
            }
        }

        //품질문제 발생빈도
        private void dgdtpetxtQualOccurRate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEval != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MoldEval.QualOccurRate = Lib.Instance.returnNumStringTwoExceptDot(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    MoldEval.QualOccurRateScore =
                        GetScore("품질 문제 발생 횟 수/월", MoldEval.QualOccurRate, 4);
                    MoldEval.Score = TotalScore(MoldEval.AvgWorkHourScore, MoldEval.HitCountScore,
                                MoldEval.QualPartEasyChangeRateScore, MoldEval.QualOccurRateScore,
                                MoldEval.QualAvgRepairHourScore);
                    MoldEval.EvalGrade = Lib.Instance.ReturnGrade(MoldEval.Score);
                    sender = tb1;
                }
            }
        }

        //품질문제 발생빈도 평가
        private void dgdtpetxtQualOccurRateScore_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEval != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MoldEval.QualOccurRateScore = Lib.Instance.returnNumStringTwoExceptDot(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    //MaxAndMinLimit(tb1.Text.Replace(",", ""), 4);
                    MoldEval.Score = TotalScore(MoldEval.AvgWorkHourScore, MoldEval.HitCountScore,
                                MoldEval.QualPartEasyChangeRateScore, MoldEval.QualOccurRateScore,
                                MoldEval.QualAvgRepairHourScore);
                    MoldEval.EvalGrade = Lib.Instance.ReturnGrade(MoldEval.Score);
                    sender = tb1;
                }
            }
        }

        //평균수리 소요시간
        private void dgdtpetxtQualAvgRepairHour_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEval != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MoldEval.QualAvgRepairHour = Lib.Instance.returnNumStringTwoExceptDot(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    MoldEval.QualAvgRepairHourScore =
                        GetScore("수리 횟 수/월", MoldEval.QualAvgRepairHour, 5);
                    MoldEval.Score = TotalScore(MoldEval.AvgWorkHourScore, MoldEval.HitCountScore,
                                MoldEval.QualPartEasyChangeRateScore, MoldEval.QualOccurRateScore,
                                MoldEval.QualAvgRepairHourScore);
                    MoldEval.EvalGrade = Lib.Instance.ReturnGrade(MoldEval.Score);
                    sender = tb1;
                }
            }
        }

        //평균수리 소요시간 평가
        private void dgdtpetxtQualAvgRepairHourScore_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEval != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MoldEval.QualAvgRepairHourScore = Lib.Instance.returnNumStringTwoExceptDot(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    //MaxAndMinLimit(tb1.Text.Replace(",", ""), 5);
                    MoldEval.Score = TotalScore(MoldEval.AvgWorkHourScore, MoldEval.HitCountScore,
                                MoldEval.QualPartEasyChangeRateScore, MoldEval.QualOccurRateScore,
                                MoldEval.QualAvgRepairHourScore);
                    MoldEval.EvalGrade = Lib.Instance.ReturnGrade(MoldEval.Score);
                    sender = tb1;
                }
            }
        }

        //비고
        private void dgdtpetxtComments_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (strFlag.Equals("I") || strFlag.Equals("U"))
            {
                var MoldEval = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if (MoldEval != null)
                {
                    TextBox tb1 = sender as TextBox;
                    MoldEval.Comments = tb1.Text;
                    sender = tb1;
                }
            }
        }

        //
        private void NumericText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Lib.Instance.CheckIsNumeric((TextBox)sender, e);
        }

        //
        private void btnGoBasis_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChkAllCheck_Checked(object sender, RoutedEventArgs e)
        {
            if (dgdMain.Items.Count > 0)
            {
                for (int i = 0; i < dgdMain.Items.Count; i++)
                {
                    var Check = dgdMain.Items[i] as Win_dvl_MoldEvaCal_U_CodeView;
                    if (Check != null)
                    {
                        Check.Flag = true;
                    }
                }
            }
        }

        private void ChkAllCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgdMain.Items.Count > 0)
            {
                for (int i = 0; i < dgdMain.Items.Count; i++)
                {
                    var Check = dgdMain.Items[i] as Win_dvl_MoldEvaCal_U_CodeView;
                    if (Check != null)
                    {
                        Check.Flag = false;
                    }
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            MoldEvalCal = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

            if (MoldEvalCal != null)
            {
                if(MoldEvalCal.Flag)
                    MoldEvalCal.Flag = false;
                else
                    MoldEvalCal.Flag = true;
            }
        }

        #region 금형평가 경과 등급에 따른 점검 주기 변경
        private void Update_MoldEvalBasis_InspectCycle(string strEvalGrade, string strMoldID, string strMoldInspectCycleDate)
        {
            try
            {
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                sqlParameter.Add("EvalGrade", strEvalGrade);
                sqlParameter.Add("MoldID", strMoldID);
                sqlParameter.Add("MoldInsCycleDate", strMoldInspectCycleDate);
                sqlParameter.Add("UpdateUserID", MainWindow.CurrentUser);

                string[] result = DataStore.Instance.ExecuteProcedure("xp_dvlMold_uMoldEval_InspectCycle", sqlParameter, false);
                DataStore.Instance.CloseConnection();

                if(result[0].Equals("success"))
                {
                    MessageBox.Show("금형평가 등급에 의해 점검 주기가 변경되었습니다.");
                }

            }
            catch(Exception ee)
            {
                MessageBox.Show("오류지점 - " + ee.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }


        #endregion

        //기준일 변경
        private void DataGridCellTextBoxMoldInspectCycleDate_Loaded(object sender, RoutedEventArgs e)
        {
            InputMethod.SetIsInputMethodEnabled((TextBox)sender, false);
        }

        //기준일 변경
        private void DataGridCellTextBoxMoldInspectCycleDate_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Lib.Instance.CheckIsNumeric((TextBox)sender, e);
        }

        //기준일 변경
        private void DataGridCellTextBoxMoldInspectCycleDate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(strFlag.Equals("I"))
            {
                MoldEvalCal = dgdMain.CurrentItem as Win_dvl_MoldEvaCal_U_CodeView;

                if(MoldEvalCal != null)
                {
                    TextBox tb1 = sender as TextBox;

                    if(tb1 != null)
                    {
                        MoldEvalCal.MoldInspectCycleDate = tb1.Text;
                    }

                    sender = tb1;
                }
            }
        }
    }

    class Win_dvl_MoldEvaCal_U_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        public int Num { get; set; }
        public bool Flag { get; set; }

        public string MoldEvalID { get; set; }
        public string EvalDate { get; set; }
        public string MoldID { get; set; }
        public string MoldNo { get; set; }
        public string Article { get; set; }

        public string BuyerArticleNo { get; set; }
        public string AvgWorkHour { get; set; }
        public string HitCount { get; set; }
        public string QualPartEasyChangeRate { get; set; }
        public string QualOccurRate { get; set; }

        public string AvgWorkHourScore { get; set; }
        public string HitCountScore { get; set; }
        public string QualPartEasyChangeRateScore { get; set; }
        public string QualOccurRateScore { get; set; }
        public string QualAvgRepairHour { get; set; }

        public string QualAvgRepairHourScore { get; set; }
        public string Score { get; set; }
        public string EvalGrade { get; set; }
        public string EvalPersonName { get; set; }
        public string Comments { get; set; }
        public string MoldInspectCycleDate { get; set; }
        public string RepairCount12Month { get; set; } //2022-02-17 12개월내수리횟수

        public ObservableCollection<CodeView> OvcMold { get; set; }
    }

    class NewMoldEval : BaseView
    {
        public string EvalDate { get; set; }
        public string MoldID { get; set; }
        public string MoldNo { get; set; }
        public string Article { get; set; }

        public string BuyerArticleNo { get; set; }
        public string AvgWorkHour { get; set; }
        public string HitCount { get; set; }
        public string AvgRepairHour { get; set; }
        public string DefectOccur12Month { get; set; } //2022-02-17 최근 12개월 내 불량발생 수
        public string RepairCount12Month { get; set; } //2022-02-17 12개월내수리횟수
    }
}
