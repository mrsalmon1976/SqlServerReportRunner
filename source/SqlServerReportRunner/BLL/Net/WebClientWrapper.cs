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

        /// <summary>
        /// Gets or sets the length of time, in milliseconds, that the web request times out.
        /// </summary>
        int Timeout { get; set; }
    }

    public class WebClientWrapper : WebClient, IWebClientWrapper
    {

        public WebClientWrapper(int timeout) : base()
        {
            this.Timeout = timeout;
        }

        /// <summary>
        /// Gets or sets the length of time, in milliseconds, that the web request times out.
        /// </summary>
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = this.Timeout;
            return w;
        }

    }
}
