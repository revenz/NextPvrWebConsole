var ns = namespace('Controllers.Recordings');

ns.RecurringController = function ($scope, $http) {
    "use strict";
    var self = this;


    $scope.recordings = [];

    $scope.refresh = function () {
        $http.get('/api/recordings/recurring').success(function (data) {
            $scope.recordings = data;
        });
    };

    $scope.delete = function (recording) {
        console.log(recording);
        gui.confirmMessage({
            message: $.i18n._("Are you sure you want to delete the pending recording '%s'?", [recording.Title]),
            yes: function () {
                $http.delete('/api/recordings/' + recording.OID).success(function (result) {
                    $scope.recordings.remove(recording);
                });
            }
        });
    };

    $scope.refresh();

};
ns.RecurringController.$inject = ['$scope', '$http'];
