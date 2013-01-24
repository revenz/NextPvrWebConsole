var ns = namespace('Controllers.UserSettings');

ns.ChannelGroupsController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $scope.channels = [];

    $scope.refresh = function () {
        $http.get('/api/channelgroups').success(function (data) {
            console.log(data);
            $scope.channelgroups = data;
        });
    };

    $scope.add = function () {
        $scope.channelgroups.push({
            Oid: -1,
            Name: "",
            OrderOid: -1,
            UserOid: -1,
            Enabled: true,
            ParentOid: 0,
            ChannelOids: null,
            IsShared: false
        });
    };

    $scope.remove = function (item) {
        $scope.channelgroups.remove(item);
    }

    $scope.refresh();

};
ns.ChannelGroupsController.$inject = ['$scope', '$http', '$rootScope'];
