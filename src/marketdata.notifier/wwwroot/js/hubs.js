"use strict";

async function getAccessToken() {
    const response = await fetch('/api/account/token', {
        method: 'GET',
        credentials: 'include'
    });
    if (response.ok) {
        const data = await response.json();
        return data.AccessToken;
    }
    throw new Error("Failed to get access token");
}

async function connectToHub() {
    const token = await getAccessToken();

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/trade-hub", {
            accessTokenFactory: () => token
        })
        .build();

    connection.on("ReceiveMessage", function (message) {
        const li = document.createElement("li");
        document.getElementById("tradesList").appendChild(li);
        li.textContent = `${message}`;
    });

    await connection.start();
    console.log("Conectado al SignalR Hub");
}

connectToHub().catch(err => console.error("Error al conectar al SignalR Hub:", err));