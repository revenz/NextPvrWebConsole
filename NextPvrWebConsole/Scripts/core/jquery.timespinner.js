$(function () {
    $.widget("ui.timespinner", $.ui.spinner, {
        options: {
            // seconds
            step: 60* 60 * 1000,
            // hours
            page: 60
        },

        _parse: function (value) {
            if (typeof value === "string") {
                // already a timestamp
                if (Number(value) == value) {
                    return Number(value);
                }
                var hour = parseInt(value.substr(0, value.indexOf(':')), 10);
                var mins = parseInt(value.substr(value.indexOf(':') + 1, 2), 10);
                var pm = value.toLowerCase().indexOf('pm') > 0;

                if (hour == 12 && !pm)
                    hour = 0;
                else if (hour < 12 && pm)
                    hour += 12;
                var tmins = (hour * 60 + mins);
                var date = new Date();
                date.setHours(hour);
                date.setMinutes(mins);
                return date.getTime();
            }
            return value;
        },

        _format: function (value) {
            return $.format.date(new Date(value), "h:mm a");
        }
    });
});