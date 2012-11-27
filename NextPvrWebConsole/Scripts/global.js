/// <reference path="apihelper.js" />
/// <reference path="functions.js" />
/// <reference path="core/jquery.i18n.js" />
/// <reference path="core/jquery-ui-1.9.0.js" />
/// <reference path="core/jquery-1.8.2.js" />
/// <reference path="core/knockout-2.2.0.js" />

var DIRECTORY_REGULAR_EXPRSESION = '^([^"*/:?|<>\\\\.\\x00-\\x20]([^"*/:?|<>\\\\\\x00-\\x1F]*[^"*/:?|<>\\\\.\\x00-\\x20])?)$';
var DIRECTORY_ERROR_MESSAGE = $.i18n._('Invalid folder name');
var DIRECTORY_CREATE_MESSAGE = $.i18n._('Type in the name of the folder to create.');
var DIRECTORY_CREATE_TITLE = $.i18n._('Create Recording Folder');
$(function () {
    $('html').removeClass('no-js');
    var userAgent = navigator.userAgent.toLowerCase();
    $.browser.chrome = /chrome/.test(navigator.userAgent.toLowerCase());

    // Is this a version of Chrome?
    if ($.browser.chrome)
        $('html').addClass('chrome webkit');
    else if ($.browser.safari)// Is this a version of Safari?
        $('html').addClass('safari webkit');

    // Is this a version of Mozilla?
    if ($.browser.mozilla) {
        //Is it Firefox?
        if (navigator.userAgent.toLowerCase().indexOf('firefox') != -1)
            $('html').addClass('firefox mozilla');
        else // If not then it must be another Mozilla
            $('html').addClass('moizlla');
    }

    // Is this a version of Opera?
    if ($.browser.opera)
        $('html').addClass('opera');

    $(":checkbox:not(.noibutton)").iButton();

    var changePasswordDialog = $('#ChangePasswordContainer');
    $('#ChangePasswordContainer input').keypress(function (e) {
        if (e.which == 13) {
            $('#changePasswordForm').submit();
            changePasswordDialog.dialog('close');
        }
    });
    $('#lnkChangePassword').click(function () {
        changePasswordDialog.find('input').val(''); // reset the inputs
        changePasswordDialog.find('input:eq(0)').focus();
        var dialog_buttons = {};
        dialog_buttons[$.i18n._("OK")] = function () {
            if ($('#changePasswordForm').valid() == false)
                return;
            $('#changePasswordForm').submit();
            changePasswordDialog.dialog('close');
        };
        dialog_buttons[$.i18n._("Cancel")] = function () {
            changePasswordDialog.dialog('close');
        };
        changePasswordDialog.dialog({
            modal: true,
            minWidth: 550,
            minHeight: 250,
            maxWidth: 550,
            maxHeight: 250,
            title: $.i18n._("Change Password"),
            buttons: dialog_buttons
        });
    });
});

function ChangePassword_OnComplete(obj) {
    var data = eval('(' + obj.responseText + ')');
    if (data.success != true) {
        var msg = $.i18n._('Failed to change password.');
        if(data.message && data.message.length > 0)
            msg = data.message;
        gui.showError(msg);
    }
}