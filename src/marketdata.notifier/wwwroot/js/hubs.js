"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/tradetHub").build();

connection.on("ReceiveMessage", function (message) {
    var li = document.createElement("li");
    document.getElementById("tradesList").appendChild(li);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    li.textContent = `${message}`;
});

connection.start();