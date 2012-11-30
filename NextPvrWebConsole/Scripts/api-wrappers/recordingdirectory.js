function RecordingDirectory(data) {
    if (data == null) 
        data = { Oid: 0, Name: '', Path: '', IsDefault: false, RecordingDirectoryId: '' };
    var self = this;    
    self.oid = ko.observable(data.Oid);
    self.name = ko.observable(data.Name);
    self.path = ko.observable(data.Path);
    self.isDefault = ko.observable(data.IsDefault);
    self.userOid = ko.observable(data.UserOid);
    self.recordingDirectoryId = ko.observable(data.RecordingDirectoryId);
    self.displayName = ko.computed(function () {
        if (self.userOid() == 1)
            return '[' + $.i18n._('Shared') + '] ' + self.name();
        return self.name();
    });
    self.isShared = ko.computed(function () {
        return self.userOid() == 1;
    });

    self.toApiObject = function () {
        data.Oid = self.oid();
        data.Name = self.name();
        data.Path = self.path();
        data.IsDefault = self.isDefault();
        data.UserOid = self.userOid();
        data.RecordingDirectoryId = self.recordingDirectoryId();
        return data;
    };
}