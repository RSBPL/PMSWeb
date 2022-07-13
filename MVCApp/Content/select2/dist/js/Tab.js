/*!
   SpryMedia Ltd.

 This source file is free software, available under the following license:
   MIT license - http://datatables.net/license/mit

 This source file is distributed in the hope that it will be useful, but
 WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 or FITNESS FOR A PARTICULAR PURPOSE. See the license files for details.

 For details please refer to: http://www.datatables.net
 KeyTable 2.7.0
 ©2009-2022 SpryMedia Ltd - datatables.net/license
*/
var $jscomp = $jscomp || {};
$jscomp.scope = {};
$jscomp.arrayIteratorImpl = function (c) {
    var h = 0;
    return function () {
        return h < c.length ? { done: !1, value: c[h++] } : { done: !0 };
    };
};
$jscomp.arrayIterator = function (c) {
    return { next: $jscomp.arrayIteratorImpl(c) };
};
$jscomp.ASSUME_ES5 = !1;
$jscomp.ASSUME_NO_NATIVE_MAP = !1;
$jscomp.ASSUME_NO_NATIVE_SET = !1;
$jscomp.SIMPLE_FROUND_POLYFILL = !1;
$jscomp.ISOLATE_POLYFILLS = !1;
$jscomp.defineProperty =
    $jscomp.ASSUME_ES5 || "function" == typeof Object.defineProperties
        ? Object.defineProperty
        : function (c, h, k) {
            if (c == Array.prototype || c == Object.prototype) return c;
            c[h] = k.value;
            return c;
        };
