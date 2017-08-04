using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace SqlServerReportRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = BootStrapper.Boot();
            HostFactory.Run(
                            configuration =>
                            {
                                var importFileWatcher = container.Resolve<IReportProcessorService>();
                                var appSettings = container.Resolve<IAppSettings>();

                                configuration.Service<IReportProcessorService>(
                                    service =>
                                    {
                                        service.ConstructUsing(x => importFileWatcher);
                                        service.WhenStarted(x => x.Start());
                                        service.WhenStopped(x => x.Stop());
                                    });

                                string serviceUserName = appSettings.ServiceUserName;
                                string servicePassword = appSettings.ServicePassword;

                                if (serviceUserName.Length > 0 && servicePassword.Length > 0)
                                {
                                    configuration.RunAs(serviceUserName, servicePassword);
                                }
                                else
                                {
                                    configuration.RunAsLocalSystem();
                                }

                                configuration.StartAutomatically();
                                configuration.SetServiceName("SqlServerReportRunner");
                                configuration.SetDisplayName("SqlServerReportRunner");
                                configuration.SetDescription("Runs SQL Server reports in a background queue");
                            });
        }
    }

}

