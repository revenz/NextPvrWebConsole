/// <reference path="jquery-1.8.2.js" />
/// <reference path="functions.js" />
/// <reference path="jquery.signalR-0.5.3.js" />

var api = new function()
{
    var _json = function (type, url, data, callback) {
        gui.doWork();
        $.ajax(
        {
            url: "/api/" + url, 
            type: type,
            //contentType: 'application/json; charset=utf-8',
            accepts: 'application/json',
            dataType: 'json',
            data: data,
            statusCode:
            {
                200: function(data)
                {
                    if(callback)
                        callback(data);
                },
                401: function(jqXHR, textStatus, errorThrown) {
                    self.location = '/Account/Login/';
                },
            },
            error: function(jqXHR, textStatus, errorThrown)
            {
                var error = $.parseJSON(jqXHR.responseText); 
                gui.showError(error && error.message ? error.message : 'An unexpected server error occurred.');
            },
            complete: function(jqXHR, textStatus)
            {
                gui.finishWork();
            }
        });
    }

    this.getJSON = function (url, callback) {
        _json('GET', url, null, callback);
    }

    this.postJSON = function (url, data, callback) {
        _json('POST', url, data, callback);
    }
}

var npvrevent;
$(function(){
    npvrevent = $.connection.nextPvrEvent;
    npvrevent.showInfoMessage = function(message, title) { gui.showInfo(message, title); };
    npvrevent.showErrorMessage = function(message, title) { gui.showError(message, title); };
    npvrevent.showSuccessMessage = function(message, title) { gui.showSuccess(message, title); };
    npvrevent.showWarningMessage = function(message, title) { gui.showWarning(message, title); };
    
    // Start the connection
    $.connection.hub.start();
});