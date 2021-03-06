Ext.define('AM.view.common.MessagePanel', {
    extend: 'Ext.window.Window',
    alias: 'widget.messagepanel',

    width: 400,
    height: 225,
    modal: true,
    autoShow: true,
    msg: '',
    layout: 'fit',

    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            buttonAlign: 'center',
            buttons: [{
                text: 'OK',
                handler: function () {
                    this.up('window').close();
                }
            }],
            items: [{
                xtype: 'textarea',
                overflowY: 'auto',
                border: false,
                readOnly: true,
                value: me.msg
            }]
        });

        me.callParent(arguments);
    }
});