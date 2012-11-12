$(function () {
    
    $('form').submit(function () {
        if ($(this).valid()) {
            $('.setup-page-content').css('display', 'none');
            $('.setup-page-content-loading').css('display', '');
        }
    }).keypress(function (e) {
        if (e.which == 13) {
            $(this).blur();
            $('#btnSubmit').focus().click();
        }
    });
});