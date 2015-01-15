
Ext.define('USM.view.users.UserForm', {
    extend: 'Ext.form.Panel',
    alias: 'widget.userform',
    frame: false,
    width: 400,
    bodyPadding: 10,

    initComponent: function() {
        var me = this;

        Ext.applyIf(me, {
            items: [
                {
                    xtype: 'hiddenfield',
                    name: 'ActionType',
                    value: 'ADD'
                },
                {
                    xtype: 'hiddenfield',
                    name: 'UserId'
                },
                {
                    xtype: 'hiddenfield',
                    name: 'SiteId'
                },
                {
                    xtype: 'textfield',
                    fieldLabel: 'User Name',
                    name: 'UserName',
                    anchor: '100%',
                    maxLength: 100,
                    allowBlank: false,
                    vtype: 'alphanum'
                },
                {
                    xtype: 'textfield',
                    fieldLabel: 'First Name',
                    anchor: '100%',
                    name: 'UserFirstName',
                    maxLength: 50
                },
                {
                    xtype: 'textfield',
                    fieldLabel: 'Last Name',
                    anchor: '100%',
                    name: 'UserLastName',
                    maxLength: 50
                },
                 {
                     xtype: 'textfield',
                     fieldLabel: 'Email',
                     name: 'UserEmail',
                     anchor: '100%',
                     maxLength: 50,
                     vtype: 'email'
                 },
                {
                    xtype: 'textfield',
                    fieldLabel: 'Phone',
                    anchor: '100%',
                    name: 'UserPhone',
                    regex: /^[0-9-]+$/,
                    regexText: 'Please enter valid number.'
                },
                {
                    xtype: 'textfield',
                    fieldLabel: 'Description',
                    anchor: '100%',
                    name: 'UserDesc',
                    maxLength: 255
                }
            ]
        });

        me.callParent(arguments);
    }

});