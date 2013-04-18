﻿using NextPvrWebConsole.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace NextPvrWebConsole.Models.ConfigurationModels
{
    public class GeneralConfiguration 
    {
        [Required]
        [DisplayName("Live TV Buffer")]
        [Directory]
        public string LiveTvBufferDirectory { get; set; }

        public bool UpdateDvbEpgDuringLiveTv { get; set; }

        [Range(0, 23)]
        [DisplayName("EPG Update Hour")]
        public int EpgUpdateHour { get; set; }

        public bool EnableUserSupport { get; set; }

        [DisplayName("User Recording Directory")]
        [Directory(Required = false)]
        public string UserBaseRecordingDirectory { get; set; }

    }
}