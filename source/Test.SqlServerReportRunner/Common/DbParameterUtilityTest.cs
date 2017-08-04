using NUnit.Framework;
using SqlServerReportRunner.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SqlServerReportRunner.Tests.Common
{
    [TestFixture]
    public class DbParameterUtilityTest
    {
        private IDbParameterUtility _dbParameterUtility;

        [SetUp]
        public void DbParameterUtilityTest_SetUp()
        {
            _dbParameterUtility = new DbParameterUtility();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ConvertXmlToDbParameters_NullOrEmptyXml_ReturnsEmptyArray(string xml)
        {
            SqlParameter[] result = _dbParameterUtility.ConvertXmlToDbParameters(xml);
            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void ConvertXmlToDbParameters_SingleParameter_ConvertsCorrectly()
        {
            string xml = "<row><Arg1>1</Arg1></row>";
            SqlParameter[] result = _dbParameterUtility.ConvertXmlToDbParameters(xml);

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("Arg1", result[0].ParameterName);
            Assert.AreEqual("1", result[0].Value);

        }

        [Test]
        public void ConvertXmlToDbParameters_TwoParameters_ConvertsCorrectly()
        {
            string xml = "<row><Arg1>1</Arg1><Arg2>TWO</Arg2></row>";
            SqlParameter[] result = _dbParameterUtility.ConvertXmlToDbParameters(xml);

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("Arg1", result[0].ParameterName);
            Assert.AreEqual("1", result[0].Value);
            Assert.AreEqual("Arg2", result[1].ParameterName);
            Assert.AreEqual("TWO", result[1].Value);

        }
    }
}
