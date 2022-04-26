$(function () {

    //display dashboard chart view....
    //var $d_view = $(".d_view");
    //if ($d_view.length > 0) {
    //    $d_view.unbind("click").bind("click", function () {
            
    //        $(".modal-title").html($(this).siblings('small').html());
    //        var selectedChartType = $(this).siblings('.selectedChartType').attr('data-chart-type');
    //        chartType = selectedChartType;
    //        var chart_element = $(this).attr("data-id");
    //        $("#" + chart_element).remove();
    //        var createElement = "<div class='text-center animated fadeInUp' id=" + chart_element + " style='width:98%'></div>";
    //        $("#chart-area").html(createElement);
    //          $(".modal-header").removeClass("mh1 mh2 mh3 mh4").addClass(getHeaderClass);

    //         var data_caller = $(this).attr("");
    //        var callback = $(this).attr("data-caller");

    //        var x = eval(callback)
    //        if (typeof x == 'function') {
    //            if (chart_element == "divAttendanceChartSlotWise-modal") {
    //                var selecteShift = $("select[id*=drpShifts]").val();
    //                chart_attendance_slot_wise_caller(selecteShift, chart_element, chartType);
    //                chart_get_avgInTime_caller();
    //            }
    //            else {
    //                x(chart_element, chartType);
    //            }
    //        }
    //        $('#rz-viewModal').removeAttr("style");
    //        $('#rz-viewModal').css({ "top": "0px", "padding-right": "0px" });
    //        $(".modal-dialog").css({ "width": "100%", "margin": "0px auto" });
    //        $(".modal-body").css({ "height": $("body").height() - 400 });
    //        $('#rz-viewModal').modal('show');
    //    });
    //};
    
    //display dashboard chartType view.....
    var $chartTypes = $('span.chartType');
    if ($chartTypes.length > 0) {
        $chartTypes.unbind("click").bind("click", function () {
            chartType = $(this).attr('data-chart-type');
            var chart_element = $(this).attr("data-id");
            $(this).addClass('selectedChartType').siblings().removeClass("selectedChartType");

            // $("#" + chart_element).remove();
            //var createElement = "<div class='text-center animated fadeInUp' id=" + chart_element + " style='width:98%'></div>";
            //$("#chart-area").html(createElement);
            var callback = $(this).attr("data-caller");
            var x = eval(callback)
            if (typeof x == 'function') {
                if (chart_element == "divAttendanceChartSlotWise") {
                    var selecteShift = $("select[id*=drpShifts]").val();
                    chart_attendance_slot_wise_caller(selecteShift, chart_element, chartType);
                    chart_get_avgInTime_caller();
                }
                else {
                    x(chart_element, chartType);
                }
            }
        });
    };
});

