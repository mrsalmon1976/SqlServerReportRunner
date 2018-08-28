using Nancy;
using Nancy.Testing;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using SqlServerReportRunner;
using SqlServerReportRunner.BLL.Repositories;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Modules;
using SqlServerReportRunner.Modules.Navigation;
using SqlServerReportRunner.ViewModels.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.SqlServerReportRunner.Modules
{
    [TestFixture]
    public class DashboardModuleTest
    {

        private IAppSettings _appSettings;
        private IReportJobRepository _reportJobRepository;

        [SetUp]
        public void DashboardModuleTest_SetUp()
        {
            _appSettings = Substitute.For<IAppSettings>();
            _reportJobRepository = Substitute.For<IReportJobRepository>();
        }

        #region DashboardIndex Tests

        [Test]
        public void DashboardIndex_OnExecute_ConnectionsAppearInHtml()
        {
            // setup
            string connName = Guid.NewGuid().ToString();

            var browser = CreateBrowser();
            _appSettings.ConnectionSettings.Returns(new ConnectionSetting[] { new ConnectionSetting(connName, Guid.NewGuid().ToString()) });

            // execute
            var response = browser.Get(Actions.Dashboard.Index, (with) =>
            {
                with.HttpRequest();
                //with.FormsAuth(currentUser.Id, new Nancy.Authentication.Forms.FormsAuthenticationConfiguration());
                //with.Query("id", connectionId.ToString());
            });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            string body = response.Body.AsString();
            Assert.IsTrue(body.Contains(connName));
        }

        #endregion

        #region DashboardStatistics Tests

        [Test]
        public void DashboardStatistics_OnExecute_PopulatesModel()
        {
            // setup
            string connName = Guid.NewGuid().ToString();
            string connString = Guid.NewGuid().ToString();
            int totalReportCount = new Random().Next();

            _appSettings.GetConnectionStringByName(connName).Returns(connString);
            _reportJobRepository.GetTotalReportCount(connString).Returns(totalReportCount);

            var browser = CreateBrowser();

            // execute
            var response = browser.Post(Actions.Dashboard.Statistics, (with) =>
            {
                with.HttpRequest();
                with.FormValue("ConnName", connName);
            });
            StatisticsViewModel result = JsonConvert.DeserializeObject<StatisticsViewModel>(response.Body.AsString());

            // assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.AreEqual(totalReportCount, result.TotalReportCount);
            _appSettings.Received(1).GetConnectionStringByName(connName);
            _reportJobRepository.Received(1).GetTotalReportCount(connString);
        }

        #endregion


        #region Private Methods

        private Browser CreateBrowser()
        {
            var browser = new Browser((bootstrapper) =>
                            bootstrapper.Module(new DashboardModule(_appSettings, _reportJobRepository))
                                .RootPathProvider(new TestRootPathProvider())
                                .RequestStartup((container, pipelines, context) => {
                                    //context.CurrentUser = currentUser;
                                    context.ViewBag.Scripts = new List<string>();
                                    //context.ViewBag.Claims = new List<string>();
                                    //context.CurrentUser = currentUser;
                                    //if (currentUser != null)
                                    //{
                                    //    context.ViewBag.CurrentUserName = currentUser.UserName;
                                    //}
                                })
                            );
            return browser;
        }

        /// <summary>
        /// Override the root path provider for testing so Nancy can locate the views.
        /// </summary>
        public class TestRootPathProvider : Nancy.IRootPathProvider
        {
            public string GetRootPath()
            {
                return TestContext.CurrentContext.TestDirectory;
            }
        }

        #endregion
    }
}
