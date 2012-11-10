using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace NextPvrWebConsole.Controllers
{
    [Authorize]
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
            }
            builder.AppendLine("</ul>");

            return Content(builder.ToString());
        }

        private string AddDirectory(string Name, string Path, bool Selected = false, string _Class = "directory")
        {
            return "<li class=\"{3} collapsed{2}\"><a href=\"#\" rel=\"{0}\">{1}</a></li>\n".FormatStr(Path, Name, Selected ? " selected" : "", _Class);
        }
    }
}
