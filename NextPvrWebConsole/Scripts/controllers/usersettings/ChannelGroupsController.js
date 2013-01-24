var ns = namespace('Controllers.UserSettings');

ns.ChannelGroupsController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $scope.channelgroups = [];
    $scope.channels = [];

    $scope.refresh = function () {
        $http.get('/api/channelgroups').success(function (data) {
            console.log(data);
            $scope.channelgroups = data;
        });
    };

    $scope.add = function () {
        $scope.edit({
            Oid: 0,
            Name: "",
            OrderOid: -1,
            UserOid: -1,
            Enabled: true,
            ParentOid: 0,
            ChannelOids: null,
            IsShared: false
        });
    };

    $scope.edit = function (item) {
        $scope.selected = item;
        if (!$scope.selected.Channels) {
            $http.get('/api/channelgroups/channels/' + item.Oid + '?onlyenabled=false').success(function (data) {
                $scope.selected.Channels = data;

                // not really the angular way...
                $('#channel-group-editor').modal();
            });
        } else {
            $('#channel-group-editor').modal();
        }
    };

    $scope.updateGroup = function () {
        if ($scope.selected == null)
            return;
        if (!$scope.selected.Name || !$scope.selected.Name.trim().length)
            return;
        $scope.selected.Name = $scope.selected.Name.trim();

        // check for duplicates
        var duplicate = false;
        $.each($scope.channelgroups, function (i, ele) {
            if (ele != $scope.selected && ele.Name.toLowerCase() == $scope.selected.Name.toLowerCase())
                duplicate = true;
        });

        if (duplicate)
            return;

        if ($scope.channelgroups.indexOf($scope.selected) < 0)
            $scope.channelgroups.push($scope.selected);
        // not really the angular way...
        $('#channel-group-editor').modal('hide');
    };

    $scope.remove = function (item) {
        $scope.channelgroups.remove(item);
    }

    $scope.refresh();

};
ns.ChannelGroupsController.$inject = ['$scope', '$http', '$rootScope'];
