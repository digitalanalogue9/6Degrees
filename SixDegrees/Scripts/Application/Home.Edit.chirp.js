﻿$(function () {
    $("#accordion").accordion();

    $("#editbuttoncontainer").on("click", "#deletebutton", function (event) {
        $("#deleteform").submit();
    });
});