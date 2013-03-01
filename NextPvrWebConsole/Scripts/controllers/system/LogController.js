var ns = namespace('Controllers.System');

ns.LogController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $http.get('/api/logs').success(function (data) {
        $scope.model = data;
    });

};
ns.LogController.$inject = ['$scope', '$http', '$rootScope'];

ns.LogViewController = function ($scope, $http, $routeParams) {
    "use strict";
    var self = this;

    $http.get('/api/logs/log?oid=' + encodeURI($routeParams['oid']) + '&name=' + encodeURI($routeParams['name'])).success(function (data) {
        $scope.model = data;
        console.log(data);
    });
};
ns.LogViewController.$inject = ['$scope', '$http', '$routeParams'];
