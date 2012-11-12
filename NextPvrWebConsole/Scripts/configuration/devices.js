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

        $('#btnDevicesSave').click(function () {
            var devices = new Array();
            var priority = 1;
            $.each(self.devices(), function (i, ele) {
                ele.priority(priority++);
                ele.enabled($('#device_' + ele.oid() + '_enabled:checked').length > 0);
                devices.push(ele.toApiObject());
            });
            console.log(devices);
            ajax.postJSON('Configuration/UpdateDevices',
                {
                    UseReverseOrderForLiveTv: $('#modelDevices_UseReverseOrderForLiveTv:checked').length > 0,
                    Devices: devices
                },
                function () {
                    console.log('success');
                }
            );
        });
    }
    ko.applyBindings(new DevicesViewModel(), $('#configuration-tab-devices').get(0));

    $('#configuration-tab-devices td.enabled input[type=checkbox]').iButton();
});