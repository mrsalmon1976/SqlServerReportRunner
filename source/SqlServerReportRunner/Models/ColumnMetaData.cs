using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Models
{
    public class ColumnMetaData
    {
        public string Name { get; set; }

        public string DataType { get; set; }

        public int Size { get; set; }
    }
}
