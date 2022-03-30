using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace AnalyStock.DataManage;
[Table("Infodaily")]
public class Infodaily   //12 fields
{
    public string Ts_code { get; set; }
    [Key]
    public string Trade_date { get; set; }
    public float Open { get; set; }
    public float High { get; set; }
    public float Close { get; set; }
    public float Low { get; set; }
    public float Pre_close { get; set; }
    public float Change { get; set; }
    public float Pct_chg { get; set; }
    public float Vol { get; set; }
    public float Amount { get; set; }
    public float Adj_factor { get; set; }
    public override bool Equals(object obj)
    {
        if (obj is not Infodaily inforObj)
        {
            return false;
        }

        if (Trade_date == inforObj.Trade_date)
        {
            return true;
        }

        return false;
    }
    public override int GetHashCode()
    {
        return Trade_date.GetHashCode();
    }
}


public class AdjFactor
{
    public string Ts_code { get; set; }
    [Key]
    public string Trade_date { get; set; }
    public float Adj_factor { get; set; }
}

public class TradeCalendar
{
    public string Exchange { get; set; }
    public string Cal_date { get; set; }
    public string Is_open { get; set; }
    public string Pretrade_date { get; set; }
}

[Table("BasicStock")]
public class BasicStock  //7 fields
{
    [Key]
    public string Ts_code { get; set; }
    public string Symbol { get; set; }
    public string Name { get; set; }
    public string Area { get; set; }
    public string Industry { get; set; }
    public string Market { get; set; }
    public string List_date { get; set; }
}

[Table("BakDaily")]
public class BakDaily //26 fields
{
    [Key]
    public string Ts_code { get; set; }
    public string Name { get; set; }
    public string Trade_date { get; set; }
    public float Pct_change { get; set; }
    public float Close { get; set; }
    public float Change { get; set; }
    public float Open { get; set; }
    public float High { get; set; }
    public float Low { get; set; }
    public float Pre_close { get; set; }
    public float Vol_ratio { get; set; }
    public float Turn_over { get; set; }
    public float Swing { get; set; }
    public float Vol { get; set; }
    public float Amount { get; set; }
    public float Selling { get; set; }
    public float Buying { get; set; }
    public float Total_share { get; set; }
    public float Float_share { get; set; }
    public float Pe { get; set; }
    public float Float_mv { get; set; }
    public float Total_mv { get; set; }
    public float Strength { get; set; }
    public float Activity { get; set; }
    public float Avg_turnover { get; set; }
    public float Attack { get; set; }
}

[Table("SeleStockofSelf")]
public class SeleStockofSelf
{
    [Key]
    public string Ts_code { get; set; }
    public string Name { get; set; }
    public string Industry { get; set; }
    public string Conception { get; set; }
    public string SelfType { get; set; }
}

[Table("MainFinancialIndex")]
public class FinancialIndex
{
    [Display(Name = "股票代码")]
    public string Ts_code { get; set; }
    [Key]
    [Display(Name = "报告日期")]
    public string ReportDt { get; set; }
    [Display(Name = "基本每股收益(元)")]
    public float EPS { get; set; }
    [Display(Name = "每股净资产(元)")]
    public float NetAssetPS { get; set; }
    [Display(Name = "每股经营活动产生的现金流量净额(元)")]
    public float OpeCFPS { get; set; }
    [Display(Name = "主营业务收入(万元)")]
    public float MainOpeRevenue { get; set; }
    [Display(Name = "主营业务利润(万元)")]
    public float MainOpeProfit { get; set; }
    [Display(Name = "营业利润(万元)")]
    public float OpeProfit { get; set; }
    [Display(Name = "投资收益(万元)")]
    public float InvestIncome { get; set; }
    [Display(Name = "营业外收支净额(万元)")]
    public float NonOpeRevenue { get; set; }
    [Display(Name = "利润总额(万元)")]
    public float TotalIncome { get; set; }
    [Display(Name = "净利润(万元)")]
    public float NetIncome { get; set; }
    [Display(Name = "净利润(扣除非经常性损益后)(万元)")]
    public float NetIncomeCut { get; set; }
    [Display(Name = "经营活动产生的现金流量净额(万元)")]
    public float OpeNetCF { get; set; }
    [Display(Name = "现金及现金等价物净增加额(万元)")]
    public float CCEGrow { get; set; }
    [Display(Name = "总资产(万元)")]
    public float TotalAsset { get; set; }
    [Display(Name = "流动资产(万元)")]
    public float CurrtAsset { get; set; }
    [Display(Name = "总负债(万元)")]
    public float TotalLiab { get; set; }
    [Display(Name = "流动负债(万元)")]
    public float CurrtLiab { get; set; }
    [Display(Name = "股东权益不含少数股东权益(万元)")]
    public float EquityWithoutMinority { get; set; }
    [Display(Name = "净资产收益率加权(%)")]
    public float WROE { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not FinancialIndex finanObj)
        {
            return false;
        }

