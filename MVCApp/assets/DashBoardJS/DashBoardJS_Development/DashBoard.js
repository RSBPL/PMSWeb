
var divProduction = "divProduction";
var divProductionModelWise = "divProductionModelWise"
var divProductionPhaseWise = "divProductionPhaseWise";
var divDispatch = "divDispatch";
var divRejection = "divRejection";
var divStockInWareHouse = "divStockInWareHouse";
var divPendingDispatch = "divPendingDispatch";
var divChartPopupClickChart = "divChartPopupClickChart";
var ChartType = "Pie";

var Param = null;
var txt = null;
var durationType = null;
var ChartClicData = null;
var chartType = "Pie";
var chartType = {
    type: "Pie",
}
//START AVILABLESTOCK GRAPH
function chart_Available_Stock_caller(chart_element, chartType) {
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "DeshboardVer3.aspx/GetAvilableStock",       
        data: "{'unitcode':'" + $('input[id$=HiddenUnit]').val() + "'}",
        dataType: "json",
        success: function (Result) {
            Result = Result.d;
            var data = [];
            for (var i in Result) {
                var series = new Array(Result[i].Model, Result[i].Quantity);
                data.push(series);
            }
            DrawAvilableStock(data, chart_element, chartType);
        },
        error: function (Result) {
            //alert(Result);
        }
    });
}

function DrawAvilableStock(series, chart_element, chartType) {    
    
    //alert(chartType);
    var chart = new Highcharts.Chart({ // Not Return Series please check it again.
        chart: {
                       
            renderTo: chart_element,
            type: chartType.toLowerCase(),

            events: {
                click: (e) => {
                    if (!($(event.target)[0].textContent)) {
                        console.log('clicked'); //this is printing
                        this.drillDown(); // how to call typescript function here?
                    }
                },
                redraw: function () {
                    chart_Available_Stock_caller(divAvailableStock, chartType);
                }
            },
        },
        title:
        {
            text: null
        },
        
        legend: {
            enabled: true,
            verticalAlign: 'bottom',
            width: 300,
            itemWidth: 150,
            labelFormatter: function () {
                var words = this.name.split(/[\s]+/);
                var numWordsPerLine = 2;
                var str = [];

                for (var word in words) {
                    if (word > 0 && word % numWordsPerLine == 0)
                        str.push('<br>');

                    str.push(words[word]);
                }

                return str.join(' ');
            }
        },
        tooltip: {
            formatter: function () {
                txt = this.point.name;
                return '<b>' + this.point.name + '</b>: ' + this.point.y;
            }
        },

        credits: {
            enabled: false
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    style: {
                        width: '100px'
                    },
                    enabled: true,         /*true*/
                    format: '<b>{point.name}</b>: Quantity {point.y}'
                },
                showInLegend: false,
            },
            series: {
                cursor: 'pointer',
                point: {
                    events: {
                        click: function () {                           
                            durationType = "NonDateWise";
                            Param = "POPUPCATEGORY";
                            ChartPopupData(txt, durationType);
                            ChartClickPopupPiChart_Caller(divChartPopupClickChart, ChartType);
                        }
                    }
                }
            },
        },
        series: [{
            name: 'Availabile Stock',
            data: series,
        }],
        navigation: {
            buttonOptions: {
                enabled: true
            }
        },
    });
}


$(document).ready(function () {
    //int_it();
    //some trigger..........
    //$("select[id*=drpShifts]").change(function () {
    //    var selecteShift = $(this).val();
    //    //chart_Production_Phase_Wise_on_change_caller(divProductionPhaseWise, chartType.type);
    //    //chart_get_avgInTime_caller();
    //});
    //$("select[id*=drpShifts]").trigger("change");
}); 
     
//END Shift Wise Phase Wise Production
function int_it() {
    chart_Available_Stock_caller(divAvailableStock, chartType);
    chart_Production_Model_Wise_caller(divProductionModelWise, chartType);
    chart_Production_Phase_Wise_caller(divProductionPhaseWise, chartType);
    chart_Dispatch_caller(divDispatch, chartType);
    chart_Rejection_caller(divRejection, chartType);
    chart_Stock_In_Warehouse_caller(divStockInWareHouse, chartType);
    chart_Pending_Dispatch_caller(divPendingDispatch, chartType);   
   
    //$("select[id*=drpShifts]").trigger("change");      
}



$("#SearchSrNo").hide();
$("#Table0").hide();
$("#Table1").hide();
$("#Table2").hide();
$("#Table3").hide();
$("#Table4").hide();
$('input[id*=txtSrNo]').val("");
//$("select[id*=drpShifts]").attr('checked', false);

