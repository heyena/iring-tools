Ext.define('AM.view.common.ExceptionPanel', {
    extend: 'Ext.window.Window',
    alias: 'widget.exceptionpanel',
    msg: '',
    autoShow: true,
    autoHeight:true,
    height: 280,
    width: 470,
	minWidth:460,
	minHeight:280,
    layout:'anchor',
    
    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
        	modal: true,
            items: [{
                xtype: 'textareafield',
                border: false,
                anchor: '100% 40%',
                //height: 100,
                //width: 400,
                id: 'expValue',
                //overflowY: 'auto',
                readOnly: true
            },
            {
                xtype: 'fieldset',
                title: 'Details',
                collapsible: true,
                collapsed : true,
                anchor: '100% 100%',
                items: [
                    {
						xtype: 'textareafield',
						height: 200,
						anchor: '100% 60%',
						border: false,
						id: 'expValue2',
						//overflowY: 'auto',
						readOnly: true
					}
                ]
        }
            ]
        });

        me.callParent(arguments);
    }
});