var ns = namespace('Controllers.System');

ns.LogController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $rootScope.getConfiguration(function (config) {
        $scope.model = config;
    });

};
ns.LogController.$inject = ['$scope', '$http', '$rootScope'];
