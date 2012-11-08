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

        self.delete = function (group) {
            api.deleteJSON("channelgroups/" + group.oid(), null, function () {
                self.channelgroups.remove(group);
            });
        };

        self.add = function () {
            console.log('adding');
            var group = new ChannelGroup();
            showEditor(group, function () {
                self.channelgroups.push(group);
            });
        };

        self.select = function (channelGroup) {
            showEditor(channelGroup);
        };

        var showEditor = function (group, callback) {
            api.getJSON("channelgroups/channels/" + group.oid(), null, function (allData) {
                var mapped = $.map(allData, function (item) { return new ChannelGroupEditorChannel(item) });
                group.channels(mapped)

                self.selectedChannelGroup(group);

                var editor = $('#channel-group-editor').dialog({
                    autoOpen: true,
                    modal: true,
                    minWidth: 600,
                    minHeight: 300,
                    title: 'Channel Group Editor',
                    buttons: {
                        'Save': function () {
                            var channelsStr = '';
                            $.each($(this).find('.channels input:checked'), function (i, ele) {
                                channelsStr += $(ele).val() + ',';
                            });
                            if (channelsStr.length > 0)
                                channelsStr = channelsStr.substring(0, channelsStr.length - 1);
                            SaveChannelGroup({ oid: group.oid(), name: group.name(), orderOid: group.orderOid, channels: channelsStr }, function () {
                                if (callback)
                                    callback();
                                editor.dialog('close');
                            });
                        },
                        'Cancel': function () {
                            editor.dialog('close');
                        }
                    },
                    close: function () {
                        self.selectedChannelGroup(null);
                    }
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

    var div = $('.user-settings .vtab-content > .channel-groups');
    if (div.length > 0)
        ko.applyBindings(new ChannelGroupsViewModel(), div.get(0));

    function ChannelGroupEditorChannel(channel) {
        var self = this;
        self.name = ko.observable(channel.Name);
        self.oid = ko.observable(channel.Oid);
        self.enabled = ko.observable(channel.Enabled);
    }

    function SaveChannelGroup(channelGroup, callBack) {
        console.log('saving channel group');
        api.getJSON('ChannelGroups/SaveChannelGroup', channelGroup, callBack);
    }
});