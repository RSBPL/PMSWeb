// Place or remove nav active state.

$(document).ready(function () {
    var isTabClick = localStorage.getItem("IsTabChange", true);
    if (isTabClick) {
        //alert(localStorage.getItem("selectedolditem"));
        $("#myTab a").click(function () {
            var id = $(this);
            localStorage.setItem("IsTabChange", true);
            $(".active").removeClass("active");
            $(id).addClass("active");
            localStorage.setItem("selectedolditem", $(id).text());
            //alert(localStorage.getItem("selectedolditem"));
        });

        var selectedolditem = localStorage.getItem('selectedolditem');

        if (selectedolditem !== null) {
            var tabName = localStorage.getItem("selectedolditem");
            if (tabName == 'Tractor Assembly Items') {
                $('#Menu1').addClass('active');
                $(".fade").removeClass("fade");
            }
            else if (tabName == 'Tractor Sub Assembly Items') {
                $('#Menu4').addClass('active');
                $(".fade").removeClass("fade");
            }
            else if (tabName == 'Torque') {
                $('#Menu2').addClass('active');
                $(".fade").removeClass("fade");
            }
            else {
                $('#Menu3').addClass('active');
                $(".fade").removeClass("fade");
            }
            if ($("a:contains('" + selectedolditem + "')").length > 0) {
                $("li a:contains('" + selectedolditem + "')").parent().addClass('active');
            }
        }
        else {
            $("#myTab li").eq(0).addClass('active');
            $('#Menu1').addClass('active');
        }
    }
    else {
        $("#myTab li").eq(0).addClass('active');
        $('#Menu1').addClass('active');
    }


    $("#Update").hide();
    $.noConflict();

    ddlplant();
    AC_ItemCode();
    AC_Transmission();
    AC_Engine();
    AC_RearTyre();
    AC_RHRearTyre();
    AC_Battery();
    AC_HydraulicPump();
    AC_SteeringAssembly();
    AC_RadiatorAssembly();
    AC_Alternator();
    AC_BRAKE_PEDAL();
    AC_CLUTCH_PEDAL();
    AC_SPOOL_VALUE();
    AC_TANDEM_PUMP();
    AC_FENDER();
    AC_RearAxel();
    AC_Hydraulic();
    AC_FrontTyre();
    AC_RHFrontTyre();
    AC_Backend();
    AC_SteeringMotor();
    AC_SteeringCylinder();
    AC_ClusterAssembly();
    AC_StartorMotor();
    AC_Rops();
    AC_FENDER_RAILING();
    AC_HEAD_LAMP();
    AC_STEERING_WHEEL();
    AC_REAR_HOOD_WIRING_HARNESS();
    AC_SEAT();
   /* BindT3Plant();*/
});


