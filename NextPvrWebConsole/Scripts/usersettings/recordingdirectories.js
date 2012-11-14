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

        self.delete = function (directory) {
            gui.confirmMessage({
                message: $.i18n._("Are you sure you want to delete the Recording Directory '%s'?", [directory.name()]),
                yes: function () {
                    api.deleteJSON("recordingdirectories/" + directory.oid(), null, function () {
                        self.recordingdirectories.remove(directory);
                    });
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
                    api.putJSON('recordingdirectories/' + directory.oid(), name, function () {
                        directory.name(name);
                    });
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
                    api.postJSON('recordingdirectories', name, function (item) {
                        var directory = new RecordingDirectory(item);
                        self.recordingdirectories.push(directory);
                    });
                }
            });
        };

        var refreshRecordingDirectories = function () {
            api.getJSON("recordingdirectories", null, function (allData) {
                var mapped = $.map(allData, function (item) { return new RecordingDirectory(item) });
                self.recordingdirectories(mapped);
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