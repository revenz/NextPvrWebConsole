function refreshChannelInformation(callback) {
    var url = getForTheRecordUrl('GetChannels');
    console.log('url: ' + url);
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    var tzBias = getTimeZoneBias();
    today.setMinutes(today.getMinutes() + tzBias);
    var end = new Date(today);
    end = end.setDate(today.getDate() + 7) / 1000;
    console.log('Getting data from webservice: ' + today + ', ' + end);
    var callbackfunction = function (data) {
        console.log(data);
        if (console && console.log) {
            console.log(data);
        }
        var channels = [];
        for (var i = 0; i < data.length; i++) {
            var channel = new Channel();
            channel.Id = data[i].Id;
            channel.Name = data[i].Name;
            channel.Number = data[i].LogicalChannelNumber;
            for (var j = 0; j < data[i].ProgramGuide.length; j++) {
                var program = new ProgramListing();
                program.Id = data[i].ProgramGuide[j].Id;
                program.Title = data[i].ProgramGuide[j].Title;
                program.Subtitle = data[i].ProgramGuide[j].Subtitle;
                program.StartUtc = new Date(parseInt(data[i].ProgramGuide[j].StartUtc.substr(6), 10));
                program.EndUtc = new Date(parseInt(data[i].ProgramGuide[j].EndUtc.substr(6), 10));
                program.Summary = data[i].ProgramGuide[j].Summary;
                program.ChannelId = data[i].Id;
                channel.ProgramListings.push(program);
            }
            channels.push(channel);
        }
        console.log('channels: ' + channels);
        if (callback)
            callback(channels);
    }
    console.log('url: ' + url);
    var data = { ChannelGroupId: 'Test', StartUtc: today.getTime() / 1000, EndUtc: end };
    $.getJSON(url, data, function (result) {
        callbackfunction(result);
    });
}