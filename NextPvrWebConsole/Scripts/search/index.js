/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../core/jquery-1.8.2.js" />
/// <reference path="../core/jquery-ui-1.9.0.js" />
/// <reference path="../core/knockout-2.2.0.js" />
/// <reference path="../core/jquery.dateFormat-1.0.js" />

$(function () {

    function SearchViewModel() {
        // Data
        var self = this;
        self.selectedListing = ko.observable();

        var prePadding = parseInt($('#PrePadding').val(), 10);
        var postPadding = parseInt($('#PostPadding').val(), 10);

        $('.btnCancelRecording').click(function () {
            var tr = $(this).closest('tr');
            var recordingOid = parseInt(tr.attr('data-recordingoid'), 10);
            var title = tr.find('.title').text();
            gui.confirmMessage({
                message: $.i18n._("Are you sure you want to cancel the recording '%s'?", [title]),
                yes: function () {
                    api.deleteJSON('recording/' + recordingOid, null, function (result) {
                        if (result) {
                            tr.find('.btnRecord').removeAttr('style');
                            tr.find('.btnCancelRecording').attr('style', 'display:none');
                        }
                    });
                }
            });
        });
        $('.btnRecord').click(function () {
            var tr = $(this).closest('tr');
            var oid = parseInt(tr.attr('data-oid'), 10);
            api.getJSON('guide/epglisting/' + oid, null, function (result) {
                console.log(result);
                var listing = new Listing(null, result);
                listing.prePadding(prePadding);
                listing.postPadding(postPadding);
                listing.type = ko.computed({
                    read: function () { return listing.recordingType(); },
                    write: function (value) { listing.recordingType(value); }
                });
                self.selectedListing(listing);
                var dialog = $('#Search-ScheduleEditor');
                var dialog_buttons = {};
                dialog_buttons[$.i18n._("Record")] = function () {
                    dialog.dialog('close');
                    api.postJSON('guide/record',
                    {
                        oid: listing.oid(),
                        prePadding: listing.prePadding(),
                        postPadding: listing.postPadding(),
                        recordingdirectoryid: listing.recordingDirectoryId(),
                        numbertokeep: listing.keep(),
                        type: listing.type()
                    }, function (result) {
                        console.log(result);
                        if (result) {
                            tr.find('.btnCancelRecording').removeAttr('style');
                            tr.find('.btnRecord').attr('style', 'display:none');
                        }
                    });
                };
                dialog_buttons[$.i18n._("Close")] = function () { dialog.dialog('close'); };
                dialog.dialog({
                    modal: true,
                    title: result.Title,
                    width: 670,
                    height: 340,
                    resizable: false,
                    buttons: dialog_buttons
                });
            });
        });
    }
    var viewModel = new SearchViewModel();
    ko.applyBindings(viewModel, $('#schedule-editor').get(0));

});