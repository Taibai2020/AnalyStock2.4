using AnalyStock.DataAnalysis;
using AnalyStock.DataManage;
using static AnalyStock.DataManage.StaticCollection;
namespace AnalyStock.GrapImage;

public partial class ChartCandlePage
{
    public bool IsPricePanel { get; set; } = true;
    public bool IsMa2line { get; set; }
    public bool IsMa4line { get; set; }
    public bool IsInflectPoint { get; set; }
    public bool IsTrendLine { get; set; }
    public bool IsDoubleVolLine { get; set; }

    /// <summary>
    /// 画价格拐点线或趋势线
    /// </summary>
    /// <param name="typeLine"></param>
    /// 
    private void PaintMutTrendLine()
    {
        if (IsInflectPoint)
        {
            //生成K线拐点集合
            InflectPoints = CreatPaintPoints.CreateInflectPointsByClose().ToList();
            PaintSingleTrendLine(Brushes.Aqua);
        }

        if (IsTrendLine)
        {
            //生成K线模拟模型点集合
            InflectPoints = CreatPaintPoints.CreateFittingPointsBySample().ToList();
            PaintSingleTrendLine(Brushes.Orange);
            PaintTrendExpression();
        }

        if (IsDoubleVolLine)
        {
            //生产K线倍量点集合
            InflectPoints = CreatPaintPoints.CreateDoubleVolPointsByLow().ToList();
            PaintSingleTrendLine(Brushes.Orange);
        }
        InflectPoints?.Clear();
    }

    /// <summary>
    /// 根据点集合画出趋势线
    /// </summary>
    /// <param name="brushLine"></param>
    private void PaintSingleTrendLine(Brush brushLine)
    {
        if (InflectPoints.Count < 2) { return; }//点数小于两个引起画线函数异常 
        var polyline = new Polyline()
        {
            Stroke = brushLine,
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection { 4, 4 },
        };
        double XPoint, YPoint;
        foreach (var signPoint in InflectPoints)
        {
            XPoint = GetXOfLocation(signPoint.LocationIndex, true);
            YPoint = GetYOfLocation(signPoint.PointValue, true);
            polyline.Points.Add(new Point(XPoint, YPoint));
        }
        _ = DrawCanvasTrendLine.Children.Add(polyline);
    }

    /// <summary>
    /// 显示模拟趋势方程模型
    /// </summary>
    private void PaintTrendExpression()
    {
        TextBlock textBlockFunction = new()
        {
            Text = $"{InflectPoints.AnalyTrendOfKline().TrendDescribe}",
            FontSize = 10,
            Foreground = Brushes.DarkGray,
        };
        Canvas.SetTop(textBlockFunction, -6);
        Canvas.SetLeft(textBlockFunction, 10);
        _ = DrawCanvasTrendLine.Children.Add(textBlockFunction);
    }
    /// <summary>
    /// 计算均线值，在面板上画出5，10，20，30，60日均线
    /// </summary>
    private void PaintMutMALine()
    {
        if (IsMa2line)
        {
            MaOfKline.ma5?.Clear();
            MaOfKline.ma30?.Clear();
            MaOfKline.ma5 = PaintMALine(5, true);
            MaOfKline.ma30 = PaintMALine(30, true);
        }
        if (IsMa4line)
        {
            MaOfKline.ma5?.Clear();
            MaOfKline.ma10?.Clear();
            MaOfKline.ma20?.Clear();
            MaOfKline.ma60?.Clear();
            MaOfKline.ma5 = PaintMALine(5, true);
            MaOfKline.ma10 = PaintMALine(10, true);
            MaOfKline.ma20 = PaintMALine(20, true);
            MaOfKline.ma60 = PaintMALine(60, true);
        }
        //交易量的平均线
        MaOfVol.ma5?.Clear();
        MaOfVol.ma10?.Clear();
        MaOfVol.ma5 = PaintMALine(5, false);
        MaOfVol.ma10 = PaintMALine(10, false);
    }
    /// <summary>
    ///
    /// </summary>
    /// <param name="ma_period"></param>
    /// <param name="isCandleOrVol"></param>
    /// <returns></returns>
    private List<MAPoint> PaintMALine(int ma_period, bool isCandleOrVol)
    {
        List<MAPoint> maSeries = CandlePoints
            .Select(n => isCandleOrVol ? n.Close : n.Vol).ToArray().CalculateMAVal(ma_period);
        if (maSeries.Count < 2) { return maSeries; }
        var polyline = new Polyline();
        foreach (var ma in maSeries)
        {
            polyline.Points.Add(
                new Point(
                    GetXOfLocation(ma.LocationIndex, true),
                    GetYOfLocation(ma.MAValue, isCandleOrVol)));
        }
        polyline.StrokeThickness = 1;
        polyline.Stroke = ma_period switch
        {
            5 => Brushes.DarkGray,
            10 => Brushes.DarkGreen,
            20 => Brushes.DarkOrange,
            30 => Brushes.IndianRed,
            60 => Brushes.DarkCyan,
            _ => Brushes.DarkGray,
        };
        DrawCanvasTrendLine.Children.Add(polyline);
        return maSeries;
    }

}

