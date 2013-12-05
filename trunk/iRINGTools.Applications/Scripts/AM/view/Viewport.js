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
                height: 55,
                bodyCls: 'banner',
                layout: 'hbox',
                items: [
                {
                    xtype: 'image',
                    src: '../../../content/img/iringtools_logo.png',
                    padding: 2
                },
                {
                    xtype: 'label',
                    text: 'Adapter Manager',
                    cls: 'text-banner'
                },
                {
                    xtype: 'container',
                    layout: {
                        type: 'hbox',
                        pack: 'end'
                    },
                    flex: 1,
                    items: [
                    {
                        xtype: 'label',
                        cls: 'link-banner',
                        html: 'Help',
                        listeners: {
                            element: 'el',
                            click: function () {
                                window.open('http://iringug.org/wiki/index.php?title=IRINGTools');
                            },
                            mouseover: function (e, el) {
                                el.style.cursor = 'hand';
                            },
                            mouseout: function (e, el) {
                                el.style.cursor = 'pointer';
                            }
                        }
                    },
                    {
                        xtype: 'label',
                        cls: 'link-banner',
                        html: 'About',
                        listeners: {
                            element: 'el',
                            click: function () {
                                var win = new Ext.Window({
                                    title: 'About Adapter Manager',
                                    autoLoad: 'about.aspx',
                                    bodyStyle: 'background:#fff;padding:5px',
                                    width: 700,
                                    height: 500,
                                    closable: true,
                                    autoScroll: true,
                                    modal: true
                                });
                                win.show();
                            },
                            mouseover: function (e, el) {
                                el.style.cursor = 'hand';
                            },
                            mouseout: function (e, el) {
                                el.style.cursor = 'pointer';
                            }
                        }
                    }]
                }]
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
            }],
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