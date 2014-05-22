Ext.define('df.view.DataFilterWin', {
    extend: 'Ext.window.Window',
    alias: 'widget.dataFilterWin',
    //height: 400,
	minWidth:500,
	minHeight:400,
	modal:true,
	//maxHeight:450,
	//autoScroll : true,
    layout: {
        type: 'fit'
    },
    title: 'Data Filter',
    initComponent: function () {
        var me = this;
        Ext.applyIf(me, {
            items: {
                xtype: 'dataFilterForm'
            },
            dockedItems: [
                {
                    xtype: 'toolbar',
                    dock: 'bottom',
                    items: [
                        {
                            xtype: 'tbfill'
                        },
                        {
                            xtype: 'button',
                            action: 'saveDataFilter',
                            text: 'Save'
                        },
                        {
                            xtype: 'button',
                            handler: function (button, event) {
                                me.onReset();
                            },
                            text: 'Cancel'
                        }
                    ]
                }
            ],
            listeners: {
                close: {
                    fn: me.onReset,
                    scope: me
                }
            }
        });

        me.callParent(arguments);
    },
    onReset: function () {
        var me = this;
        //var win = me.up('.window');
        me.down('dataFilterForm').getForm().reset();
        me.destroy();
        panelEnable();

    }

});

