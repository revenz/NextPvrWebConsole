var app = angular.module('nextpvr', []);

app.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/dashboard', { templateUrl: 'dashboard' })
                  .when('/guide', { templateUrl: 'guide' })
                  //.when('/phones/:phoneId', { templateUrl: 'partials/phone-detail.html', controller: PhoneDetailCtrl })
                  .otherwise({ redirectTo: '/dashboard' });

} ]);

app.run(function ($rootScope) {
    // setup the watcher to set the 'active' item in the main menu
    $rootScope.$on("$routeChangeSuccess", function (current, previous) {
        if (previous && previous.templateUrl && previous.templateUrl.length) {
            var url = previous.templateUrl;
            if (url.substr(0, 1) == '/')
                url = url.substr(1);
            if (url.indexOf('/') > 0)
                url = url.substr(0, url.indexOf('/'));

            $('#mainmenu .active').removeClass('active');
            if (url.length)
                $('#mainmenu .' + url).addClass('active');
        }
    });

    $rootScope.$on('$viewContentLoaded', function () {
        translateElement($('#maincontent'));
    });
});