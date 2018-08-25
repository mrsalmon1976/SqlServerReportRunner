using SqlServerReportRunner.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test.SqlServerReportRunner
{
    internal class TestUtility
    {
        internal static string TestDbConnectionString(string rootFolder)
        {
            string folder = Path.Combine(rootFolder, "App_Data");
            string connString = ConfigurationManager.ConnectionStrings["testdb"].ConnectionString;
            return connString.Replace("|DataDirectory|", folder);
        }

        internal static string TestRootFolder
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }
    }
}
