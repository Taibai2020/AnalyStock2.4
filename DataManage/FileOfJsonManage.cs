namespace AnalyStock.DataManage;
public class JsonDataManage
{
    public static void SaveJosnFileOfSeleStockBySelf(List<SeleStockofSelf> seleStockofSelf)
    {
        string tmpListJson = JsonSerializer.Serialize(seleStockofSelf);
        try
        {
            DeleJsonFile(SystemParam.SqlDbPath + "SeleStockBySelf.Json");
            FileStream fs = new(SystemParam.SqlDbPath + "SeleStockBySelf.Json", FileMode.Create);
            StreamWriter sw = new(fs);
            //开始写入
            sw.Write(System.Text.RegularExpressions.Regex.Unescape(tmpListJson)); //反转义字符串转义函数处理JSON数据
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close(); sw.Dispose();
            fs.Close(); fs.Dispose();
        }
        catch (Exception e)
        {
            throw new Exception("Json文件保存异常：" + e.Message);
        }
    }
    public static List<SeleStockofSelf> ReadJsonFileOfSeleStockBySelf()
    {
        List<SeleStockofSelf> seleStockofSelf = new();
        try
        {
            if (File.Exists(SystemParam.SqlDbPath + "SeleStockBySelf.Json"))
            {
                FileStream fs = new(SystemParam.SqlDbPath + "SeleStockBySelf.Json", FileMode.Open);
                StreamReader sr = new(fs);
                //开始写入
                string strJson = sr.ReadToEnd();
                seleStockofSelf = JsonSerializer.Deserialize<List<SeleStockofSelf>>(strJson);
                //关闭流
                sr.Close();
                fs.Close();
                sr.Dispose();
                fs.Dispose();
            }

        }
        catch (Exception e)
        {
            throw (new Exception("Json文件读取异常：" + e.Message));
        }
        return seleStockofSelf;
    }
    public static void SaveFailRecordToJson()
    {
        if (!ErrorLogPool.ErrorLogs.Any())
        {
            return;
        }
        string tmpListJson = JsonSerializer.Serialize(ErrorLogPool.ErrorLogs);
        try
        {
            DeleJsonFile(SystemParam.SqlDbPath + "ErrorLogs.Json");
            FileStream fs = new(SystemParam.SqlDbPath + "ErrorLogs.Json", FileMode.Create);
            StreamWriter sw = new(fs);
            //开始写入
            sw.Write(System.Text.RegularExpressions.Regex.Unescape(tmpListJson)); //反转义字符串函数处理JSON数据
            //清空缓冲区
            sw.Flush();
            //关闭流
            sw.Close(); sw.Dispose();
            fs.Close(); fs.Dispose();
        }
        catch (Exception e)
        {
            throw (new Exception("Json文件保存异常：" + e.Message));
        }
    }
    private static void DeleJsonFile(string pathfile)
    {
        try
        {
            if (File.Exists(pathfile)) { File.Delete(pathfile); }
        }
        catch (Exception e)
        {
            throw (new Exception("删除XML文件异常：" + e.Message));
        }
    }
}
