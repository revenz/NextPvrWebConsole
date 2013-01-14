var ns = namespace('Controllers.General');

ns.TabController = function ($scope) {
    "use strict";
    var self = this;

    $scope.selectedTabIndex = 0;
}
ns.TabController.$inject = ['$scope'];
