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
    this.formatDateTimeShort = function (date) {
        if (!(date instanceof Date))
            date = new Date(date);
        return $.format.date(date, 'h:mm a') + ' (' + $.format.date(date, 'd MMM') + ')';
    };

    this.showMessage = function (message, title) {
        alert(message); // for now
    };

    this.showSuccess = function (message, title) {
        toastr.success(message, title ? title : $.i18n._("Success"));
    };

    this.showInfo = function (message, title) {
        toastr.success(message, title ? title : $.i18n._("Information"));
    };

    this.showError = function (message, title) {
        toastr.error(message, title ? title : $.i18n._("Error"));
    };

    this.showWarning = function (message, title) {
        toastr.success(message, title ? title : $.i18n._("Warning"));
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
    var unloadFunction = function (e) {
        if (doWorkCount > 0) {
            var message = $.i18n._("This page is currently procesing a request.");
            e = e || window.event;
            // For IE and Firefox
            if (e) {
                e.returnValue = message;
            }
            // For Safari
            return message;
        }
    };
    if (window.addEventListener)
        window.addEventListener('unload', unloadFunction, false);
    else if (window.attachEvent)
        window.attachEvent('unload', unloadFunction);

    this.confirmMessage = function (settings) {
        settings = $.extend({
            title: $.i18n._('Confirm'),
            message: $.i18n._('Are you sure?'),
            yesText: $.i18n._('Yes'),
                noText: $.i18n._('No'),
            yes: null,
            no: null,
            minWidth: 400,
            minHeight: 200
        }, settings);
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

    this.promptMessage = function (settings) {
        settings = $.extend({
            title: $.i18n._('Input'),
            message: $.i18n._('Please inut a value'),
            validationExpression: "(.*?)",
            validationMessage: $.i18n._('Input is required.'),
            initialValue: '',
            success: null,
            maxLength:100
        }, settings);

        var div = $('<div><span class="message"></span><input type="text" style="width: 99%;padding: 0;margin: 15px 0 5px;" /><span class="field-validation-error errormessage" /> </div>');
        div.appendTo('body')
        div.find('.message').text(settings.message);
        div.find('.errormessage').text(settings.validationMessage).css('display', 'none');
        var dialog_buttons = {};
        var input = div.find('input');
        if(settings.maxLength > 0)
            input.attr('maxlength', settings.maxLength);
        input.val(settings.initialValue);
        var errorMessage = div.find('.errormessage');
        var rgxValidate = null;
        if (settings.validationExpression) {
            rgxValidate = new RegExp(settings.validationExpression);
            input.keyup(function () {
                var value = input.val();
                errorMessage.css('display', rgxValidate.test(value) ? 'none' : '');
            });
        }
        dialog_buttons[$.i18n._('OK')] = function ()
        {
            var value = input.val();
            // validate
            if (rgxValidate && !rgxValidate.test(value)) 
                return; // its invalid

            if (settings.success) { settings.success(value); }

            div.dialog('close');
        };
        dialog_buttons[$.i18n._('Cancel')] = function () { div.dialog('close'); }
        input.keypress(function (e) {
            if (e.which == 13)
                div.closest('.ui-dialog').find('.ui-dialog-buttonset button:first').click();
        });
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