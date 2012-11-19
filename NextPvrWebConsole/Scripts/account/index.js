$(function () {
    $('#lnkForgotPassword').click(function () {
        $('.loginForm-container').css('display', 'none').find('input[type=password], input[type=text]').val('');
        $('.forgotpasswordForm-container').css('display', 'block').find('input').val('');
        $('.forgotpasswordForm-container input:first').focus();
    });

    $('#btnForgotPasswordCancel').click(function () {
        $('.loginForm-container').css('display', 'block').find('input[type=password], input[type=text]').val('');
        $('.forgotpasswordForm-container').css('display', 'none').find('input').val('');
        $('.loginForm-container input:first').focus();
    });
});