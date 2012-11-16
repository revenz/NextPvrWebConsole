/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery.dateFormat-1.0.js" />

$(function () {

    function PendingRecordingsViewModel() {
        // Data
        var self = this;
        self.recordings = ko.observableArray([]);

        self.remove = function (item) {
            gui.confirmMessage({
                message: $.i18n._("Are you sure you want to delete the pending recording '%s'?", [item.name()]),
                yes: function () {
                    api.deleteJSON('recordings/' + item.oid(), null, function (result) {
                        self.recordings.remove(item);
                    });
                }
            });
        };

        // Load initial state from server, convert it to Task instances, then populate self.tasks
        api.getJSON("recordings/pending", null, function (allData) {
            var index = 0;
            var mapped = $.map(allData, function (item) { return new Recording(item) });
            self.recordings(mapped);
        });
    }
    var viewModel = new PendingRecordingsViewModel();
    ko.applyBindings(viewModel, $('#recordings-tab-pending').get(0));

    $('#recordings-tab-pending > div').removeAttr('style');

});