﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole.Models
{
    public class NextPvrWebConsoleModel
    {
        [PetaPoco.Ignore]
        public string ObjectType
        {
            get
            {
                return this.GetType().ToString();
            }
        }
    }
}