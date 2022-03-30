using static AnalyStock.DataManage.HttpClientUtility;
namespace AnalyStock.DataManage;

public static class UtilityOfQuotesMoney
{
    /// <summary>
    /// 获取股票的历史财务指标数据
    /// </summary>
    /// <param name="stockcode"></param>
    /// <returns></returns>
    public static async Task<DataTable> GetFinancialInfor(string stockcode)
    {
        var uri = SelecAddress("FinancialIndex", new string[] { stockcode[..6], "", "" });
        var results = await HttpClientPostAsync(uri, null).ConfigureAwait(false);
        if (string.IsNullOrEmpty(results)) { throw new Exception(results); }
        return results.ConvertToDataTable();
    }
    /// <summary>
    /// 获取及时的日线交易数据
    /// </summary>
    /// <param name="stockcode"></param>
    /// <returns></returns>
    public static async Task<CurrentInforOfQutoes> GetCurrentInfors(string stockcode)
    {
        var uri = SelecAddress("CurrentInfor", new string[] { stockcode[..6], "", "" });
        var results = await HttpClientPostAsync(uri, null).ConfigureAwait(false);
        if (string.IsNullOrEmpty(results)) { throw new Exception(results); }
        int idxSplitStart = results.IndexOf(":{") + 1;
        int idxSplitEnd = results.LastIndexOf("})");
        return  JsonSerializer.Deserialize<CurrentInforOfQutoes>(results[idxSplitStart..idxSplitEnd]);        
    }
    // param string stockcode, string startdate,string enddate
    private static Uri SelecAddress(string typeOfclassify, string[] stockparam)
    {
        //深圳 1 沪市6开头 0 
        string codeOfMarket = (stockparam[0][..1] == "6") ? "0" : "1";
        return new Uri(
            typeOfclassify switch
            {
                "HistoryInfor" => "http://quotes.money.163.com/service/chddata.html?"
                                + $"code={codeOfMarket + stockparam[0]}"
                                + $"&start={stockparam[1]}"
                                + $"&end={stockparam[2]}"
                                + $"&fields=TCLOSE;HIGH;LOW;TOPEN;LCLOSE;CHG;PCHG;VOTURNOVER;VATURNOVER",
                "FinancialIndex" => $"http://quotes.money.163.com/service/zycwzb_{stockparam[0]}.html?type=report",
                "CurrentInfor" => $"http://api.money.126.net/data/feed/{codeOfMarket + stockparam[0]},money.api?",
                _ => throw new NotImplementedException(),
            });
    }
    /// <summary>
    /// 处理行文本格式数据到数据表格式
    /// </summary>
    /// <param name="resResult"></param>
    /// <returns></returns>
    private static DataTable ConvertToDataTable(this string resResult)
    {
        string[] strLines = resResult.Replace("\t", "")
                                     .Replace("--", "0")//处理空值为0
                                     .Replace("-", "")  //处理日期格式YYYY-MM-dd为yyyyMMdd
                                     .Trim()
                                     .Split("\r\n", StringSplitOptions.None);
        int lineNo = 0;//行编号
        DataTable dataTable = new();
        foreach (var itemLine in strLines)
        {
            string[] cellValue = itemLine.Trim().Split(",", StringSplitOptions.None);
            dataTable.Columns.Add(cellValue[0]);  //第一列是field名            
            for (int i = 1; i < cellValue.Length; i++)//后面列是每期的值,i为各期值编号
            {
                if (lineNo == 0)//如果是第一个指标，一次性增加所有数据行
                {
                    if (!string.IsNullOrWhiteSpace(cellValue[i]))//防止有些空值行  
                    {
                        var newDr = dataTable.NewRow();
                        newDr[lineNo] = cellValue[i];
                        dataTable.Rows.Add(newDr);
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(cellValue[i]))
                    {
                        dataTable.Rows[i - 1][lineNo] = cellValue[i];
                    }
                }
            }
            lineNo++;
        }
        return dataTable;
    }
    
   
}

