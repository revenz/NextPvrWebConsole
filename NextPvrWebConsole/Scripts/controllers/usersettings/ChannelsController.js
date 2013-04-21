var ns = namespace('Controllers.UserSettings');

ns.ChannelsController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;
    
    $scope.channels = [];

    $scope.refresh = function () {
        $http.get('/api/channel').success(function (data) {
            $scope.channels = data;
        });
    };

    $scope.refresh();

};
ns.ChannelsController.$inject = ['$scope', '$http', '$rootScope'];
