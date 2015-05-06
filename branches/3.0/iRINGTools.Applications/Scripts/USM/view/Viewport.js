
Ext.define('USM.view.Viewport', {
    extend: 'Ext.container.Viewport',
    alias: 'widget.viewport',
    id:'viewportid',
    layout: {
        type: 'border'
    },
    requires: ['USM.view.UserSecurityTabPanel', 'USM.view.SecurityGrid'],
    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'panel',
                    id: 'pageheaderid',
                    region: 'north',
                    height: 52,
                    html: '<div id="header" class="banner">' +
                          '  <span style="float:left">' +
                          '    <img style="margin:0 5px 0 5px" src="./Content/img/iringtools-logo.png"/>' +
                          '       <font style="font-size:24px;font-family:Arial">Security Manager</font>' +
                          '  </span>' +
                          '  <span style="float:right;margin-top:16px">' +
                          '      <a href="http://iringug.org/wiki/index.php?title=IRINGTools" target="_blank" class="header-link">Help</a>' +
                          '      <a id="about-link" href="#" class="header-link">About</a>' +
//                          '      <a id="login-link" href="#" class="header-link"> Login/Logout</a>' +
                          '  </span>' +
                          '</div>'
                },
                {
                    xtype: 'usersecuritytabpanel',
                    id: 'maincontent',
                    region: 'center',
					listeners: {
						afterrender: {
							fn: me.onPanelAfterRender,
							scope: me
						}
                }
                },
                {
                    xtype: 'securitygrid',
                    width: 260,
                    id: 'dp',
                    region: 'west',
                    split: true,
                    title: 'Security'
                }
            ]
        });
        me.callParent(arguments);
   },
   onPanelAfterRender: function (component, eOpts) {
        Ext.getBody().on("contextmenu", Ext.emptyFn, null, {
            preventDefault: true
        });
    }
});