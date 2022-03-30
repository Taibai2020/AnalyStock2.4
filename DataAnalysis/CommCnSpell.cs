namespace AnalyStock.DataAnalysis;

public static class CommCNSpell
{
    /// <summary>

    /// 在指定的字符串列表CnStr中检索符合拼音索引字符串

    /// </summary>

    /// <param name="CnStr">汉字字符串</param>

    /// <returns>相对应的汉语拼音首字母串</returns>

    public static string GetChineseSpell(string CnStr)
    {
        string strTemp = string.Empty;
        int iLen = CnStr.Length;
        for (int i = 0; i < iLen; i++)
        {
            strTemp += GetFirstPingOfChinese(CnStr.Substring(i, 1));
        }
        return strTemp + strTemp.ToLower();
    }

    /// <summary>
    /// 得到一个汉字的拼音第一个字母，如果是一个英文字母则直接返回大写字母
    /// </summary>
    /// <param name="CnChar">单个汉字</param>
    /// <returns>单个大写字母</returns>
    private static string GetFirstPingOfChinese(string iCnChar)
    {
        if (System.Text.Encoding.Default.GetBytes(iCnChar).Length == 1)
        {
            return iCnChar.ToUpper();
        }
        else
        {

            if (iCnChar.CompareTo("芭") < 0)
            {
                return "A";
            }

            if (iCnChar.CompareTo("擦") < 0)
            {
                return "B";
            }

            if (iCnChar.CompareTo("搭") < 0)
            {
                return "C";
            }

            if (iCnChar.CompareTo("蛾") < 0)
            {
                return "D";
            }

            if (iCnChar.CompareTo("发") < 0)
            {
                return "E";
            }

            if (iCnChar.CompareTo("噶") < 0)
            {
                return "F";
            }

            if (iCnChar.CompareTo("哈") < 0)
            {
                return "G";
            }

            if (iCnChar.CompareTo("击") < 0)
            {
                return "H";
            }

            if (iCnChar.CompareTo("喀") < 0)
            {
                return "J";
            }

            if (iCnChar.CompareTo("垃") < 0)
            {
                return "K";
            }

            if (iCnChar.CompareTo("妈") < 0)
            {
                return "L";
            }

            if (iCnChar.CompareTo("拿") < 0)
            {
                return "M";
            }

            if (iCnChar.CompareTo("哦") < 0)
            {
                return "N";
            }

            if (iCnChar.CompareTo("啪") < 0)
            {
                return "O";
            }

            if (iCnChar.CompareTo("期") < 0)
            {
                return "P";
            }

            if (iCnChar.CompareTo("然") < 0)
            {
                return "Q";
            }

            if (iCnChar.CompareTo("撒") < 0)
            {
                return "R";
            }

            if (iCnChar.CompareTo("塌") < 0)
            {
                return "S";
            }

            if (iCnChar.CompareTo("挖") < 0)
            {
                return "T";
            }

            if (iCnChar.CompareTo("误") <= 0)
            {
                return "W";
            }

            if (iCnChar.CompareTo("压") < 0)
            {
                return "X";
            }

            if (iCnChar.CompareTo("匝") < 0)
            {
                return "Y";
            }

            if (iCnChar.CompareTo("座") < 0)
            {
                return "Z";
            }

            return "?";
        }


    }

}
