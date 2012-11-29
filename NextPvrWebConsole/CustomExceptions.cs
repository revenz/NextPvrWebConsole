using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NextPvrWebConsole
{
    public class SetupException:HttpException 
    {
        public SetupException() : base(510, "NextPVR Console Setup Required.") { }
    }
}