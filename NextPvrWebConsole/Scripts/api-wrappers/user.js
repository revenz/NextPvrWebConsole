/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="listing.js" />

var USERROLE_DASHBOARD = 1;
var USERROLE_GUIDE = 2;
var USERROLE_RECORDINGS = 4;
var USERROLE_USERSETTINGS = 8;
var USERROLE_CONFIGURATION = 16;
var USERROLE_SYSTEM = 32;

function User(data) {
    if (!data) {
        data = { Oid: 0, Username: '', EmailAddress: '', UserRole: 0, LastLoggedInUtc: new Date(0), DateCreatedUtc: new Date(0), PasswordHash: '', ReadOnly: false, Administrator: false, Password: '', ConfirmPassword: '' };
    }
    var self = this;
    self.oid = ko.observable(data.Oid);
    self.username = ko.observable(data.Username);
    self.emailaddress = ko.observable(data.EmailAddress);
    self.userrole = ko.observable(data.UserRole);
    if (typeof (data.LastLoggedInUtc) == 'string')
    {
        if(data.LastLoggedInUtc.indexOf('.') > 0)
            data.LastLoggedInUtc = data.LastLoggedInUtc.substr(0, data.LastLoggedInUtc.indexOf('.')) + 'Z';
        data.LastLoggedInUtc = new Date(data.LastLoggedInUtc);
    }

    self.lastLoggedIn = ko.observable(data.LastLoggedInUtc);
    self.dateCreated = ko.observable(data.DateCreatedUtc);
    self.readonly = ko.observable(data.ReadOnly);
    self.passwordHash = ko.observable(data.PasswordHash);
    self.password = ko.observable(data.Password ? data.Password : '');
    self.passwordconfirm = ko.observable(data.ConfirmPassword ? data.ConfirmPassword : '');
    self.administrator = ko.observable(data.Administrator);
    
    self.lastLoggedInString = ko.computed(function () {
        if (self.lastLoggedIn().getFullYear() < 2000)
            return $.i18n._('Never');
        return gui.formatDateLong(self.lastLoggedIn());
    });
    self.dateCreatedString = ko.computed(function () {
        return gui.formatDateLong(self.dateCreated());
    });
    self.administratorString = ko.computed(function () {
        return self.administrator() ? $.i18n._('Yes') : $.i18n._('No');
    });
    self.roleDashboard = ko.computed({
        read: function () { return (self.userrole() & USERROLE_DASHBOARD) == USERROLE_DASHBOARD; },
        write: function (value) { self.userrole(self.userrole() | USERROLE_DASHBOARD); }
    });
    self.roleGuide = ko.computed({
        read: function () { return (self.userrole() & USERROLE_GUIDE) == USERROLE_GUIDE; },
        write: function (value) { self.userrole(self.userrole() | USERROLE_GUIDE); }
    });
    self.roleRecordings = ko.computed({
        read: function () { return (self.userrole() & USERROLE_RECORDINGS) == USERROLE_RECORDINGS; },
        write: function (value) { self.userrole(self.userrole() | USERROLE_RECORDINGS); }
    });
    self.roleConfiguration = ko.computed({
        read: function () { return (self.userrole() & USERROLE_CONFIGURATION) == USERROLE_CONFIGURATION; },
        write: function (value) { self.userrole(self.userrole() | USERROLE_CONFIGURATION); }
    });
    self.roleSystem = ko.computed({
        read: function () { return (self.userrole() & USERROLE_SYSTEM) == USERROLE_SYSTEM; },
        write: function (value) { self.userrole(self.userrole() | USERROLE_SYSTEM); }
    });
    self.roleUserSettings = ko.computed({
        read: function () { return (self.userrole() & USERROLE_USERSETTINGS) == USERROLE_USERSETTINGS; },
        write: function (value) { self.userrole(self.userrole() | USERROLE_USERSETTINGS); }
    });

    self.toApiObject = function () {
        data.Oid = self.oid();
        data.Username = self.username();
        data.EmailAddress = self.emailaddress();
        data.UserRole = self.userrole();
        data.LastLoggedInUtc = self.lastLoggedIn();
        data.DateCreatedUtc = self.dateCreated();
        data.ReadOnly = self.readonly();
        data.PasswordHash = self.passwordHash();
        data.Administrator = self.administrator();
        data.Password = self.password();
        data.ConfirmPassword = self.passwordconfirm();
        return data;
    };
}