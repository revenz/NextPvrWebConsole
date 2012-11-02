/// <reference path="../jquery-ui-1.9.0.js" />
/// <reference path="../jquery-1.8.2.js" />
/// <reference path="../jquery.linq.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../jquery.signalR-0.5.3.js" />
/// <reference path="../knockout-2.2.0.js" />
/// <reference path="../modernizr-2.6.2.js" />

var guideStart;
var minuteWidth = 5;
var guideData = null;

function showInfo(apiChannel, apiShow) {
    var self = this;
    self.apiChannel = apiChannel;
    self.apiShow = apiShow;
    self.oid = ko.observable(apiShow.oid);
    self.title = ko.observable(apiShow.title);
    self.subtitle = ko.observable(apiShow.subtitle);
    self.description = ko.observable(apiShow.description);
    self.channelName = ko.observable(apiChannel.Name);
    self.channelNumber = ko.observable(apiChannel.Number);
    self.startTimeLong = ko.computed(function () { return gui.formatDate(Date.parse(apiShow.startTime)); });
    self.endTimeShort = ko.computed(function () { return gui.formatTime(Date.parse(apiShow.endTime)); });
    self.duration = ko.computed(function () { return Math.floor((Math.abs(Date.parse(apiShow.endTime) - Date.parse(apiShow.startTime)) / 1000) / 60) + ' minutes' });
    self.genresString = ko.computed(function () {
        if (apiShow.genres)
            return $.Enumerable.From(apiShow.genres).Select(function (x) {
                x = x.replace('/', ', ');
                x = x.replace(/\w\S*/g, function (txt) { return txt.charAt(0).toUpperCase() + txt.substr(1); });
                return x.replace(' , ', ', ');
            }).ToString(', ');
        return '';
    });
    self.channelLogoVisible = ko.computed(function () { return apiChannel.Icon && apiChannel.Icon.length > 0; });
    self.channelLogoData = ko.computed(function () {
        if (apiChannel.Icon && apiChannel.Icon.length > 0)
            return 'data:image/png;base64,' + apiChannel.Icon;
        return '';
    });
}

function getMinutesFromStartOfGuide(time) {
    var diff = time - guideStart;
    var minutes = Math.floor((diff / 1000) / 60);
    return minutes;
}

function updateTimeIndicator() {
    var date = new Date();
    var mins = (date.getHours() * 60) + date.getMinutes();
    var guideMins = (guideStart.getHours() * 60) + guideStart.getMinutes();
    mins = mins - guideMins;
    var sameday = guideStart.getDate() == date.getDate() && guideStart.getMonth() == date.getMonth();
    $('#epg-time-indicator').css({ left: mins * minuteWidth + 'px', height: $('.epg-listings-channel').height(), display: sameday ? '' : 'none' });
}

