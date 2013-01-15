﻿var npvrapp = angular.module('nextpvr', []);

npvrapp.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/dashboard', { templateUrl: 'dashboard' })
                  .when('/guide', { templateUrl: 'guide', controller: 'Controllers.GuideController' })
                  //.when('/recordings/available', { templateUrl: 'recordings', controller: 'Controllers.Recordings.AvailableController' })
                  //.when('/recordings/pending', { templateUrl: 'recordings', controller: 'Controllers.Recordings.PendingController' })
                  //.when('/recordings/recurring', { templateUrl: 'recordings', controller: 'Controllers.Recordings.RecurringController' })
                  //.when('/recordings/:tabid', { templateUrl: 'recordings' })
                  .when('/recordings', { templateUrl: 'recordings', controller: 'Controllers.General.TabController' })
                  //.when('/recordings', { redirectTo: '/recordings/available' })
                  //.when('/phones/:phoneId', { templateUrl: 'partials/phone-detail.html', controller: PhoneDetailCtrl })
                  .otherwise({ redirectTo: '/dashboard' });

} ]);

npvrapp.run(function ($rootScope, $http, $location) {
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
    $rootScope.openScheduleEditor = function (input, scheduleEditorCallback) {
        var isListing = input.ObjectType.endsWith('EpgListing');
        var isRecurring = input.ObjectType.endsWith('RecurringRecording');
        var data = {
            RecordingOid: isListing || input.RecordingOid ? input.RecordingOid : input.Oid,
            RecurrenceOid: isRecurring ? input.Oid : input.RecurrenceOid,
            EpgEventOid: isListing ? input.Oid : 0,
            Title: input.Title ? input.Title : input.Name,
            ChannelName: input.ChannelName,
            Type: input.RecordingType ? input.RecordingType : input.Type,
            PrePadding: input.PrePadding,
            PostPadding: input.PostPadding,
            RecordingDirectoryId: input.RecordingDirectoryId,
            NumberToKeep: input.Keep ? input.Keep : 0
        };
        if (!data.Type || data.Type < 1)
            data.Type = 1; // default of 'Record Once'
        console.log('input');
        console.log(input);
        console.log('data');
        console.log(data);


        self.scheduleEditorCallback = scheduleEditorCallback;
        var fun = function () {
            if (!data.RecordingDirectoryId)
                data.RecordingDirectoryId = $rootScope.recordingDirectories[0].RecordingDirectoryId;
            $rootScope.scheduleEditorRecording = data;
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
        $http.post('/api/recordings/saverecording', $rootScope.scheduleEditorRecording).success(function (result) {
            if (result && self.scheduleEditorCallback)
                self.scheduleEditorCallback(result);
        });
    };

    $rootScope.tabSelected = function (address) {
        return $location.$$path.indexOf(address) > 0 ? 'selected' : '';
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
