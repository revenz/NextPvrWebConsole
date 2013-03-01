var npvrapp = angular.module('nextpvr', []);

npvrapp.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/dashboard', { templateUrl: 'dashboard' })
                  .when('/guide', { templateUrl: 'guide', controller: 'Controllers.GuideController' })
                  .when('/recordings', { templateUrl: 'recordings', controller: 'Controllers.General.TabController' })
                  .when('/usersettings', { templateUrl: 'usersettings', controller: 'Controllers.General.TabController' })
                  .when('/configuration', { templateUrl: 'configuration', controller: 'Controllers.General.TabController' })
                  .when('/system', { templateUrl: 'system', controller: 'Controllers.General.TabController' })
                  .when('/system/log/:oid/:name', { templateUrl: 'system/log', controller: 'Controllers.System.LogViewController' })
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


    $rootScope.configuration = null;    
    $rootScope.getConfiguration = function (callback) {
        if ($rootScope.configuration == null) {
            $http.get('/api/configuration').success(function (data) {
                $rootScope.configuration = data;
                callback($rootScope.configuration);
            });
        } else {
            callback($rootScope.configuration);
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
