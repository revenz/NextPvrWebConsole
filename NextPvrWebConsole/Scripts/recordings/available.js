/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery.dateFormat-1.0.js" />

$(function () {

    function AvailableRecordingsViewModel() {
        // Data
        var self = this;
        self.recordingGroups = ko.observableArray([]);
        self.selectedRecordingGroupName = ko.observable();
        self.recordingDirectories = ko.observableArray([]);
        self.selectedRecordingDirectory = ko.observable();

        self.selectedRecordingGroup = ko.computed(function () {
            for (var i = 0; i < self.recordingGroups().length; i++) {
                var data = self.recordingGroups()[i];
                if (data.name() === self.selectedRecordingGroupName()) {
                    return data;
                }
            }
            return null;
        });

        self.move = function () {
            var groupname = self.selectedRecordingGroupName();
            if (!groupname)
                return;
            var dialog = $('#recording-group-mover');
            var dialog_buttons = {};
            dialog_buttons[$.i18n._("Move")] = function () {
                var destination = self.selectedRecordingDirectory();
                api.getJSON('recordings/moverecordings', { groupname: groupname, DestinationRecordingDirectoryId: destination }, function () {
                    gui.showInfo($.i18n._("Starting moving recordings, this may take a while"));
                    dialog.dialog('close');
                });
            };
            dialog_buttons[$.i18n._("Cancel")] = function () { dialog.dialog('close'); };

            dialog.dialog({
                title: $.i18n._('Move Channel Group'),
                buttons: dialog_buttons,
                width: 500,
                height: 270,
                resizable: false
            });
        };

        self.remove = function (item) {
            gui.confirmMessage({
                message: $.i18n._("Are you sure you want to delete the recording '%s'?", [item.displayName()]),
                yes: function () {
                    api.deleteJSON('recordings/' + item.oid(), null, function (result) {
                        var group = self.selectedRecordingGroup();
                        group.recordings.remove(item);
                        if (group.recordings().length == 0) {
                            self.recordingGroups.remove(group);
                        }
                    });
                }
            });
        };

        // Load initial state from server, convert it to Task instances, then populate self.tasks
        api.getJSON("recordings/available", null, function (allData) {
            var index = 0;
            var mapped = $.map(allData, function (item) { return new RecordingGroup(item, ++index) });
            self.recordingGroups(mapped);
        });
        api.getJSON("recordingdirectories?includeshared=true", null, function (allData) {
            console.log(allData);
            var mapped = $.map(allData, function (item) { return new RecordingDirectory(item) });
            console.log(mapped);
            self.recordingDirectories(mapped);
        });
    }
    var viewModel = new AvailableRecordingsViewModel();
    ko.applyBindings(viewModel, $('#recordings-tab-available').get(0));

    $('#recordings-tab-available > div').removeAttr('style');

});