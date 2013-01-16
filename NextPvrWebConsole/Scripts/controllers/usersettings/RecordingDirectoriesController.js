var ns = namespace('Controllers.UserSettings');

ns.RecordingDirectoriesController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $scope.directories = [];
    $scope.model = {
        defaultRecordingDirectoryIndex: 0
    };

    $scope.refresh = function () {
        $http.get('/api/recordingdirectories?IncludeShared=true').success(function (data) {
            console.log(data);
            $.each(data, function (i, ele) {
                if (ele.IsDefault)
                    $scope.model.defaultRecordingDirectoryIndex = i;
            });
            $scope.defaultRecordingDirectoryIndex = 2;
            $scope.directories = data;
        });
    };

    $scope.save = function () {
        var data = $scope.directories;
        $.each(data, function (i, ele) {
            ele.IsDefault = $scope.model.defaultRecordingDirectoryIndex == i;
        });

        $http.post('/api/recordingdirectories', data).success(function () {
            console.log('directories saved!');
        });
    };

    $scope.refresh();

};
ns.RecordingDirectoriesController.$inject = ['$scope', '$http', '$rootScope'];
