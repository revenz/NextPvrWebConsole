var npvrapp = angular.module('nextpvr', []);

npvrapp.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/dashboard', { templateUrl: 'dashboard' })
                  .when('/guide', { templateUrl: 'guide', controller: 'Controllers.GuideController' })
                  //.when('/phones/:phoneId', { templateUrl: 'partials/phone-detail.html', controller: PhoneDetailCtrl })
                  .otherwise({ redirectTo: '/dashboard' });

} ]);

npvrapp.run(function ($rootScope, $http) {
    var self = this;
    // setup the watcher to set the 'active' item in the main menu
    $rootScope.$on("$routeChangeSuccess", function (current, previous) {
        if (previous && previous.templateUrl && previous.templateUrl.length) {
            var url = previous.templateUrl;
            if (url.substr(0, 1) == '/')
                url = url.substr(1);
            if (url.indexOf('/') > 0)
                url = url.substr(0, url.indexOf('/'));

            $('#mainmenu .active').removeClass('active');
            if (url.length)
                $('#mainmenu .' + url).addClass('active');
        }
    });

    $rootScope.$on('$viewContentLoaded', function () {
        translateElement($('#maincontent'));
    });

    $rootScope.recordingDirectories = [];
    self.scheduleEditorCallback = null;
    $rootScope.openScheduleEditor = function (listing, scheduleEditorCallback) {
        self.scheduleEditorCallback = scheduleEditorCallback;
        var fun = function () {
            if (!listing.RecordingDirectoryId)
                listing.RecordingDirectoryId = $rootScope.recordingDirectories[0].RecordingDirectoryId;
            if (listing.RecordingType < 1)
                listing.RecordingType = 1; // default of 'Record Once'
            $rootScope.scheduleEditorRecording = listing;
            $('#recording-options').modal({});
        };
        if ($rootScope.recordingDirectories == null || $rootScope.recordingDirectories.length < 1)
            $http.get('/api/recordingdirectories?IncludeShared=true').success(function (data) {
                $rootScope.recordingDirectories = data;
                fun();
            });
        else
            fun();
    };
    $rootScope.saveScheduleEditor = function () {
        var data = {
            Oid: $rootScope.scheduleEditorRecording.Oid,
            PrePadding: $rootScope.scheduleEditorRecording.PrePadding,
            PostPadding: $rootScope.scheduleEditorRecording.PostPadding,
            RecordingDirectoryId: $rootScope.scheduleEditorRecording.RecordingDirectoryId,
            NumberToKeep: $rootScope.scheduleEditorRecording.Keep,
            Type: $rootScope.scheduleEditorRecording.RecordingType
        };
        $http.post('/api/guide/record', data).success(function(result) {
            if (result && self.scheduleEditorCallback)
                self.scheduleEditorCallback(result);
        });
    };
});

npvrapp.directive('toggleBox', function () {
    return {
        restrict: 'E',
        scope: {
            modelvalue: '=ngModel',
            enabledText: '@tbEnabledText',
            disabledText: '@tbDisabledText',
            enabledStyle: '@tbEnabledStyle',
            disabledStyle: '@tbDisabledStyle',
            ngDisabled: '=ngDisabled',
            width: '@width',
        },
        template: '<div>' +
                    '<input type="checkbox">' +
                  '</div>',
        replace: true,
        require: 'ngModel',
        controller: function ($scope, $element) {

            $scope.$watch(function () { return $($element).children().length; }, function (newValue, oldValue) {
                console.log($scope.ngDisabled);
                if ($scope.ngDisabled) {
                    $($element).find('input').attr('disabled', 'disabled');
                }
                console.log('$scope.enabledText:' + $scope.enabledText);
                $scope.toggleButton = $($element).toggleButtons({
                    width: $scope.width && $scope.width.length ? $scope.width : 100,
                    label: {
                        enabled: $scope.enabledText && $scope.enabledText.length ? $scope.enabledText : 'ON',
                        disabled: $scope.disabledText && $scope.disabledText.length ? $scope.disabledText : 'OFF'
                    },
                    style: {
                        enabled: $scope.enabledStyle && $scope.enabledStyle.length ? $scope.enabledStyle : 'primary',
                        disabled: $scope.disabledStyle && $scope.disabledStyle.length ? $scope.disabledStyle : ''
                    },
                    onChange:function($el, status, e){
                        $scope.$apply(function () {
                            $scope.modelvalue = status;
                        });
                    }
                });
            });

            $scope.$watch(function () { return $scope.modelvalue; }, function (newValue, oldValue) {
                $($element).toggleButtons('setState', newValue, true);
            });
        }
    };
});
