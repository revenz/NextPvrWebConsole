/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../jquery-1.8.2.js" />
/// <reference path="../jquery-ui-1.9.0.js" />
/// <reference path="../knockout-2.2.0.js" />
/// <reference path="../jquery.dateFormat-1.0.js" />
/*
STATUS_PENDING = 0,
STATUS_IN_PROGRESS = 1,
STATUS_COMPLETED = 2,
STATUS_COMPLETED_WITH_ERROR = 3,
STATUS_PLACE_HOLDER = 4,
STATUS_CONFLICT = 5,
STATUS_DELETED = 6,
*/

$(function () {
    function RecordingGroup(data) {
        var self = this;
        self.name = ko.observable(data.Name);
        self.recordings = ko.observableArray([]);
        self.numberOfRecordings = ko.computed(function () { return data.Recordings.length; });
        var mapped = $.map(data.Recordings, function (item) { if (item.Status == 0) { return; } return new Recording(item) });
        self.recordings(mapped);
    }

    function Recording(data) {
        var self = this;
        self.filename = ko.observable(data.Filename);
        self.name = ko.observable(data.Name);
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
            console.log(data);
            if (data.Subtitle && date.Subtitle.length > 0)
                return data.Subtitle;
            return $.format.date(data.StartTime, 'd MMMM h:mm a')
        });
    }

    function RecordingsViewModel() {
        // Data
        var self = this;
        self.recordingGroups = ko.observableArray([]);

        // Operations
        self.deleteRecording = function (recording) { self.recordings.remove(recording) };

        // Load initial state from server, convert it to Task instances, then populate self.tasks
        api.getJSON("recordings", function (allData) {
            var mapped = $.map(allData, function (item) { return new RecordingGroup(item) });
            self.recordingGroups(mapped);
        });
    }

    ko.applyBindings(new RecordingsViewModel());
});