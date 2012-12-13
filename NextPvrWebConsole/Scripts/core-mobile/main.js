function loadFooter(Target, SelectedTabId, page, loaded) {
    $.get('footer.html', function (html) {
        html = html.replace('id="' + SelectedTabId + '"', 'id="' + SelectedTabId + '" class="ui-btn-active"');
        html = html.replace('id="footer"', 'id="' + SelectedTabId + 'footer"');
        Target.replaceWith(html);
        page.page().page('destroy').page();
        loaded();
    });
}
function getTimeZoneBias() {
    return new Date().getTimezoneOffset();
}