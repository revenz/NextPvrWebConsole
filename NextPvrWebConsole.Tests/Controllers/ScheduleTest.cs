//using System;
//using System.Text;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace NextPvrWebConsole.Tests.Controllers
//{
//    [TestClass]
//    public class ScheduleTest : NextPvrWebConsoleTest
//    {
//        public ScheduleTest()
//        {              
//        }

//        private Models.EpgListing GetListingToRecord(NUtility.DayMask DayMask = NUtility.DayMask.ANY)
//        {
//            var controller = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);
//            string groupName = Models.ChannelGroup.LoadAll(User.Oid, false).First().Name;

//            DateTime date = DateTime.Now;
//            switch (DayMask)
//            {
//                case NUtility.DayMask.MONDAY: date = date.Next(DayOfWeek.Monday); break;
//                case NUtility.DayMask.TUESDAY: date = date.Next(DayOfWeek.Tuesday); break;
//                case NUtility.DayMask.WEDNESDAY: date = date.Next(DayOfWeek.Wednesday); break;
//                case NUtility.DayMask.THURSDAY: date = date.Next(DayOfWeek.Thursday); break;
//                case NUtility.DayMask.FRIDAY: date = date.Next(DayOfWeek.Friday); break;
//                case NUtility.DayMask.SATURDAY: date = date.Next(DayOfWeek.Saturday); break;
//                case NUtility.DayMask.SUNDAY: date = date.Next(DayOfWeek.Sunday); break;
//                case NUtility.DayMask.SATURDAY | NUtility.DayMask.SUNDAY: if (date.DayOfWeek != DayOfWeek.Sunday) { date = date.Next(DayOfWeek.Saturday); } break;
//                case NUtility.DayMask.MONDAY | NUtility.DayMask.TUESDAY | NUtility.DayMask.WEDNESDAY | NUtility.DayMask.THURSDAY | NUtility.DayMask.FRIDAY: if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) { date = date.Next(DayOfWeek.Monday); } break;
//            }

//            var listings = controller.Get(date, groupName);

//            var listing = listings.Where(x => x.Listings.Count > 0).SelectMany(x => x.Listings)
//                                  .Where(x => x.StartTime > DateTime.UtcNow.AddMinutes(15))
//                                  .OrderBy(x => Guid.NewGuid()) // random order
//                                  .FirstOrDefault();
//            return listing;
//        }

//        [TestMethod]
//        public void ScheduleTest_QuickRecord()
//        {
//            var listing = GetListingToRecord();
//            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);

//            Assert.IsNotNull(guide.QuickRecord(listing.Oid));
                
//            // load pending to ensure its due to record
//            var recordingController = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingsController>(User);
//            var pending = recordingController.Pending();
//            var recording = pending.Where(x => x.EventOid == listing.Oid).FirstOrDefault();
//            Assert.IsNotNull(recording);

//            // cancel the recording
//            Assert.IsTrue(recordingController.Delete(recording.OID));
//        }

//        [TestMethod]
//        public void ScheduleTest_RecordOnce()
//        {
//            var listing = GetListingToRecord();
//            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);
//            var recordingController = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingsController>(User);

//            Assert.IsNotNull(recordingController.SaveRecording(new Models.RecordingSchedule() { EpgEventOid = listing.Oid, PrePadding = 2, PostPadding = 5, Type = Models.RecordingType.Record_Once }));
                
//            // load pending to ensure its due to record
//            var pending = recordingController.Pending();
//            var recording = pending.Where(x => x.EventOid == listing.Oid).FirstOrDefault();
//            Assert.IsNotNull(recording);

//            // cancel the recording
//            Assert.IsTrue(recordingController.Delete(recording.OID));
//        }

//        [TestMethod]
//        public void ScheduleTest_RecordSeasonNewThisChannel()
//        {
//            var listing = GetListingToRecord();
//            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);
//            var recordingController = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingsController>(User);

//            Assert.IsNotNull(recordingController.SaveRecording(new Models.RecordingSchedule() { EpgEventOid = listing.Oid, PrePadding = 2, PostPadding = 5, Type = Models.RecordingType.Record_Season_New_This_Channel }));

//            // load pending to ensure its due to record
//            var pending = recordingController.Pending();
//            var recording = pending.Where(x => x.EventOid == listing.Oid).FirstOrDefault();
//            Assert.IsNotNull(recording);

//            // cancel the recurring recording
//            Assert.IsTrue(recordingController.DeleteRecurring(recording.RecurrenceOid));
//        }

//        [TestMethod]
//        public void ScheduleTest_RecordSeasonAllThisChannel()
//        {
//            var listing = GetListingToRecord();
//            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);
//            var recordingController = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingsController>(User);

//            Assert.IsNotNull(recordingController.SaveRecording(new Models.RecordingSchedule() { EpgEventOid = listing.Oid, PrePadding = 2, PostPadding = 5, Type = Models.RecordingType.Record_Season_All_This_Channel }));

