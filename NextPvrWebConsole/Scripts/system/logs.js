/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery.dateFormat-1.0.js" />

$(function () {

    function LogsViewModel() {
        // Data
        var self = this;
        self.logs = ko.observableArray([]);

        self.open = function (item) {
            var dialog = $('#logFileWindow');
            dialog.find('iframe').attr('src', '/system/log?oid=' + encodeURIComponent(item.oid()));

            var width = $('.body-wrapper').width() - 200;
            var height = $('.body-wrapper').height() - 200;
            var dialog_buttons = {};
            dialog_buttons[$.i18n._("Close")] = function () { dialog.dialog('close'); }

            dialog.dialog({
                modal: true,
                title: item.name(),
                width: width,
                height: height,
                buttons: dialog_buttons
            });
        };

        // Load initial state from server
        api.getJSON("logs", null, function (allData) {
            var mapped = $.map(allData, function (item) { return new Log(item) });
            self.logs(mapped);
        });
    }
    var viewModel = new LogsViewModel();
    ko.applyBindings(viewModel, $('#system-tab-logs').get(0));

    $('#system-tab-logs > table').removeAttr('style');

});