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
        private IDbConnectionFactory _dbConnectionFactory;
        private IReportWriterFactory _reportWriterFactory;
        private IDbParameterUtility _dbParameterUtility;

        public ReportExecutorFactory(IDbConnectionFactory dbConnectionFactory, IReportWriterFactory reportWriterFactory, IDbParameterUtility dbParameterUtility)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _reportWriterFactory = reportWriterFactory;
            _dbParameterUtility = dbParameterUtility;
        }

        public IReportExecutor GetReportExecutor(string commandType)
        {
            switch (commandType.ToLower())
            {
                case CommandType.Sql:
                case CommandType.StoredProcedure:
                    return new StoredProcedureReportExecutor(_dbConnectionFactory, _reportWriterFactory, _dbParameterUtility);
                default:
                    break;
            }

            throw new NotImplementedException(String.Format("No report executor implemented for command type '{0}'", commandType));
        }

    }
}
