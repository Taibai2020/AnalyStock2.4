using AnalyStock.DataManage;
using MathNet.Numerics;
using static AnalyStock.DataManage.CandlesExtend;
using static AnalyStock.DataManage.InflectPsExtend;
using static AnalyStock.DataManage.StaticCollection;
namespace AnalyStock.DataAnalysis;
/// <summary>
/// 填充各类基础数据集合，为批量分析准备
/// </summary>
#region
internal static class LoadDataCollection
{
    internal static bool IsFilltoBasicDatas(string stockCode) =>
        IsFilltoCandleSeries(stockCode) && IsFilltoInflectPoints() && IsFilltoFinancialIndex();
    internal static bool IsFilltoMergeBakdaily()
    {
        try
        {
            MergeBakDailes?.Clear();
            MergeBakDailes = (CommDataHelper<MergeBakDaily>
                .GetDataOnDbSetAsync().GetAwaiter().GetResult())
                .Where(n => !n.Name.Contains("ST")).ToList();//不含ST股
            //没有记录，抛出异常放弃执行
            if (!MergeBakDailes.Any())
            {
                throw new Exception("没有交易数据...");
            }
        }
        catch (Exception ex)
        {
            ErrorLogPool.LogErrorMessage("BakDailys", "all", "", ex.Message);
            return false;
        }
        return MergeBakDailes.Any();
    }
    internal static bool IsFilltoCandleSeries(string stockCode)
    {
        try
        {
            var infoDailies =  CommDataHelper<Infodaily>.GetDataOnDbSetAsync
                    ( stockCode,DateTime.Today.AddMonths(-6).ToString("yyyyMMdd"),
                    DateTime.Today.ToString("yyyyMMdd")).GetAwaiter().GetResult();
            //没有记录，抛出异常放弃执行
            if (!infoDailies.Any())
            {
                throw new Exception("没有交易数据...");
            }

            CandlePointProperty.IsAdj = true;
            CandlePointProperty.PeriodType = "dailys";
            CandlePointData.CreatCandlePoints(infoDailies);
        }
        catch (Exception ex)
        {
            ErrorLogPool.LogErrorMessage("Infordaily", stockCode, "", ex.Message);
            return false;
        }
        return CandlePoints.Any();
    }
    internal static bool IsFilltoInflectPoints()
    {
        InflectPoints?.Clear();
        InflectPoints = CreatPaintPoints.CreateInflectPointsByClose().ToList();        
        return InflectPoints.Any();
    }
    internal static bool IsFilltoFinancialIndex()
    {
        try
        {
            FinancialIndexs?.Clear();
            if (!CandlePoints.Any())
            {
                throw new Exception("蜡烛点的数据获取错误，终止...");
            }

            FinancialIndexs =(CommDataHelper<FinancialIndex>.GetDataOnDbSetAsync
                (CandlePoints[0].StockCode, CandlePoints[0].Date,
                CandlePoints[^1].Date).GetAwaiter().GetResult()).ToList();           
            //没有记录，抛出异常放弃执行
            if (!FinancialIndexs.Any())
            {
                throw new Exception("没有财务指标数据...");
            }
        }
        catch (Exception ex)
        {
            ErrorLogPool.LogErrorMessage("Financialindex", CandlePoints[0].StockCode, "", ex.Message);
            return false;
        }
        return FinancialIndexs.Any();
    }
}
#endregion

