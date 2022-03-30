using AnalyStock.DataManage;
using static AnalyStock.DataManage.CandlesExtend;
using static AnalyStock.DataManage.InflectPsExtend;
using static AnalyStock.DataManage.StaticCollection;

namespace AnalyStock.DataAnalysis;

internal static class SeleStockMethod
{
    //获得换手率
    public static float GetTurnOverRateOnCurrent(this IList<CandlePoint> list, MergeBakDaily mergeBak)
    {
        return mergeBak.Float_share > 0
            ? list.Last().Vol / (mergeBak.Float_share * 10000.0f)//亿股
            : 0.0f;
    }
    //获取量比值
    public static float GetQuantityRelativerRatioOnCurrent(this IList<CandlePoint> list)
    {
        var ma5Vol = list.Count >= 6
             ? list.SkipLast(1).TakeLast(5).Sum(n => n.Vol) / 5
             : list.Count >= 2
                 ? list.SkipLast(1).TakeLast(list.Count - 1).Sum(n => n.Vol) / (list.Count - 1)
                 : list.First().Vol;
        return list.Last().Vol / ma5Vol;
    }
    /// <summary>
    /// 取得区间的K线各类值
    /// </summary>
    /// <param name="days"></param>
    /// <param name="basicStock"></param>
    /// <returns></returns>
    public static (bool HasValue, TrendIndexInPeriod IndexValue)
        GetTrendIndexsInLastDays(this IList<CandlePoint> list, int days)
    {
        var candles = list.TakeLast(days);
        if (!candles.Any())
        {
            return (false, null);
        }
        var current = candles.Last();
        var maxClose = candles.Max(n => n.Close);
        var minClose = candles.Min(n => n.Close);
        var indexMax = candles.Last(n => n.Close == maxClose).LocationIndex;
        var indexMin = candles.Last(n => n.Close == minClose).LocationIndex;
        var indextotal = candles.Last().LocationIndex;
        //10个交易日期红色进攻倍量
        int countDoubleVol = candles.TakeLast(10)
                            .Where(n => n.IsDoubleVol && n.IsCloseUpOpen).Count();
        TrendIndexInPeriod tmpindex = new()
        {
            StockCode = current.StockCode,
            StockName = current.Name,
            Current = current.Close,
            Max = maxClose,
            Min = minClose,
            MaxIdx = indexMax,
            MinIdx = indexMin,
            WaveRate = minClose / maxClose,
            RatioCurrToMax = current.Close / maxClose,
            DaysToMax = indextotal - indexMax,
            DaysToMin = indextotal - indexMin,
            DoubleVolsWithin20Days = countDoubleVol
        };
        return (true, tmpindex);
    }
    /// <summary>
    /// 取得最近的主要财务指标
    /// </summary>
    /// <param name="basicStock"></param>
    /// <returns></returns>
    public static (bool HasValue, FinancialIndex IndexValue)
        GetLastFinaIndex(this IList<FinancialIndex> list)
    {
        if (!list.Any())
        {
            return (false, null);
        }

        return (true, list.Last());
    }
    /// <summary>
    /// 近期最近高拐点
    /// </summary>
    /// <param name="inflectPointOfPrices"></param>
    /// <returns></returns>
    public static (bool HasValue, TopInflectPoint IndexValue)
        GetLastTopPoint(this IList<InflectPoint> list)
    {
        var tops = list.TakePoints(InflectPType.Top);
        if (!tops.Any()) { return (false, null); }
        var lastTop = tops.Last();
        var current = list.Last();
        TopInflectPoint topPoint = new()
        {
            LocationIndex = lastTop.LocationIndex,
            PointValue = lastTop.Close,
            GrowToCurrent = current.Close / lastTop.Close,
            DaysToCurrent = current.LocationIndex - lastTop.LocationIndex,
        };
        //附近是否三日内有涨停
        topPoint.IsSkyRocketing = lastTop.LocationIndex switch
        {
            0 => CandlePoints[0].IsSkyrocketing,
            1 => CandlePoints[1].IsSkyrocketing
                 || CandlePoints[0].IsSkyrocketing,
            2 => CandlePoints[2].IsSkyrocketing
                 || CandlePoints[1].IsSkyrocketing
                 || CandlePoints[0].IsSkyrocketing,
            >= 3 => CandlePoints[topPoint.LocationIndex - 3].IsSkyrocketing
                 || CandlePoints[topPoint.LocationIndex - 2].IsSkyrocketing
                 || CandlePoints[topPoint.LocationIndex - 1].IsSkyrocketing
                 || CandlePoints[topPoint.LocationIndex].IsSkyrocketing,
            _ => false,
        };
        return (true, topPoint);
    }
    /// <summary>
    /// Days内Ma5和Ma20线金叉次数，最近金叉距离目前交易日
    /// </summary>
    /// <param name="withinDays"></param>
    /// <returns></returns>
    public static (int CountGoldCross, int DaysToCurrent)
        AnalyGoldCrossMa5toMa20InDays(this IList<CandlePoint> list, int withinDays)
    {
        float[] inSeries = list.TakeCloseArray();
        MaOfKline.ma5?.Clear();
        MaOfKline.ma5 = inSeries.CalculateMAVal(5);
        MaOfKline.ma20?.Clear();
        MaOfKline.ma20 = inSeries.CalculateMAVal(20);
        int countGoldCross = 0;
        int daysToCurrent = -1;
        if (!MaOfKline.ma20.Any()) { return (0, -1); }
        (int stratIndex, int endIndex) = (MaOfKline.ma5.Count - 1,
              MaOfKline.ma5.Count > withinDays ? MaOfKline.ma5.Count - withinDays : 0);
        for (int i = stratIndex; i > endIndex; i--)
        {
            var valSection = (
                  MaOfKline.ma5[i].MAValue - MaOfKline.ma20[i].MAValue,
                  MaOfKline.ma5[--i].MAValue - MaOfKline.ma20[--i].MAValue);
            //形成金叉
            if (valSection.Item1 >= 0 && valSection.Item2 < 0)
            {
                daysToCurrent = countGoldCross++ is 0 ? MaOfKline.ma5.Count - i : daysToCurrent;
            }
        }
        return (countGoldCross, daysToCurrent);
    }
    /// <summary>
    /// 根据九宫格个价格分布判断走势
    /// </summary>
    /// <param name="inflectPoints"></param>
    /// <returns></returns>
    public static (string TrendLable, string TrendDescribe)
        AnalyTrendOfKline(this IList<InflectPoint> samples)
    {
        if (!samples.Any()) { return ("?", ""); }
        float maxClose = samples.Max(n => n.Close);
        float minClose = samples.Min(n => n.Close);
        float divMaxtoMin = maxClose - minClose;
        int idxsOfSect = samples.Count / 6;
        var valSectMean = (
            samples.Take(idxsOfSect).Sum(n => n.Close) / idxsOfSect,
            samples.Skip((samples.Count - idxsOfSect) / 2).Take(idxsOfSect).Sum(n => n.Close) / idxsOfSect,
            samples.TakeLast(idxsOfSect).Sum(n => n.Close) / idxsOfSect);
        //形成价格的相对位置幅度，标准化每个股票价格波动
        var valSectPercent = (
             (valSectMean.Item1 - minClose) / divMaxtoMin,
             (valSectMean.Item2 - minClose) / divMaxtoMin,
             (valSectMean.Item3 - minClose) / divMaxtoMin);
        //标准化各区域值：价格在顶部1，中部0，底部-1
        var valSectStandard = CommCalculate.RevisedValOnStandard(valSectPercent);
        // /:上行；\\:下行；L：回调振荡；F：冲高振荡；W：振荡；V：V型回转；A：A型小调   \\:深度下调破位
        var analyTrend = valSectStandard switch
        {
            (1, 1, 1) => ("F", "顶部持续振荡"),
            (1, 1, 0) => ("L", "顶部振荡回调"),
            (1, 1, -1) => ("L \\", "顶部振荡深调"),

            (1, 0, 1) => ("V F", "顶部回调冲高"),
            (1, 0, 0) => ("V L F", "顶部回调振荡"),
            (1, 0, -1) => ("L \\", "顶部持续回调"),

            (1, -1, 1) => ("V F", "顶部深调冲高"),
            (1, -1, 0) => ("V L", "顶部深调回升"),
            (1, -1, -1) => ("L \\", "顶部深调振荡"),
            //
            (0, 1, 1) => ("F", "中部持续上行"),
            (0, 1, 0) => ("A F", "中部上行振荡"),
            (0, 1, -1) => ("A L", "中部上行回调"),

            (0, 0, 1) => ("F", "中部振荡上行"),
            (0, 0, 0) => ("W F", "中部持续振荡"),
            (0, 0, -1) => ("W L", "中部振荡下行"),

            (0, -1, 1) => ("V F", "中部下调回升"),
            (0, -1, 0) => ("V F", "中部下调振荡"),
            (0, -1, -1) => ("L", "中部持续下调"),
            //
            (-1, 1, 1) => ("F", "底部持续走高"),
            (-1, 1, 0) => ("A F", "底部走高振荡"),
            (-1, 1, -1) => ("A L", "底部走高下行"),

            (-1, 0, 1) => ("F", "底部持续走高"),
            (-1, 0, 0) => ("A F", "底部回升振荡"),
            (-1, 0, -1) => ("A L", "底部回升下行"),

            (-1, -1, 1) => ("W F", "底部振荡冲高"),
            (-1, -1, 0) => ("W F", "底部振荡回升"),
            (-1, -1, -1) => ("W L", "底部持续振荡"),
            _ => ("?", ""),
        };
        return (analyTrend.Item1, $"趋势:{ analyTrend.Item2}/{ analyTrend.Item1}");
        //$"Δ三区段位置:  {valSectPercent.Item1:P1}/{valSectPercent.Item2:P1}/{valSectPercent.Item3:P1}" +
        // $";    Δ三区段标值:  {valSectStandard.Item1}/{valSectStandard.Item2}/{valSectStandard.Item3}" +
        //$";   趋势:  {analyTrend.Item2}   {analyTrend.Item1}");
    }
    /// <summary>
    /// 分析底部波动，是否触底三次以上
    /// </summary>
    /// <param name="inflectPointOfPrices"></param>
    /// <returns></returns>
    public static (bool IsThanThreeHitBottom, int CountHitBottomLessThan10P)
        AnalyWaveOnBottomPoints(this IList<InflectPoint> list)
    {
        var bottoms = list.TakeCloseArray(InflectPType.Bottom);
        if (bottoms.Length < 3) { return (false, 0); }
        var current = list.Last().Close;//近期的收盘价
                                        //如果股价在创新低，或者涨幅超过20%，则不形成底部振荡
        if (current / bottoms.Min() is < 0.90f or > 1.20f)
        {
            return (false, 0);
        }

        int countHitBottom = 0;
        //底部波动：每个底部点与最近的底部点比较，如果波动幅度不超过15%，认为在底部波动
        for (int i = 1; i <= bottoms.Length; i++)
        {
            float waveRateofAdjacentPoints = MathF.Abs(bottoms.Last() / bottoms[^i] - 1.0f);
            if (waveRateofAdjacentPoints <= 0.15f) { countHitBottom++; }
            else
            {
                break;
            }
        }
        //底部拐点至少3次回调，可以初步认定为在底部振荡
        return (countHitBottom >= 3, countHitBottom);
    }
}




