
  
var reloading;
function checkReloading() {

    if (window.location.hash == "#autoreload") {        
        var interval = $('#txtInterval').val();
        reloading = setTimeout("window.location.reload(true);", parseInt(interval) * 1000);
        document.getElementById("reloadCB").checked = true;
    }
}




function checkIfCheckboxIsDefaultCheck() {
    if ($(document.getElementById("reloadCB")).prop("checked") == true) {
        var interval = $('#txtInterval').val();
        reloading = setTimeout("window.location.reload(true);", parseInt(interval) * 1000);
        document.getElementById("reloadCB").checked = true;        
    }
}

function toggleAutoRefresh(cb) {
    if (cb.checked) {
        window.location.replace("#autoreload");
       
        var interval = $('#txtInterval').val();
        reloading = setTimeout("window.location.reload(true);", parseInt(interval) * 1000);
    } else {
        window.location.replace("#");
        clearTimeout(reloading);
    }
}
window.onload = checkReloading();

var viewData = {};
viewData.GridBinder = function () {
    $('#divAvilable').modal('show');
}

$(function () {
    $("#idshowhide").click(function () {
        $("#main").slideToggle(500, function () {
            var txt = $('#t1').html();           
            $('#t1').html((txt == '<i class="fa fa-caret-left" aria-hidden="true"></i>') ? '<i class="fa fa-caret-down" aria-hidden="true"></i>' : '<i class="fa fa-caret-left" aria-hidden="true"></i>');
        });
    });

})

