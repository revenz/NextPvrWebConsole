var ns = namespace('Controllers.System');

ns.UserController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $http.get('/api/users').success(function (data) {
        $scope.model = data;
    });

};
ns.UserController.$inject = ['$scope', '$http', '$rootScope'];
