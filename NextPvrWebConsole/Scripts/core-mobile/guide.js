var currentDay;
var currentChannelGroupIndex = 0;
var initialLoadDone = false;

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
    setEpgDate(new Date());
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

function refreshEpgData() {
    console.log('refreshEpgData()');
    var programList = $('#programList');
    programList.empty();
    var channelicons = $('#channelicons');
    channelicons.empty();
    $('#pageGuide #timeline').empty();
    $.mobile.showPageLoadingMsg();
    console.log('channelGroups: ' + channelGroups);
    $('#channelGroupName').text(channelGroups[currentChannelGroupIndex].Name);
    var tzBias = getTimeZoneBias();
    var startUtc = new Date(currentDay);
    startUtc.setMinutes(startUtc.getMinutes() + tzBias);
    console.log('utc date: ' + startUtc);

    loadEpgData(startUtc, channelGroups[currentChannelGroupIndex].Id, function (channels, totalMinutes) {
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
            var iconUrl = getForTheRecordUrl('GetChannelLogo') + '&ChannelName=' + escape(channel.Name) + '&Width=50&Height=40';
            console.log(iconUrl);
            //var liChannel = '<li class="channel" style="background-image:url(' + "'" + iconUrl + "'" + ')>';
            var liChannel = '<li class="channel">';
            console.log('Channel: ' + channel.Name + ' , programs: ' + channel.ProgramListings.length);
            $.each(channel.ProgramListings, function (j, pg) {
                var duration = pg.getDuration();
                var xpos = (pg.StartUtc - currentDay) / (60 * 1000) * minWidth;
                liChannel += '<span class="program" data-start="' + pg.StartUtc + '" data-end="' + pg.EndUtc + '" data-duration="' + duration + '" ' +
                                ' style="width:' + (minWidth * duration - 1) + 'px;position:absolute;left:' + xpos + 'px"><div>' + $('<div/>').text(pg.Title).html() + '</div></span>';
            });

            liChannel += '</li>';
            programList.append(liChannel);
            //channelicons.append('<img class="icon" src="' + iconUrl + '" alt="' + channel.Name + '" />');
            channelicons.append('<div class="icon"><span class="text">' + $('<div/>').text(channel.Name).html() + '</span><span class="logo" style="background: transparent url(' + iconUrl + ') no-repeat 0 0;"></span>');
        });

        $('#programList span').bind('taphold', function () {
            alert('taphold!');
        });
        $.mobile.hidePageLoadingMsg();
    });
}
