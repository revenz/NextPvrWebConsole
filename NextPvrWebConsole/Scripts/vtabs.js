/// <reference path="jquery-1.8.2.js" />
/// <reference path="functions.js" />

$(function () {

    if ($('.vtab-container').length == 0)
        return;

    $('body').css('overflow-y', 'hidden');

    var resizeTabs = function () {
        $('.vtab-container').css('height', $(window).height() - 140);
    };

    $(window).resize(resizeTabs);

    $('.vtab-buttons li').click(function () {
        var $this = $(this);
        var dataTab = $this.attr('data-tab');
        $.address.value("/tab/" + dataTab);
    });

    resizeTabs();
});

$.address.change(function (event) {
    if (event.value.startsWith('/tab/')) {
        var tabname = event.value.substr(5);
        if (tabname.indexOf('/') > 0)
            tabname = tabname.substr(0, tabname.indexOf('/'));
        console.log('tabname: ' + tabname);
        var tabButton = $('.vtab-buttons [data-tab=' + tabname + ']');
        var container = tabButton.closest('.vtab-container');
        container.find('.vtab-buttons .selected').removeClass('selected');
        tabButton.addClass('selected');
        container.find('.vtab-content.selected').removeClass('selected');
        container.find('.vtab-content[id$=' + tabname + ']').addClass('selected');
    }
});