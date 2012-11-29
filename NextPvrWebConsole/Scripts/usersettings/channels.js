/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />

$(function () {

    function ChannelsViewModel() {
        var self = this;
        var pendingChanges = false;

        self.channels = ko.observableArray([]);

        var refreshChannels = function () {
            api.getJSON("channels", { IncludeDisabled: true }, function (allData) {
                var mapped = $.map(allData, function (item) {
                    var c = new Channel(item);
                    c.number.subscribe(function (newValue) { pendingChanges = true });
                    c.enabled.subscribe(function (newValue) { pendingChanges = true });
                    return c;
                });
                self.channels(mapped);
                $('#usersettings-channels input[type=checkbox]').iButton();
            });
        };
        refreshChannels();

        $('#usersettings-channels').on('click.channellist', '.ibutton-handle', function () {
            pendingChanges = true;
        });

        self.save = function () {
            var apiChannels = new Array();
            $.each(self.channels(), function (i, ele) {
                ele.enabled($('#usersettings-channels tbody tr:eq(' + i + ') [type=checkbox]:checked').length > 0);
                apiChannels.push(ele.toApiObject());
            });
            api.postJSON("channels/update", apiChannels, function () {
                pendingChanges = false;
            });
        };

        $(window).on('beforeunload', function () {
            if (pendingChanges)
                return $.i18n._('There are unsaved changes pending to your Channels.');
        });
    }

    var div = $('#user-settings-tab-channels');
    if (div.length > 0) {
        ko.applyBindings(new ChannelsViewModel(), div.get(0));
        $('#usersettings-channels').removeAttr('style');
    }
});