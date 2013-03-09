var ns = namespace('Controllers.Configuration');

ns.XmlTvController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $scope.model = {
        fileBrowserResult: null,
        sources: []
    };

    gui.doWork();
    $http.get('/api/configuration/xmltvsources').success(function (result) {
        gui.finishWork();
        $.each(result, function (i, ele) {
            if (ele.LastScanTime && !ele.LastScanTime.getFullYear)
                ele.LastScanTime = new Date(ele.LastScanTime);
        });
        $scope.model.sources = result;
    }).error(function () {
        gui.finishWork();
    });
    $scope.scan = function (item) {
        gui.doWork();
        $http.post('/api/configuration/XmlTvSourceScan/' + item.Oid).success(function (result) {
            gui.finishWork();
            if (result.LastScanTime && !result.LastScanTime.getFullYear)
                result.LastScanTime = new Date(result.LastScanTime);
            $.extend(item, result);
        }).error(function () {
            gui.finishWork();
        });
    };

    $scope.import = function () {
        gui.doWork();
        $http.post('/api/configuration/XmlTvSourceImport').success(function (result) {
            gui.finishWork();
            if (!($scope.model || !Array.isArray($scope.model)))
                $scope.model = [];
            $.each(result, function (i, ele) {
                if (ele.LastScanTime && !ele.LastScanTime.getFullYear)
                    ele.LastScanTime = new Date(ele.LastScanTime);
                $scope.model.push(ele);
            });
        }).error(function () {
            gui.finishWork();
        });
    };

    $scope.save = function () {
        gui.doWork();
        $http.post('/api/configuration/xmltvsources', $scope.model.sources).success(function (result) {
            gui.finishWork();
            $.each(result, function (i, ele) {
                if (ele.LastScanTime && !ele.LastScanTime.getFullYear)
                    ele.LastScanTime = new Date(ele.LastScanTime);
            });
            $scope.model.sources = result;
        }).error(function () {
            gui.finishWork();
        });
    };

    $scope.add = function () {
        gui.fileBrowser({
            xmlFiles: true
        }).success(function (result) {

            // TODO: check if already in list.

            $scope.$apply(function () {
                if (!$scope.model.sources)
                    $scope.model.sources = [];
                $scope.model.sources.push({
                    Oid: 0,
                    ShortName: result.shortName,
                    Filename: result.fullName
                });
            });
        });
    };

    $scope.remove = function (item) {
        // TODO: confirm removal
        $scope.model.sources.remove(item);
    };

};
ns.XmlTvController.$inject = ['$scope', '$http', '$rootScope'];
