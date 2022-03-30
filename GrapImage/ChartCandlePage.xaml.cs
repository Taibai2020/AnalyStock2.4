using AnalyStock.DataManage;
using static AnalyStock.DataManage.StaticCollection;
namespace AnalyStock.GrapImage;

/// <summary>
/// ChartStockPage.xaml 的交互逻辑
/// </summary>
public partial class ChartCandlePage : UserControl
{
    //交易量面板的位置高度
    private double volPaintPanel_top, volPaintPanel_high;
    // 画K线图界面的长宽设定        
    private double candlePaintPanel_top, candlePaintPanel_left, candlePaintPanel_width, candlePaintPanel_high;
    //作图每个索引占用的像素点
    private double pointsofUnitHigh, pointsofUnitWidth;
    private double pointsofCandleWidth, pointsofCandleUpDownLineWidth;
    //计算加载数据集中的最高价，最低价；
    private float maxHigh, minLow, maxVol;
    private int indexofMaxHigh, indexofMinLow;
    public double MarginUnitPoint { get; set; } = 15;
    public int TotalIndexofPanel { get; set; }
    public int CountofXline { get; set; } = 4;
    private PricePanel pricePanel;  //价格面板       
    public ChartCandlePage()
    {
        InitializeComponent();
        MouseDown += Content_MouseDown;
        Unloaded += Content_UnLoaded;
        MouseMove += Content_MouseMove;// new MouseEventHandler(Content_MouseMove);
    }
    public void PaintKlineOnPeriod()
    {
        if (!CandlePoints.Any()) { return; }
        IniRectOfPanel();
        IniUnitPointsOfEveryIndex();// 初始化每个索引的单位高度像素点，单位宽度像素点；
        PaintXScaleLine();       //画横轴的价格线
        PaintKLineSeries(); //画蜡烛图与交易量柱状图
        PaintMaxMinValue(); //画最高值，最低值
        PaintMutMALine();       //MAMutLinePaint(); //画交易量均线 ,根据isMaline标志画价格均线                                     
        PaintMutTrendLine();
    }
    /// <summary>
    /// 初始画面板的的高宽和左右起点位置，位置信息
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    /// <param name="height"></param>
    /// <param name="width"></param>
    private void IniRectOfPanel()
    {
        candlePaintPanel_top = MarginUnitPoint;
        candlePaintPanel_left = 3 * MarginUnitPoint;
        candlePaintPanel_high = ActualHeight * 3 / 4 - MarginUnitPoint;
        candlePaintPanel_width = this.ActualWidth - 4 * MarginUnitPoint;
        volPaintPanel_high = ActualHeight * 1 / 4; //1/4的高度用于显示交易量柱
        volPaintPanel_top = ActualHeight - volPaintPanel_high;
        DrawCanvasBack.Children.Clear();
        DrawCanvasTrendLine.Children.Clear();
        DrawCanvasFront.Children.Clear();
    }
    private void IniUnitPointsOfEveryIndex()
    {
        GetMaxMinValue(); //取得极值，用于换算作图的单位像素点；
        TotalIndexofPanel = CandlePoints.Count < 60 ? 60 : CandlePoints.Count;
        pointsofUnitHigh = (maxHigh != minLow)
            ? candlePaintPanel_high / (maxHigh - minLow)
            : candlePaintPanel_high / maxHigh; //以最低点为起点，单位价格的高度点
        pointsofUnitWidth = candlePaintPanel_width / TotalIndexofPanel; //单位索引的宽度点            
        pointsofCandleWidth = pointsofUnitWidth - 2.0f;
        pointsofCandleWidth = (pointsofCandleWidth < 0.5f) ? 0.5f : pointsofCandleWidth;
        pointsofCandleUpDownLineWidth = pointsofCandleWidth / 2.0f;
    }
    private void GetMaxMinValue()
    {
        var maxPoint = CandlePoints.MaxBy(n => n.High);
        maxHigh = maxPoint.High;
        indexofMaxHigh = maxPoint.LocationIndex;
        var minPoint = CandlePoints.MinBy(n => n.Low);
        minLow = minPoint.Low;
        indexofMinLow = minPoint.LocationIndex;
        maxVol = CandlePoints.Max(n => n.Vol);
    }
    private void PaintXScaleLine()
    {
        double unit_price = (maxHigh - minLow) / (CountofXline + 1);
        double y_Line;
        for (int i = 0; i <= CountofXline; i++)
        {
            y_Line = candlePaintPanel_top + candlePaintPanel_high - i * unit_price * pointsofUnitHigh;
            DrawCanvasBack.Children.Add(new Line()
            {
                X1 = candlePaintPanel_left,
                Y1 = y_Line,
                X2 = candlePaintPanel_left + candlePaintPanel_width,
                Y2 = y_Line,
                Stroke = Brushes.DarkGray,
                StrokeThickness = 0.2
            });
            TextBlock textBlock = new()
            {
                Text = $"{minLow + (i * unit_price):F2}",
                //FontFamily ="",
                FontSize = 9,
                Foreground = Brushes.DarkCyan
            };
            Canvas.SetTop(textBlock, y_Line - 15);
            Canvas.SetLeft(textBlock, 1);
            DrawCanvasBack.Children.Add(textBlock);
        }
    }
    /// <summary>
    /// 画连续的K线图
    /// </summary>       
    private void PaintKLineSeries()
    {
        ///在区间内画蜡烛线           
        foreach (CandlePoint singleCandle in CandlePoints)
        {
            PaintSingleCandle(singleCandle);
            PaintSingleVol(singleCandle);
        }
    }
    ///画单个蜡烛线形
    private void PaintSingleCandle(CandlePoint singleCandle)
    {
        var x_Candle = GetXOfLocation(singleCandle.LocationIndex, false);
        var x_UpdownLine = x_Candle + pointsofCandleUpDownLineWidth;
        var y_Candle_Open = GetYOfLocation(singleCandle.Open, true);
        var y_Candle_Close = GetYOfLocation(singleCandle.Close, true);
        var y_Candle_Up = GetYOfLocation(singleCandle.High, true);
        var y_Candle_Down = GetYOfLocation(singleCandle.Low, true);
        var high_Candle = Math.Abs(y_Candle_Open - y_Candle_Close);
        if (high_Candle < 2) { high_Candle = 2; }
        //画最高价和最低价的中影线
        DrawCanvasBack.Children.Add(new Line()
        {
            X1 = x_UpdownLine,
            Y1 = y_Candle_Up,
            X2 = x_UpdownLine,
            Y2 = y_Candle_Down,
            Stroke = singleCandle.ColorCandle,
            StrokeThickness = 1
        });
        //画开盘和收盘主体框
        Rectangle rectCandle = new()
        {
            Stroke = singleCandle.ColorCandle,
            StrokeThickness = singleCandle.IsDoubleVol ? 2.0 : 1,
            Fill = singleCandle.IsCloseUpOpen
                    ? new SolidColorBrush(
                        CommDataMethod.ConvertoColor(
                            ConfigurationManager.AppSettings["KlineBackground"]))
                    : singleCandle.ColorCandle,
            Width = pointsofCandleWidth,
            Height = high_Candle
        };

        Canvas.SetTop(rectCandle, singleCandle.IsCloseUpOpen ? y_Candle_Close : y_Candle_Open);
        Canvas.SetLeft(rectCandle, x_Candle);
        DrawCanvasBack.Children.Add(rectCandle);
        //涨停标识
        if (singleCandle.IsSkyrocketing)
        {
            var tbSkyRocketing = (
                 TextBlock: new TextBlock()
                 {
                     Text = "●",
                     FontSize = 9 * (pointsofUnitWidth / 8.0f),
                     Foreground = Brushes.DarkOrange
                 },
                 Location_x: x_UpdownLine - pointsofUnitWidth / 2.0f,
                 Location_y: y_Candle_Up - 12.0f * (pointsofUnitWidth / 8.0f));
            Canvas.SetTop(tbSkyRocketing.TextBlock, tbSkyRocketing.Location_y);
            Canvas.SetLeft(tbSkyRocketing.TextBlock, tbSkyRocketing.Location_x);
            DrawCanvasBack.Children.Add(tbSkyRocketing.TextBlock);
        }

    }
    ///画单个交易量柱状图
    private void PaintSingleVol(CandlePoint singleCandle)
    {
        var x_Vol = GetXOfLocation(singleCandle.LocationIndex, false);
        var y_Vol = GetYOfLocation(singleCandle.Vol, false);
        var high_Vol = ActualHeight - y_Vol;
        var width_Vol = pointsofCandleWidth;
        Rectangle rectVol = new()
        {
            Stroke = singleCandle.ColorVol,
            StrokeThickness = 0.5,
            Width = width_Vol,
            Height = high_Vol,
            Fill = singleCandle.IsDoubleVol ? singleCandle.ColorVol : null
        };
        Canvas.SetTop(rectVol, y_Vol - 4.0);
        Canvas.SetLeft(rectVol, x_Vol);
        DrawCanvasBack.Children.Add(rectVol);

    }
    //画最高值，最低值
    private void PaintMaxMinValue()
    {
        TextBlock textBlockMaxValue = new()
        {
            Text = $"<{maxHigh:F2}>",
            FontSize = 9,
            Foreground = Brushes.Red
        };
        Canvas.SetTop(textBlockMaxValue, GetYOfLocation(maxHigh, true) - 10);
        Canvas.SetLeft(textBlockMaxValue, GetXOfLocation(indexofMaxHigh, false) - 2 * pointsofUnitWidth);
        TextBlock textBlockMinValue = new()
        {
            Text = $"<{minLow:F2}>",
            FontSize = 9,
            Foreground = Brushes.LightSteelBlue
        };
        Canvas.SetTop(textBlockMinValue, GetYOfLocation(minLow, true));
        Canvas.SetLeft(textBlockMinValue, GetXOfLocation(indexofMinLow, false) - 2 * pointsofUnitWidth);
        _ = DrawCanvasBack.Children.Add(textBlockMaxValue);
        _ = DrawCanvasBack.Children.Add(textBlockMinValue);
    }
    private (string Close_MaValue, string Vol_MaValue) DispalyInforOfMaValue(int index)
    {
        string closeMaValue = "";
        if (IsMa2line)  //显示均值线
        {
            closeMaValue = $"ma5: { MaOfKline.ma5[index].MAValue:0.00}";
            closeMaValue += $" ma30: { MaOfKline.ma30[index].MAValue:0.00}";
        }
        if (IsMa4line)  //显示均值线
        {
            closeMaValue = $"ma5: { MaOfKline.ma5[index].MAValue:0.00}";
            closeMaValue += $" ma10: { MaOfKline.ma10[index].MAValue:0.00}";
            closeMaValue += $" ma20: { MaOfKline.ma20[index].MAValue:0.00}";
            closeMaValue += $" ma60: { MaOfKline.ma60[index].MAValue:0.00}";
        }
        string volMaValue = $"ma5: {MaOfVol.ma5[index].MAValue:F0}";
        volMaValue += $" ma10: {MaOfVol.ma10[index].MAValue:F0}";
        return (closeMaValue, volMaValue);
    }
    private void DisplayCrossLine(double location_x, double location_y)
    {
        if (TotalIndexofPanel == 0)
        {
            return;
        }

        int index = GetIndexOfLocation(location_x);
        if (index < 0 || index >= CandlePoints.Count)
        {
            return;
        }
        //显示X轴横向线
        DrawCanvasFront.Children.Add(new Line()
        {
            X1 = location_x,
            Y1 = 0,
            X2 = location_x,
            Y2 = candlePaintPanel_high,
            Stroke = Brushes.White,
            StrokeThickness = 0.2
        });
        DrawCanvasFront.Children.Add(new Line()
        {
            X1 = 0,
            Y1 = location_y,
            X2 = candlePaintPanel_width + candlePaintPanel_left,
            Y2 = location_y,
            Stroke = Brushes.Cyan,
            StrokeThickness = 0.2
        });

        //显示均值ma5 ma10 ma20    
        TextBlock textBlockCloseMa = new()
        {
            Text = DispalyInforOfMaValue(index).Close_MaValue,
            FontSize = 10,
            Foreground = Brushes.DarkGray,
        };
        Canvas.SetTop(textBlockCloseMa, -5);
        Canvas.SetLeft(textBlockCloseMa, 5);
        DrawCanvasFront.Children.Add(textBlockCloseMa);

        //显示交易量ma5 ma10
        TextBlock textBlockVolMa = new()
        {
            Text = DispalyInforOfMaValue(index).Vol_MaValue,
            FontSize = 10,
            Foreground = Brushes.DarkGray
        };
        Canvas.SetTop(textBlockVolMa, ActualHeight - volPaintPanel_high);
        Canvas.SetLeft(textBlockVolMa, 5);
        DrawCanvasFront.Children.Add(textBlockVolMa);
        //显示X轴的日期，和Y轴的价格指示
        double location_x_offset = location_x - location_x / candlePaintPanel_width * 50;
        Rectangle rectDateShadow = new()
        {
            Fill = Brushes.Peru,
            Width = 50,
            RadiusX = 2,
            RadiusY = 2,
            Height = 12
        };
        Canvas.SetTop(rectDateShadow, candlePaintPanel_high);
        Canvas.SetLeft(rectDateShadow, location_x_offset);
        DrawCanvasFront.Children.Add(rectDateShadow);

        var tradedate = CandlePoints[index].Date;
        TextBlock textBlockDate = new()
        {
            //yyyy-MM-dd
            Text = string.Join("-", new string[] { tradedate[..4], tradedate[4..6], tradedate[6..8] }),
            FontSize = 9,
            Foreground = Brushes.White
        };
        Canvas.SetTop(textBlockDate, candlePaintPanel_high);
        Canvas.SetLeft(textBlockDate, location_x_offset);
        DrawCanvasFront.Children.Add(textBlockDate);

        //显示价格
        double price = maxHigh - (location_y - candlePaintPanel_top) * (maxHigh - minLow) / candlePaintPanel_high;

        Rectangle rectPriceShadow = new()
        {
            Fill = Brushes.Peru,
            Width = 28,
            RadiusX = 2,
            RadiusY = 2,
            Height = 10
        };
        Canvas.SetTop(rectPriceShadow, location_y - 5);
        Canvas.SetLeft(rectPriceShadow, 0);
        DrawCanvasFront.Children.Add(rectPriceShadow);

        TextBlock textBlockPrice = new()
        {
            Text = $"{price:F2}",
            FontSize = 9,
            Foreground = Brushes.White
        };
        Canvas.SetTop(textBlockPrice, location_y - 5);
        Canvas.SetLeft(textBlockPrice, 0);
        DrawCanvasFront.Children.Add(textBlockPrice);

        ////显示增幅
        double raise = price / minLow - 1;
        Rectangle rectRaiseShadow = new()
        {
            Fill = Brushes.Peru,
            Width = 28,
            RadiusX = 2,
            RadiusY = 2,
            Height = 10
        };
        Canvas.SetTop(rectRaiseShadow, location_y - 5);
        Canvas.SetLeft(rectRaiseShadow, candlePaintPanel_left + candlePaintPanel_width - 7.0);
        DrawCanvasFront.Children.Add(rectRaiseShadow);
        TextBlock textBlockRaise = new()
        {
            Text = $"{raise:P1}",
            FontSize = 9,
            Foreground = Brushes.White
        };
        Canvas.SetTop(textBlockRaise, location_y - 5);
        Canvas.SetLeft(textBlockRaise, candlePaintPanel_left + candlePaintPanel_width - 5.0);
        DrawCanvasFront.Children.Add(textBlockRaise);

    }
    private void DisplayPricePanel(double location_x)
    {
        int index = GetIndexOfLocation(location_x);
        if (index < 0 || index >= CandlePoints.Count)
        {
            return;
        }

        pricePanel?.DisplayPricePanel(index);
    }
    private double GetYOfLocation(float valPriceorVol, bool isPriceorVol)
    {
        return isPriceorVol
            ? candlePaintPanel_top + (maxHigh - valPriceorVol) * pointsofUnitHigh
            : volPaintPanel_top + (1 - valPriceorVol / maxVol) * volPaintPanel_high;

    }
    private double GetXOfLocation(int location_index, bool isMidpoint)
    {
        //判断是否中间点
        return isMidpoint
            ? candlePaintPanel_left + location_index * pointsofUnitWidth + pointsofCandleUpDownLineWidth
            : candlePaintPanel_left + location_index * pointsofUnitWidth;
    }
    /// <summary>
    /// 通过鼠标在X点像素换算K线的位置索引；
    /// </summary>
    /// <param name="loaction_x"></param>
    /// <returns></returns>
    private int GetIndexOfLocation(double loaction_x)
    {
        return (int)((loaction_x - candlePaintPanel_left) * TotalIndexofPanel / candlePaintPanel_width);
    }
    /// <summary>
    /// 鼠标点击，移动显示面板位置
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 
    private void Content_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (pricePanel is null)
        {
            pricePanel = new(PointFromScreen(new Point(0, 0)));
            //窗口拖曳
            pricePanel.MouseDown += (object sender, MouseButtonEventArgs e) => pricePanel.DragMove();
            //窗口隐蔽
            pricePanel.MouseDoubleClick += (object sender, MouseButtonEventArgs e) => pricePanel.Hide();
            //外部委托句柄               
            MainWindow.EventHidePanel += () => pricePanel.Hide();
        }

        if (pricePanel.IsVisible) { pricePanel.Hide(); }
        else { pricePanel.Show(); }
    }
    /// <summary>
    /// 鼠标移动，显示面板信息和鼠标信息指示同步变化
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Content_MouseMove(object sender, MouseEventArgs e)
    {
        var uiPoint = e.GetPosition(sender as UIElement);
        if (TotalIndexofPanel == 0) { return; }
        if (uiPoint.Y <= (ActualHeight * 3 / 4 + MarginUnitPoint)
            && uiPoint.X > 3 * MarginUnitPoint
            && uiPoint.X < (ActualWidth - MarginUnitPoint))
        {
            DrawCanvasFront.Children.Clear();
            DisplayCrossLine(uiPoint.X, uiPoint.Y);
            DisplayPricePanel(uiPoint.X);
        }
    }
    private void Content_UnLoaded(object sender, RoutedEventArgs e)
    {
        pricePanel?.Hide();
    }

}

