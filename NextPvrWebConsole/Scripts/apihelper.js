﻿/// <reference path="jquery-1.8.2.js" />
/// <reference path="functions.js" />
/// <reference path="jquery.signalR-0.5.3.js" />

var ajax = new function () {
    var _json = function (type, url, data, callback, errorCallback) {
        gui.doWork();
        $.ajax(
        {
            url: url,
            type: type,
            accepts: 'application/json',
            contentType: 'application/json',
            dataType: 'json',
            data: data != null && type != 'GET' ? JSON.stringify(data) : data,
            statusCode:
            {
                200: function (data) {
                    if(data != null && data._error == true)
                    {                         
                        var msg = data.message ? data.message : 'An unexpected server error occurred.';
                        if (errorCallback)
                            errorCallback(msg);
                        else
                            gui.showError(msg);
                    } else {
                    if (callback)
                        callback(data);
                    }
                },
                204: function (data) /* no data, usually from a delete action */ {
                    if (callback)
                        callback(data);
                },
                401: function (jqXHR, textStatus, errorThrown) {
                    self.location = '/Account/Login/';
                },
            },
            error: function (jqXHR, textStatus, errorThrown) {            
                var error = $.parseJSON(jqXHR.responseText);
                var msg = error && error.message ? error.message : 'An unexpected server error occurred.';
                if (errorCallback)
                    errorCallback(msg);
                else
                    gui.showError(msg);
            },
            complete: function (jqXHR, textStatus) {
                gui.finishWork();
            }
        });
    }
    this.getJSON = function (url, data, callback, errorCallback) {
        _json('GET', url, data, callback, errorCallback);
    }
    this.postJSON = function (url, data, callback, errorCallback) {
        _json('POST', url, data, callback, errorCallback);
    }
    this.oncomplete = function () {
        gui.finishWork();
    }
    this.onbegin = function () {
        gui.doWork();
    }
    this.onsuccess = function (data) {
        if(data != null && data.get_response)
            data = data.get_response(); // from Ajax.BeginForm
        if(data != null && data._error == true)
        {                         
            var msg = data.message ? data.message : 'An unexpected server error occurred.';
            gui.showError(msg);
        }
    }
    this.onfailure = function () {
    }
}

var api = new function()
{
    var _json = function (type, url, data, callback, errorCallback) {
        if (type != 'GET' && data) {
            if (typeof (input) == 'string') // simple objects are read using [FromBody] in webapi and require to be "={the value}"
                data = '=' + data;
            else
                data = JSON.stringify(data);
        }
        gui.doWork();
        $.ajax(
        {
            url: "/api/" + url, 
            type: type,
            accepts: 'application/json',
            contentType: 'application/json',
            dataType: 'json',
            data: data,
            statusCode:
            {
                200: function(data)
                {
                    if(callback)
                        callback(data);
                },
                204: function(data) /* no data, usually from a delete action */
                {
                    if (callback)
                        callback(data);
                },
                401: function(jqXHR, textStatus, errorThrown) {
                    self.location = '/Account/Login/';
                },
            },
            error: function(jqXHR, textStatus, errorThrown)
            {
                var error = $.parseJSON(jqXHR.responseText);
                var msg = error && error.message ? error.message : 'An unexpected server error occurred.';
                if (errorCallback)
                    errorCallback(msg);
                else
                    gui.showError(msg);
            },
            complete: function(jqXHR, textStatus)
            {
                gui.finishWork();
            }
        });
    }

    this.getJSON = function (url, data, callback, errorCallback) {
        _json('GET', url, data, callback, errorCallback);
    }

    this.postJSON = function (url, data, callback, errorCallback) {
        _json('POST', url, data, callback, errorCallback);
    }

    this.putJSON = function (url, data, callback, errorCallback) {
        _json('PUT', url, data, callback, errorCallback);
    }

    this.deleteJSON = function (url, data, callback, errorCallback) {
        _json('DELETE', url, data, callback, errorCallback);
    };

    this.oncomplete = function () {
        gui.finishWork();
    }
    this.onbegin = function () {
        gui.doWork();
    }
    this.onsuccess = function () {
    }
    this.onfailure = function () {
    }
}

var npvrevent;
$(function(){
    if(!$.connection || !$.connection.nextPvrEvent)
        return;
    npvrevent = $.connection.nextPvrEvent;
    npvrevent.showInfoMessage = function(message, title) { gui.showInfo(message, title); };
    npvrevent.showErrorMessage = function(message, title) { gui.showError(message, title); };
    npvrevent.showSuccessMessage = function(message, title) { gui.showSuccess(message, title); };
    npvrevent.showWarningMessage = function(message, title) { gui.showWarning(message, title); };
    
    // Start the connection
    $.connection.hub.start({ transport: 'longPolling' }, function(){
    });
});