function Channel(data) {
	var self = this;
	self.oid = ko.observable(data.Oid);
	self.name = ko.observable(data.Name);
	self.number = ko.observable(data.Number);
}