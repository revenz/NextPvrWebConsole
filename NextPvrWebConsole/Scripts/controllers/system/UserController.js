var ns = namespace('Controllers.System');

ns.UserController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $rootScope.getConfiguration(function (config) {
        $scope.model = config;
    });

};
ns.UserController.$inject = ['$scope', '$http', '$rootScope'];
