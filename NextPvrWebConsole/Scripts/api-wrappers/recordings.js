/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="listing.js" />

function Recording(data) {
    var self = this;
    if(data.OID)
        self.oid = ko.observable(data.OID);
    else
        self.oid = ko.observable(data.Oid);
    self.filename = ko.observable(data.Filename);
    self.name = ko.observable(data.Name);
    self.startTime = ko.observable(data.StartTime);
    self.endTime = ko.observable(data.EndTime);
    self.startTimeStr = ko.computed(function () { return gui.formatDateLong(data.StartTime); });
    self.endTimeStr = ko.computed(function () { return gui.formatDateLong(data.EndTime); });
    self.timeStr = ko.computed(function () {
        return $.i18n._('Time-Range', [gui.formatDateLong(data.StartTime), gui.formatTime(data.EndTime)]);
    });
    self.channelName = ko.observable(data.ChannelName);
    self.channelOid = ko.observable(data.ChannelOID);
    self.channelHasIcon = ko.observable(data.ChannelHasIcon);
    self.channelIconSrc = ko.computed(function () {
        if (!self.channelHasIcon())
            return '';
        return '/channelicon/' + self.channelOid();
    });
    self.recordingDirectoryId = ko.observable(data.RecordingDirectoryId);
    self.status = ko.observable(data.Status);
    self.status_pending = ko.computed(function () { return data.Status == 0 });
    self.status_inProgress = ko.computed(function () { return data.Status == 1 });
    self.status_completed = ko.computed(function () { return data.Status == 2 });
    self.status_completedWithError = ko.computed(function () { return data.Status == 3 });
    self.status_placeHolder = ko.computed(function () { return data.Status == 4 });
    self.status_conflict = ko.computed(function () { return data.Status == 5 });
    self.status_deleted = ko.computed(function () { return data.Status == 6 });
    self.cssClass = ko.computed(function () {
        var _class = '';
        if (_class) _class += ' ';
        else _class = '';
        if (self.status_pending()) _class += 'status-pending ';
        else if (self.status_inProgress()) _class += 'status-inprogress ';
        else if (self.status_completed()) _class += 'status-completed ';
        else if (self.status_completedWithError()) _class += 'status-completed-with-error';
        else if (self.status_placeHolder()) _class += 'status-placeholder ';
        else if (self.status_conflict()) _class += 'status-conflict ';
        else if (self.status_deleted()) _class += 'status-deleted ';
        return _class;
    });
    self.displayName = ko.computed(function () {
        if (data.Subtitle && date.Subtitle.length > 0)
            return data.Subtitle;
        return $.format.date(data.StartTime, 'd MMMM h:mm a')
    });
}
