using AnalyStock.DataManage;
using System.Threading;
using static AnalyStock.DataManage.CommDataMethod;
namespace AnalyStock.GrapImage;
public partial class ChartKline : UserControl
{
    //KLine参数
    public string StockCode { get; set; }
    public string StockName { get; set; }
    public IList<MergeBakDaily> mergeBakDailes;

    //当外部需要调用 Mergbakdailes数据时，使用该标志；
    //当数据加载完成时使用事件在异步中触发；避免因异步接受的数据集为NULL。
    public bool IsWaitFillBakdailes = false;
    public event EventHandler FillBakDailesed;
    //创建异步上下文环境
    readonly SynchronizationContext syncContext;//创建异步上下文环境
    string queryStartDate = DateTime.Today.AddMonths(-6).ToString("yyyyMMdd"); //默认显示半年内的数据
    string queryEndDate = DateTime.Today.ToString("yyyyMMdd");
    readonly DateTime firstDayIn10Years = DateTime.Today.AddYears(-10);
    readonly DateTime lastDayIn10Years = DateTime.Today;
    //启用查询代码窗口
    public QueryStockCode queryCodePanel;
    bool isQueryCodePanel, isFillF10Panel = true;
    /// <summary>
    /// 初始化，加载股票备份数据
    /// </summary>
    public ChartKline()
    {
        InitializeComponent();
        syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
        _ = GetMergBakdailyAsync();
    }
    /// <summary>
    /// K线变化、刷新
    /// </summary>
    /// <param name="args"></param>
    #region
    public async void KlineChanged(params string[] args)
    {
        if (string.IsNullOrEmpty(args[0])) { return; }
        StockCode = args[0];
        StockName = args[1];
        TextHeaderItem();
        await PaintKLinePanelAsync(); //画蜡烛线
        await PaintF10PanelAsync(); //画股票MACD、KDJ指标线
    }
    public void KlineRefresh()
    {
        if (string.IsNullOrEmpty(StockCode)) { return; }
        ChartStock.PaintKlineOnPeriod(); //画K线图
        ChartIndex.PaintKIndexOnPeriod(); //画指标线图 
    }
    private async Task PaintKLinePanelAsync()
    {
        await CreatCandleSeries();
        ChartStock.PaintKlineOnPeriod(); //画K线图
        ChartIndex.PaintKIndexOnPeriod(); //画指标线图        
    }
    private async Task PaintF10PanelAsync()
    {
        DisplayPanel.Children.Clear();
        FillStockBakInfor(); //显示股票面板属性数据
        await FillFinancialIndex(); //显示主要财务指标数据
    }
    private void FillStockBakInfor()
    {
        if (mergeBakDailes?.Count is null) { return; }
        var currbakinfor = mergeBakDailes.First(n => n.Ts_code == StockCode);
        DisplayPanel.Children.Add(new TextBlock()
        {
            Text = string.Join("\n", currbakinfor.ToValueString()),
            Width = 135,
            Style = FindResource("TextBlockStyle") as Style
        });
    }
    private async Task FillFinancialIndex()
    {
        var financialIndexs = await GetFinancialIndexAsync();
        //如果没有筛选指标返回
        if (!financialIndexs.Any()) { return; }
        //显示分项题头
        DisplayPanel.Children.Add(new TextBlock()
        {
            Text = string.Join("\n", FinancialIndex.GetPropertyChineseName()),
            Width = 130,
            Style = FindResource("TextBlockStyle") as Style,

        });
        //显示最新财务指标值
        DisplayPanel.Children.Add(new TextBlock()
        {
            Text = string.Join("\n", financialIndexs.Last().GetValueArray()),
            Width = 65,
            Style = FindResource("TextBlockStyle") as Style,
        });

        //显示最新指标的增长速度
        if (financialIndexs.Count > 4 && financialIndexs[^5] is not null)//上年同期
        {
            var curretfinaidx = financialIndexs.Last().GetValueArray();
            var lastyearfinaidx = financialIndexs[^5].GetValueArray();
            string[] increasefinaidx = new string[curretfinaidx.Length];

            for (int i = 0; i < curretfinaidx.Length; i++)
            {
                increasefinaidx[i] = (i < 1) ? "同比增长"
                                       : $"{(float)curretfinaidx[i] / (float)lastyearfinaidx[i] - 1:P1}";
            }

            DisplayPanel.Children.Add(new TextBlock()
            {
                Text = string.Join("\n", increasefinaidx),
                Width = 55,
                Style = FindResource("TextBlockStyle") as Style,
            });

        }
        //显示财务指标历史数据
        for (int i = financialIndexs.Count - 2; i >= 0; i--)
        {
            DisplayPanel.Children.Add(new TextBlock()
            {
                Text = string.Join("\n", financialIndexs[i].GetValueArray()),
                Width = 65,
                Style = FindResource("TextBlockStyle") as Style
            });
        }
        financialIndexs.Clear();
    }
    private void TextHeaderItem()
    {
        StockNameItem.Text = ((CandlePointProperty.PeriodType is "dailys") ? "日线趋势: " :
            (CandlePointProperty.PeriodType is "weeks") ? "周线趋势: " : "月线趋势: ") + StockName;
    }
    /// <summary>
    /// 加载交易数据，生成蜡烛线数据
    /// </summary>
    /// <returns></returns>
    private async Task CreatCandleSeries()
    {
        var infodailies = await GetInfoDailyAsync();
        if (!infodailies.Any()) { return; }
        CandlePointData.CreatCandlePoints(infodailies);
    }
    private async Task GetMergBakdailyAsync()
    {
        await Task.Run(async () =>
        {   //股票面板数据加载
            mergeBakDailes = await GetDataOnDbSetAsync<MergeBakDaily>();
            if (IsWaitFillBakdailes)
            {
                //数据加载完成后发出通知，触发后台事件，以提供其他窗体调用该数据集合，防止出现空调用异常
                syncContext.Post((object e) => FillBakDailesed?.Invoke(this, (EventArgs)e), null);
            }
        });
    }
    private async Task<IList<Infodaily>> GetInfoDailyAsync()
    {
        var infodailies = CheckBInline.IsChecked.Value
             ? await GetDataOnlineAsync<Infodaily>(StockCode, queryStartDate, queryEndDate)
             : await GetDataOnDbSetAsync<Infodaily>(StockCode, queryStartDate, queryEndDate);
        return CheckBCurrentInline.IsChecked.Value
            ? await infodailies.AddCurrentDataOnlineAsync()
            : infodailies;
    }
    private async Task<IList<FinancialIndex>> GetFinancialIndexAsync()
    {
        return CheckBFinaceInline.IsChecked.Value
           ? await GetDataOnlineAsync<FinancialIndex>
           (StockCode, $"{DateTime.Today.Year - 3}0101", DateTime.Today.ToString("yyyyMMdd"))
           : await GetDataOnDbSetAsync<FinancialIndex>
           (StockCode, $"{DateTime.Today.Year - 3}0101", DateTime.Today.ToString("yyyyMMdd"));
    }
    #endregion
    /// <summary>
    /// K线图起始日期变化
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    #region 
    private void ChangDaysCount_Click(object sender, RoutedEventArgs e)
    {
        (string typeofAddSub, int days) = (sender as Button) switch
        {
            { Name: "BtnAAddDates" } => ("addDays", 180),
            { Name: "BtnAddDates" } => ("addDays", 60),
            { Name: "BtnSubDates" } => ("subDays", 60),
            { Name: "BtnSSubDates" } => ("subDays", 180),
            _ => ("iniDays", 180),
        };
        ChangeQueryDates(typeofAddSub, days);
    }
    private void ChangeDateType_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(StockCode)) { return; }
        switch (sender as Button)
        {
            case { Name: "BtnDailyData" }:
                CandlePointProperty.PeriodType = "dailys";
                StockNameItem.Text = $"日线趋势:{StockName}";
                ChangeQueryDates("iniDays", 180);
                break;
            case { Name: "BtnWeekData" }:
                CandlePointProperty.PeriodType = "weeks";
                StockNameItem.Text = $"周线趋势:{StockName}";
                queryStartDate = DateTime.Today.AddDays(-180).ToString("yyyyMMdd"); //显示半年内的数据
                queryEndDate = DateTime.Today.ToString("yyyyMMdd");
                ChangeQueryDates("addDays", 540);
                break;
            case { Name: "BtnMonthData" }:
                CandlePointProperty.PeriodType = "months";
                StockNameItem.Text = $"月线趋势:{StockName}";
                queryStartDate = DateTime.Today.AddDays(-180).ToString("yyyyMMdd"); //显示半年内的数据
                queryEndDate = DateTime.Today.ToString("yyyyMMdd");
                ChangeQueryDates("addDays", 1200);
                break;
        }
    }
    private async void ChangeQueryDates(string typeofAddSub, int days)
    {
        DateTime startDate = DateTime.ParseExact(queryStartDate,
            "yyyyMMdd", System.Globalization.CultureInfo.CurrentCulture);
        switch (typeofAddSub)
        {
            case "addDays": //增加日期，增加面板蜡烛图数据
                if (startDate > firstDayIn10Years)
                {
                    queryStartDate = startDate.AddDays(-days).ToString("yyyyMMdd");
                }
                break;
            case "subDays": //减少日期，减少面板蜡烛图数据
                if (startDate < lastDayIn10Years)
                {
                    if (ChartStock.TotalIndexofPanel > days)
                    {
                        queryStartDate = startDate.AddDays(days).ToString("yyyyMMdd");
                    }
                }
                break;
            case "iniDays": //恢复到初始期间
                queryStartDate = DateTime.Today.AddDays(-days).ToString("yyyyMMdd"); //显示半年内的数据                
                break;
        }
        await PaintKLinePanelAsync();
    }
    #endregion
    /// <summary>
    /// 技术指标图
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    #region
    private void ChartPriceLine_Click(object sender, RoutedEventArgs e)
    {
        switch (sender as Button)
        {
            case { Name: "BtnMA2" }: //均线5,30
                ChartStock.IsMa2line = !ChartStock.IsMa2line;
                ChartStock.IsMa4line = !ChartStock.IsMa2line && ChartStock.IsMa4line;
                break;
            case { Name: "BtnMA4" }: //均线5,10,30,60
                ChartStock.IsMa4line = !ChartStock.IsMa4line;
                ChartStock.IsMa2line = !ChartStock.IsMa4line && ChartStock.IsMa2line;
                break;
            case { Name: "BtnInflectPoint" }: //拐点线
                ChartStock.IsInflectPoint = !ChartStock.IsInflectPoint;
                break;
            case { Name: "BtnTrendLine" }:
                ChartStock.IsTrendLine = !ChartStock.IsTrendLine;
                break;
            case { Name: "BtnDoubleVolLine" }:
                ChartStock.IsDoubleVolLine = !ChartStock.IsDoubleVolLine;
                break;
            default:
                break;
        }
        try
        {
            ChartStock.PaintKlineOnPeriod();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + $"生成{(sender as Button).Name}线图像错误...");
        }
    }
    private void ChartKlineIndex_Click(object sender, RoutedEventArgs e)
    {
        switch (sender as Button)
        {
            case { Name: "BtnMACD" }:
                ChartIndex.IsMACD = true;
                ChartIndex.IsKDJ = false;
                ChartIndex.IsEPS = false;
                break;
            case { Name: "BtnKDJ" }:
                ChartIndex.IsMACD = false;
                ChartIndex.IsKDJ = true;
                ChartIndex.IsEPS = false;
                break;
            case { Name: "BtnEPS" }:
                ChartIndex.IsMACD = false;
                ChartIndex.IsKDJ = false;
                ChartIndex.IsEPS = true;
                break;
            default:
                break;
        }
        try
        {
            ChartIndex.PaintKIndexOnPeriod();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message + $"生成{(sender as Button).Name}线图像错误...");
        }
    }
    private void ChangeF10Panel_Click(object sender, RoutedEventArgs e)
    {
        isFillF10Panel = !isFillF10Panel;
        FinacialGridRow.Height = isFillF10Panel
            ? new GridLength(3.0, GridUnitType.Star)
            : new GridLength(0.0, GridUnitType.Star);
        UpdateLayout();
        KlineRefresh();
    }
    #endregion
    /// <summary>
    /// 编码快速查询窗口打开
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    #region 
    private void BtnQuery_Click(object sender, RoutedEventArgs e)
    {
        //根据查询按钮在屏幕的位置，打开查询窗口
        if (!isQueryCodePanel)
        {
            queryCodePanel = new(ref mergeBakDailes, (sender as Button).PointFromScreen(new Point(0, 0)));
            queryCodePanel.DGStock.SelectionChanged += QueryStockCode_Changed;
            queryCodePanel.DGStock.MouseDoubleClick += QueryStockCode_Changed;
            queryCodePanel.Closed += (object sender, EventArgs e) => isQueryCodePanel = false;
            queryCodePanel.Activated += (object sender, EventArgs e) => isQueryCodePanel = true;
            MainWindow.EventHidePanel += () => queryCodePanel?.Hide();
        }
        queryCodePanel?.Show();
    }
    private void QueryStockCode_Changed(object sender, RoutedEventArgs e)
    {
        if ((sender as DataGrid).Items.Count is 0) { return; }
        if ((sender as DataGrid).CurrentItem is StockNames item)
        {
            KlineChanged(item.CodeWithName[..9], item.CodeWithName[10..14]);
        }
    }
    #endregion
}

