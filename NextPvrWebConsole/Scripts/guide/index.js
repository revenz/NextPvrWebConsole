/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery.linq.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery.signalR-0.5.3.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../modernizr-2.6.2.js" />
/// <reference path="../api-wrappers/listing.js" />
/// <reference path="../api-wrappers/channel.js" />

var guideStart;
var minuteWidth = 5;
var guideData = null;

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
var epgGridInitDone = false;
var epgScroller = null;
function initEpgGrid() {
    // called once page is loaded

    epgScroller = $(".epg-container").niceScroll();

    $('#epg-groups').addClass('epg-groups');

    var epgtime = $('.epg-time');
    var epgchannels = $('.epg-channels');
    var groupWidth = $('#epg-groups').width();
    $('.epg-container').scroll(function () {
        var top = $(this).scrollTop();
        epgtime.css('top', top);
        var left = $(this).scrollLeft();
        epgchannels.css('left', left + groupWidth);
    });


    var pageResize = function () {
        var epgGroupsHeight = $('#epg-groups').height();
        $('#epg-groups li').css({ width: epgGroupsHeight, left: -epgGroupsHeight });
    }
    pageResize();
    $(window).resize(pageResize);

    epgGridInitDone = true;
}

$(function () {

    guideStart = new Date(new Date().setHours(0, 0, 0, 0));
    setInterval(updateTimeIndicator, 15 * 1000);

    // set scroll pos.
    $('.epg-container').scrollLeft(getMinutesFromStartOfGuide(new Date()) * minuteWidth);

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
            return _class;
        }
        self.channelClass = function (channel, _class) {
            if (_class) _class += ' ';
            else _class = '';
            if (channel.hasIcon())
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
            days.push(new epgDate(tDate, i == 0, $.i18n._(daysOfWeekString[currentDayOfWeek]) + " (" + tDate.getDate() + '/' + (tDate.getMonth() + 1) + ')'));
            if (++currentDayOfWeek >= 7)
                currentDayOfWeek = 0;
        }
        self.epgdays = ko.observableArray(days);

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

        // Operations
        self.changeEpgDay = function (day) {
            $.each(self.epgdays(), function (i, ele) { ele.selected(false); });
            day.selected(true);
            self.loadEpgData(day.date());
        }

        // Load initial state from server, convert it to Task instances, then populate self.tasks
        self.loadEpgData(guideStart);

        self.selectedshow = ko.observable();

        self.openListing = function (listing) {
            self.selectedshow(listing);

            var dialog_buttons = {};
            dialog_buttons[$.i18n._("Quick Record")] = function () {
                api.postJSON('guide/quickrecord?oid=' + listing.oid(), null, function (data) {
                    console.log('data...');
                    console.log(data);
                });
                $('#show-info').dialog('close');
            };
            dialog_buttons[$.i18n._('Record')] = function () {
                showRecordingOptions(listing);
                $('#show-info').dialog('close');
            };
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

    var viewModel = new GuideViewModel();
    ko.applyBindings(viewModel);
    $('.epg-days li:eq(0)').addClass('selected');


    $('.epg-groups-button.next').click(function () {
        $('#epg-groups .selected').removeClass('selected').next().addClass('selected');
        if ($('#epg-groups .selected').length == 0)
            $('#epg-groups li:first-child').addClass('selected');
        viewModel.loadEpgData(guideStart);
    });
    $('.epg-groups-button.previous').click(function () {
        $('#epg-groups .selected').removeClass('selected').prev().addClass('selected');
        if ($('#epg-groups .selected').length == 0)
            $('#epg-groups li:last-child').addClass('selected');
        viewModel.loadEpgData(guideStart);
    });

    function showRecordingOptions(listing) {
        var dialog_buttons = {};
        dialog_buttons[$.i18n._("OK")] = function () {
            var type = $('#recording-type').val();
            var prepadding = $('#recording-prepadding').val();
            var postpadding = $('#recording-postpadding').val();
            var directory = $('#recording-directory').val();
            var keep = $('#recording-keep').val();

            api.postJSON('guide/record', { oid: listing.oid(), prepadding: prepadding, postpadding: postpadding, recordingdirectoryid: directory, numbertokeep: keep, type: type }, function (result) {
                console.log(result);
            });

            $('#recording-options').dialog('close');
        };
        dialog_buttons[$.i18n._("Cancel")] = function () {
            $('#recording-options').dialog('close');
        }

        $('#recording-options').dialog({
            modal: true,
            title: listing.title(),
            width: 600,
            height: 300,
            buttons: dialog_buttons
        });
    }

    function epgDate(date, selected, displayText) {
        var self = this;
        self.displayText = ko.observable(displayText);
        self.selected = ko.observable(selected);
        self.date = ko.observable(date);
        self.cssClass = ko.computed(function () {
            return 'epg-day ' + (self.selected() ? 'selected' : '');
        });
    }
});