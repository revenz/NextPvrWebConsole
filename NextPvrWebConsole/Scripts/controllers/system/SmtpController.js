var ns = namespace('Controllers.System');

ns.SmtpController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $rootScope.getConfiguration(function (config) {
        $scope.model = config;
    });

};
ns.SmtpController.$inject = ['$scope', '$http', '$rootScope'];
