var ns = namespace('Controllers');

ns.UpcomingController = function($scope, $http) {
    "use strict";

    $scope.items = {};

    $scope.refreshUpcoming = function () {
        $http.get('/api/recordings/getupcoming').success(function (data) {
            $scope.items = data;
        });
    };
    $scope.refreshUpcoming();

    $scope.cancel = function (item) {
        console.log(item);
    };
}
ns.UpcomingController.$inject = ['$scope', '$http'];
