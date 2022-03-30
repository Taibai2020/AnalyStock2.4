using AnalyStock.DataAnalysis;
using AnalyStock.DataManage;

namespace AnalyStock.GrapImage;

/// <summary>
/// Window1.xaml 的交互逻辑
/// </summary>
public partial class QueryStockCode : Window
{
    private readonly List<StockNames> stocknamelst = new();
    public QueryStockCode(ref IList<MergeBakDaily> stockinfor, Point startPointPage)
    {
        InitializeComponent();
        foreach (var item in stockinfor)
        {
            stocknamelst.Add(new StockNames()
            {
                CodeWithName = $"{item.Ts_code},{item.Name}{CommCNSpell.GetChineseSpell(item.Name)}",
            });
        }
        Left = (-startPointPage.X) - Width;
        Top = (-startPointPage.Y) - Height;
    }

    private void TextBoxStockName_TextChanged(object sender, TextChangedEventArgs e)
    {
        DGStock.ItemsSource = stocknamelst.Where(n => n.CodeWithName.Contains(TextBoxStockName.Text));
    }

}

public class StockNames
{
    public string CodeWithName { get; set; }
}

