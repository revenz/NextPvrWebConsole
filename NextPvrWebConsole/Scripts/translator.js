if (language == null)
    return;

$.i18n.setDictionary(language);


$.each($('[data-lang]'), function (i, ele) {
    var html = $('<div />').text($.i18n._($(this).attr('data-lang'))).html().replace(/\n/g, '<br />');
    $(this).html(html);
});
