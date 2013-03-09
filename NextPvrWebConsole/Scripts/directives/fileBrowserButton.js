
npvrapp.directive('fileBrowserButton', function () {
    return {
        restrict: 'E',
        scope: {
            modelvalue: '=ngModel',
            ngDisabled: '=ngDisabled',
            xmlFiles: '@',
            text: '@',
            onOk:'&'
        },
        template: '<div class="file-browser-button">' +
	              '     <button class="btn" ng-disabled="ngDisabled" ng-click="openBrowser()">{{text}}</button> ' +
	              '     <div class="FolderBrowserWindow modal hide">' +
		          '         <div class="modal-header"> ' +
                  '             <button type="button" class="close" ng-click="close()" aria-hidden="true">&times;</button>' +
			      '             <h3 data-lang="Folder Browser"></h3>' +
		          '         </div>' +
		          '         <div class="modal-body">' +
                  '             <div class="FileTree">' +
				  '                 <div></div>' +
			      '             </div>' +
		          '         </div>' +
		          '         <div class="modal-footer">' +
			      '             <button class="btn btn-primary" ng-click="select()">OK</button>' +
			      '             <button class="btn" ng-click="close()>Cancel</button>' +
		          '         </div>' +
	              '     </div>' +
                  '</div>',
        replace: true,
        require: 'ngModel',
        controller: function ($scope, $element) {

            $scope.openBrowser = function () {
                $($element).find('.FolderBrowserWindow').modal();

                var filetree = $($element).find('.FileTree div');
                filetree.children().remove();

                var script = '/file/LoadDirectory';
                if ($scope.xmlFiles)
                    script = '/file/LoadDirectoryAndXml';

                filetree.fileTree(
                {
                    root: '%root%',
                    script: script,
                }, function (file) {
                    alert(file);
                });
                return false;
            };

            $scope.close = function () {
                $($element).find('.FolderBrowserWindow').modal('hide');
                var filetree = $($element).find('.FileTree div');
                filetree.children().remove();
            };

            $scope.select = function () {
                var selected = $($element).find('a.selected');
                if (selected.length > 0) {
                    $scope.modelvalue = selected.attr('rel');
                }
                $scope.close();
                console.log('about to call onOk: ' + $scope.modelvalue);
                console.log(selected);
                $scope.onOk($scope.modelvalue);
            };
        }
    };
});