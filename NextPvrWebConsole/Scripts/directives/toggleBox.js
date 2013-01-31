
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
                    onChange: function ($el, status, e) {
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