/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery.i18n.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../functions.js" />
/// <reference path="../core/jquery.i18n.js" />

function Log(data) {
    var self = this;
    self.name = ko.observable(data.Name);
    self.size = ko.observable(data.Size);
    self.oid = ko.observable(data.Oid);
    self.dateModified = ko.observable(data.DateModified);
    self.dateModifiedString = ko.computed(function () {
        return gui.formatDateLong(self.dateModified());
    });
    self.sizeString = ko.computed(function () {
        return addCommas(Math.round(self.size() / 1024)) + ' KB';
    });
} 

function addCommas(nStr) {
    nStr += '';
    var x = nStr.split('.');
    var x1 = x[0];
    var x2 = x.length > 1 ? '.' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + ',' + '$2');
    }
    return x1 + x2;
}