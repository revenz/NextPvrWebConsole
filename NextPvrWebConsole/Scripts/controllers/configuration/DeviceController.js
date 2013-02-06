var ns = namespace('Controllers.Configuration');

ns.DeviceController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $scope.model = { Devices: {}, UseReverseOrderForLiveTv: false };
    
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
};
ns.DeviceController.$inject = ['$scope', '$http', '$rootScope'];
