function RecordingDirectory(data) {
    if (data == null) 
        data = { Oid: 0, Name: '', Path: '', IsDefault: false };
    var self = this;
    self.oid = ko.observable(data.Oid);
    self.name = ko.observable(data.Name);
    self.path = ko.observable(data.Path);
    self.isDefault = ko.observable(data.IsDefault);
    self.toApiObject = function () {
        data.Oid = self.oid();
        data.Name = self.name();
        data.Path = self.path();
        data.IsDefault = self.isDefault();
        return data;
    };
}