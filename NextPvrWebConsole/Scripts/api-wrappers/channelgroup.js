function ChannelGroup(data) {
    var self = this;
    self.oid = ko.observable(data ? data.Oid : 0);
    self.name = ko.observable(data ? data.Name : '');
    self.orderoid = ko.observable(data ? data.OrderOid : -1);

    self.channelOids = ko.observable(data.ChannelOids ? data.ChannelOids : []); // used by configuration "channel groups"
    self.channels = ko.observable([]); // what uses this??

    self.numberOfChannels = ko.computed(function () {
        return self.channelOids().length;
    });

    self.toApiObject = function () {
        data.Oid = self.oid();
        data.Name = self.name();
        data.OrderOid = self.orderoid();
        data.ChannelOids = self.channelOids();
        return data;
    };
}

function ChannelGroupChannel(data) {
    var self = this;
    self.name = ko.observable(data.Name);
    self.enabled = ko.observable(data.Enabled);
    self.oid = ko.observable(data.Oid);

    self.toApiObject = function () {
        data.Oid = self.oid();
        data.Name = self.name();
        data.Enabled = self.enabled();
        return data;
    };
}
