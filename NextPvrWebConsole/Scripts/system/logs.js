/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery.dateFormat-1.0.js" />

$(function () {

    function LogsViewModel() {
        // Data
        var self = this;
        self.logs = ko.observableArray([]);

        self.open = function (item) {
            window.open('/api/logs/getcontent?filename=' + encodeURIComponent(item.fullName()));
        };

        // Load initial state from server
        api.getJSON("logs", null, function (allData) {
            var mapped = $.map(allData, function (item) { return new Log(item) });
            self.logs(mapped);
        });
    }
    var viewModel = new LogsViewModel();
    ko.applyBindings(viewModel, $('#system-tab-logs').get(0));

    $('#system-tab-logs > table').removeAttr('style');

});