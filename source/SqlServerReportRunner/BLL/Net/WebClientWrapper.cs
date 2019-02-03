using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.BLL.Net
{
    public interface IWebClientWrapper : IDisposable
    {
        bool UseDefaultCredentials { get; set; }

        void DownloadFile(string url, string filePath);
    }

    public class WebClientWrapper : WebClient, IWebClientWrapper
    {

        public WebClientWrapper() : base()
        {
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 10 * 60 * 1000;
            return w;
        }

    }
}
