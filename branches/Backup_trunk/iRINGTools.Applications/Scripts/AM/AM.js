Ext.Loader.setConfig({
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
          'AdapterManager'
        , 'Configuration'
        , 'Mapping'
        , 'Search'
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
        autoLoad: 'about.aspx',
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


Ext.ns('Ext.ux');
Ext.define('Ext.ux.plugin.GridPageSizer', {
  extend: 'Object',
  alias: 'plugin.PagingToolbarResizer',

  options: [5, 10, 15, 20, 25, 30, 50, 75, 100, 200, 300, 500, 1000],

  mode: 'remote',
  displayText: 'Records per Page',
  prependCombo: false,

  constructor: function (config) {
    Ext.apply(this, config);
    this.callParent();
  },

  init: function (pagingToolbar) {
    var comboStore = this.options;
    var combo = new Ext.form.field.ComboBox({
      typeAhead: false,
      triggerAction: 'all',
      forceSelection: true,
      selectOnFocus: true,
      editable: true,
      mode: this.mode,
      value: pagingToolbar.pageSize,
      width: 50,
      store: comboStore
    });

    combo.on('select', this.onPageSizeChanged, pagingToolbar);
    var index = 0;
    if (this.prependCombo) {
      index = pagingToolbar.items.indexOf(pagingToolbar.first);
      index--;
    } else {
      index = pagingToolbar.items.indexOf(pagingToolbar.refresh);
      pagingToolbar.insert(++index, '-');
    }
    pagingToolbar.insert(++index, this.displayText);
    pagingToolbar.insert(++index, combo);

    if (this.prependCombo) {
      pagingToolbar.insert(++index, '-');
    }
    pagingToolbar.on({
      beforedestroy: function () {
        combo.destroy();
      }
    });
  },
  onPageSizeChanged: function (combo) {
    this.store.pageSize = parseInt(combo.getRawValue(), 10);
    this.doRefresh();
  }
});

///overrides required to display correct text on dragstart

Ext.override(Ext.view.DragZone, {
    getDragText: function () {
        if (this.dragField) {
            var fieldValue = this.dragData.records[0].get(this.dragField);
            return Ext.String.format(this.dragText, fieldValue);
        } else {
            var count = this.dragData.records.length;
            return Ext.String.format(this.dragText, count, count == 1 ? '' : 's');
        }
    }
});
 

Ext.override(Ext.tree.plugin.TreeViewDragDrop, {
    onViewRender: function (view) {
        var me = this;

        if (me.enableDrag) {
            me.dragZone = Ext.create('Ext.tree.ViewDragZone', {
                view: view,
                ddGroup: me.dragGroup || me.ddGroup,
                dragText: me.dragText,
                dragField: me.dragField,
                repairHighlightColor: me.nodeHighlightColor,
                repairHighlight: me.nodeHighlightOnRepair
            });
        }

        if (me.enableDrop) {
            me.dropZone = Ext.create('Ext.tree.ViewDropZone', {
                view: view,
                ddGroup: me.dropGroup || me.ddGroup,
                allowContainerDrops: me.allowContainerDrops,
                appendOnly: me.appendOnly,
                allowParentInserts: me.allowParentInserts,
                expandDelay: me.expandDelay,
                dropHighlightColor: me.nodeHighlightColor,
                dropHighlight: me.nodeHighlightOnDrop
            });
        }
    }
});

var ifExistSibling = function (str, node, state) {
  var ifExist = false;
  var childNodes = node.childNodes;
  var repeatTime = 0;

  for (var i = 0; i < childNodes.length; i++) {
    if (childNodes[i].data.text == str) {
      if (state == 'new')
        ifExist = true;
      else {
        repeatTime++;
        if (repeatTime > 1)
          ifExist = true;
      }
    }
  }

  return ifExist;
};


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