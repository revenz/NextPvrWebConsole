var ns = namespace('Controllers.Configuration');

ns.ChannelSelectorController = function ($scope) {
    "use strict";
    var self = this;

    $scope.ok = function () {
        $scope.model.result = 1;
        $scope.model.isOpen = false;
    };

    $scope.cancel = function () {
        $scope.model.result = 0;
        $scope.model.isOpen = false;
    };

};
ns.ChannelSelectorController.$inject = ['$scope'];


var _ChannelSelectorTemplate;

function ChannelSelector(listOfChannels, $scope, $http, $compile) {
    "use strict";

    var self = this;

    self.successCallback = null;

    self.success = function (successCallback) {
        self.successCallback = successCallback;
    }

    $scope.model = {
        items: listOfChannels,
        result: 0,
        isOpen: false
    };

    self.id = 'channelSelector_' + Math.floor(Math.random() * 10000);

    $scope.$watch(function () { return $scope.model.isOpen; }, function (newValue, oldValue) {
        console.log('newValue: ' + newValue);
        if (!newValue) {
            // it was closed
            $('#' + self.id + ' > div').modal('hide');
            $('#' + self.id).remove();
            if (self.successCallback && $scope.model.result == 1)
                self.successCallback($scope.model.items);
        }
    });

    self.open = function () {
        if (!_ChannelSelectorTemplate) {
            $http.get('/control/channelSelector').success(function (html) {
                _ChannelSelectorTemplate = html;
                self.open();
            });
        } else {
            var element = $compile('<div id="' + self.id + '">' + _ChannelSelectorTemplate + '</div>')($scope);
            $('body').append(element);
            $('#' + self.id + ' > div').modal();
            $scope.model.isOpen = true;
        }
    };


    self.open();

    return self;
}
