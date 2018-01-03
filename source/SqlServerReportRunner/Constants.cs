using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Constants
{

    public class CommandType
    {
        public const string StoredProcedure = "storedprocedure";
        public const string Sql = "sql";
    }

    public class JobStatus
    {
        public const string Pending = "Pending";
        public const string Processing = "Processing";
        public const string Complete = "Complete";
        public const string Error = "Error";
    }

    public class ReportFormat
    {
        public const string Csv = "csv";
        public const string Delimited = "delimited";
        public const string Excel = "excel";
    }
}
