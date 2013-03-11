var ns = namespace('Controllers.Configuration');

ns.SchedulesDirectController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $scope.model = {
        config: null
    };

    gui.doWork();

    $http.get('/api/configuration/schedulesDirect').success(function (result) {
        gui.finishWork();
        $scope.model.config = result;
    }).error(function () {
        gui.finishWork();
    });

    $scope.save = function () {
        gui.doWork();
        $http.post('/api/configuration/schedulesDirect', $scope.model.config).success(function (result) {
            gui.finishWork();
            $scope.scan();
        }).error(function () {
            gui.finishWork();
        });
    };

    $scope.scan = function () {
        gui.doWork();
        $http.post('/api/configuration/schedulesDirectScan', $scope.model.config).success(function (result) {
            console.log(result);
            $rootScope.root.SchedulesDirectLineups = result;
            gui.finishWork();
        }).error(function () {
            gui.finishWork();
        });
    };
};
ns.SchedulesDirectController.$inject = ['$scope', '$http', '$rootScope'];
