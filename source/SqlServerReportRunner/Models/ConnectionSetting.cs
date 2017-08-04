using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Models
{
    public class ConnectionSetting
    {
        public ConnectionSetting(string name, string connectionString)
        {
            this.Name = name;
            this.ConnectionString = connectionString;
        }

        public string Name { get; set; }

        public string ConnectionString { get; set; }
    }
}
