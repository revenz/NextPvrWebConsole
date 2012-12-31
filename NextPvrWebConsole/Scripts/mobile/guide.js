/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/knockout-2.1.0.debug.js" />
/// <reference path="../core/modernizr-2.6.2.js" />

var currentDay;
var currentChannelGroupIndex = 0;
var initialLoadDone = false;
var channelGroups = [{ Name: 'All Channels', Id: ''}];

$(function () {

    $('#guidePrevDay').live('click', function () {
        if (!currentDay)
            return;
        var date = date_by_subtracting_days(currentDay, 1);
        setEpgDate(date);
    });

    $('#guideNextDay').live('click', function () {
        if (!currentDay)
            return;
        var date = date_by_adding_days(currentDay, 1);
        setEpgDate(date);
    });

    $('#channelGroupNext').live('click', function () {
        currentChannelGroupIndex += 1;
        if (currentChannelGroupIndex >= channelGroups.length)
            currentChannelGroupIndex = 0;
        refreshEpgData();
    });
    $('#channelGroupPrev').live('click', function () {
        currentChannelGroupIndex -= 1;
        if (currentChannelGroupIndex < 0)
            currentChannelGroupIndex = channelGroups.length - 1;
        refreshEpgData();
    });

    $('#pageGuide').live('pageshow', function (event) {
        console.log('showing guide page');
        if (!initialLoadDone) {
            $.mobile.showPageLoadingMsg();
            initialLoadEpgPage();
        }
    });

    $('#btnShowInfoBack').click(function () {
        epgShowProgram(0);
    });
});

function initialLoadEpgPage() {
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    refreshChannelGroups(function () { setEpgDate(today); });    
    initialLoadDone = true;
}

function setEpgDate(newDate) {
    console.log('setEpgDate');
    currentDay = newDate
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    var tomorrow = new Date();
    tomorrow.setHours(0, 0, 0, 0);
    tomorrow.setDate(today.getDate() + 1);
    var twodaystime = new Date();
    twodaystime.setHours(0, 0, 0, 0);
    twodaystime.setDate(today.getDate() + 2);

    var text = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"][currentDay.getDay()] + ", " +
               ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"][currentDay.getMonth()] + " " + currentDay.getDate();

    if (currentDay >= today && currentDay < tomorrow)
        text = 'Today';
    else if (currentDay >= tomorrow && currentDay < twodaystime)
        text = 'Tomorrow';
    $('#epgDate').html(text);
    refreshEpgData();
}

function refreshChannelGroups(callback) {
    api.getJSON('channelgroups', null, function (data) {
        channelGroups = [{ Name: 'All Channels', Id: ''}];
        console.log(data);
        for (var i = 0; i < data.length; i++)
            channelGroups.push(data[i]);
        if (callback)
            callback();
    });
}

function refreshEpgData() {
    console.log('refreshEpgData()');

    $.mobile.showPageLoadingMsg();

    console.log(channelGroups);
    console.log('currentChannelGroupIndex: ' + currentChannelGroupIndex);
    $('#channelGroupName').text(channelGroups[currentChannelGroupIndex].Name);

    var id = channelGroups[currentChannelGroupIndex].Id;
    if(id == null)
        id = channelGroups[currentChannelGroupIndex].Name;

    var iframe = true;

    if (iframe) {
        $('#epg iframe').attr('src', '/guide/epg?date=' + $.format.date(currentDay, 'yyyy-MM-dd') + '&group=' + encodeURIComponent(id) + '&rand=' + Math.random());
        $.mobile.hidePageLoadingMsg();
    } else {

        var epg = $('#epg');
        loadEpgData(currentDay, id, function (channels, totalMinutes) {
            epg.css('visibility', 'hidden');
            epg.get(0).innerHTML = channels;

            epg.niceScroll();


            var timeline = $('#timeline');
            var channelicons = $('#channelicons');
            var scroller = epg.getNiceScroll()[0];
            scroller.scrollstart(function (info) {
                channelicons.css('left', (info.end.x) + 'px');
                timeline.css('top', (info.end.y) + 'px');
            });
            scroller.scrollend(function (info) {
                channelicons.css('left', (info.current.x) + 'px');
                timeline.css('top', (info.current.y) + 'px');
            });
            scroller.scrollcancel(function (info) {
                channelicons.css('left', (info.current.x) + 'px');
                timeline.css('top', (info.current.y) + 'px');
            });
            scroller.scroll(function (info) {
                console.log('scroll');
                console.log(info);
                if (info.end.x > info.current.x)
                    channelicons.css('left', (info.current.x) + 'px');
                else
                    channelicons.css('left', (info.end.x) + 'px');
                if (info.end.y > info.current.y)
                    timeline.css('top', (info.current.y) + 'px');
                else
                    timeline.css('top', (info.end.y) + 'px');
            });

            //$('#epg').html(channels);

            //$('#programList span').bind('taphold', function () {
            //    console.log('taphold!');
            //});
            epg.css('visibility', 'visible');
            $.mobile.hidePageLoadingMsg();
        });
    }
}

