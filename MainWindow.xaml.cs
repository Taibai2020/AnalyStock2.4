using AnalyStock.DataManage;
using System.Windows.Navigation;

namespace AnalyStock;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : NavigationWindow
{
    internal delegate void HidePanelEventHandler();
    internal static event HidePanelEventHandler EventHidePanel; //查询窗体关闭的事件委托
    public MainWindow()
    {
        InitializeComponent();
        Closing += new System.ComponentModel.CancelEventHandler(NavigationWindow_Closing);
        Unloaded += new RoutedEventHandler(NavigationWindow_Unloaded);
        StateChanged += new EventHandler(NavigationWindow_StateChanged);
    }

    private void NavigationWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (MessageBox.Show("是否退出系统?", "AnalyStock2.2",
            MessageBoxButton.OKCancel, MessageBoxImage.Question) is MessageBoxResult.OK)
        {
            if (SystemParam.IsBusyOfDownDataOnLine)
            {
                MessageBox.Show("数据导入工作没有完成，请停止后台导入后退出...");
                e.Cancel = true;
            }
            return;
        }
        e.Cancel = true;
    }

    private void NavigationWindow_Unloaded(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void NavigationWindow_StateChanged(object sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            EventHidePanel?.Invoke();//触发关闭价格面板和查询面板
        }
    }
}
