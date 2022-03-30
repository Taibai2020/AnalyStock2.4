using static AnalyStock.DataManage.StaticCollection;
namespace AnalyStock.DataManage;

internal record struct CandlesVolColor
(
    SolidColorBrush RaisePrice,
    SolidColorBrush DownPrice,
    SolidColorBrush RaiseVolume,
    SolidColorBrush DownVolume,
    SolidColorBrush SkyrocketingPrice
);
internal class CandlePointProperty
{
    internal static bool IsAdj { get; set; } = true;  //是否复权
    internal static int AdjType { get; set; } //0:前复权，1后复权
    internal static string PeriodType { get; set; } = "dailys"; //"weeks","months"
    internal static int IndexOfColorType { get; set; } = 0;
    internal static float DoubleVolValue { get; set; } = 1.85f; //交易量大于上日交易量的倍值    
    protected static readonly CandlesVolColor[] CandleVolColors = new CandlesVolColor[]
    {
      new ( Brushes.Red, Brushes.DarkTurquoise, Brushes.DarkOrange, Brushes.MediumTurquoise, Brushes.Orange), //ColorTypeNo=0                 
      new ( Brushes.Crimson, Brushes.DarkCyan, Brushes.DarkSalmon, Brushes.DarkCyan, Brushes.DarkOrange),
      new ( Brushes.DarkOrange, Brushes.SteelBlue, Brushes.DarkOrange, Brushes.SteelBlue, Brushes.Crimson)
    };
    //{new SolidColorBrush(Color.FromRgb(230,1,1))
}

