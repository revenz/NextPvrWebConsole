/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery.i18n.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../functions.js" />
/// <reference path="../core/jquery.i18n.js" />


function Listing(channel, epgevent) {
    var self = this;
    self.channel = channel;
    self.epgevent = epgevent;
    self.oid = ko.observable(epgevent.oid);
    self.title = ko.observable(epgevent.title);
    self.subtitle = ko.observable(epgevent.subtitle);
    self.description = ko.observable(epgevent.description);
    self.channelName = ko.observable(channel.Name);
    self.channelNumber = ko.observable(channel.Number);
    self.startTime = ko.observable(epgevent.startTime);
    self.endTime = ko.observable(epgevent.endTime);
    self.startTimeLong = ko.computed(function () { return gui.formatDateLong(Date.parse(epgevent.startTime)); });
    self.endTimeShort = ko.computed(function () { return gui.formatTime(Date.parse(epgevent.endTime)); });
    self.duration = ko.computed(function () { return Math.floor((Math.abs(Date.parse(epgevent.endTime) - Date.parse(epgevent.startTime)) / 1000) / 60) + ' ' + $.i18n._('Minutes') });
    self.genresString = ko.computed(function () {
        if (epgevent.genres)
            return $.Enumerable.From(epgevent.genres).Select(function (x) {
                x = x.replace('/', ', ');
                x = x.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1); });
                return x.replace(' , ', ', ');
            }).ToString(', ');
        return '';
    });
    self.channelLogoVisible = ko.computed(function () { return channel.Icon && channel.Icon.length > 0; });
    self.channelLogoData = ko.computed(function () {
        if (channel.Icon && channel.Icon.length > 0)
            return 'data:image/png;base64,' + channel.Icon;
        return '';
    });
}