$(function () {
    guideStart = new Date(new Date().setHours(0, 0, 0, 0));
    setInterval(updateTimeIndicator, 15 * 1000);

    var epgtime = $('.epg-time');
    var epgchannels = $('.epg-channels');
    $('.epg-container').scroll(function () {
        var top = $(this).scrollTop();
        epgtime.css('top', top);
        var left = $(this).scrollLeft();
        epgchannels.css('left', left);
    });

    // set scroll pos.
    $('.epg-container').scrollLeft(getMinutesFromStartOfGuide(new Date()) * minuteWidth);

    function Channel(data) {
        this.name = ko.observable(data.Name);
        this.number = ko.observable(data.Number);
        this.icon = ko.computed(function () {
            if (data.Icon && data.Icon.length > 0)
                return 'data:image/png;base64,' + data.Icon;
            return '';
        });
        this.iconVisible = (data.Icon && data.Icon.length > 0);
        this.oid = ko.observable(data.OID);
        this.listings = ko.observable(data.Listings);
    }

    function GuideViewModel() {
        // Data
        var self = this;

        self.listingCss = function (listing) {
            var css = '';
            var dStart = Date.parse(listing.startTime);
            var dEnd = Date.parse(listing.endTime);

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
            if (Date.parse(listing.startTime) < guideStart)
                _class += 'pre-guide-start ';
            if (Date.parse(listing.endTime) > end)
                _class += 'post-guide-end ';
            return _class;
        }
        self.channelClass = function (channel, _class) {
            if (_class) _class += ' ';
            else _class = '';
            if (channel.iconVisible)
                _class += 'logo-available ';
            return _class;
        }
        self.formattedDate = function (date) {
            date = new Date(date);
            if (date.getMinutes() < 10)
                return date.getHours() + ':0' + date.getMinutes() + ' (' + date.getDate() + '/' + (date.getMonth() + 1) + ')';
            return date.getHours() + ':' + date.getMinutes() + ' (' + date.getDate() + '/' + (date.getMonth() + 1) + ')';
        }

        // might shift this into server side code...
        var days = new Array();
        var daysOfWeekString = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        var currentDayOfWeek = new Date().getDay();
        for (var i = 0; i < 7; i++) {
            var tDate = new Date();
            tDate.setDate(tDate.getDate() + i);
            tDate.setHours(0, 0, 0, 0);
            days.push({ name: daysOfWeekString[currentDayOfWeek], link: '#' + currentDayOfWeek, date: tDate, displayText: daysOfWeekString[currentDayOfWeek] + " (" + tDate.getDate() + '/' + (tDate.getMonth() + 1) + ')' });
            if (++currentDayOfWeek >= 7)
                currentDayOfWeek = 0;
        }
        self.epgdays = ko.observableArray(days);

        self.channels = ko.observableArray([]);
        var loadEpgData = function (date) {
            guideStart = date;
            self.channels.removeAll();
            $('#epg-time-indicator').css({ display: 'none' });
            api.getJSON('guide?date=' + date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate(), function (allData) {
                var mapped = $.map(allData, function (item) { return new Channel(item) });
                self.channels(mapped);
                guideData = allData;
                updateTimeIndicator();
            });
        }

        // Operations
        self.changeEpgDay = function (day) { loadEpgData(day.date); }

        // Load initial state from server, convert it to Task instances, then populate self.tasks
        loadEpgData(guideStart);

        self.selectedshow = ko.observable();

        $('.epg-listings').on('click.epg', '.listing', function () {
            // get listing info, helper for this.
            var showElement = $(this);
            var showInfo = getShowInfo(showElement);
            console.log('showInfo...');
            console.log(showInfo);

            self.selectedshow(showInfo);
            $('#show-info').dialog({
                modal: true,
                title: showInfo.title(),
                minWidth: 600,
                beforeClose: function (event, ui) {
                    self.selectedshow(null);
                },
                buttons: {
                    'Quick Record': function () {
                        api.postJSON('guide/quickrecord?oid=' + showInfo.oid(), null, function (data) {
                            console.log('data...');
                            console.log(data);
                        });
                        $(this).dialog('close');
                    },
                    'Record': function () {
                        showRecordingOptions(showElement, showInfo);
                        $(this).dialog('close');
                    },
                    'Find All': function () {
                        $(this).dialog('close');
                    },
                    'Close': function () {
                        $(this).dialog('close');
                    }
                }
            });
        });
    }

    ko.applyBindings(new GuideViewModel());

    function getShowInfo(liElement) {
        var channelOid = parseInt(liElement.closest('ul').attr('data-channeloid'), 10);
        var programOid = parseInt(liElement.attr('data-oid'), 10);

        var channel = $.Enumerable.From(guideData)
                                          .Where(function (x) { return x.OID == channelOid; })
                                          .FirstOrDefault();
        if (!channel)
            return null;

        var show = $.Enumerable.From(channel.Listings)
                                       .Where(function (x) { return x.oid == programOid; })
                                       .FirstOrDefault();
        if (!show)
            return null;

        return new showInfo(channel, show);
    }

    function showRecordingOptions(element, showInfo) {
        $('#recording-options').dialog({
            modal: true,
            title: 'Record: ' + showInfo.title(),
            width: 600,
            height: 300,
            buttons: {
                'OK': function () {
                    var type = $('#recording-type').val();
                    var prepadding = $('#recording-prepadding').val();
                    var postpadding = $('#recording-postpadding').val();
                    var directory = $('#recording-directory').val();
                    var keep = $('#recording-keep').val();

                    api.postJSON('guide/record', { oid: showInfo.oid(), prepadding: prepadding, postpadding: postpadding, recordingdirectoryid: directory, numbertokeep: keep, type: type }, function (result) {
                        console.log('result...');
                        console.log(result);
                    });

                    $('#recording-options').dialog('close');
                },
                'Cancel': function () { $('#recording-options').dialog('close'); }
            }
        });
    }
});