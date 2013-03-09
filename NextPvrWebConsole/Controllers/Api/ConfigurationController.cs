using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize(Roles="Configuration")]
    public class ConfigurationController : NextPvrWebConsoleApiController
    {
        // GET api/configuration
        public Models.Configuration Get()
        {
            return new Models.Configuration();
        }

        [HttpGet]
        public IEnumerable<Models.Device> Devices()
        {
            return Models.Device.LoadAll();
        }

        [HttpGet]
        public IEnumerable<Models.XmltvSource> XmlTvSources()
        {
            return Models.XmltvSource.LoadAll();
        }

        [HttpPost]
        public IEnumerable<Models.XmltvSource> XmlTvSources(Models.XmltvSource[] Sources)
        {
            if (!Models.XmltvSource.Save(Sources))
                throw new Exception("Failed to save.");
            return Sources;
        }

        public Models.XmltvSource XmlTvSourceScan(int Oid)
        {
            var xmltv = Models.XmltvSource.LoadByOid(Oid);
            if (xmltv == null)
                return null;
            xmltv.Scan();
            return xmltv;
        }

        [HttpPost]
        public IEnumerable<Models.XmltvSource> XmlTvSourceImport()
        {
            return Models.XmltvSource.ImportFromNextPvr();
        }
    }
}
