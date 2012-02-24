﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'AM': 'Scripts/AM'
    }
});

Ext.require('Ext.container.Viewport');

Ext.application({
  name: 'AM',
  enableQuickTips: true,
  appFolder: 'Scripts/AM',
  controllers: [
          'AdapterManagerController'
        , 'ConfigurationController'
    ],
  launch: function () {
    Ext.QuickTips.init();

    this.initStateProvider();

    Ext.create('AM.view.Viewport');

    Ext.get('about-link').on('click', function () {
      var win = Ext.create('Ext.window.Window', {
        title: 'About Adapter Manager',
        bodyStyle: 'background-color:white;padding:5px',
        width: 700,
        height: 500,
        closable: true,
        resizable: false,
        autoScroll: true,
        buttons: [{
          text: 'Close',
          handler: function () {
            Ext.getBody().unmask();
            win.close();
          }
        }],
        autoLoad: 'about.html',
        listeners: {
          close: {
            fn: function () {
              Ext.getBody().unmask();
            }
          }
        }
      });
      Ext.getBody().mask();
      win.show();
    });
  },

  initStateProvider: function () {
    var days = '';
    if (days) {
      var date = new Date();
      date.setTime(date.getTime()+(days*24*60*60*1000));
      var exptime = "; expires="+date.toGMTString();
    } else {
      var exptime = null;
    }
    // register provider with state manager.
    Ext.state.Manager.setProvider(Ext.create('Ext.state.CookieProvider', {
      path: '/',
      expires: exptime,
      domain: null,
      secure: false
    }));

  }
});

String.format = String.prototype.format = function () {
  var i = 0;
  var string = (typeof (this) == "function" && !(i++)) ? arguments[0] : this;
  for (; i < arguments.length; i++)
    string = string.replace(/\{\d+?\}/, arguments[i]);
  return string;
};

//Function to activate context menus with touch events
(function () {
    var EM = Ext.EventManager,
    body = document.body,
    activeTouches = {},
    onTouchStart = function (e, t) {
        var be = e.browserEvent;
        Ext.id(t);
        if (be.touches.length === 1) {
            activeTouches[t.id] = fireContextMenu.defer(1200, null, [e, t]);
        } else {
            cancelContextMenu(e, t);
        }
    },
    fireContextMenu = function (e, t) {
        var touch = e.browserEvent.touches[0];
        var me = document.createEvent("MouseEvents");
        me.initMouseEvent("contextmenu", true, true, window,
        1, // detail
        touch.screenX,
        touch.screenY,
        touch.clientX,
        touch.clientY,
        false, false, false, false, // key modifiers
        2, // button
        null // relatedTarget
    );
        t.dispatchEvent(me);
    },
    cancelContextMenu = function (e, t) {
        clearTimeout(activeTouches[t.id]);
    };
    if (navigator.userAgent.match(/iPad/i) != null) {
        Ext.onReady(function () {
            EM.on(body, "touchstart", onTouchStart);
            EM.on(body, "touchmove", cancelContextMenu);
            EM.on(body, "touchend", cancelContextMenu);
        });
    }
})();