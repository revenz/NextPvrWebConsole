if (language == null)
    return;
$.i18n.setDictionary(language);

$.each($('[data-lang]'), function (i, ele) {
    $(this).text($.i18n._($(this).attr('data-lang')));
});
