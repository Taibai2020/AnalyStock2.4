namespace AnalyStock.DataManage;

public static class HttpClientUtility
{
    public static async Task<string> HttpClientPostAsync(Uri url, object datajson)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using HttpClient httpClient = new(); //handler             
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpClient.Timeout = new TimeSpan(0, 0, 5);
        //转为链接需要的格式
        using HttpContent httpContent = JsonContent.Create(datajson);            //请求
        using HttpResponseMessage response = await httpClient.PostAsync(url, httpContent).ConfigureAwait(false);
        // Note that because of the ConfigureAwait(false), we are not on the original context here.
        // Instead, we're running on the thread pool.
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            // The second call to ConfigureAwait(false) is not *required*, but it is Good Practice.
            return result;
        }
        throw new Exception($"{url}数据下载错误:{response.ReasonPhrase}");
    }

}
