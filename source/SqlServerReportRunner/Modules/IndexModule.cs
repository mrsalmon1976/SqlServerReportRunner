using Nancy;
using Nancy.Responses;
using SqlServerReportRunner.Modules.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Modules
{
    public class IndexModule : CustomModule
    {
        public IndexModule()
        {
            Get["/"] = x =>
            {
                return this.Response.AsRedirect(Actions.Dashboard.Default);
            };
        }
    }
}