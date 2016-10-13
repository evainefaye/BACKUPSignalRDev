$(document).ready(function () {
    // Reference to Hub
    myHub = $.connection.myHub;
    

    /* Retrieve the userId from a prompt because it could not be retrieved otherwise */
    myHub.client.getUserId = function () {
        if (typeof (myHub.server.userId) == 'undefined') {
            userId = prompt("No authentication detected. Please Enter your ATTUID:", "");
            userId = $.trim(userId).toLowerCase();
            if (userId == "" || userId == null) {
                myHub.client.getUserRecurse();
                return;
            }
            myHub.server.authenticatedUser(userId);
        }
    };

    // Functions that may be run on the client from the hub
    myHub.client.registerMonitor = function (userId) {
        monitorUserRecord = Database.lookupMonitorUsers(userId);
    };

    /* Supplied username was blank or a space, ask for it again */
    myHub.client.getUserRecurse = function () {
        myHub.client.getUserId();
    };

    myHub.client.setupMonitor = function (json) {
        console.log(json);
        $.each($.parseJSON(json), function (key, value) {
            if (typeof(value) != 'object') {
                console.log('key: ' + key + ' value: ' + value);
            } else {
                console.log(value);

            }
        });
    };

    myHub.client.receiveDictionary = function (dictionary) {
        $("div.dictionary-area").html(dictionary);
        $("div.dictionary-area").treeview({
            collapsed: true
        });
    }

    myHub.client.addSashaSessions = function (connectionId, userId, userName, sessionStartTime) {
        $("ul#sashaConnections").append("<li id='" + connectionId + "'>" + userId + " " + userName);
    }

    $.connection.hub.start().done(function () {
        myHub.server.checkAuthenticated();
    });

});
