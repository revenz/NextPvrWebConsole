function recording(data) {
    var self = this;
    self.oid = ko.observable(data.OID);
    self.startTime = ko.observable(data.StartTime);
    self.endTime = ko.observable(data.EndTime);
    self.postPadding = ko.observable(data.PostPadding);
    self.prePadding = ko.observable(data.PrePadding);
    self.subtitle = ko.observable(data.Subtitle);
    self.title = ko.observable(data.Title);
    self.name = ko.observable(data.Name);
    self.channelNumber = ko.observable(data.ChannelNumber);
    self.channelName = ko.observable(data.ChannelName);
    self.channelIconAvailable = ko.computed(function () { return data.ChannelIcon && data.ChannelIcon.length > 0; });
    self.channelIconData = ko.computed(function () {
        if (data.ChannelIcon && data.ChannelIcon.length > 0)
            return 'data:image/png;base64,' + data.ChannelIcon;
        return '';
    });
    self.startTimeString = ko.computed(function () { return gui.formatTime(data.StartTime); });
    self.endTimeString = ko.computed(function () { return gui.formatTime(data.EndTime); });
    self.displayName = ko.computed(function () {
        if (data.Title)
            return data.Title;
        return data.Name;
    });
}
