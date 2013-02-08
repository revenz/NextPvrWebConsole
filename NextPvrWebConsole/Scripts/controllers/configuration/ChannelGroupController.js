var ns = namespace('Controllers.Configuration');

ns.ChannelGroupController = function ($scope, $http, $rootScope, $compile) {
    "use strict";
    var self = this;

    $scope.model = { ChannelGroups: [] };

    $http.get('/api/channelgroups/getshared?LoadChannelOids=true').success(function (data) {
        $scope.model.ChannelGroups = data;
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

    $scope.remove = function (item) {
        gui.confirmMessage({
            message: $.i18n._("Are you sure you want to remove the Channel Group '%s'?", [item.Name]),
            yes: function () {
                $scope.$apply(function () {
                    $scope.model.ChannelGroups.remove(item);
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

    $scope.channels = null;

    $scope.selectChannels = function (group) {

        if (!$scope.channels) {
            $http.get('/api/channels/getshared').success(function (data) {
                $scope.channels = data;
                $scope.selectChannels(group);
            });
        } else {
            var channelData = [];
            $.each($scope.channels, function (i, ele) {
                channelData.push($.extend({ Active: $.inArray(ele.Oid, group.ChannelOids) >= 0 }, ele));
            });
            new ChannelSelector(channelData, $rootScope.$new(), $http, $compile).success(function (data) {
                var channelOids = [];
                $.each(data, function (i, ele) {
                    if (ele.Active)
                        channelOids.push(ele.Oid);
                });
                group.ChannelOids = channelOids;
            });
        }
    };
};
ns.ChannelGroupController.$inject = ['$scope', '$http', '$rootScope', '$compile'];
