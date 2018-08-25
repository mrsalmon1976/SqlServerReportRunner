using Nancy;
using Nancy.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Modules
{
    public class CustomModule : NancyModule
    {
        public CustomModule()
        {
        }

        protected void AddScript(string script)
        {
            if (Context.ViewBag.Scripts == null)
            {
                Context.ViewBag.Scripts = new List<string>();
            }
            ((List<string>)Context.ViewBag.Scripts).Add(script);
        }

    }
}
