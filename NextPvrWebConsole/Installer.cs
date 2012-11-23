using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.IO;
using System.Text;


namespace NextPvrWebConsole
{
    [RunInstaller(true)]
    public partial class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            InitializeComponent();
        }
    
        private Guid AppId = new Guid("3C12BF4E-DF3A-4D50-8391-3EB051409901");

        public override void Install(IDictionary stateSaver)
        {
            StringBuilder log = new StringBuilder();
            log.AppendLine("Started");
            try
            {
                base.Install(stateSaver);
                string installDir = Context.Parameters["DP_TargetDir"];
                log.AppendLine("installDir: " + installDir);

                string regapp = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), @"UltiDev\Web Server\UWS.RegApp.exe");
                if (!File.Exists(regapp))
                    regapp = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), @"UltiDev\Web Server\UWS.RegApp.exe");
                log.AppendLine("regapp: " + regapp);
                if (!File.Exists(regapp))
                    throw new Exception("Failed to located UltiDev web server.");
                log.AppendLine("about to execute");
                System.Diagnostics.Process.Start(regapp,
                    "/r /AppID=\"{0}\" /aspnet:4 /force32 /url=http://*:8877/ /AppName=NextPVRWebConsole /path:\"{1}\" /vdir:/".FormatStr(AppId.ToString(), installDir));
                log.AppendLine("executed");
            }
            catch (Exception ex)
            {
                log.AppendLine("Error: " + ex.Message);
            }
            //System.IO.File.WriteAllText(log.ToString(), @"C:\nextpvr.log");
        }
    }
}
