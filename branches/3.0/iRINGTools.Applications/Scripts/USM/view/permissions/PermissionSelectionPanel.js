/*Ext.define('USM.view.permissions.PermissionSelectionPanel', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.permissionselectionpanel',
    store: 'PermissionS',
    initComponent: function () {
        var me = this;
        Ext.applyIf(me, {
            columns: [
                {
                    xtype: 'gridcolumn',
                    dataIndex: 'PermissionName',
                    text: 'Permission',
                    flex: 2
                    //menuDisabled: true
                }, 
				{
					xtype: 'checkcolumn',
					header: 'Select',
					dataIndex: '',
					flex: 2
				}
            ]
        });

        me.callParent(arguments);
    }

});
*/
Ext.define('USM.view.permissions.PermissionSelectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.permissionselectionpanel',

    bodyStyle: 'background:#fff;padding:10px',
    autoScroll: true,
    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                labelWidth: 110
            },
            items: [
            {
                xtype: 'combo',
				margin:'0 0 10 0',
                fieldLabel: 'Add and edit permission to role',
				labelWidth:'200',
				store: 'RoleS',
				queryMode: 'local',
				displayField: 'RoleName',
				valueField: 'RoleName'
            },
			{
                    xtype: 'grid',
                    forceFit: true,
                    store: 'PermissionS',
                    columns: [
								{
									xtype: 'gridcolumn',
									dataIndex: 'PermissionName',
									text: 'Permission',
									flex: 2
									//menuDisabled: true
								}, 
								{
									xtype: 'checkcolumn',
									header: 'Select',
									dataIndex: '',
									flex: 2
								}
					]
                }
            ]
        });

        me.callParent(arguments);
    }
});