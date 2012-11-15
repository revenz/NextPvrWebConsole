/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery.dateFormat-1.0.js" />

$(function () {

    function RecordingsViewModel() {
        // Data
        var self = this;
        self.recordingGroups = ko.observableArray([]);
        self.selectedRecordingGroup = ko.observable();

        // Operations
        self.deleteRecording = function (recording) { self.recordings.remove(recording) };

        // Load initial state from server, convert it to Task instances, then populate self.tasks
        api.getJSON("recordings", null, function (allData) {
            var index = 0;
            var mapped = $.map(allData, function (item) { return new RecordingGroup(item, ++index) });
            self.recordingGroups(mapped);
        });
    }
    var viewModel = new RecordingsViewModel();
    ko.applyBindings(viewModel);

    $('.recording-groups, .recordings-list').removeAttr('style');

    function RecordingGroup(data, index) {
        var self = this;
        self.name = ko.observable(data.Name);
        self.recordings = ko.observableArray([]);
        self.numberOfRecordings = ko.computed(function () { return data.Recordings.length; });
        self.index = index;
        var mapped = $.map(data.Recordings, function (item) { if (item.Status == 0) { return; } return new Recording(self, item) });
        self.recordings(mapped);
        self.select = function (group) {
            $('.recordings-groups-container .selected').removeClass('selected'); // clear last selection
            $('#rg-' + index).addClass('selected');
            viewModel.selectedRecordingGroup(group);
        };
    }

    function Recording(group, data) {
        var self = this;
        self.filename = ko.observable(data.Filename);
        self.name = ko.observable(data.Name);
        self.startTime = ko.observable(data.StartTime);
        self.endTime = ko.observable(data.EndTime);
        self.startTimeStr = ko.computed(function () { return gui.formatDateLong(data.StartTime); });
        self.endTimeStr = ko.computed(function () { return gui.formatDateLong(data.EndTime); });
        self.channelName = ko.observable(data.ChannelName);
        self.channelOid = ko.observable(data.ChannelOID);
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
        self.remove = function () {
            gui.confirmMessage({
                message: $.i18n._("Are you sure you want to delete the recording '%s'?", [self.displayName()]),
                yes: function () {
                    api.deleteJSON('recordings/' + data.OID, null, function (result) {
                        group.recordings.remove(self);
                        if (group.recordings().length == 0) {
                            viewModel.recordingGroups.remove(group);
                            viewModel.selectedRecordingGroup(null);
                        }
                    });
                }
            });
        };
    }
});