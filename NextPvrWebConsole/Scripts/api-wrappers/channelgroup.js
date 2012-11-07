function ChannelGroup(data) {
    var self = this;
    self.oid = ko.observable(data ? data.Oid : 0);
    self.name = ko.observable(data ? data.Name : '');
    self.orderoid = ko.observable(data ? data.OrderOid: -1);
    self.channels = ko.observable([]);
}