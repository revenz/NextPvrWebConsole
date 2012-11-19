/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery.dateFormat-1.0.js" />

$(function () {
    $('#btnSmtpSettingsSave').click(function () {
        if (!$('#updateSmtpSettingsForm').valid())
            return;
        ajax.postJSON('System/UpdateSmtpSettings',
            {
                Server: $('[id$=smtpModel_Server]').val(),
                Port: parseInt($('[id$=smtpModel_Port]').val(), 10),
                Username: $('[id$=smtpModel_Username]').val(),
                Password: $('[id$=smtpModel_Password]').val(),
                Sender: $('[id$=smtpModel_Sender]').val(),
                UseSsl: $('[id$=smtpModel_UseSsl]:checked').length > 0
            },
            function () {
            }
        );
    });
});