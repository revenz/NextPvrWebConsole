var DIRECTORY_REGULAR_EXPRSESION = '^([^"*/:?|<>\\\\.\\x00-\\x20]([^"*/:?|<>\\\\\\x00-\\x1F]*[^"*/:?|<>\\\\.\\x00-\\x20])?)$';
var DIRECTORY_ERROR_MESSAGE = $.i18n._('Invalid folder name');
var DIRECTORY_CREATE_MESSAGE = $.i18n._('Type in the name of the folder to create.');
var DIRECTORY_CREATE_TITLE = $.i18n._('Create Recording Folder');

$(function () {
    $(":checkbox:not(.noibutton)").iButton();
});