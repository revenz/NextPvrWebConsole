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
        var container = $this.closest('.vtab-container');
        container.find('.vtab-buttons .selected').removeClass('selected');
        $this.addClass('selected');
        var tabid = $this.attr('data-tab');
        container.find('.vtab-content.selected').removeClass('selected');
        $('#' + tabid).addClass('selected');
    });

    resizeTabs();
});