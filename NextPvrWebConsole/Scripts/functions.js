/// <reference path="jquery-1.8.2.js" />
/// <reference path="jquery.dateFormat-1.0.js" />
/// <reference path="jquery-ui-1.9.0.js" />

var gui = new function () {

    var doWorkCount = 0;

    this.formatDateLong = function (date) {
        if (!(date instanceof Date))
            date = new Date(date);
        return $.format.date(date, 'd MMMM yyyy h:mm a');
    }
    this.formatDateShort = function (date) {
        if (!(date instanceof Date))
            date = new Date(date);
        return $.format.date(date, 'd MMMM');
    }
    this.formatTime = function (date) {
        if (!(date instanceof Date))
            date = new Date(date);
        return $.format.date(date, 'h:mm a');
    }

    this.showMessage = function (message, title) {
        alert(message); // for now
    };

    this.showSuccess = function (message, title) {
        toastr.success(message, title ? title : 'Success');
    };

    this.showInfo = function (message, title) {
        toastr.success(message, title ? title : 'Information');
    };

    this.showError = function (message, title) {
        toastr.error(message, title ? title : 'Error');
    };

    this.showWarning = function (message, title) {
        toastr.success(message, title ? title : 'Warning');
    };

    this.doWork = function () {
        doWorkCount++;

        // show working div
        $('#working').show();
    };

    this.finishWork = function () {
        if (--doWorkCount < 1)
            $('#working').hide();
        else
            doWorkCount = 0; // make sure this doesnt drop below 0
    };
    window.onbeforeunload = function (e) {
        if (doWorkCount > 0) {
            var message = "This page is currently procesing a request.";
            e = e || window.event;
            // For IE and Firefox
            if (e) {
                e.returnValue = message;
            }
            // For Safari
            return message;
        }
    };

    this.confirmMessage = function (settings) {
        settings = $.extend({
            title: 'Confirm',
            message: 'Are you sure?',
            yesText: 'Yes',
            noText: 'No',
            yes: null,
            no: null,
            minWidth: 400,
            minHeight: 200
        }, settings);
        console.log(settings);
        var div = $('<div><span></span></div>');
        div.appendTo('body')
        div.find('span').text(settings.message);
        var dialog_buttons = {};
        dialog_buttons[settings.yesText] = function () { if (settings.yes) { settings.yes(); } div.dialog('close'); };
        dialog_buttons[settings.noText] = function () { if (settings.no) { settings.no(); } div.dialog('close'); }
        div.dialog(
        {
            resizable: false,
            modal: true,
            minWidth: settings.minWidth,
            minHeight: settings.minHeight,
            title: settings.title,
            buttons: dialog_buttons
        });
    };
}