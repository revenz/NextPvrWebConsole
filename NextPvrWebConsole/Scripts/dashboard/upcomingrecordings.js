/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../jquery-1.8.2.js" />
/// <reference path="../jquery-ui-1.9.0.js" />
/// <reference path="../knockout-2.2.0.js" />

$(function () {
    function UpcomingRecordingsViewModel() {
        // Data
        var self = this;

        self.upcomingRecordings = ko.observableArray([]);

        var refreshUpcomingRecordings = function () {
            api.getJSON("recordings/getupcoming", function (allData) {
                var mapped = $.map(allData, function (item) { return new UpcomingRecording(item) });
                self.upcomingRecordings(mapped);
            });
        };
        refreshUpcomingRecordings();
    }

    ko.applyBindings(new UpcomingRecordingsViewModel(), $('.upcoming-recordings').get(0));
});

function UpcomingRecording(data) {
    var self = this;
    self.oid = ko.observable(data.OID);
    self.startTime = ko.observable(data.StartTime);
    self.endTime = ko.observable(data.EndTime);
    self.postPadding = ko.observable(data.PostPadding);
    self.prePadding = ko.observable(data.PrePadding);
    self.subtitle = ko.observable(data.Subtitle);
    self.title = ko.observable(data.Title);
    self.name = ko.observable(data.Name);
    self.displayName = ko.computed(function () {
        if (data.Title)
            return data.Title;
        return data.Name;
    });
}
