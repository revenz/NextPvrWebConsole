/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery.i18n.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../functions.js" />
/// <reference path="../core/jquery.i18n.js" />


function Listing(channel, epgListing) {
    var self = this;
    self.channel = channel;
    self.epgListing = epgListing;
    self.oid = ko.observable(epgListing.Oid);
    self.title = ko.observable(epgListing.Title);
    self.subtitle = ko.observable(epgListing.Subtitle);
    self.description = ko.observable(epgListing.Description);
    self.channelName = ko.observable(channel.Name);
    self.channelNumber = ko.observable(channel.Number);
    self.startTime = ko.observable(epgListing.StartTime);
    self.endTime = ko.observable(epgListing.EndTime);
    self.startDateTimeShort = ko.computed(function () { return gui.formatDateTimeShort(Date.parse(epgListing.StartTime)); });
    self.startTimeShort = ko.computed(function () { return gui.formatTime(Date.parse(epgListing.StartTime)); });
    self.startTimeLong = ko.computed(function () { return gui.formatDateLong(Date.parse(epgListing.StartTime)); });
    self.endTimeShort = ko.computed(function () { return gui.formatTime(Date.parse(epgListing.EndTime)); });
    self.duration = ko.computed(function () { return Math.floor((Math.abs(Date.parse(epgListing.EndTime) - Date.parse(epgListing.StartTime)) / 1000) / 60) + ' ' + $.i18n._('Minutes') });
    self.genresString = ko.computed(function () {
        if (epgListing.Genres)
            return $.Enumerable.From(epgListing.Genres).Select(function (x) {
                x = x.replace('/', ', ');
                x = x.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1); });
                return x.replace(' , ', ', ');
            }).ToString(', ');
        return '';
    });
    self.channelLogoVisible = ko.computed(function () { return channel.Icon && channel.Icon.length > 0; });
    self.channelLogoData = ko.computed(function () {
        if (channel.Icon && channel.Icon.length > 0)
            return 'data:image/png;base64,' + channel.Icon;
        return '';
    });

    self.isRecording = ko.observable(epgListing.IsRecording);
    self.isRecurring = ko.observable(epgListing.IsRecurring);
    self.prePadding = ko.observable(epgListing.PrePadding);
    self.postPadding = ko.observable(epgListing.PostPadding);
    self.keep = ko.observable(epgListing.Keep);
    self.recordingType = ko.observable(epgListing.RecordingType);
    self.recordingDirectoryId = ko.observable(epgListing.RecordingDirectoryId);
    self.recordingOid = ko.observable(epgListing.RecordingOid);
}
