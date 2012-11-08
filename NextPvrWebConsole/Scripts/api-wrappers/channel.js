/// <reference path="../knockout-2.2.0.js" />
/// <reference path="../jquery-1.8.2.js" />
/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />

function Channel(data) {
    var self = this;
    console.log(data);
	self.oid = ko.observable(data.Oid);
	self.name = ko.observable(data.Name);
	self.number = ko.observable(data.Number);
	self.enabled = ko.observable(data.Enabled);

	self.toApiObject = function () {
	    data.Oid = self.oid();
	    data.Name = self.name();
	    data.Number = self.number();
	    data.Enabled = self.enabled();
	    return data;
	};
}