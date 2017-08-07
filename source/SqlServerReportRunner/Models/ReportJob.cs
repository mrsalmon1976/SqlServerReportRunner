using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Models
{
    public class ReportJob
    {
        public int Id { get; set; }

        public string ReportName { get; set; }

        /// <summary>
        /// Type of command to exexcute, with possible values being StoredProcedure, SQL, or SSRS.
        /// </summary>
        public string CommandType { get; set; }

        /// <summary>
        /// Details of the command to exexcute, e.g. stored procecure name, SQL Text, SSRS report name.
        /// </summary>
        public string Command { get; set; }

        public string Parameters { get; set; }

        public string UserName { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? ProcessStartDate { get; set; }

        public DateTime? ProcessEndDate { get; set; }

        public string Status { get; set; }

        public string OutputFileName { get; set; }

        public string OutputFilePath { get; set; }

        /// <summary>
        /// Format of the report e.g. Delimited, CSV, Excel.
        /// </summary>
        public string OutputFormat { get; set; }

        /// <summary>
        /// Gets/sets the delimiter for delimiter file format.  Value is ignored for other formats.
        /// </summary>
        public string Delimiter { get; set; }

        public string ErrorMessage { get; set; }

        public string ErrorStackTrace { get; set; }

    }
}
