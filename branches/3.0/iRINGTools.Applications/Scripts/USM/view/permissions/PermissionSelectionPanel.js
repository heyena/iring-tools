
Ext.define('USM.view.permissions.PermissionSelectionPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.permissionselectionpanel',

    bodyStyle: 'background:#fff;padding:10px',
    autoScroll: false,
    initComponent: function () {
        var me = this;

        Ext.applyIf(me, {
            defaults: {
                anchor: '100%',
                labelWidth: 110
            },
            items: [{
                xtype: 'panel',
                layout: 'form',
                bodyPadding: 2,
                border: false,
                items: [{
                    xtype: 'combobox',
                    disabled: false,
                    name: 'roleId',
                    allowBlank: false,
                    fieldLabel: 'Role',
                    emptyText: 'Select Role',
                    editable: false,
                    width: 100,
                    displayField: 'RoleName',
                    forceSelection: false,
                    store: 'RoleS',
                    valueField: 'RoleId'
                }]
            },
			{
				xtype: 'panel',
				autoScroll: true,
				items: [{
					xtype: 'grid',
					autoScroll : true,
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
				}]
			}]
        });

        me.callParent(arguments);
    }
});