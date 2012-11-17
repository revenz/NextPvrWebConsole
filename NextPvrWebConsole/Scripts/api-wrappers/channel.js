/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="listing.js" />

function Channel(data) {
    var self = this;
	self.oid = ko.observable(data.Oid);
	self.name = ko.observable(data.Name);
	self.number = ko.observable(data.Number);
	self.enabled = ko.observable(data.Enabled);
	self.hasIcon = ko.observable(data.HasIcon);
	self.iconSrc = ko.computed(function () {
	    if (!self.hasIcon())
	        return '';
	    return '/channelicon/' + self.oid();
	});
	self.listings = ko.observableArray([]);
	if (data.Listings) {
	    var mapped = $.map(data.Listings, function (item) { return new Listing(data, item) });
        self.listings(mapped);
	}

	self.toApiObject = function () {
	    data.Oid = self.oid();
	    data.Name = self.name();
	    data.Number = self.number();
	    data.Enabled = self.enabled();
	    data.HasIcon = self.hasIcon();
	    return data;
	};
}