$jscomp.getGlobal = function (c) {
    c = ["object" == typeof globalThis && globalThis, c, "object" == typeof window && window, "object" == typeof self && self, "object" == typeof global && global];
    for (var h = 0; h < c.length; ++h) {
        var k = c[h];
        if (k && k.Math == Math) return k;
    }
    throw Error("Cannot find global object");
};
$jscomp.global = $jscomp.getGlobal(this);
$jscomp.IS_SYMBOL_NATIVE = "function" === typeof Symbol && "symbol" === typeof Symbol("x");
$jscomp.TRUST_ES6_POLYFILLS = !$jscomp.ISOLATE_POLYFILLS || $jscomp.IS_SYMBOL_NATIVE;
$jscomp.polyfills = {};
$jscomp.propertyToPolyfillSymbol = {};
$jscomp.POLYFILL_PREFIX = "$jscp$";
var $jscomp$lookupPolyfilledValue = function (c, h) {
    var k = $jscomp.propertyToPolyfillSymbol[h];
    if (null == k) return c[h];
    k = c[k];
    return void 0 !== k ? k : c[h];
};
$jscomp.polyfill = function (c, h, k, m) {
    h && ($jscomp.ISOLATE_POLYFILLS ? $jscomp.polyfillIsolated(c, h, k, m) : $jscomp.polyfillUnisolated(c, h, k, m));
};
$jscomp.polyfillUnisolated = function (c, h, k, m) {
    k = $jscomp.global;
    c = c.split(".");
    for (m = 0; m < c.length - 1; m++) {
        var n = c[m];
        if (!(n in k)) return;
        k = k[n];
    }
    c = c[c.length - 1];
    m = k[c];
    h = h(m);
    h != m && null != h && $jscomp.defineProperty(k, c, { configurable: !0, writable: !0, value: h });
};
$jscomp.polyfillIsolated = function (c, h, k, m) {
    var n = c.split(".");
    c = 1 === n.length;
    m = n[0];
    m = !c && m in $jscomp.polyfills ? $jscomp.polyfills : $jscomp.global;
    for (var u = 0; u < n.length - 1; u++) {
        var w = n[u];
        if (!(w in m)) return;
        m = m[w];
    }
    n = n[n.length - 1];
    k = $jscomp.IS_SYMBOL_NATIVE && "es6" === k ? m[n] : null;
    h = h(k);
    null != h &&
        (c
            ? $jscomp.defineProperty($jscomp.polyfills, n, { configurable: !0, writable: !0, value: h })
            : h !== k &&
            (($jscomp.propertyToPolyfillSymbol[n] = $jscomp.IS_SYMBOL_NATIVE ? $jscomp.global.Symbol(n) : $jscomp.POLYFILL_PREFIX + n),
                (n = $jscomp.propertyToPolyfillSymbol[n]),
                $jscomp.defineProperty(m, n, { configurable: !0, writable: !0, value: h })));
};
$jscomp.initSymbol = function () { };
$jscomp.polyfill(
    "Symbol",
    function (c) {
        if (c) return c;
        var h = function (n, u) {
            this.$jscomp$symbol$id_ = n;
            $jscomp.defineProperty(this, "description", { configurable: !0, writable: !0, value: u });
        };
        h.prototype.toString = function () {
            return this.$jscomp$symbol$id_;
        };
        var k = 0,
            m = function (n) {
                if (this instanceof m) throw new TypeError("Symbol is not a constructor");
                return new h("jscomp_symbol_" + (n || "") + "_" + k++, n);
            };
        return m;
    },
    "es6",
    "es3"
);
$jscomp.initSymbolIterator = function () { };
$jscomp.polyfill(
    "Symbol.iterator",
    function (c) {
        if (c) return c;
        c = Symbol("Symbol.iterator");
        for (var h = "Array Int8Array Uint8Array Uint8ClampedArray Int16Array Uint16Array Int32Array Uint32Array Float32Array Float64Array".split(" "), k = 0; k < h.length; k++) {
            var m = $jscomp.global[h[k]];
            "function" === typeof m &&
                "function" != typeof m.prototype[c] &&
                $jscomp.defineProperty(m.prototype, c, {
                    configurable: !0,
                    writable: !0,
                    value: function () {
                        return $jscomp.iteratorPrototype($jscomp.arrayIteratorImpl(this));
                    },
                });
        }
        return c;
    },
    "es6",
    "es3"
);
$jscomp.initSymbolAsyncIterator = function () { };
$jscomp.iteratorPrototype = function (c) {
    c = { next: c };
    c[Symbol.iterator] = function () {
        return this;
    };
    return c;
};
$jscomp.iteratorFromArray = function (c, h) {
    c instanceof String && (c += "");
    var k = 0,
        m = {
            next: function () {
                if (k < c.length) {
                    var n = k++;
                    return { value: h(n, c[n]), done: !1 };
                }
                m.next = function () {
                    return { done: !0, value: void 0 };
                };
                return m.next();
            },
        };
    m[Symbol.iterator] = function () {
        return m;
    };
    return m;
};
$jscomp.polyfill(
    "Array.prototype.keys",
    function (c) {
        return c
            ? c
            : function () {
                return $jscomp.iteratorFromArray(this, function (h) {
                    return h;
                });
            };
    },
    "es6",
    "es3"
);
(function (c) {
    "function" === typeof define && define.amd
        ? define(["jquery", "datatables.net"], function (h) {
            return c(h, window, document);
        })
        : "object" === typeof exports
            ? (module.exports = function (h, k) {
                h || (h = window);
                (k && k.fn.dataTable) || (k = require("datatables.net")(h, k).$);
                return c(k, h, h.document);
            })
            : c(jQuery, window, document);
})(function (c, h, k, m) {
    var n = c.fn.dataTable,
        u = 0,
        w = 0,
        t = function (a, b) {
            if (!n.versionCheck || !n.versionCheck("1.10.8")) throw "KeyTable requires DataTables 1.10.8 or newer";
            this.c = c.extend(!0, {}, n.defaults.keyTable, t.defaults, b);
            this.s = { dt: new n.Api(a), enable: !0, focusDraw: !1, waitingForDraw: !1, lastFocus: null, namespace: ".keyTable-" + u++, tabInput: null };
            this.dom = {};
            a = this.s.dt.settings()[0];
            if ((b = a.keytable)) return b;
            a.keytable = this;
            this._constructor();
        };
    c.extend(t.prototype, {
        blur: function () {
            this._blur();
        },
        enable: function (a) {
            this.s.enable = a;
        },
        enabled: function () {
            return this.s.enable;
        },
        focus: function (a, b) {
            this._focus(this.s.dt.cell(a, b));
        },
        focused: function (a) {
            if (!this.s.lastFocus) return !1;
            var b = this.s.lastFocus.cell.index();
            return a.row === b.row && a.column === b.column;
        },
        _constructor: function () {
            this._tabInput();
            var a = this,
                b = this.s.dt,
                e = c(b.table().node()),
                d = this.s.namespace,
                g = !1;
            "static" === e.css("position") && e.css("position", "relative");
            c(b.table().body()).on("click" + d, "th, td", function (f) {
                if (!1 !== a.s.enable) {
                    var q = b.cell(this);
                    q.any() && a._focus(q, null, !1, f);
                }
            });
            c(k).on("keydown" + d, function (f) {
                g || a._key(f);
            });
            if (this.c.blurable)
                c(k).on("mousedown" + d, function (f) {
                    c(f.target).parents(".dataTables_filter").length && a._blur();
                    c(f.target).parents().filter(b.table().container()).length ||
                        c(f.target).parents("div.DTE").length ||
                        c(f.target).parents("div.editor-datetime").length ||
                        c(f.target).parents("div.dt-datetime").length ||
                        c(f.target).parents().filter(".DTFC_Cloned").length ||
                        a._blur();
                });
            if (this.c.editor) {
                var p = this.c.editor;
                p.on("open.keyTableMain", function (f, q, r) {
                    "inline" !== q &&
                        a.s.enable &&
                        (a.enable(!1),
                            p.one("close" + d, function () {
                                a.enable(!0);
                            }));
                });
                if (this.c.editOnFocus)
                    b.on("key-focus" + d + " key-refocus" + d, function (f, q, r, v) {
                        a._editor(null, v, !0);
                    });
                b.on("key" + d, function (f, q, r, v, x) {
                    a._editor(r, x, !1);
                });
                c(b.table().body()).on("dblclick" + d, "th, td", function (f) {
                    !1 !== a.s.enable && b.cell(this).any() && ((a.s.lastFocus && this !== a.s.lastFocus.cell.node()) || a._editor(null, f, !0));
                });
                p.on("preSubmit", function () {
                    g = !0;
                })
                    .on("preSubmitCancelled", function () {
                        g = !1;
                    })
                    .on("submitComplete", function () {
                        g = !1;
                    });
            }
            b.on("stateSaveParams" + d, function (f, q, r) {
                r.keyTable = a.s.lastFocus ? a.s.lastFocus.cell.index() : null;
            });
            b.on("column-visibility" + d, function (f) {
                a._tabInput();
            });
            b.on("column-reorder" + d, function (f, q, r) {
                (f = a.s.lastFocus) && f.cell && ((q = f.relative.column), (f.cell[0][0].column = r.mapping.indexOf(q)), (f.relative.column = r.mapping.indexOf(q)));
            });
            b.on("draw" + d, function (f) {
                a._tabInput();
                if (!a.s.focusDraw && a.s.lastFocus) {
                    var q = a.s.lastFocus.relative,
                        r = b.page.info(),
                        v = q.row + r.start;
                    0 !== r.recordsDisplay && (v >= r.recordsDisplay && (v = r.recordsDisplay - 1), a._focus(v, q.column, !0, f));
                }
            });
            this.c.clipboard && this._clipboard();
            b.on("destroy" + d, function () {
                a._blur(!0);
                b.off(d);
                c(b.table().body())
                    .off("click" + d, "th, td")
                    .off("dblclick" + d, "th, td");
                c(k)
                    .off("mousedown" + d)
                    .off("keydown" + d)
                    .off("copy" + d)
                    .off("paste" + d);
            });
            var l = b.state.loaded();
            if (l && l.keyTable)
                b.one("init", function () {
                    var f = b.cell(l.keyTable);
                    f.any() && f.focus();
                });
            else this.c.focus && b.cell(this.c.focus).focus();
        },
        _blur: function (a) {
            if (this.s.enable && this.s.lastFocus) {
                var b = this.s.lastFocus.cell;
                c(b.node()).removeClass(this.c.className);
                this.s.lastFocus = null;
                a || (this._updateFixedColumns(b.index().column), this._emitEvent("key-blur", [this.s.dt, b]));
            }
        },
        _clipboard: function () {
            var a = this.s.dt,
                b = this,
                e = this.s.namespace;
            h.getSelection &&
                (c(k).on("copy" + e, function (d) {
                    d = d.originalEvent;
                    var g = h.getSelection().toString(),
                        p = b.s.lastFocus;
                    !g && p && (d.clipboardData.setData("text/plain", p.cell.render(b.c.clipboardOrthogonal)), d.preventDefault());
                }),
                    c(k).on("paste" + e, function (d) {
                        var g = d.originalEvent,
                            p = b.s.lastFocus,
                            l = k.activeElement;
                        d = b.c.editor;
                        var f;
                        !p ||
                            (l && "body" !== l.nodeName.toLowerCase()) ||
                            (g.preventDefault(),
                                h.clipboardData && h.clipboardData.getData ? (f = h.clipboardData.getData("Text")) : g.clipboardData && g.clipboardData.getData && (f = g.clipboardData.getData("text/plain")),
                                d ? ((g = b._inlineOptions(p.cell.index())), d.inline(g.cell, g.field, g.options).set(d.displayed()[0], f).submit()) : (p.cell.data(f), a.draw(!1)));
                    }));
        },
        _columns: function () {
            var a = this.s.dt,
                b = a.columns(this.c.columns).indexes(),
                e = [];
            a.columns(":visible").every(function (d) {
                -1 !== b.indexOf(d) && e.push(d);
            });
            return e;
        },
        _editor: function (a, b, e) {
            if (this.s.lastFocus && (!b || "draw" !== b.type)) {
                var d = this,
                    g = this.s.dt,
                    p = this.c.editor,
                    l = this.s.lastFocus.cell,
                    f = this.s.namespace + "e" + w++;
                if (!(c("div.DTE", l.node()).length || (null !== a && ((0 <= a && 9 >= a) || 11 === a || 12 === a || (14 <= a && 31 >= a) || (112 <= a && 123 >= a) || (127 <= a && 159 >= a))))) {
                    b && (b.stopPropagation(), 13 === a && b.preventDefault());
                    var q = function () {
                        var r = d._inlineOptions(l.index());
                        p.one("open" + f, function () {
                            p.off("cancelOpen" + f);
                            e || c("div.DTE_Field_InputControl input, div.DTE_Field_InputControl textarea").select();
                            g.keys.enable(e ? "tab-only" : "navigation-only");
                            g.on("key-blur.editor", function (v, x, y) {
                                p.displayed() && y.node() === l.node() && p.submit();
                            });
                            e && c(g.table().container()).addClass("dtk-focus-alt");
                            p.on("preSubmitCancelled" + f, function () {
                                setTimeout(function () {
                                    d._focus(l, null, !1);
                                }, 50);
                            });
                            p.on("submitUnsuccessful" + f, function () {
                                d._focus(l, null, !1);
                            });
                            p.one("close" + f, function () {
                                g.keys.enable(!0);
                                g.off("key-blur.editor");
                                p.off(f);
                                c(g.table().container()).removeClass("dtk-focus-alt");
                                d.s.returnSubmit && ((d.s.returnSubmit = !1), d._emitEvent("key-return-submit", [g, l]));
                            });
                        })
                            .one("cancelOpen" + f, function () {
                                p.off(f);
                            })
                            .inline(r.cell, r.field, r.options);
                    };
                    13 === a
                        ? ((e = !0),
                            c(k).one("keyup", function () {
                                q();
                            }))
                        : q();
                }
            }
        },
        _inlineOptions: function (a) {
            return this.c.editorOptions ? this.c.editorOptions(a) : { cell: a, field: m, options: m };
        },
        _emitEvent: function (a, b) {
            this.s.dt.iterator("table", function (e, d) {
                c(e.nTable).triggerHandler(a, b);
            });
        },
        _focus: function (a, b, e, d) {
            var g = this,
                p = this.s.dt,
                l = p.page.info(),
                f = this.s.lastFocus;
            d || (d = null);
            if (this.s.enable) {
                if ("number" !== typeof a) {
                    if (!a.any()) return;
                    var q = a.index();
                    b = q.column;
                    a = p.rows({ filter: "applied", order: "applied" }).indexes().indexOf(q.row);
                    if (0 > a) return;
                    l.serverSide && (a += l.start);
                }
                if (-1 !== l.length && (a < l.start || a >= l.start + l.length))
                    (this.s.focusDraw = !0),
                        (this.s.waitingForDraw = !0),
                        p
                            .one("draw", function () {
                                g.s.focusDraw = !1;
                                g.s.waitingForDraw = !1;
                                g._focus(a, b, m, d);
                            })
                            .page(Math.floor(a / l.length))
                            .draw(!1);
                else if (-1 !== c.inArray(b, this._columns())) {
                    l.serverSide && (a -= l.start);
                    l = p.cells(null, b, { search: "applied", order: "applied" }).flatten();
                    l = p.cell(l[a]);
                    if (f) {
                        if (f.node === l.node()) {
                            this._emitEvent("key-refocus", [this.s.dt, l, d || null]);
                            return;
                        }
                        this._blur();
                    }
                    this._removeOtherFocus();
                    f = c(l.node());
                    f.addClass(this.c.className);
                    this._updateFixedColumns(b);
                    if (e === m || !0 === e) this._scroll(c(h), c(k.body), f, "offset"), (e = p.table().body().parentNode), e !== p.table().header().parentNode && ((e = c(e.parentNode)), this._scroll(e, e, f, "position"));
                    this.s.lastFocus = { cell: l, node: l.node(), relative: { row: p.rows({ page: "current" }).indexes().indexOf(l.index().row), column: l.index().column } };
                    this._emitEvent("key-focus", [this.s.dt, l, d || null]);
                    p.state.save();
                }
            }
        },
        _key: function (a) {
            if (this.s.waitingForDraw) a.preventDefault();
            else {
                var b = this.s.enable;
                this.s.returnSubmit = ("navigation-only" !== b && "tab-only" !== b) || 13 !== a.keyCode ? !1 : !0;
                var e = !0 === b || "navigation-only" === b;
                if (b && (!(0 === a.keyCode || a.ctrlKey || a.metaKey || a.altKey) || (a.ctrlKey && a.altKey))) {
                    var d = this.s.lastFocus;
                    if (d)
                        if (this.s.dt.cell(d.node).any()) {
                            d = this.s.dt;
                            var g = this.s.dt.settings()[0].oScroll.sY ? !0 : !1;
                            if (!this.c.keys || -1 !== c.inArray(a.keyCode, this.c.keys))
                                switch (a.keyCode) {
                                    case 9:
                                        this._shift(a, a.shiftKey ? "left" : "right", !0);
                                        break;
                                    case 27:
                                        this.c.blurable && !0 === b && this._blur();
                                        break;
                                    case 33:
                                    case 34:
                                        e && !g && (a.preventDefault(), d.page(33 === a.keyCode ? "previous" : "next").draw(!1));
                                        break;
                                    case 35:
                                    case 36:
                                        e && (a.preventDefault(), (b = d.cells({ page: "current" }).indexes()), (e = this._columns()), this._focus(d.cell(b[35 === a.keyCode ? b.length - 1 : e[0]]), null, !0, a));
                                        break;
                                    case 37:
                                        e && this._shift(a, "left");
                                        break;
                                    case 38:
                                        e && this._shift(a, "up");
                                        break;
                                    case 39:
                                        e && this._shift(a, "right");
                                        break;
                                    case 40:
                                        e && this._shift(a, "down");
                                        break;
                                    case 113:
                                        if (this.c.editor) {
                                            this._editor(null, a, !0);
                                            break;
                                        }
                                    default:
                                        !0 === b && this._emitEvent("key", [d, a.keyCode, this.s.lastFocus.cell, a]);
                                }
                        } else this.s.lastFocus = null;
                }
            }
        },
        _removeOtherFocus: function () {
            var a = this.s.dt.table().node();
            c.fn.dataTable.tables({ api: !0 }).iterator("table", function (b) {
                this.table().node() !== a && this.cell.blur();
            });
        },
        _scroll: function (a, b, e, d) {
            var g = e[d](),
                p = e.outerHeight(),
                l = e.outerWidth(),
                f = b.scrollTop(),
                q = b.scrollLeft(),
                r = a.height();
            a = a.width();
            "position" === d && (g.top += parseInt(e.closest("table").css("top"), 10));
            g.top < f && b.scrollTop(g.top);
            g.left < q && b.scrollLeft(g.left);
            g.top + p > f + r && p < r && b.scrollTop(g.top + p - r);
            g.left + l > q + a && l < a && b.scrollLeft(g.left + l - a);
        },
        _shift: function (a, b, e) {
            var d = this.s.dt,
                g = d.page.info(),
                p = g.recordsDisplay,
                l = this._columns(),
                f = this.s.lastFocus;
            if (f) {
                var q = f.cell;
                q &&
                    ((f = d.rows({ filter: "applied", order: "applied" }).indexes().indexOf(q.index().row)),
                        g.serverSide && (f += g.start),
                        (g = d.columns(l).indexes().indexOf(q.index().column)),
                        (q = l[g]),
                        "rtl" === c(d.table().node()).css("direction") && ("right" === b ? (b = "left") : "left" === b && (b = "right")),
                        "right" === b ? (g >= l.length - 1 ? (f++, (q = l[0])) : (q = l[g + 1])) : "left" === b ? (0 === g ? (f--, (q = l[l.length - 1])) : (q = l[g - 1])) : "up" === b ? f-- : "down" === b && f++,
                        0 <= f && f < p && -1 !== c.inArray(q, l) ? (a && a.preventDefault(), this._focus(f, q, !0, a)) : e && this.c.blurable ? this._blur() : a && a.preventDefault());
            }
        },
        _tabInput: function () {
            var a = this,
                b = this.s.dt,
                e = null !== this.c.tabIndex ? this.c.tabIndex : b.settings()[0].iTabIndex;
            -1 != e &&
                (this.s.tabInput ||
                    ((e = c('<div><input type="text" tabindex="' + e + '"/></div>').css({ position: "absolute", height: 1, width: 0, overflow: "hidden" })),
                        e.children().on("focus", function (d) {
                            var g = b.cell(":eq(0)", a._columns(), { page: "current" });
                            g.any() && a._focus(g, null, !0, d);
                        }),
                        (this.s.tabInput = e)),
                    (e = this.s.dt.cell(":eq(0)", "0:visible", { page: "current", order: "current" }).node()) && c(e).prepend(this.s.tabInput));
        },
        _updateFixedColumns: function (a) {
            var b = this.s.dt,
                e = b.settings()[0];
            if (e._oFixedColumns) {
                var d = e.aoColumns.length - e._oFixedColumns.s.iRightColumns;
                (a < e._oFixedColumns.s.iLeftColumns || a >= d) && b.fixedColumns().update();
            }
        },
    });
    t.defaults = {
        blurable: !0,
        className: "focus",
        clipboard: !0,
        clipboardOrthogonal: "display",
        columns: "",
        editor: null,
        editOnFocus: !1,
        editorOptions: null,
        focus: null,
        keys: null,
        tabIndex: null
    };
    t.version = "2.7.0";
    c.fn.dataTable.KeyTable = t;
    c.fn.DataTable.KeyTable = t;
    n.Api.register("cell.blur()", function () {
        return this.iterator("table", function (a) {
            a.keytable && a.keytable.blur();
        });
    });
    n.Api.register("cell().focus()", function () {
        return this.iterator("cell", function (a, b, e) {
            a.keytable && a.keytable.focus(b, e);
        });
    });
    n.Api.register("keys.disable()", function () {
        return this.iterator("table", function (a) {
            a.keytable && a.keytable.enable(!1);
        });
    });
    n.Api.register("keys.enable()", function (a) {
        return this.iterator("table", function (b) {
            b.keytable && b.keytable.enable(a === m ? !0 : a);
        });
    });
    n.Api.register("keys.enabled()", function (a) {
        a = this.context;
        return a.length ? (a[0].keytable ? a[0].keytable.enabled() : !1) : !1;
    });
    n.Api.register("keys.move()", function (a) {
        return this.iterator("table", function (b) {
            b.keytable && b.keytable._shift(null, a, !1);
        });
    });
    n.ext.selector.cell.push(function (a, b, e) {
        b = b.focused;
        a = a.keytable;
        var d = [];
        if (!a || b === m) return e;
        for (var g = 0, p = e.length; g < p; g++) ((!0 === b && a.focused(e[g])) || (!1 === b && !a.focused(e[g]))) && d.push(e[g]);
        return d;
    });
    c(k).on("preInit.dt.dtk", function (a, b, e) {
        "dt" === a.namespace && ((a = b.oInit.keys), (e = n.defaults.keys), a || e) && ((e = c.extend({}, e, a)), !1 !== a && new t(b, e));
    });
    return t;
});
