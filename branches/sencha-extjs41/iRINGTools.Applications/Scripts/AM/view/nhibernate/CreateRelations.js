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
      allowBlank: false,
      listeners: { 'keydown': function (field, e) {
        if (e.getKey() == e.ENTER) {
          addRelationship(me, node, contextName, endpoint);
        }
      }
      }
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
          var relationNameField = me.getForm().findField('relationName');
          relationNameField.setValue(null);
          relationNameField.clearInvalid();

          for (i = 0; i < node.childNodes.length; i++) {
            if (node.childNodes[i].data.text != '')
              relations.push([node.childNodes[i].data.text]);
          }

          var dataGridPanel = me.items.items[2];
          var gridPane = dataGridPanel.items.items[0];
          var store = gridPane.store;

          if (store.data) {
            gridPane.store.removeAll();
          }

          gridPane.store.loadData(relations);
          dataGridPanel.doLayout();
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

	if (deleteDataRelationPane.items) {
		var gridPane = deleteDataRelationPane.items.items[0];
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

		  var newRelationRecord = new AM.model.RelationNameModel({
		    relationName: relationName
		  });

		  gridPane.store.add(newRelationRecord);
		  deleteDataRelationPane.doLayout();
		}
	}
};



Ext.override(Ext.form.Field, {
  fireKey: function (e) {
    if (((Ext.isIE && e.type == 'keydown') || e.type == 'keypress') && e.isSpecialKey()) {
      this.fireEvent('specialkey', this, e);
    }
    else {
      this.fireEvent(e.type, this, e);
    }
  }
          , initEvents: function () {
            //                this.el.on(Ext.isIE ? "keydown" : "keypress", this.fireKey,  this);
            this.el.on("focus", this.onFocus, this);
            this.el.on("blur", this.onBlur, this);
            this.el.on("keydown", this.fireKey, this);
            this.el.on("keypress", this.fireKey, this);
            this.el.on("keyup", this.fireKey, this);

            // reference to original value for reset
            this.originalValue = this.getValue();
          }
});

