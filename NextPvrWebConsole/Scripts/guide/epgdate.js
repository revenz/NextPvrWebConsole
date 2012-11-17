$(function(){
    function EpgDateViewModel(){
        var self = this;

        var days = new Array();
        var daysOfWeekString = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        var currentDayOfWeek = new Date().getDay();
        for (var i = 0; i < 7; i++) {
            var tDate = new Date();
            tDate.setDate(tDate.getDate() + i);
            tDate.setHours(0, 0, 0, 0);
            days.push(new epgDate(tDate, i == 0, $.i18n._(daysOfWeekString[currentDayOfWeek]) + " (" + tDate.getDate() + '/' + (tDate.getMonth() + 1) + ')'));
            if (++currentDayOfWeek >= 7)
                currentDayOfWeek = 0;
        }
        self.epgdays = ko.observableArray(days);

        self.changeEpgDay = function (day) {
            if (guideViewModel == null)
                return; // must not be loaded yet
            $.each(self.epgdays(), function (i, ele) { ele.selected(false); });
            day.selected(true);
            guideViewModel.loadEpgData(day.date());
        }

    };

    var viewModel = new EpgDateViewModel();
    ko.applyBindings(viewModel, $('.epg-days').get(0));
    $('.epg-days li:eq(0)').addClass('selected');
});