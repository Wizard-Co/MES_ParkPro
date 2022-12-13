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
    /// Win_dvl_MoldEvalBasis_U.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_dvl_MoldEvalBasis_U : UserControl
    {
        string strFlag = string.Empty;
        int rowNum = 0;
        Win_dvl_MoldEvalBasis_U_CodeView WinMoldEvalBasis = new Win_dvl_MoldEvalBasis_U_CodeView();

        public Win_dvl_MoldEvalBasis_U()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Lib.Instance.UiLoading(sender);
        }

        /// <summary>
        /// 수정,추가 저장 후
        /// </summary>
        private void CanBtnControl()
        {
            ////gbxMcPart.IsEnabled = false;
            Lib.Instance.UiButtonEnableChange_IUControl(this);
        }

        /// <summary>
        /// 수정,추가 진행 중
        /// </summary>
        private void CantBtnControl()
        {
            ////gbxMcPart.IsEnabled = true;
            Lib.Instance.UiButtonEnableChange_SCControl(this);
        }

        //추가
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CantBtnControl();
            strFlag = "I";

            //lblMsg.Visibility = Visibility.Visible;
            tbkMsg.Text = "자료 입력 중";
            rowNum = dgdMain.SelectedIndex;
            this.DataContext = null;

            btnMainAdd.IsEnabled = true;
            btnMainDel.IsEnabled = true;
        }

        //수정
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            WinMoldEvalBasis = dgdMain.SelectedItem as Win_dvl_MoldEvalBasis_U_CodeView;

            if (WinMoldEvalBasis != null)
            {
                rowNum = dgdMain.SelectedIndex;
                tbkMsg.Text = "자료 수정 중";
                //lblMsg.Visibility = Visibility.Visible;
                CantBtnControl();
                strFlag = "U";

                btnMainAdd.IsEnabled = true;
                btnMainDel.IsEnabled = true;
            }
        }

        //삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgdMain.Items.Count <= 0)
            {
                MessageBox.Show("삭제할 데이터가 존재하지 않습니다.");
            }
            else
            {
                if (MessageBox.Show("표에 보이는 모든 항목이 삭제됩니다. 삭제하시겠습니까?", "삭제 전 확인",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    DeleteData();
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
            rowNum = 0;
            re_Search(rowNum);
        }

        //저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (DeleteData())
            {
                if (SaveData(strFlag))
                {
                    CanBtnControl();
                    //lblMsg.Visibility = Visibility.Hidden;
                    rowNum = 0;
                    re_Search(rowNum);

                    btnMainAdd.IsEnabled = false;
                    btnMainDel.IsEnabled = false;
                }
            }
        }

        //취소
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            CanBtnControl();

            if (!strFlag.Equals(string.Empty))
            {
                re_Search(rowNum);
            }

            strFlag = string.Empty;

            btnMainAdd.IsEnabled = false;
            btnMainDel.IsEnabled = false;
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;
            string Name = string.Empty;

            string[] lst = new string[2];
            lst[0] = "금형등급평가기준";
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
                DataSet ds = null;
                Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                sqlParameter.Clear();
                ds = DataStore.Instance.ProcedureToDataSet("xp_dvlMold_sMoldEvalBasis", sqlParameter, false);

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
                            var WinMoldEvalBasis = new Win_dvl_MoldEvalBasis_U_CodeView()
                            {
                                seq = dr["seq"].ToString(),
                                EvalGroupName = dr["EvalGroupName"].ToString(),
                                EvalName = dr["EvalName"].ToString(),
                                EvalItemSpec = dr["EvalItemSpec"].ToString(),
                                EvalScore = dr["EvalScore"].ToString(),
                                EvalSpecMax = dr["EvalSpecMax"].ToString(),
                                EvalSpecMin = dr["EvalSpecMin"].ToString(),
                                AutoEvalYN = dr["AutoEvalYN"].ToString()
                            };

                            if (Lib.Instance.IsNumOrAnother(WinMoldEvalBasis.EvalSpecMin))
                            {
                                if (WinMoldEvalBasis.EvalSpecMin.Contains(".") &&
                                    int.Parse(WinMoldEvalBasis.EvalSpecMin.Substring(WinMoldEvalBasis.EvalSpecMin.IndexOf(".") + 1)) <= 0)
                                {
                                    WinMoldEvalBasis.EvalSpecMin = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.EvalSpecMin);
                                }
                                else if (!WinMoldEvalBasis.EvalSpecMin.Contains("."))
                                {
                                    WinMoldEvalBasis.EvalSpecMin = Lib.Instance.returnNumStringOne(WinMoldEvalBasis.EvalSpecMin);
                                }
                                else
                                {
                                    WinMoldEvalBasis.EvalSpecMin = Lib.Instance.returnNumStringOne(WinMoldEvalBasis.EvalSpecMin);
                                }
                            }

                            if (Lib.Instance.IsNumOrAnother(WinMoldEvalBasis.EvalSpecMax))
                            {
                                if (WinMoldEvalBasis.EvalSpecMax.Contains(".") &&
                                    int.Parse(WinMoldEvalBasis.EvalSpecMax.Substring(WinMoldEvalBasis.EvalSpecMax.IndexOf(".") + 1)) <= 0)
                                {
                                    WinMoldEvalBasis.EvalSpecMax = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.EvalSpecMax);
                                }
                                else if (!WinMoldEvalBasis.EvalSpecMax.Contains("."))
                                {
                                    WinMoldEvalBasis.EvalSpecMax = Lib.Instance.returnNumStringOne(WinMoldEvalBasis.EvalSpecMax);
                                }
                                else
                                {
                                    WinMoldEvalBasis.EvalSpecMax = Lib.Instance.returnNumStringOne(WinMoldEvalBasis.EvalSpecMax);
                                } 
                            }

                            if (Lib.Instance.IsNumOrAnother(WinMoldEvalBasis.EvalScore))
                            {
                                if (WinMoldEvalBasis.EvalScore.Contains(".") &&
                                    int.Parse(WinMoldEvalBasis.EvalScore.Substring(WinMoldEvalBasis.EvalScore.IndexOf(".") + 1)) <= 0)
                                {
                                    WinMoldEvalBasis.EvalScore = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.EvalScore);
                                }
                                else if (!WinMoldEvalBasis.EvalScore.Contains("."))
                                {
                                    WinMoldEvalBasis.EvalScore = Lib.Instance.returnNumStringZero(WinMoldEvalBasis.EvalScore);
                                }
                                else
                                {
                                    WinMoldEvalBasis.EvalScore = Lib.Instance.returnNumStringOne(WinMoldEvalBasis.EvalScore);
                                }
                            }

                            dgdMain.Items.Add(WinMoldEvalBasis);
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
        /// 실삭제 , 테이블 내의 데이터 모두 삭제
        /// </summary>
        /// <returns></returns>
        private bool DeleteData()
        {
            bool flag = false;

            Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
            sqlParameter.Clear();

            string[] result = DataStore.Instance.ExecuteProcedure("xp_dvlMold_dMoldEvalBasis", sqlParameter, false);
            DataStore.Instance.CloseConnection();

            if (result[0].Equals("success"))
            {
                //MessageBox.Show("성공 *^^*");
                flag = true;
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
                if (CheckData())
                {
                    #region 추가&&수정

                    if (strFlag.Equals("I")|| strFlag.Equals("U"))
                    {
                        for (int i = 0; i < dgdMain.Items.Count; i++)
                        {
                            WinMoldEvalBasis = dgdMain.Items[i] as Win_dvl_MoldEvalBasis_U_CodeView;

                            Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
                            sqlParameter.Clear();
                            sqlParameter.Add("Seq", "");
                            sqlParameter.Add("EvalName", WinMoldEvalBasis.EvalName);
                            sqlParameter.Add("EvalSpecMin", WinMoldEvalBasis.EvalSpecMin.Replace(",",""));
                            sqlParameter.Add("EvalSpecMax", WinMoldEvalBasis.EvalSpecMax.Replace(",", ""));
                            sqlParameter.Add("EvalScore", WinMoldEvalBasis.EvalScore.Replace(",", ""));
                            sqlParameter.Add("AutoEvalYN", WinMoldEvalBasis.AutoEvalYN);
                            sqlParameter.Add("EvalItemSpec", WinMoldEvalBasis.EvalItemSpec);
                            sqlParameter.Add("EvalGroupName", WinMoldEvalBasis.EvalGroupName);
                            sqlParameter.Add("CreateUserID", MainWindow.CurrentUser);

                            Procedure pro1 = new Procedure();
                            pro1.Name = "xp_dvlMold_iMoldEvalBasis";
                            pro1.OutputUseYN = "N";
                            pro1.OutputName = "Seq";
                            pro1.OutputLength = "2";

                            Prolist.Add(pro1);
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
            return flag;
        }

        //메인 그리드 행추가
        private void btnMainAdd_Click(object sender, RoutedEventArgs e)
        {
            MainAddRow();
        }

        private void MainAddRow()
        {
            int i = 1;

            if (dgdMain.Items.Count > 0)
            {
                i = dgdMain.Items.Count + 1;
            }

            var WinMoldEval = new Win_dvl_MoldEvalBasis_U_CodeView()
            {
                seq = i.ToString(),
                EvalGroupName = "",
                EvalItemSpec = "",
                EvalName = "",
                EvalSpecMin = "",
                EvalSpecMax ="",
                EvalScore="",
                AutoEvalYN=""
            };
            dgdMain.Items.Add(WinMoldEval);
        }

        //메인 그리드 행삭제
        private void btnMainDel_Click(object sender, RoutedEventArgs e)
        {
            MainRemoveRow();
        }

        private void MainRemoveRow()
        {
            if (dgdMain.Items.Count > 0)
            {
                if (dgdMain.CurrentItem != null)
                {
                    dgdMain.Items.Remove((dgdMain.CurrentItem as Win_dvl_MoldEvalBasis_U_CodeView));
                }
                else
                {
                    dgdMain.Items.Remove((dgdMain.Items[dgdMain.Items.Count - 1]) as Win_dvl_MoldEvalBasis_U_CodeView);
                }
                dgdMain.Refresh();
            }
        }

        private void DataGridCell_KeyDown(object sender, KeyEventArgs e)
        {
            WinMoldEvalBasis = dgdMain.CurrentItem as Win_dvl_MoldEvalBasis_U_CodeView;
            int rowCount = dgdMain.Items.IndexOf(dgdMain.CurrentItem);
            int colCount = dgdMain.Columns.IndexOf(dgdMain.CurrentCell.Column);

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                (sender as DataGridCell).IsEditing = false;

                if (dgdMain.Columns.Count - 1 == colCount &&
                    dgdMain.Items.Count - 1 == rowCount)
                {
                    MainAddRow();
                    dgdMain.CurrentCell = new DataGridCellInfo
                        (dgdMain.Items[rowCount + 1], dgdMain.Columns[1]);
                }
                else if ((dgdMain.Columns.Count - 1 > colCount &&
                    dgdMain.Items.Count - 1 == rowCount) || (dgdMain.Columns.Count - 1 > colCount &&
                    dgdMain.Items.Count - 1 > rowCount))
                {
                    dgdMain.CurrentCell = new DataGridCellInfo
                        (dgdMain.Items[rowCount], dgdMain.Columns[colCount + 1]);
                }
                else if (dgdMain.Columns.Count - 1 == colCount &&
                    dgdMain.Items.Count - 1 > rowCount)
                {
                    dgdMain.CurrentCell = new DataGridCellInfo
                        (dgdMain.Items[rowCount + 1], dgdMain.Columns[1]);
                }
                else
                {
                    MessageBox.Show("?");
                }
            }
        }

        //TextBox or ComboBox Cell Focus(keydown)
        private void TextBoxFocusInDataGrid(object sender, KeyEventArgs e)
        {
            Lib.Instance.DataGridINControlFocus(sender, e);
        }

        //TextBox or ComboBox Cell Focus(MouseClick)
        private void TextBoxFocusInDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Lib.Instance.DataGridINBothByMouseUP(sender, e);
        }

        //cellEditingMode 진입
        private void DataGridCell_GotFocus(object sender, RoutedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                DataGridCell cell = sender as DataGridCell;
                cell.IsEditing = true;
            }
        }

        //구분
        private void dgdtpetxtEvalGroupName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldEvalBasis = dgdMain.CurrentItem as Win_dvl_MoldEvalBasis_U_CodeView;

                if (WinMoldEvalBasis != null)
                {
                    TextBox tb1 = sender as TextBox;
                    WinMoldEvalBasis.EvalGroupName = tb1.Text;
                    sender = tb1;
                }
            }
        }

        //항목명
        private void dgdtpetxtEvalName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldEvalBasis = dgdMain.CurrentItem as Win_dvl_MoldEvalBasis_U_CodeView;

                if (WinMoldEvalBasis != null)
                {
                    TextBox tb1 = sender as TextBox;
                    WinMoldEvalBasis.EvalName = tb1.Text;
                    sender = tb1;
                }
            }
        }

        //Spec
        private void dgdtpetxtEvalItemSpec_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldEvalBasis = dgdMain.CurrentItem as Win_dvl_MoldEvalBasis_U_CodeView;

                if (WinMoldEvalBasis != null)
                {
                    TextBox tb1 = sender as TextBox;
                    WinMoldEvalBasis.EvalItemSpec = tb1.Text;
                    sender = tb1;
                }
            }
        }

        //EvalScore1
        private void dgdtpetxtEvalScore1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldEvalBasis = dgdMain.CurrentItem as Win_dvl_MoldEvalBasis_U_CodeView;

                if (WinMoldEvalBasis != null)
                {
                    TextBox tb1 = sender as TextBox;
                    WinMoldEvalBasis.EvalSpecMin = Lib.Instance.returnNumStringTwoExceptDot(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    sender = tb1;
                }
            }
        }

        //EvalScore2
        private void dgdtpetxtEvalScore2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldEvalBasis = dgdMain.CurrentItem as Win_dvl_MoldEvalBasis_U_CodeView;

                if (WinMoldEvalBasis != null)
                {
                    TextBox tb1 = sender as TextBox;
                    WinMoldEvalBasis.EvalSpecMax = Lib.Instance.returnNumStringTwoExceptDot(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    sender = tb1;
                }
            }
        }

        //EvalScore3
        private void dgdtpetxtEvalScore3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldEvalBasis = dgdMain.CurrentItem as Win_dvl_MoldEvalBasis_U_CodeView;

                if (WinMoldEvalBasis != null)
                {
                    TextBox tb1 = sender as TextBox;
                    WinMoldEvalBasis.AutoEvalYN = tb1.Text;
                    sender = tb1;
                }
            }
        }

        //EvalScore4
        private void dgdtpetxtEvalScore4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (lblMsg.Visibility == Visibility.Visible)
            {
                WinMoldEvalBasis = dgdMain.CurrentItem as Win_dvl_MoldEvalBasis_U_CodeView;

                if (WinMoldEvalBasis != null)
                {
                    TextBox tb1 = sender as TextBox;
                    WinMoldEvalBasis.EvalScore = Lib.Instance.returnNumStringTwoExceptDot(tb1.Text);
                    tb1.SelectionStart = tb1.Text.Length;
                    sender = tb1;
                }
            }
        }

        //숫자만 입력(소수점)
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //Lib.Instance.CheckIsNumeric(sender as TextBox, e);
        }
    }

    class Win_dvl_MoldEvalBasis_U_CodeView : BaseView
    {
        public override string ToString()
        {
            return (this.ReportAllProperties());
        }

        //public int Num { get; set; }
        public string seq { get; set; }
        public string EvalGroupName { get; set; }
        public string EvalName { get; set; }
        public string EvalSpecMin { get; set; }
        public string EvalSpecMax { get; set; }
        public string EvalScore { get; set; }
        public string EvalItemSpec { get; set; }
        public string AutoEvalYN { get; set; } 
    }
}
