var ns = namespace('Controllers.Configuration');

ns.ChannelController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $scope.model = {
        channels: []
    };

    gui.doWork();
    $http.get('/api/channel/getConfigurationChannels').success(function (data) {
        gui.finishWork();
        $scope.model.channels = data;
    }).error(function () {
        gui.finishWork();
    });

    $scope.emptyEpg = function () {
        $http.get('/api/channel/emptyEpg').success(function (result) {
            console.log('empty epg result: ' + result);
        });
    };

    $scope.updateEpg = function () {
        $http.get('/api/channel/updateEpg').success(function (result) {
            console.log('update epg result: ' + result);
        });
    };

    $scope.getTvSource = function (epgSource) {
        if (epgSource.startsWith('XMLTV-')) {
            var index = parseInt(epgSource.substr(6), 10);
            if (!isNaN(index) && $rootScope.root.xmltvSources) {
                for (var i = 0; i < $rootScope.root.xmltvSources.length; i++) {
                    if ($rootScope.root.xmltvSources[i].Oid == index)
                        return $rootScope.root.xmltvSources[i].Channels
                }
            }
        } else if (epgSource.startsWith('SD-')) {
            // schedule direct
        }
        return null;
    };

    $scope.import = function () {
        $http.get('/api/channel/importMissing').success(function (result) {
            var knownOids = [];
            $.each($scope.model.channels, function (i, ele) {
                knownOids.push(ele.Oid);
            });
            $.each(result, function (i, ele) {
                if ($.inArray(ele.Oid, knownOids) < 0)
                    $scope.model.channels.push(ele);
            });
        });
    };

    $scope.epgSourceSelected = function (source) {
        if (!source.EpgSource.startsWith('XMLTV'))
            return;
        var xmltv = $scope.getXmlTvSource(source.EpgSource);
        if (xmltv == null)
            return;

        // look for the source.
        for (var i = 0; i < xmltv.Channels.length; i++) {
            if (xmltv.Channels[i].Name.toLowerCase().trim() == source.Name.toLowerCase().trim()
                || 
                xmltv.Channels[i].Oid.toLowerCase().trim() == source.Name.toLowerCase().trim()
                ) {
                source.XmlTvChannel = xmltv.Channels[i].Oid;
                break;
            }
        }
    };

    $scope.remove = function (item) {
        gui.confirm({
            message: $.i18n._("Are you sure you want to remove the channel '%s'?", [item.Name]),
            yes: function () {
                $scope.$apply(function () {
                    $scope.model.channels.remove(item);
                });
            }
        });
    };

    $scope.save = function () {
        if ($scope.model.channels == null)
            return;
        
        for (var i = 0; i < $scope.model.channels.length; i++) {
            if (String.isNullOrWhitespace($scope.model.channels[i].Name)) {
                gui.alert($.i18n._('Channel name missing'), $.i18n._('Error'));
                return;
            }
            if (!$scope.model.channels[i].Number || isNaN($scope.model.channels[i].Number)) {
                gui.alert($.i18n._('Channel number invalid'), $.i18n._('Error'));
                return;
            }
        }

        gui.doWork();
        $http.post('/api/channel/updateShared', $scope.model.channels).success(function (result) {
            gui.finishWork();
        }).error(function () {
            gui.finishWork();
        });
    };

    $scope.isEpgSourceSelected = function (channel, sourceName) {
        return channel.EpgSource == sourceName;
    };
};
ns.ChannelController.$inject = ['$scope', '$http', '$rootScope'];
