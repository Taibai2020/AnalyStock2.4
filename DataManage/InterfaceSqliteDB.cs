using Microsoft.EntityFrameworkCore;
namespace AnalyStock.DataManage;
/// <summary>
/// 通用数据接口
/// </summary>
/// <typeparam name="T"></typeparam>
#region
interface IDbReadWrite<T>
{
    Task QueryDataFromDbAsync(params string[] args);
    ValueTask<int> SaveDataToDbAsync(params string[] args);
    ValueTask<int> DeleDataToDbAsync();
    IList<T> ListProcessing { set; get; }
}
class DbReadWrite<T> : IDbReadWrite<T> where T : class
{
    private IList<T> _list;
    public IList<T> ListProcessing
    {
        set => _list = value;
        get => _list;
    }
    public virtual async Task QueryDataFromDbAsync(params string[] args)
    {
        _list = await SqiltTableHelper<T>.QueryDataOnTableAsync();
    }

    //不能使用 asny void 会导致错误异常点在异步发生，导致无法捕获。    
    public async ValueTask<int> SaveDataToDbAsync(params string[] args)
    {
        return (args[0] is null)
            ? await SqiltTableHelper<T>.ReWriteDataOnTableAsync(_list)
            : await SqiltTableHelper<T>.InsertDataOnTableAsync(args[0], _list);
    }
    public async ValueTask<int> DeleDataToDbAsync()
    {
        return await SqiltTableHelper<T>.DelDataOnTableAsync(_list);
    }
}
#endregion
/// <summary>
/// 泛型数据结构 定义类及方法
/// </summary>
#region
class SqliteContext<T> : DbContext where T : class
{
    private readonly string ConnString;
    public DbSet<T> QTable { get; set; }
    public SqliteContext(string stockCode = "")
    {
        ConnString = $@"Data Source={GetDbFilePath(stockCode)}";
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.UseSqlite(ConnString);
    }
    private static string GetDbFilePath(string stockCode = "") => stockCode switch
    {
        null or "" => SystemParam.BasicinfostockDbPath,
        _ => $"{SystemParam.InforOfDailyDbPath}{stockCode}.db3",
    };
}
class SqiltTableHelper<T> where T : class
{
    internal static async Task<IList<T>> QueryDataOnTableAsync<Torder>
        (string stockcode, Expression<Func<T, bool>> expWhere, Expression<Func<T, Torder>> expOrder)
    {
        await using var dbSqlite = new SqliteContext<T>(stockcode);
        return await dbSqlite.QTable.Where(expWhere).OrderBy(expOrder).AsNoTracking().ToListAsync();
    }
    internal static async Task<IList<T>> QueryDataOnTableAsync()
    {
        await using var dbsqlite = new SqliteContext<T>();
        return await dbsqlite.QTable.AsNoTracking().ToListAsync();
    }
    internal static async ValueTask<int> InsertDataOnTableAsync(string stockCode, IList<T> list)
    {
        //判断数据文件是否存在，不存在就并创建
        _ = await IsHaveTableAsync(stockCode);
        //执行保存数据
        await using var dbSqlite = new SqliteContext<T>(stockCode);
        foreach (var item in list)
        {
            if (!(await dbSqlite.QTable.ContainsAsync(item)))
            {
                await dbSqlite.QTable.AddAsync(item);
            }
        }
        return await dbSqlite.SaveChangesAsync();
    }
    internal static async ValueTask<bool> IsHaveTableAsync(string stockCode)
    {
        //判断并，创建数据库，包括所有实体Table
        await using var dbSqlite = new SqliteContext(stockCode);
        return await dbSqlite.Database.EnsureCreatedAsync();
    }

