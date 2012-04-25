Ext.define('AM.view.nhibernate.CreateRelations', {
    extend: 'Ext.form.Panel',
    alias: 'widget.createrelations',   
    border: false,
    contextName: null,
    node: null,
    endpoint: null,
    autoScroll: false,
    monitorValid: true,
    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
    defaults: {
      anchor: '100%',
      labelWidth: 130,
      allowBlank: false 
    },

    initComponent: function () {
      this.items = [{
        xtype: 'label',
        text: 'Add/Remove relationship',
        cls: 'x-form-item',
        style: 'font-weight:bold;'
      }, {
        xtype: 'textfield',
        name: 'relationName',
        fieldLabel: 'Relationship Name',
        allowBlank: false
      }, {
        xtype: 'panel',
        id: this.contextName + '.' + this.endpoint + '.dataRelationPane.' + this.node.id,
        name: 'relationGridPanel',
        bodyStyle: 'background:#eee',
        anchor: '100% -50',
        height: 300,
        layout: 'fit',
        border: false,
        items: [],
        frame: false
      }];

      this.keys = [{
        key: [Ext.EventObject.ENTER], handler: function () {
          addRelationship(relationCreateFormPanel, node, scopeName, appName);
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
          handler: function () {
            var deleteDataRelationPane = relationCreateFormPanel.items.items[2];
            var gridLabel = scopeName + '.' + appName + '.' + node.id;
            var gridPane = deleteDataRelationPane.items.map[gridLabel];
            if (gridPane) {
              var mydata = gridPane.store.data.items;

                for (var j = 0; j < node.childNodes.length; j++) {
                  exitNode = false;
                  for (var i = 0; i < mydata.length; i++) {
                    newNodeText = mydata[i].data.relationName;
                    if (node.childNodes[j].text.toLowerCase() == newNodeText.toLowerCase()) {
                      exitNode = true;
                      break;
                    }
                  }
                  if (exitNode == false) {
                    var deleteNode = node.childNodes[j];
                    node.childNodes.splice(j, 1);
                    j--;
                    node.removeChild(deleteNode);
                  }
                }
              }
            }
        }, {
          xtype: 'tbspacer',
          width: 4
        }, {
          xtype: 'button',
          icon: 'Content/img/16x16/edit-clear.png',
          text: 'Reset',
          tooltip: 'Reset to the latest applied changes',
          handler: function () {
            var relations = new Array();
            relationCreateFormPanel.getForm().reset();
            for (i = 0; i < node.childNodes.length; i++) {
              if (node.childNodes[i].text != '')
                relations.push([node.childNodes[i].text]);
            }
            var colModel = new Ext.grid.ColumnModel([
              { id: "relationName", header: "Data Relationship Name", dataIndex: 'relationName' }
            ]);
            var dataStore = new Ext.data.Store({
              autoDestroy: true,
              proxy: new Ext.data.MemoryProxy(relations),
              reader: new Ext.data.ArrayReader({}, [
                { name: 'relationName' }
              ])
            });
            createRelationGrid(scopeName + '.' + appName + '.' + node.id, deleteDataRelationPane, colModel, dataStore, scopeName + '.' + appName + '.-nh-config', scopeName + '.' + appName + '.dataObjectsPane', scopeName + '.' + appName + '.relationCreateForm.' + node.id, 0, scopeName, appName, '');
          }
        }]
      });
      
      this.callParent(arguments);
    }
});


function addRelationship(relationCreateFormPanel, node, scopeName, appName) {
	var deleteDataRelationPane = relationCreateFormPanel.items.items[2];
	var relationName = relationCreateFormPanel.getForm().findField("relationName").getValue().replace(/^\s*/, "").replace(/\s*$/, "");
	if (relationName == "") {
		var message = 'Relationship name cannot be blank.';
		showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
		return;
	}

	var gridLabel = scopeName + '.' + appName + '.' + node.id;
	if (deleteDataRelationPane.items) {
		var gridPane = deleteDataRelationPane.items.map[gridLabel];
		var myArray = new Array();
		var i = 0;
		if (gridPane) {
			var dataStore = gridPane.store;
			var mydata = dataStore.data.items;

			for (var i = 0; i < mydata.length; i++)
				if (mydata[i].data.relationName.toLowerCase() == relationName.toLowerCase()) {
					var message = relationName + 'already exits.';
					showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
					return;
				}

			var relationRecord = Ext.data.Record.create([
        { name: "relationName" }
      ]);

			var newRelationRecord = new relationRecord({
				relationName: relationName
			});

			dataStore.add(newRelationRecord);
			dataStore.commitChanges();
		}
	}
}		

function findNodeRelatedObjMap(node, relatedObjName) {
	if (node.attributes.attributes)
		var attribute = node.attributes.attributes;
	else
		var attribute = node.attributes;

	if (attribute)
		var relatedObjMap = attribute.relatedObjMap;
	var relateObjItem;
	var ifHas = false;

	if (relatedObjMap)
		for (var i = 0; i < relatedObjMap.length; i++) {
			if (relatedObjMap[i].relatedObjName)
				if (relatedObjMap[i].relatedObjName == relatedObjName) {
					ifHas = true;
					relateObjItem = relatedObjMap[i];
				}
		}

	if (ifHas == false) {
		relateObjItem = {};
		relateObjItem.relatedObjName = relatedObjName;
		relateObjItem.propertyMap = new Array();
		relatedObjMap.push(relateObjItem);
	}

	return relateObjItem.propertyMap;
}



