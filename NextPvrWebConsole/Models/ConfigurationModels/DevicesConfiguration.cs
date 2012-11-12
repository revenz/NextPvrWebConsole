using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models.ConfigurationModels
{
    public class DevicesConfiguration
    {
        public bool UseReverseOrderForLiveTv { get; set; }

        public List<Device> Devices { get; set; }

        public DevicesConfiguration()
        {
        }
    }
}