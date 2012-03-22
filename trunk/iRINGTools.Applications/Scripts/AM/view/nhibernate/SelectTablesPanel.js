Ext.define('AM.view.nhibernate.SelectTablesPanel', {
  extend: 'Ext.form.Panel',
  alias: 'widget.selecttables',
  frame: false,
  border: false,
  contextName: null,
  endpoint: null,
  selected: {},
  available: {},
  autoScroll: true,
  bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
  labelWidth: 140,
  monitorValid: true,

  initComponent: function () {
    var me = this;
    this.items = [{
      xtype: 'label',
      fieldLabel: 'Select Tables',
      labelSeparator: '',
      itemCls: 'form-title'
    }, {
      xtype: 'itemselector',
      hideLabel: true,
      bodyStyle: 'background:#eee',
      frame: true,
      name: 'tableSelector',
      imagePath: 'scripts/ext-3.3.1/examples/ux/images/',
      multiselects: [{
        width: 240,
        height: 370,
        blankText: 'No Tables Available',
        store: '',
      //  displayField: 'tableName',
        valueField: 'tableValue',
        border: 0
      }, {
        width: 240,
        height: 370,
        blankText: 'No Tables Selected',
        store:'',
       // displayField: 'tableName',
        valueField: 'tableValue',
        border: 0
      }],
      listeners: {
        change: function (itemSelector, selectedValuesStr) {
          var selectTables = itemSelector.toMultiselect.store.data.items;
          for (var i = 0; i < selectTables.length; i++) {
            var selectTableName = selectTables[i].data.text;
            if (selectTableName == '')
              itemSelector.toMultiselect.store.removeAt(i);
          }

          var availTables = itemSelector.fromMultiselect.store.data.items;
          for (var i = 0; i < availTables.length; i++) {
            var availTableName = availTables[i].data.text
            if (availTables[i].data.text == '')
              itemSelector.fromMultiselect.store.removeAt(i);
          }
        }
      }
    }];

    this.tbar = new Ext.Toolbar({
      items: [{
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/apply.png',
        text: 'Apply',
        tooltip: 'Apply the current changes to the data objects tree',
        action: 'applydatatables'
        
      }, {
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/edit-clear.png',
        text: 'Reset',
        tooltip: 'Reset to the latest applied changes',
        action: 'resettables'
        
      }]
    });

    
    this.callParent(arguments);
  }
});		

	

//function setTableNames () {
//    // populate selected tables		
//    var dbDict = AM.view.nhibernate.dbDict.value;	
//	var selectTableNames = new Array();

//	for (var i = 0; i < dbDict.dataObjects.length; i++) {
//		var tableName = (dbDict.dataObjects[i].tableName ? dbDict.dataObjects[i].tableName : dbDict.dataObjects[i]);
//		selectTableNames.push(tableName);
//	}

//	return selectTableNames;
//};



//function setSelectTables (dbObjectsTree) {
//	var selectTableNames = new Array();

//	if (!dbObjectsTree.disabled) {
//		var rootNode = dbObjectsTree.getRootNode();
//		for (var i = 0; i < rootNode.childNodes.length; i++) {
//			var nodeText = rootNode.childNodes[i].data.property.tableName;
//			selectTableNames.push([nodeText, nodeText]);
//		}
//	}

//	return selectTableNames;
//};