var ns = namespace('Controllers.Configuration');

ns.DeviceController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $http.get('/api/configuration/devices').success(function(data) {
        $scope.model = data;
        console.log('devices');
        console.log(data);
    });

};
ns.DeviceController.$inject = ['$scope', '$http', '$rootScope'];
