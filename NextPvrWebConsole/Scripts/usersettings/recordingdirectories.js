/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />

$(function () {
    function RecordingDirectoriesViewModel() {
        var self = this;
        self.recordingdirectories = ko.observableArray([]);

        self.selectedChannelGroup = ko.observable();

        self.selectedDefault = ko.observable();

        $('#user-settings-tab-recordingdirectories').on('click.recordingdirectories', 'input[type=radio]', function () {
            self.selectedDefault(parseInt($(this).val(), 10));
        });

        self.delete = function (directory) {
            gui.confirmMessage({
                message: $.i18n._("Are you sure you want to delete the Recording Directory '%s'?", [directory.name()]),
                yes: function () {
                    self.recordingdirectories.remove(directory);
                }
            });
        };

        self.edit = function (directory) {
            gui.promptMessage({
                title: $.i18n._("Edit Recording Folder"),
                message: $.i18n._("Type in the name of the folder."),
                validationMessage: DIRECTORY_ERROR_MESSAGE,
                validationExpression: DIRECTORY_REGULAR_EXPRSESION,
                initialValue: directory.name(),
                success: function (name) {
                    var duplicate = false;
                    $.each(self.recordingdirectories(), function(i, ele){
                        if(directory == ele)
                            return;
                        if(ele.name().trim().toLowerCase() == name.trim().toLowerCase())
                            duplicate = true;
                    });
                    if(duplicate)
                        gui.showError( $.i18n._('Recording Directory names must be unique.') );
                    else
                        directory.name(name);
                }
            });
        };

        self.add = function () {
            gui.promptMessage({
                title: DIRECTORY_CREATE_TITLE,
                message: DIRECTORY_CREATE_MESSAGE,
                validationMessage: DIRECTORY_ERROR_MESSAGE,
                validationExpression: DIRECTORY_REGULAR_EXPRSESION,
                success: function (name) {
                    var duplicate = false;
                    $.each(self.recordingdirectories(), function(i, ele){
                        if(ele.name().trim().toLowerCase() == name.trim().toLowerCase())
                            duplicate = true;
                    });
                    if(duplicate)
                        gui.showError( $.i18n._('Recording Directory names must be unique.') );
                    else {
                        var rd = new RecordingDirectory();
                        rd.name(name);
                        self.recordingdirectories.push(rd);
                    }
                }
            });
        };

        self.save = function(){        
            var directories = new Array();
            $.each(self.recordingdirectories(), function (i, ele) {
                ele.isDefault(i == self.selectedDefault());
                directories.push(ele.toApiObject());
            });
            if (directories.length == 0) {
                gui.showError($.i18n._('At least one recording directory is required.'));
            } else {
                api.postJSON('recordingdirectories', directories);
            }
        };

        var refreshRecordingDirectories = function () {
            api.getJSON("recordingdirectories?IncludeShared=true", null, function (allData) {
                var mapped = $.map(allData, function (item) { return new RecordingDirectory(item) });
                self.recordingdirectories(mapped);
                
                for (var i = 0; i < mapped.length; i++) {
                    if (mapped[i].isDefault())
                        self.selectedDefault(i);
                }
            });
        };
        refreshRecordingDirectories();
    }

    var div = $('#user-settings-tab-recordingdirectories');
    if (div.length > 0) {
        ko.applyBindings(new RecordingDirectoriesViewModel(), div.get(0));
        div.find('ul').removeAttr('style');
    }
});