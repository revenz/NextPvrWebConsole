/// <reference path="jquery-1.8.2.js" />
/// <reference path="jquery.dateFormat-1.0.js" />
/// <reference path="jquery-ui-1.9.0.js" />

var gui = new function () {

    var doWorkCount = 0;

    this.formatDateLong = function (date) {
        if (!(date instanceof Date))
            date = new Date(date);
        return $.format.date(date, 'd MMMM yyyy h:mm a');
    }
    this.formatDateShort = function (date) {
        if (!(date instanceof Date))
            date = new Date(date);
        return $.format.date(date, 'd MMMM');
    }
    this.formatTime = function (date) {
        if (!(date instanceof Date))
            date = new Date(date);
        return $.format.date(date, 'h:mm a');
    }
    this.formatDateTimeShort = function (date) {
        if (!(date instanceof Date))
            date = new Date(date);
        return $.format.date(date, 'h:mm a') + ' (' + $.format.date(date, 'd MMM') + ')';
    };

    this.showSuccess = function (message, title) {
        toastr.success(message, title ? title : $.i18n._("Success"));
    };

    this.showInfo = function (message, title) {
        toastr.success(message, title ? title : $.i18n._("Information"));
    };

    this.showError = function (message, title) {
        toastr.error(message, title ? title : $.i18n._("Error"));
    };

    this.showWarning = function (message, title) {
        toastr.success(message, title ? title : $.i18n._("Warning"));
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
    var unloadFunction = function (e) {
        if (doWorkCount > 0) {
            var message = $.i18n._("This page is currently procesing a request.");
            e = e || window.event;
            // For IE and Firefox
            if (e) {
                e.returnValue = message;
            }
            // For Safari
            return message;
        }
    };
    if (window.addEventListener)
        window.addEventListener('unload', unloadFunction, false);
    else if (window.attachEvent)
        window.attachEvent('unload', unloadFunction);

    this.showMessageBox = function (message, title) {
        var div = $('<div class="modal">' +
                        '<div class="modal-header">' +
                            '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>' +
                            '<h3></h3>' +
                        '</div>' +
                        '<div class="modal-body"></div>' +
                        '<div class="modal-footer">' +
                            '<button data-dismiss="modal" aria-hidden="true" class="btn btn-primary"></button>' +
                        '</div>' +
                    '</div>');
        div.find('h3').text(title ? title : $.i18n._("Message"));
        div.find('.modal-body').text(message);
        div.find('.btn').text($.i18n._('OK'));
        div.appendTo('body')
        div.modal({});
        div.on('hidden', function () {
            div.remove();
        });
    };

    this.confirm = function (settings) {
        settings = $.extend({
            title: $.i18n._('Confirm'),
            message: $.i18n._('Are you sure?'),
            yesText: $.i18n._('Yes'),
            noText: $.i18n._('No'),
            yes: null,
            no: null,
            minWidth: 400,
            minHeight: 200
        }, settings);
        var div = $('<div class="modal">' +
                        '<div class="modal-header">' +
                            '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>' +
                            '<h3></h3>' +
                        '</div>' +
                        '<div class="modal-body"></div>' +
                        '<div class="modal-footer">' + 
                            '<a href="#" data-dismiss="modal" aria-hidden="true" class="btn btn-yes"></a>' +
                            '<a href="#" data-dismiss="modal" aria-hidden="true" class="btn btn-primary btn-no"></a>' +
                        '</div>' +
                    '</div>');
        div.find('h3').text(settings.title);
        div.find('.modal-body').text(settings.message);
        div.find('.btn-yes').text(settings.yesText).click(function () { if (settings.yes) { settings.yes(); } });
        div.find('.btn-no').text(settings.noText).click(function () { if (settings.no) { settings.no(); } });
        div.appendTo('body')
        div.modal({});
        div.on('hidden', function () {
            div.remove();
        });
    };

    this.promptMessage = function (settings) {

        settings = $.extend({
            title: $.i18n._('Input'),
            message: $.i18n._('Please input a value.'),
            validationExpression: "(.*?)",
            validationMessage: $.i18n._('Input is required.'),
            initialValue: '',
            success: null,
            maxLength: 100
        }, settings);

        var div = $('<div class="modal">' +
                        '<div class="modal-header">' +
                            '<button type="button" class="close" aria-hidden="true">&times;</button>' +
                            '<h3></h3>' +
                        '</div>' +
                        '<div class="modal-body">' +
                            '<span class="message"></span> ' +
                            '<input type="text" style="width: 99%;padding: 0;margin: 15px 0 5px;" /> ' +
                            '<span class="field-validation-error errormessage" /> ' +
                        '</div>' +
                        '<div class="modal-footer">' +
                            '<button aria-hidden="true" class="btn btn-primary btn-ok"></button>' +
                            '<button aria-hidden="true" class="btn btn-cancel"></button>' +
                        '</div>' +
                    '</div>');
        div.find('h3').text(settings.title);
        div.find('.message').text(settings.message);
        div.find('.errormessage').text(settings.validationMessage).css('display', 'none');
        var btnOk = div.find('.btn-ok');
        btnOk.text($.i18n._("OK"));
        var btnCancel = div.find('.btn-cancel');
        btnCancel.text($.i18n._("Cancel"));
        var input = div.find('input');

        if (settings.maxLength > 0)
            input.attr('maxlength', settings.maxLength);
        input.val(settings.initialValue);

        var errorMessage = div.find('.errormessage');
        var rgxValidate = null;
        if (settings.validationExpression) {
            rgxValidate = new RegExp(settings.validationExpression);
            input.keyup(function () {
                var value = input.val();
                errorMessage.css('display', rgxValidate.test(value) ? 'none' : '');
            });
        }
        input.keypress(function (e) {
            if (e.which == 13)
                div.closest('.ui-dialog').find('.ui-dialog-buttonset button:first').click();
        });

        btnOk.click(function ()
        {
            var value = input.val();
            // validate
            if (rgxValidate && !rgxValidate.test(value))
                return; // its invalid

            if (settings.success) { settings.success(value); }

            div.modal('hide');
        });
        btnCancel.click(function () {
            div.modal('hide');
        });
        div.appendTo('body');
        div.modal({});

        div.on('hidden', function () {
            div.remove();
        });
    };

    this.folderBrowser = function (settings) {

        settings = $.extend({
            title: $.i18n._('Folder Browser'),
            message: $.i18n._('Please select a folder.'),
            selected: null,
        }, settings);

        var div = $( '<div class="FolderBrowserWindow modal hide">' +
		             '  <div class="modal-header"> ' +
                  '         <button type="button" class="close" ng-click="close()" aria-hidden="true">&times;</button>' +
			      '         <h3></h3>' +
		          '     </div>' +
		          '     <div class="modal-body">' +
                  '         <span class="message"></span>' +
                  '         <div class="FileTree">' +
				  '             <div></div>' +
			      '         </div>' +
		          '     </div>' +
		          '     <div class="modal-footer">' +
			      '         <button class="btn btn-primary btn-ok">OK</button>' +
			      '         <button class="btn btn-cancel">Cancel</button>' +
		          '     </div>' + 
	              '</div>');
        div.find('h3').text(settings.title);
        div.find('.message').text(settings.message);
        var btnOk = div.find('.btn-ok');
        btnOk.text($.i18n._("OK"));
        var btnCancel = div.find('.btn-cancel');
        btnCancel.text($.i18n._("Cancel"));
        
        btnOk.click(function () {
            var selected = div.find('a.selected');
            if (selected.length < 1)
                return;

            div.modal('hide');

            if (settings.selected)
                settings.selected(selected.attr('rel'));

        });
        btnCancel.click(function () {
            div.modal('hide');
        });
        div.appendTo('body');
        div.modal({});

        var filetree = div.find('.FileTree div');
        filetree.fileTree(
        {
            root: '%root%',
            script: '/file/LoadDirectory',
        }, function (file) {
            alert(file);
        });

        div.on('hidden', function () {
            div.remove();
        });
    };

    this.fileBrowser = function (settings) {
        var self = this;
        settings = $.extend({
            title: $.i18n._('File Browser'),
            message: $.i18n._('Please select a file.'),
            xmlFiles: false
        }, settings);

        var div = $('<div class="FolderBrowserWindow modal hide">' +
		             '  <div class="modal-header"> ' +
                  '         <button type="button" class="close" ng-click="close()" aria-hidden="true">&times;</button>' +
			      '         <h3></h3>' +
		          '     </div>' +
		          '     <div class="modal-body">' +
                  '         <span class="message"></span>' +
                  '         <div class="FileTree">' +
				  '             <div></div>' +
			      '         </div>' +
		          '     </div>' +
		          '     <div class="modal-footer">' +
			      '         <button class="btn btn-primary btn-ok">OK</button>' +
			      '         <button class="btn btn-cancel">Cancel</button>' +
		          '     </div>' +
	              '</div>');
        div.find('h3').text(settings.title);
        div.find('.message').text(settings.message);
        var btnOk = div.find('.btn-ok');
        btnOk.text($.i18n._("OK"));
        var btnCancel = div.find('.btn-cancel');
        btnCancel.text($.i18n._("Cancel"));

        btnOk.click(function () {
            var selected = div.find('.file a.selected');
            if (selected.length < 1)
                return;

            div.modal('hide');
            
            var rel = selected.attr('rel');

            if (self.successFun)
                self.successFun({
                    shortName: selected.text(),
                    fullName: rel
                    });

        });
        btnCancel.click(function () {
            div.modal('hide');
        });
        div.appendTo('body');
        div.modal({});

        var script = '/file/LoadDirectory';
        if (settings.xmlFiles)
            script = '/file/LoadDirectoryAndXml';

        var filetree = div.find('.FileTree div');
        filetree.fileTree(
        {
            root: '%root%',
            script: script,
        }, function (file) {
        });

        div.on('hidden', function () {
            div.remove();
        });

        self.success = function (successFun) {
            self.successFun = successFun;
        };

        return self;
    };
}

if (typeof String.prototype.startsWith != 'function') {
    String.prototype.startsWith = function (str) {
        return this.indexOf(str) == 0;
    };
}
if (typeof String.prototype.endsWith != 'function') {
    String.prototype.endsWith = function (suffix) {
        return this.indexOf(suffix, this.length - suffix.length) !== -1;
    };
}
if (typeof Number.prototype.pad != 'function') {
    Number.prototype.pad = function (number) {
        return Array(number - String(this).length + 1).join('0') + this;
    };
}
if (typeof String.isNullOrWhitespace != 'function') {
    String.isNullOrWhitespace = function (text) {
        return !/\S/.test(text);
    };
}

if (typeof Array.prototype.remove != 'function') {
    Array.prototype.remove = function () {
        var what, a = arguments, L = a.length, ax;
        while (L && this.length) {
            what = a[--L];
            while ((ax = this.indexOf(what)) !== -1) {
                this.splice(ax, 1);
            }
        }
        return this;
    };
}