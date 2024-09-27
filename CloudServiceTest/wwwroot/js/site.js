// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub")
        .build();

//connection.on("ReceiveMessage", function (user, message) {
//    const msg = `${user}: ${message}`;
//    const li = document.createElement("li");
//    li.textContent = msg;
//    document.getElementById("messagesList").appendChild(li);
//});

//document.getElementById("sendButton").addEventListener("click", function (event) {
//    const user = document.getElementById("userInput").value;
//    const message = document.getElementById("messageInput").value;
//    connection.invoke("SendMessage", user, message).catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});

connection.start().then(function () {
    console.log("SignalR connected");
}).catch(function (err) {
    return console.error(err.toString());
});