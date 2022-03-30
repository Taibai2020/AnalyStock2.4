using AnalyStock.DataManage;

namespace AnalyStock.WpfForm;

public partial class LoadDataPage
{
    //导入单日交易数据根据交易日历
    private List<TradeCalendar> tradeCalendars; //保存日历集合
    private int idxCalendarLst; //日历索引，为根据日历循环下载数据计数 
    private IList<Infodaily> infodailies_one, infodailies_two;
    //导入期间数据数据根据股票代码
    private IList<string> stockCodes_one, stockCodes_two;
    private CustomProgress custProgress;
    private void BtnCancelProgress_Click(object sender, RoutedEventArgs e)
    {
        if (custProgress == null) return;
        if (custProgress.IsBusy) custProgress.IsCancellation = true;
    }
    private void IniCustomProgress()
    {
        if (custProgress is not null)
        {
            custProgress.IsBusy = false;
            custProgress.IsCancellation = false;
            return;
        }
        custProgress = new();
        custProgress.ProgressChanged += (object sender, MultitaskChangedEventArgs e) =>
         {
             if (e.TaskNo is "No1")
             {
                 barProgress1.Value = e.ProgressPercentage;
                 labelPrograss1.Content = e.UserState;
             }
             else
             {
                 barProgress2.Value = e.ProgressPercentage;
                 labelPrograss2.Content = e.UserState;
             }
         };

    }
    /// <summary>
    /// 获取下载数据
    /// </summary>
    /// <returns></returns>
    #region    
    //获取交易日历
    private async ValueTask<bool> IsGetTradeCalendarAsync()
    {
        var (IsSuccees, IListData) = await CommDataMethod
            .IsGetDataOnlineAsync<TradeCalendar>(errorCode: "", startDate, endDate);
        tradeCalendars?.Clear();
        if (IsSuccees)
        {
            tradeCalendars = IListData.ToList();
        }
        return IsSuccees;
    }
    //循环获取每日的全部股票交易数据
    private async ValueTask<bool> IsGetDailyDataOnLoopAsync()
    {
    LoopStart:
        if (idxCalendarLst >= tradeCalendars.Count) { return false; }
        var (IsSuccees, IListData) = await CommDataMethod
            .IsGetDataOnlineAsync<Infodaily>(errorCode: "all", tradeCalendars[idxCalendarLst++].Cal_date);
        infodailies_one?.Clear(); infodailies_two?.Clear();
        if (IsSuccees)  //数据下载成功
        {
            if (checkBoxMutilTask.IsChecked.Value)  //是否双任务
            {
                int countInDoubleSection = IListData.Count / 2;
                infodailies_one = IListData.Take(countInDoubleSection).ToList();
                infodailies_two = IListData.Skip(countInDoubleSection).ToList();
            }
            else
            {
                infodailies_one = IListData.ToList();
            }
            return true;
        }
        //循环执行，遍历交易日历
        //return await IsGetDailyDataOnLoopAsync();
        goto LoopStart;
    }
    //获取股票代码
    private async ValueTask<bool> IsGetStockCodeAsync()
    {
        var (IsSuccees, IListData) =
            await CommDataMethod.IsGetDataOnlineAsync<BasicStock>(errorCode: "");
        stockCodes_one?.Clear(); stockCodes_two?.Clear();
        if (IsSuccees)
        {
            var stockCodes = IListData.Select(n => n.Ts_code);
            if (checkBoxMutilTask.IsChecked.Value)  //是否双任务
            {
                int countInDoubleSection = stockCodes.Count() / 2;
                stockCodes_one = stockCodes.Take(countInDoubleSection).ToList();
                stockCodes_two = stockCodes.Skip(countInDoubleSection).ToList();
                IListData.Clear();
            }
            else //单后台任务执行
            {
                stockCodes_one = stockCodes.ToList();
            }
        }
        return IsSuccees;
    }
    #endregion
    /// <summary>
    /// 补充下载失败记录
    /// </summary>
    #region
    private async Task DownFailRecordOnLoopAsync()
    {
        //如果过放弃下载执行,错误记录为零,放弃补充下载，则退出
        if (custProgress.IsCancellation ||
            ErrorLogPool.ErrorLogs.Count == 0 ||
            !IsConfirmMessage($"{ErrorLogPool.ErrorLogs.Count}个失败记录，是否补充导入?"))
        {
            QuitDoWorks();
            return;
        }
        //初始化工作过程...
        ClearListOnWorking();
        //整理失败记录
        RepartErrorLogs();
        //处理根据交易日历导入的失败记录
        if (tradeCalendars.Count > 0)
        {
            await RepeatDownDataOnDailyAsync();
        }
        //处理根据股票编码导入历史数据的失败记录
        if (stockCodes_one.Count > 0)
        {
            await RepeatDownDataOnPeriodAsync();
        }
        await DownFailRecordOnLoopAsync(); //循环处理错误记录        
    }
    private async Task RepeatDownDataOnDailyAsync()
    {
        //不是当日全部交易数据下载错误，则转到个股下载错误处理流程
        if (await IsGetDailyDataOnLoopAsync())
        {
            await DoWorkOnDailyAsync(custProgress);
        }
    }
    private async Task RepeatDownDataOnPeriodAsync()
    {
        if (typeofLoadInBatch == "DownDataOfFinance")
        {
            await DoWorkOnPeriodAsync<FinancialIndex>(custProgress);
        }
        else
        {
            await DoWorkOnPeriodAsync<Infodaily>(custProgress);
        }
    }
    private void RepartErrorLogs()
    {       
        //获取下载错误日的交易日历
        tradeCalendars = ErrorLogPool.ErrorLogs
                            .Where(n => n.ProcessName== "Infodaily" && n.StockCode is "all")
                            .Select(n => new TradeCalendar { Cal_date = n.TradeDate }).ToList();
        //获取个股下载错误的记录
        stockCodes_one = ErrorLogPool.ErrorLogs
                            .Where(n => n.ProcessName == "Infodaily" && n.StockCode is not "all")
                            .Select(n => n.StockCode).ToList();
        ErrorLogPool.ErrorLogs?.Clear();
    }
    //初始化相关记录
    private void InitDoWorks()
    {
        SystemParam.IsBusyOfDownDataOnLine = true;
        btnLoadAllData.IsEnabled = false;
        btnStop.IsEnabled = true;
        ErrorLogPool.ErrorLogs ??= new();
        tradeCalendars ??= new();
        labelPrograss1.Content = "开始下载数据...";
        IniCustomProgress();
        ClearListOnWorking();
    }
    private void QuitDoWorks()
    {
        SystemParam.IsBusyOfDownDataOnLine = false;
        btnLoadAllData.IsEnabled = true;
        btnStop.IsEnabled = false;
        labelPrograss1.Content = custProgress.IsCancellation ? "数据下载终止..." : "数据下载任务结束...";
        ClearListOnWorking();
    }
    private void ClearListOnWorking()
    {
        idxCalendarLst = 0;
        tradeCalendars?.Clear();
        stockCodes_one?.Clear(); stockCodes_two?.Clear();
        infodailies_one?.Clear(); infodailies_two?.Clear();
        labelPrograss2.Content = "";
        barProgress1.Value = 0; barProgress2.Value = 0;
    }
    #endregion
    /// <summary>
    /// 双任务下载入口
    /// </summary>
    /// <returns></returns>
    private async Task DownLoadByMultiTaskAsync()
    {
        if (IsWrongOfCheckFormatting("DownAllStockOnPeriod") ||
            !IsConfirmMessage("批量追加信息需要较长时间,是否确认?")) { return; }
        InitDoWorks();
        switch (typeofLoadInBatch)
        {
            case "DownDataOnDaily":
                if (await IsGetTradeCalendarAsync() && await IsGetDailyDataOnLoopAsync())
                {
                    await DoWorkOnDailyAsync(custProgress);
                }
                break;
            case "DownDataOnPeriod":
                if (await IsGetStockCodeAsync())
                {
                    await DoWorkOnPeriodAsync<Infodaily>(custProgress);
                }
                break;
            case "DownDataOfFinance":
                if (await IsGetStockCodeAsync())
                {
                    await DoWorkOnPeriodAsync<FinancialIndex>(custProgress);
                }
                break;
            default:
                break;
        }
        await DownFailRecordOnLoopAsync();
    }
    /// <summary>
    /// 下载每日全部股票的交易数据
    /// </summary>
    /// <param name="progress"></param>
    /// <returns></returns>
    #region
    private async Task DoWorkOnDailyAsync(ICustomProgress progress)
    {
        if (progress.IsCancellation || progress.IsBusy)
        {
            return;
        }

        List<Task> downDatatoSaveDbSetTasks = new();
        if (infodailies_one.Any())
        {
            downDatatoSaveDbSetTasks.Add(DownDatasToDbSetOnDailyAsync(progress, "No1", infodailies_one));
        }
        if (infodailies_two.Any())
        {
            downDatatoSaveDbSetTasks.Add(DownDatasToDbSetOnDailyAsync(progress, "No2", infodailies_two));
        }
        await Task.WhenAll(downDatatoSaveDbSetTasks);

        if (await IsGetDailyDataOnLoopAsync())
        {
            await DoWorkOnDailyAsync(custProgress);
        }
    }
    private static async Task DownDatasToDbSetOnDailyAsync(ICustomProgress progress,
        string noTask, IList<Infodaily> infodailies)
    {
        progress.IsBusy = true;
        int idxDaily = 0;
        string trdateDate = infodailies.First().Trade_date;
        await foreach (var item 
            in SaveDataToDbsetWithAllStocksAsync(infodailies).ConfigureAwait(false))
        {
            progress.Report(noTask, idxDaily * 100 / infodailies.Count + 1,
                $"{trdateDate}日数据已下载第:{idxDaily++}条,记录共{infodailies.Count}条股票记录");
            if (progress.IsCancellation)
            {
                progress.Report(noTask, 100, $"{trdateDate}日数据已下载第{idxDaily}条记录,处理终止...");
                break;
            }
        }
        progress.IsBusy = false;
    }
    private static async IAsyncEnumerable<int>
        SaveDataToDbsetWithAllStocksAsync(IList<Infodaily> list)
    {
        foreach (var item in list)
        {
            yield return await SaveDataToDbSetWithSingleStockAsync(item).ConfigureAwait(false);

        }
    }
    private static async ValueTask<int>
        SaveDataToDbSetWithSingleStockAsync(Infodaily infodaily)
    {
        try
        {
            await Task.Delay(2);
            return await CommDataHelper<Infodaily>.SaveDataToDbSetAsync
                (new List<Infodaily> { infodaily }, false, infodaily.Ts_code).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ErrorLogPool.LogErrorMessage("Infodaily", infodaily.Ts_code, infodaily.Trade_date, ex.Message);
            return -1;
        }

    }
    #endregion

