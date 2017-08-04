using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner
{

    public class CommandType
    {
        public const string StoredProcedure = "StoredProcedure";
    }

    public class JobStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Complete = "Complete";
        public const string Error = "Error";
    }
}
