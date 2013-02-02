
npvrapp.directive('timeSpinner', function () {
    return {
        restrict: 'E',
        scope: {
            modelvalue: '=ngModel',
            ngDisabled: '=ngDisabled',
        },
        template: '<input type="text" value="@time" id="@spinnerid" />',
        replace: true,
        require: 'ngModel',
        controller: function ($scope, $element) {
            
            $scope.$watch(function () { return $($element).children().length; }, function (newValue, oldValue) {
                if ($scope.loaded)
                    return;
                $scope.loaded = true;
                var timeSpinner = $($element).timespinner().keydown(function () { return false; });
                var spinner = $($element).siblings('.ui-spinner');
                spinner.find('.ui-spinner-button').click(function () {
                    var hour = new Date(parseInt(timeSpinner.attr('aria-valuenow'), 10));
                    $scope.$apply(function () {
                        $scope.modelvalue = hour.getHours();
                    });
                });
            });
        }
    };
});