    internal static async ValueTask<int> DelDataOnTableAsync(IList<T> list)
    {
        await using var dbSqlite = new SqliteContext<T>();
        foreach (var item in list)
        {
            if (await dbSqlite.QTable.ContainsAsync(item))
            {
                dbSqlite.Entry(item).State = EntityState.Deleted;
            }
        }
        return await dbSqlite.SaveChangesAsync();
    }
    internal static async ValueTask<int> ReWriteDataOnTableAsync(IList<T> list)
    {
        await using var dbSqlite = new SqliteContext<T>();
        await dbSqlite.QTable.ForEachAsync(n =>
       {
           dbSqlite.Entry(n).State = EntityState.Deleted;
       });
        foreach (var item in list)
        {
            await dbSqlite.QTable.AddAsync(item);
        }
        return await dbSqlite.SaveChangesAsync();
    }

}
#endregion
//数据库实体结构类，临时创建Db文件时需要实体结构
class SqliteContext : DbContext
{
    private readonly string ConnString;
    public DbSet<Infodaily> Infodailies { get; set; }
    public DbSet<FinancialIndex> MainFinancialIndices { get; set; }
    public DbSet<BasicStock> BasicStocks { get; set; }
    public DbSet<BakDaily> BakDailies { get; set; }
    public DbSet<SeleStockofSelf> SeleStockofSelves { get; set; }
    public SqliteContext(string stockCode = "")
    {
        ConnString = $@"Data Source={GetDbFilePath(stockCode)}";
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _ = optionsBuilder.UseSqlite(ConnString);
    }
    private static string GetDbFilePath(string stockCode = "") => stockCode switch
    {
        null or "" => SystemParam.BasicinfostockDbPath,
        _ => $"{SystemParam.InforOfDailyDbPath}{stockCode}.db3",
    };
}

//泛型实类，多态实现实类查询数据返回
class DbReadWrite_Infodaliy : DbReadWrite<Infodaily>
{
    public override async Task QueryDataFromDbAsync(params string[] args)
    {
        ListProcessing = await SqiltTableHelper<Infodaily>.QueryDataOnTableAsync(args[0],
                    p => p.Trade_date.CompareTo(args[1]) >= 0 && p.Trade_date.CompareTo(args[2]) <= 0,
                    p => p.Trade_date).ConfigureAwait(false);
    }
}
class DbReadWrite_FinacialIndex : DbReadWrite<FinancialIndex>
{
    public override async Task QueryDataFromDbAsync(params string[] args)
    {
        ListProcessing = await SqiltTableHelper<FinancialIndex>.QueryDataOnTableAsync(args[0],
                    p => p.ReportDt.CompareTo(args[1]) >= 0 && p.ReportDt.CompareTo(args[2]) <= 0,
                    p => p.ReportDt).ConfigureAwait(false);
    }
}
class DbReadWrite_MergeBakDaily : DbReadWrite<MergeBakDaily>
{
    public override async Task QueryDataFromDbAsync(params string[] args)
    {
        var quryBakDaily = await SqiltTableHelper<BakDaily>.QueryDataOnTableAsync().ConfigureAwait(false);
        var quryBasicStock = await SqiltTableHelper<BasicStock>.QueryDataOnTableAsync().ConfigureAwait(false);
        var querySeleStock = await SqiltTableHelper<SeleStockofSelf>.QueryDataOnTableAsync().ConfigureAwait(false);
        var mergeCollection = from bkdata in quryBakDaily
                              join bsdata in quryBasicStock
                                    on bkdata.Ts_code equals bsdata.Ts_code into group1
                              from m1 in group1.DefaultIfEmpty(new BasicStock()) //没有左联上，取默认值，右边主键为空
                              join seledata in querySeleStock
                                    on bkdata.Ts_code equals seledata.Ts_code into group2
                              from m2 in group2.DefaultIfEmpty(new SeleStockofSelf())
                              select new MergeBakDaily
                              {
                                  Ts_code = bkdata.Ts_code,
                                  Name = bkdata.Name,
                                  Pe = bkdata.Pe,
                                  Float_mv = bkdata.Float_mv,
                                  Total_mv = bkdata.Total_mv,
                                  Total_share = bkdata.Total_share,
                                  Float_share = bkdata.Float_share,
                                  Area = m1.Area,
                                  Market = m1.Market,
                                  Industry = m1.Industry,
                                  Turn_over = bkdata.Turn_over,
                                  Vol_ratio = bkdata.Vol_ratio,
                                  Issele = m2.Name != null,
                                  Trade_date = bkdata.Trade_date,
                              };

        ListProcessing = mergeCollection.ToList();
    }
}

