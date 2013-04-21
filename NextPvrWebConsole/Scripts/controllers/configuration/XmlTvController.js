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
        console.log('got xmltv source');
        console.log(result.Channels);
        gui.finishWork();
        console.log(result);
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

    $scope.importFromNextPVR = function () {
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
            $rootScope.root.xmltvSources = result.slice();
        }).error(function () {
            gui.finishWork();
        });
    };

    $scope.add = function () {
        gui.fileBrowser({
            xmlFiles: true
        }).success(function (result) {

            if (!$scope.model.sources)
                $scope.model.sources = [];

            // check if already in list.
            for (var i = 0; i < $scope.model.sources.length; i++) {
                if (result.fullName.toLowerCase().trim() == $scope.model.sources[i].Filename.toLowerCase().trim()) {
                    gui.alert($.i18n._("XMLTV file already in list."));
                    return;
                }
            }

            $scope.$apply(function () {
                $scope.model.sources.push({
                    Oid: 0,
                    ShortName: result.shortName,
                    Filename: result.fullName
                });
            });
        });
    };

    $scope.remove = function (item) {
        gui.confirm({
            message: $.i18n._("Are you sure you want to remove the XMLTV file '%s'?", [item.ShortName]),
            yes: function () {
                $scope.$apply(function () {
                    $scope.model.sources.remove(item);
                });
            }
        });
    };

};
ns.XmlTvController.$inject = ['$scope', '$http', '$rootScope'];
