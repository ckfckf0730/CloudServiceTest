// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

connection.start().then(function () {
    console.log("SignalR connected");
}).catch(function (err) {
    return console.error(err.toString());
});


async function getUserId(userName) {
    const response = await fetch(`/Chat/GetUserIdByName?userName=${encodeURIComponent(userName)}`);
    const data = await response.json();
    return data.userId;
}