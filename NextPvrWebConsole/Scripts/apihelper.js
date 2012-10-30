var api = new function()
{
    this.getJSON = function (url, callback) {
        $.ajax(
        {
            url: "/api/" + url, 
            dataType: 'json',
            accepts: "application/json",
            statusCode:
            {
                200: function(data)
                {
                    if(callback)
                        callback(data);
                },
                401: function(jqXHR, textStatus, errorThrown) {
                    self.location = '/Account/Login/';
                }
            }
        });
    }
}