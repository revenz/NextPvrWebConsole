/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../jquery-1.8.2.js" />
/// <reference path="../jquery-ui-1.9.0.js" />
/// <reference path="../knockout-2.2.0.js" />

$(function () {
    var container = $('.user-settings .channels');

    function ChannelsViewModel() {
        var self = this;
        self.channels = ko.observableArray([]);

        var refreshChannels = function () {
            api.getJSON("channels", { IncludeDisabled: true }, function (allData) {
                var mapped = $.map(allData, function (item) { return new Channel(item) });
                self.channels(mapped);
                resizeTable();
            });
        };
        refreshChannels();

        self.edit = function () {
            funEdit(true);
        };

        var funEdit = function (editing) {
            if (editing) {
                container.find('table').removeClass('readonly');
            } else {
                container.find('table').addClass('readonly');
            }
            container.find('input').attr('readonly', editing ? null : 'readonly').attr('disabled', editing ? null : 'disabled');
            $('#user-settings-channels-edit').css('display', editing ? 'none' : '');
            $('#user-settings-channels-save').css('display', editing ? '' : 'none');
            $('#user-settings-channels-cancel').css('display', editing ? '' : 'none');
        };

        self.cancel = function () {
            funEdit(false);
            // need to revert to last saved changes
            refreshChannels();
        };

        self.save = function () {
            funEdit(false);
            var apiChannels = new Array();
            $.each(self.channels(), function (i, ele) {
                apiChannels.push(ele.toApiObject());
            });
            api.postJSON("channels/update", apiChannels, function () {
                refreshChannels();
            });
        };
    }

    var div = $('.user-settings .vtab-content > .channels');
    if (div.length > 0)
        ko.applyBindings(new ChannelsViewModel(), div.get(0));

    var resizeTable = function () {
        var width = $('.vtab-content.selected').width();
        var height = $('.vtab-content.selected').height();
        var tbl = $('#usersettings-channels');
        tbl.width(width);
        tbl.find('tbody').height(height - 100);
        $('#usersettings-channels .name').css('width', width - 270);
    };

    $(window).resize(resizeTable);
    resizeTable();
});