$('#btnSearchSrNo').on('click', function () {
    SearchSerialNo();
});

function SendSMS() {
      
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "DeshboardVer3.aspx/sendsms",
        data: "{'SerialNo':'" + $('input[id*=txtSrNo]').val() + "', 'unitcode': '" + $('input[id$=HiddenUnit]').val() + "', 'PhoneNo': '" + $('input[id*=txtphoneno]').val() + "'}",
        dataType: "json",
        success: function (result) {
            if (result.d == 1) {
                alert('SMS sent Successfully.');

            }
            if (result.d == 0) {
                alert('SMS not Sent.');

            }
            if (result.d == 2) {
                alert('This serial number does not exist.');

            }
        },
        error: function (errormessage) {
            alert('Somthing went wrong.');
        }
    });
}
     

$('#btnsms').on('click', function () {

    if ($('input[id*=txtphoneno]').val() == '' || $('input[id*=txtSrNo]').val() == '') {
        alert('Mobile number OR Serial number should not be left blank.');
     }

    else
    {
        SendSMS();
    }
   
   
});


$("#SearchSrNoClose1, #SearchSrNoClose2").on("click", function () {
    $("#SearchSrNo").hide();
    $("#Table0").hide();
    $("#Table1").hide();
    $("#Table2").hide();
    $("#Table3").hide();
    $("#Table4").hide();
    $('input[id*=txtSrNo]').val("");
    //$("select[id*=drpShifts]").attr('checked', false);
});
//END SEARCH FOR SR. NO

//START Chart Click Popup Render

function ChartPopupData(ItemCetory, duration) {    
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "DeshboardVer3.aspx/GetPopupCategory",
        data: "{'startDate':'" + $('input[id*=txtfromdate]').val() + "','endDate':'" + $('input[id*=txttodate]').val() + "','unitcode':'" + $('input[id$=HiddenUnit]').val() + "','itemCetory':'" + ItemCetory + "','duration':'" + duration + "'}",
        dataType: "json",
        success: function (result) {
            $('#thHeading').text(ItemCetory + " : " + $('input[id*=txtfromdate]').val() + " : " + $('input[id*=txttodate]').val());
            $('#divChartAvailableStock').modal('show');
            var html = '';
            var Total = 0;
            html += '<tr>';
            $.each(result.d, function (key, item) {
                if (item.SrNo == null || item.SrNo == "") {
                    html += '<td>' + "--NA--" + '</td>';
                }
                else {
                    html += '<td>' + item.SrNo + '</td>'
                }
                if (item.ItemCode == null || item.ItemCode == "") {
                    html += '<td>' + "--NA--" + '</td>';
                }
                else {
                    html += '<td>' + item.ItemCode + '</td>'
                }
                if (item.ItemName == null || item.ItemName == "") {
                    html += '<td>' + "--NA--" + '</td>';
                }
                else {
                    html += '<td>' + item.ItemName + '</td>'
                }
                if (item.Quantity == null || item.Quantity == "") {
                    html += '<td>' + "--NA--" + '</td>';
                }
                else {
                    html += '<td>' + item.Quantity + '</td>'
                    Total += parseInt(item.Quantity);
                }
                if (item.MRP == null || item.MRP == "") {
                    html += '<td>' + "--NA--" + '</td>';
                }
                else {
                    html += '<td>' + item.MRP + '</td>'
                }
                html += '</tr>';
            });
            html += '<tr"><td></td><td></td><td class="text-right"><strong><b>Total : </b></strong></td><td><strong><b>' + Total + '</b></strong></td><td></td></tr>'
            $('#PopupChartStockTbody').html(html);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}

function ChartClickPopupPiChart_Caller(chart_element, chartType) {
    $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        url: "DeshboardVer3.aspx/GetPopupCategoryChart",
        data: "{'startDate':'" + $('input[id*=txtfromdate]').val() + "','endDate':'" + $('input[id*=txttodate]').val() + "','unitcode':'" + $('input[id$=HiddenUnit]').val() + "','itemCetory':'" + txt + "','duration':'" + durationType + "'}",
        dataType: "json",
        success: function (Result) {   
            var count = null;
            $.each(Result.d, function (key, item) {       
                count++;                 
            });
           
            if (count > 0) {
                ChartClicData = Result.d;
                Result = Result.d
            }
            else {
                Result = ChartClicData;
            }

            Result = Result;
            var data = [];
            for (var i in Result) {
                var series = new Array(Result[i].Model, Result[i].Quantity);
                data.push(series);
            }
            ChartClickPopupPiChart(data, chart_element, chartType);
        },
        error: function (errormessage) {
            alert(errormessage.responseText);
        }
    });
}

