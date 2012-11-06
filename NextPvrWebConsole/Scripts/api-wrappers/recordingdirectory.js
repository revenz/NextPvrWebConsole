function RecordingDirectory(data) {
    var self = this;
    self.oid = ko.observable(data.Oid);
    self.name = ko.observable(data.Name);
}