function AC_ItemCode() {
    $("#ItemCode").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                ItemCode: $('#ItemCode').val()
            };
            $.ajax({
                url: ItemCode,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {
                        /*console.log(item.DESCRIPTION);*/
                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_Transmission() {
    $("#Transmission").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                Transmission: $('#Transmission').val()
            };
            $.ajax({
                url: TransMission,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_Engine() {
    $("#Engine").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                Engine: $('#Engine').val()
            };
            $.ajax({
                url: Engine,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_RearTyre() {
    $("#RearTyre").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                RearTyre: $('#RearTyre').val()
            };
            $.ajax({
                url: RearTyre,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_RHRearTyre() {
    $("#RHRearTyre").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                RHRearTyre: $('#RHRearTyre').val()
            };
            $.ajax({
                url: RHRearTyre,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_Battery() {
    $("#Battery").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                Battery: $('#Battery').val()
            };
            $.ajax({
                url: Battery,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_HydraulicPump() {
    $("#HydraulicPump").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                HydraulicPump: $('#HydraulicPump').val()
            };
            $.ajax({
                url: HydraulicPump,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_SteeringAssembly() {
    $("#SteeringAssembly").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                SteeringAssembly: $('#SteeringAssembly').val()
            };
            $.ajax({
                url: SteeringAssembly,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_RadiatorAssembly() {
    $("#RadiatorAssembly").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                RadiatorAssembly: $('#RadiatorAssembly').val()
            };
            $.ajax({
                url: RadiatorAssembly,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_Alternator() {
    $("#Alternator").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                Alternator: $('#Alternator').val()
            };
            $.ajax({
                url: Alternator,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_BRAKE_PEDAL() {
    $("#BRAKE_PEDAL").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                BRAKE_PEDAL: $('#BRAKE_PEDAL').val()
            };
            $.ajax({
                url: BRAKEPEDAL,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_CLUTCH_PEDAL() {
    $("#CLUTCH_PEDAL").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                CLUTCH_PEDAL: $('#CLUTCH_PEDAL').val()
            };
            $.ajax({
                url: CLUTCHPEDAL,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_SPOOL_VALUE() {
    $("#SPOOL_VALUE").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                SPOOL_VALUE: $('#SPOOL_VALUE').val()
            };
            $.ajax({
                url: SPOOLVALUE,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_TANDEM_PUMP() {
    $("#TANDEM_PUMP").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                TANDEM_PUMP: $('#TANDEM_PUMP').val()
            };
            $.ajax({
                url: TANDEMPUMP,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_FENDER() {
    $("#FENDER").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                FENDER: $('#FENDER').val()
            };
            $.ajax({
                url: FENDER,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_RearAxel() {
    $("#RearAxel").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                RearAxel: $('#RearAxel').val()
            };
            $.ajax({
                url: RearAxel,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_Hydraulic() {
    $("#Hydraulic").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                Hydraulic: $('#Hydraulic').val()
            };
            $.ajax({
                url: Hydraulic,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_FrontTyre() {
    $("#FrontTyre").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                FrontTyre: $('#FrontTyre').val()
            };
            $.ajax({
                url: FrontTyre,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_RHFrontTyre() {
    $("#RHFrontTyre").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                RHFrontTyre: $('#RHFrontTyre').val()
            };
            $.ajax({
                url: RHFrontTyre,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_Backend() {
    $("#Backend").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                Backend: $('#Backend').val()
            };
            $.ajax({
                url: Backend,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_SteeringMotor() {
    $("#SteeringMotor").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                SteeringMotor: $('#SteeringMotor').val()
            };
            $.ajax({
                url: SteeringMotor,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_SteeringCylinder() {
    $("#SteeringCylinder").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                SteeringCylinder: $('#SteeringCylinder').val()
            };
            $.ajax({
                url: SteeringCylinder,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_ClusterAssembly() {
    $("#ClusterAssembly").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                ClusterAssembly: $('#ClusterAssembly').val()
            };
            $.ajax({
                url: ClusterAssembly,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_StartorMotor() {
    $("#StartorMotor").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                StartorMotor: $('#StartorMotor').val()
            };
            $.ajax({
                url: StartorMotor,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_Rops() {
    $("#Rops").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                Rops: $('#Rops').val()
            };
            $.ajax({
                url: Rops,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_FENDER_RAILING() {
    $("#FENDER_RAILING").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                FENDER_RAILING: $('#FENDER_RAILING').val()
            };
            $.ajax({
                url: FENDERRAILING,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_HEAD_LAMP() {
    $("#HEAD_LAMP").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                HEAD_LAMP: $('#HEAD_LAMP').val()
            };
            $.ajax({
                url: HEADLAMP,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_STEERING_WHEEL() {
    $("#STEERING_WHEEL").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                STEERING_WHEEL: $('#STEERING_WHEEL').val()
            };
            $.ajax({
                url: STEERINGWHEEL,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_REAR_HOOD_WIRING_HARNESS() {
    $("#REAR_HOOD_WIRING_HARNESS").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                REAR_HOOD_WIRING_HARNESS: $('#REAR_HOOD_WIRING_HARNESS').val()
            };
            $.ajax({
                url: REARHOODHARNESS,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}
function AC_SEAT() {
    $("#SEAT").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#Plant').val(),
                Family: $('#Family').val(),
                SEAT: $('#SEAT').val()
            };
            $.ajax({
                url: SEAT,
                type: "POST",
                contentType: 'application/json;charset=utf-8',
                dataType: "json",
                data: JSON.stringify({ data: Data }),
                success: function (data) {
                    response($.map(data, function (item) {

                        return { label: item.Text, value: item.Text };
                    }))
                },
                error: function (err) {
                    alert(err);
                }
            });
        },

        minLength: 4

    });
}


$('.nav-pills a[href="#Menu1"]').on("click", function () {
    $("#SearchItem").show();
});
$('.nav-pills a[href="#Menu2"]').on("click", function () {
    $("#SearchItem").show();
});
$('.nav-pills a[href="#Menu3"]').on("click", function () {
    $("#SearchItem").hide();
});

$("#Plant").on("change", function () {
    ChangeLable();
    DDLFamilyByPlant();

});
function ChangeLable() {

    if ($('#Plant').val() == "T04") {
        $('.lblDynamic').html("SKID");
    }
    else {
        $('.lblDynamic').html("Backend");
    }
}

$("#ElectricMotorChk").change(function () {
    if (this.checked) {
        $('.lblMotor').html("Motor");
    } else {
        $('.lblMotor').html("Engine");
    }
});

$("#T3_Plant").on("change", function () {
    DDLT3_FamilyByT3_Plant();
});
$("#T3_Family").on("change", function () {
    Fill_T3_ItemMaster();
});
$("#gleSearch").on("change", function () {

    Clear();
    gleSearch_EditValueChanged();

});

$("#Add").on("click", function () {
    localStorage.setItem("IsTabChange", true);
    Add();
    //$('#ItemCode').val("");
    //$('#TransmissionChk').prop("checked", false);
    //$('#Transmission').val("");
    //$('#EngineChk').prop("checked", false);
    //$('#Engine').val("");
    //$('#RearTyreChk').prop("checked", false);
    //$('#RearTyre').val("");
    //$('#BatteryChk').prop("checked", false);
    //$('#Battery').val("");
    //$('#HydraulicPumpChk').prop("checked", false);
    //$('#HydraulicPump').val("");
    //$('#SteeringAssemblyChk').prop("checked", false);
    //$('#SteeringAssembly').val("");
    //$('#RadiatorAssemblyChk').prop("checked", false);
    //$('#RadiatorAssembly').val("");
    //$('#AlternatorChk').prop("checked", false);
    //$('#Alternator').val("");
    //$('#BRAKE_PEDAL').val("");
    //$('#CLUTCH_PEDAL').val("");
    //$('#SPOOL_VALUE').val("");
    //$('#TANDEM_PUMP').val("");
    //$('#FENDER').val("");
    //$('#Prefix1').val("");
    //$('#Prefix2').val("");
    //$('#Prefix3').val("");
    //$('#Prefix4').val("");
    //$('#Remarks').val("");
    //$('#EnableCarButtonChk').prop("checked", false);
    //$('#Seq_Not_RequireChk').prop("checked", false);
    //$('#GenerateSerialNoChk').prop("checked", false);
    //$('#DomesticExport').prop("checked", false);
    //$('#ShortDesc').val("");
    //$('#RearAxelChk').prop("checked", false);
    //$('#RearAxel').val("");
    //$('#HydraulicChk').prop("checked", false);
    //$('#Hydraulic').val("");
    //$('#FrontTyreChk').prop("checked", false);
    //$('#FrontTyre').val("");
    //$('#BackendChk').prop("checked", false);
    //$('#Backend').val("");
    //$('#SteeringMotorChk').prop("checked", false);
    //$('#SteeringMotor').val("");
    //$('#SteeringCylinderChk').prop("checked", false);
    //$('#SteeringCylinder').val("");
    //$('#ClusterAssemblyChk').prop("checked", false);
    //$('#ClusterAssembly').val("");
    //$('#StartorMotorChk').prop("checked", false);
    //$('#StartorMotor').val("");
    //$('#RopsChk').prop("checked", false);
    //$('#Rops').val("");
    //$('#FENDER_RAILING').val("");
    //$('#HEAD_LAMP').val("");
    //$('#STEERING_WHEEL').val("");
    //$('#REAR_HOOD_WIRING_HARNESS').val("");
    //$('#SEAT').val("");
    //$('#NoOfBoltsFrontAxel').val("");
    //$('#NoOfBoltsHydraulic').val("");
    //$('#NoOfBoltsFrontTYre').val("");
    //$('#NoOfBoltsRearTYre').val("");
    //$('#NoOfBoltsEnToruqe1').val("");
    //$('#NoOfBoltsEnToruqe2').val("");
    //$('#NoOfBoltsEnToruqe3').val("");
    //$('#NoOfBoltsTRANSAXELToruqe1').val("");
    //$('#NoOfBoltsTRANSAXELToruqe2').val("");

});

$("#Update").on("click", function () {
    localStorage.setItem("IsTabChange", true);
    Update();
    //$('#ItemCode').val("");
    //$('#TransmissionChk').prop("checked", false);
    //$('#Transmission').val("");
    //$('#EngineChk').prop("checked", false);
    //$('#Engine').val("");
    //$('#RearTyreChk').prop("checked", false);
    //$('#RearTyre').val("");
    //$('#BatteryChk').prop("checked", false);
    //$('#Battery').val("");
    //$('#HydraulicPumpChk').prop("checked", false);
    //$('#HydraulicPump').val("");
    //$('#SteeringAssemblyChk').prop("checked", false);
    //$('#SteeringAssembly').val("");
    //$('#RadiatorAssemblyChk').prop("checked", false);
    //$('#RadiatorAssembly').val("");
    //$('#AlternatorChk').prop("checked", false);
    //$('#Alternator').val("");
    //$('#BRAKE_PEDAL').val("");
    //$('#CLUTCH_PEDAL').val("");
    //$('#SPOOL_VALUE').val("");
    //$('#TANDEM_PUMP').val("");
    //$('#FENDER').val("");
    //$('#Prefix1').val("");
    //$('#Prefix2').val("");
    //$('#Prefix3').val("");
    //$('#Prefix4').val("");
    //$('#Remarks').val("");
    //$('#EnableCarButtonChk').prop("checked", false);
    //$('#Seq_Not_RequireChk').prop("checked", false);
    //$('#GenerateSerialNoChk').prop("checked", false);
    //$('#DomesticExport').prop("checked", false);
    //$('#ShortDesc').val("");
    //$('#RearAxelChk').prop("checked", false);
    //$('#RearAxel').val("");
    //$('#HydraulicChk').prop("checked", false);
    //$('#Hydraulic').val("");
    //$('#FrontTyreChk').prop("checked", false);
    //$('#FrontTyre').val("");
    //$('#BackendChk').prop("checked", false);
    //$('#Backend').val("");
    //$('#SteeringMotorChk').prop("checked", false);
    //$('#SteeringMotor').val("");
    //$('#SteeringCylinderChk').prop("checked", false);
    //$('#SteeringCylinder').val("");
    //$('#ClusterAssemblyChk').prop("checked", false);
    //$('#ClusterAssembly').val("");
    //$('#StartorMotorChk').prop("checked", false);
    //$('#StartorMotor').val("");
    //$('#RopsChk').prop("checked", false);
    //$('#Rops').val("");
    //$('#FENDER_RAILING').val("");
    //$('#HEAD_LAMP').val("");
    //$('#STEERING_WHEEL').val("");
    //$('#REAR_HOOD_WIRING_HARNESS').val("");
    //$('#SEAT').val("");
    //$('#NoOfBoltsFrontAxel').val("");
    //$('#NoOfBoltsHydraulic').val("");
    //$('#NoOfBoltsFrontTYre').val("");
    //$('#NoOfBoltsRearTYre').val("");
    //$('#NoOfBoltsEnToruqe1').val("");
    //$('#NoOfBoltsEnToruqe2').val("");
    //$('#NoOfBoltsEnToruqe3').val("");
    //$('#NoOfBoltsTRANSAXELToruqe1').val("");
    //$('#NoOfBoltsTRANSAXELToruqe2').val("");
});

$("#T3_Add").on("click", function () {
    Add_T3();
});

$("#Clear").on("click", function () {
    location.reload();
    //Clear();
    localStorage.setItem("IsTabChange", true);
});
$("#T3_Clear").on("click", function () {
    location.reload();
    //Clear();
    localStorage.setItem("IsTabChange", true);
});

function FillSearchItem() {
    var Data = {
        Plant: $('#Plant').val(),
        Family: $('#Family').val()
    };
    $.ajax({
        url: SearchItem,
        data: JSON.stringify({ obj: Data }),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        success: function (result) {
            $("#gleSearch").html(result);
            //$('#Add').show();
            //$('#Update').hide();
        },


        error: function (errormessage) {

        }

    });
};


function ddlplant() {
    $.ajax({
        url: updateUrl,
        type: "post",
        contenttype: "application/json;charset=utf-8",
        success: function (result) {
            $("#Plant").html(result);
            $("#T3_Plant").html(result);
            DDLFamilyByPlant();
            DDLT3_FamilyByT3_Plant();


        },
        error: function (errormessage) {
            //alert(errormessage.responsetext);
        }
    });
};

function DDLFamilyByPlant() {
    var selectedValue = $("#Plant").val();
    $.ajax({
        url: Family,
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify({ Plant: selectedValue }),
        success: function (result) {
            $("#Family").html(result);
            FillSearchItem();

        },
        error: function (errormessage) {
            //alert(errormessage.responseText);
        }
    });
};

//function BindT3Plant() {
//    $.ajax({
//        url: T3Plant,
//        type: "post",
//        contenttype: "application/json;charset=utf-8",
//        success: function (result) {
//            $("#T3_Plant").html(result);
//            $("#T3_Family").html(result);
//            //BindT4Family();
//            DDLT3_FamilyByT3_Plant();


//        },
//        error: function (errormessage) {
//            //alert(errormessage.responsetext);
//        }
//    });
//};

function DDLT3_FamilyByT3_Plant() {
    var selectedValue = $("#T3_Plant").val();
    $.ajax({
        url: T3Family,
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify({ Plant: selectedValue }),
        success: function (result) {
            $("#T3_Family").html(result);
            BindGrid();
            Fill_T3_ItemMaster();
        },
        error: function (errormessage) {
            //alert(errormessage.responseText);
        }
    });
};

////////////////////////////////////////////////////////////////////////////

function Fill_T3_ItemMaster() {
    var Data = {
        T3_Plant: $('#T3_Plant').val(),
        T3_Family: $('#T3_Family').val(),
        T3_ItemCode: $('#T3_ItemCode').val()
    };
    $("#divLoader").show();
    $.ajax({
        url: T3Item,
        data: JSON.stringify({ obj: Data }),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        cache: true,
        success: function (data) {

            $("#T3_ItemCode").empty();

            $.each(data.res, function (i, item) {
                $("#T3_ItemCode").append('<option value="' + item.Value + '">' + item.Text + '</option>');
            });

            $("#divLoader").hide();
        },
        error: function (errormessage) {

        }
    });
};

//Function to bind T3_Grid
function BindGrid() {
    $("#divLoader").show();
    var Data = {
        T3_Plant: $('#T3_Plant').val(),
        T3_Family: $('#T3_Family').val()
    };
    $.ajax({
        url: T3Grid,
        data: JSON.stringify(Data),
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        success: function (result) {
            $("#T3_grid").html(result);
            $("#divLoader").hide();
        },
        error: function (errormessage) {
            //alert(errormessage.responseText);
        }
    });
};

function Add_T3() {
    $("#divLoader").show();
    var Data = {
        T3_Plant: $('#T3_Plant').val(),
        T3_Family: $('#T3_Family').val(),
        T3_ItemCode: $('#T3_ItemCode option:selected').text(),
        T3_ItemCode_Value: $('#T3_ItemCode').val(),
        T3_chkNotGenrateSrno: $('#T3_chkNotGenrateSrno').prop("checked")
    };
    $.ajax({
        url: T3SubAssembly,
        data: JSON.stringify({ obj: Data }),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            $("#divLoader").hide();
            if (data.includes("Saved")) {
                $('#alert').append('<div class="alert alert-success role = "alert"><strong>' + data + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
                setTimeout(function () { $(".alert").alert('close'); }, 5000);
            }
            else if (data.includes("Invalid")) {
                $('#alert').append('<div class="alert alert-danger role = "alert"><strong>' + data + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
                setTimeout(function () { $(".alert").alert('close'); }, 5000);
            }
            else if (data.includes("Error")) {
                $('#alert').append('<div class="alert alert-danger role = "alert"><strong>' + data + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
                setTimeout(function () { $(".alert").alert('close'); }, 5000);
            }
            $('#T3_chkNotGenrateSrno').prop("checked", false);
            BindGrid();
        },
        error: function (errormessage) {

        }
    });
};

//////////////////////////////////////////////////////////////////////////

function Add() {
    $("#divLoader").show();
    var Data = {
        Plant: $('#Plant').val(),
        Family: $('#Family').val(),
        ItemCode: $('#ItemCode').val(),
        TransmissionChk: $('#TransmissionChk').prop("checked"),
        Transmission: $('#Transmission').val(),
        EngineChk: $('#EngineChk').prop("checked"),
        Engine: $('#Engine').val(),
        RearTyreChk: $('#RearTyreChk').prop("checked"),
        RearTyre: $('#RearTyre').val(),
        RHRearTyreChk: $('#RHRearTyreChk').prop("checked"),
        RHRearTyre: $('#RHRearTyre').val(),
        BatteryChk: $('#BatteryChk').prop("checked"),
        Battery: $('#Battery').val(),
        HydraulicPumpChk: $('#HydraulicPumpChk').prop("checked"),
        HydraulicPump: $('#HydraulicPump').val(),
        SteeringAssemblyChk: $('#SteeringAssemblyChk').prop("checked"),
        SteeringAssembly: $('#SteeringAssembly').val(),

        RadiatorAssemblyChk: $('#RadiatorAssemblyChk').prop("checked"),
        RadiatorAssembly: $('#RadiatorAssembly').val(),
        AlternatorChk: $('#AlternatorChk').prop("checked"),
        Alternator: $('#Alternator').val(),

        BRAKE_PEDAL: $('#BRAKE_PEDAL').val(),

        CLUTCH_PEDAL: $('#CLUTCH_PEDAL').val(),

        SPOOL_VALUE: $('#SPOOL_VALUE').val(),

        TANDEM_PUMP: $('#TANDEM_PUMP').val(),
        FENDER: $('#FENDER').val(),
        Prefix1: $('#Prefix1').val(),
        Prefix2: $('#Prefix2').val(),
        Prefix3: $('#Prefix3').val(),
        Prefix4: $('#Prefix4').val(),
        Remarks: $('#Remarks').val(),

        EnableCarButtonChk: $('#EnableCarButtonChk').prop("checked"),
        Seq_Not_RequireChk: $('#Seq_Not_RequireChk').prop("checked"),
        GenerateSerialNoChk: $('#GenerateSerialNoChk').prop("checked"),
        ElectricMotorChk: $('#ElectricMotorChk').prop("checked"),


        DomesticExport: $("input[name='DomesticExport']:checked").val(),
        /* TransmissionChk: $('#TransmissionChk').prop("checked"),*/

        ShortDesc: $('#ShortDesc').val(),

        RearAxelChk: $('#RearAxelChk').prop("checked"),
        RearAxel: $('#RearAxel').val(),
        HydraulicChk: $('#HydraulicChk').prop("checked"),
        Hydraulic: $('#Hydraulic').val(),
        FrontTyreChk: $('#FrontTyreChk').prop("checked"),
        FrontTyre: $('#FrontTyre').val(),
        RHFrontTyreChk: $('#RHFrontTyreChk').prop("checked"),
        RHFrontTyre: $('#RHFrontTyre').val(),
        BackendChk: $('#BackendChk').prop("checked"),
        Backend: $('#Backend').val(),
        SteeringMotorChk: $('#SteeringMotorChk').prop("checked"),
        SteeringMotor: $('#SteeringMotor').val(),
        SteeringCylinderChk: $('#SteeringCylinderChk').prop("checked"),
        SteeringCylinder: $('#SteeringCylinder').val(),
        ClusterAssemblyChk: $('#ClusterAssemblyChk').prop("checked"),
        ClusterAssembly: $('#ClusterAssembly').val(),
        StartorMotorChk: $('#StartorMotorChk').prop("checked"),
        StartorMotor: $('#StartorMotor').val(),
        RopsChk: $('#RopsChk').prop("checked"),
        Rops: $('#Rops').val(),

        FENDER_RAILING: $('#FENDER_RAILING').val(),
        HEAD_LAMP: $('#HEAD_LAMP').val(),
        STEERING_WHEEL: $('#STEERING_WHEEL').val(),
        REAR_HOOD_WIRING_HARNESS: $('#REAR_HOOD_WIRING_HARNESS').val(),
        SEAT: $('#SEAT').val(),

        NoOfBoltsFrontAxel: $("#NoOfBoltsFrontAxel").val(),
        NoOfBoltsHydraulic: $("#NoOfBoltsHydraulic").val(),
        NoOfBoltsFrontTYre: $("#NoOfBoltsFrontTYre").val(),
        NoOfBoltsRearTYre: $("#NoOfBoltsRearTYre").val(),
        NoOfBoltsEnToruqe1: $("#NoOfBoltsEnToruqe1").val(),
        NoOfBoltsEnToruqe2: $("#NoOfBoltsEnToruqe2").val(),
        NoOfBoltsEnToruqe3: $("#NoOfBoltsEnToruqe3").val(),
        NoOfBoltsTRANSAXELToruqe1: $("#NoOfBoltsTRANSAXELToruqe1").val(),
        NoOfBoltsTRANSAXELToruqe2: $("#NoOfBoltsTRANSAXELToruqe2").val()


    };
    $.ajax({
        url: updateSave,
        data: JSON.stringify({ obj: Data }),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            $("#divLoader").hide();
            if (data != "" || data != null) {
                if (data.includes != undefined) {
                    if (data.includes("Save")) {
                        $('#alert').append('<div class="alert alert-success role = "alert"><strong>' + data + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
                        //setTimeout(function () { $(".alert").alert('close'); }, 5000);
                        setTimeout(function () {
                            $.each($('.alert'), function () {
                                closeAlert(this);
                            });
                        }, 5000);

                    }
                    else {
                        $('#alert').append('<div class="alert alert-danger role = "alert"><strong>' + data + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
                        //setTimeout(function () { $(".alert").alert('close'); }, 5000);
                        setTimeout(function () {
                            $.each($('.alert'), function () {
                                closeAlert(this);
                            });
                        }, 5000);

                    }
                }
                else {
                    $('#alert').append('<div class="alert alert-danger role = "alert"><strong>' + data.Msg + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
                    //setTimeout(function () { $(".alert").alert('close'); }, 5000);
                    setTimeout(function () {
                        $.each($('.alert'), function () {
                            closeAlert(this);
                        });
                    }, 5000);

                }

            }
        },
        error: function (errormessage) {

        }
    });
};
function Update() {
    $("#divLoader").show();
    var modlType;
    if ($("#Dom").is(":checked")) {
        modlType = $("#Dom").val();
    }
    else if ($("#Exp").is(":checked")) {
        modlType = $("#Exp").val();
    }

    var Data = {
        Plant: $('#Plant').val(),
        Family: $('#Family').val(),
        ItemCode: $('#ItemCode').val(),
        TransmissionChk: $('#TransmissionChk').prop("checked"),
        Transmission: $('#Transmission').val(),
        EngineChk: $('#EngineChk').prop("checked"),
        Engine: $('#Engine').val(),
        RearTyreChk: $('#RearTyreChk').prop("checked"),
        RearTyre: $('#RearTyre').val(),
        RHRearTyreChk: $('#RHRearTyreChk').prop("checked"),
        RHRearTyre: $('#RHRearTyre').val(),
        BatteryChk: $('#BatteryChk').prop("checked"),
        Battery: $('#Battery').val(),
        HydraulicPumpChk: $('#HydraulicPumpChk').prop("checked"),
        HydraulicPump: $('#HydraulicPump').val(),
        SteeringAssemblyChk: $('#SteeringAssemblyChk').prop("checked"),
        SteeringAssembly: $('#SteeringAssembly').val(),

        RadiatorAssemblyChk: $('#RadiatorAssemblyChk').prop("checked"),
        RadiatorAssembly: $('#RadiatorAssembly').val(),
        AlternatorChk: $('#AlternatorChk').prop("checked"),
        Alternator: $('#Alternator').val(),

        BRAKE_PEDAL: $('#BRAKE_PEDAL').val(),

        CLUTCH_PEDAL: $('#CLUTCH_PEDAL').val(),

        SPOOL_VALUE: $('#SPOOL_VALUE').val(),

        TANDEM_PUMP: $('#TANDEM_PUMP').val(),
        FENDER: $('#FENDER').val(),
        Prefix1: $('#Prefix1').val(),
        Prefix2: $('#Prefix2').val(),
        Prefix3: $('#Prefix3').val(),
        Prefix4: $('#Prefix4').val(),
        Remarks: $('#Remarks').val(),

        EnableCarButtonChk: $('#EnableCarButtonChk').prop("checked"),
        Seq_Not_RequireChk: $('#Seq_Not_RequireChk').prop("checked"),
        GenerateSerialNoChk: $('#GenerateSerialNoChk').prop("checked"),
        ElectricMotorChk: $('#ElectricMotorChk').prop("checked"),

        DomesticExport: modlType,
        /*DomesticExport: $("input[name='DomesticExport']:checked").val(),*/
        /* TransmissionChk: $('#TransmissionChk').prop("checked"),*/

        ShortDesc: $('#ShortDesc').val(),

        RearAxelChk: $('#RearAxelChk').prop("checked"),
        RearAxel: $('#RearAxel').val(),
        HydraulicChk: $('#HydraulicChk').prop("checked"),
        Hydraulic: $('#Hydraulic').val(),
        FrontTyreChk: $('#FrontTyreChk').prop("checked"),
        FrontTyre: $('#FrontTyre').val(),
        RHFrontTyreChk: $('#RHFrontTyreChk').prop("checked"),
        RHFrontTyre: $('#RHFrontTyre').val(),
        BackendChk: $('#BackendChk').prop("checked"),
        Backend: $('#Backend').val(),
        SteeringMotorChk: $('#SteeringMotorChk').prop("checked"),
        SteeringMotor: $('#SteeringMotor').val(),
        SteeringCylinderChk: $('#SteeringCylinderChk').prop("checked"),
        SteeringCylinder: $('#SteeringCylinder').val(),
        ClusterAssemblyChk: $('#ClusterAssemblyChk').prop("checked"),
        ClusterAssembly: $('#ClusterAssembly').val(),
        StartorMotorChk: $('#StartorMotorChk').prop("checked"),
        StartorMotor: $('#StartorMotor').val(),
        RopsChk: $('#RopsChk').prop("checked"),
        Rops: $('#Rops').val(),

        FENDER_RAILING: $('#FENDER_RAILING').val(),
        HEAD_LAMP: $('#HEAD_LAMP').val(),
        STEERING_WHEEL: $('#STEERING_WHEEL').val(),
        REAR_HOOD_WIRING_HARNESS: $('#REAR_HOOD_WIRING_HARNESS').val(),
        SEAT: $('#SEAT').val(),

        NoOfBoltsFrontAxel: $("#NoOfBoltsFrontAxel").val(),
        NoOfBoltsHydraulic: $("#NoOfBoltsHydraulic").val(),
        NoOfBoltsFrontTYre: $("#NoOfBoltsFrontTYre").val(),
        NoOfBoltsRearTYre: $("#NoOfBoltsRearTYre").val(),
        NoOfBoltsEnToruqe1: $("#NoOfBoltsEnToruqe1").val(),
        NoOfBoltsEnToruqe2: $("#NoOfBoltsEnToruqe2").val(),
        NoOfBoltsEnToruqe3: $("#NoOfBoltsEnToruqe3").val(),
        NoOfBoltsTRANSAXELToruqe1: $("#NoOfBoltsTRANSAXELToruqe1").val(),
        NoOfBoltsTRANSAXELToruqe2: $("#NoOfBoltsTRANSAXELToruqe2").val()


    };
    $.ajax({
        url: updateUpdate,
        data: JSON.stringify({ obj: Data }),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {
            $("#divLoader").hide();
            if (data != "" || data != null) {
                if (data.includes("Update")) {
                    $('#alert').append('<div class="alert alert-success role = "alert"><strong>' + data + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
                    //setTimeout(function () { $(".alert").alert('close'); }, 5000);
                    setTimeout(function () {
                        $.each($('.alert'), function () {
                            closeAlert(this);
                        });
                    }, 5000);
                    

                }
                else {
                    $('#alert').append('<div class="alert alert-danger role = "alert"><strong>' + data + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
                    //setTimeout(function () { $(".alert").alert('close'); }, 5000);
                    setTimeout(function () {
                        $.each($('.alert'), function () {
                            closeAlert(this);
                        });
                    }, 5000);

                }

            }
        },
        error: function (errormessage) {

        }
    });
};

$("#Export").on("click", function () {
    DownloadExcel();
});

function DownloadExcel() {
    var Data = {
        //cbTractorMaster: $("input[name='cbTractorMaster']:checked").val(),
        Plant: $('#Plant').val(),
        Family: $('#Family').val()
    };
    $("#divLoader").show();
    $.ajax({
        url: ExportTractor,
        data: JSON.stringify(Data),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        //dataType: "json",
        success: function (data) {
            if (data.ErrorNo == "1") {
                $('#alert').append('<div class="alert ' + data.ID + '"role = "alert"><strong>' + data.Msg + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
                setTimeout(function () {
                    $.each($('.alert'), function () {
                        closeAlert(this);
                    });
                }, 5000);
            }

            else {
                $('#alert').append('<div class="alert ' + data.ID + '"role = "alert"><strong>' + data.Msg + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
                setTimeout(function () {
                    $.each($('.alert'), function () {
                        closeAlert(this);
                    });
                }, 5000);
                var file = data.ExcelName + ".xlsx";
                window.location.href = ExcelDownLoad + file;

            }

        },
        complete: function () {
            $("#divLoader").hide();
        },
        error: function (errormessage) {

        }
    });
}

function closeAlert(alert) {

    $(alert).hide();
};


function Clear() {
    // $('select#ItemCode').val(1).select2();
    $('#TransmissionChk').prop("checked", false);
    $('#Transmission').val(null).trigger("change");
    $('#EngineChk').prop("checked", false);
    $('#Engine').val('');
    $('#RearTyreChk').prop("checked", false);
    $('#RearTyre').val('');
    $('#RHRearTyreChk').prop("checked", false);
    $('#RHRearTyre').val('');
    $('#BatteryChk').prop("checked", false);
    $('#Battery').val('');
    $('#HydraulicPumpChk').prop("checked", false);
    $('#HydraulicPump').val('');
    $('#SteeringAssemblyChk').prop("checked", false);
    $('#SteeringAssembly').val('');
    $('#RadiatorAssemblyChk').prop("checked", false);
    $('#RadiatorAssembly').val('');
    $('#AlternatorChk').prop("checked", false);
    $('#Alternator').val('');
    $('#BRAKE_PEDAL').val('');
    $('#CLUTCH_PEDAL').val('');
    $('#SPOOL_VALUE').val('');
    $('#TANDEM_PUMP').val('');
    $('#FENDER').val("");
    $('#Prefix1').val("");
    $('#Prefix2').val("");
    $('#Prefix3').val("");
    $('#Prefix4').val("");
    $('#Remarks').val("");
    $('#EnableCarButtonChk').prop("checked", false);
    $('#GenerateSerialNoChk').prop("checked", false);
    $('#Seq_Not_RequireChk').prop("checked", false);
    $('#ElectricMotorChk').prop("checked", false);
    $("input[name='DomesticExport']:checked").val("Domestic");
    $('#TransmissionChk').prop("checked", false);
    $('#ShortDesc').val("");
    $('#RearAxelChk').prop("checked", false);
    $('#RearAxel').val('');
    $('#HydraulicChk').prop("checked", false);
    $('#Hydraulic').val('');
    $('#FrontTyreChk').prop("checked", false);
    $('#FrontTyre').val('');
    $('#RHFrontTyreChk').prop("checked", false);
    $('#RHFrontTyre').val('');
    $('#BackendChk').prop("checked", false);
    $('#Backend').val('');
    $('#SteeringMotorChk').prop("checked", false);
    $('#SteeringMotor').val('');
    $('#SteeringCylinderChk').prop("checked", false);
    $('#SteeringCylinder').val('');
    $('#ClusterAssemblyChk').prop("checked", false);
    $('#ClusterAssembly').val('');
    $('#StartorMotorChk').prop("checked", false);
    $('#StartorMotor').val('');
    $('#RopsChk').prop("checked", false);
    $('#Rops').val('');
    $('#FENDER_RAILING').val('');
    $('#HEAD_LAMP').val('');
    $('#STEERING_WHEEL').val('');
    $('#REAR_HOOD_WIRING_HARNESS').val('');
    $('#SEAT').val('');
    $("#NoOfBoltsFrontAxel").val("");
    $("#NoOfBoltsHydraulic").val("");
    $("#NoOfBoltsFrontTYre").val("");
    $("#NoOfBoltsRearTYre").val("");
    $("#NoOfBoltsEnToruqe1").val("");
    $("#NoOfBoltsEnToruqe2").val("");
    $("#NoOfBoltsEnToruqe3").val("");
    $("#NoOfBoltsTRANSAXELToruqe1").val("");
    $("#NoOfBoltsTRANSAXELToruqe2").val("");
};

function gleSearch_EditValueChanged() {
    
    var Data = {
        gleSearch: $('#gleSearch').val(),
        Plant: $('#Plant').val(),
        Family: $('#Family').val()
    };
    $.ajax({
        url: EditValueChanged,
        data: JSON.stringify({ obj: Data }),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (data) {


            if (data.Msg != "") {
                $('#alert').append('<div class="alert alert-danger role = "alert"><strong>' + data.Msg + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');

                setTimeout(function () {
                    $.each($('.alert'), function () {
                        closeAlert(this);
                    });
                }, 5000);

            }
            else {

                if (data.Result.ItemCode != "" && data.Result.ItemCode != null) {
                    $("#ItemCode").val(data.Result.ItemCode);

                }
                if (data.Result.Transmission != "" && data.Result.Transmission != null) {
                    $("#Transmission").val(data.Result.Transmission);

                }
                if (data.Result.Engine != "" && data.Result.Engine != null) {
                    $("#Engine").val(data.Result.Engine);

                }
                if (data.Result.RearTyre != "" && data.Result.RearTyre != null) {

                    $("#RearTyre").val(data.Result.RearTyre);

                }
                if (data.Result.RHRearTyre != "" && data.Result.RHRearTyre != null) {

                    $("#RHRearTyre").val(data.Result.RHRearTyre);

                }
                if (data.Result.Battery != "" && data.Result.Battery != null) {
                    $("#Battery").val(data.Result.Battery);

                }
                if (data.Result.HydraulicPump != "" && data.Result.HydraulicPump != null) {
                    $("#HydraulicPump").val(data.Result.HydraulicPump);

                }
                if (data.Result.SteeringAssembly != "" && data.Result.SteeringAssembly != null) {
                    $("#SteeringAssembly").val(data.Result.SteeringAssembly);

                }
                if (data.Result.RadiatorAssembly != "" && data.Result.RadiatorAssembly != null) {
                    $("#RadiatorAssembly").val(data.Result.RadiatorAssembly);

                }
                if (data.Result.Alternator != "" && data.Result.Alternator != null) {
                    $("#Alternator").val(data.Result.Alternator);

                }
                if (data.Result.BRAKE_PEDAL != "" && data.Result.BRAKE_PEDAL != null) {
                    $("#BRAKE_PEDAL").val(data.Result.BRAKE_PEDAL);

                }
                if (data.Result.CLUTCH_PEDAL != "" && data.Result.CLUTCH_PEDAL != null) {
                    $("#CLUTCH_PEDAL").val(data.Result.CLUTCH_PEDAL);

                }
                if (data.Result.SPOOL_VALUE != "" && data.Result.SPOOL_VALUE != null) {
                    $("#SPOOL_VALUE").val(data.Result.SPOOL_VALUE);

                }
                if (data.Result.TANDEM_PUMP != "" && data.Result.TANDEM_PUMP != null) {
                    $("#TANDEM_PUMP").val(data.Result.TANDEM_PUMP);

                }
                if (data.Result.FENDER != "" && data.Result.FENDER != null) {
                    $("#FENDER").val(data.Result.FENDER);

                }
                if (data.Result.RearAxel != "" && data.Result.RearAxel != null) {
                    $("#RearAxel").val(data.Result.RearAxel);

                }
                if (data.Result.Hydraulic != "" && data.Result.Hydraulic != null) {
                    $("#Hydraulic").val(data.Result.Hydraulic);

                }

                if (data.Result.FrontTyre != "" && data.Result.FrontTyre != null) {
                    $("#FrontTyre").val(data.Result.FrontTyre);

                }
                if (data.Result.RHFrontTyre != "" && data.Result.RHFrontTyre != null) {
                    $("#RHFrontTyre").val(data.Result.RHFrontTyre);

                }
                if (data.Result.Backend != "" && data.Result.Backend != null) {
                    $("#Backend").val(data.Result.Backend);

                }
                if (data.Result.SteeringMotor != "" && data.Result.SteeringMotor != null) {
                    $("#SteeringMotor").val(data.Result.SteeringMotor);

                }
                if (data.Result.SteeringCylinder != "" && data.Result.SteeringCylinder != null) {
                    $("#SteeringCylinder").val(data.Result.SteeringCylinder);

                }
                if (data.Result.ClusterAssembly != "" && data.Result.ClusterAssembly != null) {
                    $("#ClusterAssembly").val(data.Result.ClusterAssembly);

                }
                if (data.Result.StartorMotor != "" && data.Result.StartorMotor != null) {
                    $("#StartorMotor").val(data.Result.StartorMotor);

                }
                if (data.Result.Rops != "" && data.Result.Rops != null) {
                    $("#Rops").val(data.Result.Rops);

                }
                if (data.Result.FENDER_RAILING != "" && data.Result.FENDER_RAILING != null) {
                    $("#FENDER_RAILING").val(data.Result.FENDER_RAILING);

                }
                if (data.Result.HEAD_LAMP != "" && data.Result.HEAD_LAMP != null) {
                    $("#HEAD_LAMP").val(data.Result.HEAD_LAMP);

                }
                if (data.Result.STEERING_WHEEL != "" && data.Result.STEERING_WHEEL != null) {
                    $("#STEERING_WHEEL").val(data.Result.STEERING_WHEEL);

                }
                if (data.Result.REAR_HOOD_WIRING_HARNESS != "" && data.Result.REAR_HOOD_WIRING_HARNESS != null) {
                    $("#REAR_HOOD_WIRING_HARNESS").val(data.Result.REAR_HOOD_WIRING_HARNESS);

                }
                if (data.Result.SEAT != "" && data.Result.SEAT != null) {
                    $("#SEAT").val(data.Result.SEAT);

                }


                /////////////////////textboxes/////////////////////////

                $("#Prefix1").val(data.Result.Prefix1);
                $("#Prefix2").val(data.Result.Prefix2);
                $("#Prefix3").val(data.Result.Prefix3);
                $("#Prefix4").val(data.Result.Prefix4);
                $("#Remarks").val(data.Result.Remarks);
                $("#ShortDesc").val(data.Result.ShortDesc);

                /////////////////////checkboxes/////////////////////////
                $("#TransmissionChk").prop('checked', data.Result.TransmissionChk);
                $("#EngineChk").prop('checked', data.Result.EngineChk);
                $("#RearTyreChk").prop('checked', data.Result.RearTyreChk);
                $("#RHRearTyreChk").prop('checked', data.Result.RHRearTyreChk);
                $("#BatteryChk").prop('checked', data.Result.BatteryChk);
                $("#HydraulicPumpChk").prop('checked', data.Result.HydraulicPumpChk);
                $("#SteeringAssemblyChk").prop('checked', data.Result.SteeringAssemblyChk);
                $("#RadiatorAssemblyChk").prop('checked', data.Result.RadiatorAssemblyChk);
                $("#AlternatorChk").prop('checked', data.Result.AlternatorChk);
                $("#EnableCarButtonChk").prop('checked', data.Result.EnableCarButtonChk);
                $("#GenerateSerialNoChk").prop('checked', data.Result.GenerateSerialNoChk);
                $("#Seq_Not_RequireChk").prop('checked', data.Result.Seq_Not_RequireChk);              
                if ($("#ElectricMotorChk").prop('checked', data.Result.ElectricMotorChk)) {
                    if (data.Result.ElectricMotorChk == true)
                    {
                        $('.lblMotor').html("Motor");
                       
                       
                    } else
                    {
                        $('.lblMotor').html("Engine");
                       
                    } 
                }
                
                $('[name="DomesticExport"]').removeAttr('checked');
                if (data.Result.DomesticExport !== "" && data.Result.DomesticExport != null) {
                    if ("Domestic" == data.Result.DomesticExport) {
                        $('#Dom').prop('checked', true);
                    }
                    else if ("Export" == data.Result.DomesticExport) {
                        $('#Exp').prop('checked', true);
                        
                    }
                    
                }
                

                /*$("#DomesticExprt").prop('checked',data.Result.DomesticExport);*/
                $("#RearAxelChk").prop('checked', data.Result.RearAxelChk);
                $("#HydraulicChk").prop('checked', data.Result.HydraulicChk);
                $("#FrontTyreChk").prop('checked', data.Result.FrontTyreChk);
                $("#RHFrontTyreChk").prop('checked', data.Result.RHFrontTyreChk);
                $("#BackendChk").prop('checked', data.Result.BackendChk);
                $("#SteeringMotorChk").prop('checked', data.Result.SteeringMotorChk);
                $("#SteeringCylinderChk").prop('checked', data.Result.SteeringCylinderChk);
                $("#ClusterAssemblyChk").prop('checked', data.Result.ClusterAssemblyChk);
                $("#StartorMotorChk").prop('checked', data.Result.StartorMotorChk);
                $("#RopsChk").prop('checked', data.Result.RopsChk);

                ////////////////////toruqe tab////////////////////////////
                $("#NoOfBoltsFrontAxel").val(data.Result.NoOfBoltsFrontAxel);
                $("#NoOfBoltsHydraulic").val(data.Result.NoOfBoltsHydraulic);
                $("#NoOfBoltsFrontTYre").val(data.Result.NoOfBoltsFrontTYre);
                $("#NoOfBoltsEnToruqe1").val(data.Result.NoOfBoltsEnToruqe1);
                $("#NoOfBoltsEnToruqe2").val(data.Result.NoOfBoltsEnToruqe2);
                $("#NoOfBoltsEnToruqe3").val(data.Result.NoOfBoltsEnToruqe3);
                $("#NoOfBoltsRearTYre").val(data.Result.NoOfBoltsRearTYre);
                $("#NoOfBoltsTRANSAXELToruqe1").val(data.Result.NoOfBoltsTRANSAXELToruqe1);
                $("#NoOfBoltsTRANSAXELToruqe2").val(data.Result.NoOfBoltsTRANSAXELToruqe2);

                $('#Add').hide();
                $('#Update').show();
            }

        },
        error: function (errormessage) {

        }
    });
    
};

jQuery("#gleSearch").select2(
    //allowClear : true,
    //width: '100%',
);
jQuery("#T3_Plant,#T3_Family,#T3_ItemCode,#Plant,#Family").select2({
    allowClear: true,
    width: '100%',
}
);

