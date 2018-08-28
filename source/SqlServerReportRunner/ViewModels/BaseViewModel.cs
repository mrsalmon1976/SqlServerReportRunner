using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.ViewModels
{
    public class BaseViewModel
    {
        public string AppVersion
        {
            get
            {
                Assembly a = Assembly.GetEntryAssembly();
                if (a == null)
                {
                    a = Assembly.GetExecutingAssembly();
                }
                return a.GetName().Version.ToString(3);
            }
        }

    }
}
