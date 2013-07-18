/*
 * File: Scripts/AM/view/directory/GraphMapForm.js
 *
 * This file was generated by Sencha Architect version 2.2.2.
 * http://www.sencha.com/products/architect/
 *
 * This file requires use of the Ext JS 4.1.x library, under independent license.
 * License of Sencha Architect does not include license for Ext JS 4.1.x. For more
 * details see http://www.sencha.com/license or contact license@sencha.com.
 *
 * This file will be auto-generated each and everytime you save your project.
 *
 * Do NOT hand edit this file.
 */

Ext.define('AM.view.directory.GraphMapForm', {
  extend: 'Ext.form.Panel',
  alias: 'widget.graphmapform',

  height: 300,
  width: 490,
  bodyBorder: false,
  bodyStyle: 'padding:10px 5px 0',
  method: 'POST',
  url: 'mapping/graphMap',

  initComponent: function() {
    var me = this;

    me.addEvents(
      'save',
      'reset'
    );

    me.initialConfig = Ext.apply({
      method: 'POST',
      url: 'mapping/graphMap'
    }, me.initialConfig);

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'fieldset',
          hidden: true,
          layout: {
            type: 'fit'
          },
          items: [
            {
              xtype: 'hiddenfield',
              name: 'scope'
            },
            {
              xtype: 'hiddenfield',
              name: 'app'
            },
            {
              xtype: 'hiddenfield',
              name: 'oldGraphName'
            },
            {
              xtype: 'hiddenfield',
              name: 'baseUrl'
            },
            {
              xtype: 'hiddenfield',
              name: 'mappingNode'
            },
            {
              xtype: 'hiddenfield',
              name: 'objectName'
            },
            {
              xtype: 'hiddenfield',
              name: 'className'
            },
            {
              xtype: 'hiddenfield',
              name: 'classId'
            },
            {
              xtype: 'hiddenfield',
              name: 'oldClassLabel'
            },
            {
              xtype: 'hiddenfield',
              name: 'oldClassUrl'
            },
            {
              xtype: 'hiddenfield',
              name: 'identifier'
            }
          ]
        },
        {
          xtype: 'label',
          text: 'Graph Name:'
        },
        {
          xtype: 'textfield',
          anchor: '100%',
          margin: '5 0 15 25',
          name: 'graphName',
          allowBlank: false
        },
        {
          xtype: 'label',
          margin: '10 0 0 0',
          text: 'Identifier Delimiter (required for composite identifier):'
        },
        {
          xtype: 'textfield',
          margin: '5 0 15 25',
          name: 'delimiter',
          value: '_'
        },
        {
          xtype: 'label',
          margin: '10 0 0 0',
          text: 'Identifier:'
        },
        {
          xtype: 'container',
          anchor: '100%',
          html: 'Drop a Property Node here.',
          itemId: 'gmfpcontainer',
          margin: '5 0 15 25',
          style: 'border:1px silver solid;margin:5px;padding:8px;height:40px',
          styleHtmlContent: true
        },
        {
          xtype: 'label',
          margin: '10 0 0 0',
          text: 'Root Class:'
        },
        {
          xtype: 'container',
          anchor: '100%',
          html: 'Drop a Class Node here.',
          itemId: 'gmfccontainer',
          margin: '5 0 15 25',
          style: 'border:1px silver solid;margin:5px;padding:8px;height:40px',
          styleHtmlContent: true
        }
      ],
      listeners: {
        afterrender: {
          fn: me.onFormAfterRender,
          scope: me
        }
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
              text: 'Ok'
            },
            {
              xtype: 'button',
              handler: function(button, event) {
                me.onIndentifierReset();
              },
              text: 'Reset'
            },
            {
              xtype: 'button',
              handler: function(button, event) {
                me.onReset();
              },
              text: 'Cancel'
            }
          ]
        }
      ]
    });

    me.callParent(arguments);
  },

  onFormAfterRender: function(component, eOpts) {
    var me = this;
    var ptarget = me.down('#gmfpcontainer');
    var ctarget = me.down('#gmfccontainer');

    var propertyDd = new Ext.dd.DropTarget(ptarget.getEl(), {
      ddGroup: 'propertyGroup',
      notifyEnter: function (propertyDd, e, data) {
        if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode')
        return this.dropNotAllowed;
        else
        return this.dropAllowed;
      },
      notifyOver: function (propertyDd, e, data) {
        if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode')
        return this.dropNotAllowed;
        else
        return this.dropAllowed;
      },  
      notifyDrop: function (propertyDd, e, data) {
        if (data.records[0].data.type != 'DataPropertyNode' && data.records[0].data.type != 'KeyDataPropertyNode') {
          return false;
        }
        else {
          var ident = getLastXString(data.records[0].data.id, 1);
          var object = getLastXString(data.records[0].data.id, 2);
          //var key1 = getLastXString(data.records[0].data.id, 2)
          //var key2 = getLastXString(data.records[0].data.id, 1);
          var key = object+'.'+ident;//key1+'.'+key2;
          if(me.getForm().findField('identifier').getValue()!='Drop property node(s) here.'){

            var existingIdentifier =  me.getForm().findField('identifier').getValue();
            if(existingIdentifier!=''){
              var tempObjName = existingIdentifier.split('.')[0];
              if (object != tempObjName) {
                var message = 'Properties must root from the same data object as graph!';
                showDialog(400, 100, 'Error', message, Ext.Msg.OK, null);
                return false;
              }

              if(existingIdentifier.indexOf(key)!= -1) {
                var message = 'Duplicate properties are not allowed!';
                showDialog(400, 100, 'Error', message, Ext.Msg.OK, null);
                return false;
              }
              key = existingIdentifier+','+ key;
            }else
            key = key;
            me.getForm().findField('identifier').setValue(key);
          }else{
            me.getForm().findField('identifier').setValue(key);
          }
          me.getForm().findField('objectName').setValue(object);

          //ptarget.update(msg);
          ptarget.update(key);
          return true;
        }
      }   
    });

    var classdd = new Ext.dd.DropTarget(ctarget.getEl(), {
      ddGroup: 'refdataGroup',
      notifyEnter: function (classdd, e, data) {
        if (data.records[0].data.type != 'ClassNode')
        return this.dropNotAllowed;
        else
        return this.dropAllowed;
      },
      notifyOver: function (classdd, e, data) {
        if (data.records[0].data.type != 'ClassNode')
        return this.dropNotAllowed;
        else
        return this.dropAllowed;
      },
      notifyDrop: function (classdd, e, data) {
        if (data.records[0].data.type != 'ClassNode') {
          var message = 'Please slect a RDL Class...';
          showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
          return false;
        } else {
          var tempClassLabel = me.getForm().findField('className').getValue();
          var tempClassUrl = me.getForm().findField('classId').getValue();
          if (tempClassLabel !== "") {
            me.getForm().findField('oldClassLabel').setValue(tempClassLabel);
            me.getForm().findField('oldClassUrl').setValue(tempClassUrl);
          }
          me.getForm().findField('className').setValue(data.records[0].data.record.Label);
          me.getForm().findField('classId').setValue(data.records[0].data.record.Uri);
          var msg = 'Class Label: ' + data.records[0].data.record.Label;
          //ctarget.update(msg);
          ctarget.update(data.records[0].data.record.Label);
          return true;
        }
      } 
    }); 
  },

  updateDDContainers: function(record) {
    var me = this;
    var pcon = me.down('#gmfpcontainer');
    var ccon = me.down('#gmfccontainer');
    var identifier = 'Drop property node(s) here.';
    var classlabel = 'Drop a class node here.';
    if (record != null) {
      identifier =  record.classTemplateMaps[0].classMap.identifiers[0];
      if(record.classTemplateMaps[0].classMap.identifiers.length>1){
        for(var i=1;i<record.classTemplateMaps[0].classMap.identifiers.length;i++){
          identifier = identifier+','+record.classTemplateMaps[0].classMap.identifiers[i];
        }
      }
      //identifier = getLastXString(record.record.classTemplateMaps[0].classMap.identifiers[0], 1).split('.')[1];
      //identifier = record.classTemplateMaps[0].classMap.identifiers[0];
      classlabel = record.classTemplateMaps[0].classMap.name;
    }
    //if(record!=null)
    //identifier = 'Identifier: ' + identifier;

    pcon.update(identifier);

    //if(record!=null)
    //var classlabel = 'Class Label: ' + label;
    // else
    // var classlabel = 'Drop a class node here.';

    ccon.update(classlabel);
    //ccon.update(classlabel);
  },

  onReset: function() {
    var me = this;
    var win = me.up('window');
    win.fireEvent('reset', me);

  },

  onSave: function() {
    var me = this;
    var win = me.up('window');
    if (me.getForm().findField('objectName').getValue() === '' ||
    me.getForm().findField('graphName').getValue() === '' ||
    me.getForm().findField('className').getValue() === '') {
      showDialog(400, 50, 'Warning', 'Please fill in every field in this form.', Ext.Msg.OK, null);
      return;
    }
    me.getForm().submit({
      waitMsg: 'Saving Data...',
      success: function (f, a) {

        win.fireEvent('save', me);
      },
      failure: function (f, a) {
        var message = 'Error saving changes!';
        showDialog(400, 50, 'Warning', message, Ext.Msg.OK, null);
      }
    });
  },

  onIndentifierReset: function() {
    var me = this;
    var win = me.up('window');
    me.getForm().findField('objectName').setValue('');
    me.getForm().findField('identifier').setValue('Drop property node(s) here.');
    me.down('#gmfpcontainer').update('Drop property node(s) here.');
  }

});