using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class RecordingDirectoriesController : NextPvrWebConsoleApiController
    {
        // GET api/recordingdirectories
        public IEnumerable<Models.RecordingDirectory> Get(bool IncludeShared = false)
        {
            return Models.RecordingDirectory.LoadForUser(this.GetUser().Oid, IncludeShared);
        }

        // POST api/recordingdirectories/updatename
        [HttpPost]
        public Models.RecordingDirectory UpdateName([FromBody] string RecordingDirectoryName)
        {
            var user = this.GetUser();
            if (String.IsNullOrWhiteSpace(RecordingDirectoryName) || !Models.RecordingDirectory.IsValidRecordingDirectoryName(RecordingDirectoryName))
                throw new ArgumentException("Invalid Recording Directory Name.");

            return Models.RecordingDirectory.Create(user.Oid, RecordingDirectoryName);
        }

        
        public bool Post(List<Models.RecordingDirectory> RecordingDirectories)
        {
            if (RecordingDirectories == null || RecordingDirectories.Count == 0)
                throw new ArgumentException("At least one Recording Directory is required.");
            var user = this.GetUser();
            return Models.RecordingDirectory.SaveForUser(user.Oid, RecordingDirectories);
        }

        // PUT api/recordingdirectories
        public bool Put(int Oid, [FromBody] string RecordingDirectoryName)
        {
            var user = this.GetUser();
            Models.RecordingDirectory original = Models.RecordingDirectory.Load(Oid);
            if (original == null)
                throw new ArgumentException("Recording Directory not found.");

            if (original.UserOid != user.Oid)
                throw new UnauthorizedAccessException();

            RecordingDirectoryName = RecordingDirectoryName.Trim();

            if (String.IsNullOrWhiteSpace(RecordingDirectoryName) || !Models.RecordingDirectory.IsValidRecordingDirectoryName(RecordingDirectoryName))
                throw new ArgumentException("Invalid Recording Directory Name.");

            var rd = Models.RecordingDirectory.LoadByName(user.Oid, RecordingDirectoryName);
            if (rd != null && rd.Oid != original.Oid)
                throw new ArgumentException("A Recording Directory with the name '{0}' already exists".FormatStr(RecordingDirectoryName));

            original.Name = RecordingDirectoryName;

            original.Save();

            return true;
        }

        // DELETE api/recordingdirectories/5
        public bool Delete(int Oid)
        {
            var user = this.GetUser();

            Models.RecordingDirectory.Delete(user.Oid, Oid);

            return true;
        }
    }
}
