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
    console.log('test');
    var timeline = $('#pageGuide #timeline');
    var channelicons = $('#pageGuide #channelicons');

    var scrollNode = document.querySelector("#programs");
    var options = {
        onMove: function (x, y) {
            timeline.css('-webkit-transform', 'translate(' + x + 'px, 0px)');
            channelicons.css('-webkit-transform', 'translate(0px, ' + y + 'px)');
        }
    };
    var scroller = new TouchScroll(scrollNode, options);
});

$('#guidePrevDay').live('click', function () {
    if (!currentDay)
        return;
    var date = new Date();
    date.setHours(0, 0, 0, 0);
    if (currentDay <= date)
        return;
    date.setDate(currentDay.getDate() - 1);
    setEpgDate(date);
});

$('#guideNextDay').live('click', function () {
    if (!currentDay)
        return;
    var date = new Date();
    date.setHours(0, 0, 0, 0);
    date.setDate(currentDay.getDate() + 1);
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


function initialLoadEpgPage() {
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    refreshChannelGroups(function () { setEpgDate(today); });    
    initialLoadDone = true;
}

function setEpgDate(newDate) {
    currentDay = newDate
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    var tomorrow = new Date();
    tomorrow.setHours(0, 0, 0, 0);
    tomorrow.setDate(today.getDate() + 1);
    var twodaystime = new Date();
    twodaystime.setHours(0, 0, 0, 0);
    twodaystime.setDate(today.getDate() + 2);

    var text = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"][currentDay.getDate()] + ", " +
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

    var programList = $('#programList');
    programList.empty();
    var channelicons = $('#channelicons');
    channelicons.empty();
    $('#pageGuide #timeline').empty();
    $.mobile.showPageLoadingMsg();

    console.log(channelGroups);
    console.log('currentChannelGroupIndex: ' + currentChannelGroupIndex);
    $('#channelGroupName').text(channelGroups[currentChannelGroupIndex].Name);

    var id = channelGroups[currentChannelGroupIndex].Id;
    if(id == null)
        id = channelGroups[currentChannelGroupIndex].Name;
    loadEpgData(currentDay, id, function (channels, totalMinutes) {
        $('#pageGuide #epg').html(channels);
        /*
        var timeline = '';
        var minWidth = 3;
        programList.width(totalMinutes * minWidth + 'px');
        for (var i = 0; i < 48; i++) {
            timeline += '<span class="bucket 30min" style="width:' + (30 * minWidth) + 'px">&nbsp;&nbsp;';
            timeline += Math.floor(i / 2) + ':';
            if (i % 2 == 1)
                timeline += '30';
            else
                timeline += '00';
            timeline += '</span>';
        }
        $('#pageGuide #timeline').append('<span class="spacer">&nbsp;</span>' + timeline);
        $.each(channels, function (i, channel) {
            var id = 'channel' + channel.Name.split(' ').join('_');
            var iconUrl = ''; // getForTheRecordUrl('GetChannelLogo') + '&ChannelName=' + escape(channel.Name) + '&Width=50&Height=40';
            //console.log(iconUrl);
            //var liChannel = '<li class="channel" style="background-image:url(' + "'" + iconUrl + "'" + ')>';
            var liChannel = '<li class="channel">';
            $.each(channel.Listings, function (j, pg) {
                console.log(pg);
                var diffMs = new Date(pg.EndTime) - new Date(pg.StartTime);
                var duration = Math.floor((diffMs / 1000) / 60)
                console.log(duration + ', start: ' + pg.StartTime + ', end: ' + pg.EndTime + ', ' + currentDay);
                //var duration = pg.getDuration();
                var xpos = (new Date(pg.StartTime) - currentDay) / (60 * 1000) * minWidth;
                liChannel += '<span class="program" data-start="' + pg.StartTime + '" data-end="' + pg.EndTime + '" data-duration="' + duration + '" ' +
                                ' style="width:' + (minWidth * duration - 1) + 'px;position:absolute;left:' + xpos + 'px"><div>' + $('<div/>').text(pg.Title).html() + '</div></span>';
            });

            liChannel += '</li>';
            programList.append(liChannel);
            //channelicons.append('<img class="icon" src="' + iconUrl + '" alt="' + channel.Name + '" />');
            channelicons.append('<div class="icon"><span class="text">' + $('<div/>').text(channel.Name).html() + '</span><span class="logo" style="background: transparent url(' + iconUrl + ') no-repeat 0 0;"></span>');
        });
        */

        $('#programList span').bind('taphold', function () {
            alert('taphold!');
        });
        $.mobile.hidePageLoadingMsg();
    });
}

function loadEpgData(startUtc, groupName, callback) {
    console.log('loading epg data');

    $.get('/guide/epg?date=' + $.format.date(startUtc, 'yyyy-MM-dd') + '&group=' + encodeURIComponent(groupName) + '&rand=' + Math.random(), null, function (data, textStatus, jqXHR) {
        if (callback)
            callback(data, 24 * 60);
    });
}