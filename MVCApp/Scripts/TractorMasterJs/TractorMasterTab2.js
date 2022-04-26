

$(document).ready(function () {
    $("#UpdateS").hide();
    jQuery.noConflict();

    BindT4Plant();
    AC_FrontSupport();
    AC_CenterAxel();
    AC_Slider();
    AC_SteeringColumn();
    AC_SteeringBase();
    AC_Lowerlink();
    AC_RBFrame();
    AC_FuelTank();
    AC_Cylinder();
    AC_FenderRH();
    AC_FenderLH();
    AC_FenderHarnessRH();
    AC_FenderHarnessLH();
    AC_FenderLamp4Types();
    AC_RBHarnessLH();
    AC_FrontRim();
    AC_RearRim();
    AC_TyreMake();
    AC_RearHood();
    AC_ClusterMeter();
    AC_IPHarness();
    AC_RadiatorShell();
    AC_AirCleaner();
    AC_HeadLampLH();
    AC_HeadLampRH();
    AC_FrontGrill();
    AC_MainHarnessBonnet();
    AC_Spindle();
    /*AC_Motor();*/
    AC_T4_ItemCode();

    //---------Add New----------
    AC_SLIDER_RH();
    AC_BRKPAD();
    AC_FRBRH();
    AC_FRBLH();
    AC_FRASRB();
    //------------------Add New Close-------------------
});


