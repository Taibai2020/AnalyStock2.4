using AnalyStock.DataManage;
using static AnalyStock.DataAnalysis.LoadDataCollection;
using static AnalyStock.DataManage.CandlesExtend;
using static AnalyStock.DataManage.StaticCollection;
namespace AnalyStock.DataAnalysis;

public static class SeleStockTactics
{
    public static IList<DoworkOfTactics> LoadTactics() => new List<DoworkOfTactics>
        {
            new("1.上升走势/接近近期最高拐点95-105%",SeleNeartoLastTopPoint,null),
            new("2.回调走势/120日内振幅50%/底部振荡/价格在近期高点75%-115%",SeleBottomWaveInLastDays, new object[]{120,}),
            new("3.✦涨停回调不破5日均线",SeleSkyrocketingtoBack,new object[]{5,}),
            new("4.✦涨停回调15-30%之间",SeleSkyrocketingtoBack30P,null),
            new("5.✦尾盘30分钟异动股",SeleChangeOn30MinutesOfLateDay,null)
        };
    /// <summary>
    ///1 股价创新高
    /// </summary>
    /// <param name="index"></param>    
    public static void SeleNeartoLastTopPoint(MergeBakDaily bakDaily, params object[] args)
    {
        //取得阶段性顶点的拐点系列值            
        if (!IsFilltoBasicDatas(bakDaily.Ts_code)) { return; }
        //1.选择趋势向上
        var (trendLable, trendDescribe)
            = CreatPaintPoints.CreateFittingPointsBySample().AnalyTrendOfKline();
        if (!trendLable.Contains('F') || string.IsNullOrEmpty(trendDescribe)) { return; }
        //2.选择当前价格在近期高点的95%-105%之间
        var (isTop, lastTop) = InflectPoints.GetLastTopPoint();
        if (!isTop || lastTop.GrowToCurrent is < 0.95f or > 1.05f) { return; }
        var (isFina, finaIndex) = FinancialIndexs.GetLastFinaIndex();
        SeleStockKeyIndicators.Add(new()
        {
            StockCode = bakDaily.Ts_code,
            StockName = bakDaily.Name,
            CurrentClose = InflectPoints.Last().Close,
            LastEps = isFina ? finaIndex.EPS : 0,
            FloatMv = bakDaily.Float_mv,
            DaysToMaxCLose = lastTop.DaysToCurrent,
            GrowRate = $"{lastTop.GrowToCurrent * 100.0f:F2}%",
            Industry = bakDaily.Industry,
            Trend = trendDescribe,
            Description = $"股价超过{lastTop.DaysToCurrent}日前拐点"
                         + $"增长了{lastTop.GrowToCurrent * 100.0f:0.00}%",
        });
    }
    /// <summary>
    /// 冲高走势的股票，days日内倍量冲高，回调不破位
    /// </summary>
    /// <param name="index"></param>
    /// <param name="days"></param>
    public static void SeleUptoDoubleVolOnUpward(MergeBakDaily bakDaily, params object[] args)
    {
        if (!IsFilltoBasicDatas(bakDaily.Ts_code)) { return; }
        //1、趋势开始向上
        var (trendLable, trendDescribe)
           = CreatPaintPoints.CreateFittingPointsBySample().AnalyTrendOfKline();
        if (!trendLable.Contains('F') || string.IsNullOrEmpty(trendDescribe)) { return; }
        //2.近期存在倍量
        var doubleVols = CandlePoints.TakeLastDayPoints(CandleType.DoubleVol, (int)args[0]).ToArray();
        if (doubleVols.Length is 0) { return; }
        //3.当前价格在倍量的二一位置以上
        float currVal = CandlePoints.Last().Close;
        float midVal = (doubleVols.Last().Open + doubleVols.Last().Close) / 2.0f;
        if (currVal < midVal) { return; }
        //4、如果增幅超过15%，则过了最佳买点，退出 
        float rateGrow = currVal / doubleVols.Last().Close - 1.0f;
        if (rateGrow > 0.15f) { return; }
        //5、股价振荡在50%以内 
        var (isKline, klineIndex) = CandlePoints.GetTrendIndexsInLastDays(120);
        if (!isKline || klineIndex.WaveRate < 0.50f) { return; }
        //4、EPS在0.10以上
        var (isFina, finaIndex) = FinancialIndexs.GetLastFinaIndex();
        if (!isFina || finaIndex.EPS < 0.05f) { return; }
        SeleStockKeyIndicators.Add(new()
        {
            StockCode = bakDaily.Ts_code,
            StockName = bakDaily.Name,
            CurrentClose = currVal,
            LastEps = isFina ? finaIndex.EPS : 0,
            FloatMv = bakDaily.Float_mv,
            DaysToMaxCLose = klineIndex.DaysToMax,
            GrowRate = $"{klineIndex.RatioCurrToMax * 100.0f:F1}%",
            Industry = bakDaily.Industry,
            Trend = "股价在最近倍量柱1/2位以上",
            Description = $"{CandlePoints.Last().LocationIndex - doubleVols.Last().LocationIndex}日内，进攻倍量柱，回调未破位",
        });
        return;
    }
    /// <summary>
    ///2 底部触底振荡的股票
    /// </summary>
    /// <param name="days"></param>
    /// <param name="bakDaily"></param>
    public static void SeleBottomWaveInLastDays(MergeBakDaily bakDaily, params object[] args)
    {
        if (!IsFilltoBasicDatas(bakDaily.Ts_code)) { return; }
        //1.取拐点序列，分析价格走势,股价振荡下行           
        var (trendLable, trendDescribe)
            = CreatPaintPoints.CreateFittingPointsBySample().AnalyTrendOfKline();
        if (!trendLable.Contains('L') || string.IsNullOrEmpty(trendDescribe)) { return; }
        //2、股价振荡在50%以内 
        var (isKline, klineIndex) = CandlePoints.GetTrendIndexsInLastDays((int)args[0]);
        if (!isKline || klineIndex.WaveRate < 0.50f) { return; }
        //振荡少于8周的不考虑
        if (klineIndex.DaysToMax < 40) { return; }
        //3.底部振荡，底部拐点的振荡幅度在15%内。
        var (isThanThreeHitBottom, countHitBottomLessThan10P)
            = InflectPoints.AnalyWaveOnBottomPoints();
        if (!isThanThreeHitBottom) { return; }
        //4.近10日交易倍量出现1次以上
        if (klineIndex.DoubleVolsWithin20Days < 1) { return; }
        //5.两个月40日内5日和20日金叉至少两次，最后一次金叉距离不超过一周
        var (countGoldCross, daysToCurrent)
            = CandlePoints.AnalyGoldCrossMa5toMa20InDays(40);
        if (countGoldCross < 2 || daysToCurrent > 5) { return; }
        //6.业绩EPS要大于0.05
        var (isFina, finaIndex)
            = FinancialIndexs.GetLastFinaIndex();
        if (!isFina || finaIndex.EPS < 0.05f) { return; }
        SeleStockKeyIndicators.Add(new()
        {
            StockCode = bakDaily.Ts_code,
            StockName = bakDaily.Name,
            CurrentClose = klineIndex.Current,
            DaysToMaxCLose = klineIndex.DaysToMax,
            LastEps = finaIndex.EPS,
            FloatMv = bakDaily.Float_mv,
            GrowRate = $"股价在最高价{klineIndex.RatioCurrToMax * 100.0f:F2}%",
            Industry = bakDaily.Industry,
            Trend = $"触底10%内振荡{countHitBottomLessThan10P}次",
            Description = $"收盘价振幅:{(1 - klineIndex.WaveRate) * 100.0f:F2}%"
        });
    }
    /// <summary>
    /// 3 龙头战法，days日内涨停，且回调不破5日均线，抓再次涨停龙头
    /// </summary>
    /// <param name="days"></param>
    /// <param name="bakDaily"></param>
    public static void SeleSkyrocketingtoBack(MergeBakDaily bakDaily, params object[] args)
    {
        if (!IsFilltoBasicDatas(bakDaily.Ts_code)) { return; }
        //1、days日有内涨停
        var skyrocketings = CandlePoints.TakeLastDayPoints(CandleType.SkyrocKeting, (int)args[0]);
        if (!skyrocketings.Any()) { return; }

        var ma5Line = CandlePoints.Select(n => n.Close).ToArray().CalculateMAVal(5);
        if (ma5Line.Count < 1) { return; }
        //2、当前价格在5日均线以上
        if (CandlePoints.Last().Close < ma5Line.Last().MAValue) { return; }
        //3.EPS在0.05
        var (isFina, finaIndex) = FinancialIndexs.GetLastFinaIndex();
        if (!isFina || finaIndex.EPS < 0.05f) { return; }
        //涨停后趋势描述
        string trendDescription = "";
        int skyrocketingIndex = skyrocketings.Last().LocationIndex;
        if (skyrocketingIndex != CandlePoints.Last().LocationIndex)
        {
            var skyrocketedTrend = (
                VolTrend: CandlePoints[skyrocketingIndex + 1].Vol > CandlePoints[skyrocketingIndex].Vol,
                PriceTrend: CandlePoints[skyrocketingIndex + 1].Close >= CandlePoints[skyrocketingIndex].Close,
                NextDayUpWard: CandlePoints[skyrocketingIndex + 1].IsCloseUpOpen);
            trendDescription = skyrocketedTrend switch
            {
                (true, true, true) => "涨停后放量冲高//次日价格冲高...",
                (true, true, false) => "涨停后放量冲高//次日价格回落...",
                (true, false, true or false) => "涨停后放量回调清洗...",
                (false, true, true) => "涨停后缩量拉升//次日价格冲高...",
                (false, true, false) => "涨停后缩量拉升//次日价格回落...",
                (false, false, true or false) => "涨停后缩量回调...",
            };
        }
        else
        {
            trendDescription = "当日冲高涨停..."; //当日涨停
        }

        SeleStockKeyIndicators.Add(new()
        {
            StockCode = bakDaily.Ts_code,
            StockName = bakDaily.Name,
            CurrentClose = CandlePoints.Last().Close,
            LastEps = isFina ? finaIndex.EPS : 0,
            FloatMv = bakDaily.Float_mv,
            DaysToMaxCLose = CandlePoints.Last().LocationIndex - skyrocketingIndex,
            GrowRate = $"{CandlePoints.Last().Close / skyrocketings.Last().Close - 1.0f:P1}",
            Industry = bakDaily.Industry,
            Trend = trendDescription,
            Description = $"{CandlePoints.Last().LocationIndex - skyrocketingIndex}日内，涨停进攻，回调未破位5日均线",
        });
        return;
    }
    /// <summary>
    /// 4 涨停后回调在70-85%之间的振荡，可考虑择机介入
    /// </summary>
    /// <param name="bakDaily"></param>
    public static void SeleSkyrocketingtoBack30P(MergeBakDaily bakDaily, params object[] args)
    {
        if (!IsFilltoBasicDatas(bakDaily.Ts_code)) { return; }
        //1.最近的高位拐点存在涨停
        var (isTop, lastTop) = InflectPoints.GetLastTopPoint();
        if (!isTop || !lastTop.IsSkyRocketing) { return; }
        //2、当日股价处于回调状态，位置为70-85%之间，基本调整到位
        if (lastTop.GrowToCurrent is < 0.68f or > 0.85f) { return; }
        //3.EPS在0.05
        var (isFina, finaIndex) = FinancialIndexs.GetLastFinaIndex();
        if (!isFina || finaIndex.EPS < 0.05f) { return; }
        SeleStockKeyIndicators.Add(new()
        {
            StockCode = bakDaily.Ts_code,
            StockName = bakDaily.Name,
            CurrentClose = CandlePoints.Last().Close,
            LastEps = isFina ? finaIndex.EPS : 0,
            FloatMv = bakDaily.Float_mv,
            DaysToMaxCLose = lastTop.DaysToCurrent,
            GrowRate = $"{lastTop.GrowToCurrent:P1}",
            Industry = bakDaily.Industry,
            Trend = "涨停回调",
            Description = $"{lastTop.DaysToCurrent}日内，涨停回调处于高点的70%-85%振荡",
        });
    }
    /// <summary>
    /// 5 尾盘30分钟，异动选股策略
    /// </summary>
    /// <param name="bakDaily"></param>
    public static void SeleChangeOn30MinutesOfLateDay(MergeBakDaily bakDaily, params object[] args)
    {
        if (!IsFilltoBasicDatas(bakDaily.Ts_code)) { return; }
        //涨幅在3%-5%之间
        float pctPriceRaise = CandlePoints.Last().Pctchg;
        if (pctPriceRaise is < 3.0f or > 5.0f) { return; }
        //量比大于1；
        float ratioQR = CandlePoints.GetQuantityRelativerRatioOnCurrent();
        if (ratioQR <= 1.0) { return; }
        //换手率在5%到10%之间
        float rateTurnOver = CandlePoints.GetTurnOverRateOnCurrent(bakDaily);
        if (rateTurnOver is < 5.0f or >= 10.0f) { return; }
        //流通市值在30亿-200亿之间
        if (bakDaily.Float_mv is < 30.0f or >= 200.0f) { return; }
        var (isFina, finaIndex) = FinancialIndexs.GetLastFinaIndex();
        if (!isFina || finaIndex.EPS < 0.05f) { return; }

        SeleStockKeyIndicators.Add(new()
        {
            StockCode = bakDaily.Ts_code,
            StockName = bakDaily.Name,
            CurrentClose = CandlePoints.Last().Close,
            LastEps = isFina ? finaIndex.EPS : 0,
            FloatMv = bakDaily.Float_mv,
            //DaysToMaxCLose =0,
            //GrowRate = 0,
            Industry = bakDaily.Industry,
            Trend = "尾盘异动",
            Description = $"价格涨幅:{pctPriceRaise:F1},量比:{ratioQR:F1},换手率:{rateTurnOver:F1}",
        });



    }
}

public record DoworkOfTactics(string Description, Action<MergeBakDaily, object[]> DoworkAction, object[] Args);
public class StockKeyIndicators
{
    public string StockCode { get; set; }
    public string StockName { get; set; }
    public bool IsSelect { get; set; }
    public string Industry { get; set; }
    public float CurrentClose { get; set; }
    public float LastEps { get; set; }
    public float FloatMv { get; set; }
    public int DaysToMaxCLose { get; set; }
    public string Trend { get; set; }
    public string GrowRate { get; set; }
    public string Description { get; set; }
}



