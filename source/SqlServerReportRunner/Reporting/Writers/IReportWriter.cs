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
        /// Gets the file path the writer is sending the file to.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Gets the delimiter being used to separate columns.
        /// </summary>
        string Delimiter { get; }

        /// <summary>
        /// Initialises the writer.  In some writer instances, the output file is created on initialisation.
        /// </summary>
        /// <param name="filePath">The path to the output file.</param>
        /// <param name="delimiter">The delimiter used for the file.  Not necessary for binary report writers e.g. Excel.</param>
        void Initialise(string filePath, string delimiter);

        void WriteHeader(IEnumerable<string> columnNames);

        void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo);

    }
}
