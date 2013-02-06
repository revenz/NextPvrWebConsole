var ns = namespace('Controllers.Configuration');

ns.ChannelGroupController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $scope.model = { ChannelGroups: [] };

    $http.get('/api/channelgroups/getshared?LoadChannelOids=true').success(function (data) {
        $scope.model.ChannelGroups = data;
        console.log('channel groups');
        console.log(data);
    });

    $scope.create = function () {
        gui.promptMessage({
            title: $.i18n._('Create Channel Group'),
            message: $.i18n._('Type in the name of the channel group to create.'),
            success: function (name) {
                if (name.toLowerCase() == 'all channels') {
                    gui.showError($.i18n._("Cannot create a Channel Group 'All Channels' as it is reserved."));
                    return;
                }
                $scope.$apply(function () {
                    $scope.model.ChannelGroups.push({ Oid: 0, Name: name, OrderOid: -1, ChannelOids: [] });
                });
            }
        });
    };

    $scope.rename = function (item) {
        gui.promptMessage({
            title: $.i18n._('Rename Channel Group'),
            initialValue: item.Name,
            message: $.i18n._('Type in the new name of the channel group.'),
            success: function (name) {
                if (name.toLowerCase() == 'all channels') {
                    gui.showError($.i18n._("Cannot create a Channel Group 'All Channels' as it is reserved."));
                    return;
                }
                $scope.$apply(function () {
                    item.Name = name;
                });
            }
        });
    };

    $scope.save = function () {
        var orderoid = 0;
        $.each($scope.model.ChannelGroups, function (i, ele) {
            ele.OrderOid = orderoid++;
        });
        $http.post('Configuration/UpdateChannelGroups', $scope.model).success(function () {
        });
    };
};
ns.ChannelGroupController.$inject = ['$scope', '$http', '$rootScope'];
