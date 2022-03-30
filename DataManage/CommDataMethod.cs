namespace AnalyStock.DataManage;

internal class CommDataMethod
{
    /// <summary>
    /// 通用数据处理泛型模型，从数据库提取数据，从在线提取数据，提取并保存数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args">stockCode,startDate,endDate,tradeDate </param>
    /// <returns></returns>
    #region 
    //获取在线数据，如果正常获取则返回，否则返回为空集合
    internal static IList<T> GetDataOnline<T>(params string[] args) where T : class, new()
    {
        try
        {
            return CommDataHelper<T>.GetDataOnlineAsync(args).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            ErrorMessage(ex.Message);
        }
        return new List<T>();
    }
    internal static async Task<IList<T>> GetDataOnlineAsync<T>(params string[] args) where T : class, new()
    {
        try
        {
            return await CommDataHelper<T>.GetDataOnlineAsync(args);
        }
        catch (Exception ex)
        {
            ErrorMessage(ex.Message);
        }
        return new List<T>();
    }
    // 获取在线的数据集合，同时返回获取成功的标志   
    internal static async Task<(bool IsSuccees, IList<T> IListData)>
        IsGetDataOnlineAsync<T>(string errorCode = default, params string[] args) where T : class, new()
    {
        try
        {
            var result = await CommDataHelper<T>.GetDataOnlineAsync(args);
            if (result.Count == 0)
            {
                throw new Exception("No data was downloaded...");
            }

            return (true, result);
        }
        catch (Exception ex)
        {
            ErrorLogPool.LogErrorMessage(typeof(T).Name, errorCode, "", ex.Message);
            return (false, new List<T>());
        }
    }
    //获取库文件数据，如果正常获取则返回，否则返回为空集合
    internal static IList<T> GetDataOnDbSet<T>(params string[] args) where T : class, new()
    {
        try
        {
            return CommDataHelper<T>.GetDataOnDbSetAsync(args).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            ErrorMessage(ex.Message);
        }
        return new List<T>();
    }
    //异步获取数据库文件
    internal static async Task<IList<T>> GetDataOnDbSetAsync<T>(params string[] args) where T : class, new()
    {
        try
        {
            return await CommDataHelper<T>.GetDataOnDbSetAsync(args);
        }
        catch (Exception ex)
        {
            ErrorMessage(ex.Message);
        }
        return new List<T>();
    }   
    /// <summary>
    /// 异步下载保存数据库文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="isReWrite"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal static async Task DownDataToDbSetAsync<T>(bool isReWrite, params string[] args) where T : class, new()
    {
        try
        {
            //获取在线数据文件
            IList<T> listResult = await CommDataHelper<T>.GetDataOnlineAsync(args);
            //没有记录，抛出异常放弃执行
            if (!listResult.Any()) { throw new Exception("获取在线数据错误...!"); }
            //保存到数据库文件
            var intResult = await CommDataHelper<T>.SaveDataToDbSetAsync(listResult, isReWrite, args);
            MessageBox.Show($"数据处理完成，在线获取数据{listResult.Count}条，更新数据库添加:{intResult}条记录！",
                "完成提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ErrorMessage(ex.Message);
        }
    }
    
    #endregion
    /// <summary>
    /// 自选股保存、删除的处理方法
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static async ValueTask SaveSeleStockAsync(params string[] args)
    {
        try
        {
            var intResult = await CommDataHelper<SeleStockofSelf>
                .SaveDataToDbSetAsync(new List<SeleStockofSelf>{
                    new SeleStockofSelf(){
                        Ts_code = args[0],
                        Name = args[1],
                        Industry = args[2],
                        Conception = "",
                        SelfType = ""}}, false, "");

            if (intResult != 0)
            {
                CommDataMethod.InforMessage($"{args[1]}股票添加自选股，完成...");
                return;
            }
            throw new Exception("该股票已经在自选股中...");
        }
        catch (Exception ex)
        {
            CommDataMethod.ErrorMessage(ex.Message);
        }
    }
    public static async ValueTask DelSeleStockAsync(params string[] args)
    {
        try
        {
            var intResult = await CommDataHelper<SeleStockofSelf>
                .DeleDataOnDbSetAsync(new List<SeleStockofSelf> {
                    new SeleStockofSelf(){
                        Ts_code = args[0],
                        Name = args[1],
                        Industry = args[2],
                        Conception = "",
                        SelfType = ""
            }});
            if (intResult != 0)
            {
                CommDataMethod.InforMessage("该股票已从自选中删除...");
                return;
            }
            throw new Exception("该股票不在自选股中...");
        }
        catch (Exception ex)
        {
            CommDataMethod.ErrorMessage(ex.Message);
        }
    }

    public static void ErrorMessage(string message)
    {
        MessageBox.Show(message, "错误提示", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    public static void InforMessage(string message)
    {
        MessageBox.Show(message, "完成提示", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    /// <summary>
    /// #FFFFFFF 颜色的通用字符串
    /// </summary>
    /// <param name="colorStr"></param>
    /// <returns></returns>
    public static Color ConvertoColor(string colorStr)
    {
        return Color.FromArgb(
            byte.Parse(colorStr[1..3], System.Globalization.NumberStyles.HexNumber),
            byte.Parse(colorStr[3..5], System.Globalization.NumberStyles.HexNumber),
            byte.Parse(colorStr[5..7], System.Globalization.NumberStyles.HexNumber),
            byte.Parse(colorStr[7..9], System.Globalization.NumberStyles.HexNumber));
    }
}


