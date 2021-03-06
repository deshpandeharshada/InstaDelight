!function () {
    "use strict";
    var e = {};
    e.RELAXED = !1, e.IGNORE_RECORD_LENGTH = !1, e.IGNORE_QUOTES = !1, e.LINE_FEED_OK = !0,
    e.CARRIAGE_RETURN_OK = !0, e.DETECT_TYPES = !0, e.IGNORE_QUOTE_WHITESPACE = !0,
    e.DEBUG = !1, e.COLUMN_SEPARATOR = ",", e.ERROR_EOF = "UNEXPECTED_END_OF_FILE",
    e.ERROR_CHAR = "UNEXPECTED_CHARACTER", e.ERROR_EOL = "UNEXPECTED_END_OF_RECORD",
    e.WARN_SPACE = "UNEXPECTED_WHITESPACE";
    var r = '"', t = "\r", n = "\n", o = " ", E = "	", s = 0, u = 1, i = 2, c = 4;
    e.parse = function (c) {
        var d = e.result = [];
        e.offset = 0, e.str = c, e.record_begin(), e.debug("parse()", c);
        for (var f; ;) {
            if (f = c[e.offset++], e.debug("c", f), null == f) {
                e.escaped && e.error(e.ERROR_EOF), e.record && (e.token_end(), e.record_end()),
                e.debug("...bail", f, e.state, e.record), e.reset();
                break;
            }
            if (null == e.record) {
                if (e.RELAXED && (f == n || f == t && c[e.offset + 1] == n)) continue;
                e.record_begin();
            }
            if (e.state == s) {
                if ((f === o || f === E) && e.next_nonspace() == r) {
                    if (e.RELAXED || e.IGNORE_QUOTE_WHITESPACE) continue;
                    e.warn(e.WARN_SPACE);
                }
                if (f == r && !e.IGNORE_QUOTES) {
                    e.debug("...escaped start", f), e.escaped = !0, e.state = u;
                    continue;
                }
                e.state = u;
            }
            e.state == u && e.escaped ? f == r ? c[e.offset] == r ? (e.debug("...escaped quote", f),
            e.token += r, e.offset++) : (e.debug("...escaped end", f), e.escaped = !1, e.state = i) : (e.token += f,
            e.debug("...escaped add", f, e.token)) : f == t ? (c[e.offset] == n ? e.offset++ : e.CARRIAGE_RETURN_OK || e.error(e.ERROR_CHAR),
            e.token_end(), e.record_end()) : f == n ? (e.LINE_FEED_OK || e.RELAXED || e.error(e.ERROR_CHAR),
            e.token_end(), e.record_end()) : f == e.COLUMN_SEPARATOR ? e.token_end() : e.state == u ? (e.token += f,
            e.debug("...add", f, e.token)) : f === o || f === E ? e.IGNORE_QUOTE_WHITESPACE || e.error(e.WARN_SPACE) : e.RELAXED || e.error(e.ERROR_CHAR);
        }
        return d;
    }, e.json = function () {
        var e = new require("stream").Transform({
            objectMode: !0
        });
        return e._transform = function (r, t, n) {
            e.push(JSON.stringify("" + r) + require("os").EOL), n();
        }, e;
    }, e.stream = function () {
        var r = new require("stream").Transform({
            objectMode: !0
        });
        return r.EOL = "\n", r.prior = "", r.emitter = function (r) {
            return function (t) {
                r.push(e.parse(t + r.EOL));
            };
        }(r), r._transform = function (e, r, t) {
            var n = "" == this.prior ? ("" + e).split(this.EOL) : (this.prior + ("" + e)).split(this.EOL);
            this.prior = n.pop(), n.forEach(this.emitter), t();
        }, r._flush = function (e) {
            "" != this.prior && (this.emitter(this.prior), this.prior = ""), e();
        }, r;
    }, e.reset = function () {
        e.state = null, e.token = null, e.escaped = null, e.record = null, e.offset = null,
        e.result = null, e.str = null;
    }, e.next_nonspace = function () {
        for (var r, t = e.offset; t < e.str.length;) if (r = e.str[t++], r != o && r !== E) return r;
        return null;
    }, e.record_begin = function () {
        e.escaped = !1, e.record = [], e.token_begin(), e.debug("record_begin");
    }, e.record_end = function () {
        e.state = c, !e.IGNORE_RECORD_LENGTH && !e.RELAXED && e.result.length > 0 && e.record.length != e.result[0].length && e.error(e.ERROR_EOL),
        e.result.push(e.record), e.debug("record end", e.record), e.record = null;
    }, e.resolve_type = function (e) {
        return e.match(/^\d+(\.\d+)?$/) ? e = parseFloat(e) : e.match(/^(true|false)$/i) ? e = !!e.match(/true/i) : "undefined" === e ? e = void 0 : "null" === e && (e = null),
        e;
    }, e.token_begin = function () {
        e.state = s, e.token = "";
    }, e.token_end = function () {
        e.DETECT_TYPES && (e.token = e.resolve_type(e.token)), e.record.push(e.token), e.debug("token end", e.token),
        e.token_begin();
    }, e.debug = function () {
        e.DEBUG && console.log(arguments);
    }, e.dump = function (r) {
        return [r, "at char", e.offset, ":", e.str.substr(e.offset - 50, 50).replace(/\r/gm, "\\r").replace(/\n/gm, "\\n").replace(/\t/gm, "\\t")].join(" ");
    }, e.error = function (r) {
        var t = e.dump(r);
        throw e.reset(), t;
    }, e.warn = function (r) {
        var t = e.dump(r);
        try {
            return void console.warn(t);
        } catch (n) { }
        try {
            console.log(t);
        } catch (n) { }
    }, function (e, r, t) {
        var n;
        "undefined" != typeof module && module.exports ? module.exports = t() : "function" == typeof n && "object" == typeof n.amd ? n(t) : r[e] = t();
    }("CSV", Function("return this")(), function () {
        return e;
    });
}();