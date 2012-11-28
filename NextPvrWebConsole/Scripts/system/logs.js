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
            $.address.value("/tab/logs/" + item.name());
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

    $('#btnLogClose').click(function () {
        $.address.value("/tab/logs");
    });
});

$.address.change(function (event) {
    console.log(event);
    console.log(event.value.substr(0, 6));
    if (event.value.startsWith('/tab/logs/')) {
        var logName = event.value.substr(10);
        console.log('logname: ' + logName);

        $('.system .vtab-buttons .selected').removeClass('selected');
        $('.system .vtab-content .selected').removeClass('selected');
        $('#system-tab-logs').addClass('selected');
        $('.system .vtab-buttons .logs').addClass('selected');

        var tr = $('#system-tab-logs tr[data-name="' + logName + '"]');
        var oid = tr.attr('data-oid');
        var name = tr.attr('data-name');
        $('#logFileWindow .name').text(logName);
        $('#logFileWindow pre').text($.i18n._('Loading please wait...'));
        $('#logFileWindow pre').load('/system/log?oid=' + encodeURIComponent(oid ? oid : '') + '&name=' + encodeURIComponent(logName));
        $('#system-tab-logs table').css('display', 'none');
        $('#logFileWindow').removeAttr('style');
    }
    else if (event.value == '/tab/logs') {
        $('#system-tab-logs table').removeAttr('style');
        $('#logFileWindow').css('display', 'none');
    }
});

