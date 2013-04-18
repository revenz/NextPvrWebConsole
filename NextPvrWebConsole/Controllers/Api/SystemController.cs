using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Text.RegularExpressions;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize(Roles="System")]
    public class SystemController : NextPvrWebConsoleApiController
    {
        public IEnumerable<Models.DriveUsage> GetDriveStatistics()
        {
            var config = new Models.Configuration();

            Dictionary<char, Models.DriveUsage> drives = new Dictionary<char, Models.DriveUsage>();

            // add live tv buffer
            string liveTvBufferDir = config.LiveTvBufferDirectory;
            var liveTvDriveInfo = new System.IO.DriveInfo(liveTvBufferDir.Substring(0, 1));
            drives.Add((char)1, new Models.DriveUsage()
            {
                Name = "Live TV Buffer",
                Size = liveTvDriveInfo.TotalSize,
                FreeSpace = liveTvDriveInfo.TotalFreeSpace,
                Used = liveTvDriveInfo.TotalSize - liveTvDriveInfo.TotalFreeSpace,
                RecordingsSize = Size(new System.IO.DirectoryInfo(liveTvBufferDir))
            });

            foreach (var rd in Models.RecordingDirectory.LoadForUser(GetUser().Oid, true))
            {
                if (Regex.IsMatch(rd.Path, @"^[c-zC-Z]:\\"))
                {
                    char drive = rd.Path[0];
                    if (!drives.ContainsKey(drive))
                    {
                        var driveInfo = new System.IO.DriveInfo(drive.ToString());
                        drives.Add(drive, new Models.DriveUsage()
                        {
                            Name = drive.ToString().ToUpper() + " Drive Recordings",
                            Size = driveInfo.TotalSize,
                            FreeSpace = driveInfo.TotalFreeSpace,
                            Used = driveInfo.TotalSize - driveInfo.TotalFreeSpace
                        });
                    }
                    drives[drive].RecordingsSize += Size(new System.IO.DirectoryInfo(rd.Path));
                }
            }
            return drives.Values;
        }

        private long Size(System.IO.DirectoryInfo Dir)
        {
            if (!Dir.Exists)
                return 0;
            long size = 0;
            System.IO.FileSystemInfo[] filelist = Dir.GetFileSystemInfos();
            System.IO.FileInfo[] fileInfo;
            fileInfo = Dir.GetFiles("*", System.IO.SearchOption.AllDirectories);
            for (int i = 0; i < fileInfo.Length; i++)
            {
                try
                {
                    size += fileInfo[i].Length;
                }
                catch { }
            }
            return size;
        }
    }
}
