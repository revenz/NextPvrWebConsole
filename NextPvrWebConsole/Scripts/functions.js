/// <reference path="jquery-1.8.2.js" />
/// <reference path="jquery.dateFormat-1.0.js" />

var gui = new function () {

    var doWorkCount = 0;

    this.formatDateLong = function (date) {
        return $.format.date(date, 'd MMMM yyyy h:mm a');
    }
    this.formatDateShort = function (date) {
        return $.format.date(date, 'd MMMM');
    }
    this.formatTime = function (date) {
        return $.format.date(date, 'h:mm a');
    }

    this.showMessage = function (message, title) {
        alert(message); // for now
    };
    
    this.showSuccess = function(message, title) {
        toastr.success(message, title ? title : 'Success');
    };

    this.showInfo = function(message, title) {
        toastr.success(message, title ? title : 'Information');
    };

    this.showError = function (message, title) {
        toastr.error(message, title ? title : 'Error');
    };

    this.showWarning = function (message, title) {
        toastr.success(message, title ? title : 'Warning');
    };

    this.doWork = function () {
        doWorkCount++;

        // show working div
        $('#working').show();
    };

    this.finishWork = function () {
        if (--doWorkCount < 1)
            $('#working').hide();
        else
            doWorkCount = 0; // make sure this doesnt drop below 0
    };
}