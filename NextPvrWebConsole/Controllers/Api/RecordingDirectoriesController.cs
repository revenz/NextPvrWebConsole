using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NextPvrWebConsole.Controllers.Api
{
    [Authorize]
    public class RecordingDirectoriesController : ApiController
    {
        // GET api/recordingdirectories
        public IEnumerable<Models.RecordingDirectory> Get(bool IncludeShared = false)
        {
            return Models.RecordingDirectory.LoadForUser(this.GetUser().Oid, IncludeShared);
        }

        // POST api/recordingdirectories
        public Models.RecordingDirectory Post([FromBody] string RecordingDirectoryName)
        {
            var user = this.GetUser();
            if (String.IsNullOrWhiteSpace(RecordingDirectoryName) || !Models.RecordingDirectory.IsValidRecordingDirectoryName(RecordingDirectoryName))
                throw new ArgumentException("Invalid Recording Directory Name.");

            return Models.RecordingDirectory.Create(user.Oid, RecordingDirectoryName);
        }

        // PUT api/recordingdirectories
        public void Put(int Oid, [FromBody] string RecordingDirectoryName)
        {
            var user = this.GetUser();
            Models.RecordingDirectory original = Models.RecordingDirectory.Load(Oid);
            if (original == null)
                throw new ArgumentException("Recording Directory not found.");

            if (original.UserOid != user.Oid)
                throw new AccessViolationException();

            RecordingDirectoryName = RecordingDirectoryName.Trim();

            if (String.IsNullOrWhiteSpace(RecordingDirectoryName) || !Models.RecordingDirectory.IsValidRecordingDirectoryName(RecordingDirectoryName))
                throw new ArgumentException("Invalid Recording Directory Name.");

            var rd = Models.RecordingDirectory.LoadByName(user.Oid, RecordingDirectoryName);
            if (rd != null && rd.Oid != original.Oid)
                throw new ArgumentException("A Recording Directory with the name '{0}' already exists".FormatStr(RecordingDirectoryName));

            original.Name = RecordingDirectoryName;

            original.Save();
        }

        // DELETE api/recordingdirectories/5
        public void Delete(int Oid)
        {
            var user = this.GetUser();

            Models.RecordingDirectory.Delete(user.Oid, Oid);
        }
    }
}
