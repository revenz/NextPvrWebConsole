var ns = namespace('Controllers.System');

ns.GeneralController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $rootScope.getConfiguration(function (config) {
        $scope.model = config;
    });

};
ns.GeneralController.$inject = ['$scope', '$http', '$rootScope'];
