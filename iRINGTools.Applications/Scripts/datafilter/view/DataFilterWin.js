Ext.define('df.view.DataFilterWin', {
    extend: 'Ext.window.Window',
    alias: 'widget.dataFilterWin',

    autoScroll: true,
    layout: {
        type: 'fit'
    },
    title: 'Data Filter',
    initComponent: function() {
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
                            handler: function(button, event) {
                                this.up('.window').close();

                            },
                            text: 'Cancel'
                        }
                    ]
                }
            ]
        });

        me.callParent(arguments);
    }

});

