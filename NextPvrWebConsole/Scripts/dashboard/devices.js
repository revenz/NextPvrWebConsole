/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />

$(function () {
    function DevicesViewModel() {
        // Data
        var self = this;

        self.devices = ko.observableArray([]);

        var refreshDevices = function () {
            api.getJSON("devices", null, function (allData) {
                var mapped = $.map(allData, function (item) { return new Device(item) });
                self.devices(mapped);
            });
        };
        refreshDevices();

        npvrevent.deviceStatusUpdated = function (events) {
            $.each(events, function (i, ele) {
                gui.showInfo(ele.Message, ele.CodeString);
            });
            refreshDevices();
        };
    }

    ko.applyBindings(new DevicesViewModel(), $('.tuners').get(0));
});

function Device(data) {
    var self = this;
    self.oid = ko.observable(data.Oid);
    self.identifier = ko.observable(data.Name);
    self.streams = ko.observableArray([]);
    if(data.Streams)
        self.streams($.map(data.Streams, function (item) { return new Stream(self, item); }));
}

function Stream(owner, data) {
    var self = this;
    self.type = ko.observable(data.Type);
    self.typeName = ko.computed(function () {
        if (data.Type == 1)
            return 'LiveTV';
        else if (data.Type == 2)
            return 'Recording';
        return 'Unknown';
    });
    self.handle = ko.observable(data.Handle);
    self.filename = ko.observable(data.Filename);
    self.channelName = ko.observable(data.ChannelName);
    self.channelNumber = ko.observable(data.ChannelNumber);
    self.title = ko.observable(data.Title);
    self.subtitle = ko.observable(data.Subtitle);
    self.description = ko.observable(data.Description);
    self.channelHasIcon = ko.observable(data.ChannelHasIcon);
    self.channelIconUrl = ko.computed(function () {
        if (!self.channelHasIcon())
            return '';
        return '/channelicon/' + self.oid();
    });
    self.channelOid = ko.observable(data.ChannelOid);
    self.startTimeString = ko.computed(function () { return gui.formatTime(data.StartTime); });
    self.endTimeString = ko.computed(function () { return gui.formatTime(data.EndTime); });
    self.stop = function () {
        api.deleteJSON('devices/deleteStream?handle=' + data.Handle, null, function (result) {
            if (result == true)
                owner.streams.remove(this);
            else
                gui.showError("Failed to stop stream", self.title());
        });
    };
}