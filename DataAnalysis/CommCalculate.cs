using AnalyStock.DataManage;
namespace AnalyStock.DataAnalysis;

public static class CommCalculate
{
    internal static List<MAPoint> CalculateMAVal(this float[] inDataArry, int MA_period)
    {
        List<MAPoint> tmpMaSeries = new();
        for (int i = 0; i < inDataArry.Length; i++)
        {
            tmpMaSeries.Add(new()
            {
                Clcye = MA_period,
                MAIndex = i,
                LocationIndex = i,
                MAValue = i < MA_period - 1
                        ? inDataArry.Take(i + 1).Sum() / (i + 1)
                        : inDataArry.Skip(i + 1 - MA_period).Take(MA_period).Sum() / MA_period
            });
        }
        return tmpMaSeries;
    }
    internal static List<KDJPoint> CalculateKDJVal(int RSV_P, int K_P, int D_P, float[] inHigh, float[] inLow, float[] inClose)
    {
        List<KDJPoint> tmpKDJSeries = new();
        if (inClose.Length < RSV_P)
        {
            return tmpKDJSeries;
        }

        float maxHigh, minLow;
        float RSV, K, D, J;
        for (int i = RSV_P - 1; i < inClose.Length; i++)
        {
            minLow = inLow.Skip(i + 1 - RSV_P).Take(RSV_P).Min();
            maxHigh = inHigh.Skip(i + 1 - RSV_P).Take(RSV_P).Max();
            RSV = (inClose[i] - minLow) / (maxHigh - minLow) * 100.0f;
            if (i == RSV_P - 1)
            {
                K = ((K_P - 1) * 50.0f + RSV) / K_P;
                D = ((D_P - 1) * 50.0f + K) / D_P;
                J = 3.0f * K - 2.0f * D;
            }
            else
            {
                K = ((K_P - 1) * tmpKDJSeries[i - RSV_P].KValue + RSV) / K_P;
                D = ((D_P - 1) * tmpKDJSeries[i - RSV_P].DValue + K) / D_P;
                J = 3.0f * K - 2.0f * D;
            }
            KDJPoint tmpKdjvalue = new()
            {
                KDJIndex = i + 1 - RSV_P, //0开始的数据组索引
                LocationIndex = i,  //坐标轴索引
                RSValue = RSV,
                KValue = K > 0 ? (K < 100 ? K : 100) : 0,
                DValue = D > 0 ? (D < 100 ? D : 100) : 0,
                JValue = J > 0 ? (J < 100 ? J : 100) : 0,
            };
            tmpKDJSeries.Add(tmpKdjvalue);
        }
        return tmpKDJSeries;
    }
    internal static List<MACDPoint> CalculateMACDVal(this float[] inClose, int shortP = 12, int longP = 26, int DEA_P = 9)
    {
        List<MACDPoint> tmpMacdSeries = new();
        if (inClose.Length < 1)
        {
            return tmpMacdSeries;
        }

        for (int i = 0; i < inClose.Length; i++)
        {
            MACDPoint tmpMacdvalue = new();
            if (i == 0)
            {
                tmpMacdvalue.MACDIndex = 0;
                tmpMacdvalue.LocationIndex = 0;
                tmpMacdvalue.EMAShortValue = inClose[0];
                tmpMacdvalue.EMALongValue = inClose[0];
                tmpMacdvalue.DIFValue = 0;
                tmpMacdvalue.DIFValue = 0;
                tmpMacdvalue.MACDValue = 0;
            }
            else
            {
                tmpMacdvalue.MACDIndex = i;
                tmpMacdvalue.LocationIndex = i;
                tmpMacdvalue.EMAShortValue = (tmpMacdSeries[i - 1].EMAShortValue * (shortP - 1) + inClose[i] * 2.00f) / (shortP + 1);
                tmpMacdvalue.EMALongValue = (tmpMacdSeries[i - 1].EMALongValue * (longP - 1) + inClose[i] * 2.00f) / (longP + 1);
                tmpMacdvalue.DIFValue = tmpMacdvalue.EMAShortValue - tmpMacdvalue.EMALongValue;
                tmpMacdvalue.DEAValue = (tmpMacdSeries[i - 1].DEAValue * (DEA_P - 1) + tmpMacdvalue.DIFValue * 2.00f) / (DEA_P + 1);
                tmpMacdvalue.MACDValue = 2.00f * (tmpMacdvalue.DIFValue - tmpMacdvalue.DEAValue);
            }
            tmpMacdSeries.Add(tmpMacdvalue);
        }

        return tmpMacdSeries;
    }
    /// <summary>
    /// 追踪价格走势的拐点位置,价格波动大于waveRate时形成拐点
    /// </summary>
    /// <param name="waveRate"></param>
    /// <param name="inClose"></param>
    /// <returns></returns>        
    internal static Dictionary<int, string> CalcInflectPoint(this float[] inClose, float waveRate)
    {
        var dictIdxInflectPoints = new Dictionary<int, string>();  //追加拐点索引
                                                                   //初始化起始追踪点的索引，值，方向
        var trackingPoint = (
            Index: 0,
            Value: inClose[0],
            IsTrend: inClose[1] >= inClose[0]);
        //初始点赋值
        dictIdxInflectPoints.Add(trackingPoint.Index, "Start");
        //中间点赋值
        for (int i = 0; i < inClose.Length; i++)
        {
            if ((inClose[i] >= trackingPoint.Value) == trackingPoint.IsTrend)
            {
                trackingPoint = (i, inClose[i], trackingPoint.IsTrend);
                continue;
            }
            //方向不一致时，走势出现拐点，超过5%的波动后确认上一个起点为有效拐点。同时将该点确认为新的追踪点  
            if (MathF.Abs(inClose[i] - trackingPoint.Value) / trackingPoint.Value >= waveRate)
            {
                dictIdxInflectPoints.Add(trackingPoint.Index, trackingPoint.IsTrend ? "↓" : "↑");
                //开始新的追踪点,趋势方向改变；
                trackingPoint = (i, inClose[i], !trackingPoint.IsTrend);
            }
        }
        //结束点赋值
        dictIdxInflectPoints.Add(inClose.Length - 1, "End");
        return dictIdxInflectPoints;
    }

