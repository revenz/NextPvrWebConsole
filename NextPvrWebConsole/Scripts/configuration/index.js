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
                var buttonpane = $('#FolderBrowserWindow').closest('.ui-dialog').find('.ui-dialog-buttonpane');
                buttonpane.prepend('<button id="FileTree-NewFolder" class="ui-button ui-widget ui-state-default ui-corner-all ui-button-text-only"><span></span></button>');                
                var btnNewFolder = buttonpane.find('#FileTree-NewFolder');
                btnNewFolder.find('span').text($.i18n._('New Folder'));
                btnNewFolder.click(function () {
                    var selected = $('#FolderBrowserWindow').find('.jqueryFileTree .selected').closest('li');
                    var path = selected.find('a').attr('rel');
                    gui.promptMessage({
                        title: $.i18n._('Create Folder'),
                        message: $.i18n._('Type in the name of the folder to create.'),
                        validationMessage: $.i18n._('Invalid folder name'),
                        validationExpression: '^([^"*/:?|<>\\\\.\\x00-\\x20]([^"*/:?|<>\\\\\\x00-\\x1F]*[^"*/:?|<>\\\\.\\x00-\\x20])?)$',
                        success: function (name) {
                            ajax.postJSON('File/CreateDirectory', { path: path, name: name }, function (result) {
                                if (result.success) {
                                    // easiest way to refresh this list, not the prettiest though, might improve this addon one day.
                                    if (!selected.hasClass('expanded'))
                                        selected.removeClass('expanded').addClass('collapsed');
                                    selected.addClass('collapsed');
                                    selected.find('a').click();
                                }
                            });
                        }
                    });
                });
            }
        });
    });
});