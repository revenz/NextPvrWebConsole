/// <reference path="../apihelper.js" />
/// <reference path="../global.js" />
/// <reference path="../functions.js" />

$(function () {
    if ($('#index2').length < 1) // temporary while 2 different versions of the guide
        return;

    guide.loadEpgData();
    
    function ScheduleEditorViewModel() {
        var self = this;
        self.selectedListing = ko.observable();
    };
    scheduleEditorViewModel = new ScheduleEditorViewModel();
    ko.applyBindings(scheduleEditorViewModel, $('#Guide-ScheduleEditor').get(0));
});

var guide = new function () {
    var self = this;
    var guideStart = new Date();
    guideStart.setHours(0, 0, 0, 0);

    self.minuteWidth = 5;

    var epgGridInitDone = false;
    self.initEpgGrid = function () {
        if (epgGridInitDone)
            return;
        $('#epg-groups').addClass('epg-groups').removeAttr('style');

        var pageResize = function () {
            var epgGroupsHeight = $('#epg-groups').height();
            $('#epg-groups li').css({ width: epgGroupsHeight, left: -epgGroupsHeight });
        }
        pageResize();
        $(window).resize(pageResize);

        var epgtime = $('.epg-time');
        var epgchannels = $('.epg-channels');
        var groupWidth = $('#epg-groups').width();
        var epgContainer = $('.epg-container');
        var funScrollEpgContainer = function () {
            var top = epgContainer.scrollTop();
            epgtime.css('top', top);
            var left = epgContainer.scrollLeft();
            epgchannels.css('left', left + groupWidth);
        };

        $('.epg-container').scroll(funScrollEpgContainer);
        funScrollEpgContainer(); // call it to set it up.

        // set scroll pos.
        $('.epg-container').scrollLeft(guide.getMinutesFromStartOfGuide(new Date()) * guide.minuteWidth - (30 * guide.minuteWidth));

        epgGridInitDone = true;
    };

    self.getMinutesFromStartOfGuide = function (time) {
        var diff = time - guideStart;
        var minutes = Math.floor((diff / 1000) / 60);
        return minutes;
    };

    self.epgChangeDay = function (sender) {
        $('.epg-days .selected').removeClass('selected');
        $(sender).parent().addClass('selected');
        guide.loadEpgData();
    };

    self.loadEpgData = function () {
        var date = $('.epg-days .selected').attr('data-date');
        if (!date)
            return;
        var group = $('#epg-groups .selected').attr('data-name');
        console.log('group: ' + group);
        gui.doWork();
        $('.epg-container .epg').load('/guide/epg?date=' + date + '&group=' + encodeURIComponent(group), function () {
            guide.initEpgGrid();

            gui.finishWork();
        });
        var actualDate = new Date(date);
        var today = new Date();
        var isToday = actualDate.getMonth() == today.getMonth() && actualDate.getDate() == today.getDate();
        $('#epg-time-indicator').css({ display: isToday ? 'block' : 'none' });
    };

    self.nextChannelGroup = function () {
        $('#epg-groups .selected').removeClass('selected').next().addClass('selected');
        if ($('#epg-groups .selected').length == 0)
            $('#epg-groups li:first-child').addClass('selected');
        self.loadEpgData();
    };
    self.previousChannelGroup = function () {
        $('#epg-groups .selected').removeClass('selected').prev().addClass('selected');
        if ($('#epg-groups .selected').length == 0)
            $('#epg-groups li:last-child').addClass('selected');
        self.loadEpgData();
    };

    self.openListing = function (sender) {
        var oid = $(sender).attr('data-oid');
        console.log('oid: ' + oid);

        api.getJSON('guide/epglisting/' + oid, null, function (result) {
            var showInfo = $('#show-info');
            console.log(result);
            console.log(result.HasChannelIcon);

            var channelIconUrl = $('#epg-channel-' + result.ChannelOid + ' img').attr('src');

            showInfo.find('.channelIcon').css('visible', channelIconUrl.length > 0).attr('src', channelIconUrl);
            showInfo.find('.channelnumber').text(result.ChannelNumber);
            showInfo.find('.channelname').text(result.ChannelName);
            showInfo.find('.subtitle').text(result.Subtitle);
            showInfo.find('.description').text(result.Description);
            var genres = '';
            if (result.Genres) {
                genres = $.Enumerable.From(result.Genres).Select(function (x) {
                    x = x.replace('/', ', ');
                    x = x.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1); });
                    return x.replace(' , ', ', ');
                }).ToString(', ');
            }
            showInfo.find('.genres').text(genres);
            var duration = Math.floor((Math.abs(Date.parse(result.EndTime) - Date.parse(result.StartTime)) / 1000) / 60);
            showInfo.find('.time').text($.i18n._('Time-Range-WithDuration', [gui.formatDateLong(Date.parse(result.StartTime)), gui.formatTime(Date.parse(result.EndTime)), duration]));

            var dialog_buttons = {};

            if (result.IsRecording) {
                dialog_buttons[$.i18n._("Cancel")] = function () {
                    gui.confirmMessage({
                        message: $.i18n._("Are you sure you want to cancel the recording '%s'?", [result.Title]),
                        yes: function () {
                            api.deleteJSON('recordings/' + result.RecordingOid, null, function (data) {
                                if (data)
                                    listing.isRecording(false);
                                showInfo.dialog('close');
                            });
                        }
                    });
                };
            } else {
                dialog_buttons[$.i18n._("Quick Record")] = function () {
                    api.postJSON('guide/quickrecord?oid=' + oid, null, function (data) {
                        if (data)
                            listing.isRecording(true);
                    });
                    showInfo.dialog('close');
                };
                dialog_buttons[$.i18n._('Record')] = function () {
                    console.log('not done yet');
                    showRecordingOptions(listing);
                    showInfo.dialog('close');
                };
            }
            dialog_buttons[$.i18n._('Find All')] = function () {
                showInfo.dialog('close');
            };
            dialog_buttons[$.i18n._('Close')] = function () {
                showInfo.dialog('close');
            };
            showInfo.dialog({
                modal: true,
                title: result.Title,
                minWidth: 600,
                buttons: dialog_buttons
            });
        });
    };
}