    /// <summary>
    /// 描述模拟函数式，计算模拟值，导数值
    /// </summary>
    /// <param name="funcParams"></param>
    /// <param name="xVal"></param>
    /// <returns></returns>
    internal static (string StrFunction, float FittingValue, float DiffValue)
        CalcFunctionValue(double[] funcParams, int xVal)
    {
        //模型式，计算模型值，导数值
        var (StrFunction, FitFuncValue, DiffValue) = ("", 0.0, 0.0);
        for (int i = 0; i < funcParams.Length; i++)
        {
            StrFunction += i is 0 ? $"{ funcParams[i]:F2}"
                                 : $"{(funcParams[i] >= 0 ? "+" : "")}{funcParams[i]:F3}X^{i}";
            FitFuncValue += funcParams[i] * MathF.Pow(xVal, i);
            DiffValue += i is 0 ? 0 : i * funcParams[i] * MathF.Pow(xVal, i - 1);
        }
        return (StrFunction, (float)FitFuncValue, (float)DiffValue);
    }
    /// <summary>
    /// 价格位置的标准化值转化，位置在（0-32%，32-68%，68-100%)=>(-1,0,1)
    /// </summary>
    /// <param name="valSection"></param>
    /// <returns></returns>
    internal static (int, int, int) RevisedValOnStandard((float, float, float) valSection)
    {
        var valStandard = (
             valSection.Item1 switch { < 0.32f => -1, >= 0.68f => 1, _ => 0, },
             valSection.Item2 switch { < 0.32f => -1, >= 0.68f => 1, _ => 0, },
             valSection.Item3 switch { < 0.32f => -1, >= 0.68f => 1, _ => 0, });
        //在三个区段内，初始判断值相同时:
        //如果相邻两段的值相同，但是两者价格幅度的差距超过20%，二次修订各自趋势赋值，高的加1，低的减去1，较好匹配趋势
        if (valStandard.Item1 == valStandard.Item2
            && MathF.Abs(valSection.Item2 - valSection.Item1) is >= 0.2f)
        {
            _ = valSection.Item1 > valSection.Item2 ? valStandard.Item1 += 1 : valStandard.Item2 += 1;
        }
        if (valStandard.Item2 == valStandard.Item3
            && MathF.Abs(valSection.Item2 - valSection.Item3) is >= 0.2f)
        {
            _ = valSection.Item2 > valSection.Item3 ? valStandard.Item2 += 1 : valStandard.Item3 += 1;
        }
        //修订值超过1则进行修订
        valStandard = (
            valStandard.Item1 > 1 ? 1 : valStandard.Item1,
            valStandard.Item2 > 1 ? 1 : valStandard.Item2,
            valStandard.Item3 > 1 ? 1 : valStandard.Item3);
        return valStandard;
    }

}
