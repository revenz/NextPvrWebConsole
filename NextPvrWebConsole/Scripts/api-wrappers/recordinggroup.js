/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery.dateFormat-1.0.js" />

function RecordingGroup(data, index) {
    var self = this;
    self.name = ko.observable(data.Name);
    self.recordings = ko.observableArray([]);
    self.numberOfRecordings = ko.computed(function () { return data.Recordings.length; });
    self.index = index;
    var mapped = $.map(data.Recordings, function (item) { if (item.Status == 0) { return; } return new Recording(item) });
    self.recordings(mapped);
}