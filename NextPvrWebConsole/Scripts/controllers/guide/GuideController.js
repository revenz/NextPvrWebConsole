var ns = namespace('Controllers');

ns.GuideController = function ($scope, $http, $compile, $rootScope) {
    "use strict";
    var minuteWidth = 5;

    var today = new Date(new Date().setHours(0, 0, 0, 0));

    $scope.selectedShow = null;
    $scope.guidehtml = 'loading please wait.';
    $scope.isLoaded = true;
    $scope.days = [];
    $scope.recording = null;
    for (var i = 0; i < 7; i++) {
        var date = new Date(new Date().setHours(0, 0, 0, 0));
        $scope.days.push(new Date(date.setDate(date.getDate() + i)));
    }
    $scope.selectedDateIndex = 0;
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
    $scope.refreshEpgData = function () {
        $scope.isLoading = true;
        gui.doWork();
        // add random to stop IE from caching the guide data
        console.log('about to get guide data: ' + $scope.days[$scope.selectedDateIndex]);
        $http.get('/guide/epg?date=' + $.format.date($scope.days[$scope.selectedDateIndex], 'yyyy-MM-dd') + '&group=' + encodeURIComponent($scope.channelGroups[$scope.selectedGroupIndex].Key) + '&rand=' + Math.random()).success(function (result) {
            // need to compile it to hookup the angular events.
            var element = $compile(result)($scope);
            // set the guide html to the element so its shown on the page
            $scope.guidehtml = element;

            $scope.initEpgGrid();

            gui.finishWork();
            $scope.isLoading = false;
        });
    };
    $scope.openListing = function (oid) {
        console.log('openning oid: ' + oid);
        $http.get('api/guide/epglisting/' + oid).success(function (data) {
            console.log(data);
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
    $scope.openRecordingEditor = function (listing) {
        $rootScope.recording = listing;
        $('#recording-options').modal({});
    };
}
ns.GuideController.$inject = ['$scope', '$http', '$compile', '$rootScope'];