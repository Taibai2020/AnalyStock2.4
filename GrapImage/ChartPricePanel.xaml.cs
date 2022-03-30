using static AnalyStock.DataManage.StaticCollection;
namespace AnalyStock.GrapImage;

/// <summary>
/// Window1.xaml 的交互逻辑
/// </summary>
/// 
public partial class PricePanel : Window
{

    public PricePanel(Point startPoint)
    {
        InitializeComponent();
        Left = -startPoint.X + 5.0f;
        Top = -startPoint.Y + 5.0f;
    }

    internal void DisplayPricePanel(int index)
    {
        DrawPricePanel.Children.Clear();
        //显示信息面板            
        TextBlock textBlockTradeInfo = new()
        {
            Text = $"日  期: {CandlePoints[index].Date}\n" +
                   $"开  盘: {CandlePoints[index].Open:0.00}\n" +
                   $"收  盘: {CandlePoints[index].Close:0.00}\n" +
                   $"最  高: {CandlePoints[index].High:0.00}\n" +
                   $"最  低: {CandlePoints[index].Low:0.00}\n" +
                   $"交易量: {CandlePoints[index].Vol:F0}\n" +
                   $"增  幅: {CandlePoints[index].Pctchg:0.0}%",
            Style = FindResource("TextBlockStyle") as Style,
            FontSize = 10,
        };
        Canvas.SetTop(textBlockTradeInfo, 6.0f);
        Canvas.SetLeft(textBlockTradeInfo, 2.0f);
        DrawPricePanel.Children.Add(textBlockTradeInfo);
    }
}


