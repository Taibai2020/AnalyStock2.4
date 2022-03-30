using AnalyStock.DataAnalysis;
using AnalyStock.DataManage;
using AnalyStock.GrapImage;
using Microsoft.Win32;
using static AnalyStock.DataManage.StaticCollection;
namespace AnalyStock.WpfForm;
public partial class SelectStockPage : Page
{
    private readonly CustomProgress custProgress;
    private int selectTacticIdx = 0;
    /// <summary>
    /// 选股页面控制操作
    /// </summary>
    #region
    public SelectStockPage()
    {
        InitializeComponent();
        LstSele.ItemsSource =
            SeleStockTactics.LoadTactics().Select(n => n.Description);
        LstSele.SelectionChanged += (s, e) => selectTacticIdx = LstSele.SelectedIndex;
        custProgress = new();
        custProgress.ProgressChanged += (s, e) => LbProgress.Content = e.UserState;
    }
    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ChartKline.KlineRefresh();
    }
    private async void BtnAddSeleStock_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ChartKline.StockCode)) { return; }
        var bakdaily = MergeBakDailes.First(n => n.Ts_code == ChartKline.StockCode);
        if (bakdaily is not null)
        {
            await CommDataMethod.SaveSeleStockAsync(
                 ChartKline.StockCode, bakdaily.Name, bakdaily.Industry);
        }
    }
    private void BtnDelCurStock_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ChartKline.StockCode)) { return; }
        var indexCurrent = SeleStockKeyIndicators.FindIndex(n => n.StockCode == ChartKline.StockCode);
        if (indexCurrent > 0)
        {
            if (MessageBox.Show("是否确认删除该自选股，是否同意?", "删除确认"
                        , MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                SeleStockKeyIndicators.RemoveAt(indexCurrent);
            }
            DViewSeleStock.ItemsSource = default;
            DViewSeleStock.ItemsSource = SeleStockKeyIndicators;
        }
    }
    private void BtnExportData_Click(object sender, RoutedEventArgs e)
    {
        if (!SeleStockKeyIndicators.Any()) { return; }
        if (MessageBox.Show("是否保存选择股，是否同意?", "确认"
                        , MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        {
            var lstSeleStocks = (sender as Button) switch
            {
                { Name: "BtnExportAllData" } => SeleStockKeyIndicators
                                                .OrderByDescending(n => (n.Industry, n.LastEps)).ToList(),
                { Name: "BtnExportSeleData" } => SeleStockKeyIndicators
                                                .Where(n => n.IsSelect == true)
                                                .OrderByDescending(n => (n.Industry, n.LastEps)).ToList(),
                _ => throw new Exception("无股票选择数据...")
            };
            SaveFileDialog dialog = new()
            {
                Filter = $"CSV文件(*.CSV,*.csv)|*.CSV;*.csv"
            };
            _ = dialog.ShowDialog();
            CsvDataManage.SaveDataToCSV(lstSeleStocks, dialog.FileName);
            _ = MessageBox.Show($"数据保存到{dialog.FileName},处理完毕...");
        }

    }
    private void SeleStock_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DViewSeleStock.CurrentItem is not StockKeyIndicators item) { return; }
        ChartKline.KlineChanged(item.StockCode, item.StockName);
    }
    private async void BtnSelect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await StartSeleStockAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    #endregion

    /// <summary>
    /// 执行选股
    /// </summary>
    /// <param name=></param>
    /// 
    #region

    private static void IniDatasOnProcessing()
    {
        //初始化基础数据        
        SeleStockKeyIndicators ??= new();
        ErrorLogPool.ErrorLogs ??= new();
        SeleStockKeyIndicators?.Clear();
        ErrorLogPool.ErrorLogs?.Clear();

    }
    private async Task StartSeleStockAsync()
    {
        if (custProgress.IsBusy)
        {
            MessageBox.Show("当前执行选择没有结束,不能执行新流程...");
            return;
        }
        //填充股票基础数据
        if (!LoadDataCollection.IsFilltoMergeBakdaily()) { return; }
        //关闭UI控件       
        custProgress.IsBusy = true;
        LstSele.IsEnabled = false;
        BtnStopAction.IsEnabled = true;
        IniDatasOnProcessing();
        await Task.Run(() => DoworkOfSelectingAsync(custProgress));
        DoworkCompleted();
    }
    private void DoworkOfSelectingAsync(ICustomProgress progress)
    {
        int idxStock = 0;
        foreach (var singleStock in MergeBakDailes)
        {
            SeleStockTactics.LoadTactics()[selectTacticIdx].DoworkAction
                 (singleStock, SeleStockTactics.LoadTactics()[selectTacticIdx].Args);
            //后台反馈前窗口UI更新
            progress.Report(null, 0, $"第:{idxStock++}共{MergeBakDailes.Count}只股票，选出:{SeleStockKeyIndicators.Count}只股票...");
            if (progress.IsCancellation) { break; }
        }
    }
    private void BtnStopBgWork_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("中止执行，是否同意?", "Confirm Message"
                , MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        {
            if (custProgress.IsBusy) { custProgress.IsCancellation = true; }
        }
    }
    private void DoworkCompleted()
    {
        LbProgress.Content = custProgress.IsCancellation ? $"执行中止，选择{SeleStockKeyIndicators.Count}只股票了"
                                         : $"执行完成,共选择{SeleStockKeyIndicators.Count}只股票";

        DViewSeleStock.ItemsSource = default;
        DViewSeleStock.ItemsSource = SeleStockKeyIndicators;
        LstSele.IsEnabled = true;
        BtnStopAction.IsEnabled = false;
        custProgress.IsBusy = false;
        custProgress.IsCancellation = false;
    }
    #endregion
}
