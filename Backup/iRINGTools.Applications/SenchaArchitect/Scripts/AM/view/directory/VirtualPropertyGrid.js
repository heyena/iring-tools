/*
 * File: Scripts/AM/view/directory/VirtualPropertyGrid.js
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

Ext.define('AM.view.directory.VirtualPropertyGrid', {
  extend: 'Ext.grid.Panel',
  alias: 'widget.virtualpropertygrid',

  itemId: 'virtualGrid',
  bodyBorder: false,
  frameHeader: false,
  header: false,
  title: 'My Grid Panel',
  store: 'VirtualPropertyStore',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      columns: [
        {
          xtype: 'rownumberer',
          hidden: true,
          width: 40,
          align: 'left',
          text: 'Sr. No'
        },
        {
          xtype: 'gridcolumn',
          itemId: 'typeCmb',
          dataIndex: 'propertyType',
          text: 'Type',
          flex: 1,
          editor: {
            xtype: 'combobox',
            store: [
              [
                'Constant',
                'Constant'
              ],
              [
                'Property',
                'Property'
              ]
            ],
            listeners: {
              select: {
                fn: me.onComboboxSelect,
                scope: me
              }
            }
          }
        },
        {
          xtype: 'gridcolumn',
          itemId: 'propertyNameCmb',
          dataIndex: 'propertyName',
          text: 'Property',
          flex: 1,
          editor: {
            xtype: 'combobox'
          }
        },
        {
          xtype: 'gridcolumn',
          itemId: 'lengthField',
          dataIndex: 'propertyLength',
          text: 'Length',
          flex: 1,
          editor: {
            xtype: 'numberfield'
          }
        },
        {
          xtype: 'gridcolumn',
          itemId: 'textField',
          dataIndex: 'valueText',
          text: 'Text',
          flex: 1,
          editor: {
            xtype: 'textfield'
          }
        }
      ],
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'top',
          items: [
            {
              xtype: 'button',
              handler: function(button, event) {
                me.onAddRecord();
              },
              iconCls: 'am-list-add',
              text: 'Add'
            },
            {
              xtype: 'button',
              handler: function(button, event) {
                me.onRemoveRecord();
              },
              disabled: true,
              itemId: 'removeButton',
              iconCls: 'am-list-remove',
              text: 'Remove'
            }
          ]
        }
      ],
      plugins: [
        Ext.create('Ext.grid.plugin.RowEditing', {
          autoCancel: false,
          clicksToMoveEditor: 1
        })
      ],
      listeners: {
        celldblclick: {
          fn: me.onVirtualGridCellDblClick,
          scope: me
        },
        cellclick: {
          fn: me.onVirtualGridCellClick,
          scope: me
        }
      }
    });

    me.callParent(arguments);
  },

  onComboboxSelect: function(combo, records, eOpts) {
    var me = this;
    /*
    var propertyColumn = me.down('#propertyNameCmb');
    var propertyCmb = propertyColumn.getEditor();
    var lengthField = me.down('#lengthField');
    var lengthFieldEditor = lengthField.getEditor();
    var textField = me.down('#textField');
    var textFieldEditor = textField.getEditor();

    if(records[0].data.field1 == 'Constant'){
    lengthFieldEditor.setDisabled(true);
    propertyCmb.setDisabled(true);
    textFieldEditor.setDisabled(false);
    }else{
    lengthFieldEditor.setDisabled(false);
    propertyCmb.setDisabled(false);
    textFieldEditor.setDisabled(true);
    }
    */

    me.enableDisableColumn(records[0].data.field1);
  },

  onVirtualGridCellDblClick: function(tableview, td, cellIndex, record, tr, rowIndex, e, eOpts) {
    var me = this;
    /*var propertyColumn = me.down('#propertyNameCmb');
    var propertyCmb = propertyColumn.getEditor();
    var lengthField = me.down('#lengthField');
    var lengthFieldEditor = lengthField.getEditor();
    var textField = me.down('#textField');
    var textFieldEditor = textField.getEditor();


    if(record.data.propertyType == 'Constant'){
    lengthFieldEditor.setDisabled(true);
    propertyCmb.setDisabled(true);
    textFieldEditor.setDisabled(false);
    }else{
    lengthFieldEditor.setDisabled(false);
    propertyCmb.setDisabled(false);
    textFieldEditor.setDisabled(true);
    }
    */
    me.enableDisableColumn(record.data.propertyType);
  },

  onVirtualGridCellClick: function(tableview, td, cellIndex, record, tr, rowIndex, e, eOpts) {
    var me = this;
    /*var propertyColumn = me.down('#propertyNameCmb');
    var propertyCmb = propertyColumn.getEditor();
    var lengthField = me.down('#lengthField');
    var lengthFieldEditor = lengthField.getEditor();
    var textField = me.down('#textField');
    var textFieldEditor = textField.getEditor();


    if(record.data.propertyType == 'Constant'){
    lengthFieldEditor.setDisabled(true);
    propertyCmb.setDisabled(true);
    textFieldEditor.setDisabled(false);
    }else{
    lengthFieldEditor.setDisabled(false);
    propertyCmb.setDisabled(false);
    textFieldEditor.setDisabled(true);
    }*/

    var removeButton = me.down('#removeButton');
    removeButton.setDisabled(false);
    me.enableDisableColumn(record.data.propertyType);
  },

  onAddRecord: function() {
    var me = this; //me here is grid refrence...
    var rowEditing = me.editingPlugin;
    rowEditing.cancelEdit();
    var store = me.getStore();
    var sm = me.getSelectionModel();
    // Create a model instance
    var newRec = Ext.create('AM.model.VirtualPropertyModel', {
    });

    store.insert(0, newRec);
    rowEditing.startEdit(0, 0);
    me.enableDisableColumn('');
  },

  onRemoveRecord: function() {

    var me = this; //me here is grid refrence...
    var store = me.getStore();
    var sm = me.getSelectionModel();
    var rowEditing = me.editingPlugin;
    rowEditing.cancelEdit();

    if (store.getCount() > 0){ 
      store.remove(sm.getSelection());
      sm.select(0);
    }
    else{
      showDialog(300, 70, 'Warning', "There are no more records to Remove", Ext.Msg.OK, null);
    }




  },

  enableDisableColumn: function(propertyType) {
    //alert('enableDisabledColumn mehtod...');
    var me = this;
    var propertyColumn = me.down('#propertyNameCmb');
    var propertyCmb = propertyColumn.getEditor();
    var lengthField = me.down('#lengthField');
    var lengthFieldEditor = lengthField.getEditor();
    var textField = me.down('#textField');
    var textFieldEditor = textField.getEditor();

    if(propertyType == 'Constant'){

      lengthFieldEditor.setDisabled(true);
      //lengthFieldEditor.setValue(0)
      propertyCmb.setDisabled(true);
      //propertyCmb.setValue(null)
      textFieldEditor.setDisabled(false);

    }else if(propertyType == 'Property'){

      lengthFieldEditor.setDisabled(false);
      propertyCmb.setDisabled(false);
      textFieldEditor.setDisabled(true);
      //textFieldEditor.setValue(null);

    }else{

      lengthFieldEditor.setDisabled(false);
      propertyCmb.setDisabled(false);
      textFieldEditor.setDisabled(false);

    }

  }

});