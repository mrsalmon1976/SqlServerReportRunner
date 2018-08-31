using Nancy;
using Nancy.ModelBinding;
using SqlServerReportRunner.BLL.Repositories;
using SqlServerReportRunner.Models.Console;
using SqlServerReportRunner.Modules.Navigation;
using SqlServerReportRunner.ViewModels.Dashboard;
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
        private IReportJobRepository _reportJobRepository;

        public DashboardModule(IAppSettings appSettings, IReportJobRepository reportJobRepository)
        {
            _appSettings = appSettings;
            _reportJobRepository = reportJobRepository;

            Get[Actions.Dashboard.Index] = x =>
            {
                AddScript(Scripts.Dashboard.Index);
                return this.DashboardIndex();
            };

            Post[Actions.Dashboard.Statistics] = x =>
            {
                return this.DashboardStatistics();
            };


        }

        public dynamic DashboardIndex()
        {
            var model = new ViewModels.Dashboard.IndexViewModel();
            model.ConnectionNames.AddRange(_appSettings.ConnectionSettings.Select(x => x.Name));
            return this.View[Views.Dashboard.Default, model];
        }

        public dynamic DashboardStatistics()
        {
            DashboardModel data = this.Bind<DashboardModel>();
            string connString = _appSettings.GetConnectionStringByName(data.ConnName);

            Task<int> totalReportCountTask = Task.Run(() =>_reportJobRepository.GetTotalReportCount(connString, data.StartDate, data.EndDate));
            Task.WaitAll(totalReportCountTask);

            StatisticsViewModel viewModel = new StatisticsViewModel();
            viewModel.TotalReportCount = totalReportCountTask.Result;

            return Response.AsJson(viewModel);


        }

    }
}