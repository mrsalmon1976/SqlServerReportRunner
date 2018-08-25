using Nancy.TinyIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner
{
    public class BootStrapper
    {
        public static TinyIoCContainer Boot()
        {
            var container = TinyIoCContainer.Current;
            container.AutoRegister();
            return container;
        }
    }
}
