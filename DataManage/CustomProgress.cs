using System.Threading;
namespace AnalyStock.DataManage;


interface ICustomProgress
{
    bool IsCancellation { get; set; }
    bool IsBusy { get; set; }
    void Report(string taskNo, int progressPercentage, string userState);
}

public class CustomProgress : ICustomProgress
{
    private bool _isBusy;
    private bool _isCancelled;
    //public delegate void ProgressChangedEventHandler(ProgChangedEventArgs e);
    //public event ProgressChangedEventHandler ProgressChanged;
    public event EventHandler<MultitaskChangedEventArgs> ProgressChanged;
    private readonly SynchronizationContext _synchronizationContext;
    private readonly SendOrPostCallback _invokeHandlers;
    public bool IsCancellation
    {
        get => _isCancelled;
        set => _isCancelled = value;
    }
    public bool IsBusy
    {
        get => _isBusy;
        set => _isBusy = value;
    }
    public CustomProgress()
    {
        _synchronizationContext = SynchronizationContext.Current ?? new SynchronizationContext();
        _invokeHandlers = InvokeHandlers;
        IsCancellation = false;
        IsBusy = false;
    }
    protected virtual void OnChanged(MultitaskChangedEventArgs e)
    {
        //_synchronizationContext.Post((e)=>ProgressChanged(this, (MultitaskChangedEventArgs)e), e);
        _synchronizationContext.Post(_invokeHandlers, e);
    }
    void ICustomProgress.Report(string taskNo, int progressPercentage, string userState)
    {
        MultitaskChangedEventArgs e = new(taskNo, progressPercentage, userState);
        OnChanged(e);
    }
    private void InvokeHandlers(object e) => ProgressChanged?.Invoke(this, (MultitaskChangedEventArgs)e);
}

public class MultitaskChangedEventArgs : EventArgs
{
    private readonly int _progressPercentage;//进度百分值
    private readonly string _userState; //任务状态
    private readonly string _taskNo; //下载任务编号
    public int ProgressPercentage => _progressPercentage;
    public string UserState => _userState;
    public string TaskNo => _taskNo;
    public MultitaskChangedEventArgs(string taskNo, int progressPercentage, string userState)
    {
        _taskNo = taskNo;
        _progressPercentage = progressPercentage;
        _userState = userState;
    }
}

