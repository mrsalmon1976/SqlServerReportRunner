using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting.Executors
{
    public interface IReportWriter : IDisposable
    {
        void CreateFile(string filePath);

        void WriteHeader(IEnumerable<string> columnNames, string delimiter);

        void WriteLine(SqlDataReader reader, ColumnMetaData[] columnInfo, string delimiter);
    }
}
