var ns = namespace('Controllers.Recordings');

ns.AvailableController = function ($scope, $http) {
    "use strict";
    var self = this;

    $scope.recordingGroups = [];
    $scope.selectedRecordingGroup = null;

    $scope.refresh = function () {
        $http.get('/api/recordings/available').success(function (data) {
            $scope.recordingGroups = data;
            $scope.selectedRecordingGroup = $scope.recordingGroups && $scope.recordingGroups.length > 0 ? $scope.recordingGroups[0] : null;
        });
    };

    $scope.openPlayer = function (recording) {
        window.open('/stream/recording/' + recording.OID, 'livestream', 'width=830,height=480,status=1,resizable=0');
    }

    $scope.delete = function (recording) {
        console.log(recording);
        gui.confirmMessage({
            message: $.i18n._("Are you sure you want to delete the recording '%s'?", [recording.Subtitle && recording.Subtitle.length ? recording.Subtitle : gui.formatDateShort(recording.StartTime)]),
            yes: function () {
                $scope.selectedRecordingGroup.Recordings.remove(recording);
                if ($scope.selectedRecordingGroup.Recordings.length == 0) {
                    $scope.recordingGroups.remove($scope.selectedRecordingGroup);
                    $scope.selectedRecordingGroup = $scope.recordingGroups && $scope.recordingGroups.length > 0 ? $scope.recordingGroups[0] : null;
                }
            }
        });
    };

    $scope.move = function () {
        console.log('todo');
    };

    $scope.refresh();


};
ns.AvailableController.$inject = ['$scope', '$http'];
