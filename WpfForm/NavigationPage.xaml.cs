using System.Windows.Navigation;

namespace AnalyStock.WpfForm;

public partial class NavigationPage : Page
{
    private LoadDataPage loadDataPage;
    private LookStockPage lookStockPage;
    private TitlePage titlePage;
    private SelectStockPage selectStock;

    public NavigationPage()
    {
        InitializeComponent();
    }
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        LoadPages(ref titlePage);
    }

    private void BtnLoadChilderPage_Click(object sender, RoutedEventArgs e)
    {
        switch (sender as Button)
        {
            case { Name: "BtnLoadData" }: LoadPages(ref loadDataPage); break;
            case { Name: "BtnLookStock" }: LoadPages(ref lookStockPage); break;
            case { Name: "BtnSelectStock" }: LoadPages(ref selectStock); break;
            case { Name: "BtnGoBack" }: LoadPages(ref titlePage); break;
            case { Name: "BtnExit" }: ExitApp(); break;
            default: break;
        }
    }

    private void ExitApp()
    {
        (Parent as NavigationWindow).Close();
    }

    private void LoadPages<T>(ref T t) where T : class, new()
    {
        t ??= new();
        MainFrame.NavigationService.Navigate(t);
    }
}


