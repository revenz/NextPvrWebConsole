function getTimeZoneBias() {
    return new Date().getTimezoneOffset();
}

$(function () {

    var $footer = $('#footer');
    if ($footer.length) {
        var $masterpage = $('#masterpage');

        $footer.find('a').click(function () {
            changeAppPage($(this).attr('href').substr(1));
        });

        console.log('location.hash: ' + location.hash);
        var page = $footer.find('a:first').attr('href').substr(1);
        if (location.hash && location.hash.length > 2) {
            page = location.hash.substr(1);
        }
        changeAppPage(page);

        function changeAppPage(pageName) {
            var original = pageName;
            location.hash = original;

            if (pageName.indexOf('-') > 0)
                pageName = pageName.substr(0, pageName.indexOf('-'));

            var sender = $('#pagetab_' + pageName);
            console.log('pageName: ' + pageName);

            $.get(pageName, function (html) {
                $footer.find('.ui-btn-active').removeClass('ui-btn-active');
                $footer.detach();
                var _class = $masterpage.attr('class');
                _class = _class.replace(/page[\w]+/gi, '');
                $masterpage.html(html);
                $masterpage.trigger("pagecreate");
                $masterpage.attr('class', _class).addClass('page' + pageName);
                $footer.appendTo('#masterpage');
                sender.addClass("ui-btn-active");

                var loadscript = sender.attr('data-load-script');
                if (loadscript && loadscript.length > 0)
                    eval('(' + loadscript + ')');
            });
        }
    }
});