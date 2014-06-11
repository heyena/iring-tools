Ext.define('USM.view.users.AddUserForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.adduserform',

  requires: [
    
  ],
  border: true,
  bodyStyle: 'padding:10px 5px 0',
  //method: 'POST',
  //url: 'directory/application',

  initComponent: function() {
    var me = this;

    me.initialConfig = Ext.apply({
      method: 'POST',
      //url: 'directory/application'
    }, me.initialConfig);

    Ext.applyIf(me, {
      defaults: {
        msgTarget: 'side',
        anchor: '100%'
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
              handler: function(button, event) {
                me.onSave();
              },
			  iconCls: 'icon-accept',
              text: 'Ok'
            },
            {
              xtype: 'button',
              handler: function(button, event) {
                me.onReset();
              },
			  iconCls: 'icon-cancel',
              text: 'Cancel'
            }
          ]
        }
      ],
      items: [
        {
          xtype: 'hiddenfield',
          name: 'userId'
        },
        {
          xtype: 'hiddenfield',
          name: 'siteId'
        },
        {
          xtype: 'textfield',
          fieldLabel: 'User Name',
          name: 'UserName',
		  maxLength:100,
          allowBlank: false
        },
        {
          xtype: 'textfield',
          fieldLabel: 'First Name',
          name: 'UserFirstName',
		  maxLength:50
        },
        {
          xtype: 'textfield',
          fieldLabel: 'Last Name',
          name: 'UserLastName',
		  maxLength:50
        },
         {
          xtype: 'textfield',
          fieldLabel: 'Email',
          name: 'UserEmail',
		  maxLength:50,
		  vtype: 'email'
        },
        {
          xtype: 'textfield',
          fieldLabel: 'Phone',
          name: 'UserPhone',
		  maxLength:50
        },
        {
          xtype: 'textfield',
          fieldLabel: 'Description',
          name: 'UserDesc',
		  maxLength:255
        },
        
      ]
    });
    me.callParent(arguments);
  },
  
    onSave: function () {
        /*var me = this;
        var win = me.up('window');
        var form = me.getForm();
        var folderName = form.findField('displayName').getValue();
        var state = form.findField('state').getValue();
        var contextNameField = form.findField('contextName');
		var node = me.node;
        if (form.findField('cacheDBConnStr').getValue() == this.cacheConnStrTpl)
            form.findField('cacheDBConnStr').setValue('');

        node.eachChild(function (n) {
            if (n.data.text == folderName) {
                if (state == 'new') {
                     Ext.widget('messagepanel', { title: 'Warning', msg: 'Scope name \"' + folderName + '\" already exists.'});
					//showDialog(400, 100, 'Warning', 'Scope name \"' + folderName + '\" already exists.', Ext.Msg.OK, null);
                    return;
                }
            }
        });

        if (form.isValid()) {
            form.submit({
                waitMsg: 'Saving Data...',
                success: function (response, request) {
					Ext.example.msg('Notification', 'User saved successfully!');
                    win.fireEvent('save', me);
					var parentNode = node.parentNode;
					if(parentNode == undefined && node.data.text == 'Scopes'){
						var nodeIndex = 0;//node.lastChild.data.index+1;
						node.insertChild(nodeIndex,Ext.JSON.decode(request.response.responseText).nodes[0]); 
					}else{
						var nodeIndex = parentNode.indexOf(node); 
						parentNode.removeChild(node); 
						parentNode.insertChild(nodeIndex, Ext.JSON.decode(request.response.responseText).nodes[0]); 
					}
					me.setLoading(false); 
					//win.fireEvent('save', me);
					//node.firstChild.expand();
                    //node.expandChildren();
					//Ext.ComponentQuery.query('directorytree')[0].onReload();
                },
                failure: function (response, request) {
					var resp = Ext.decode(request.response.responseText);
					var userMsg = resp['message'];
					var detailMsg = resp['stackTraceDescription'];
					var expPanel = Ext.widget('exceptionpanel', { title: 'Error Notification'});
                    Ext.ComponentQuery.query('#expValue',expPanel)[0].setValue(userMsg);
					Ext.ComponentQuery.query('#expValue2',expPanel)[0].setValue(detailMsg);
                  
                }
            });
        } else {
            Ext.widget('messagepanel', { title: 'Warning', msg: 'Please complete all required fields.'});
			//showDialog(400, 100, 'Warning', 'Please complete all required fields...', Ext.Msg.OK, null);
            return;
        }
    */},
  onReset: function() {
    var me = this;
    var win = me.up('window');
    win.fireEvent('Cancel', me);
  }
});