function ChartClickPopupPiChart(series, chart_element, chartType) {

    //alert(chartType);
    var chart = new Highcharts.Chart({ // Not Return Series please check it again.
        chart: {
            //backgroundColor: '#a3ff9b',            
            renderTo: chart_element,
            type: chartType.toLowerCase()
        },
        title:
        {
            text: null
        },
       
        legend: {
            enabled: true,
            verticalAlign: 'bottom',
            width: 300,
            itemWidth: 150,
            labelFormatter: function () {
                var words = this.name.split(/[\s]+/);
                var numWordsPerLine = 2;
                var str = [];

                for (var word in words) {
                    if (word > 0 && word % numWordsPerLine == 0)
                        str.push('<br>');

                    str.push(words[word]);
                }

                return str.join(' ');
            }
        },
        tooltip: {
            formatter: function () {
                txt = this.point.name;
                return '<b>' + this.point.name + '</b>: ' + this.point.y;
            }
        },

        credits: {
            enabled: false
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: 'pointer',
                dataLabels: {
                    style: {
                        width: '100px'
                    },
                    enabled: true,         /*true*/
                    format: '<b>{point.name}</b>: Quantity {point.y}'
                },
                showInLegend: false,
            },
            series: {
                cursor: 'pointer',
                point: {
                    events: {
                        click: function () {
                            
                        }
                    }
                }
            },
        },
        series: [{
            name: 'Available Stock',
            data: series,
        }],
    });
}
//ChartClickPopupPiChart_Caller(divChartPopupClickChart, ChartType);
//Export into excel inside the chart click popup
$("#LinkButtonExportInsidePopup").on("click", function () {   
    $("#tblPopupChartStock").table2excel({
        // exclude CSS class
        exclude: ".noExl",
        name: "Worksheet Name",
        filename: "SomeFile" //do not include extension
    });
    //ExportInChartClickPopup(txt, Param);
})

//END Chart Click Popup Render


function check() {
    document.getElementById("reloadCB").checked = true;
    toggleAutoRefresh(document.getElementById("reloadCB"));
}
var prm = Sys.WebForms.PageRequestManager.getInstance();

prm.add_pageLoaded(pageLoaded);

function pageLoaded() {
    $("select[id*=drpShifts]").change(function () {
        var selecteShift = $(this).val();
        chart_Production_Phase_Wise_on_change_caller(divProductionPhaseWise, chartType);
        //chart_get_avgInTime_caller();
    });
    //$("select[id*=drpShifts]").trigger("change");

    int_it();
    $("#SearchSrNo").hide();
    $("#Table0").hide();
    $("#Table1").hide();
    $("#Table2").hide();
    $("#Table3").hide();
    $("#Table4").hide();
    $('input[id*=txtSrNo]').val("");
    //$("select[id*=drpShifts]").attr('checked', false);

    //Start Reload Search Sr. No
    $('#btnSearchSrNo').on('click', function () {
        SearchSerialNo();
    });

    $("#SearchSrNoClose1, #SearchSrNoClose2").on("click", function () {
        $("#SearchSrNo").hide();
        $("#Table0").hide();
        $("#Table1").hide();
        $("#Table2").hide();
        $("#Table3").hide();
        $("#Table4").hide();
        $('input[id*=txtSrNo]').val("");
        $("select[id*=drpShifts]").attr('checked', false);
    });
    //End Reload Search Sr. No

    //Start Export In Excel on click chart popup
    $("#LinkButtonExportInsidePopup").on("click", function () {
        $("#tblPopupChartStock").table2excel({
            // exclude CSS class
            exclude: ".noExl",
            name: "Worksheet Name",
            filename: "SomeFile" //do not include extension
        });
        //ExportInChartClickPopup(txt, Param);
    })
    //End Export In Excel on click chart popup

    //
    function chart_Available_Stock_caller(chart_element, chartType) {
        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "DeshboardVer3.aspx/GetAvilableStock",
            data: "{'unitcode':'" + $('input[id$=HiddenUnit]').val() + "'}",
            dataType: "json",
            success: function (Result) {
                Result = Result.d;
                var data = [];
                for (var i in Result) {
                    var series = new Array(Result[i].Model, Result[i].Quantity);
                    data.push(series);
                }
                DrawAvilableStock(data, chart_element, chartType);
            },
            error: function (Result) {
                //alert(Result);
            }
        });
    }
    //
}