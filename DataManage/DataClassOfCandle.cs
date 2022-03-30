namespace AnalyStock.DataManage;

public class CandlePoint
{
    public int LocationIndex { get; set; }
    public string Week { get; set; }//Year+Noofweek
    public string Month { get; set; }
    public string StockCode { get; set; }
    public string Name { get; set; }
    public string Date { get; set; }
    public float High { get; set; }
    public float Low { get; set; }
    public float Open { get; set; }
    public float Close { get; set; }
    public float Vol { get; set; }//交易量：手（百股)
    public float Amount { get; set; }  //交易额：千元        
    public float Change { get; set; } //增长额
    public float Pctchg { get; set; }  //增幅
    public float AdjustPowerFactor { get; set; } //复权因子
    public float Ma5 { get; set; } //均值
    public float Ma10 { get; set; }
    public float Ma20 { get; set; }
    public float Ma30 { get; set; }
    public float Ma60 { get; set; }
    public float Ma120 { get; set; }
    //属性指标
    public SolidColorBrush ColorVol { get; set; }
    public SolidColorBrush ColorCandle { get; set; }
    public bool IsCloseUpOpen { get; set; }
    public bool IsDoubleVol { get; set; }  //交易量是否倍量
    public bool IsSkyrocketing { get; set; } //是否涨停
}
public class MAPoint
{
    public int MAIndex;   //0开始的数据组索引
    public int LocationIndex;//坐标轴上的索引位置，不是具体索引
    public int Clcye;
    public float MAValue;
}
public class KDJPoint
{
    public int KDJIndex;
    public int LocationIndex;
    public float RSValue;
    public float KValue;
    public float DValue;
    public float JValue;
}
public class MACDPoint
{
    public int MACDIndex;
    public int LocationIndex;
    public float EMAShortValue;
    public float EMALongValue;
    public float DIFValue;
    public float DEAValue;
    public float MACDValue;
}
