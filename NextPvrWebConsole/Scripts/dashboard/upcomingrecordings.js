/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../jquery-1.8.2.js" />
/// <reference path="../jquery-ui-1.9.0.js" />
/// <reference path="../knockout-2.2.0.js" />
/// <reference path="../api-wrappers/recordings.js" />

$(function () {
    function UpcomingRecordingsViewModel() {
        // Data
        var self = this;

        self.upcomingRecordings = ko.observableArray([]);

        var refreshUpcomingRecordings = function () {
            api.getJSON("recordings/getupcoming", null, function (allData) {
                var mapped = $.map(allData, function (item) { return new recording(item) });
                self.upcomingRecordings(mapped);
            });
        };
        refreshUpcomingRecordings();
    }

    ko.applyBindings(new UpcomingRecordingsViewModel(), $('.upcoming-recordings').get(0));
});
