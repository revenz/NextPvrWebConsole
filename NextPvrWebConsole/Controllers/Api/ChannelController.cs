using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize(Roles="Guide")]
    public class ChannelController : NextPvrWebConsoleApiController
    {
        // GET api/channels
        public IEnumerable<Models.Channel> Get(bool IncludeDisabled = false)
        {
            return Models.Channel.LoadAll(this.GetUser().Oid, IncludeDisabled);
        }

        public IEnumerable<Models.Channel> GetShared()
        {
            return Models.Channel.LoadAll(Globals.SHARED_USER_OID, true);
        }

        public IEnumerable<Models.Channel> GetConfigurationChannels()
        {
            var xmlTvSources = Models.XmltvSource.LoadAll();
            var coreChannels = NUtility.Channel.LoadAll();
            return Models.Channel.LoadAll(Globals.SHARED_USER_OID, true).ForEach<Models.Channel>(x =>
            {
                var core = coreChannels.Where(y => y.OID == x.Oid).FirstOrDefault();
                if (core != null)
                {
                    x.EpgSource = core.EPGSource;
                    x.XmlTvChannel = Regex.Match(core.EPGMapping, "(?<=(<mapping_name>))[^<]+").Value;
                    string xmlFile = Regex.Match(core.EPGMapping, "(?<=(<file>))[^<]+").Value;
                    if (!String.IsNullOrWhiteSpace(xmlFile))
                    {
                        var source = xmlTvSources.Where(y => y.Filename.ToLower().Trim() == xmlFile.ToLower().Trim())
                                                             .FirstOrDefault();
                        if (source != null)
                        {
                            x.EpgSource = "XMLTV-" + source.Oid;
                            x.XmlTvChannel = source.Channels.Where(y => y.Name.ToLower() == x.XmlTvChannel.ToLower())
                                                            .Select(y => y.Oid).FirstOrDefault() ?? x.XmlTvChannel;
                        }
                    }
                }
            });
        }

        [HttpPost]
        public bool UpdateShared(Models.Channel[] Channels)
        {
            Models.Channel.SaveForUser(Globals.SHARED_USER_OID, Channels.ToList());
            return true;
        }

        [HttpGet]
        public bool EmptyEpg()
        {
            Helpers.NpvrCoreHelper.EmptyEpg();
            return true;
        }

        [HttpGet]
        public bool UpdateEpg()
        {
            Helpers.NpvrCoreHelper.UpdateEpg();
            return true;
        }

        [HttpGet]
        public IEnumerable<Models.Channel> ImportMissing()
        {
            int[] knownChannels = Models.Channel.LoadAll(Globals.SHARED_USER_OID, true).Select(x => x.Oid).OrderBy(x => x).ToArray();
            var npvrChannels = Helpers.Cacher.RetrieveOrStore<List<NUtility.Channel>>("NUtility.Channel.LoadAll", new TimeSpan(1, 0, 0), delegate
            {
                return NUtility.Channel.LoadAll(); 
            });
            return npvrChannels.Where(x => !knownChannels.Contains(x.OID)).OrderBy(x => x.Number).Select(x => new Models.Channel() { Oid = x.OID, Name = x.Name, Number = x.Number, Enabled = true }).OrderBy(x => x.Oid);
        }

        [HttpGet]
        public IEnumerable<NUtility.Channel> NpvrChannels()
        {
            return NUtility.Channel.LoadAll(); 
        }

    }
}
