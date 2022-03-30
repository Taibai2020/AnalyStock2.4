using AnalyStock.DataAnalysis;
using AnalyStock.DataManage;
using static AnalyStock.DataManage.StaticCollection;
namespace AnalyStock.GrapImage;

/// <summary>
/// UserControl1.xaml 的交互逻辑
/// </summary>
public partial class ChartIndexPage : UserControl
{
    private double bottomPaintBox_left, bottomPaintBox_width, bottomPaintBox_high;
    public double MarginUnitPoint { get; set; } = 15;
    public int TotalIndexofPanle { get; set; }
    public bool IsKDJ { get; set; }
    public bool IsMACD { get; set; } = true;
    public bool IsEPS { get; set; }

    public ChartIndexPage()
    {
        InitializeComponent();
    }
    private void IniRectOfPanle()
    {
        bottomPaintBox_left = 3 * MarginUnitPoint;
        bottomPaintBox_high = ActualHeight - MarginUnitPoint;
        bottomPaintBox_width = ActualWidth - 4 * MarginUnitPoint;
        DrawCanvas.Children?.Clear();
    }
    public void PaintKIndexOnPeriod()
    {
        if (!CandlePoints.Any()) { return; }
        TotalIndexofPanle = CandlePoints.Count;
        TotalIndexofPanle = TotalIndexofPanle < 60 ? 60 : TotalIndexofPanle;
        IniRectOfPanle();
        if (IsKDJ) { PaintKDJLine(); }
        if (IsMACD) { PaintMACDLine(); }
        if (IsEPS) { PaintEPSColumn(); }
    }
    /// <summary>
    /// 画MACD线
    /// </summary>
    private void PaintMACDLine()
    {

        ///获取MACD数值
        List<MACDPoint> tmpMacdvalue =
            CandlePoints.Select(candle => candle.Close).ToArray().CalculateMACDVal();
        if (tmpMacdvalue.Count < 1) { return; }
        double maxDif = tmpMacdvalue.Max(macd => macd.DIFValue);
        double minDif = tmpMacdvalue.Min(macd => macd.DIFValue);
        ///以面板中线为零轴，平均划分面板的高度，取面板Y坐标=0，对应最大值，面板Y坐标=HIGH，对应最小值
        var maxDiforDea = System.Math.Abs(maxDif) > System.Math.Abs(minDif)
                          ? System.Math.Abs(maxDif) : System.Math.Abs(minDif);
        Line lineCenter = new()
        {
            X1 = 0,
            Y1 = bottomPaintBox_high / 2,
            X2 = ActualWidth,
            Y2 = bottomPaintBox_high / 2,
            Stroke = Brushes.Gray,
            StrokeThickness = 0.5
        };
        DrawCanvas.Children.Add(lineCenter);

        Polyline polylineDif = new()
        {
            Stroke = Brushes.Orange,
            StrokeThickness = 0.8,
            StrokeLineJoin = PenLineJoin.Round
        };
        Polyline polylineDea = new()
        {
            Stroke = Brushes.SkyBlue,
            StrokeThickness = 0.8,
            StrokeLineJoin = PenLineJoin.Round
        };
        double dif_x = 0;
        double dif_y = 0;
        double dea_y = 0;
        foreach (var item in tmpMacdvalue)
        {
            dif_x = bottomPaintBox_left + item.LocationIndex * bottomPaintBox_width / TotalIndexofPanle + bottomPaintBox_width / TotalIndexofPanle / 2;
            dif_y = bottomPaintBox_high / 2 * (1 - item.DIFValue / maxDiforDea);
            dea_y = bottomPaintBox_high / 2 * (1 - item.DEAValue / maxDiforDea);
            polylineDif.Points.Add(new Point(dif_x, dif_y));
            polylineDea.Points.Add(new Point(dif_x, dea_y));
            Line lineMacd = new()
            {
                X1 = dif_x,
                Y1 = bottomPaintBox_high / 2,
                X2 = dif_x,
                Y2 = bottomPaintBox_high / 2 * (1 - item.MACDValue / maxDiforDea),
                Stroke = Brushes.Orange,
                StrokeThickness = 0.5
            };
            DrawCanvas.Children.Add(lineMacd);
        }
        DrawCanvas.Children.Add(polylineDif); DrawCanvas.Children.Add(polylineDea);
    }
    /// <summary>
    /// 画KDj线
    /// </summary>
    /// <param name="K_period"></param>
    private void PaintKDJLine()
    {
        var tmpHigh = CandlePoints.Select(candle => candle.High).ToArray();
        var tmpLow = CandlePoints.Select(candle => candle.Low).ToArray();
        var tmpClose = CandlePoints.Select(candle => candle.Close).ToArray();
        List<KDJPoint> tmpKDJSeries = CommCalculate.CalculateKDJVal(9, 3, 3, tmpHigh, tmpLow, tmpClose);
        Line lineCenter = new()
        {
            X1 = 0,
            Y1 = bottomPaintBox_high / 2,
            X2 = ActualWidth,
            Y2 = bottomPaintBox_high / 2,
            Stroke = Brushes.Gray,
            StrokeThickness = 0.5
        };

        if (tmpKDJSeries.Count > 9)  //RSV的取值周期是9
        {
            Polyline polylineK = new()
            {
                Stroke = Brushes.Orange,
                StrokeThickness = 0.8,
                StrokeLineJoin = PenLineJoin.Round
            };
            Polyline polylineJ = new()
            {
                Stroke = Brushes.SkyBlue,
                StrokeThickness = 0.8,
                StrokeLineJoin = PenLineJoin.Round
            };
            Polyline polylineD = new()
            {
                Stroke = Brushes.DarkGreen,
                StrokeThickness = 0.8,
                StrokeLineJoin = PenLineJoin.Round
            };
            double kjd_x = 0;
            double k_y = 0;
            double j_y = 0;
            double d_y = 0;
            foreach (var item in tmpKDJSeries)
            {
                kjd_x = bottomPaintBox_left + item.LocationIndex * bottomPaintBox_width / TotalIndexofPanle + bottomPaintBox_width / TotalIndexofPanle / 2;
                k_y = (1 - item.KValue / 100) * bottomPaintBox_high;
                j_y = (1 - item.JValue / 100) * bottomPaintBox_high;
                d_y = (1 - item.DValue / 100) * bottomPaintBox_high;
                polylineK.Points.Add(new Point(kjd_x, k_y));
                polylineJ.Points.Add(new Point(kjd_x, j_y));
                polylineD.Points.Add(new Point(kjd_x, d_y));
            }
            DrawCanvas.Children.Add(lineCenter);
            DrawCanvas.Children.Add(polylineK);
            DrawCanvas.Children.Add(polylineJ);
            DrawCanvas.Children.Add(polylineD);
        }
    }
    /// <summary>
    /// 画EPS每股收益柱状图
    /// </summary>
    private void PaintEPSColumn()
    {
        var lstEPSPoints = CreatPaintPoints.CreatFinacialEPSPoints();
        if (!lstEPSPoints.Any()) { return; }
        var maxEps = lstEPSPoints.Max(n => n.EPS);
        var minEps = lstEPSPoints.Min(n => n.EPS);
        var highOfPaint = bottomPaintBox_high - 10.0f;
        var unitPointHigh = (maxEps != minEps) ? highOfPaint / (maxEps - minEps) : highOfPaint / maxEps;
        foreach (var item in lstEPSPoints)
        {
            var XPoint = bottomPaintBox_left + item.LocationIndex * bottomPaintBox_width / TotalIndexofPanle
                        + bottomPaintBox_width / TotalIndexofPanle / 2;
            var YPoint = (maxEps - item.EPS) * unitPointHigh + 10.0f;
            Rectangle rectEPS = new()
            {
                Fill = Brushes.SteelBlue,
                Width = 2,
                Height = (maxEps != minEps) ? (item.EPS - minEps) * unitPointHigh : highOfPaint,
            };
            Canvas.SetTop(rectEPS, YPoint);
            Canvas.SetLeft(rectEPS, XPoint);
            DrawCanvas.Children.Add(rectEPS);

            TextBlock textBlockEPS = new()
            {
                Text = $"<{item.EPS}>",
                FontSize = 9,
                Foreground = Brushes.Orange,
            };
            Canvas.SetTop(textBlockEPS, YPoint - 10);
            Canvas.SetLeft(textBlockEPS, XPoint - 12);
            DrawCanvas.Children.Add(textBlockEPS);
        }
    }

}
