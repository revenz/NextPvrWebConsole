/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="listing.js" />

function RecurringRecording(data) {
    var self = this;
    self.oid = ko.observable(data.OID);
    self.name = ko.observable(data.Name);
    self.startTime = ko.observable(data.StartTime);
    self.endTime = ko.observable(data.EndTime);
    self.endDate = ko.observable(data.EndDate);
    self.timeslot = ko.observable(data.Timeslot);
    self.daymask = ko.observable(data.DayMask);
    self.epgtitle = ko.observable(data.EpgTitle);
    self.isManualRecording = ko.observable(data.IsManualRecording);
    self.keep = ko.observable(data.Keep);
    self.onlyNewEpisodes = ko.observable(data.onlyNewEpisodes);
    self.postPadding = ko.observable(data.PostPadding);
    self.prePadding = ko.observable(data.PrePadding);
    self.recordingDirectoryId = ko.observable(data.RecordingDirectoryId);
    self.channelOid = ko.observable(data.ChannelOid);
    self.channelName = ko.observable(data.ChannelName);
    self.channelHasIcon = ko.observable(data.ChannelHasIcon);
    self.channelIconSrc = ko.computed(function () {
        if (!self.channelHasIcon())
            return '';
        return '/channelicon/' + self.channelOid();
    });
    self.days = ko.computed(function () {
        var mask = self.daymask();
        switch (mask) {
            case 255: return $.i18n._('Daily');
            case 65: return $.i18n._('Weekends');
            case 62: return $.i18n._('Weekdays');
            case 1: return $.i18n._('Sundays');
            case 2: return $.i18n._('Mondays');
            case 4: return $.i18n._('Tuesdays');
            case 8: return $.i18n._('Wednesdays');
            case 16: return $.i18n._('Thursdays');
            case 32: return $.i18n._('Fridays');
            case 65: return $.i18n._('Saturdays');
            default:
                {
                    var t = [];
                    if ((self.mask & 1) == 1) t.push('Sun');
                    if ((self.mask & 2) == 2) t.push('Mon');
                    if ((self.mask & 4) == 4) t.push('Tue');
                    if ((self.mask & 8) == 8) t.push('Wed');
                    if ((self.mask & 16) == 16) t.push('Thu');
                    if ((self.mask & 32) == 32) t.push('Fri');
                    if ((self.mask & 64) == 64) t.push('Sat');
                    return t.join();
                }
        }
    });

    self.startTimeTimeString = ko.computed(function () { return gui.formatTime(data.StartTime); });
    self.endTimeTimeString = ko.computed(function () { return gui.formatTime(data.EndTime); });
    self.endDateTimeString = ko.computed(function () { return gui.formatTime(data.EndDate); });

    self.timeStr = ko.computed(function () {
        return $.i18n._('Time-Range', [gui.formatDateLong(data.StartTime), gui.formatTime(data.EndTime)]);
    });

    self.type = ko.computed({
        read: function () {
            var daymask = self.daymask();
            if (self.onlyNewEpisodes() && daymask == 255) return 1; // all new episodes on this channel
            else if (daymask == 255) {
                // todo determine if all episodes this channel 
                // 2 == all episodes on this channel
                // or if its daily this timeslot.
                return 3; // daily, this timeslot
            }
            else if (daymask == 1 || daymask == 2 || daymask == 4 || daymask == 8 || daymask == 16 || daymask == 32 || daymask == 64)
                return 4; // weekly, this timeslot
            else if (daymask == 62)
                return 5; // weekdays, mon to friday
            else if (daymask == 65)
                return 6; // weekends, sat/sun
            else if (daymask == 255 && self.channelOid() == 0)
                return 7; // all episodes all channels
            return 0;
        },
        write: function (value) {
            switch (value) {
                case 0:  // record once
                    {
                    }
                    break;
                case 1:  // season, all new
                    {
                        self.daymask(255);
                        self.onlyNewEpisodes(true);
                    }
                    break;
                case 2:  // season, all
                    {
                        self.daymask(255);
                        self.onlyNewEpisodes(false);
                    }
                    break;
                case 3:  // season, daily
                    {
                        self.daymask(255);
                        self.onlyNewEpisodes(false);
                    }
                    break;
                case 4:  // season, weekly
                    {
                        // need to know day of week.....
                        alert('todo');
                        self.onlyNewEpisodes(false);
                    }
                    break;
                case 5:  // season, weekdays
                    {
                        self.daymask(62);
                        self.onlyNewEpisodes(false);
                    }
                    break;
                case 6:  // season, weekends
                    {
                        self.daymask(65);
                        self.onlyNewEpisodes(false);
                    }
                    break;
                case 7:  // all episodes all channels
                    {
                        self.channelOid(0);
                        self.daymask(255);
                        self.channelName = 'All Channels';
                        self.onlyNewEpisodes(false);
                    }
                    break;
            }
        }
    });


    self.toApiObject = function () {
        data.Oid = self.oid();
        data.Name = self.name();
        data.StartTime = self.startTime()
        data.EndTime = self.endTime();
        data.EndDate = self.endDate
        data.Timeslot = self.timeslot;
        data.DayMask = self.daymask;
        data.EpgTitle = self.epgtitle;
        data.ChannelOid = self.channelOid();
        data.ChannelName = self.channelName();
        data.IsManualRecording = self.isManualRecording;
        data.Keep = self.keep;
        data.onlyNewEpisodes = self.onlyNewEpisodes;
        data.PostPadding = self.postPadding;
        data.PrePadding = self.prePadding;
        data.RecordingDirectoryId = self.recordingDirectoryId;
        return data;
    };
}
