/// <reference path="core/jquery-1.8.2.js" />
/// <reference path="core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />

$(function () {

    // setup knockoutjs data-bind on model generated inputs
    $.each($('#Configuration-User-Editor tr'), function (i, ele) {
        var _class = $(ele).attr('class');
        $(ele).find('input:not([type=checkbox])').attr('data-bind', 'value: ' + _class);
    });

    $('#Configuration-User-Editor .password input').attr('data-bind', 'value: password, event: { keyup: $root.passwordConfirmKeyUp }');
    $('#Configuration-User-Editor .passwordconfirm input').attr('data-bind', 'value: passwordconfirm, event: { keyup: $root.passwordConfirmKeyUp }');

    function UsersViewModel() {
        var self = this;

        self.users = ko.observableArray([]);
        self.user = ko.observable();

        api.getJSON('Users', null, function (data) {
            var mapped = $.map(data, function (item) { return new User(item) });
            self.users(mapped);
        });

        self.add = function () {
            var user = new User();
            showUserEditor(user, true);
        };

        self.edit = function (item) {
            showUserEditor(item, false);
        };

        self.delete = function (item) {
            gui.confirmMessage({
                message: $.i18n._("Are you sure you want to delete the user '%s'?", [item.username()]),
                yes: function () {
                    api.deleteJSON('users/delete/' + item.oid(), null, function () {
                        self.users.remove(item);
                    });
                }
            });
        };

        self.adminClick = function (item) {
            funCheckAdmin(item.administrator());
            return true;
        };

        self.passwordConfirmKeyUp = function (item) {
            var same = $('#Configuration-User-Editor .password input').val() == $('#Configuration-User-Editor .passwordconfirm input').val();
            if (same)
                $('#Configuration-User-Editor #confirmpassword-error').addClass('field-validation-valid').removeClass('field-validation-error').css('display', 'none');
            else
                $('#Configuration-User-Editor #confirmpassword-error').addClass('field-validation-error').removeClass('field-validation-valid').css('display', 'inline');
        };

        var funCheckAdmin = function (isAdmin) {
            if (isAdmin)
                $('#Configuration-User-Editor .userroles input').attr('disabled', true);
            else
                $('#Configuration-User-Editor .userroles input').attr('disabled', false);
        };

        var showUserEditor = function (user, isNew, callback) {
            self.user(user);

            funCheckAdmin(user.administrator());

            var dialog = $('#Configuration-User-Editor');
            dialog.find('.username input').attr('readonly', !isNew).attr('disabled', !isNew);
            dialog.find('tr.password, tr.passwordconfirm').css('display', isNew ? null : 'none');

            var dialog_buttons = {};
            dialog_buttons[$.i18n._(isNew ? "Create" : "Save")] = function () {

                var isValid = true;
                var form = $('#Configuration-User-Editor form');
                isValid &= (form.validate().element($('#userModel_EmailAddress ')));
                if (isNew) {
                    isValid &= form.validate().element($('#userModel_Username '));
                    isValid &= (form.validate().element($('#userModel_Password ')));
                    isValid &= $('#Configuration-User-Editor .password input').val() == $('#Configuration-User-Editor .passwordconfirm input').val();
                }

                if (isValid == false)
                    return;

                api.postJSON("Users", user.toApiObject(), function (result) {
                    if (isNew)
                        self.users.push(new User(result));
                    dialog.dialog('close');
                });
            };
            dialog_buttons[$.i18n._("Cancel")] = function () { dialog.dialog('close'); };

            dialog.dialog(
            {
                modal: true,
                minWidth: 550,
                minHeight: 400,
                title: $.i18n._(isNew ? 'Create User' : 'Edit User'),
                buttons: dialog_buttons
            });

        };
    }
    ko.applyBindings(new UsersViewModel(), $('#system-tab-users').get(0));
});