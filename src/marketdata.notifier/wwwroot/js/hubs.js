"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/tradet-hub").build();

connection.on("ReceiveMessage", function (message) {
    var li = document.createElement("li");
    document.getElementById("tradesList").appendChild(li);

    li.textContent = `${message}`;
});

connection.start();