var ns = namespace('Controllers.Configuration');

ns.ChannelController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $http.get('/api/channels/getshared').success(function (data) {
        $scope.model = data;
        console.log('channels');
        console.log(data);
    });

};
ns.ChannelController.$inject = ['$scope', '$http', '$rootScope'];
