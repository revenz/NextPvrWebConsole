/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery.linq.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery.signalR-0.5.3.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../modernizr-2.6.2.js" />
/// <reference path="../api-wrappers/listing.js" />
/// <reference path="../api-wrappers/channel.js" />


function updateTimeIndicator() {
    var date = new Date();
    var mins = (date.getHours() * 60) + date.getMinutes();
    var guideMins = (guideStart.getHours() * 60) + guideStart.getMinutes();
    mins = mins - guideMins;
    var sameday = guideStart.getDate() == date.getDate() && guideStart.getMonth() == date.getMonth();
    $('#epg-time-indicator').css({ left: mins * minuteWidth + 'px', height: $('.epg-listings-channel').height(), display: sameday ? '' : 'none' });
}

$(function () {
    setInterval(updateTimeIndicator, 15 * 1000);
});