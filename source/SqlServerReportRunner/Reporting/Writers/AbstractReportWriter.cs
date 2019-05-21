using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlServerReportRunner.Models;

namespace SqlServerReportRunner.Reporting.Writers
{
    public abstract class AbstractReportWriter : IReportWriter
    {
        public string FilePath { get; protected set; }

        public string Delimiter { get; protected set; }

        public abstract void Dispose();

        public abstract void Initialise(string filePath, string delimiter);

        public abstract void WriteHeader(IEnumerable<string> columnNames);

        public abstract void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo);
    }
}
