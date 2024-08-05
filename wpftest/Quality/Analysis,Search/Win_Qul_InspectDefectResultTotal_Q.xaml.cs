﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WizMes_ParkPro.PopUP;

namespace WizMes_ParkPro
{
    /// <summary>
    /// Win_Qul_InspectDefectResultTotal_Q.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Win_Qul_InspectDefectResultTotal_Q : UserControl
    {
        string stDate = string.Empty;
        string stTime = string.Empty;
        PlusFinder pf = new PlusFinder();

        int rowNum = 0;
        Lib lib = new Lib();
        public Win_Qul_InspectDefectResultTotal_Q()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stDate = DateTime.Now.ToString("yyyyMMdd");
            stTime = DateTime.Now.ToString("HHmm");

            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "S");

            Lib.Instance.UiLoading(sender);

            //콤보박스 셋팅       
            SetComboBox();

            //검사일자 체크
            chkDate.IsChecked = true;

            //공정 체크 해제
            chkProcess.IsChecked = false;

            //품명 체크 해제
            chkArticle.IsChecked = false;

   

            //데이트피커 오늘 날짜
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;

            //콤보박스 기본값 '전체'
            cboProcess.SelectedIndex = 0;

            //조건 박스 false
            cboProcess.IsEnabled = false;
            txtArticle.IsEnabled = false;
            txtBuyerArticleNoSrh.IsEnabled = false;
            txtInCustom.IsEnabled = false;
            txtInCustom.IsEnabled = false;
        }

        //콤보박스 셋팅
        private void SetComboBox()
        {
            //공정
            ObservableCollection<CodeView> cboProcessGroup = ComboBoxUtil.Instance.GetWorkProcess(0, "");
            this.cboProcess.ItemsSource = cboProcessGroup;
            this.cboProcess.DisplayMemberPath = "code_name";
            this.cboProcess.SelectedValuePath = "code_id";
        }

        #region 클릭 이벤트

        //입고일자 라벨 클릭 이벤트
        private void LblchkDay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkDate.IsChecked == true)
            {
                chkDate.IsChecked = false;
                dtpSDate.IsEnabled = false;
                dtpEDate.IsEnabled = false;
            }
            else
            {
                chkDate.IsChecked = true;
                dtpSDate.IsEnabled = true;
                dtpEDate.IsEnabled = true;
            }
        }

        //입고일자 체크 이벤트
        private void ChkDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = true;
            dtpEDate.IsEnabled = true;
        }

        //입고일자 체크해제 이벤트
        private void ChkDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpSDate.IsEnabled = false;
            dtpEDate.IsEnabled = false;
        }

        //전일
        private void btnYesterDay_Click(object sender, RoutedEventArgs e)
        {
            DateTime[] SearchDate = lib.BringLastDayDateTimeContinue(dtpEDate.SelectedDate.Value);

            dtpSDate.SelectedDate = SearchDate[0];
            dtpEDate.SelectedDate = SearchDate[1];
        }

        //금일
        private void btnToday_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = DateTime.Today;
            dtpEDate.SelectedDate = DateTime.Today;
        }

        // 전월 버튼 클릭 이벤트
        private void btnLastMonth_Click(object sender, RoutedEventArgs e)
        {
            DateTime[] SearchDate = lib.BringLastMonthContinue(dtpSDate.SelectedDate.Value);

            dtpSDate.SelectedDate = SearchDate[0];
            dtpEDate.SelectedDate = SearchDate[1];
        }

        // 금월 버튼 클릭 이벤트
        private void btnThisMonth_Click(object sender, RoutedEventArgs e)
        {
            dtpSDate.SelectedDate = lib.BringThisMonthDatetimeList()[0];
            dtpEDate.SelectedDate = lib.BringThisMonthDatetimeList()[1];
        }



        //공정 라벨 클릭 이벤트
        private void LblProcess_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkProcess.IsChecked == true)
            {
                chkProcess.IsChecked = false;
                cboProcess.IsEnabled = false;
            }
            else
            {
                chkProcess.IsChecked = true;
                cboProcess.IsEnabled = true;
            }
        }

        //공정 체크 이벤트
        private void ChkProcess_Checked(object sender, RoutedEventArgs e)
        {
            cboProcess.IsEnabled = true;
        }

        //공정 체크 해제 이벤트
        private void ChkProcess_Unchecked(object sender, RoutedEventArgs e)
        {
            cboProcess.IsEnabled = false;
        }

        //품명 라벨 클릭 이벤트
        private void LblArticle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkArticle.IsChecked == true)
            {
                chkArticle.IsChecked = false;
                txtArticle.IsEnabled = false;
                btnArticle.IsEnabled = false;
            }
            else
            {
                chkArticle.IsChecked = true;
                txtArticle.IsEnabled = true;
                btnArticle.IsEnabled = true;
            }
        }

        //품명 체크 이벤트
        private void ChkArticle_Checked(object sender, RoutedEventArgs e)
        {
            txtArticle.IsEnabled = true;
            btnArticle.IsEnabled = true;
        }

        //품명 체크 해제 이벤트
        private void ChkArticle_Unchecked(object sender, RoutedEventArgs e)
        {
            txtArticle.IsEnabled = false;
            btnArticle.IsEnabled = false;
        }

        //품명 텍스트박스 키다운
        private void txtArticle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                MainWindow.pf.ReturnCode(txtArticle, 77, txtArticle.Text);
            }
        }

        //품명 플러스파인더
        private void btnArticle_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.pf.ReturnCode(txtArticle, 77, txtArticle.Text);
        }
        #endregion 클릭이벤트, 날짜

        #region CRUD 버튼

        //검색(조회)
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //검색버튼 비활성화
            btnSearch.IsEnabled = false;

            Dispatcher.BeginInvoke(new Action(() =>

            {
                Thread.Sleep(2000);

                //로직
                if (CheckData())
                {
                    re_Search(rowNum);
                }

            }), System.Windows.Threading.DispatcherPriority.Background);



            Dispatcher.BeginInvoke(new Action(() =>

            {
                btnSearch.IsEnabled = true;

            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        //닫기
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DataStore.Instance.InsertLogByFormS(this.GetType().Name, stDate, stTime, "E");
            Lib.Instance.ChildMenuClose(this.ToString());
        }

        //엑셀
        //private void btnExcel_Click(object sender, RoutedEventArgs e)
        //{
        //    DataTable dt = null;
        //    string Name = string.Empty;
        //    Lib lib = new Lib();

        //    string[] lst = new string[2];
        //    lst[0] = "공정불량현황";
        //    lst[1] = dgdLeft.Name;

        //    ExportExcelxaml ExpExc = new ExportExcelxaml(lst);

        //    ExpExc.ShowDialog();

        //    if (ExpExc.DialogResult.HasValue)
        //    {
        //        if (ExpExc.Check.Equals("Y"))
        //            dt = lib.DataGridToDTinHidden(dgdLeft);
        //        else
        //            dt = lib.DataGirdToDataTable(dgdLeft);

        //        Name = dgdLeft.Name;

        //        if (lib.GenerateExcel(dt, Name))
        //        {
        //            lib.excel.Visible = true;
        //            lib.ReleaseExcelObject(lib.excel);
        //        }
        //        else
        //            return;
        //    }
        //    else
        //    {
        //        if (dt != null)
        //        {
        //            dt.Clear();
        //        }
        //    }
        //    lib = null;
        //}


        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = null;

            string[] lst = new string[4];
            lst[0] = "품번별 불량유형";
            lst[1] = "불량유형별 검사품목";
            lst[2] = dgdLeft.Name;
            lst[3] = dgdRight.Name;

            ExportExcelxaml ExpExc = new ExportExcelxaml(lst);
            ExpExc.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ExpExc.ShowDialog();

            if (ExpExc.DialogResult.HasValue)
            {
                if (ExpExc.choice.Equals(dgdLeft.Name))
                {
                    DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");

                    if (ExpExc.Check.Equals("Y"))
                        dt = lib.DataGridToDTinHidden(dgdLeft);
                    else
                        dt = lib.DataGirdToDataTable(dgdLeft);

                    if (lib.GenerateExcel(dt, dgdLeft.Name))
                    {
                        lib.excel.Visible = true;
                        lib.ReleaseExcelObject(lib.excel);
                    }
                    else
                        return;
                }
                else if (ExpExc.DialogResult.HasValue)
                {
                    if (ExpExc.choice.Equals(dgdRight.Name))
                    {
                        DataStore.Instance.InsertLogByForm(this.GetType().Name, "E");

                        if (ExpExc.Check.Equals("Y"))
                            dt = lib.DataGridToDTinHidden(dgdRight);
                        else
                            dt = lib.DataGirdToDataTable(dgdRight);

                        if (lib.GenerateExcel(dt, dgdRight.Name))
                        {
                            lib.excel.Visible = true;
                            lib.ReleaseExcelObject(lib.excel);
                        }
                        else
                            return;
                    }
                }
                else
                {
                    if (dt != null)
                        dt.Clear();
                }
            }
        }

        #endregion CRUD 버튼


        #region 데이터그리드 이벤트

        //데이터그리드 셀렉션체인지드
        private void dgdLeft_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //조회만 하는 화면이라 이 친구는 필요가 없지요.
        }

        #endregion 데이터그리드 이벤트

        #region 조회관련(Fillgrid)

        //재조회
        private void re_Search(int selectedIndex)
        {
            FillGrid();

            if (dgdLeft.Items.Count > 0)
            {
                dgdLeft.SelectedIndex = selectedIndex;
            }
        }


        private void FillGrid()
        {
            dgdLeft.Items.Clear();
            dgdRight.Items.Clear();
            dgdTotal.Items.Clear();

            try
            {
                // 2개의 그리드 호출
                for (int i = 0; i < 2; i++)
                {
                    Dictionary<string, object> sqlParameter = new Dictionary<string, object>();

                    sqlParameter.Add("chkDate", chkDate.IsChecked == true ? 1 : 0);
                    sqlParameter.Add("sDate", chkDate.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                    sqlParameter.Add("eDate", chkDate.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
                    sqlParameter.Add("chkCustomer", chkCustomer.IsChecked == true ? 1 : 0);     //거래처
                    sqlParameter.Add("Customer", chkCustomer.IsChecked == true ? txtCustomer.Text : ""); //거래처

                    sqlParameter.Add("chkInCustomer", chkInCustom.IsChecked == true ? 1 : 0);
                    sqlParameter.Add("InCustomer", chkInCustom.IsChecked == true ? txtInCustom.Text : "");

                    sqlParameter.Add("chkBuyersArticleNo", chkBuyerArticleNoSrh.IsChecked == true ? 1 : 0);
                    sqlParameter.Add("BuyersArticleNo", chkBuyerArticleNoSrh.IsChecked == true ? txtBuyerArticleNoSrh.Text : "");

                    sqlParameter.Add("chkArticle", chkArticle.IsChecked == true ? 1 : 0);
                    sqlParameter.Add("ArticleID", chkArticle.IsChecked == true ? txtArticle.Tag.ToString() : "");


                    // 구분
                    int gubun = i + 1;
                    bool defect_tot = gubun == 1;
                    sqlParameter.Add("nGubun", gubun);

                    DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_Qul_sInspectDefectResultTotal", sqlParameter, true, "R");
                    if (ds != null && ds.Tables.Count > 0)
                    {
                        DataTable dt = ds.Tables[0];
                        if (dt.Rows.Count > 0)
                        {
                            int idx = 0;
                            DataRowCollection drc = dt.Rows;
                            foreach (DataRow dr in drc)
                            {
                                idx++;
                                var Win = new Win_Qul_InspectDefectResultTotal_Q_CodeView()
                                {
                                    Num = idx,
                                    cls = dr["cls"].ToString().Trim(),
                                    ArticleID = dr["ArticleID"].ToString(),
                                    BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
                                    CtrlQty =  stringFormatN0(Convert.ToDouble(dr["CtrlQty"])),
                                    Article = dr["Article"].ToString(),
                                    DefectID = dr["DefectID"].ToString(),
                                    KDefect = dr["KDefect"].ToString(),
                                    DefectQty = stringFormatN0(dr["DefectQty"]),
                                    DefectRate = stringFormatN2(dr["DefectRate"]),
                                };

                                if (Win.cls.Equals("1"))
                                {
                                    Win.cls = "";
                                }
                                else if (Win.cls.Equals("2")) // 불량유형계 및 규격계
                                {
                                    Win.cls = defect_tot ? "불량계" : "품번계";

                                    if (defect_tot)
                                    { Win.KDefect = ""; Win.ColorGreen = "true"; }
                                    else
                                    { Win.BuyerArticleNo = ""; Win.ColorGreen = "true"; }
                            
                                }
                                else if (Win.cls.Equals("9")) // 총계
                                {
                                    Win.cls = "총계";
                                 

                                    if (i == 0)
                                        dgdTotal.Items.Add(Win);
                                }

                                if (defect_tot) dgdLeft.Items.Add(Win);
                                else            dgdRight.Items.Add(Win);
                            }
                        }
                    }
                }

                if (dgdLeft.Items.Count == 0 && dgdRight.Items.Count == 0)
                    MessageBox.Show("조회된 데이터가 없습니다.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("[오류내용]: " + ex.ToString());
            }
            finally
            {
                DataStore.Instance.CloseConnection();
            }
        }

        //조회
        //private void FillGrid()
        //{
        //    if (dgdLeft.Items.Count > 0)
        //    {
        //        dgdLeft.Items.Clear();
        //    }

        //    try
        //    {

        //        Dictionary<string, object> sqlParameter = new Dictionary<string, object>();
        //        sqlParameter.Clear();

        //        sqlParameter.Add("chkDate", chkDate.IsChecked == true ? 1 : 0);
        //        sqlParameter.Add("sDate", chkDate.IsChecked == true ? dtpSDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
        //        sqlParameter.Add("eDate", chkDate.IsChecked == true ? dtpEDate.SelectedDate.Value.ToString("yyyyMMdd") : "");
        //        sqlParameter.Add("chkCustomer", chkCustomer.IsChecked == true ? 1 : 0);     //거래처
        //        sqlParameter.Add("Customer", chkCustomer.IsChecked == true ? txtCustomer.ToString() : ""); //거래처

        //        sqlParameter.Add("chkInCustomer", chkInCustom.IsChecked == true ? 1 : 0);
        //        sqlParameter.Add("InCustomer", chkInCustom.IsChecked == true ? txtInCustom.ToString() : "");

        //        sqlParameter.Add("chkArticle", chkArticle.IsChecked == true ? 1 : 0);
        //        sqlParameter.Add("ArticleID", chkArticle.IsChecked == true ? txtArticle.Tag.ToString() : "");

        //        sqlParameter.Add("chkBuyersArticleNo", chkBuyerArticleNoSrh.IsChecked == true ? 1 : 0);
        //        sqlParameter.Add("BuyersArticleNo", chkBuyerArticleNoSrh.IsChecked == true ? txtBuyerArticleNoSrh.ToString() : "");          

        //        DataSet ds = DataStore.Instance.ProcedureToDataSet_LogWrite("xp_Inspect_sInspectDefectTotal", sqlParameter, true, "R");

        //        if (ds != null && ds.Tables.Count > 0)
        //        {
        //            DataTable dt = ds.Tables[0];

        //            if (dt.Rows.Count == 0)
        //            {
        //                MessageBox.Show("조회결과가 없습니다.");
        //                return;
        //            }
        //            else
        //            {
        //                DataRowCollection drc = dt.Rows;

        //                int i = 0;
        //                foreach (DataRow dr in drc)
        //                {
        //                    i++;
        //                    var DefectInfo = new Win_Qul_InspectDefectResultTotal_Q_CodeView()
        //                    {
        //                        Num = i,
        //                        cls = dr["cls"].ToString(),
        //                        ScanDate = dr["ScanDate"].ToString(),
        //                        ProcessID = dr["ProcessID"].ToString(),
        //                        Process = dr["Process"].ToString(),
        //                        BuyerModelID = dr["BuyerModelID"].ToString(),
        //                        ArticleID = dr["ArticleID"].ToString(),
        //                        Article = dr["Article"].ToString(),
        //                        BuyerArticleNo = dr["BuyerArticleNo"].ToString(),
        //                        DefectID = dr["DefectID"].ToString(),
        //                        KDefect = dr["KDefect"].ToString(),
        //                        DefectQty = stringFormatN0(dr["DefectQty"]),
        //                        WorkPersonID = dr["WorkPersonID"].ToString(),
        //                        WorkPersonName = dr["WorkPersonName"].ToString(),
        //                        MCNAME = dr["MCNAME"].ToString(),
        //                        LabelID = dr["LabelID"].ToString(),
        //                        ChildLabelID = dr["ChildLabelID"].ToString()
        //                    };

        //                    if ((DefectInfo.ScanDate != "" && DefectInfo.ScanDate != null))
        //                    {
        //                          DefectInfo.ScanDate = DefectInfo.ScanDate.ToString().Substring(0, 4) + "-"
        //                        + DefectInfo.ScanDate.ToString().Substring(4, 2) + "-"
        //                        + DefectInfo.ScanDate.ToString().Substring(6, 2);
        //                    }



        //                    if (DefectInfo.DefectQty.Equals("") && DefectInfo.cls.Equals("9"))
        //                    {
        //                        MessageBox.Show("조회결과가 없습니다.");
        //                        return;
        //                    }

        //                    if (DefectInfo.cls.Equals("9"))
        //                    {
        //                        dgdTotal.Items.Clear();
        //                        DefectInfo.ScanDate = "총 발생수량";
        //                        //DefectInfo.ColorLightLightGray = "false";
        //                        DefectInfo.ColorGold = "true";
        //                        dgdTotal.Items.Add(DefectInfo);
        //                    }
        //                    else
        //                    {
        //                        dgdLeft.Items.Add(DefectInfo);
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("오류 발생, 오류 내용 : " + ex.ToString());
        //    }
        //    finally
        //    {
        //        DataStore.Instance.CloseConnection();
        //    }
        //}

        //검색 조건 Check
        private bool CheckData()
        {
            bool flag = true;

            if (chkArticle.IsChecked == true)
            {
                if(txtArticle.Text == "")
                {
                    MessageBox.Show("품명 선택이 되지 않았습니다. 체크를 해제하거나 품명을 선택하고 검색해 주세요.");
                    flag = false;
                    return flag;
                }
            }
            if (chkBuyerArticleNoSrh.IsChecked == true)
            {
                if(txtBuyerArticleNoSrh.Text == "")
                {
                    MessageBox.Show("품번 선택이 되지 않았습니다. 체크를 해제하거나 품번을 선택하고 검색해 주세요.");
                    flag = false;
                    return flag;
                }
            }
            if (chkCustomer.IsChecked == true)
            {
                if (txtCustomer.Text == "")
                {
                    MessageBox.Show("거래처 선택이 되지 않았습니다. 체크를 해제하거나 거래처를 선택하고 검색해 주세요.");
                    flag = false;
                    return flag;
                }
            }
            if (chkInCustom.IsChecked == true)
            {
                if (txtInCustom.Text == "")
                {
                    MessageBox.Show("최종거래처 선택이 되지 않았습니다. 체크를 해제하거나 최종거래처를 선택하고 검색해 주세요.");
                    flag = false;
                    return flag;
                }
            }

            return flag;
        }


        #endregion 조회관련(Fillgrid)
        
        #region 기타 메소드 
        //특수문자 포함 검색
        private string Escape(string str)
        {
            string result = "";

            for (int i = 0; i < str.Length; i++)
            {
                string txt = str.Substring(i, 1);

                bool isSpecial = Regex.IsMatch(txt, @"[^a-zA-Z0-9가-힣]");

                if (isSpecial == true)
                {
                    result += (@"/" + txt);
                }
                else
                {
                    result += txt;
                }
            }
            return result;
        }

        // 천단위 콤마, 소수점 버리기
        private string stringFormatN0(object obj)
        {
            return string.Format("{0:N0}", obj);
        }

        // 천단위 콤마, 소수점 두자리
        private string stringFormatN2(object obj)
        {
            return string.Format("{0:N2}", obj);
        }
        #endregion

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

        //날짜 선택시 밸리데이션체크
        private void dtpSDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpSDate.SelectedDate > dtpEDate.SelectedDate)
            {
                MessageBox.Show("종료일자는 시작일 이후로 설정해주세요.");
                dtpSDate.SelectedDate = Convert.ToDateTime(e.RemovedItems[0].ToString());
            }

        }
        //날짜 선택시 밸리데이션체크
        private void dtpEDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpSDate.SelectedDate > dtpEDate.SelectedDate)
            {
                MessageBox.Show("종료일자는 시작일 이후로 설정해주세요.");
                dtpEDate.SelectedDate = Convert.ToDateTime(e.RemovedItems[0].ToString());
            }
        }


        //-----------------------------------------------------------------------------------------------
        //거래처 라벨클릭
        private void lblCustomer_Click(object sender, MouseButtonEventArgs e)
        {
            if (chkCustomer.IsChecked == true)
            {
                chkCustomer.IsChecked = false;
                txtCustomer.IsEnabled = false;
                btnCustomer.IsEnabled = false;
            }
            else
            {
                chkCustomer.IsChecked = true;
                txtCustomer.IsEnabled = true;
                btnCustomer.IsEnabled = true;
                txtCustomer.Focus();
            }
        }
        //거래처 체크
        private void ChkCustomer_Checked(object sender, RoutedEventArgs e)
        {
            chkCustomer.IsChecked = true;
            txtCustomer.IsEnabled = true;
            btnCustomer.IsEnabled = true;
            txtCustomer.Focus();
        }       
        //거래처 언체크
        private void ChkCustomer_Unchecked(object sender, RoutedEventArgs e)
        {
            chkCustomer.IsChecked = false;
            txtCustomer.IsEnabled = false;
            btnCustomer.IsEnabled = false;
        }
        //거래처 버튼
        private void btnCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                pf.ReturnCode(txtCustomer, 0, "");
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - btnCustomer_Click : " + ee.ToString());
            }
        }
        //거래처 키다운
        private void txtCustomer_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    pf.ReturnCode(txtCustomer, 0, "");
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("오류지점 - txtCustomer_KeyDown : " + ee.ToString());
            }
        }
        //-------------------------------------------------------------------------------


        //-------------------------------------------------------------------------------
        //최종거래처 라벨클릭
        private void lblInCustom_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkInCustom.IsChecked == true)
            {
                chkInCustom.IsChecked = false;
                txtInCustom.IsEnabled = false;
                btnInCustom.IsEnabled = false;
            }
            else
            {
                chkInCustom.IsChecked = true;
                txtInCustom.IsEnabled = true;
                btnInCustom.IsEnabled = true;
                txtInCustom.Focus();
            }
        }

        //최종거래처 체크박스
        private void chkInCustom_Click(object sender, RoutedEventArgs e)
        {
            if (chkInCustom.IsChecked == true)
            {
                chkInCustom.IsChecked = true;
                txtInCustom.IsEnabled = true;            
                btnInCustom.IsEnabled = true;
            }
            else
            {
                chkInCustom.IsChecked = false;
                txtInCustom.IsEnabled = false;
                btnInCustom.IsEnabled = false;
                txtInCustom.Focus();
            }
        }
        
        //최종거래처 버튼클릭
        private void btnInCustom_Click(object sender, RoutedEventArgs e)
        {
            pf.ReturnCode(txtInCustom, 72, "");
        }

        //최종거래처 텍스트박스
        private void txtInCustom_KeyDown(object sender, KeyEventArgs e)
        {
            pf.ReturnCode(txtInCustom, 72, "");
        }
        //----------------------------------------------------------------------------

        //----------------------------------------------------------------------------
        //품번 라벨 클릭
        private void lblBuyerArticleNoSrh_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (chkBuyerArticleNoSrh.IsChecked == false)
            {
                chkBuyerArticleNoSrh.IsChecked = true;
                txtBuyerArticleNoSrh.IsEnabled = true;
                btnBuyerArticleNoSrh.IsEnabled = true;
            }
            else
            {
                chkBuyerArticleNoSrh.IsChecked = false;
                txtBuyerArticleNoSrh.IsEnabled = false;
                btnBuyerArticleNoSrh.IsEnabled = false;
            }
        }
        //품번 체크박스
        private void chkBuyerArticleNoSrh_Click(object sender, RoutedEventArgs e)
        {
            if (chkBuyerArticleNoSrh.IsChecked == true)
            {
                chkBuyerArticleNoSrh.IsChecked = true;
                txtBuyerArticleNoSrh.IsEnabled = true;
                btnBuyerArticleNoSrh.IsEnabled = true;
            }
            else
            {
                chkBuyerArticleNoSrh.IsChecked = false;
                txtBuyerArticleNoSrh.IsEnabled = false;
                btnBuyerArticleNoSrh.IsEnabled = false;
            }
        }
        //품번 버튼
        private void btnBuyerArticleNoSrh_Click(object sender, RoutedEventArgs e)
        {
             pf.ReturnCode(txtBuyerArticleNoSrh, 83, "");
        }
        //품번 키다운
        private void txtBuyerArticleNoSrh_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
              pf.ReturnCode(txtBuyerArticleNoSrh, 83, "");
            }
        }
        //------------------------------------------------------------------------
      

        //------------------------------------------------------------------------
        //품명 라벨 클릭
        private void lblArticle_Click(object sender, MouseButtonEventArgs e)
        {
            if (chkArticle.IsChecked == true)
            {
                chkArticle.IsChecked = false;
                txtArticle.IsEnabled = false;
                btnArticle.IsEnabled = false;
            }
            else
            {
                chkArticle.IsChecked = true;
                txtArticle.IsEnabled = true;
                btnArticle.IsEnabled = true;
            }
        }


      
        //---------------------------------------------------------------------------------
  


    }

    #region 생성자들(CodeView)

    class Win_Qul_InspectDefectResultTotal_Q_CodeView : BaseView
    {
        public int Num { get; set; }
        public string cls { get; set; }
        public string ScanDate { get; set; }
        public string ProcessID { get; set; }
        public string Process { get; set; }
        public string BuyerModelID { get; set; }
        public string ArticleID { get; set; }
        public string Article { get; set; }
        public string BuyerArticleNo { get; set; }
        public string CtrlQty { get; set; }
        public string DefectID { get; set; }
        public string KDefect { get; set; }
        public string DefectQty { get; set; }
        public string DefectRate { get; set; }
        public string WorkPersonID { get; set; }
        public string WorkPersonName { get; set; }
        public string MCNAME { get; set; }

        public string LabelID { get; set; }
        public string ChildLabelID { get; set; }

        public string ColorLightLightGray { get; set; }

        public string ColorGold { get; set; }
        public string ColorRed { get; set; }
        public string ColorGreen { get; set; }

    }

    #endregion 생성자들(CodeView)
}