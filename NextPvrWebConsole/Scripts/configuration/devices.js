/// <reference path="core/jquery-1.8.2.js" />
/// <reference path="core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />

$(function () {
    function DevicesViewModel() {
        // Data
        var self = this;

        self.devices = ko.observableArray(devices);

        self.afterMove = function (arg) {
            $('#device_' + arg.item.oid() + '_enabled').iButton();
        };

        self.save = function() {
            var devices = new Array();
            var priority = 1;
            $.each(self.devices(), function (i, ele) {
                ele.priority(priority++);
                ele.enabled($('#device_' + ele.oid() + '_enabled:checked').length > 0);
                devices.push(ele.toApiObject());
            });
            ajax.postJSON('Configuration/UpdateDevices',
                {
                    UseReverseOrderForLiveTv: $('#modelDevices_UseReverseOrderForLiveTv:checked').length > 0,
                    Devices: devices
                }
            );
        };

        self.updateEpg = function () {
            api.getJSON('configuration/updateepg');
        };

        self.emptyEpg = function () {
            gui.confirmMessage({ 
                message: $.i18n._('Are sure you want to empty the EPG data?'),
                yes: function() {
                    api.getJSON('configuration/emptyepg');
                }
            });
        };
    }
    ko.applyBindings(new DevicesViewModel(), $('#configuration-tab-devices').get(0));

    $('#configuration-tab-devices td.enabled input[type=checkbox]').iButton();
});