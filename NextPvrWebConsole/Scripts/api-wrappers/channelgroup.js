function ChannelGroup(data) {
    if (data == null)
        data = { Oid: 0, Name: '', OrderOid: -1, ParentOid: 0, IsShared: false, Enabled: true, ChannelOids: [] };
    if (!data.ChannelOids)
        data.ChannelOids = [];
    var self = this;
    self.oid = ko.observable(data.Oid);
    self.name = ko.observable(data.Name);
    self.orderoid = ko.observable(data.OrderOid);
    self.parentOid = ko.observable(data.ParentOid);
    self.shared = ko.observable(data.IsShared);
    self.enabled = ko.observable(data.Enabled);
    self.personal = ko.computed(function () { return !self.shared(); });

    self.channelOids = ko.observable(data.ChannelOids); 

    self.numberOfChannels = ko.computed(function () {
        return self.channelOids().length;
    });

    self.toApiObject = function () {
        data.Oid = self.oid();
        data.Name = self.name();
        data.OrderOid = self.orderoid();
        data.ChannelOids = self.channelOids();
        data.Enabled = self.enabled();
        data.IsShared = self.shared();
        data.ParentOid = self.parentOid();
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
