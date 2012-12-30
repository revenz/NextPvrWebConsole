(function fixScrollAndTapHold() {

    var $document = $(document);
    var isScrolling = false;
    var lastX, lastY, lastT, scrollTimeout;

    $document.bind("touchstart mousedown click", function (e) {
        lastT = e.originalEvent.timeStamp;
    });

    $document.bind("scroll", function (e) {
        var t = e.originalEvent.timeStamp;

        if (!t) {
            return;
        }

        var x = window.pageXOffset;
        var y = window.pageYOffset;

        var dT = t - lastT;
        var dX = Math.abs(x - lastX);
        var dY = Math.abs(y - lastY);
        var dMax = Math.max(dX, dY);

        lastT = t;
        lastX = x;
        lastY = y;

        var speed = dMax / dT;

        if (speed >= 0.1 || dMax >= 10) {
            isScrolling = true;
        }

        clearTimeout(scrollTimeout);
        scrollTimeout = setTimeout(function () {
            isScrolling = false;
        }, 500);
    });

    /**
    * @return {Boolean}
    */
    $.mobile.isScrolling = function () {
        return isScrolling;
    };

})();