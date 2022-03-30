namespace AnalyStock.DataManage;
internal class ErrorLogPool
{    //失败记录集合
    internal static List<ErrorLog> ErrorLogs { get; set; }
    /// <summary>
    /// 失败记录导入
    /// </summary>
    /// <param name="ProcessName"></param>
    /// <param name="stockcode"></param>
    /// <param name="tradedate"></param>
    /// <param name="errmessage"></param>
    public static void LogErrorMessage(string ProcessName, string stockCode, string tradeDate, string errMessage)
    {
        ErrorLogs.Add(new()
        {
            ProcessName = ProcessName,
            ErrLogTime = DateTime.Today.ToString("f"),
            StockCode = stockCode,
            TradeDate = tradeDate,
            ErrMessage = errMessage
        });
    }
}
//数据处理过程中的错误记录类
public class ErrorLog
{
    public string ProcessName { get; set; }
    public string ErrLogTime { get; set; }
    public string StockCode { get; set; }
    public string TradeDate { get; set; }
    public string ErrMessage { get; set; }
}
