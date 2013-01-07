function DashboardController($scope, $http) {

    $scope.upcomingRecordings = {};

    $scope.refreshUpcoming = function () {
        $http.get('/api/recordings/getupcoming').success(function (data) {
            $scope.upcomingRecordings = data;
        });
    };
    $scope.refreshUpcoming();
}
DashboardController.$inject = ['$scope', '$http'];