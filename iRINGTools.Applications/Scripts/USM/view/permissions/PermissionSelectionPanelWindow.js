Ext.define('USM.view.permissions.PermissionSelectionPanelWindow', {
  extend: 'Ext.window.Window',
  alias: 'widget.permissionselectionpanelwindow',

  requires: [
   'USM.view.permissions.PermissionSelectionPanel'
  ],
  border: false,
  modal: true,
  height: 310,
  width: 500,
  buttonType: '',
  autoScroll: false,
  layout: {
    type: 'fit'
  },
  title:'Add and Remove Permissions to Role',
  initComponent: function() {
    var me = this;

    me.addEvents(
      'save',
      'reset'
    );
    Ext.applyIf(me, {
	  
      items: [
        {
		   xtype: 'permissionselectionpanel'
        }
      ],
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
                            handler: function (button, event) {
								me.buttonType = 'apply',
                                me.onSave();
                            },
                            iconCls: 'icon-accept',
                            text: 'Apply',
							itemId:'apply'
                        },
						{
                            xtype: 'button',
                            handler: function (button, event) {
							    me.buttonType = 'save',
                                me.onSave();
                            },
                            iconCls: 'icon-accept',
                            text: 'Save',
							itemId:'save'
                        },
                        {
                            xtype: 'button',
                            handler: function (button, event) {
                                me.onReset();
                            },
                            iconCls: 'icon-cancel',
                            text: 'Cancel'
                        }
                    ]
                }
            ]
    });

    me.callParent(arguments);
  },
   onReset: function () {
        var me = this;
        var win = me.up('window');
        me.down('permissionselectionpanel').getForm().reset();
        me.destroy();
    },

    onSave: function () {
        var me = this;
        var message;
        var msg;
        var form = me.down('permissionselectionpanel');
		var permGrid = Ext.ComponentQuery.query("#permgridid");
		var val = '';
		var checkval = '';
		for ( var j = 0; j < permGrid[0].store.getCount(); j++) {
			var val = permGrid[0].store.getAt(j).get('Chk');
			if (val == true) {
				var commId = permGrid[0].store.getAt(j).get('PermissionId');
				checkval = checkval + commId + ',';
			}
			val = checkval.slice(0, -1);
		}
		form.getForm().findField('SelectedPermissions').setValue(val);
        if (form.getForm().isValid()) {
            msg = new Ext.window.MessageBox();
            msg.wait('Saving Permissions ....');
            form.submit({
                url: 'usersecuritymanager/saveRolePermissions',
				success: function (f, a) {
                    msg.close();
                    if(me.buttonType == 'save')
						me.destroy();
						
                    var message = 'Group saved successfully.';
                    showDialog(400, 50, 'Alert', message, Ext.Msg.OK, null);
                    return;
                },
                failure: function (f, a) {
                    msg.close();
                    me.destroy();
                }
            });
        } else {
            message = 'Please fil the form to save.';
            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
        }
    }

});