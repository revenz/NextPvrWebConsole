using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize(Roles="Configuration")]
    public class FileController : Controller
    {
        public ActionResult CreateDirectory(string Path, string Name)
        {
            if (!System.IO.Directory.Exists(Path))
                return Json(new { success = false, message = "Directory does not exist." });
            try
            {
                string fullpath = System.IO.Path.Combine(Path, Name);
                if (System.IO.Directory.CreateDirectory(fullpath).Exists)
                    return Json(new { success = true, path =  fullpath});
                return Json(new { success = false, message = "Failed to create directory." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult LoadDirectory(string dir)
        {
            return LoadDirectory(dir, null);
        }

        public ActionResult LoadDirectoryAndXml(string dir)
        {
            return LoadDirectory(dir, "xml");
        }

        public ActionResult LoadDirectory(string dir, params string[] AdditionalFiles)
        {
            dir = HttpUtility.UrlDecode(dir);
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("<ul class=\"jqueryFileTree\" style=\"display: none;\">\n");
            if (dir == "%root%")
            {
                // list drives
                bool first = true;
                foreach (var drive in System.IO.DriveInfo.GetDrives())
                {
                    if (drive.DriveType != System.IO.DriveType.Fixed)
                        continue;
                    builder.AppendLine(AddDirectory(drive.Name, drive.RootDirectory.FullName, first, "directory hdd"));
                    first = false;
                }
                builder.AppendLine(AddDirectory("Network", "%network%", first, "directory network"));
            }
            else if (dir == "%network%")
            {
                // list network
                foreach (string computer in ListNetworkComputers())
                {
                    builder.AppendLine(AddDirectory(computer, @"\\{0}".FormatStr(computer), false, "directory computer"));
                }
            }
            else if (Regex.IsMatch(dir, @"^\\\\[^\\]+$"))
            {
                try
                {
                    foreach (string di_child in GetSharesList(dir.Substring(2)))
                    {
                        builder.AppendLine(AddDirectory(di_child, Path.Combine(dir, di_child)));
                    }
                }
                catch (Exception) { /* might not have access to that folder */ }
            }
            else
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);
                try
                {
                    foreach (System.IO.DirectoryInfo di_child in di.GetDirectories())
                    {
                        builder.AppendLine(AddDirectory(di_child.Name, di_child.FullName));
                    }
                }
                catch (Exception) { /* might not have access to that folder */ }

                if (AdditionalFiles != null && AdditionalFiles.Length > 0)
                {
                    try
                    {
                        foreach (var file in di.GetFiles().Where(x => x.Extension != null && AdditionalFiles.Contains((x.Extension.StartsWith(".") ? x.Extension.Substring(1) : x.Extension).ToLower())))
                            builder.AppendLine(AddFile(file.Name, file.FullName));
                    }
                    catch (Exception) {/* might not have access to that folder */ }
                }
            }
            builder.AppendLine("</ul>");

            return Content(builder.ToString());
        }

        private string AddDirectory(string Name, string Path, bool Selected = false, string _Class = "directory")
        {
            return "<li class=\"{3} collapsed{2}\"><a href=\"#\" rel=\"{0}\">{1}</a></li>\n".FormatStr(Path, Name, Selected ? " selected" : "", _Class);
        }

        private string AddFile(string Name, string Path, bool Selected = false, string _Class = "file")
        {
            return "<li class=\"{3} collapsed{2}\"><a href=\"#\" rel=\"{0}\">{1}</a></li>\n".FormatStr(Path, Name, Selected ? " selected" : "", _Class);
        }

        private List<string> ListNetworkComputers()
        {
            return Helpers.NetworkBrowser.getNetworkComputers();
        }

        /// <summary>
        /// Get the list of the network shares of the specified computer
        /// </summary>
        /// <param name="machineName">Name of the machine to request</param>
        /// <returns>Returns a list of the shares names</returns>
        static List<string> GetSharesList(string machineName)
        {
            List<String> results = new List<String>();
            foreach (Trinet.Networking.Share share in Trinet.Networking.ShareCollection.GetShares(machineName))
            {
                if(share.ShareType == Trinet.Networking.ShareType.Disk)
                    results.Add(share.NetName);
            }
            return results.OrderBy(x => x).ToList();
        }
    }
}