        if (ReportDt == finanObj.ReportDt)
        {
            return true;
        }

        return false;
    }
    public override int GetHashCode()
    {
        return ReportDt.GetHashCode();
    }
    public static string[] GetPropertyChineseName()
    {
        List<string> list = new();
        foreach (var pi in typeof(FinancialIndex).GetProperties())
        {
            list.Add(pi.GetCustomAttribute<DisplayAttribute>().Name);
        };
        return list.Skip(1).ToArray();
        /* return new string[]
          {
                 "01报告日期",
                 "02基本每股收益元",
                 "03每股净资产元",
                 "04每股经营现金流净额元",
                 "05主营业务收入万元",
                 "06主营业务利润万元",
                 "07营业利润万元",
                 "08投资收益万元",
                 "09营业外收支净额万元",
                 "10利润总额万元",
                 "11净利润万元",
                 "12扣非净利润",
                 "13经营性现金流量",
                 "14现金及等价物净增加",
                 "15总资产万元",
                 "16流动资产万元",
                 "17总负债万元",
                 "18流动负债万元",
                 "19权益不含少数股东权益",
                 "20净资产收益率加权%"
          };*/
    }
    public object[] GetValueArray()
    {
        List<object> list = new();
        foreach (var pi in typeof(FinancialIndex).GetProperties())
        {
            list.Add(pi.GetValue(this));
        };
        return list.Skip(1).ToArray();
    }
}

public class MergeBakDaily //30 fields
{
    [Key]
    [Display(Name = "股票代码")]
    public string Ts_code { get; set; }  //1
    [Display(Name = "股票名称")]                                //
    public string Name { get; set; }    //2
    public string Area { get; set; }  //13        
    public string Industry { get; set; }  //14        
    public string Market { get; set; }  //15    
    public float Pe { get; set; }  //6   
    //public float Close { get; set; }
    //public float Vol { get; set; }
    //public float Amount { get; set; }   
    public float Total_mv { get; set; }  //10
    public float Float_mv { get; set; }  //9 
    public float Total_share { get; set; } //11       
    public float Float_share { get; set; }  //12   
    public bool Issele { get; set; }
    public float Vol_ratio { get; set; }
    public float Turn_over { get; set; }
    public string Trade_date { get; set; }

    public string[] ToValueString()
    {
        return new string[]
        {
            "股票代码: "+Ts_code,
            "股票名称: "+Name,
            "地   区: "+Area,
            "所属行业: "+Industry,
            "所属市场: "+Market,
            "动态市盈率: "+Pe,
            "总市值/亿元: "+Total_mv,
            "流通市值/亿元: "+Float_mv,
            "总股数/亿股: "+Total_share,
            "流通股数/亿股: "+Float_share,
            "备份日期:"+Trade_date,
            "量  比:\t"+Vol_ratio,
            "换手率:\t"+Turn_over
        };
    }
    public object[] GetValueArray()
    {
        return new object[]
        {
            Ts_code, Name, Area, Industry,Market, Pe, Total_mv, Float_mv, Total_share, Float_share,Trade_date, Vol_ratio,Turn_over
        };
    }
}

public class CurrentInforOfQutoes
{
    public string name { get; set; } //中文简称
    public string code { get; set; }  //1或0+股票编码
    public string symbol { get; set; } //股票编码
    public string time { get; set; }  //"2020/11/27 15:59:54"
    public float open { get; set; }
    public float high { get; set; }
    public float price { get; set; }
    public float low { get; set; }
    public float yestclose { get; set; }
    public int volume { get; set; }
    public float turnover { get; set; }
    public float percent { get; set; } //0.005887
    public float updown { get; set; }  //0.33
    public string arrow { get; set; }  //"↑"
    public string type { get; set; }  //"SZ"
    public string update { get; set; }  //"2020/11/27 15:59:56",
    public int status { get; set; }
    //public double ask3 { get; set; }
    //public double ask2 { get; set; }
    //public double ask5 { get; set; }
    //public double ask4 { get; set; }
    //public double ask1 { get; set; }
    //public int askvol1 { get; set; }
    //public int askvol3 { get; set; }
    //public int askvol2 { get; set; }
    //public int askvol5 { get; set; }
    //public int askvol4 { get; set; }
    //public double bid5 { get; set; }
    //public double bid4 { get; set; }
    //public double bid3 { get; set; }
    //public double bid2 { get; set; }
    //public double bid1 { get; set; }
    //public int bidvol1 { get; set; }
    //public int bidvol3 { get; set; }
    //public int bidvol2 { get; set; }
    //public int bidvol5 { get; set; }
    //public int bidvol4 { get; set; }
}






