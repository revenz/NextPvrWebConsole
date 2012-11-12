/// <reference path="../apihelper.js" />
/// <reference path="../functions.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/knockout-2.2.0.js" />

$(function () {

    var dirRegularExpression = '^([^"*/:?|<>\\\\.\\x00-\\x20]([^"*/:?|<>\\\\\\x00-\\x1F]*[^"*/:?|<>\\\\.\\x00-\\x20])?)$';
    var dirErrorMessage = $.i18n._('Invalid folder name');

    function RecordingDirectoriesViewModel() {
        // Data
        var self = this;

        self.selectedDefault = ko.observable();
        for (var i = 0; i < recordingDirectories.length; i++)
            if (recordingDirectories[i].isDefault)
                self.selectedDefault(i);

        self.add = function () {
            ShowFolderBrowser(function (dir) {
                gui.promptMessage({
                    title: $.i18n._('Create Recording Folder'),
                    message: $.i18n._('Type in the name of the folder to create.'),
                    validationMessage: dirErrorMessage,
                    validationExpression: dirRegularExpression,
                    success: function (name) {
                        self.recordingDirectories.push(new RecordingDirectory({ Oid: 0, Name: name, Path: dir, IsDefault: false }));
                    }
                });
            });
        }

        $('.recording-directories').on('click.recordingdirectories', 'input[type=radio]', function () {
            self.selectedDefault(parseInt($(this).val(), 10));
        });

        self.remove = function (recordingDirectory) {
            if (recordingDirectory.isDefault())
                self.selectedDefault(0);
            self.recordingDirectories.remove(recordingDirectory);
        };

        self.recordingDirectories = ko.observableArray(recordingDirectories);

        $('#btnRecordingsSave').click(function () {
            var recordings = new Array();
            console.log('default: ' + self.selectedDefault());
            $.each(self.recordingDirectories(), function (i, ele) {
                ele.isDefault(i == self.selectedDefault());
                console.log(ele.toApiObject());
                recordings.push(ele.toApiObject());
            });
            if (recordings.length == 0) {
                gui.showError($.i18n._('At least one recording directory is required.'));
            } else {
                ajax.postJSON('Configuration/UpdateRecording',
                {
                    PrePadding: $('#modelRecording_PrePadding').val(),
                    PostPadding: $('#modelRecording_PostPadding').val(),
                    BlockShutDownWhileRecording: $('#modelRecording_BlockShutDownWhileRecording:checked').length > 0,
                    RecurringMatch: $('#modelRecording_RecurringMatch :selected').val(),
                    AvoidDuplicateRecordings: $('#modelRecording_AvoidDuplicateRecordings:checked').length > 0,
                    RecordingDirectories: recordings
                },
                function () {
                    console.log('success');
                });
            }
        });
    }
    ko.applyBindings(new RecordingDirectoriesViewModel(), $('#configuration-tab-recording').get(0));

    $.each($('.configuration.vtab-container input[type=number]'), function (i, ele) {
        var $ele = $(ele);
        var min = parseInt($ele.attr('data-val-range-min'), 10);
        var max = parseInt($ele.attr('data-val-range-max'), 10);
        if (!isNaN(min))
            $ele.attr('min', min);
        if (!isNaN(max))
            $ele.attr('max', max);
    });

    $('#btnLiveTvBufferBrowse').click(function () {
        ShowFolderBrowser(function (dir) {
            $('[id$=LiveTvBufferDirectory]').val(dir);
        });
    });

    function ShowFolderBrowser(callback) {

        var dialog_buttons = {};
        dialog_buttons[$.i18n._("OK")] = function () {
            var dir = $('#FolderBrowserWindow').find('.selected').attr('rel');
            if (callback)
                callback(dir);
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
                        validationMessage: dirErrorMessage,
                        validationExpression: dirRegularExpression,
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
    }
});