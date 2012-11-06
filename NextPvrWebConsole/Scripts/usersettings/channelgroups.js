/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../jquery-1.8.2.js" />
/// <reference path="../jquery-ui-1.9.0.js" />
/// <reference path="../knockout-2.2.0.js" />

$(function () {
    function ChannelGroupsViewModel() {
        var self = this;
        self.channelgroups = ko.observableArray([]);
        self.selectedChannelGroup = ko.observable();

        self.select = function (channelGroup) {
            api.getJSON("channelgroups/getchannels", { groupName: channelGroup.name() }, function (allData) {
                var mapped = $.map(allData, function (item) { return new ChannelGroupEditorChannel(item) });
                channelGroup.channels(mapped);
                self.selectedChannelGroup(channelGroup);

                $('#channel-group-editor').dialog({
                    autoOpen: true,
                    modal: true,
                    title: 'Channel Group Editor'
                });
            });
        };

        var refreshChannelGroups = function () {
            api.getJSON("channelgroups", null, function (allData) {
                var mapped = $.map(allData, function (item) { return new ChannelGroup(item) });
                self.channelgroups(mapped);
            });
        };
        refreshChannelGroups();
    }

    var div = $('.usersettings > .channel-groups');
    if (div.length > 0)
        ko.applyBindings(new ChannelGroupsViewModel(), div.get(0));

    function ChannelGroupEditorChannel(channel) {
        var self = this;
        self.name = ko.observable(channel.Name);
        self.oid = ko.observable(channel.Oid);
        self.enabled = ko.observable(channel.Enabled);
    }
});