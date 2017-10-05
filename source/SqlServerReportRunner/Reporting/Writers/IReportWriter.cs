using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting.Writers
{
    public interface IReportWriter : IDisposable
    {
        /// <summary>
        /// Initialises the writer.  In some writer instances, the output file is created on initialisation.
        /// </summary>
        void Initialise(string filePath);

        void WriteHeader(IEnumerable<string> columnNames, string delimiter);

        void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo, string delimiter);
    }
}
