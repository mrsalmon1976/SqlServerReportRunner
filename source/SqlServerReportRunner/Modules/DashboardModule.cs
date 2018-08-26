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

        private IAppSettings _appSettings;

        public DashboardModule(IAppSettings appSettings)
        {
            _appSettings = appSettings;

            Get[Actions.Dashboard.Default] = x =>
            {
                AddScript(Scripts.Dashboard.Index);
                return this.DashboardGet();
            };

        }

        public dynamic DashboardGet()
        {
            var model = new ViewModels.Dashboard.IndexViewModel();
            model.ConnectionNames.AddRange(_appSettings.ConnectionSettings.Select(x => x.Name));
            return this.View[Views.Dashboard.Default, model];
        }

    }
}