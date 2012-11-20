/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery.linq.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery.signalR-0.5.3.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../modernizr-2.6.2.js" />
/// <reference path="../api-wrappers/listing.js" />
/// <reference path="../api-wrappers/channel.js" />

$(function () {
    function GuideViewModel() {
        // Data
        var self = this;

        self.listingCss = function (listing) {
            var css = '';
            var dStart = Date.parse(listing.startTime());
            var dEnd = Date.parse(listing.endTime());

            var guideEnd = new Date(guideStart.getTime());
            guideEnd.setDate(guideEnd.getDate() + 1);

            if (dEnd > guideEnd) dEnd = guideEnd; // ends before end of day

            var start = getMinutesFromStartOfGuide(dStart);
            var end = getMinutesFromStartOfGuide(dEnd);

            if (start < 0) start = 0; // starts the day before

            css += 'left: ' + ((minuteWidth * start)) + 'px;width:' + ((minuteWidth * (end - start)) - 1) + 'px';
            return css;
        }
        self.listingClass = function (listing, _class) {
            if (_class) _class += ' ';
            else _class = '';

            var end = new Date(guideStart.getTime());
            end.setDate(end.getDate() + 1);
            if (Date.parse(listing.startTime()) < guideStart)
                _class += 'pre-guide-start ';
            if (Date.parse(listing.endTime()) > end)
                _class += 'post-guide-end ';
            if (listing.isRecording())
                _class += 'recording ';
            return _class;
        }
        self.channelClass = function (channel, _class) {
            if (_class) _class += ' ';
            else _class = '';
            if (channel.hasIcon())
                _class += 'logo-available ';
            return _class;
        }

        self.channels = ko.observableArray([]);
        self.loadEpgData = function (date) {
            guideStart = date;
            self.channels.removeAll();
            $('#epg-time-indicator').css({ display: 'none' });
            api.getJSON('guide?group=' + $('#epg-groups .selected').text() + '&date=' + date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate(), null, function (allData) {
                var mapped = $.map(allData, function (item) { return new Channel(item) });
                self.channels(mapped);
                guideData = allData;
                updateTimeIndicator();
                if (!epgGridInitDone) {
                    initEpgGrid();
                }
            });
        }
        // Load initial state from server, convert it to Task instances, then populate self.tasks
        self.loadEpgData(guideStart);

        self.liveStream = function (channel) {
            window.open('/streaming/' + channel.oid(), 'livestream', 'width=830,height=480,status=1,resizable=0');
        };

        self.selectedshow = ko.observable();

        self.openListing = function (listing) {
            self.selectedshow(listing);
            var dialog_buttons = {};

            if (listing.isRecording()) {
                dialog_buttons[$.i18n._("Cancel")] = function () {
                    gui.confirmMessage({
                        message: $.i18n._("Are you sure you want to cancel the recording '%s'?", [listing.title()]),
                        yes: function () {
                            api.deleteJSON('recordings/' + listing.recordingOid(), null, function (data) {
                                if (data)
                                    listing.isRecording(false);
                                $('#show-info').dialog('close');
                            });
                        }
                    });
                };
            } else {
                dialog_buttons[$.i18n._("Quick Record")] = function () {
                    api.postJSON('guide/quickrecord?oid=' + listing.oid(), null, function (data) {
                        if (data)
                            listing.isRecording(true);
                    });
                    $('#show-info').dialog('close');
                };
                dialog_buttons[$.i18n._('Record')] = function () {
                    showRecordingOptions(listing);
                    $('#show-info').dialog('close');
                };
            }
            dialog_buttons[$.i18n._('Find All')] = function () {
                $('#show-info').dialog('close');
            };
            dialog_buttons[$.i18n._('Close')] = function () {
                $('#show-info').dialog('close');
            };
            $('#show-info').dialog({
                modal: true,
                title: listing.title(),
                minWidth: 600,
                beforeClose: function (event, ui) {
                    self.selectedshow(null);
                },
                buttons: dialog_buttons
            });
        };
    }

    guideViewModel = new GuideViewModel();
    ko.applyBindings(guideViewModel, $('#epg-viewmodel-container .epg-container').get(0));
});