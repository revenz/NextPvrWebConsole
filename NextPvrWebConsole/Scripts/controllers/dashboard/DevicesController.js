var ns = namespace('Controllers');

ns.DevicesController = function($scope, $http) {
    "use strict";

    $scope.items = {};

    $scope.refresh = function () {
        $http.get('/api/devices').success(function (data) {
            $scope.items = data;
        });
    };
    $scope.refresh();

    $scope.stop = function (stream) {
        $http.delete('api/devices/deleteStream?handle=' + stream.Handle).success(function(result){
            console.log('cancel result: ' + result);
            $scope.refresh();
        });
    };

    /* signalR code, may remove
    npvrevent.deviceStatusUpdated = function (events) {
        $.each(events, function (i, ele) {
            gui.showInfo(ele.Message, ele.CodeString);
        });
        refreshDevices();
    };
    */
}
ns.DevicesController.$inject = ['$scope', '$http'];