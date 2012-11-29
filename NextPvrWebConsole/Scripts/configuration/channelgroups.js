/// <reference path="core/jquery-1.8.2.js" />
/// <reference path="core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />

$(function () {
    function ChannelGroupsViewModel() {
        var self = this;

        self.channelGroups = ko.observableArray(channelGroups);
        self.channels = ko.observableArray([]);

        self.remove = function (item) {
            self.channelGroups.remove(item);
        };

        self.save = function () {
            var groups = new Array();
            var orderoid = 0;
            $.each(self.channelGroups(), function (i, ele) {
                ele.orderoid(orderoid++);
                groups.push(ele.toApiObject());
            });
            ajax.postJSON('Configuration/UpdateChannelGroups',
                {
                    ChannelGroups: groups
                },
                function () {
                }
            );
        };

        self.rename = function (item) {
            gui.promptMessage({
                title: $.i18n._('Rename Channel Group'),
                initialValue: item.name(),
                message: $.i18n._('Type in the new name of the channel group.'),
                success: function (name) {
                    if (name.toLowerCase() == 'all channels') {
                        gui.showError($.i18n._("Cannot create a Channel Group 'All Channels' as it is reserved."));
                        return;
                    }
                    item.name(name);
                }
            });
        };

        self.create = function () {
            gui.promptMessage({
                title: $.i18n._('Create Channel Group'),
                message: $.i18n._('Type in the name of the channel group to create.'),
                success: function (name) {
                    if (name.toLowerCase() == 'all channels') {
                        gui.showError($.i18n._("Cannot create a Channel Group 'All Channels' as it is reserved."));
                        return;
                    }
                    self.channelGroups.push(new ChannelGroup({ Oid: 0, Name: name, OrderOid: -1 }));
                }
            });
        };

        self.selectChannels = function (item) {
            self.channels([]);
            api.getJSON('channels', null, function (data) {
                $.each(data, function (i, ele) {
                    self.channels.push(new ChannelGroupChannel({ Oid: ele.Oid, Name: ele.Name, Enabled: $.inArray(ele.Oid, item.channelOids()) >= 0 }));
                });

                var dialog = $('#ChannelGroups-Channels');
                var dialog_buttons = {};
                dialog_buttons[$.i18n._("OK")] = function () {
                    var newOids = new Array();
                    $.each(self.channels(), function (i, ele) {
                        if (ele.enabled())
                            newOids.push(ele.oid());
                    });
                    item.channelOids(newOids);
                    dialog.dialog('close');
                };
                dialog_buttons[$.i18n._("Cancel")] = function () { dialog.dialog('close'); };

                dialog.dialog({
                    title: $.i18n._('Group Channels'),
                    modal: true,
                    minWidth: 450,
                    minHeight: 300,
                    buttons: dialog_buttons
                });
            });
        };
    }
    ko.applyBindings(new ChannelGroupsViewModel(), $('#configuration-tab-channelgroups').get(0));

});