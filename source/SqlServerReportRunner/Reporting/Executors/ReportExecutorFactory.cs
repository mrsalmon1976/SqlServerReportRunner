using SqlServerReportRunner.Common;
using SqlServerReportRunner.Models;
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

        public ReportExecutorFactory(IDbConnectionFactory dbConnectionFactory, IReportWriterFactory reportWriterFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _reportWriterFactory = reportWriterFactory;
        }

        public IReportExecutor GetReportExecutor(string commandType)
        {
            switch (commandType)
            {
                case CommandType.StoredProcedure:
                    return new StoredProcedureReportExecutor(_dbConnectionFactory, _reportWriterFactory);
                default:
                    break;
            }

            throw new NotImplementedException(String.Format("No report executor implemented for command type '{0}'", commandType));
        }

    }
}
