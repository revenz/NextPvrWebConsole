using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class SystemController : NextPvrWebConsoleApiController
    {
        public dynamic GetDriveStatistics()
        {
            var config = new Models.Configuration();
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(config.DefaultRecordingDirectoryRoot);            
            System.IO.DriveInfo drive = new System.IO.DriveInfo(dirInfo.FullName[0].ToString());
            long usedByRecordings = Size(dirInfo);
            return new 
            {
                Total = drive.TotalSize - usedByRecordings,
                Free = drive.TotalFreeSpace,
                Recordings = usedByRecordings,
                Used = drive.TotalSize - drive.TotalFreeSpace
            };
        }

        private long Size(System.IO.DirectoryInfo Dir)
        {
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
