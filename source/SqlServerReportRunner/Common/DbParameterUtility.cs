using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SqlServerReportRunner.Common
{
    public interface IDbParameterUtility
    {
        SqlParameter[] ConvertXmlToDbParameters(string xml);
    }

    public class DbParameterUtility : IDbParameterUtility
    {
        public SqlParameter[] ConvertXmlToDbParameters(string xml)
        {
            List<SqlParameter> result = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(xml))
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                foreach (XmlNode node in doc.FirstChild.ChildNodes)
                {
                    result.Add(new SqlParameter(node.Name, node.InnerText));
                }
            }
            return result.ToArray();
        }
    }
}
