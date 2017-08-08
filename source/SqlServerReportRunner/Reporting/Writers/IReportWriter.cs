using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting.Executors
{
    public interface IReportWriter : IDisposable
    {
        string FilePath { get; set; }

        void WriteHeader(IEnumerable<string> columnNames, string delimiter);

        void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo, string delimiter);
    }
}