internal class CandlePointData : CandlePointProperty
{   /// <summary>
    /// 蜡烛数据生成
    /// </summary>
    /// <param name="infodaily"></param>
    internal static void CreatCandlePoints(IList<Infodaily> infodaily)
    {
        if (!infodaily.Any()) { return; }
        IniCandlePoints(); //设置配色方案
        ConvertoCandleData(infodaily); //加载日交易数据到蜡烛数据集合
        if (IsAdj) { ConvertoAdjustPowerData(); }  //复权化交易数据集合
        if (PeriodType is "weeks" or "months")
        {
            CandlePoints = ConvertoPeriodData();//转化周线或月线交易数据集合 
        }
    }
    private static void IniCandlePoints()
    {  //初始化数据集合

        CandlePoints ??= new(); CandlePoints?.Clear();
    }
    private static void ConvertoCandleData(IList<Infodaily> infodaily)
    {
        var lstTradeData = infodaily.OrderBy(n => n.Trade_date);
        int index = 0;
        bool isCloseUpOpen;
        float lastVol = lstTradeData.First().Vol;  //计算交易量倍量需要保存上一日的交易量
        foreach (var item in lstTradeData)
        {
            isCloseUpOpen = item.Close >= item.Open;
            CandlePoints.Add(new CandlePoint()
            {
                LocationIndex = index++,  //索引
                Week = GetWeekIndexInYear(item.Trade_date),
                Month = item.Trade_date[..6],
                StockCode = item.Ts_code, //0 :名称                   
                Date = item.Trade_date, //1:日期
                Open = item.Open, //2:open
                High = item.High, //3:high
                Low = item.Low,  //4:low
                Close = item.Close,//5:close
                Vol = item.Vol,  //6:vol
                Pctchg = item.Pct_chg,
                Change = item.Change,
                AdjustPowerFactor = item.Adj_factor, //7:adj 
                IsCloseUpOpen = isCloseUpOpen,
                IsDoubleVol = item.Vol / lastVol >= DoubleVolValue,
                IsSkyrocketing = item.Pct_chg >= 9.95f,
                ColorCandle = isCloseUpOpen ? CandleVolColors[IndexOfColorType].RaisePrice
                                            : CandleVolColors[IndexOfColorType].DownPrice,
                ColorVol = isCloseUpOpen ? CandleVolColors[IndexOfColorType].RaiseVolume
                                         : CandleVolColors[IndexOfColorType].DownVolume,
                //item.Pct_chg >= 9.95f ? CandleVolColor.SkyrocketingPrice 

            });
            lastVol = item.Vol;
        }

    }
    //价格复权
    private static void ConvertoAdjustPowerData()  //0:前复权，1后复权
    {
        float adjustFactor = (AdjType is 0) ? CandlePoints.Last().AdjustPowerFactor : 100.0f;
        foreach (var p in CandlePoints)
        {
            p.Open = p.Open * p.AdjustPowerFactor / adjustFactor;
            p.High = p.High * p.AdjustPowerFactor / adjustFactor;
            p.Low = p.Low * p.AdjustPowerFactor / adjustFactor;
            p.Close = p.Close * p.AdjustPowerFactor / adjustFactor;
        }

    }
    ///判断每日交易日属于期间内的编号周次   
    private static string GetWeekIndexInYear(string trade_date)
    {
        DateTime tradeDate = DateTime.ParseExact(trade_date, "yyyyMMdd",
                                          System.Globalization.CultureInfo.CurrentCulture);
        DateTime startDate = DateTime.ParseExact($"{trade_date[..4]}0101", "yyyyMMdd",
                                          System.Globalization.CultureInfo.CurrentCulture);

        int dayOfWeekFirstday = (int)startDate.DayOfWeek;
        DateTime mondayOfFirstweek = startDate.AddDays(1 - dayOfWeekFirstday);//确定当日周一日期
        int no_Weeks = (tradeDate - mondayOfFirstweek).Days / 7;
        string strNoOfWeeks = (no_Weeks < 10) ? $"0{no_Weeks}" : $"{no_Weeks}";
        return $"{trade_date[..4]}{strNoOfWeeks}";
    }
    /// <summary>
    /// 转换周月线数据
    /// </summary>
    /// <param name="typeofperiod"></param>
    /// <returns></returns>
    #region  
    ///获取周或月的编号列表
    private static IEnumerable<string> GetPeriodArray()
    {
        return PeriodType is "weeks"
            ? CandlePoints.Select(candle => candle.Week).OrderBy(n => n).Distinct()
            : CandlePoints.Select(candle => candle.Month).OrderBy(n => n).Distinct();
    }
    /// <summary>
    /// 获取特定特定周线或月线的交易数据
    /// </summary>
    /// <param name="noofperiod"></param>
    /// <param name="typeofperiod"></param>
    /// <returns></returns>
    private static IEnumerable<CandlePoint> GetCandleSeriesofSingleNoofPeriod(string noofperiod)
    {
        return PeriodType is "weeks"
            ? CandlePoints.Where(n => n.Week == noofperiod).OrderBy(n => n.Date)
            : CandlePoints.Where(n => n.Month == noofperiod).OrderBy(n => n.Date);
    }
    private static List<CandlePoint> ConvertoPeriodData()
    {
        List<CandlePoint> lstCandleSerOfPeriod = new();
        //获取周编号的值，提出重复项
        var arrayOfPeriodNo = GetPeriodArray();
        //索引重编号，因为节假日有时一周没有交易，根据时间计算的周编号会出现编号不连续
        int index = 0;
        float lastVol = 1.00f;  //计算交易量倍量需要保存上一日的交易量
        foreach (string noOfPeriod in arrayOfPeriodNo)
        {
            var sectionOfCandlelist = GetCandleSeriesofSingleNoofPeriod(noOfPeriod);
            CandlePoint tmpCandleSer = new()
            {
                LocationIndex = index++,
                StockCode = sectionOfCandlelist.First().StockCode,
                Date = sectionOfCandlelist.Last().Date,
                Open = sectionOfCandlelist.First().Open,
                Close = sectionOfCandlelist.Last().Close,
                High = sectionOfCandlelist.Max(n => n.High),
                Low = sectionOfCandlelist.Min(n => n.Low),
                Vol = sectionOfCandlelist.Sum(n => n.Vol),
                Week = sectionOfCandlelist.Last().Week,
                Month = sectionOfCandlelist.Last().Month,
                IsSkyrocketing = false
            };
            tmpCandleSer.IsCloseUpOpen = tmpCandleSer.Close >= tmpCandleSer.Open;
            tmpCandleSer.IsDoubleVol = index > 1 && tmpCandleSer.Vol / lastVol >= DoubleVolValue;
            tmpCandleSer.ColorCandle = tmpCandleSer.IsCloseUpOpen ? CandleVolColors[IndexOfColorType].RaisePrice
                                                                  : CandleVolColors[IndexOfColorType].DownPrice;
            tmpCandleSer.ColorVol = tmpCandleSer.IsCloseUpOpen ? CandleVolColors[IndexOfColorType].RaiseVolume
                                                               : CandleVolColors[IndexOfColorType].DownVolume;
            tmpCandleSer.Pctchg = lstCandleSerOfPeriod.Any() ? (tmpCandleSer.Close / lstCandleSerOfPeriod.Last().Close - 1.0f) : 0.00f;
            lastVol = tmpCandleSer.Vol;
            lstCandleSerOfPeriod.Add(tmpCandleSer);
        }
        return lstCandleSerOfPeriod;
    }
    #endregion
}

