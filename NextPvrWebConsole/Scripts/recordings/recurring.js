/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery.dateFormat-1.0.js" />

$(function () {

    function RecurringRecordingsViewModel() {
        // Data
        var self = this;
        self.recordings = ko.observableArray([]);
        self.selected = ko.observable();

        self.remove = function (item) {
            gui.confirmMessage({
                message: $.i18n._("Are you sure you want to delete the recurring recording '%s'?", [item.name()]),
                yes: function () {
                    //api.deleteJSON('recordings/' + data.OID, null, function (result) {
                    self.recordings.remove(item);
                    //});
                }
            });
        };

        self.edit = function (item) {
            self.selected(item);
            var dialog = $('#RecurringRecording-ScheduleEditor');
            var dialog_buttons = {};
            dialog_buttons[$.i18n._("Save")] = function () { dialog.dialog('close'); };
            dialog_buttons[$.i18n._("Cancel")] = function () { dialog.dialog('close'); };
            dialog.dialog({
                modal: true,
                title: item.name(),
                minWidth: 550,
                minHeight:300,
                buttons: dialog_buttons
            });
        };

        // Load initial state from server, convert it to Task instances, then populate self.tasks
        api.getJSON("recordings/recurring", null, function (allData) {
            var index = 0;
            var mapped = $.map(allData, function (item) { return new RecurringRecording(item) });
            self.recordings(mapped);
        });
    }
    var viewModel = new RecurringRecordingsViewModel();
    ko.applyBindings(viewModel, $('#recordings-tab-recurring').get(0));

    $('#recordings-tab-recurring > div').removeAttr('style');

});