using AnalyStock.DataManage;
using AnalyStock.GrapImage;
using Microsoft.Win32;
namespace AnalyStock.WpfForm;
public partial class LookStockPage : Page
{
    public LookStockPage()
    {
        InitializeComponent();
        Unloaded += Page_Unloaded;
        SizeChanged += Page_SizeChanged;
        ChartKline.IsWaitFillBakdailes = true;
        ChartKline.FillBakDailesed += Bakdailes_Filled;
        Background = new SolidColorBrush(
            CommDataMethod.ConvertoColor(ConfigurationManager.AppSettings["KlineBackground"]));
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        ChartKline.queryCodePanel?.Hide();
    }
    private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ChartKline.KlineRefresh();
    }
    private void Bakdailes_Filled(object sender, EventArgs e)
    {
        var markets = ChartKline.mergeBakDailes.Select(n => n.Market).Distinct();
        markets = markets.Append("自选股");
        markets = markets.Append("全部股");
        DViewStockInfor.ItemsSource = ChartKline.mergeBakDailes.OrderBy(n => n.Ts_code);
        CBoxSeleStockByMarket.ItemsSource = markets;
        CBoxSelcStockByIndustry.ItemsSource = ChartKline.mergeBakDailes.Select(n => n.Industry).Distinct();
    }
    /// <summary>
    /// K线数据筛选变化
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    #region 
    private void KlineChanged()
    {
        if (DViewStockInfor.CurrentItem is null) { return; }
        if (DViewStockInfor.CurrentItem is MergeBakDaily currentBakDaily)
        {
            ChartKline.KlineChanged(currentBakDaily.Ts_code, currentBakDaily.Name);
        }
    }
    private void CBoxSeleStockByMarket_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        GetDViewDataSource("market");
        ChartKline.HeaderItem.Text = "股票列示: " + CBoxSeleStockByMarket.SelectedItem.ToString();
    }
    private void CBoxSelcStockByIndustry_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        GetDViewDataSource("industry");
        ChartKline.HeaderItem.Text = "股票列示: " + CBoxSelcStockByIndustry.SelectedItem.ToString();
    }
    private void DViewStockInfor_KeyUp(object sender, KeyEventArgs e)
    {
        KlineChanged();
    }
    private void DViewStockInfor_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        KlineChanged();
    }
    private void GetDViewDataSource(string filterType)
    {
        if (ChartKline.mergeBakDailes is null) { return; }
        switch (filterType)
        {
            case "industry":
                DViewStockInfor.ItemsSource = ChartKline.mergeBakDailes
                    .Where(n => n.Industry == (string)CBoxSelcStockByIndustry.SelectedItem).OrderBy(n => n.Ts_code);
                break;
            case "market":
                DViewStockInfor.ItemsSource = (string)CBoxSeleStockByMarket.SelectedItem switch
                {
                    "自选股" => ChartKline.mergeBakDailes
                                .Where(n => n.Issele).OrderBy(n => n.Industry),
                    "全部股" => ChartKline.mergeBakDailes.OrderBy(n => n.Ts_code),
                    _ => ChartKline.mergeBakDailes
                                .Where(n => n.Market == (string)CBoxSeleStockByMarket.SelectedItem).OrderBy(n => n.Ts_code)
                };
                break;
            case "sele":
                DViewStockInfor.ItemsSource = ChartKline.mergeBakDailes
                                .Where(n => n.Issele).OrderBy(n => n.Industry);
                break;
        }
    }
    #endregion
    /// <summary>
    /// 自选股的增加与删除
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>   
    private async void UpdateSeleStock_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ChartKline.StockCode)) { return; }
        var mergeBakdaily = ChartKline.mergeBakDailes.First(n => n.Ts_code == ChartKline.StockCode);
        if (mergeBakdaily is null) { return; }
        switch (sender as Button)
        {
            case { Name: "BtnAddSeleStock" }:
                await CommDataMethod.SaveSeleStockAsync(ChartKline.StockCode, mergeBakdaily.Name, mergeBakdaily.Industry);
                mergeBakdaily.Issele = true;//在Dbgrid中修改为自选股票，显示数据
                break;
            case { Name: "BtnDelSeleStock" }:
                if (MessageBox.Show("是否确认删除该自选股，是否同意?", "删除确认",
                    MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) { return; }
                await CommDataMethod.DelSeleStockAsync(ChartKline.StockCode, mergeBakdaily.Name, mergeBakdaily.Industry);
                mergeBakdaily.Issele = false;
                break;
        }
        GetDViewDataSource("sele");
    }
    /// <summary>
    /// 其他数据处理方法
    /// </summary>
    /// <param name="csvPathFile"></param>
    /// <returns></returns>
    private void BtnLoadSeleOfCSV_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Multiselect = true,//该值确定是否可以选择多个文件
            Title = "请选择文件夹",
            Filter = "CSV文件(*.CSV,*.csv)|*.CSV;*.csv"
        };
        bool? result = dialog.ShowDialog();
        if (result is true)
        {
            string nameFile = dialog.FileName;
            DataTable tmpDt = LoadDataofCsvFile(nameFile);
            if (tmpDt.Rows.Count < 0)
            {
                return;
            }
            List<MergeBakDaily> stockInforOnLoad = new();
            foreach (DataRow dr in tmpDt.Rows)
            {
                stockInforOnLoad.Add(new MergeBakDaily()
                {
                    Ts_code = dr.Field<string>(0),
                    Name = dr.Field<string>(1),
                });
            }
            DViewStockInfor.ItemsSource = stockInforOnLoad.OrderBy(n => n.Ts_code);
        }
    }
    public static DataTable LoadDataofCsvFile(string csvPathFile)
    {
        try
        {
            return CsvDataManage.ReadDataofCSV(csvPathFile);
        }
        catch (Exception ex)
        {
            CommDataMethod.ErrorMessage(ex.Message);
        }
        return new();
    }

}
