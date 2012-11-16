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

        self.selectedRecordingGroup = ko.computed(function () {
            for (var i = 0; i < self.recordingGroups().length; i ++) {
                var data = self.recordingGroups()[i];
                if (data.name() === self.selectedRecordingGroupName()) {
                    return data;
                }
            }
            return null;
        });


        self.remove = function (item) {
            gui.confirmMessage({
                message: $.i18n._("Are you sure you want to delete the recording '%s'?", [item.displayName()]),
                yes: function () {
                    //api.deleteJSON('recordings/' + data.OID, null, function (result) {
                        var group = self.selectedRecordingGroup();
                        group.recordings.remove(item);
                        if (group.recordings().length == 0) {
                            self.recordingGroups.remove(group);
                        }
                    //});
                }
            });
        };

        // Load initial state from server, convert it to Task instances, then populate self.tasks
        api.getJSON("recordings/available", null, function (allData) {
            var index = 0;
            var mapped = $.map(allData, function (item) { return new RecordingGroup(item, ++index) });
            self.recordingGroups(mapped);
        });
    }
    var viewModel = new AvailableRecordingsViewModel();
    ko.applyBindings(viewModel, $('#recordings-tab-available').get(0));

    $('#recordings-tab-available > div').removeAttr('style');

});