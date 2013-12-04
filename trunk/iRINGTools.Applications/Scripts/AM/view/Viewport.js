Ext.define('AM.view.Viewport', {
    extend: 'Ext.container.Viewport',

    requires: [
    'AM.view.directory.DirectoryPanel',
    'AM.view.common.CenterPanel'
  ],

    layout: {
        type: 'border'
    },

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            items: [
        {
            xtype: 'container',
            region: 'north',
            height: 55,
            html: '<div id="header" class="exchangeBanner"><span style="float: left"><img alt="q" src="Content/img/iRINGTools_logo.png" style="margin: 0 0 0 11px; vertical-align: -20%" /><span style="margin: 0 0 0 6px;"><font size="5px" style="font-family: Arial, Helvetica, Sans-Serif">Adapter Manager</font></span> </span><span style="float: right; margin: 18px 36px 1px 0"><a href="http://iringug.org/wiki/index.php?title=IRINGTools" target="_blank" class="headerLnkBlack">Help</a>&nbsp;&nbsp;&nbsp;&nbsp;<a id="about-link" href="#" class="headerLnkBlack">About</a></span></div>'
        },
        {
            xtype: 'directorypanel',
            id: 'directoryTreeID',
            region: 'west',
            listeners: {
                afterrender: {
                    fn: me.onPanelAfterRender,
                    scope: me
                }
            }
        },
        {
            xtype: 'centerpanel',
            region: 'center'
        }
      ],
            renderTo: Ext.getBody()
        });

        me.callParent(arguments);
    },

    onPanelAfterRender: function (component, eOpts) {
        Ext.getBody().on("contextmenu", Ext.emptyFn, null, {
            preventDefault: true
        });
    }

});