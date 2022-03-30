namespace AnalyStock.DataManage
{
    internal class CommDataHelper<T> where T : class, new()
    {
        //params:stockCode,StartDate,EndDate,TradeDate
        internal static async Task<IList<T>> GetDataOnlineAsync(params string[] args)
        {
            IDownDataOnline<T> idownData = typeof(T) switch
            {
                { Name: "BasicStock" } => new DownDataOnline_BasicStock() as DownDataOnline<T>,
                { Name: "BakDaily" } => new DownDataOnline_BaKDaily() as DownDataOnline<T>,
                { Name: "Infodaily" } => new DownDataOnline_Infodaily() as DownDataOnline<T>,
                { Name: "TradeCalendar" } => new DownDataOnline_TradeCalendar() as DownDataOnline<T>,
                { Name: "FinancialIndex" } => new DownDataOnlline_FinancialIndex() as DownDataOnline<T>,
                { Name: "CurrentInforOfQutoes" } => new DownDataOnlline_CurrentInforOfQutoes() as DownDataOnline<T>,
                _ => new DownDataOnline<T>(),
            };
            await idownData.DownDataOnlineAsync(args).ConfigureAwait(false);
            return idownData.ListProcessing;
        }
        //params:stockCode,StartDate,EndDate,TradeDate
        internal static async Task<IList<T>> GetDataOnDbSetAsync(params string[] args)
        {
            IDbReadWrite<T> idbTable = typeof(T) switch
            {
                { Name: "Infodaily" } => new DbReadWrite_Infodaliy() as DbReadWrite<T>,
                { Name: "FinancialIndex" } => new DbReadWrite_FinacialIndex() as DbReadWrite<T>,
                { Name: "MergeBakDaily" } => new DbReadWrite_MergeBakDaily() as DbReadWrite<T>,
                _ => new DbReadWrite<T>(),
            };
            await idbTable.QueryDataFromDbAsync(args).ConfigureAwait(false);
            return idbTable.ListProcessing;
        }
        internal static async ValueTask<int> DeleDataOnDbSetAsync(IList<T> list)
        {
            return await new DbReadWrite<T>
            { ListProcessing = list }.DeleDataToDbAsync().ConfigureAwait(false);
        }
        //params: stockCode
        internal static async ValueTask<int> SaveDataToDbSetAsync(IList<T> list,
            bool isRewriteOrInsert = false, params string[] args)
        {
            return await new DbReadWrite<T>
            { ListProcessing = list }.SaveDataToDbAsync(isRewriteOrInsert ? null : args[0]).ConfigureAwait(false);
        }
    }

    /// 增加在线交易日及时数据 (网易数据接口)
    internal static class InfordailyExtend
    {
        internal static async Task<IList<Infodaily>> AddCurrentInforOnlineAsync(this IList<Infodaily> list)
        {
            var currentInfors =
                await CommDataMethod.GetDataOnlineAsync<CurrentInforOfQutoes>(list?.First().Ts_code);
            var currentInfor = currentInfors.FirstOrDefault();                                    
            string currentTime = DateTime.Parse(currentInfor?.time).ToString("yyyyMMdd");
            //如果库里数据不包括当日数据，则追加当日在线数据
            if (currentTime == list?.Last().Trade_date) { return list; }
            list.Add(new Infodaily
            {
                Trade_date = currentTime,
                Ts_code = list?.First().Ts_code,
                Open = currentInfor.open,
                High = currentInfor.high,
                Low = currentInfor.low,
                Close = currentInfor.price,
                Pre_close = currentInfor.yestclose,
                Change = currentInfor.updown,
                Pct_chg = currentInfor.percent * 100.0f,
                Vol = currentInfor.volume / 100.0f,
                Amount = currentInfor.turnover / 1000.0f,
                Adj_factor = list.Last().Adj_factor
            });
            return list;
        }
    }
}
