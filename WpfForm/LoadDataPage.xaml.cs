using AnalyStock.DataManage;

namespace AnalyStock.WpfForm;

public partial class LoadDataPage : Page
{
    private string startDate, endDate, stockCode;
    //批量导入单日或期间数据分标识:DownDataOnPeriod,DownDataOnDaily,DownDataOfFinance
    private string typeofLoadInBatch; //
    public LoadDataPage()
    {
        InitializeComponent();
        ListBoxSele.ItemsSource = new string[]{
                                    "⊙下载股票名录",//index 0
                                    "--------------",
                                    "⊙下载交易数据", //2
                                    "--------------",
                                    "⊙下载财务数据",//4
                                    "--------------",
                                    "⊙下载交易备份",//6
                                    "--------------",
                                    };
        btnLoadAllData.IsEnabled = false;
        btnLoadSingleData.IsEnabled = false;
        btnStop.IsEnabled = false;
    }
    private void ListBox1_SelectionChanged(object sender, EventArgs e)
    {
        switch (ListBoxSele.SelectedIndex)
        {
            case 0:
                btnLoadSingleData.IsEnabled = true;
                btnLoadAllData.IsEnabled = false;
                textBstartDate.Text = "";
                textBendDate.Text = "";
                break;
            case 2:
                btnLoadSingleData.IsEnabled = true;
                btnLoadAllData.IsEnabled = true;
                textBstartDate.Text = DateTime.Today.AddDays(-3).ToString("yyyyMMdd");
                textBendDate.Text = DateTime.Today.ToString("yyyyMMdd");
                checkBoxByDayorPeriod.IsEnabled = true;
                checkBoxByDayorPeriod.IsChecked = false;
                typeofLoadInBatch = checkBoxByDayorPeriod.IsChecked.Value ? "DownDataOnPeriod" : "DownDataOnDaily";
                break;
            case 4:
                btnLoadSingleData.IsEnabled = true;
                btnLoadAllData.IsEnabled = true;
                textBstartDate.Text = (DateTime.Today.Year - 1).ToString() + "01" + "01";
                textBendDate.Text = DateTime.Today.ToString("yyyyMMdd");
                checkBoxByDayorPeriod.IsChecked = true;
                checkBoxByDayorPeriod.IsEnabled = false;  //关闭单日全部股票数据导入功能
                typeofLoadInBatch = "DownDataOfFinance"; //后台导入财务指标数
                break;
            case 6:
                btnLoadSingleData.IsEnabled = true;
                btnLoadAllData.IsEnabled = false;
                textBstartDate.Text = "";
                textBendDate.Text = DateTime.Today.AddDays(-1).ToString("yyyyMMdd");
                break;
            default:
                break;
        }
        labelPrograss1.Content = "";
        labelPrograss2.Content = "";
    }
    private void CheckBoxByDayorPeriod_Click(object sender, RoutedEventArgs e)
    {
        typeofLoadInBatch = checkBoxByDayorPeriod.IsChecked.Value
                                 ? "DownDataOnPeriod"
                                 : "DownDataOnDaily";
    }
    private async void BtnLoadAllData_Click(object sender, EventArgs e)
    {
        switch (ListBoxSele.SelectedIndex)
        {
            case 2 or 4: await DownLoadByMultiTaskAsync(); break;
            default: CommDataMethod.InforMessage("请选择批量的下载选项..."); break;
        }
    }
    private async void BtnLoadSingleData_Click(object sender, EventArgs e)
    {
        Cursor = Cursors.Wait;
        switch (ListBoxSele.SelectedIndex)
        {
            case 0: await LoadBasicNameOfStockAsync(); break;
            case 2: await LoadInforDailyOfSingleStockAsync(); break;
            case 4: await LoadFinacialIndexOfSingleStockAsync(); break;
            case 6: await LoadBakDailyOfSingleDailyAsync(); break;
        }
        Cursor = Cursors.Arrow;
    }
    /// <summary>
    /// 文本框输入格式核验
    /// </summary>
    /// <param name="downType"></param>
    /// <returns></returns>       
    private bool IsWrongOfCheckFormatting(string downType)
    {
        startDate = textBstartDate.Text.Trim().ToLower();
        endDate = textBendDate.Text.Trim().ToLower();
        stockCode = textBCode.Text.Trim().ToUpper();
        switch (downType)
        {
            case "DownAllStockOnPeriod":
                if (startDate.Length != 8 || endDate.Length != 8)
                {
                    CommDataMethod.ErrorMessage("日期格式有误(yyyyMMdd)，请核查...");
                    return true;
                }
                break;
            case "DownSingleStockOnPeriod":
                if (startDate.Length != 8 || endDate.Length != 8 || stockCode.Length < 9)
                {
                    CommDataMethod.ErrorMessage("日期格式(yyyyMMdd)或股票代码有误，请核查...");
                    return true;
                }
                break;
            case "DownAllStockOnSingleDaily":
                if (endDate.Length != 8)
                {
                    CommDataMethod.ErrorMessage("日期格式有误(yyyyMMdd)...");
                    return true;
                }
                break;
        }
        return false;
    }
    private static bool IsConfirmMessage(string strSuggestion)
    {
        return MessageBox.Show(strSuggestion, "确认操作",
            MessageBoxButton.OKCancel, MessageBoxImage.Question) is MessageBoxResult.OK;
    }
    private async Task LoadInforDailyOfSingleStockAsync()
    {
        if (IsWrongOfCheckFormatting("DownSingleStockOnPeriod")) return;
        if (IsConfirmMessage("追加交易信息前，要删除原有时间段交易信息，是否同意?"))
        {
            await CommDataMethod.DownDataToDbSetAsync<Infodaily>(false, stockCode, startDate, endDate);
        }
    }
    private async Task LoadFinacialIndexOfSingleStockAsync()
    {
        if (IsWrongOfCheckFormatting("DownSingleStockOnPeriod")) return;
        if (IsConfirmMessage("追加交易信息前，要删除原有时间段交易信息，是否同意?"))
        {
            await CommDataMethod.DownDataToDbSetAsync<FinancialIndex>(false, stockCode, startDate, endDate);
            //_ = MessageBox.Show("数据已完成导入...");
        }
    }
    private static async Task LoadBasicNameOfStockAsync()
    {
        if (IsConfirmMessage("更新基本信息前，要删除原有全部信息，是否同意?"))
        {
            await CommDataMethod.DownDataToDbSetAsync<BasicStock>(true);
        }
    }
    private async Task LoadBakDailyOfSingleDailyAsync()
    {
        if (IsWrongOfCheckFormatting("DownAllStockOnSingleDaily")) return;
        if (IsConfirmMessage("更新基本信息前，要删除原有全部信息，是否同意?"))
        {
            await CommDataMethod.DownDataToDbSetAsync<BakDaily>(true, endDate);
        }
    }
}
