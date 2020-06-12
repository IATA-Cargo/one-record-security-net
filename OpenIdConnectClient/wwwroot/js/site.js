// Write your JavaScript code.
var gRowInPage = 10;


function SelectLanguageInit(objId, id) {
    if (objId == '') return;
    var result = $(objId).selectize({
        render: {
            option: function (data, escape) {
                return '<div>' +
                    '<span class="image"><img src="' +
                    data.image +
                    '" alt=""></span>' +
                    '<span class="title">' +
                    escape(data.text) +
                    '</span>' +
                    '</div>';
            },
            item: function (data, escape) {
                return '<div>' +
                    '<span class="image"><img src="' +
                    data.image +
                    '" alt=""></span>' +
                    escape(data.text) +
                    '</div>';
            }
        }
    });

    var obj = $(result)[0].selectize;
    if (id != '' && obj != null) {
        obj.setValue(id, true);
        return obj;
    }
    return result;
}

function GetClientCurrentDateTime() {
    var currentdate = new Date();
    return FillLengthData(currentdate.getFullYear(), 4) + "/"
         + FillLengthData(currentdate.getMonth() + 1, 2) + "/"
         + FillLengthData(currentdate.getDate(), 2) + " "
         + FillLengthData(currentdate.getHours(), 2) + ":"
         + FillLengthData(currentdate.getMinutes(), 2);
         //+ ":"
         //+ currentdate.getSeconds();
};

function FillLengthData(val, len, character) {
    val = val + '';
    if (character == null || character == '') {
        character = '0';
    }
    var temp = '';
    if (len == null || len == '') {
        return val;
    }
    for (var cnt = 0; cnt < len; cnt++) {
        temp = character + temp;
    }
    temp = temp + val;
    return temp.substr(temp.length - len);
};

function SelectInit(objId, id) {
    if (objId == '') return;
    var result = $(objId).selectize();

    var obj = $(result)[0].selectize;
    if (id != '' && obj != null) {
        obj.setValue(id, true);
        return obj;
    }
    return result;
}

function SelectSetValue(objId, id) {
    if (objId == '') return;
    var result = $(objId).selectize();

    if (id != '') {
        var obj = $(result)[0].selectize;
        obj.setValue(id, true);
    }
    return result;
}

function SelectGetValue(objId, id) {
    if (objId == '') return;
    var result = $(objId).selectize();

    if (id != '') {
        var obj = $(result)[0].selectize;
        obj.getValue(id, true);
    }
    return result;
}

window.tabler = {
    colors: {

        'blue': '#467fcf',
        'blue-darkest': '#0e1929',
        'blue-darker': '#1c3353',
        'blue-dark': '#3866a6',
        'blue-light': '#7ea5dd',
        'blue-lighter': '#c8d9f1',
        'blue-lightest': '#edf2fa',
        'azure': '#45aaf2',
        'azure-darkest': '#0e2230',
        'azure-darker': '#1c4461',
        'azure-dark': '#3788c2',
        'azure-light': '#7dc4f6',
        'azure-lighter': '#c7e6fb',
        'azure-lightest': '#ecf7fe',
        'indigo': '#6574cd',
        'indigo-darkest': '#141729',
        'indigo-darker': '#282e52',
        'indigo-dark': '#515da4',
        'indigo-light': '#939edc',
        'indigo-lighter': '#d1d5f0',
        'indigo-lightest': '#f0f1fa',
        'purple': '#a55eea',
        'purple-darkest': '#21132f',
        'purple-darker': '#42265e',
        'purple-dark': '#844bbb',
        'purple-light': '#c08ef0',
        'purple-lighter': '#e4cff9',
        'purple-lightest': '#f6effd',
        'pink': '#f66d9b',
        'pink-darkest': '#31161f',
        'pink-darker': '#622c3e',
        'pink-dark': '#c5577c',
        'pink-light': '#f999b9',
        'pink-lighter': '#fcd3e1',
        'pink-lightest': '#fef0f5',
        'red': '#e74c3c',
        'red-darkest': '#2e0f0c',
        'red-darker': '#5c1e18',
        'red-dark': '#b93d30',
        'red-light': '#ee8277',
        'red-lighter': '#f8c9c5',
        'red-lightest': '#fdedec',
        'orange': '#fd9644',
        'orange-darkest': '#331e0e',
        'orange-darker': '#653c1b',
        'orange-dark': '#ca7836',
        'orange-light': '#feb67c',
        'orange-lighter': '#fee0c7',
        'orange-lightest': '#fff5ec',
        'yellow': '#f1c40f',
        'yellow-darkest': '#302703',
        'yellow-darker': '#604e06',
        'yellow-dark': '#c19d0c',
        'yellow-light': '#f5d657',
        'yellow-lighter': '#fbedb7',
        'yellow-lightest': '#fef9e7',
        'lime': '#7bd235',
        'lime-darkest': '#192a0b',
        'lime-darker': '#315415',
        'lime-dark': '#62a82a',
        'lime-light': '#a3e072',
        'lime-lighter': '#d7f2c2',
        'lime-lightest': '#f2fbeb',
        'green': '#5eba00',
        'green-darkest': '#132500',
        'green-darker': '#264a00',
        'green-dark': '#4b9500',
        'green-light': '#8ecf4d',
        'green-lighter': '#cfeab3',
        'green-lightest': '#eff8e6',
        'teal': '#2bcbba',
        'teal-darkest': '#092925',
        'teal-darker': '#11514a',
        'teal-dark': '#22a295',
        'teal-light': '#6bdbcf',
        'teal-lighter': '#bfefea',
        'teal-lightest': '#eafaf8',
        'cyan': '#17a2b8',
        'cyan-darkest': '#052025',
        'cyan-darker': '#09414a',
        'cyan-dark': '#128293',
        'cyan-light': '#5dbecd',
        'cyan-lighter': '#b9e3ea',
        'cyan-lightest': '#e8f6f8',
        'gray': '#868e96',
        'gray-darkest': '#1b1c1e',
        'gray-darker': '#36393c',
        'gray-dark': '#6b7278',
        'gray-light': '#aab0b6',
        'gray-lighter': '#dbdde0',
        'gray-lightest': '#f3f4f5',
        'gray-dark': '#343a40',
        'gray-dark-darkest': '#0a0c0d',
        'gray-dark-darker': '#15171a',
        'gray-dark-dark': '#2a2e33',
        'gray-dark-light': '#717579',
        'gray-dark-lighter': '#c2c4c6',
        'gray-dark-lightest': '#ebebec'
    }
};

