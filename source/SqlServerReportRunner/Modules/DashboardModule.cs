using SqlServerReportRunner.Modules.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Modules
{
    public class DashboardModule : CustomModule
    {
        public DashboardModule()
        {
            Get[Actions.Dashboard.Default] = x =>
            {
                //AddScript(Scripts.LoginView);
                return this.DashboardGet();
            };

        }

        public dynamic DashboardGet()
        {
            var model = new Object();// this.Bind<LoginViewModel>();
            return this.View[Views.Dashboard.Default, model];
        }

    }
}