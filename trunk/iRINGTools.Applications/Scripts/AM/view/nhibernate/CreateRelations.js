Ext.define('AM.view.nhibernate.CreateRelations', {
  extend: 'Ext.form.Panel',
  alias: 'widget.createrelations',
  border: false,
  contextName: null,
  node: null,
  endpoint: null,
  autoScroll: false,
  monitorValid: true,
  editor: null,
  rootNode: null,
  bodyStyle: 'background:#eee;padding:10 0 0 10',
  defaults: {
    labelWidth: 130,
    allowBlank: false
  },

  initComponent: function () {
    var me = this;
    var node = me.node;
    var contextName = me.contextName;
    var endpoint = me.endpoint;
    var rootNode = me.rootNode;

    me.items = [{
      xtype: 'label',
      text: 'Add/Remove relationship',
      cls: 'x-form-item',
      style: 'font-weight:bold;'
    }, {
      xtype: 'textfield',
      name: 'relationName',
      fieldLabel: 'Relationship Name',
      anchor: '100%',
      allowBlank: false
    }, {
      xtype: 'panel',
      id: contextName + '.' + endpoint + '.dataRelationPane.' + node.id,
      name: 'relationGridPanel',
      bodyStyle: 'background:#eee',
      anchor: '100% -10',
      border: false,
      items: [],
      frame: false
    }];

    me.keys = [{
      key: [Ext.EventObject.ENTER], handler: function () {
        addRelationship(me, node, contextName, endpoint);
      }
    }];

    me.tbar = new Ext.Toolbar({
      items: [{
        xtype: 'tbspacer',
        width: 4
      }, {
        xtype: 'button',
        icon: 'Content/img/16x16/apply.png',
        text: 'Apply',
        tooltip: 'Apply the current changes to the data objects tree',
        handler: function () {
          var deleteDataRelationPane = me.items.items[2];
          var gridLabel = contextName + '.' + endpoint + '.relationsGrid' + node.id;
          var gridPane = deleteDataRelationPane.items.map[gridLabel];
          if (gridPane) {
            var mydata = gridPane.store.data.items;

            for (var j = 0; j < node.childNodes.length; j++) {
              exitNode = false;
              for (var i = 0; i < mydata.length; i++) {
                newNodeText = mydata[i].data.relationName;
                if (node.childNodes[j].data.text.toLowerCase() == newNodeText.toLowerCase()) {
                  exitNode = true;
                  break;
                }
              }
              if (exitNode == false) {
                node.removeChild(node.childNodes[j], true);
                j--;
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
          me.getForm().reset();
          for (i = 0; i < node.childNodes.length; i++) {
            if (node.childNodes[i].text != '')
              relations.push([node.childNodes[i].text]);
          }
          var gridLabel = contextName + '.' + endpoint + '.relationsGrid' + node.id;
          var deleteDataRelationPane = me.items.items[2];
          createRelationGrid(me.editor, me, rootNode, node, gridLabel, deleteDataRelationPane, relations, contextName + '.' + endpoint + '.-nh-config', contextName + '.' + endpoint + '.dataObjectsPane', contextName + '.' + endpoint + '.relationCreateForm.' + node.id, 0, contextName, endpoint, '');
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
		}
	}
};		





