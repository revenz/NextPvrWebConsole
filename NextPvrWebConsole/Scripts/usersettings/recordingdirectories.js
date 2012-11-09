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
            api.deleteJSON("recordingdirectories/" + directory.oid(), null, function () {
                self.recordingdirectories.remove(directory);
            });
        };

        self.add = function () {
            var directory = new RecordingDirectory();
            self.recordingdirectories.push(directory);
        };

        self.select = function (channelGroup) {
            
        };

        var refreshRecordingDirectories = function () {
            api.getJSON("recordingdirectories", null, function (allData) {
                var mapped = $.map(allData, function (item) { return new RecordingDirectory(item) });
                self.recordingdirectories(mapped);
            });
        };
        refreshRecordingDirectories();
    }

    var div = $('.user-settings .vtab-content > .recording-directories');
    if(div.length > 0)
        ko.applyBindings(new RecordingDirectoriesViewModel(), div.get(0));
});