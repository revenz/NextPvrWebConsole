function RecordingDirectory(data) {
    if (data == null) 
        data = { Oid: 0, Name: '' };
    var self = this;
    self.oid = ko.observable(data.Oid);
    self.name = ko.observable(data.Name);
}