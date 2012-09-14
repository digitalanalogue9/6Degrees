/// <reference path="../../jquery-1.8.1-vsdoc.js" />
function RunMatterTeamPlugin(){
    $("#pluginbuttoncontainer").append("<button class='btn btn-large' type='button' id='mymattersbutton'>My Matters</button>");

    $("#pluginbuttoncontainer").on("click", "#mymattersbutton", function (event) {
        $("#mymattersmodal").dialog('open');
    });

    $(".choosematter").on("click", "#matterteamsbutton", function (event) {
        var thisid = $(this).attr("id");
        var data = $(this).data("#" + thisid);
    });

    $("#pluginmodalcontainer").append("<div id='mymattersmodal' class='hide' title='My Matter Teams'><ul class='unstyled' id='mymatterteams'></ul></div>");
    var $mymatterteams = $("#mymatterteams");
    $("#mymattersmodal").dialog({
		autoOpen: false,
		height: 400,
		width: 600,
		modal: true
	});
    var dateinfo = getDateInfo(new Date());
    var person = getUser();
    $.ajax({
        url: "/Home/MyMatters",
        type: "GET",
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        data: { From: toISOString(dateinfo.PreviousMonday), To: toISOString(dateinfo.NextSunday), Person: person },
        cache: false
    })
    .done(function (data) {
        $mymatterteams.empty();
        $.each(data.TeamMemberOf, function (i, v) {
            $mymatterteams.append("<li class='choosematter'>" +
                v.Name + "(" + v.Number + ")" +
                "<input type='text' name='matterteamsamount" + i + "' id='matterteamsamount" + i + "' style='border:0;width: 30px; font-weight:bold;' /></p>" +
                "<button type='button' class='matterteamsbutton' id='matterteamsbutton" + i + "'>Add</button>" + 
                "<div id='matterteamsslider" + i + "' class='slider' style='width:300px;'></div></li>");
            $('#matterteamsslider' + i).slider({
                range: 'min',
                min: 0,
                max: $("#daysinmyweek").val(),
                value: 0,
                step: 0.5,
                slide: function (event, ui) {
                    $('#matterteamsamount' + i).val(ui.value);
                    $("matterteamsbutton" + i).data("matterteamval" + i, { Name: v.Name, Number: v.Number, Percentage: ui.value });
                }
            });
            $("matterteamsbutton" + i).data("matterteamval" + i, { Name: v.Name, Number: v.Number, Percentage: 0 });
        });
    })
    .fail(function () { alert('error'); })
    .always(function () {
    });

}

/*






*/