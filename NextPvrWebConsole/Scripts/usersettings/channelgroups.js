/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />

$(function () {
    function ChannelGroupsViewModel() {
        var self = this;
        var pendingChanges = false;

        self.channelgroups = ko.observableArray([]);
        self.selectedChannelGroup = ko.observable();

        self.remove = function (group) {
            gui.confirmMessage({
                message: $.i18n._("Are you sure you want to remove the Channel Group '%s'?", [group.name()]),
                yes: function () {
                    pendingChanges = true;
                    self.channelgroups.remove(group);
                }
            });
        };

        $('#channelgrouplist').on('click.channelgrouplist', '.ibutton-handle', function () {
            pendingChanges = true;
        });

        self.afterMove = function (arg) {
            pendingChanges = true;
            $('#channelgrouplist li:eq(' + arg.targetIndex + ') [type=checkbox]:visible').iButton();
        };

        self.add = function () {
            var group = new ChannelGroup();
            showEditor(group, function () {
                pendingChanges = true;
                self.channelgroups.push(group);
            });
        };

        self.select = function (channelGroup) {
            showEditor(channelGroup, function () {
                pendingChanges = true;
            });
        };

        self.save = function () {
            // push all channelgroups to server
            var groups = [];
            $.each(self.channelgroups(), function (i, ele) {
                ele.enabled($('#channelgrouplist li:eq(' + i + ') [type=checkbox]:checked').length > 0);
                groups.push(ele.toApiObject());
            });
            api.postJSON('channelgroups/update', groups, function () {
                pendingChanges = false;
            });
        };

        self.channels = ko.observableArray([]);

        var showEditor = function (group, callback) {
            var editing = new ChannelGroup();
            editing.name(group.name());
            editing.channelOids(group.channelOids());
            api.getJSON('channels', null, function (allData) {
                var mapped = $.map(allData, function (item) {
                    return new ChannelGroupEditorChannel({ Name: item.Name, Oid: item.Oid, Enabled: $.inArray(item.Oid, group.channelOids()) >= 0 })
                });
                self.channels(mapped)

                self.selectedChannelGroup(editing);
                var dialog_buttons = {};
                dialog_buttons[$.i18n._("OK")] = function () {
                    // check name is unique
                    var duplicate = false;
                    $.each(self.channelgroups(), function (i, ele) {
                        if (ele.shared()) return;
                        if (ele == group) return;
                        if (ele.name().toLowerCase().trim() == editing.name().toLowerCase().trim())
                            duplicate = true;
                    });

                    if (duplicate) {
                        gui.showError($.i18n._("Channel Group names must be unique."));
                        return;
                    };

                    var channelOids = [];
                    $.each($(this).find('.channels input:checked'), function (i, ele) {
                        channelOids.push(parseInt($(ele).val(), 10));
                    });
                    // update original object
                    group.name(editing.name());
                    group.channelOids(channelOids);
                    if (callback)
                        callback(group);
                    editor.dialog('close');
                };
                dialog_buttons[$.i18n._("Cancel")] = function () { editor.dialog('close'); };

                var editor = $('#channel-group-editor').dialog({
                    autoOpen: true,
                    modal: true,
                    minWidth: 600,
                    minHeight: 300,
                    title: $.i18n._('Channel Group Editor'),
                    buttons: dialog_buttons,
                    close: function () {
                        self.selectedChannelGroup(null);
                        self.channels([]);
                    }
                });
            });
        };

        var refreshChannelGroups = function () {
            api.getJSON('channelgroups?LoadChannelOids=true', null, function (allData) {
                var mapped = $.map(allData, function (item) { return new ChannelGroup(item) });
                self.channelgroups(mapped);
                $('#channelgrouplist [type=checkbox]:visible').iButton();
            });
        };
        refreshChannelGroups();

        $(window).on('beforeunload', function () {
            if (pendingChanges)
                return $.i18n._('There are unsaved changes pending to your Channel Groups.');
        });
    }

    var div = $('#user-settings-tab-channelgroups');
    if (div.length > 0) {
        ko.applyBindings(new ChannelGroupsViewModel(), div.get(0));
        $('#channelgrouplist').removeAttr('style');
    }

    function ChannelGroupEditorChannel(channel) {
        var self = this;
        self.name = ko.observable(channel.Name);
        self.oid = ko.observable(channel.Oid);
        self.enabled = ko.observable(channel.Enabled);
    }
});
