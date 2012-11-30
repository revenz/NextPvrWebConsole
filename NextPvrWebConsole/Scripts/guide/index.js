/// <reference path="../apihelper.js" />
/// <reference path="../global.js" />
/// <reference path="../functions.js" />

$(function () {
    guide.loadEpgData();
    
    function ScheduleEditorViewModel() {
        var self = this;
        self.selectedListing = ko.observable();
    };
    scheduleEditorViewModel = new ScheduleEditorViewModel();
    ko.applyBindings(scheduleEditorViewModel, $('#Guide-ScheduleEditor').get(0));

    setInterval(guide.updateTimeIndicator, 15 * 1000);
});

var guide = new function () {
    var self = this;
    self.guideStart = new Date();
    self.guideStart.setHours(0, 0, 0, 0);

    self.minuteWidth = 5;

    var epgGridInitDone = false;
    self.initEpgGrid = function () {
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
        if (!epgGridInitDone)
            $('.epg-container').scrollLeft(guide.getMinutesFromStartOfGuide(new Date()) * guide.minuteWidth - (30 * guide.minuteWidth));

        epgGridInitDone = true;
    };

    self.getMinutesFromStartOfGuide = function (time) {
        var diff = time - self.guideStart;
        var minutes = Math.floor((diff / 1000) / 60);
        return minutes;
    };

    self.epgChangeDay = function (sender) {
        $('.epg-days .selected').removeClass('selected');
        $(sender).parent().addClass('selected');
        guide.loadEpgData();
    };

    var epgLoadTarget = document.getElementById('epg-load-target');
    self.loadEpgData = function () {
        var date = $('.epg-days .selected').attr('data-date');
        if (!date)
            return;
        var group = $('#epg-groups .selected').attr('data-name');
        gui.doWork();
        var actualDate = new Date(date);
        var today = new Date();
        var isToday = actualDate.getMonth() == today.getMonth() && actualDate.getDate() == today.getDate();

        // add random to stop IE from caching the guide data
        $.get('/guide/epg?date=' + date + '&group=' + encodeURIComponent(group) + '&rand=' + Math.random(), null, function (data, textStatus, jqXHR) {
            epgLoadTarget.innerHTML = data;
            guide.initEpgGrid();
            $('#epg-time-indicator').css({ display: isToday ? 'block' : 'none' });
            if (isToday)
                self.updateTimeIndicator();
            gui.finishWork();
        });
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

    self.updateTimeIndicator = function () {
        var date = new Date();
        var mins = (date.getHours() * 60) + date.getMinutes();
        var guideMins = (self.guideStart.getHours() * 60) + self.guideStart.getMinutes();
        mins = mins - guideMins;
        var sameday = self.guideStart.getDate() == date.getDate() && self.guideStart.getMonth() == date.getMonth();
        $('#epg-time-indicator').css({ left: mins * self.minuteWidth + 'px', height: $('.epg-listings-channel').height(), display: sameday ? '' : 'none' });
    };

    self.openListing = function (sender) {
        var $sender = $(sender);
        var oid = $sender.attr('data-oid');
        console.log('oid: ' + oid);

        api.getJSON('guide/epglisting/' + oid, null, function (result) {
            var showInfo = $('#show-info');
            console.log(result);

            var channelIconUrl = $('#epg-channel-' + result.ChannelOid + ' img').attr('src');
            
            showInfo.find('.channelIcon').css('visibility', channelIconUrl && channelIconUrl.length > 0 ? 'visible' : 'hidden').attr('src', channelIconUrl);
            showInfo.find('.channelnumber').text(result.ChannelNumber);
            showInfo.find('.channelname').text(result.ChannelName);
            showInfo.find('.subtitle').text(result.Subtitle);
            showInfo.find('.description').text(result.Description);
            showInfo.find('.repeat').text(result.FirstRun ? '' : $.i18n._('[REPEAT]'));
            var seasonAndEpisode = '';
            if (result.Season > 0 && result.Episode > 0)
                seasonAndEpisode = $.i18n._('Season %s, Episode %s', [result.Season, result.Episode]);
            showInfo.find('.seasonAndEpisode').text(seasonAndEpisode);

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
                if (result.IsRecurring) {
                    dialog_buttons[$.i18n._("Cancel Series")] = function () {
                        gui.confirmMessage({
                            message: $.i18n._("Are you sure you want to cancel the series '%s'?", [result.Title]),
                            yes: function () {
                                api.deleteJSON('recordings/deleterecurring/' + result.RecurrenceOid, null, function (data) {
                                    if (data) {
                                        $sender.removeClass('recording');
                                        $sender.attr('data-recordingoid', '');
                                        $sender.attr('data-isrecurring', '0');
                                    }
                                    showInfo.dialog('close');
                                });
                            }
                        });
                    };
                }

                dialog_buttons[$.i18n._("Cancel Recording")] = function () {
                    gui.confirmMessage({
                        message: $.i18n._("Are you sure you want to cancel the recording '%s'?", [result.Title]),
                        yes: function () {
                            api.deleteJSON('recordings/' + result.RecordingOid, null, function (data) {
                                if (data) {
                                    $sender.removeClass('recording');
                                    $sender.attr('data-recordingoid', '');
                                    $sender.attr('data-isrecurring', '0');
                                }
                                showInfo.dialog('close');
                            });
                        }
                    });
                };
            } else {
                dialog_buttons[$.i18n._("Quick Record")] = function () {
                    api.postJSON('guide/quickrecord?oid=' + oid, null, function (data) {
                        console.log(data);
                        if (data) {
                            $sender.attr('data-recordingoid', data.oid);
                            $sender.addClass('recording');
                        }
                    });
                    showInfo.dialog('close');
                };
                dialog_buttons[$.i18n._('Record')] = function () {
                    guide.showRecordingOptions(new Listing(null, result), function (temp) {
                        $sender.attr('data-recordingoid', temp.recordingOid);
                        $sender.attr('data-isrecurring', temp.recurringOid > 0 ? '1' : '0');
                        $sender.addClass('recording');
                    });
                    showInfo.dialog('close');
                };
            }
            dialog_buttons[$.i18n._('Find All')] = function () {
                alert('Not implemented yet.');
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

    self.showRecordingOptions = function (listing, successCallback) {
        listing.type = ko.computed({
            read: function () { return listing.recordingType(); },
            write: function (value) { listing.recordingType(value); }
        });
        scheduleEditorViewModel.selectedListing(listing);

        var dialog = $('#recording-options');
        var dialog_buttons = {};
        dialog_buttons[$.i18n._("OK")] = function () {
            api.postJSON('guide/record',
                {
                    oid: listing.oid(),
                    prePadding: listing.prePadding(),
                    postPadding: listing.postPadding(),
                    recordingdirectoryid: listing.recordingDirectoryId(),
                    numbertokeep: listing.keep(),
                    type: listing.type()
                }, function (result) {
                    console.log(result);
                    if (result && successCallback)
                        successCallback({ recurrenceOid: result.recurrenceOID, recordingOid: result.OID });
                });

            dialog.dialog('close');
        };
        dialog_buttons[$.i18n._("Cancel")] = function () {
            dialog.dialog('close');
        }
        dialog.dialog({
            modal: true,
            title: listing.title(),
            minWidth: 650,
            minHeight: 350,
            buttons: dialog_buttons
        });
    }

    self.liveStream = function (oid) {
        window.open('/stream/' + oid, 'livestream', 'width=830,height=480,status=1,resizable=0');
    };
}