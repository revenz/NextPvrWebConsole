function ChannelGroup(data) {
    var self = this;
    self.oid = ko.observable(data.Oid);
    self.name = ko.observable(data.Name);
    self.orderoid = ko.observable(data.OrderOid);
    self.channels = ko.observable([]);
}