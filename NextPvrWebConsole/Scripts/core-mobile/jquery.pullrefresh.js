/**
* jQuery WP7 Pull to Refresh Plugin 1.0
*
* http://www.jeroenvanwissen.nl/
*
* Copyright (c) 2012 Jeroen van Wissen
*/

(function ($) {
    $.fn.pullrefresh = function (options) {
        var defaults = {
            pullDown: '#pullDown',
            pullDownIcon: '.pullDownIcon',
            url: '',
            urlFunction: undefined,
            data: '',
            dataType: 'json',
            success: undefined,
            pullDownLabel: '.pullDownLabel',
            pullDownLabelText: 'Pull down to refresh...',
            loadingText: 'Loading...'
        };
        options = $.extend(defaults, options);
        var refreshTimer;
        var self = this;

        // scroll to the first item of the list,
        // and make pullDown element "hidden" outside the box.
        $(options.pullDownLabel).text(options.pullDownLabelText);
        var pullDownHeight = $(options.pullDown).height();
        this.scrollTop(pullDownHeight);

        self.scroll(function () {
            if (0 === self.scrollTop()) {
                setTimeout(function () {
                    $(options.pullDownIcon).addClass('loading');
                    $(options.pullDownLabel).text(options.loadingText);
                }, 500);

                refreshTimer = setTimeout(function () {
                    var url = options.url;
                    if (options.urlFunction)
                        url = options.urlFunction();

                    $.ajax({
                        url: url,
                        dataType: options.dataType,
                        data: options.data,
                        success: function (data) {
                            if ('undefined' !== typeof options.success) {
                                options.success(data);
                            }
                            setTimeout(function () {
                                $('.scrollable').scrollTop(pullDownHeight);
                                $(options.pullDownIcon).removeClass('loading');
                            }, 100);
                        }
                    });
                }, 1500);
            } else {
                clearTimeout(refreshTimer);
                $(options.pullDownIcon).removeClass('loading');
                $(options.pullDownLabel).text(options.pullDownLabelText);
            }
        });

    };
})(jQuery);