var npvrapp = angular.module('nextpvr', []);

npvrapp.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/dashboard', { templateUrl: 'dashboard' })
                  .when('/guide', { templateUrl: 'guide', controller: 'Controllers.GuideController' })
                  .when('/recordings', { templateUrl: 'recordings', controller: 'Controllers.General.TabController' })
                  .when('/usersettings', { templateUrl: 'usersettings', controller: 'Controllers.General.TabController' })
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

npvrapp.filter('daymask', function () {
    return function (input) {
        var mask = parseInt(input, 10);
        switch (mask) {
            case 255: return $.i18n._('Daily');
            case 65: return $.i18n._('Weekends');
            case 62: return $.i18n._('Weekdays');
            case 1: return $.i18n._('Sundays');
            case 2: return $.i18n._('Mondays');
            case 4: return $.i18n._('Tuesdays');
            case 8: return $.i18n._('Wednesdays');
            case 16: return $.i18n._('Thursdays');
            case 32: return $.i18n._('Fridays');
            case 65: return $.i18n._('Saturdays');
            default:
                {
                    var t = [];
                    if ((self.mask & 1) == 1) t.push('Sun');
                    if ((self.mask & 2) == 2) t.push('Mon');
                    if ((self.mask & 4) == 4) t.push('Tue');
                    if ((self.mask & 8) == 8) t.push('Wed');
                    if ((self.mask & 16) == 16) t.push('Thu');
                    if ((self.mask & 32) == 32) t.push('Fri');
                    if ((self.mask & 64) == 64) t.push('Sat');
                    return t.join();
                }
        }
        return input;
    };
});

npvrapp.directive("ngSortable", function () {
    return {
        link: function (scope, element, attrs) {
            var handle = null;
            if (element.attr('data-handle'))
                handle = element.attr('data-handle');
            element.sortable({
                handle: handle,
                update: function (event, ui) {
                    var model = scope.$eval(attrs.ngSortable);
                    
                    var newArray = [];
                    // loop through items in new order
                    element.children().each(function (index) {
                        var oldIndex = parseInt($(this).attr("ng-sortable-index"), 10);
                        newArray.push(model[oldIndex]);
                    });

                    for (var i = 0; i < newArray.length; i++) {
                        model[i] = newArray[i];
                    }

                    scope.$apply();
                }
            });
        }
    };
});