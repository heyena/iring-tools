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
         'AdapterManagerController'
    ],
    launch: function () {
        Ext.QuickTips.init();
        var days = '';
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            var exptime = "; expires=" + date.toGMTString();
        } else {
            var exptime = null;
        }
        Ext.state.Manager.setProvider(new Ext.state.CookieProvider()); 

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
    }
});
