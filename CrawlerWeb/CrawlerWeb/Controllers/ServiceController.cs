using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CrawlerWeb.Controllers
{
    public class ServiceController : ApiController
    {
        [Route("api/getunis")]
        public string Get([FromUri(Name = "name", SuppressPrefixCheck = false)] string name = "",
            [FromUri(Name = "sname", SuppressPrefixCheck = false)] string secondName = "")
        {
            return "Hi there, " + name + " " + secondName + "!";
        }
    }
}
