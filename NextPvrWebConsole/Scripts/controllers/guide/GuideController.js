var ns = namespace('Controllers');

ns.GuideController = function ($scope, $http, $compile, $rootScope) {
    "use strict";
    var self = this;
    var minuteWidth = 5;

    var today = new Date(new Date().setHours(0, 0, 0, 0));

    $scope.channels = [];
    $scope.selectedShow = null;
    $scope.selectedListing = null;
    $scope.guidehtml = 'loading please wait.';
    $scope.isLoaded = true;
    $scope.days = [];
    $scope.recording = null;
    for (var i = 0; i < 7; i++) {
        var date = new Date(new Date().setHours(0, 0, 0, 0));
        $scope.days.push(new Date(date.setDate(date.getDate() + i)));
    }
    $scope.selectedDateIndex = 0;
    self.guideStart = $scope.days[$scope.selectedDateIndex];
    $scope.channelGroups = [];
    $scope.selectedGroupIndex = -1;

    $scope.$watch(function () { return $scope.channelGroups.length > 0; }, function (newValue, oldValue) {
        $scope.isLoaded = newValue;
        $scope.selectedGroupIndex = 0;
    });
    $scope.$watch(function () { return $scope.selectedGroupIndex + '|' + $scope.selectedDateIndex + '|' + $scope.isLoaded }, function (newValue, oldValue) {
        if ($scope.isLoaded)
            $scope.refreshEpgData();
    });

    $http.get('/api/channelgroups').success(function (data) {
        $scope.channelGroups = [{ Name: 'All Channels', Key: ''}];
        for (var i = 0; i < data.length; i++)
            $scope.channelGroups.push({ Name: data[i].Name, Key: data[i].Name });
    });

    $scope.nextChannelGroup = function () {
        if ($scope.channelGroups.length < 2)
            return;
        if (++$scope.selectedGroupIndex >= $scope.channelGroups.length)
            $scope.selectedGroupIndex = 0;
    };
    $scope.prevChannelGroup = function () {
        if ($scope.channelGroups.length < 2)
            return;
        if (--$scope.selectedGroupIndex < 0)
            $scope.selectedGroupIndex = $scope.channelGroups.length - 1;
    };
    $scope.isSelectedGroup = function (index) {
        return $scope.selectedGroupIndex == index ? 'selected' : '';
    };
    $scope.isSelectedDay = function ($index) {
        return $index == $scope.selectedDateIndex ? 'selected' : '';
    };
    $scope.changeDay = function (index) {
        $scope.selectedDateIndex = index;
    };
    $scope.todaySelected = function () {
        return $scope.days[$scope.selectedDateIndex].getTime() == new Date(new Date().setHours(0, 0, 0, 0)).getTime();
    }
    $scope.getMinutesFromStartOfGuide = function (time) {
        var diff = time - self.guideStart;
        var minutes = Math.floor((diff / 1000) / 60);
        return minutes;
    };
    $scope.refreshEpgData = function () {
        $scope.isLoading = true;
        gui.doWork();
        // add random to stop IE from caching the guide data
        console.log('about to get guide data: ' + $scope.days[$scope.selectedDateIndex]);
        //$http.get('/guide/epg?date=' + $.format.date($scope.days[$scope.selectedDateIndex], 'yyyy-MM-dd') + '&group=' + encodeURIComponent($scope.channelGroups[$scope.selectedGroupIndex].Key) + '&rand=' + Math.random()).success(function (result) {
        $http.get('/api/guide?date=' + $.format.date($scope.days[$scope.selectedDateIndex], 'yyyy-MM-dd') + '&group=' + encodeURIComponent($scope.channelGroups[$scope.selectedGroupIndex].Key) + '&rand=' + Math.random()).success(function (result) {
            // need to compile it to hookup the angular events.
            //var element = $compile(result)($scope);
            // set the guide html to the element so its shown on the page
            //$scope.guidehtml = element;
           
            self.guideStart = $scope.days[$scope.selectedDateIndex];

            $scope.channels = result;

            $scope.initEpgGrid();

            gui.finishWork();
            $scope.isLoading = false;
        });
    };
    $scope.openListing = function (listing) {
        $http.get('api/guide/epglisting/' + listing.Oid).success(function (data) {
            console.log(data);
            listing.ChannelNumber = data.ChannelNumber;
            listing.ChannelName = data.ChannelName;
            listing.ChannelHasIcon = data.ChannelHasIcon;
            $scope.selectedListing = listing;
            $scope.selectedShow = data;

            $('#show-info-dialog').modal({});
        });
    };
    $scope.liveStream = function (oid) {
        window.open('/stream/' + oid, 'livestream', 'width=830,height=480,status=1,resizable=0');
    };
    $scope.channelIconSrc = function (selectedShow) {
        return selectedShow != null && selectedShow.ChannelHasIcon ? '/channelicon/' + selectedShow.ChannelOid : '';
    };
    $scope.openRecordingEditor = function () {
        $rootScope.openScheduleEditor($scope.selectedListing, function (result) {
            if (result) {
                $scope.selectedListing.RecurrenceOid = result.recurrenceOid;
                $scope.selectedListing.IsRecurring = result.recurrenceOid > 0;
                $scope.selectedListing.RecordingOid = result.recordingOid;
                $scope.selectedListing.IsRecording = result.recordingOid > 0;
            }
        });
    };
    $scope.quickRecord = function () {
        $http.post('/api/guide/quickrecord?oid=' + $scope.selectedListing.Oid).success(function (result) {
            if (result) {
                $scope.selectedListing.RecurrenceOid = result.recurrenceOid;
                $scope.selectedListing.IsRecurring = result.recurrenceOid > 0;
                $scope.selectedListing.RecordingOid = result.recordingOid;
                $scope.selectedListing.IsRecording = result.recordingOid > 0;
            }
        });
    };

    $scope.cancelRecurring = function () {
        gui.confirm({            
            message: $.i18n._("Are you sure you want to cancel the series '%s'?", [$scope.selectedListing.Title]),
            yes: function () {
                $http.delete('/api/recordings/deleterecurring/' + $scope.selectedListing.RecurrenceOid).success(function (data) {
                    if (data) {
                        $scope.selectedListing.RecurrenceOid = 0;
                        $scope.selectedListing.IsRecurring = true;
                        $scope.selectedListing.RecordingOid = 0;
                        $scope.selectedListing.IsRecording = false;
                    }
                });
            }
        });
    };
    $scope.cancelRecording = function () {        
        gui.confirm({            
            message: $.i18n._("Are you sure you want to cancel the recording '%s'?", [$scope.selectedListing.Title]),
            yes: function () {
                $http.delete('/api/recordings/' + $scope.selectedListing.RecordingOid).success(function (data) {
                    if (data) {
                        $scope.selectedListing.RecordingOid = 0;
                        $scope.selectedListing.IsRecording = false;
                    }
                });
            }
        });
    };

    // styling, maybe move out of here, not really the angular way.
    $scope.listingCss = function (listing) {
        var guideEnd = new Date(self.guideStart);
        guideEnd.setDate(guideEnd.getDate() + 1);
        var _class = '';
        if (listing.StartTime < self.guideStart) {
            _class += "pre-guide-start ";
        }
        if (listing.EndTime > guideEnd) {
            _class += "post-guide-end ";
        }
        if (listing.IsRecording) {
            _class += "recording ";
        }
        return _class;
    };
    $scope.listingStyle = function (listing) {
        var guideStart = self.guideStart;
        var guideEnd = new Date(guideStart);
        guideEnd.setDate(guideEnd.getDate() + 1);

        var dEnd = listing.EndTime;
        var dStart = listing.StartTime;

        if (listing.EndTime > guideEnd)
            dEnd = guideEnd; // ends before end of day

        dStart = new Date(dStart).getTime();
        dEnd = new Date(dEnd).getTime();
        guideStart = guideStart.getTime();
        var start = Math.round((dStart - guideStart) / 60000);
        var end = Math.round((dEnd - guideStart) / 60000);
        if (start < 0)
            start = 0; // starts the day before
        
        var left = ((minuteWidth * start));
        var width = ((minuteWidth * (end - start)) - 1);
        return { left: left + 'px', width: width + 'px' };
    };
    $scope.initEpgGrid = function () {
        var pageResize = function () {
            var epgGroupsHeight = $('#epg-groups').height();
            $('#epg-groups li').css({ width: epgGroupsHeight, left: -epgGroupsHeight });
        }
        pageResize();
        $(window).resize(pageResize);

        var epgtime = null; var epgchannels = null;
        var groupWidth = $('#epg-groups').width();
        var epgContainer = $('.epg-container');
        var funScrollEpgContainer = function () {
            if (epgchannels == null || epgchannels.length < 1)
                epgchannels = $('.epg-channels');
            if (epgtime == null || epgtime.length < 1)
                epgtime = $('.epg-time');

            var top = epgContainer.scrollTop();
            epgtime.css('top', top);

            var left = epgContainer.scrollLeft();
            epgchannels.css('left', left + groupWidth);
        };

        $('.epg-container').scroll(funScrollEpgContainer);
        funScrollEpgContainer(); // call it to set it up.

        $('.epg-container').scrollLeft($scope.getMinutesFromStartOfGuide(new Date()) * minuteWidth - (30 * minuteWidth));
    };
}
ns.GuideController.$inject = ['$scope', '$http', '$compile', '$rootScope'];