/**
 *
 */
let hexToRgba = function (hex, opacity) {
    let result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    let rgb = result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;

    return 'rgba(' + rgb.r + ', ' + rgb.g + ', ' + rgb.b + ', ' + opacity + ')';
};

/**
 *
 */
$(document).ready(function () {
    /** Constant div card */
    const DIV_CARD = 'div.card';

    /** Initialize tooltips */
    $('[data-toggle="tooltip"]').tooltip();

    /** Initialize popovers */
    $('[data-toggle="popover"]').popover({
        html: true
    });

    /** Function for remove card */
    $('[data-toggle="card-remove"]').on('click', function (e) {
        let $card = $(this).closest(DIV_CARD);

        $card.remove();

        e.preventDefault();
        return false;
    });

    /** Function for collapse card */
    $('[data-toggle="card-collapse"]').on('click', function (e) {
        let $card = $(this).closest(DIV_CARD);

        $card.toggleClass('card-collapsed');

        e.preventDefault();
        return false;
    });

    /** Function for fullscreen card */
    $('[data-toggle="card-fullscreen"]').on('click', function (e) {
        let $card = $(this).closest(DIV_CARD);

        $card.toggleClass('card-fullscreen').removeClass('card-collapsed');

        e.preventDefault();
        return false;
    });

    /**  */
    if ($('[data-sparkline]').length) {
        let generateSparkline = function ($elem, data, params) {
            $elem.sparkline(data, {
                type: $elem.attr('data-sparkline-type'),
                height: '100%',
                barColor: params.color,
                lineColor: params.color,
                fillColor: 'transparent',
                spotColor: params.color,
                spotRadius: 0,
                lineWidth: 2,
                highlightColor: hexToRgba(params.color, .6),
                highlightLineColor: '#666',
                defaultPixelsPerValue: 5
            });
        };

            $('[data-sparkline]').each(function () {
                let $chart = $(this);

                generateSparkline($chart, JSON.parse($chart.attr('data-sparkline')), {
                    color: $chart.attr('data-sparkline-color')
                });
            });
    }

    /**  */
    if ($('.chart-circle').length) {
            $('.chart-circle').each(function () {
                let $this = $(this);

                $this.circleProgress({
                    fill: {
                        color: tabler.colors[$this.attr('data-color')] || tabler.colors.blue
                    },
                    size: $this.height(),
                    startAngle: -Math.PI / 4 * 2,
                    emptyFill: '#F4F4F4',
                    lineCap: 'round'
                });
            });
    }
});

