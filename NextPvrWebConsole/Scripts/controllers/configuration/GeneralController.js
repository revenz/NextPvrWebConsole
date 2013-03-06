var ns = namespace('Controllers.Configuration');

ns.GeneralController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $scope.model = {
        EpgUpdateHour: 0
    };

    $rootScope.getConfiguration(function (config) {
        $scope.model = config;
    });

    $scope.save = function () {
        
        if (!$('#frmConfigGeneral').valid())
            return;

        $http.post('/configuration/updategeneral',
            {
                LiveTvBufferDirectory: $scope.model.LiveTvBufferDirectory,
                UpdateDvbEpgDuringLiveTv: $scope.model.UpdateDvbEpgDuringLiveTv,
                EpgUpdateHour: $scope.model.EpgUpdateHour,
                EnableUserSupport: $scope.model.EnableUserSupport,
                UserBaseRecordingDirectory: $scope.model.UserBaseRecordingDirectory
            }).success(function (data) {

            });
    };

};
ns.GeneralController.$inject = ['$scope', '$http', '$rootScope'];
