using AnalyStock.DataAnalysis;
namespace AnalyStock.DataManage;

static class StaticCollection
{
    internal static List<CandlePoint> CandlePoints { get; set; }
    internal static List<InflectPoint> InflectPoints { get; set; }
    internal static List<MergeBakDaily> MergeBakDailes { get; set; }
    internal static List<FinancialIndex> FinancialIndexs { get; set; }
    //均线集合
    internal static (
        List<MAPoint> ma5,
        List<MAPoint> ma10,
        List<MAPoint> ma20,
        List<MAPoint> ma30,
        List<MAPoint> ma60) MaOfKline;
    internal static (
        List<MAPoint> ma5,
        List<MAPoint> ma10) MaOfVol;
    //选股的数据集合
    internal static List<StockKeyIndicators> SeleStockKeyIndicators { get; set; }
}

static class CandlesExtend
{
    //CandlePoints
    internal enum CandleType
    {
        DoubleVol,   //倍量
        RedDoubleVol,  //当日为红色，倍量
        GreenDoubleVol,
        SkyrocKeting,  //涨停
        All               //
    }
    static Func<CandlePoint, bool> FuncCondition(CandleType qtype)
        => qtype switch
        {
            CandleType.DoubleVol => p => p.IsDoubleVol,
            CandleType.RedDoubleVol => p => p.IsDoubleVol && p.IsCloseUpOpen,
            CandleType.SkyrocKeting => p => p.IsSkyrocketing,
            CandleType.All => n => true,
            _ => n => false,
        };

    internal static IEnumerable<CandlePoint> TakePoints(
       this IList<CandlePoint> list, CandleType qtype) => list.Where(FuncCondition(qtype));

    internal static IEnumerable<CandlePoint> TakeLastDayPoints(
        this IList<CandlePoint> list, CandleType qtype, int days) => list.TakeLast(days).Where(FuncCondition(qtype));

    internal static IEnumerable<CandlePoint> TakeLastDayPoints(
       this IList<CandlePoint> list, int days) => list.TakeLast(days);

    internal static float[] TakeCloseArray(
       this IList<CandlePoint> list, CandleType qtype) => list.Where(FuncCondition(qtype)).Select(n => n.Close).ToArray();

    internal static float[] TakeCloseArray(
      this IList<CandlePoint> list) => list.Select(n => n.Close).ToArray();
    internal static float[] TakeCloseArray(
        this IList<CandlePoint> list, Func<CandlePoint, bool> func) => list.Where(func).Select(n => n.Close).ToArray();
}

static class InflectPsExtend
{
    //  internal static IEnumerable<T> TakePoints<T>(this IList<T> list, Func<T, bool> func) => list.Where(func);
    //  internal static IEnumerable<object> TakePoints<T>(this IList<T> list, Func<T,object> func) => list.Select(func);
    //InflectPoints
    internal enum InflectPType
    {
        Top, Bottom, NotTop, NotBottom, NotStartEnd, Start, End, All //顶部点，底部点，起始点，结尾点，全部点
    }

    //static Expression<Func<InflectPoint, bool>> ExpFuncInflcetP(InflectPType qtype)
    //     => qtype switch
    //     {
    //         InflectPType.Top => p => p.TrendType == "↓",
    //         InflectPType.NotBottom => p => p.TrendType == "↑",   //包括顶点、StartP,EndP            
    //         InflectPType.Bottom => p => p.TrendType == "↑",
    //         InflectPType.NotTop => p => p.TrendType != "↓",
    //         InflectPType.NotStartEnd => p => p.TrendType != "start" || p.TrendType != "end",
    //         InflectPType.Start => p => p.TrendType == "start",
    //         InflectPType.End => p => p.TrendType == "end",
    //         InflectPType.All => p => true,
    //         _ => p => false
    //     };


    //Func的模式，会将全表提取到内存中执行，大型查询不建议使用
    static Func<InflectPoint, bool> FuncType(InflectPType qtype)
            => qtype switch
            {
                InflectPType.Top => p => p.TrendType is "↓", //顶点
                InflectPType.NotBottom => p => p.TrendType is not "↑",   //包括顶点、StartP,EndP            
                InflectPType.Bottom => p => p.TrendType is "↑",//底点
                InflectPType.NotTop => p => p.TrendType is not "↓", //包括底点，startP，EndP
                InflectPType.NotStartEnd => p => p.TrendType is not ("start" or "end"), //不包括初始和结尾点
                InflectPType.Start => p => p.TrendType is "start", //初始点
                InflectPType.End => p => p.TrendType is "end", //结尾点
                InflectPType.All => p => true,
                _ => p => false
            };

    internal static IEnumerable<InflectPoint> TakePoints(
                this IList<InflectPoint> list, InflectPType qtype) => list.Where(FuncType(qtype));

    internal static float[] TakeCloseArray(
        this IList<InflectPoint> list, InflectPType qtype) => list.Where(FuncType(qtype)).Select(n => n.Close).ToArray();

    internal static float[] TakeCloseArray(
        this IList<InflectPoint> list) => list.Select(n => n.Close).ToArray();

    internal static T1[] TakeArray<T1>(this IEnumerable<InflectPoint> ienum, Func<InflectPoint, T1> result)
    {
        return ienum.Select(result).ToArray();
    }

    internal static T1[] TakeArray<T, T1>(this IEnumerable<T> ienumT, T1 t)
    {
        static Func<T, T1> func(T1 t) => T => t;
        return ienumT.Select(func(t)).ToArray();
    }

}










