using System.Reflection;
namespace AnalyStock.DataManage;
/// <summary>
/// 将CSV文件的数据读取到DataTable中
/// </summary>
/// <param name="fileName">CSV文件路径</param>
/// <returns>返回读取了CSV数据的DataTable</returns>
public class CsvDataManage
{
    public static DataTable ReadDataofCSV(string filePath)
    {
        //Encoding encoding= Encoding.GetEncoding(filePath); //Encoding.ASCII;
        DataTable dt = new();
        FileStream fs = new(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
        //StreamReader sr = new StreamReader(fs, Encoding.UTF8);
        StreamReader sr = new(fs, Encoding.Default);
        //标示是否是读取的第一行
        bool IsFirst = true;
        string strLine;
        //逐行读取CSV中的数据
        while ((strLine = sr.ReadLine()) != null)
        {
            if (IsFirst)
            {
                foreach (var item in strLine.Split(','))
                {
                    dt.Columns.Add(item); //创建列
                }
                IsFirst = false;
            }
            else
            {
                dt.Rows.Add(strLine.Split(','));
            }
        }
        sr.Close();
        fs.Close();
        return dt;
    }

    /// <summary>
    /// 将DataTable中数据写入到CSV文件中
    /// </summary>
    /// <param name="dt">提供保存数据的DataTable</param>
    /// <param name="fileName">CSV的文件路径</param>
    public static void SaveDataToCSV<T>(List<T> lsT, string fullPath) where T : class, new()
    {
        if (string.IsNullOrEmpty(fullPath))
        {
            return;
        }

        FileInfo fi = new(fullPath);
        if (!fi.Directory.Exists)
        {
            fi.Directory.Create();
        }
        using FileStream fs = new(fullPath, FileMode.Create, FileAccess.Write);
        using StreamWriter sw = new(fs, Encoding.UTF8);//System.Text.Encoding.Default)
        PropertyInfo[] propertys = typeof(T).GetProperties();
        sw.WriteLine(string.Join(",", propertys.Select(n => n.Name).ToArray()));
        //写出各行数据            
        string[] strLine = new string[propertys.Length];
        foreach (T item in lsT)
        {
            int i = 0;
            foreach (PropertyInfo pi in propertys) //遍历该对象的所有属性
            {
                strLine[i++] = pi.GetValue(item).ToString();
            }
            sw.WriteLine(string.Join(",", strLine));
        }
        sw.Close();
        fs.Close();
    }
}
