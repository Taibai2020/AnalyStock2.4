using System.ComponentModel.DataAnnotations;
using System.Reflection;
namespace AnalyStock.DataManage;

interface IDownDataOnline<T>
{
    IList<T> ListProcessing { set; get; }
    Task DownDataOnlineAsync(params string[] args);
}
class DownDataOnline<T> : IDownDataOnline<T> where T : class, new()
{
    private IList<T> _list;
    public IList<T> ListProcessing
    {
        set => _list = value;
        get => _list;
    }
    public virtual async Task DownDataOnlineAsync(params string[] args)
    {
        await Task.Delay(5);
        _list = new List<T>();
    }
}
static class CommDataTableToCollection
{
    public static IList<T> ConvertTableToList<T>
        (this DataTable dtResult, bool isCustomAttribute = false) where T : class, new()
    {
        List<T> lstT = new(); // 定义集合 
        foreach (DataRow dr in dtResult.Rows) //遍历DataTable中所有的数据行 
        {
            T classT = new();
            foreach (PropertyInfo pi in typeof(T).GetProperties()) //遍历该对象的所有属性
            {
                string piName = isCustomAttribute
                    ? pi.GetCustomAttribute<DisplayAttribute>().Name//因在线取得的中文名，所以使用注解中文属性给属性进行赋值  
                    : pi.Name.ToLower();//将小写的属性名称赋值给临时变量 ，数据库字段名为小写               
                if (dtResult.Columns.Contains(piName) && dr.Field<string>(piName) is not null) //检查DataTable是否包含此列（列名==对象的属性名） 
                {
                    pi.SetValue(classT, Convert.ChangeType(dr.Field<string>(piName), pi.PropertyType));
                }
            }
            lstT.Add(classT);
        }
        return lstT;
    }
}

