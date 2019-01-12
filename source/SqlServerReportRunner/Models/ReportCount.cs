using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Models
{
    public class ReportCount
    {

        public ReportCount()
        {

        }

        public ReportCount(string key, int count)
        {
            this.Key = key;
            this.Count = count;
        }

        public int Count { get; set; }

        public string Key { get; set; }
    }
}
