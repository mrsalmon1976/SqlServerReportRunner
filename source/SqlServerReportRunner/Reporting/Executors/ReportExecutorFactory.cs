using SqlServerReportRunner.BLL.Net;
using SqlServerReportRunner.Common;
using SqlServerReportRunner.Constants;
using SqlServerReportRunner.Reporting.Writers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting.Executors
{
    public interface IReportExecutorFactory
    {
        IReportExecutor GetReportExecutor(string commandType);
    }
    public class ReportExecutorFactory : IReportExecutorFactory
    {
        private IAppSettings _appSettings;

        public ReportExecutorFactory(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public IReportExecutor GetReportExecutor(string commandType)
        {
            switch (commandType.ToLower())
            {
                case CommandType.Sql:
                case CommandType.StoredProcedure:
                    return new StoredProcedureReportExecutor(new DbConnectionFactory(), new ReportWriterFactory(), new DbParameterUtility());
                case CommandType.Ssrs:
                    return new ReportingServicesReportExecutor(new WebClientWrapper(_appSettings.ReportingServicesRequestTimeout));
                default:
                    break;
            }

            throw new NotImplementedException(String.Format("No report executor implemented for command type '{0}'", commandType));
        }

    }
}
