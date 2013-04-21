var ns = namespace('Controllers.Recordings');

ns.RecurringController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;


    $scope.recordings = [];

    $scope.refresh = function () {
        $http.get('/api/recordings/recurring').success(function (data) {
            $scope.recordings = data;
        });
    };

    $scope.deleteRecording = function (recording) {
        console.log(recording);
        gui.confirm({
            message: $.i18n._("Are you sure you want to delete the pending recording '%s'?", [recording.Title]),
            yes: function () {
                $http['delete']('/api/recordings/' + recording.OID).success(function (result) {
                    $scope.recordings.remove(recording);
                });
            }
        });
    };

    $scope.edit = function (recording) {
        console.log(recording);
        recording.RecordingType = recording.Type;
        $rootScope.openScheduleEditor(recording, function (result) {
            console.log(result);
            if (result) {
                recording.Type = result.RecordingType;
                recording.RecurrenceOid = result.recurrenceOID;
                recording.IsRecurring = result.recurrenceOID > 0;
                recording.RecordingOid = result.oid;
                recording.IsRecording = result.oid > 0;
            }
        });
    };

    $scope.refresh();

};
ns.RecurringController.$inject = ['$scope', '$http', '$rootScope'];
