var app = angular.module('nextpvr', []);

app.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.
        when('/dashboard', { templateUrl: 'dashboard', controller: DashboardController }).
    //when('/phones/:phoneId', { templateUrl: 'partials/phone-detail.html', controller: PhoneDetailCtrl }).
        otherwise({ redirectTo: '/dashboard' });
} ]);

app.run(function ($rootScope) {
    $rootScope.$on('$viewContentLoaded', function () {
        translateElement($('#maincontent'));
    });
});

app.filter('datetimeFormatter', function () {
    return function (date, format) {
        switch (format) {
            case 'shortDate': return gui.formatDateShort(date);
            case 'longDate': return gui.formatDateLong(date);
            case 'time': return gui.formatTime(date);
            case 'shortDateTime': return gui.formatDateTimeShort(date);
        }
        return gui.formatDateLong(date);
    };
});