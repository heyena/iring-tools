Ext.define('AM.view.Viewport', {
    extend: 'Ext.container.Viewport',

    requires: [
        'AM.view.directory.DirectoryPanel',
        'AM.view.common.CenterPanel'
    ],

    layout: 'border',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            items: [
            {
                xtype: 'panel',
                region: 'north',
                border: false,
                height: 50,
                html: '<div id="header" class="banner">' +
                      '  <span style="float:left">' +
                      '    <img style="margin:0 5px 0 5px" src="./Content/img/iringtools-logo.png"/>' +
                      '       <font style="font-size:24px;font-family:Arial">Adapter Manager</font>' +
                      '  </span>' +
                      '  <span style="float:right;margin-top:16px">' +
                      '      <a href="http://iringug.org/wiki/index.php?title=IRINGTools" target="_blank" class="header-link">Help</a>' +
                      '      <a id="about-link" href="#" class="header-link">About</a>' +
                      '  </span>' +
                      '</div>'
            },
            {
                xtype: 'directorypanel',
                id: 'directoryTreeID',
                padding: 2,
                width: 260,
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
            }],
            renderTo: Ext.getBody()
        });

        me.callParent(arguments);

        Ext.onReady(function () {
            Ext.get('about-link').on('click', function () {
                var win = new Ext.Window({
                    title: 'About Adapter Manager',
                    autoLoad: 'about.aspx',
                    bodyStyle: 'background:#fff;padding:5px',
                    width: 725,
                    height: 508,
                    closable: true,
                    autoScroll: true,
                    modal: true
                });
                win.show();
            })
        });
    },

    onPanelAfterRender: function (component, eOpts) {
        Ext.getBody().on("contextmenu", Ext.emptyFn, null, {
            preventDefault: true
        });
    }
});