//            // load pending to ensure its due to record
//            var pending = recordingController.Pending();
//            var recording = pending.Where(x => x.EventOid == listing.Oid).FirstOrDefault();
//            Assert.IsNotNull(recording);

//            // cancel the recurring recording
//            Assert.IsTrue(recordingController.DeleteRecurring(recording.RecurrenceOid));
//        }

//        [TestMethod]
//        public void ScheduleTest_RecordSeasonDailyThisTimeslot()
//        {
//            var listing = GetListingToRecord();
//            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);
//            var recordingController = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingsController>(User);

//            Assert.IsNotNull(recordingController.SaveRecording(new Models.RecordingSchedule() { EpgEventOid = listing.Oid, PrePadding = 2, PostPadding = 5, Type = Models.RecordingType.Record_Season_Daily_This_Timeslot }));

//            // load pending to ensure its due to record
//            var pending = recordingController.Pending();
//            var recording = pending.Where(x => x.EventOid == listing.Oid).FirstOrDefault();
//            Assert.IsNotNull(recording);

//            // cancel the recurring recording
//            Assert.IsTrue(recordingController.DeleteRecurring(recording.RecurrenceOid));
//        }

//        [TestMethod]
//        public void ScheduleTest_RecordSeasonWeekdaysThisTimeslot()
//        {
//            var listing = GetListingToRecord(NUtility.DayMask.MONDAY | NUtility.DayMask.TUESDAY | NUtility.DayMask.WEDNESDAY | NUtility.DayMask.THURSDAY | NUtility.DayMask.FRIDAY);
//            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);
//            var recordingController = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingsController>(User);

//            Assert.IsNotNull(recordingController.SaveRecording(new Models.RecordingSchedule() { EpgEventOid = listing.Oid, PrePadding = 2, PostPadding = 5, Type = Models.RecordingType.Record_Season_Weekdays_This_Timeslot }));

//            // load pending to ensure its due to record
//            var pending = recordingController.Pending();
//            var recording = pending.Where(x => x.EventOid == listing.Oid).FirstOrDefault();
//            Assert.IsNotNull(recording);

//            // cancel the recurring recording
//            Assert.IsTrue(recordingController.DeleteRecurring(recording.RecurrenceOid));
//        }

//        [TestMethod]
//        public void ScheduleTest_RecordSeasonWeekendsThisTimeslot()
//        {
//            var listing = GetListingToRecord(NUtility.DayMask.SATURDAY | NUtility.DayMask.SUNDAY);
//            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);
//            var recordingController = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingsController>(User);

//            Assert.IsNotNull(recordingController.SaveRecording(new Models.RecordingSchedule() { EpgEventOid = listing.Oid, PrePadding = 2, PostPadding = 5, Type = Models.RecordingType.Record_Season_Weekends_This_Timeslot }));

//            // load pending to ensure its due to record
//            var pending = recordingController.Pending();
//            var recording = pending.Where(x => x.EventOid == listing.Oid).FirstOrDefault();
//            Assert.IsNotNull(recording);

//            // cancel the recurring recording
//            Assert.IsTrue(recordingController.DeleteRecurring(recording.RecurrenceOid));
//        }

//        [TestMethod]
//        public void ScheduleTest_RecordSeasonWeeklyThisTimeslot()
//        {
//            var listing = GetListingToRecord();
//            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);
//            var recordingController = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingsController>(User);

//            Assert.IsNotNull(recordingController.SaveRecording(new Models.RecordingSchedule() { EpgEventOid = listing.Oid, PrePadding = 2, PostPadding = 5, Type = Models.RecordingType.Record_Season_Weekly_This_Timeslot }));

//            // load pending to ensure its due to record
//            var pending = recordingController.Pending();
//            var recording = pending.Where(x => x.EventOid == listing.Oid).FirstOrDefault();
//            Assert.IsNotNull(recording);

//            // cancel the recurring recording
//            Assert.IsTrue(recordingController.DeleteRecurring(recording.RecurrenceOid));
//        }

//        [TestMethod]
//        public void ScheduleTest_RecordSeasonAllEpisodesAllChannels()
//        {
//            var listing = GetListingToRecord();
//            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);
//            var recordingController = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingsController>(User);

//            Assert.IsNotNull(recordingController.SaveRecording(new Models.RecordingSchedule() { EpgEventOid = listing.Oid, PrePadding = 2, PostPadding = 5, Type = Models.RecordingType.Record_Season_All_Episodes_All_Channels }));

//            // load pending to ensure its due to record
//            var pending = recordingController.Pending();
//            var recording = pending.Where(x => x.EventOid == listing.Oid).FirstOrDefault();
//            Assert.IsNotNull(recording);

//            // cancel the recurring recording
//            Assert.IsTrue(recordingController.DeleteRecurring(recording.RecurrenceOid));
//        }
//    }
//}