/// <summary>
/// 生成走势中各类点的点点值，K线拐点、模拟曲线的点值、倍量的点值、财务EPS点值
/// </summary>
internal class CreatPaintPoints
{
    private static readonly float rateWave = 0.05f;
    /// <summary>
    /// 获取价格的拐点数据，形成拐点系列值，依据价格变动的幅度：波动率
    /// </summary>
    /// <param name="candleSeries"></param>
    /// <param name="rateWave"></param>
    /// <returns></returns>
    public static IList<InflectPoint> CreateInflectPointsByClose()
    {
        List<InflectPoint> inflectPointOfPrices = new();
        if (CandlePoints.Count < 2) { return inflectPointOfPrices; }
        foreach (var item in
            CandlePoints.TakeCloseArray().CalcInflectPoint(rateWave))
        {
            inflectPointOfPrices.Add(new()
            {
                LocationIndex = item.Key,
                StockCode = CandlePoints[item.Key].StockCode,
                TradeDate = CandlePoints[item.Key].Date,
                Close = CandlePoints[item.Key].Close,
                PointValue = CandlePoints[item.Key].Close,
                TrendType = item.Value,
                PeriodType = "daily",
            });
        }
        return inflectPointOfPrices;
    }
    /// <summary>
    /// 获取拐点线的趋势线，模拟趋势方程为3次函数
    /// </summary>
    /// <param name="candleSeries"></param>
    /// <param name="rateWave"></param>
    /// <returns></returns>
    public static IList<InflectPoint> CreateFittingPointsBySample()
    {
        List<InflectPoint> samplePoints = new();
        if (CandlePoints.Count < 5)
        {
            return samplePoints;
        }

        var maxCloseCandle = CandlePoints.MaxBy(n => n.Close);
        var minCloseCandle = CandlePoints.MinBy(n => n.Close);
        var idxDivSection = CandlePoints.Count / 27.0f;//3个部分，每部分9个取值点
        int indexDiv = 0;
        for (int i = 0; i <= 27; i++)
        {
            indexDiv = i * idxDivSection < 2 ? 0 : (int)(i * idxDivSection) - 1;
            samplePoints.Add(new()
            {
                LocationIndex = indexDiv,
                StockCode = CandlePoints[indexDiv].StockCode,
                TradeDate = CandlePoints[indexDiv].Date,
                Close = CandlePoints[indexDiv].Close,
                PeriodType = "Daily"
            });
        }
        samplePoints.Add(new()
        {
            LocationIndex = maxCloseCandle.LocationIndex,
            StockCode = maxCloseCandle.StockCode,
            TradeDate = maxCloseCandle.Date,
            Close = maxCloseCandle.Close,
            PeriodType = "Daily"
        });
        samplePoints.Add(new()
        {
            LocationIndex = minCloseCandle.LocationIndex,
            StockCode = minCloseCandle.StockCode,
            TradeDate = minCloseCandle.Date,
            Close = minCloseCandle.Close,
            PeriodType = "Daily"
        });
        samplePoints = samplePoints
            .OrderBy(n => n.LocationIndex)
            .DistinctBy(n => n.LocationIndex).ToList();
        //线性模型           
        var paramsFitFunction = Fit.Polynomial(
               samplePoints.Select(n => (double)n.LocationIndex).ToArray(),
               samplePoints.Select(n => (double)n.Close).ToArray(), 3);
        samplePoints.ForEach(n =>
        {
            var (StrFunction, FittingValue, DiffValue)
               = CommCalculate.CalcFunctionValue(paramsFitFunction, n.LocationIndex);
            n.PointValue = FittingValue;
            n.DiffValue = DiffValue;
            n.FuncExpression = StrFunction;
            n.TrendType = "FittingFunction";
        });
        return samplePoints;
    }
    /// <summary>
    /// 获取交易量倍量点的所有点，当日交易最低价
    /// </summary>
    /// <param name="candleSeries"></param>
    /// <returns></returns>
    public static IList<InflectPoint> CreateDoubleVolPointsByLow()
    {
        List<InflectPoint> attackPointOfPrices = new();
        if (CandlePoints.Count < 2) { return attackPointOfPrices; }
        foreach (var item in CandlePoints.TakePoints(CandleType.DoubleVol))
        {
            attackPointOfPrices.Add(new()
            {
                LocationIndex = item.LocationIndex,
                StockCode = item.StockCode,
                TradeDate = item.Date,
                TrendType = "DoubleVol",
                Close = item.Close,
                PointValue = item.Low,
                PeriodType = "daily"
            });
        }
        return attackPointOfPrices;
    }
    /// <summary>
    /// 获取EPS的图像数据，及坐标索引
    /// </summary>
    /// <param name="candleSeries"></param>
    /// <returns></returns>
    public static IList<EPSPoint> CreatFinacialEPSPoints()
    {
        List<EPSPoint> epsPoints = new();
        if (!LoadDataCollection.IsFilltoFinancialIndex())
        {
            return epsPoints;
        }

        foreach (var (ReportDt, EPS) in FinancialIndexs.Select(n => (n.ReportDt, n.EPS)))
        {
            var candle = CandlePoints.Last(n => n.Month == ReportDt[..6]);
            if (candle != null)
            {
                epsPoints.Add(new()
                {
                    LocationIndex = candle.LocationIndex,
                    ReportDate = ReportDt,
                    EPS = EPS,
                });
            }
        }
        return epsPoints;
    }
}

///分析中使用个数据点位值或截面值，结构类    
#region
/// <summary>
/// 财务指标EPS的点位值
/// </summary>
public class EPSPoint
{
    public int LocationIndex { get; set; }
    public string ReportDate { get; set; }
    public float EPS { get; set; }
}

/// <summary>
/// K线走势中拐点的点位值
/// </summary>
public class InflectPoint
{
    public int LocationIndex { set; get; }//坐标轴上的索引位置，不是具体索引
    public string StockCode { set; get; }
    public string TradeDate { get; set; }
    public float Close { get; set; } //收盘价
    public string PeriodType { get; set; } //期间类型
    public string TrendType { get; set; } // "↓" 上部拐点，走势方向向下了；"↑"：下部拐点，走势方向向上
    public float PointValue { get; set; }    //选取点的收盘价或模型值       
    public float DiffValue { get; set; } //导数值，斜率；      
    public string FuncExpression { get; set; } //函数表达式       
}
/// <summary>
/// K线走势中顶部拐点的点位值
/// </summary>
public class TopInflectPoint
{
    public int LocationIndex { get; set; }
    public float PointValue { get; set; }  //该点的价格
    public float GrowToCurrent { get; set; } //该点到当前的价格增长幅度
    public bool IsSkyRocketing { get; set; }//是否涨停
    public int DaysToCurrent { get; set; } //该高点到当前的日期数
}

/// <summary>
/// 取得交易区间主要的趋势各类指标
/// </summary>
public class TrendIndexInPeriod
{
    public string StockCode { set; get; }  //股票码
    public string StockName { set; get; }
    public float Current { get; set; } //当前收盘价
    public float Max { get; set; }  //最高收盘价
    public float Min { get; set; } //最低收盘价        
    public float WaveRate { get; set; }//收盘价振荡幅度
    public float RatioCurrToMax { get; set; }   //收盘价距最高价比例，位置
    public int MaxIdx { get; set; }//最高价的索引
    public int MinIdx { get; set; }//最低价的索引                      
    public int DaysToMax { get; set; }   //当前距离最高价的交易日数
    public int DaysToMin { get; set; }//当前距离最低价的交易日数
    public int DoubleVolsWithin20Days { get; set; }//20日内的交易量倍量数
}
/// <summary>
/// 最近的财务指标值
/// </summary>
public class LastFinaIndex
{
    public float Eps { get; set; }//最近的EPS
    public float FloatMv { get; set; }//最近的流通市值
    public float TotalMv { get; set; }
}
#endregion

