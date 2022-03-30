using static AnalyStock.DataManage.HttpClientUtility;
namespace AnalyStock.DataManage;
/// <summary>
/// 封装对Tu Share API的调用
/// </summary>
public static class UtilityOfTuShare
{
    static readonly Uri baseUrl = new("http://api.waditu.com/");
    /// <summary>
    /// 调用TuShare API
    /// </summary>
    /// <param name="apiName"></param>
    /// <param name="parmaMap"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    public static async Task<DataTable> GetTradeDataAsync(string apiName, Dictionary<string, string> parmaMap, params string[] fields)
    {
        //设置适应国标GB213码，防止部分数据源显示乱码           
        // Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var tuShareParamObj = new TuShareParamObj()
        { ApiName = apiName, Params = parmaMap, Fields = string.Join(",", fields) };
        //做Http调用
        var taskResults = await HttpClientPostAsync(baseUrl, tuShareParamObj).ConfigureAwait(false);
        var desResults = JsonSerializer.Deserialize<TuShareResult>(taskResults);
        //如果调用失败，抛出异常
        if (!string.IsNullOrWhiteSpace(desResults.Msg))
        {
            throw new Exception(desResults.Msg);
        }
        return desResults.ConvertToDaTaTable();
    }
    /// <summary>
    /// 转换为DaTaTable数据格式。返回结果分成两部分，一部分是列头信息，另一部分是数据本身。
    /// </summary>
    /// <param name="results"></param>
    /// <returns></returns>
    private static DataTable ConvertToDaTaTable(this TuShareResult results)
    {
        DataTable dtResult = new();

        foreach (var dataField in results.Data.Fields)
        {
            dtResult.Columns.Add(dataField);
        }
        foreach (var dataItemRow in results.Data.Items)
        {
            dtResult.Rows.Add(dataItemRow); //将获取的json值统一转成string类型了
        }
        return dtResult;
    }


    private class TuShareParamObj
    {
        [JsonPropertyName("api_name")]
        public string ApiName { get; set; }

        [JsonPropertyName("token")]
        public string Token { get; } = "402be898903cd5e3430250815ffe8cc6e1da8e5ef2da59dd83f2c51d";
        //public string Token { get; } = "9a2d3a4c80130c2b84675051650606db41455af5582c9954f56822d2";//Token           

        [JsonPropertyName("params")]
        public Dictionary<string, string> Params { get; set; }

        [JsonPropertyName("fields")]
        public string Fields { get; set; }
    }
    private class TuShareResult
    {
        [JsonPropertyName("code")]
        public object Code { get; set; }

        [JsonPropertyName("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("data")]
        public TuShareData Data { get; set; }
    }
    private class TuShareData
    {
        [JsonPropertyName("fields")]
        public string[] Fields { get; set; }

        [JsonPropertyName("items")]
        public object[][] Items { get; set; }
    }
}

