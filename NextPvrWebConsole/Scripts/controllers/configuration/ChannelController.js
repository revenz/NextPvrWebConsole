var ns = namespace('Controllers.Configuration');

ns.ChannelController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    gui.doWork();
    $http.get('/api/channel/getConfigurationChannels').success(function (data) {
        gui.finishWork();
        $scope.model = data;
    }).error(function () {
        gui.finishWork();
    });

    gui.doWork();
    $http.get('/api/configuration/xmltvsources').success(function (result) {
        gui.finishWork();
        $scope.XmlTvSources = result;
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

    $scope.getXmlTvSource = function (epgSource) {
        var index = parseInt(epgSource.substr(6), 10);
        if (!isNaN(index) && $scope.XmlTvSources && $scope.XmlTvSources.length > index)
            return $scope.XmlTvSources[index];
        return null;
    };

    $scope.import = function () {
        $http.get('/api/channel/importMissing').success(function (result) {
            var knownOids = [];
            $.each($scope.model, function (i, ele) {
                knownOids.push(ele.Oid);
            });
            $.each(result, function (i, ele) {
                if ($.inArray(ele.Oid, knownOids) < 0)
                    $scope.model.push(ele);
            });
        });
    };

    $scope.epgSourceSelected = function (source) {
        if (!source.EpgSource.startsWith('XMLTV'))
            return;
        var xmltv = $scope.getXmlTvSource(source.EpgSource);
        if (xmltv == null)
            return;
        if (String.isNullOrWhitespace(source.XmlTvChannel)) {
            // look for the source.
            for (var i = 0; i < xmltv.ChannelOids.length; i++) {
                if (xmltv.ChannelOids[i].toLowerCase().trim() == source.Name.toLowerCase().trim()) {
                    source.XmlTvChannel = xmltv.ChannelOids[i];
                    break;
                }
            }
        }
    };

    $scope.remove = function (item) {
        gui.confirm({
            message: $.i18n._("Are you sure you want to remove the channel '%s'?", [item.Name]),
            yes: function () {
                $scope.$apply(function () {
                    $scope.model.remove(item);
                });
            }
        });
    };

    $scope.save = function () {
        if ($scope.model == null)
            return;
        
        for (var i = 0; i < $scope.model.length; i++) {
            if (String.isNullOrWhitespace($scope.model[i].Name)) {
                gui.showMessageBox($.i18n._('Channel name missing'), $.i18n._('Error'));
                return;
            }
            if (!$scope.model[i].Number || isNaN($scope.model[i].Number)) {
                gui.showMessageBox($.i18n._('Channel number invalid'), $.i18n._('Error'));
                return;
            }
        }

        gui.doWork();
        $http.post('/api/channel/updateShared', $scope.model).success(function (result) {
            gui.finishWork();
        }).error(function () {
            gui.finishWork();
        });
    };
};
ns.ChannelController.$inject = ['$scope', '$http', '$rootScope'];
