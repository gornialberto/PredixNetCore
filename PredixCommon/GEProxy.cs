using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PredixCommon
{
    public class GEProxy : IWebProxy
    {
        public Uri GetProxy(Uri destination)
        {
            return new Uri("http://PITC-Zscaler-EMEA-London3PR.proxy.corporate.ge.com:80");
        }

        public bool IsBypassed(Uri host)
        {
            return false;
        }

        public ICredentials Credentials { get; set; }
    } 
}
