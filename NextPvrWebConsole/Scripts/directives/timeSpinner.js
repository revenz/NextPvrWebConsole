
npvrapp.directive('timeSpinner', function () {
    return {
        restrict: 'E',
        scope: {
            modelvalue: '=ngModel',
            max: '@',
            min: '@',
            ngDisabled: '=ngDisabled',
        },
        template: '<div class="spinner">' +
                  '     <input type="text" readonly class="input-mini spinner-input" ng-model="displayValue">' +
                  '     <div class="spinner-buttons  btn-group btn-group-vertical">' +
                  '         <button class="btn spinner-up" ng-click="up()">' +
                  '             <i class="icon-chevron-up"></i>' +
                  '         </button>' +
                  '         <button class="btn spinner-down" ng-click="down()">' + 
                  '             <i class="icon-chevron-down"></i>' +
                  '         </button>' +
                  '     </div>' +
                  '</div>',
        replace: true,
        require: 'ngModel',
        controller: function ($scope, $element) {
            
            $scope.$watch(function () { return $($element).children().length; }, function (newValue, oldValue) {
                if ($scope.loaded)
                    return;
                $scope.loaded = true;
            });

            $scope.$watch(function () { return $scope.modelvalue; }, function (newValue, oldValue) {
                if (newValue == 0)
                    $scope.displayValue = '12:00 am';
                else if (newValue < 12)
                    $scope.displayValue = newValue + ':00 am';
                else if (newValue == 12)
                    $scope.displayValue = newValue + ':00 pm';
                else
                    $scope.displayValue = (newValue - 12) + ':00 pm';
            });

            $scope.up = function () {
                if ($scope.modelvalue + 1 > 23)
                    return;
                $scope.modelvalue = $scope.modelvalue + 1;
            };

            $scope.down = function () {
                if ($scope.modelvalue < 1)
                    return;
                $scope.modelvalue = $scope.modelvalue - 1;
            };
        }
    };
});