function loadEpgData(startUtc, groupName, callback) {
    console.log('loading epg data: ');

    $.get('/guide/epg?date=' + $.format.date(startUtc, 'yyyy-MM-dd') + '&group=' + encodeURIComponent(groupName) + '&rand=' + Math.random(), null, function (data, textStatus, jqXHR) {
        if (callback)
            callback(data, 24 * 60);
    });
}

function date_by_subtracting_days(date, days) {
    return new Date(
        date.getFullYear(),
        date.getMonth(),
        date.getDate() - days,
        date.getHours(),
        date.getMinutes(),
        date.getSeconds(),
        date.getMilliseconds()
    );
}

function date_by_adding_days(date, days) {
    return new Date(
        date.getFullYear(),
        date.getMonth(),
        date.getDate() + days,
        date.getHours(),
        date.getMinutes(),
        date.getSeconds(),
        date.getMilliseconds()
    );
}

var currentProgramOid = 0;
var viewModel = null;

function epgShowProgram(oid) {
    console.log('epgShowProgram: ' + oid);
    if (currentProgramOid === oid)
        return;
    console.log('about to select show: ' + oid);
    currentProgramOid = oid;
    viewModel.selectShow(oid);
}


function MobileEpgViewModel() {
    // Data
    var self = this;
    self.selectedShow = ko.observable();

    self.selectShow = function (oid) {

        if (oid)
            location.hash = 'guide-' + oid;
        else
            location.hash = 'guide';

        console.log('selecting show: ' + oid);
        if (oid == 0) {
            $('#showdetails').hide();
            $('#showinfo').hide();
            $('#epgcontainer').show();
            self.selectedShow(null);
            return;
        } else {
            $('#epgcontainer').hide();
            $('#showinfo').show();
        }
        api.getJSON('guide/epglisting/' + oid, null, function (result) {
            $('#showdetails').show();
            console.log(result);
            self.selectedShow(new Listing(null, result));

            $('#masterpage').trigger("pagecreate");
        });
    };
}

$(function () {
    viewModel = new MobileEpgViewModel();
    console.log('showinfo...');
    console.log($('#showinfo').get(0));
    ko.applyBindings(viewModel, $('#showinfo').get(0));

    if (location.hash && location.hash.indexOf('-') > 0) {
        var hash = location.hash.substr(location.hash.indexOf('-') + 1);
        console.log('guide hash: ' + hash);
        epgShowProgram(hash);
    }

    $('#showinfo').on('change', '#Recording_Type', function () {
        console.log('change');
        var value = $(this).val();
        $('#showinfo .advanced').css('display', value == 0 ? 'none' : 'block');
    });

    $('#showinfo').on('click', '#btnRecord', function () {
        var parameters = [];
        $.each($('#showinfo [id^=Recording_]'), function (i, ele) {
            parameters[$(ele).attr('name')] = $(ele).val();
        });
        parameters = $.extend({}, parameters);
        ajax.postJSON('guide/record', parameters, function (result) {
            if (result && result.success) {
                viewModel.selectedShow().isRecording(result.result.OID > 0);
                viewModel.selectedShow().isRecurring(result.result.RecurrenceOID > 0);
                viewModel.selectedShow().prePadding(result.result.PrePadding);
                viewModel.selectedShow().postPadding(result.result.PostPadding);
            }
        });
    });

    $('#showinfo').on('click', '.cancel-recording', function () {
        api.deleteJSON('recordings/' + viewModel.selectedShow().recordingOid(), null, function (data) {
            if (data) {
                viewModel.selectedShow().isRecording(false);                
            }
        });
    });

    $('#showinfo').on('click', '.cancel-recurring', function () {
        api.deleteJSON('recordings/deleterecurring/' + viewModel.selectedShow().recurrenceOid(), null, function (result) {
            if (result) {
                viewModel.selectedShow().isRecording(false);
                viewModel.selectedShow().isRecurring(false);
            }
        });
    });
});