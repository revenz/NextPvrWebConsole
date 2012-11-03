/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../jquery-1.8.2.js" />
/// <reference path="../jquery-ui-1.9.0.js" />
/// <reference path="../knockout-2.2.0.js" />

$(function () {
    function DashboardViewModel() {
        // Data
        var self = this;

        self.devices = ko.observableArray([]);
        // Operations
        //self.deleteRecording = function (recording) { self.recordings.remove(recording) };

        // Load initial state from server, convert it to Task instances, then populate self.tasks
        

        var refreshDevices = function () {
            api.getJSON("devices", function (allData) {
                var mapped = $.map(allData, function (item) { return new Device(item) });
                self.devices(mapped);
            });
        };
        refreshDevices();
        setInterval(refreshDevices, 30 * 1000); // maybe change this to a signalr update?
    }

    ko.applyBindings(new DashboardViewModel());
});

function Device(data) {
    var self = this;
    self.oid = ko.observable(data.Oid);
    self.identifier = ko.observable(data.Identifier);
    self.streams = ko.observableArray([]);
    self.streams($.map(data.Streams, function (item) { return new Stream(self, item); }));
}

function Stream(owner, data) {
    var self = this;
    self.type = ko.observable(data.Type);
    self.handle = ko.observable(data.Handle);
    self.filename = ko.observable(data.Filename);
    self.stop = function () {
        api.deleteJSON('devices/deleteStream?handle=' + data.Handle, null, function (result) {
            console.log(result);
            if(result)
                owner.streams.remove(this);
        });
    };
}