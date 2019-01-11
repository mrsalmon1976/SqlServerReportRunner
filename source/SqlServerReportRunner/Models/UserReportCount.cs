using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Models
{
    public class UserReportCount
    {

        public UserReportCount()
        {

        }

        public UserReportCount(string userName, int reportCount)
        {
            this.UserName = userName;
            this.ReportCount = reportCount;
        }

        public int ReportCount { get; set; }

        public string UserName { get; set; }
    }
}
