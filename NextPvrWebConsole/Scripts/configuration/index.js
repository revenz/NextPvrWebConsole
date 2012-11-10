/// <reference path="../apihelper.js" />
/// <reference path="../functions.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/knockout-2.2.0.js" />

$(function () {
    $('#btnLiveTvBufferBrowse').click(function () {
        
        var dialog_buttons = {};
        dialog_buttons[$.i18n._("OK")] = function () {
            var dir = $('#FolderBrowserWindow').find('.selected').attr('rel');
            $('[id$=LiveTvBufferDirectory]').val(dir);
            // get selected folder...
            dialog.dialog('close');
        };
        dialog_buttons[$.i18n._("Cancel")] = function () { dialog.dialog('close'); };

        var dialog = $('#FolderBrowserWindow').dialog({
            title: $.i18n._('Folder Browser'),
            buttons: dialog_buttons,
            width: 450,
            height: 400,
            modal: true,
            open: function () {
                $('#FolderBrowserWindow').closest('.ui-dialog').find('.ui-dialog-buttonpane').prepend('<button id="FileTree-NewFolder" class="ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only"><span>Create Folder</span></button>');
            }
        });
    });
});