function InitDeleteDataTable(tableId) {
    if (tableId == '') return;
    return $(tableId).DataTable({
        "paging": true,
        "ordering": true,
        "info": false,
        "lengthChange": false,
        "pageLength": gRowInPage,
        "searching": false,
        "fnDrawCallback": function (oSettings) {
            var dateTime = GetClientCurrentDateTime();
            var arr = $('.onlyForSelect');
            $(arr).html(dateTime);
        }
    });
}

function InitDataTable(tableId, cols, url, method, data, colDef) {
    if (tableId == '') return;
    if (!url) url = null;
    if (!data) data = function (d) { };
    if (!method) method = null;
    if (!cols) cols = [];
    if (!colDef) colDef = [];
    if (!url) {
        return $("#" + tableId).DataTable({
            "paging": true,
            "ordering": true,
            "info": false,
            "lengthChange": false,
            "pageLength": gRowInPage,
            "language": {
                "search": "",
                "searchPlaceholder": "Search..."
            },
            "columnDefs": colDef,
            "columns": cols,

        });
    } else {
        return $("#" + tableId).DataTable({
            "paging": true,
            "ordering": true,
            "info": false,
            "lengthChange": false,
            "pageLength": gRowInPage,
            "serverSide": true,
            "ajax": {
                "url": url, "type": method, "data": data, "error": function (xhr, error, code)
                {
                    if (xhr.status == 401) {
                        document.location.href = '/account/logout';
                    }
                }},
            "language": {
                "search": "",
                "searchPlaceholder": "Search..."
            },
            "columnDefs": colDef,
            "columns": cols,
        });
    }
}


function CallAjax(url, method, body, fncSuccess, fncErr, headers) {
    try {
        $.ajax({
            type: method,
            url: url,
            data: body,
            beforeSend: function (xhr) {
                $('#progressBarBg').css('display', 'block');
                $('#progressBarContent').css('display', 'block');
            },
            success: function (resp) {
                if (fncSuccess) fncSuccess(resp)
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                if (xhr.status == 401) {
                    document.location.href = '/account/logout';
                    return;
                }
                if (fncSuccess) fncErr(XMLHttpRequest, textStatus, errorThrown);
            },
            complete: function () {
                $('#progressBarBg').css('display', 'none');
                $('#progressBarContent').css('display', 'none');
            }
        });
    } catch (e) {
        fncErr(null, 'unknown', '')
    }
}

function AddEmptyToFirstSelectTag(id, text, value) {
    if (!!!id) return;
    if (!!!value) value = '';
    if (!!!text) text = '';
    var el = document.getElementById(id);
    if (!el) el = $('#' + id);
    $(el).prepend('<option value="' + value + '">' + text+ '</option>');
}

function SetValueSelectTag(id, value) {
    if (!!!id) return;
    if (!!!value) value = '';
    var el = document.getElementById(id);
    if (!el) el = $('#' + id);
    $(el).val(value);
}

function getStringInDB(val) {
    if (!val) return '';
    return val + '';
}

function copy2Clipboard(copyText) {
    if (copyText == null) return;
    var el = document.createElement('textarea');
    el.value = copyText;
    document.body.appendChild(el);
    el.select();
    //document.execCommand('copy');
    var successful = document.execCommand('copy');
    if (successful); //alert('Secret key has been copied.');
    document.body.removeChild(el);
    //return false;
}

function uiTabConfig(store_var_name) {
    
    if (localStorage != null
        && localStorage.getItem(store_var_name) != null
        && localStorage.getItem(store_var_name) != undefined) {
        var selectedTabID = localStorage.getItem(store_var_name);
        //var tab = $(".card-header").find("#" + selectedTabID);
        $("#" + selectedTabID + " a").trigger("click");

        var tab = $("#" + selectedTabID + " a").parent();
        setTimeout(function () {
            $('html, body').animate({
                //scrollTop: $(tab).offset().top - $(tab).height()
                scrollTop: $(tab).offset().top
            }, 300);
        }, 100);
    }
    //scroll selected section to top of screen
    $(".collapsed").click(function () {
        if ($(this).attr("aria-expanded") == 'false') {
            var tab = $(this).parent();
            setTimeout(function () {
                $('html, body').animate({
                    scrollTop: $(tab).offset().top - $(tab).height()                    
                }, 300);
            }, 100);
            //then save selected tab to localstorage
            //localStorage.setItem(store_var_name, $(tab).attr('id'));
        }
        else {
            //localStorage.removeItem(store_var_name);
        }
    });
}

function downloadTxtFile(content, filename) {
    var text = content;
    var element = document.createElement('a');
    element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text + ''));
    element.setAttribute('download', filename);
    element.style.display = 'none';
    document.body.appendChild(element);
    element.click();
    document.body.removeChild(element);
    return false;
}