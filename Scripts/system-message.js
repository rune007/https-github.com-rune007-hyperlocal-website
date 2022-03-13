/// <reference path="jquery-1.5.1.js" />

// We use the "system-message" div to communicate error messages to the UI in case something goes wrong, I have an SystemMessageModel with the property
// "SystemMessage", which is used to transport the error message from the controller to the view.
var systemMessageDiv = $("#system-message");
systemMessageDiv.hide();

// Displays the error message through the "system-message" div.
function ShowSystemMessage(systemMessage) {
    systemMessageDiv.text(systemMessage);
    systemMessageDiv.addClass("error");
    systemMessageDiv.show();
}