var ns = namespace('Controllers.Configuration');

ns.DeviceController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $scope.model = {
        Devices: {},
        UseReverseOrderForLiveTv: false,
        IsScanning: false,
        ScanningStatus: ''
    };
    
    $rootScope.getConfiguration(function (config) {
        $scope.model.UseReverseOrderForLiveTv = config.UseReverseOrderForLiveTv;
    });

    $http.get('/api/configuration/devices').success(function(data) {
        $scope.model.Devices = data;
    });

    $scope.save = function () {
        var priority = 1;
        $.each($scope.model.Devices, function (i, ele) {
            ele.Priority = priority++;
        });
        $http.post('Configuration/UpdateDevices', $scope.model).success(function (result) {
        });
    };

    $scope.scan = function (oid) {
        var scanOid = '';
        $http.get('/api/device/scan/' + oid).success(function (result) {
            console.log('result: ' + result);
            scanOid = result;
            $scope.model.ScanningStatus = 'Scanning';
            $scope.model.IsScanning = true;
            setInterval(function () {
                $http.post('/api/device/scanStatus?oid=' + scanOid).success(function (result) {
                    $scope.model.ScanningStatus = result;
                });
            }, 10 * 1000);
        });
    };
};
ns.DeviceController.$inject = ['$scope', '$http', '$rootScope'];