    /// <summary>
    /// 批量导入：根据股票代码保存每只股票历史数据或财务数据
    /// </summary>    
    #region 
    private async Task DoWorkOnPeriodAsync<T>(ICustomProgress progress) where T : class, new()
    {
        if (progress.IsBusy)
        {
            return; //任务执行则放弃       
        }

        List<Task> downDatatoSaveDbSetTasks = new();
        if (stockCodes_one.Any())
        {
            downDatatoSaveDbSetTasks.Add(DownDatasToDbSetOnPeriodAsync<T>(progress, "No1", stockCodes_one, startDate, endDate));
        }
        if (stockCodes_two.Any())
        {
            downDatatoSaveDbSetTasks.Add(DownDatasToDbSetOnPeriodAsync<T>(progress, "No2", stockCodes_two, startDate, endDate));
        }
        await Task.WhenAll(downDatatoSaveDbSetTasks);

    }
    private static async Task DownDatasToDbSetOnPeriodAsync<T>(ICustomProgress progress, string noTask,
        IList<string> stockCodes, string startDate, string endDate) where T : class, new()
    {
        progress.IsBusy = true;
        int idxStockcode = 0;
        await foreach (var result 
            in DownDataToDbSetWithAllStocksAsync<T>(stockCodes, startDate, endDate).ConfigureAwait(false))
        {

            progress.Report(noTask, idxStockcode * 100 / stockCodes.Count + 1,
                $"后台进程已处理完成第:{idxStockcode++}条,记录共{stockCodes.Count}条股票记录");
            if (progress.IsCancellation)
            {
                progress.Report(noTask, 100, $"后台进程已处理完成 第{idxStockcode}条记录,处理终止...");
                break;
            }
        }
        progress.IsBusy = false;
    }
    private static async IAsyncEnumerable<int>
        DownDataToDbSetWithAllStocksAsync<T>
        (IList<string> stockCodes, string startDate, string endDate) where T : class, new()
    {
        foreach (var code in stockCodes)
        {
            yield return await 
                DownDataToDbSetWithSingleStockAsync<T> (code, startDate, endDate).ConfigureAwait(false);
        }
    }
    private static async ValueTask<int>
        DownDataToDbSetWithSingleStockAsync<T>(string stockCode, string startDate, string endDate) where T : class, new()
    {
        try
        {
            var result = 
                await CommDataHelper<T>.GetDataOnlineAsync(stockCode, startDate, endDate).ConfigureAwait(false);           
            return 
                await CommDataHelper<T>.SaveDataToDbSetAsync(result, false, stockCode).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ErrorLogPool.LogErrorMessage("Infodaily", stockCode, endDate, ex.Message);
            return -1;
        }

    }
    #endregion   

}






