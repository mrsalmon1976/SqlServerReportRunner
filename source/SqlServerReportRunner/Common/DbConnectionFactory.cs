using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Common
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection(string connectionString, bool open = true);
    }

    public class DbConnectionFactory : IDbConnectionFactory
    {
        public IDbConnection CreateConnection(string connectionString, bool open = true)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            if (open)
            {
                conn.Open();
            }
            return conn;
        }
    }
}
