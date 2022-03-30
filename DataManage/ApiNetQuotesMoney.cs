using static AnalyStock.DataManage.HttpClientUtility;
namespace AnalyStock.DataManage;

public static class UtilityOfQuotesMoney
{
    /// <summary>
    /// ��ȡ��Ʊ����ʷ����ָ������
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
    /// ��ȡ��ʱ�����߽�������
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
        //���� 1 ����6��ͷ 0 
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
    /// �������ı���ʽ���ݵ����ݱ��ʽ
    /// </summary>
    /// <param name="resResult"></param>
    /// <returns></returns>
    private static DataTable ConvertToDataTable(this string resResult)
    {
        string[] strLines = resResult.Replace("\t", "")
                                     .Replace("--", "0")//�����ֵΪ0
                                     .Replace("-", "")  //�������ڸ�ʽYYYY-MM-ddΪyyyyMMdd
                                     .Trim()
                                     .Split("\r\n", StringSplitOptions.None);
        int lineNo = 0;//�б��
        DataTable dataTable = new();
        foreach (var itemLine in strLines)
        {
            string[] cellValue = itemLine.Trim().Split(",", StringSplitOptions.None);
            dataTable.Columns.Add(cellValue[0]);  //��һ����field��            
            for (int i = 1; i < cellValue.Length; i++)//��������ÿ�ڵ�ֵ,iΪ����ֵ���
            {
                if (lineNo == 0)//����ǵ�һ��ָ�꣬һ������������������
                {
                    if (!string.IsNullOrWhiteSpace(cellValue[i]))//��ֹ��Щ��ֵ��  
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

