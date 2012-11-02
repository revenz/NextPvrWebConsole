/// <reference path="../functions.js" />
/// <reference path="../apihelper.js" />
/// <reference path="../jquery-1.8.2.js" />
/// <reference path="../jquery-ui-1.9.0.js" />
/// <reference path="../knockout-2.2.0.js" />

$(function () {
    function DashboardViewModel() {
        // Data
        var self = this;

        self.tuners = ko.observableArray([]);
        // Operations
        //self.deleteRecording = function (recording) { self.recordings.remove(recording) };

        // Load initial state from server, convert it to Task instances, then populate self.tasks
        api.getJSON("tuners", function (allData) {
            console.log(allData);
            var mapped = $.map(allData, function (item) { return new Tuner(item) });
            console.log(mapped);
            self.tuners(mapped);
        });
    }

    ko.applyBindings(new DashboardViewModel());
});

function Tuner(data) {
    this.oid = ko.observable(data.OID);
    this.name = ko.observable(data.Name);
    this.priority = ko.observable(data.Priority);
    this.enabled = ko.observable(data.Enabled);
    this.sourceType = ko.observable(data.SourceType);
    this.preset = ko.observable(data.Preset);
    this.deviceStandard = ko.observable(new Recorder(data.Recorder));
}

function Recorder(data) {
    var self = this;
    self.captureSourceOid = data.CaptureSourceOID;
    self.deviceFilter = data.DeviceFilter;
    self.diSEqC = data.DiSEqC;
    self.deviceInstance = data.DeviceInstance;
    self.commonInterface = data.CommonInterface;
    self.commonInterfaceInstances = data.CommonInterfaceInstances;
    self.deviceStandard = data.DeviceStandard;
    self.iniFile = data.iniFile;
    self.lnbs = new Array();
    if ($.isArray(data.lnbs)) {
        $.each(data.lnbs, function (i, ele) {
            self.lnbs.push(new Lnb(ele));
        });
    }
}

function Lnb(data){
    this.iniFile = data.iniFile;
    this.lnbLowOsc = data.lnbLowOsc;
    this.lnbHighOsc = data.lnbHighOsc;
    this.lnbSwitch = data.lnbSwitch
}