var ns = namespace('Controllers.Configuration');

ns.RecordingController = function ($scope, $http, $rootScope) {
    "use strict";
    var self = this;

    $scope.model  = {
        PrePadding: 2,
        PostPadding: 5,
        BlockShutDownWhileRecording: true,
        RecurringMatch: 0,
        AvoidDuplicateRecordings: false,
        RecordingDirectories: [],
        defaultRecordingDirectoryIndex: 0
    };

    $rootScope.getConfiguration(function (config) {
        $scope.model.PrePadding = config.PrePadding;
        $scope.model.PostPadding = config.PostPadding;
        $scope.model.BlockShutDownWhileRecording = config.BlockShutDownWhileRecording;
        $scope.model.RecurringMatch = config.RecurringMatch;
        $scope.model.AvoidDuplicateRecordings = config.AvoidDuplicateRecordings;
    });

    $http.get('/api/recordingdirectories/getshared').success(function (data) {
        $scope.model.RecordingDirectories = data;
        for(var i=0; i<data.length;i++) {
            if (data[i].IsDefault)
                $scope.model.defaultRecordingDirectoryIndex = i;
        }
    });

    $scope.save = function () {
        if ($scope.model.RecordingDirectories.length == 0) {
            gui.showError($.i18n._('At least one recording directory is required.'));
            return;
        }

        for(var i=0; i < $scope.model.RecordingDirectories.length;i++)
            $scope.model.RecordingDirectories[i].IsDefault = i == $scope.model.defaultRecordingDirectoryIndex;

        $http.post('Configuration/UpdateRecording', $scope.model).success(function (result) {
        });
    };

    $scope.removeRecordingDirectory = function (rd) {
        gui.confirmMessage({
            message: $.i18n._("Are you sure you want to remove the '%s' recording directory?", [ rd.Name ]),
            yes: function () {
                $scope.$apply(function () {
                    $scope.model.RecordingDirectories.remove(rd);
                });
            }
        });
    };

    $scope.addRecordingDirectory = function () {

        // todo, get folder browser dialog input
        gui.folderBrowser({
            selected: function (path) {
                console.log('path: ' + path);
                gui.promptMessage({
                    title: DIRECTORY_CREATE_TITLE,
                    message: DIRECTORY_CREATE_MESSAGE,
                    validationMessage: DIRECTORY_ERROR_MESSAGE,
                    validationExpression: DIRECTORY_REGULAR_EXPRSESION,
                    success: function (name) {
                        $scope.$apply(function () {
                            $scope.model.RecordingDirectories.push({ Oid: 0, Name: name, Path: path, IsDefault: false });
                        });
                    }
                });
            }
        });
    };

};
ns.RecordingController.$inject = ['$scope', '$http', '$rootScope'];
