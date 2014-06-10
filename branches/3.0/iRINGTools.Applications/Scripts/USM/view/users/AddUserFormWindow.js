Ext.define('USM.view.users.AddUserFormWindow', {
  extend: 'Ext.window.Window',
  alias: 'widget.addUserformwindow',

  requires: [
   'USM.view.users.AddUserForm'
  ],
  border: false,
  height: 240,
  width: 550,
  minHeight:240,
  minWidth:400,
  layout: {
    type: 'fit'
  },

  initComponent: function() {
    var me = this;

    me.addEvents(
      'save',
      'reset'
    );
    Ext.applyIf(me, {
      items: [
        {
          xtype: 'adduserform'
        }
      ]
    });

    me.callParent(arguments);
  }

});