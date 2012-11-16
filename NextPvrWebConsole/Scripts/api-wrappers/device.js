/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="listing.js" />

function Device(data) {
    var self = this;
    self.oid = ko.observable(data.Oid);
    self.name = ko.observable(data.Name);
    self.numberOfChannels = ko.observable(data.NumberOfChannels);
    self.enabled = ko.observable(data.Enabled);
    self.present = ko.observable(data.Present);
    self.priority = ko.observable(data.Priority);
    self.sourceType = ko.observable(data.SourceType);
    self.presentString = ko.computed(function () {
        return self.present() ? $.i18n._('Yes') : $.i18n._('No');
    });

    self.toApiObject = function () {
        data.Oid = self.oid();
        data.Name = self.name();
        data.Present = self.present();
        data.Enabled = self.enabled();
        data.NumberOfChannels = self.numberOfChannels();
        data.Priority = self.priority();
        data.SourceType = self.sourceType();
        return data;
    };
}