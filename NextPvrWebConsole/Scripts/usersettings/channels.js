/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../jquery-1.8.2.js" />
/// <reference path="../jquery-ui-1.9.0.js" />
/// <reference path="../knockout-2.2.0.js" />

$(function () {
    function ChannelsViewModel() {
        var self = this;
        self.channels = ko.observableArray([]);

        var refreshChannels = function () {
            api.getJSON("channels", null, function (allData) {
                var mapped = $.map(allData, function (item) { return new Channel(item) });
                self.channels(mapped);
            });
        };
        refreshChannels();
    }

    var div = $('.usersettings > .channels');
    if (div.length > 0)
        ko.applyBindings(new ChannelsViewModel(), div.get(0));
});