///泛型实类化获取数据
#region
/// <summary>
/// 获取股票基本数据，参数args： null,null,null,null
/// </summary>
class DownDataOnline_BasicStock : DownDataOnline<BasicStock>
{
    public override async Task DownDataOnlineAsync(params string[] args)
    {
        Dictionary<string, string> paramKey = new()
        {
            { "list_status", "L" },
            { "exchange", "" },
            { "fields", "ts_code,symbol,name,area,industry,market,list_date" }
        };
        using var dtResulte = await UtilityOfTuShare
             .GetTradeDataAsync("stock_basic", paramKey, "")
             .ConfigureAwait(false);
        ListProcessing = dtResulte.ConvertTableToList<BasicStock>();
    }
}
/// <summary>
///  null, null, null, tradedate
/// </summary>
class DownDataOnline_BaKDaily : DownDataOnline<BakDaily>
{
    public override async Task DownDataOnlineAsync(params string[] args)
    {
        Dictionary<string, string> paramKey = new()
        {
            { "trade_date", args[0] }
        };
        using var dtResulte =
            await UtilityOfTuShare.GetTradeDataAsync("bak_daily", paramKey, "").ConfigureAwait(false);
        ListProcessing = dtResulte.ConvertTableToList<BakDaily>();
    }
}
/// <summary>
/// null, startdate, enddate, null
/// </summary>
class DownDataOnline_TradeCalendar : DownDataOnline<TradeCalendar>
{
    public override async Task DownDataOnlineAsync(params string[] args)
    {
        Dictionary<string, string> paramKey = new()
        {
            { "start_date", args[0] },
            { "end_date", args[1] },
            { "exchange", "SSE" }, //上交所              
            { "is_open", "1" },
        };
        using var dtResulte =
            await UtilityOfTuShare.GetTradeDataAsync("trade_cal", paramKey, "").ConfigureAwait(false);
        ListProcessing = dtResulte.ConvertTableToList<TradeCalendar>();
    }
}
/// <summary>
/// null, null, null, tradedate，单个交易日全部股票数据
/// ts_code, startdate, enddate, null，某个股票期间全部交易数据
/// </summary>
class DownDataOnline_Infodaily : DownDataOnline<Infodaily>
{
    public override async Task DownDataOnlineAsync(params string[] args)
    {
        Dictionary<string, string> paramKey = new();
        if (args.Length > 1)
        {
            paramKey.Add("ts_code", args[0]);
            paramKey.Add("start_date", args[1]);
            paramKey.Add("end_date", args[2]);
        }
        else
        {
            paramKey.Add("trade_date", args[0]);
        }
        using var dtInfordaily = await UtilityOfTuShare
             .GetTradeDataAsync("daily", paramKey, "").ConfigureAwait(false);
        using var dtAdjFactor = await UtilityOfTuShare
              .GetTradeDataAsync("adj_factor", paramKey, "").ConfigureAwait(false);
        IEnumerable<Infodaily> mergeInfordailes;
        if (args.Length > 1)
        {
            //根据日期关联,排序
            mergeInfordailes = from trdata in dtInfordaily.ConvertTableToList<Infodaily>()
                               join adjdata in dtAdjFactor.ConvertTableToList<AdjFactor>()
                               on trdata.Trade_date equals adjdata.Trade_date
                               orderby trdata.Trade_date
                               select new Infodaily
                               {
                                   Ts_code = trdata.Ts_code,
                                   Trade_date = trdata.Trade_date,
                                   Open = trdata.Open,
                                   High = trdata.High,
                                   Low = trdata.Low,
                                   Close = trdata.Close,
                                   Pre_close = trdata.Pre_close,
                                   Change = trdata.Change,
                                   Pct_chg = trdata.Pct_chg,
                                   Vol = trdata.Vol,
                                   Amount = trdata.Amount,
                                   Adj_factor = adjdata.Adj_factor,
                               };

        }
        else
        {
            //根据股票代码关联，排序
            mergeInfordailes = from trdata in dtInfordaily.ConvertTableToList<Infodaily>()
                               join adjdata in dtAdjFactor.ConvertTableToList<AdjFactor>()
                               on trdata.Ts_code equals adjdata.Ts_code
                               orderby trdata.Ts_code
                               select new Infodaily
                               {
                                   Ts_code = trdata.Ts_code,
                                   Trade_date = trdata.Trade_date,
                                   Open = trdata.Open,
                                   High = trdata.High,
                                   Low = trdata.Low,
                                   Close = trdata.Close,
                                   Pre_close = trdata.Pre_close,
                                   Change = trdata.Change,
                                   Pct_chg = trdata.Pct_chg,
                                   Vol = trdata.Vol,
                                   Amount = trdata.Amount,
                                   Adj_factor = adjdata.Adj_factor,
                               };
        }
        ListProcessing = mergeInfordailes.ToList();

    }
}
/// <summary>
///ts_code, startdate, enddate, null，某个股票期间财务数据 
/// </summary>
class DownDataOnlline_FinancialIndex : DownDataOnline<FinancialIndex>
{
    public override async Task DownDataOnlineAsync(params string[] args)
    {
        using var dtFinancialIdx = await UtilityOfQuotesMoney.GetFinancialInfor(args[0]).ConfigureAwait(false);
        ListProcessing = dtFinancialIdx.ConvertTableToList<FinancialIndex>(true).Where(n => string.Compare(n.ReportDt, args[1], StringComparison.CurrentCulture) >= 0
                                  && string.Compare(n.ReportDt, args[2], StringComparison.CurrentCulture) <= 0).OrderBy(n => n.ReportDt).ToList();

    }
}
/// <summary>
/// ts_code, null, null, null，
/// </summary>
class DownDataOnlline_CurrentInforOfQutoes : DownDataOnline<CurrentInforOfQutoes>
{
    public override async Task DownDataOnlineAsync(params string[] args)
    {
        ListProcessing = new List<CurrentInforOfQutoes>(){
          await  UtilityOfQuotesMoney.GetCurrentInfors(args[0]).ConfigureAwait(false)};
    }
}
#endregion

