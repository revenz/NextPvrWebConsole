var ns = namespace('Controllers.Configuration');

ns.XmlTvController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    gui.doWork();
    $http.get('/api/configuration/xmltvsources').success(function (result) {
        gui.finishWork();
        $scope.model = result;
    }).error(function () {
        gui.finishWork();
    });

    $scope.add = function () {
    };

    $scope.remove = function (item) {
        $scope.model.remove(item);
    };

    $scope.scan = function (item) {
    };

    $scope.import = function () {
        gui.doWork();
        $http.post('/api/configuration/XmlTvSourceImport').success(function (result) {
            gui.finishWork();
            if (!($scope.model || !Array.isArray($scope.model)))
                $scope.model = [];
            $.each(result, function (i, ele) {
                $scope.model.push(ele);
            });
        }).error(function () {
            gui.finishWork();
        });
    };

    $scope.save = function () {
        gui.doWork();
        $http.post('/api/configuration/xmltvsources', $scope.model).success(function (result) {
            gui.finishWork();
        }).error(function () {
            gui.finishWork();
        });
    };

};
ns.XmlTvController.$inject = ['$scope', '$http', '$rootScope'];
