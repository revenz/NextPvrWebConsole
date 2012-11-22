/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery.linq.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery.signalR-0.5.3.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../modernizr-2.6.2.js" />
/// <reference path="../api-wrappers/listing.js" />
/// <reference path="../api-wrappers/channel.js" />

var guideStart = new Date(new Date().setHours(0, 0, 0, 0));
var minuteWidth = 5;
var guideData = null;
var epgGridInitDone = false;
var epgScroller = null;
var guideViewModel = null;
var scheduleEditorViewModel = null;

function getMinutesFromStartOfGuide(time) {
    var diff = time - guideStart;
    var minutes = Math.floor((diff / 1000) / 60);
    return minutes;
}

function initEpgGrid() {
    // called once page is loaded
    //epgScroller = $(".epg-container").niceScroll();

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

    epgGridInitDone = true;
}

function showRecordingOptions(listing) {
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
                if (result)
                    listing.isRecording(true);
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

function epgDate(date, selected, displayText) {
    var self = this;
    self.displayText = ko.observable(displayText);
    self.selected = ko.observable(selected);
    self.date = ko.observable(date);
    self.cssClass = ko.computed(function () {
        return 'epg-day ' + (self.selected() ? 'selected' : '');
    });
}


$(function () {

    // set scroll pos.
    $('.epg-container').scrollLeft(getMinutesFromStartOfGuide(new Date()) * minuteWidth);
        
    $('.epg-groups-button.next').click(function () {
        $('#epg-groups .selected').removeClass('selected').next().addClass('selected');
        if ($('#epg-groups .selected').length == 0)
            $('#epg-groups li:first-child').addClass('selected');
        guideViewModel.loadEpgData(guideStart);
    });
    $('.epg-groups-button.previous').click(function () {
        $('#epg-groups .selected').removeClass('selected').prev().addClass('selected');
        if ($('#epg-groups .selected').length == 0)
            $('#epg-groups li:last-child').addClass('selected');
        guideViewModel.loadEpgData(guideStart);
    });

    function ScheduleEditorViewModel() {
        var self = this;
        self.selectedListing = ko.observable();
    };
    scheduleEditorViewModel = new ScheduleEditorViewModel();
    ko.applyBindings(scheduleEditorViewModel, $('#Guide-ScheduleEditor').get(0));
});