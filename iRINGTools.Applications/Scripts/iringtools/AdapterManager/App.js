/*!
* Ext JS Library 3.2.1
* Copyright(c) 2006-2010 Ext JS, Inc.
* licensing@extjs.com
* http://www.extjs.com/license
*/
/**
* Ext.App
* @extends Ext.util.Observable
* @author Chris Scott
* @modified by Gert Jansen van Rensburg
*/
Ext.App = function (config) {

    // set up StateProvider
    this.initStateProvider();

    // array of views
    this.views = [];

    Ext.apply(this, config);
    if (!this.api.actions) { this.api.actions = {}; }

    // init when onReady fires.
    Ext.onReady(this.onReady, this);

    Ext.App.superclass.constructor.apply(this, arguments);
}
Ext.extend(Ext.App, Ext.util.Observable, {

    /***
    * response status codes.
    */
    STATUS_EXCEPTION: 'exception',
    STATUS_VALIDATION_ERROR: "validation",
    STATUS_ERROR: "error",
    STATUS_NOTICE: "notice",
    STATUS_OK: "ok",
    STATUS_HELP: "help",

    /**
    * @cfg {Object} api
    * remoting api.  should be defined in your own config js.
    */
    api: {
        url: null,
        type: null,
        actions: {}
    },

    // private, ref to message-box Element.
    msgCt: null,

    // @protected, onReady, executes when Ext.onReady fires.
    onReady: function () {
        // create the msgBox container.  used for App.setAlert
        this.msgCt = Ext.DomHelper.insertFirst(document.body, { id: 'msg-div' }, true);
        this.msgCt.setStyle('position', 'absolute');

        this.msgCt.setStyle('z-index', 9999);

        this.msgCt.setWidth(860);
    },

    initStateProvider: function () {
        /*
        * set days to be however long you think cookies should last
        */
        var days = '';        // expires when browser closes
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            var exptime = "; expires=" + date.toGMTString();
        } else {
            var exptime = null;
        }

        // register provider with state manager.
        Ext.state.Manager.setProvider(new Ext.state.CookieProvider({
            path: '/',
            expires: exptime,
            domain: null,
            secure: false
        }));
    },

    /**
    * registerView
    * register an application view component.
    * @param {Object} view
    */
    registerView: function (view) {
        this.views.push(view);
    },

    /**
    * getViews
    * return list of registered views
    */
    getViews: function () {
        return this.views;
    },

    /**
    * registerActions
    * registers new actions for API
    * @param {Object} actions
    */
    registerActions: function (actions) {
        Ext.apply(this.api.actions, actions);
    },

    /**
    * getAPI
    * return Ext Remoting api
    */
    getAPI: function () {
        return this.api;
    },

    /***
    * setAlert
    * show the message box.  Aliased to addMessage
    * @param {String} msg
    * @param {Bool} status
    */
    setAlert: function (state, title, msg) {
        if (msg == null)
            msg = "";
        this.addMessage(state, title, msg);
    },

    /***
    * adds a message to queue.
    * @param {String} msg
    * @param {Bool} status
    */
    addMessage: function (state, title, msg) {
        var delay = 3;    // <-- default delay of msg box is 1 second.
        if (state == false) {
            delay = 5;    // <-- when status is error, msg box delay is 3 seconds.
        }
        // add some smarts to msg's duration (div by 13.3 between 3 & 9 seconds)
        delay = msg.length / 13.3;
        if (delay < 3) {
            delay = 3;
        }
        else if (delay > 9) {
            delay = 59;
        }

        //this.msgCt.alignTo(document, 't-t');
        //Ext.DomHelper.append(this.msgCt, { html: this.buildMessageBox(state, title, String.format.apply(String, Array.prototype.slice.call(arguments, 2))) }, true).slideIn('t').pause(delay).ghost("t", { remove: true });
        Ext.Msg.show({
            title: title,
            msg: '<textarea class="dialog-textbox" readonly="yes">' + msg + '</textarea>',
            buttons: Ext.Msg.OK
        });
    },

    /***
    * buildMessageBox
    */
    buildMessageBox: function (state, title, msg) {
        var status;

        if (msg.length > 2000) {
            msg = msg.substring(0, 2000);
            msg = msg.substring(0, msg.lastIndexOf(')') + 1);
        }

        switch (state) {
            case true:
                status = this.STATUS_OK;
                break;
            case false:
                status = this.STATUS_ERROR;
                break;
        }
        return [
            '<div class="app-msg">',
            '<div class="x-box-tl"><div class="x-box-tr"><div class="x-box-tc"></div></div></div>',
            '<div class="x-box-ml">'
            	+ '<div class="x-box-mr">'
            		+ '<div class="x-box-mc">'
            			+ '<h3 class="x-icon-text icon-status-' + status + '">', title, '</h3>', msg,
            			'<div style=""><button id="closeButton" type="button" onclick="javascript:Ext.App.closeDomHelper()">Close</button></div>',
            		+'</div>'
            	+ '</div>'
            + '</div>',

            '<div class="x-box-bl"><div class="x-box-br"><div class="x-box-bc"></div></div></div>',
            '</div>'
        ].join('');
    },

    closeDomHelper: function () {
        alert("here");
        Ext.DomHelper.append(this.msgCt, true).slideIn('t').ghost("t", { remove: true });
    },

    /**
    * decodeStatusIcon
    * @param {Object} status
    */
    decodeStatusIcon: function (status) {
        iconCls = '';
        switch (status) {
            case true:
            case this.STATUS_OK:
                iconCls = this.ICON_OK;
                break;
            case this.STATUS_NOTICE:
                iconCls = this.ICON_NOTICE;
                break;
            case false:
            case this.STATUS_ERROR:
                iconCls = this.ICON_ERROR;
                break;
            case this.STATUS_HELP:
                iconCls = this.ICON_HELP;
                break;
        }
        return iconCls;
    },

    /***
    * setViewState, alias for Ext.state.Manager.set
    * @param {Object} key
    * @param {Object} value
    */
    setViewState: function (key, value) {
        Ext.state.Manager.set(key, value);
    },

    /***
    * getViewState, aliaz for Ext.state.Manager.get
    * @param {Object} cmd
    */
    getViewState: function (key) {
        return Ext.state.Manager.get(key);
    },

    /**
    * t
    * translation function.  needs to be implemented.  simply echos supplied word back currently.
    * @param {String} to translate
    * @return {String} translated.
    */
    t: function (words) {
        return words;
    },

    handleResponse: function (res) {
        if (res.type == this.STATUS_EXCEPTION) {
            return this.handleException(res);
        }
        if (res.message.length > 0) {
            this.setAlert(res.status, res.message);
        }
    },

    handleException: function (res) {
        Ext.MessageBox.alert(res.type.toUpperCase(), res.message);
    }
});

//Overrides
Ext.override(Ext.form.ComboBox, {
    setValue: function (v) {
        var text = v;
        if (this.valueField) {
            if (this.mode == 'remote' && !Ext.isDefined(this.store.totalLength)) {
                this.store.on('load', this.setValue.createDelegate(this, arguments), null, { single: true });
                if (this.store.lastOptions === null) {
                    var params;
                    if (this.valueParam) {
                        params = {};
                        params[this.valueParam] = v;
                    } else {
                        var q = this.allQuery;
                        this.lastQuery = q;
                        this.store.setBaseParam(this.queryParam, q);
                        params = this.getParams(q);
                    }
                    this.store.load({ params: params });
                }
                return;
            }
            var r = this.findRecord(this.valueField, v);
            if (r) {
                text = r.data[this.displayField];
            } else if (this.valueNotFoundText !== undefined) {
                text = this.valueNotFoundText;
            }
        }
        this.lastSelectionText = text;
        if (this.hiddenField) {
            this.hiddenField.value = v;
        }
        Ext.form.ComboBox.superclass.setValue.call(this, text);
        this.value = v;
    }
});