function AC_FrontSupport() {
    $("#FrontSupport").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FrontSupport: $('#FrontSupport').val()
            };
            $.ajax({
                url: FrontSupport,
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
function AC_CenterAxel() {
    $("#CenterAxel").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                CenterAxel: $('#CenterAxel').val()
            };
            $.ajax({
                url: CenterAxel,
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
function AC_Slider() {
    $("#Slider").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                Slider: $('#Slider').val()
            };
            $.ajax({
                url: Slider,
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
function AC_SteeringColumn() {
    $("#SteeringColumn").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                SteeringColumn: $('#SteeringColumn').val()
            };
            $.ajax({
                url: SteeringColumn,
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
function AC_SteeringBase() {
    $("#SteeringBase").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                SteeringBase: $('#SteeringBase').val()
            };
            $.ajax({
                url: SteeringBase,
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
function AC_Lowerlink() {
    $("#Lowerlink").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                Lowerlink: $('#Lowerlink').val()
            };
            $.ajax({
                url: Lowerlink,
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
function AC_RBFrame() {
    $("#RBFrame").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                RBFrame: $('#RBFrame').val()
            };
            $.ajax({
                url: RBFrame,
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
function AC_FuelTank() {
    $("#FuelTank").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FuelTank: $('#FuelTank').val()
            };
            $.ajax({
                url: FuelTank,
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
function AC_Cylinder() {
    $("#Cylinder").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                Cylinder: $('#Cylinder').val()
            };
            $.ajax({
                url: Cylinder,
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
function AC_FenderRH() {
    $("#FenderRH").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FenderRH: $('#FenderRH').val()
            };
            $.ajax({
                url: FenderRH,
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
function AC_FenderLH() {
    $("#FenderLH").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FenderLH: $('#FenderLH').val()
            };
            $.ajax({
                url: FenderLH,
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
function AC_FenderHarnessRH() {
    $("#FenderHarnessRH").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FenderHarnessRH: $('#FenderHarnessRH').val()
            };
            $.ajax({
                url: FenderHarnessRH,
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
function AC_FenderHarnessLH() {
    $("#FenderHarnessLH").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FenderHarnessLH: $('#FenderHarnessLH').val()
            };
            $.ajax({
                url: FenderHarnessLH,
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
function AC_FenderLamp4Types() {
    $("#FenderLamp4Types").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FenderLamp4Types: $('#FenderLamp4Types').val()
            };
            $.ajax({
                url: FenderLamp4Types,
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
function AC_RBHarnessLH() {
    $("#RBHarnessLH").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                RBHarnessLH: $('#RBHarnessLH').val()
            };
            $.ajax({
                url: RBHarnessLH,
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
function AC_FrontRim() {
    $("#FrontRim").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FrontRim: $('#FrontRim').val()
            };
            $.ajax({
                url: FrontRim,
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
function AC_RearRim() {
    $("#RearRim").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                RearRim: $('#RearRim').val()
            };
            $.ajax({
                url: RearRim,
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
function AC_TyreMake() {
    $("#TyreMake").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                TyreMake: $('#TyreMake').val()
            };
            $.ajax({
                url: TyreMake,
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
function AC_RearHood() {
    $("#RearHood").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                RearHood: $('#RearHood').val()
            };
            $.ajax({
                url: RearHood,
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
function AC_ClusterMeter() {
    $("#ClusterMeter").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                ClusterMeter: $('#ClusterMeter').val()
            };
            $.ajax({
                url: ClusterMeter,
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
function AC_IPHarness() {
    $("#IPHarness").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                IPHarness: $('#IPHarness').val()
            };
            $.ajax({
                url: IPHarness,
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
function AC_RadiatorShell() {
    $("#RadiatorShell").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                RadiatorShell: $('#RadiatorShell').val()
            };
            $.ajax({
                url: RadiatorShell,
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
function AC_AirCleaner() {
    $("#AirCleaner").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                AirCleaner: $('#AirCleaner').val()
            };
            $.ajax({
                url: AirCleaner,
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
function AC_HeadLampLH() {
    $("#HeadLampLH").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                HeadLampLH: $('#HeadLampLH').val()
            };
            $.ajax({
                url: HeadLampLH,
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
function AC_HeadLampRH() {
    $("#HeadLampRH").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                HeadLampRH: $('#HeadLampRH').val()
            };
            $.ajax({
                url: HeadLampRH,
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
function AC_FrontGrill() {
    $("#FrontGrill").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FrontGrill: $('#FrontGrill').val()
            };
            $.ajax({
                url: FrontGrill,
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
function AC_MainHarnessBonnet() {
    $("#MainHarnessBonnet").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                MainHarnessBonnet: $('#MainHarnessBonnet').val()
            };
            $.ajax({
                url: MainHarnessBonnet,
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
function AC_Spindle() {
    $("#Spindle").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                Spindle: $('#Spindle').val()
            };
            $.ajax({
                url: Spindle,
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
function AC_SLIDER_RH() {
    $("#Slider_RH").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                Slider_RH: $('#Slider_RH').val()
            };
            $.ajax({
                url: SliderRH,
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

        minLength: 4,
        response: function (event, ui) {
            if (!ui.content.length) {
                var noResult = { value: "", label: "No results found" };
                ui.content.push(noResult);

            } else {
                //$("#message").empty();
            }
        }
    });
}
function AC_BRKPAD() {
    $("#BRK_PAD").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                BRK_PAD: $('#BRK_PAD').val()
            };
            $.ajax({
                url: BRKPAD,
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

        minLength: 4,
        response: function (event, ui) {
            if (!ui.content.length) {
                var noResult = { value: "", label: "No results found" };
                ui.content.push(noResult);

            } else {
                //$("#message").empty();
            }
        }

    });
}
function AC_FRBRH() {
    $("#FRB_RH").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FRB_RH: $('#FRB_RH').val()
            };
            $.ajax({
                url: FRBRH,
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

        minLength: 4,
        response: function (event, ui) {
            if (!ui.content.length) {
                var noResult = { value: "", label: "No results found" };
                ui.content.push(noResult);

            } else {
                //$("#message").empty();
            }
        }

    });
}
function AC_FRBLH() {
    $("#FRB_LH").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FRB_LH: $('#FRB_LH').val()
            };
            $.ajax({
                url: FRBLH,
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

        minLength: 4,
        response: function (event, ui) {
            if (!ui.content.length) {
                var noResult = { value: "", label: "No results found" };
                ui.content.push(noResult);

            } else {
                //$("#message").empty();
            }
        }

    });
}
function AC_FRASRB() {
    $("#FR_AS_RB").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                FR_AS_RB: $('#FR_AS_RB').val()
            };
            $.ajax({
                url: FRASRB,
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

        minLength: 4,
        response: function (event, ui) {
            if (!ui.content.length) {
                var noResult = { value: "", label: "No results found" };
                ui.content.push(noResult);

            } else {
                //$("#message").empty();
            }
        }

    });
}

function AC_T4_ItemCode() {
    $("#T4_ItemCode").autocomplete({
        source: function (request, response) {

            var Data = {
                Plant: $('#T4_Plant').val(),
                Family: $('#T4_Family').val(),
                T4_ItemCode: $('#T4_ItemCode').val()
            };
            $.ajax({
                url: ItemCodes,
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

$('.nav-pills a[href="#Menu4"]').on("click", function () {
    $("#SearchItem").show();
});

$("#T4_Plant").on("change", function () {

    BindT4Family();
});

$("#gleSearch").on("change", function () {
    ClearS();
    gleSearch_EditValueChangedTab2();
});

$("#AddS").on("click", function () {
    localStorage.setItem("IsTabChange", true);
    AddS();
    //$('#T4_ItemCode').val("");
    //$('#FrontSupport').val("");
    //$('#CenterAxel').val("");
    //$('#Slider').val("");
    //$('#SteeringColumn').val("");
    //$('#SteeringBase').val("");
    //$('#Lowerlink').val("");
    //$('#RBFrame').val("");
    //$('#FuelTank').val("");
    //$('#Cylinder').val("");
    //$('#FenderRH').val("");
    //$('#FenderLH').val("");
    //$('#FenderHarnessRH').val("");
    //$('#FenderHarnessLH').val("");
    //$('#FenderLamp4Types').val("");
    //$('#RBHarnessLH').val("");
    //$('#FrontRim').val("");
    //$('#RearRim').val("");
    //$('#TyreMake').val("");
    //$('#RearHood').val("");
    //$('#ClusterMeter').val("");
    //$('#IPHarness').val("");
    //$('#RadiatorShell').val("");
    //$('#AirCleaner').val("");
    //$('#HeadLampRHLH').val("");
    //$('#FrontGrill').val("");
    //$('#MainHarnessBonnet').val("");
    //$('#Spindle').val("");

});
$("#UpdateS").on("click", function () {
    localStorage.setItem("IsTabChange", true);
    UpdateS();
    //$('#T4_ItemCode').val("");
    //$('#FrontSupport').val("");
    //$('#CenterAxel').val("");
    //$('#Slider').val("");
    //$('#SteeringColumn').val("");
    //$('#SteeringBase').val("");
    //$('#Lowerlink').val("");
    //$('#RBFrame').val("");
    //$('#FuelTank').val("");
    //$('#Cylinder').val("");
    //$('#FenderRH').val("");
    //$('#FenderLH').val("");
    //$('#FenderHarnessRH').val("");
    //$('#FenderHarnessLH').val("");
    //$('#FenderLamp4Types').val("");
    //$('#RBHarnessLH').val("");
    //$('#FrontRim').val("");
    //$('#RearRim').val("");
    //$('#TyreMake').val("");
    //$('#RearHood').val("");
    //$('#ClusterMeter').val("");
    //$('#IPHarness').val("");
    //$('#RadiatorShell').val("");
    //$('#AirCleaner').val("");
    //$('#HeadLampRHLH').val("");
    //$('#FrontGrill').val("");
    //$('#MainHarnessBonnet').val("");
    //$('#Spindle').val("");
});




$("#ClearS").on("click", function () {
    localStorage.setItem("IsTabChange", true);
    location.reload();
    //ClearS();
});


// function to Bind Tab4 BindT4_Plant

function BindT4Plant() {
    $.ajax({
        url: T4Plant,
        type: "post",
        contenttype: "application/json;charset=utf-8",
        success: function (result) {
            $("#T4_Plant").html(result);
            $("#T4_Family").html(result);
            BindT4Family();
            //DDLT3_FamilyByT3_Plant();


        },
        error: function (errormessage) {
            //alert(errormessage.responsetext);
        }
    });
};

// function to Bind Tab4 BindT4_Plant
function BindT4Family() {
    var selectedValue = $("#T4_Plant").val();
    $.ajax({
        url: T4Family,
        type: "POST",
        contentType: "application/json;charset=UTF-8",
        data: JSON.stringify({ T4_Plant: selectedValue }),
        success: function (result) {
            $("#T4_Family").html(result);
            FillSearchItemS();

        },
        error: function (errormessage) {
            //alert(errormessage.responseText);
        }
    });
};

function AddS() {
    $("#divLoader").show();
    var Data = {
        T4_Plant: $('#T4_Plant').val(),
        T4_Family: $('#T4_Family').val(),
        T4_ItemCode: $('#T4_ItemCode').val(),

        FrontSupport: $("#FrontSupport").val(),
        CenterAxel: $("#CenterAxel").val(),
        Slider: $("#Slider").val(),
        SteeringColumn: $("#SteeringColumn").val(),
        SteeringBase: $("#SteeringBase").val(),
        Lowerlink: $("#Lowerlink").val(),
        RBFrame: $("#RBFrame").val(),
        FuelTank: $("#FuelTank").val(),
        Cylinder: $("#Cylinder").val(),
        FenderRH: $("#FenderRH").val(),
        FenderLH: $("#FenderLH").val(),
        FenderHarnessRH: $("#FenderHarnessRH").val(),
        FenderHarnessLH: $("#FenderHarnessLH").val(),
        FenderLamp4Types: $("#FenderLamp4Types").val(),
        RBHarnessLH: $("#RBHarnessLH").val(),
        FrontRimChk: $('#FrontRimChk').prop("checked"),
        FrontRim: $("#FrontRim").val(),
        RearRimChk: $('#RearRimChk').prop("checked"),
        RearRim: $("#RearRim").val(),
        TyreMake: $("#TyreMake").val(),
        RearHood: $("#RearHood").val(),
        ClusterMeter: $("#ClusterMeter").val(),
        IPHarness: $("#IPHarness").val(),
        RadiatorShell: $("#RadiatorShell").val(),
        AirCleaner: $("#AirCleaner").val(),
        HeadLampLH: $("#HeadLampLH").val(),
        HeadLampRH: $("#HeadLampRH").val(),
        FrontGrill: $("#FrontGrill").val(),
        MainHarnessBonnet: $("#MainHarnessBonnet").val(),
        Spindle: $("#Spindle").val(),
        BRK_PAD: $("#BRK_PAD").val(),
        FRB_RH: $("#FRB_RH").val(),
        FRB_LH: $("#FRB_LH").val(),
        FR_AS_RB: $("#FR_AS_RB").val(),
        Slider_RH: $("#Slider_RH").val()
        
    };
    $.ajax({
        url: updateSaveS,
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
function UpdateS() {
    $("#divLoader").show();
    var Data = {
        T4_Plant: $('#T4_Plant').val(),
        T4_Family: $('#T4_Family').val(),
        T4_ItemCode: $('#T4_ItemCode').val(),

        FrontSupport: $("#FrontSupport").val(),
        CenterAxel: $("#CenterAxel").val(),
        Slider: $("#Slider").val(),
        SteeringColumn: $("#SteeringColumn").val(),
        SteeringBase: $("#SteeringBase").val(),
        Lowerlink: $("#Lowerlink").val(),
        RBFrame: $("#RBFrame").val(),
        FuelTank: $("#FuelTank").val(),
        Cylinder: $("#Cylinder").val(),
        FenderRH: $("#FenderRH").val(),
        FenderLH: $("#FenderLH").val(),
        FenderHarnessRH: $("#FenderHarnessRH").val(),
        FenderHarnessLH: $("#FenderHarnessLH").val(),
        FenderLamp4Types: $("#FenderLamp4Types").val(),
        RBHarnessLH: $("#RBHarnessLH").val(),
        FrontRimChk: $('#FrontRimChk').prop("checked"),
        FrontRim: $("#FrontRim").val(),
        RearRimChk: $('#RearRimChk').prop("checked"),
        RearRim: $("#RearRim").val(),
        TyreMake: $("#TyreMake").val(),
        RearHood: $("#RearHood").val(),
        ClusterMeter: $("#ClusterMeter").val(),
        IPHarness: $("#IPHarness").val(),
        RadiatorShell: $("#RadiatorShell").val(),
        AirCleaner: $("#AirCleaner").val(),
        HeadLampLH: $("#HeadLampLH").val(),
        HeadLampRH: $("#HeadLampRH").val(),
        FrontGrill: $("#FrontGrill").val(),
        MainHarnessBonnet: $("#MainHarnessBonnet").val(),
        Spindle: $("#Spindle").val(),
        Motor: $("#Motor").val(),
        BRK_PAD: $("#BRK_PAD").val(),
        FRB_RH: $("#FRB_RH").val(),
        FRB_LH: $("#FRB_LH").val(),
        FR_AS_RB: $("#FR_AS_RB").val(),
        Slider_RH: $("#Slider_RH").val()
    };
    $.ajax({
        url: updateUpdateS,
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
                    //$('#AddS').show();
                    //$('#UpdateS').hide();
                }
                else {
                    $('#alert').append('<div class="alert alert-success role = "alert"><strong>' + data + '</strong><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
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

$("#T4_Plant").select2({
    allowClear: true,
    width: '100%',
});
$("#T4_Family").select2({
    allowClear: true,
    width: '100%',
});

function ClearS() {
    $('#FrontSupport').val('');
    $('#CenterAxel').val('');
    $('#Slider').val('');
    $('#SteeringColumn').val('');
    $('#SteeringBase').val('');
    $('#Lowerlink').val('');
    $('#RBFrame').val('');
    $('#FuelTank').val('');
    $('#Cylinder').val('');
    $('#FenderRH').val('');
    $('#FenderLH').val('');
    $('#FenderHarnessRH').val("");
    $('#FenderHarnessLH').val("");
    $('#FenderLamp4Types').val("");
    $('#RBHarnessLH').val("");
    $('#FrontRimChk').prop("checked", false);
    $('#FrontRim').val("");
    $('#RearRimChk').prop("checked", false);
    $('#RearRim').val("");
    $('#TyreMake').val('');
    $('#RearHood').val('');
    $('#ClusterMeter').val('');
    $('#IPHarness').val('');
    $("#RadiatorShell").val("");
    $("#AirCleaner").val("");
    $("#HeadLampLH").val("");
    $("#HeadLampRH").val("");
    $("#FrontGrill").val("");
    $("#MainHarnessBonnet").val("");
    $("#Spindle").val("");
    $("#Motor").val("");
};



function gleSearch_EditValueChangedTab2() {
    var Data = {
        gleSearch: $('#gleSearch').val(),
        Plant: $('#T4_Plant').val(),
        Family: $('#T4_Family').val()
    };
    $.ajax({
        url: EditValueChangedTab2,
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

                if (data.Result.T4_ItemCode != "" && data.Result.T4_ItemCode != null) {
                    $("#T4_ItemCode").val(data.Result.T4_ItemCode);

                }
                if (data.Result.FrontSupport != "" && data.Result.FrontSupport != null) {
                    $("#FrontSupport").val(data.Result.FrontSupport);

                }
                if (data.Result.CenterAxel != "" && data.Result.CenterAxel != null) {
                    $("#CenterAxel").val(data.Result.CenterAxel);

                }
                if (data.Result.Slider != "" && data.Result.Slider != null) {
                    $("#Slider").val(data.Result.Slider);

                }
                if (data.Result.SteeringColumn != "" && data.Result.SteeringColumn != null) {
                    $("#SteeringColumn").val(data.Result.SteeringColumn);

                }
                if (data.Result.SteeringBase != "" && data.Result.SteeringBase != null) {
                    $("#SteeringBase").val(data.Result.SteeringBase);

                }
                if (data.Result.Lowerlink != "" && data.Result.Lowerlink != null) {
                    $("#Lowerlink").val(data.Result.Lowerlink);

                }
                if (data.Result.RBFrame != "" && data.Result.RBFrame != null) {
                    $("#RBFrame").val(data.Result.RBFrame);

                }
                if (data.Result.FuelTank != "" && data.Result.FuelTank != null) {
                    $("#FuelTank").val(data.Result.FuelTank);

                }
                if (data.Result.Cylinder != "" && data.Result.Cylinder != null) {
                    $("#Cylinder").val(data.Result.Cylinder);

                }
                if (data.Result.FenderRH != "" && data.Result.FenderRH != null) {
                    $("#FenderRH").val(data.Result.FenderRH);

                }
                if (data.Result.FenderLH != "" && data.Result.FenderLH != null) {
                    $("#FenderLH").val(data.Result.FenderLH);

                }
                if (data.Result.FenderHarnessRH != "" && data.Result.FenderHarnessRH != null) {
                    $("#FenderHarnessRH").val(data.Result.FenderHarnessRH);

                }
                if (data.Result.FenderHarnessLH != "" && data.Result.FenderHarnessLH != null) {
                    $("#FenderHarnessLH").val(data.Result.FenderHarnessLH);

                }
                if (data.Result.FenderLamp4Types != "" && data.Result.FenderLamp4Types != null) {
                    $("#FenderLamp4Types").val(data.Result.FenderLamp4Types);

                }
                if (data.Result.RBHarnessLH != "" && data.Result.RBHarnessLH != null) {
                    $("#RBHarnessLH").val(data.Result.RBHarnessLH);

                }
                if (data.Result.FrontRim != "" && data.Result.FrontRim != null) {
                    $("#FrontRim").val(data.Result.FrontRim);

                }
                if (data.Result.RearRim != "" && data.Result.RearRim != null) {
                    $("#RearRim").val(data.Result.RearRim);

                }
                if (data.Result.TyreMake != "" && data.Result.TyreMake != null) {
                    $("#TyreMake").val(data.Result.TyreMake);

                }
                if (data.Result.RearHood != "" && data.Result.RearHood != null) {
                    $("#RearHood").val(data.Result.RearHood);

                }
                if (data.Result.ClusterMeter != "" && data.Result.ClusterMeter != null) {
                    $("#ClusterMeter").val(data.Result.ClusterMeter);

                }
                if (data.Result.IPHarness != "" && data.Result.IPHarness != null) {
                    $("#IPHarness").val(data.Result.IPHarness);

                }
                if (data.Result.RadiatorShell != "" && data.Result.RadiatorShell != null) {
                    $("#RadiatorShell").val(data.Result.RadiatorShell);

                }
                if (data.Result.AirCleaner != "" && data.Result.AirCleaner != null) {
                    $("#AirCleaner").val(data.Result.AirCleaner);

                }
                if (data.Result.HeadLampLH != "" && data.Result.HeadLampLH != null) {
                    $("#HeadLampLH").val(data.Result.HeadLampLH);

                }
                if (data.Result.HeadLampRH != "" && data.Result.HeadLampRH != null) {
                    $("#HeadLampRH").val(data.Result.HeadLampRH);

                }
                if (data.Result.FrontGrill != "" && data.Result.FrontGrill != null) {
                    $("#FrontGrill").val(data.Result.FrontGrill);

                }
                if (data.Result.MainHarnessBonnet != "" && data.Result.MainHarnessBonnet != null) {
                    $("#MainHarnessBonnet").val(data.Result.MainHarnessBonnet);

                }
                if (data.Result.Spindle != "" && data.Result.Spindle != null) {
                    $("#Spindle").val(data.Result.Spindle);

                }
                if (data.Result.Motor != "" && data.Result.Motor != null) {
                    $("#Motor").val(data.Result.Motor);

                }

                //---------------------------Add New Start----------------------------------
                if (data.Result.Slider_RH != "" && data.Result.Slider_RH != null) {
                    $("#Slider_RH").val(data.Result.Slider_RH);

                }
                if (data.Result.BRK_PAD != "" && data.Result.BRK_PAD != null) {
                    $("#BRK_PAD").val(data.Result.BRK_PAD);

                }
                if (data.Result.FRB_RH != "" && data.Result.FRB_RH != null) {
                    $("#FRB_RH").val(data.Result.FRB_RH);

                }
                if (data.Result.FRB_LH != "" && data.Result.FRB_LH != null) {
                    $("#FRB_LH").val(data.Result.FRB_LH);

                }

                if (data.Result.FR_AS_RB != "" && data.Result.FR_AS_RB != null) {
                    $("#FR_AS_RB").val(data.Result.FR_AS_RB);

                }
              
                 //---------------------------Add New Start----------------------------------

                /////////////////////checkboxes/////////////////////////
                $("#FrontRimChk").prop('checked', data.Result.FrontRimChk);
                $("#RearRimChk").prop('checked', data.Result.RearRimChk);

                $('#AddS').hide();
                $('#UpdateS').show();
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

function FillSearchItemS() {
    var Data = {
        Plant: $('#T4_Plant').val(),
        Family: $('#T4_Family').val()
    };
    $.ajax({
        url: SearchItemS,
        data: JSON.stringify({ obj: Data }),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        success: function (result) {
            $("#gleSearch").html(result);
        },
        error: function (errormessage) {

        }
    });
};


