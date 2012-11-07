/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../jquery-1.8.2.js" />
/// <reference path="../jquery-ui-1.9.0.js" />
/// <reference path="../knockout-2.2.0.js" />

$(function () {
    function RecordingDirectoriesViewModel() {
        var self = this;
        self.recordingdirectories = ko.observableArray([]);

        var refreshRecordingDirectories = function () {
            api.getJSON("recordingdirectories", null, function (allData) {
                var mapped = $.map(allData, function (item) { return new RecordingDirectory(item) });
                self.recordingdirectories(mapped);
            });
        };
        refreshRecordingDirectories();
    }

    var div = $('.user-settings > .recording-directories');
    if(div.length > 0)
        ko.applyBindings(new RecordingDirectoriesViewModel(), div.get(0));
});