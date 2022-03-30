///系统引用汇集
global using System;
global using System.Collections.Generic;
global using System.Configuration;
global using System.Data;
global using System.IO;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Net.Http.Json;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Threading.Tasks;
global using System.Windows;
global using System.Windows.Controls;
global using System.Windows.Input;
global using System.Windows.Media;
global using System.Windows.Shapes;

namespace AnalyStock.DataManage;
public static class SystemParam
{
    internal static string SqlDbPath = ConfigurationManager.AppSettings["SqlDbPath"];
    internal static string InforOfDailyDbPath = ConfigurationManager.AppSettings["InforOfDailyDbPath"];
    internal static string BasicinfostockDbPath = ConfigurationManager.AppSettings["BasicinfostockDbPath"];
    internal static bool IsBusyOfDownDataOnLine { get; set; }
}
