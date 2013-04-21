var ns = namespace('Controllers.Recordings');

ns.PendingController = function ($scope, $http) {
    "use strict";
    var self = this;

    $scope.recordings = [];

    $scope.refresh = function () {
        $http.get('/api/recordings/pending').success(function (data) {
            $scope.recordings = data;
        });
    };

    $scope.cancel = function (recording) {
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
    
    $scope.refresh();

};
ns.PendingController.$inject = ['$scope', '$http'];
