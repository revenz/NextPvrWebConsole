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

function ForgotPassword_Complete(args) {
    //var form = $(args.$1).parent();
    var form = $('#formForgotPassword');
    form.find('button, input[type!=checkbox]').removeAttr('disabled').removeAttr('readonly');

    var data = eval('(' + args.responseText + ')');
    if (data.success != true) {
        var msg = $.i18n._('Failed to reset password.');
        if (data.message && data.message.length > 0)
            msg = data.message;
        gui.alert(msg);
    }
    else {
        gui.alert($.i18n._('A password reset request has been emailed to your account.'));
        $('#btnForgotPasswordCancel').click();
    }
}

function ForgotPassword_Begin(args) {
    //var form = $(args.$1).parent();
    var form = $('#formForgotPassword');
    form.find('button, input[type!=checkbox]').attr('disabled', true).attr('readonly', true);
}