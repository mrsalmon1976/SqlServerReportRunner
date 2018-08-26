using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.ViewModels.Dashboard
{
    public class IndexViewModel : BaseViewModel
    {
        private List<string> _connectionNames = new List<string>();

        public List<string> ConnectionNames
        {
            get
            {
                return _connectionNames;
            }
        }

    }
}
