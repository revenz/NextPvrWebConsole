﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NextPvrWebConsole.Tests.Controllers
{
    [TestClass]
    public class ScheduleTest : NextPvrWebConsoleTest
    {
        public ScheduleTest()
        {
        }

        private Models.EpgListing GetListingToRecord(NUtility.DayMask DayMask = NUtility.DayMask.ANY)
        {
            var controller = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);

            DateTime date = DateTime.Now.AddDays(1);
            switch (DayMask)
            {
                case NUtility.DayMask.MONDAY: date = date.Next(DayOfWeek.Monday); break;
                case NUtility.DayMask.TUESDAY: date = date.Next(DayOfWeek.Tuesday); break;
                case NUtility.DayMask.WEDNESDAY: date = date.Next(DayOfWeek.Wednesday); break;
                case NUtility.DayMask.THURSDAY: date = date.Next(DayOfWeek.Thursday); break;
                case NUtility.DayMask.FRIDAY: date = date.Next(DayOfWeek.Friday); break;
                case NUtility.DayMask.SATURDAY: date = date.Next(DayOfWeek.Saturday); break;
                case NUtility.DayMask.SUNDAY: date = date.Next(DayOfWeek.Sunday); break;
                case NUtility.DayMask.SATURDAY | NUtility.DayMask.SUNDAY: if (date.DayOfWeek != DayOfWeek.Sunday) { date = date.Next(DayOfWeek.Saturday); } break;
                case NUtility.DayMask.MONDAY | NUtility.DayMask.TUESDAY | NUtility.DayMask.WEDNESDAY | NUtility.DayMask.THURSDAY | NUtility.DayMask.FRIDAY: if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) { date = date.Next(DayOfWeek.Monday); } break;
            }

            var listings = Models.ChannelGroup.LoadAll(User.Oid, false)
                                              .Take(1)
                                              .SelectMany(x => controller.Get(date, x.Name))
                                              .SelectMany(x => x.Listings)
                                              .OrderByDescending(x => x.StartTime)
                                              .ToList();

            var listing = listings.Where(x => x.StartTime > DateTime.UtcNow.AddMinutes(15))
                                  .OrderBy(x => Guid.NewGuid()) // random order
                                  .FirstOrDefault();
            return listing;
        }

        [TestMethod]
        public void ScheduleTest_QuickRecord()
        {
            var listing = GetListingToRecord();
            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);

            Assert.IsNotNull(guide.QuickRecord(listing.Oid));
            System.Threading.Thread.Sleep(5000);

            // load pending to ensure its due to record
            var recordingController = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingsController>(User);
            var pending = recordingController.Pending();
            var recording = pending.Where(x => x.Title == listing.Title || x.Name == listing.Title).FirstOrDefault();
            Assert.IsNotNull(recording);

            // cancel the recording
            Assert.IsTrue(recordingController.Delete(recording.OID));
        }

        [TestMethod]
        public void ScheduleTest_RecordOnce()
        {
            TestRecordingSchedule(Models.RecordingType.Record_Once);
        }

        [TestMethod]
        public void ScheduleTest_RecordSeasonNewThisChannel()
        {
            TestRecordingSchedule(Models.RecordingType.Record_Season_New_This_Channel);
        }

        [TestMethod]
        public void ScheduleTest_RecordSeasonAllThisChannel()
        {
            TestRecordingSchedule(Models.RecordingType.Record_Season_All_This_Channel);
        }

        [TestMethod]
        public void ScheduleTest_RecordSeasonDailyThisTimeslot()
        {
            TestRecordingSchedule(Models.RecordingType.Record_Season_Daily_This_Timeslot);
        }

        [TestMethod]
        public void ScheduleTest_RecordSeasonWeekdaysThisTimeslot()
        {
            TestRecordingSchedule(Models.RecordingType.Record_Season_Weekdays_This_Timeslot);
        }

        [TestMethod]
        public void ScheduleTest_RecordSeasonWeekendsThisTimeslot()
        {
            TestRecordingSchedule(Models.RecordingType.Record_Season_Weekends_This_Timeslot); 
        }

        [TestMethod]
        public void ScheduleTest_RecordSeasonWeeklyThisTimeslot()
        {
            TestRecordingSchedule(Models.RecordingType.Record_Season_Weekly_This_Timeslot); 
        }

        [TestMethod]
        public void ScheduleTest_RecordSeasonAllEpisodesAllChannels()
        {
            TestRecordingSchedule(Models.RecordingType.Record_Season_All_Episodes_All_Channels);
        }

        private void TestRecordingSchedule( Models.RecordingType Type)
        {
            Models.EpgListing listing = null;
            if(Type == Models.RecordingType.Record_Season_Weekends_This_Timeslot)
                listing = GetListingToRecord(NUtility.DayMask.SATURDAY | NUtility.DayMask.SUNDAY);
            else if(Type == Models.RecordingType.Record_Season_Weekdays_This_Timeslot)
                listing = GetListingToRecord(NUtility.DayMask.MONDAY | NUtility.DayMask.TUESDAY | NUtility.DayMask.WEDNESDAY | NUtility.DayMask.THURSDAY | NUtility.DayMask.FRIDAY);
            else
                listing = GetListingToRecord();
            var guide = base.LoadController<NextPvrWebConsole.Controllers.Api.GuideController>(User);
            var recordingController = base.LoadController<NextPvrWebConsole.Controllers.Api.RecordingsController>(User);

            Assert.IsNotNull(recordingController.SaveRecording(new Models.RecordingSchedule()
                                                                        { EpgEventOid = listing.Oid, 
                                                                          PrePadding = 2, 
                                                                          PostPadding = 5, 
                                                                          Type = Type
                                                                        }));

            Models.Recording recording = null;
            int count = 0;
            do
            {
                System.Threading.Thread.Sleep(1000);
                // load pending to ensure its due to record
                var pending = recordingController.Pending().ToArray();
                recording = pending.Where(x => x.Title == listing.Title || x.Name == listing.Title).FirstOrDefault();
            } while (recording == null && count++ < 5);
            Assert.IsNotNull(recording);

            if (Type == Models.RecordingType.Record_Once)
            {
                // cancel the recording
                Assert.IsTrue(recordingController.Delete(recording.OID));
            }
            else
            {
                // cancel the recurring recording
                Assert.IsTrue(recordingController.DeleteRecurring(recording.RecurrenceOid));
            }
        }
    }
}
