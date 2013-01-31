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