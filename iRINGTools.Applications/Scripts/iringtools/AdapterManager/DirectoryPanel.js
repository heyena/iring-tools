Ext.ns('AdapterManager');
/**
* @class AdapterManager.directoryPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.DirectoryPanel = Ext.extend(Ext.Panel, {
  title: 'Directory',
  width: 220,

  collapseMode: 'mini',
  collapsible: true,
  collapsed: false,

  layout: 'border',
  border: false,
  split: true,

  navigationUrl: null,
  directoryPanel: null,
  scopesMenu: null,
  scopeMenu: null,
  valueListsMenu: null,
  valueListMenu: null,
  graphsMenu: null,
  graphMenu: null,
  applicationMenu: null,
  rootNode: null,
  treeLoader: null,
  propertyPanel: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

    this.addEvents({
      NewScope: true,
      NewApplication: true,
      EditScope: true,
      EditApplication: true,
      OpenMapping: true,
      DeleteScope: true,
      DeleteApplication: true,
      ReloadScopes: true,
      ReloadNode: true
    });

    this.tbar = this.buildToolbar();

    this.scopesMenu = new Ext.menu.Menu();
    this.scopesMenu.add(this.buildScopesMenu());

    this.scopeMenu = new Ext.menu.Menu();
    this.scopeMenu.add(this.buildScopeMenu());

    this.applicationMenu = new Ext.menu.Menu();
    this.applicationMenu.add(this.buildApplicationMenu());

    this.valueListsMenu = new Ext.menu.Menu();
    this.valueListsMenu.add(this.buildvalueListsMenu());

    this.valueListMenu = new Ext.menu.Menu();
    this.valueListMenu.add(this.buildvalueListMenu());

    this.graphsMenu = new Ext.menu.Menu();
    this.graphsMenu.add(this.buildGraphsMenu());

    this.graphMenu = new Ext.menu.Menu();
    this.graphMenu.add(this.buildGraphMenu());

    this.treeLoader = new Ext.tree.TreeLoader({
      baseParams: { type: null },
      url: this.navigationUrl
    });

    this.treeLoader.on("beforeload", function (treeLoader, node) {
      treeLoader.baseParams.type = node.attributes.type;

    }, this);

    this.rootNode = new Ext.tree.AsyncTreeNode({
      id: 'root',
      text: 'Scopes',
      expanded: true,
      draggable: true,
      icon: 'Content/img/internet-web-browser.png',
      type: 'ScopesNode'
    });

    this.propertyPanel = new Ext.grid.PropertyGrid({
      title: 'Details',
      region: 'south',
      // layout: 'fit',
      stripeRows: true,
      collapsible: true,
      autoScroll: true,
      border: false,
      frame: false,
      height: 150,
      selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
      // bodyStyle: 'padding-bottom:15px;background:#eee;',
      source: {},
      listeners: {
        beforeedit: function (e) {
          e.cancel = true;
        },
        // to copy but not edit content of property grid				
        afteredit: function (e) {
          e.grid.getSelectionModel().selections.items[0].data.value = e.originalValue;
          e.record.data.value = e.originalValue;
          e.value = e.originalValue;
          e.grid.getView().refresh();
        }
      }
    });

    this.directoryPanel = new Ext.tree.TreePanel({
      enableDrag: true,
      ddGroup: 'propertyGroup',
      region: 'center',
      collapseMode: 'mini',
      height: 300,
      //layout: 'fit',
      border: false,
      split: true,
      expandAll: true,
      rootVisible: true,
      lines: true,
      autoScroll: true,
      //singleExpand: true,     
      loader: this.treeLoader,
      root: this.rootNode

    });

    this.directoryPanel.on('contextmenu', this.showContextMenu, this);
    this.directoryPanel.on('click', this.onClick, this);
    this.directoryPanel.on('dblclick', this.onDoubleClick, this);
    this.directoryPanel.on('addgraphmap', this.onAddGraphmap, this);



    this.items = [
			this.directoryPanel,
			this.propertyPanel
		];

    var state = Ext.state.Manager.get("AdapterManager");

    if (state) {
      if (this.directoryPanel.expandPath(state) == false) {
        Ext.state.Manager.clear("AdapterManager");
        this.directoryPanel.root.reload();
      }
    }

    // super
    AdapterManager.DirectoryPanel.superclass.initComponent.call(this);
  },

  getSelectedNode: function () {
    var selectedNode = this.directoryPanel.getSelectionModel().getSelectedNode();
    return selectedNode;
  },

  buildToolbar: function () {
    return [
			{
			  xtype: 'button',
			  text: 'Reload Tree',
			  handler: this.onReload,
			  icon: 'Content/img/16x16/view-refresh.png',
			  scope: this
			}
		]
  },

  buildScopesMenu: function () {
    return [
			{
			  text: 'Add Scope',
			  handler: this.onNewScope,
			  icon: 'Content/img/16x16/document-new.png',
			  scope: this
			}
		]
  },

  buildScopeMenu: function () {
    return [
			{
			  text: 'Edit Scope',
			  handler: this.onEditScope,
			  icon: 'Content/img/16x16/document-properties.png',
			  scope: this
			},
			{
			  text: 'Delete Scope',
			  handler: this.onDeleteScope,
			  icon: 'Content/img/16x16/edit-delete.png',
			  scope: this
			},
			{
			  xtype: 'menuseparator'
			},
			{
			  text: 'Add Application'
			}
		]
  },

  buildApplicationMenu: function () {
    return [
			{
			  text: 'Edit Application',
			  handler: this.onEditApplication,
			  icon: 'Content/img/16x16/document-properties.png',
			  scope: this
			},
			{
			  text: 'Delete Application',
			  handler: this.onDeleteApplication,
			  icon: 'Content/img/16x16/edit-delete.png',
			  scope: this
			},
			{
			  xtype: 'menuseparator'
			}//,
    //			{
    //			  text: 'Open Mapping',
    //			  handler: this.onOpenMapping,
    //			  icon: 'Content/img/16x16/mapping.png',
    //			  scope: this
    //			}
		]
  },

  buildvalueListsMenu: function () {
    return [
    {
      text: 'Add Value List',
      handler: this.onNewValueList,
      icon: 'Content/img/list-add.png',
      scope: this
    }
    ]
  },

  buildvalueListMenu: function () {
    return [
    {
      text: 'Edit Value List',
      handler: this.onEditValueList,
      icon: 'Content/img/16x16/document-properties.png',
      scope: this
    },
    {
      text: 'Delete ValueList',
      handler: this.onDeleteValueList,
      icon: 'Content/img/16x16/edit-delete.png',
      scope: this
    }
    ]
  },

  buildGraphsMenu: function () {
    return [
    {
      text: 'Add GraphMap',
      handler: this.onAddGraphMap,
      icon: 'Content/img/list-add.png',
      scope: this
    }
    ]
  },

  buildGraphMenu: function () {
    return [
    {
      text: 'Edit GraphMap',
      handler: this.onEditGraphMap,
      icon: 'Content/img/16x16/document-properties.png',
      scope: this
    }]
  },



  showContextMenu: function (node, event) {

    //  if (node.isSelected()) { 
    var x = event.browserEvent.clientX;
    var y = event.browserEvent.clientY;

    var obj = node.attributes;

    if (obj.type == "ScopesNode") {
      this.scopesMenu.showAt([x, y]);
    } else if (obj.type == "ScopeNode") {
      this.scopeMenu.showAt([x, y]);
    } else if (obj.type == "ApplicationNode") {
      this.applicationMenu.showAt([x, y]);
    } else if (obj.type == "ValueListsNode") {
      this.valueListsMenu.showAt([x, y]);
    } else if (obj.type == "ValueListNode") {
      this.valueListMenu.showAt([x, y]);
    } else if (obj.type == "GraphsNode") {
      this.graphsMenu.showAt([x, y]);
    } else if (obj.type == "GraphNode") {
      this.graphMenu.showAt([x, y]);
    }
    this.directoryPanel.getSelectionModel().select(node);
    this.onClick(node);
    //}
  },

  onNewScope: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('NewScope', this, node);
  },

  onAddGraphMap: function (btn, e) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('addgraphmap', this, node);
  },

  onAddGraphMap: function (node) {
    var dirnode = this.directoryPanel.getSelectionModel().getSelectedNode();
    var formid = 'graphtarget-' + dirnode.parentNode.parentNode.text + '-' + dirnode.parentNode.text;
    var form = new Ext.form.FormPanel({
      id: formid,
      layout: 'form',
      method: 'POST',
      border: false,
      frame: false,
      bbar: [
        { text: 'Submit', scope: this, handler: this.onSubmitGraphMap },
        { text: 'Close', scope: this, handler: this.onClose }
        ],
      url: 'mapping/addgraphmap',
      items: [{ xtype: 'textfield', name: 'graphName', id: 'graphName', fieldLabel: 'Graph Name', width: 120, required: true },
              { xtype: 'hidden', name: 'objectName', id: 'objectName' },
              { xtype: 'hidden', name: 'classLabel', id: 'classLabel' },
              { xtype: 'hidden', name: 'classUrl', id: 'classUrl' },
              { xtype: 'hidden', name: 'mappingNode', id: 'mappingNode', value: this.rootNode }
             ],
      html: '<div class="property-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Key Property Node here.</div>'
          + '<div class="class-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Class Node here. </div>',

      afterRender: function (cmp) {
        Ext.FormPanel.prototype.afterRender.apply(this, arguments);

        var propertyTarget = this.body.child('div.property-target' + formid);
        var propertydd = new Ext.dd.DropTarget(propertyTarget, {
          ddGroup: 'propertyGroup',
          notifyEnter: function (dd, e, data) {
            if (data.node.attributes.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyOver: function (dd, e, data) {
            if (data.node.attributes.type != 'KeyDataPropertyNode')
              return this.dropNotAllowed;
            else
              return this.dropAllowed;
          },
          notifyDrop: function (dd, e, data) {
            if (data.node.attributes.type != 'KeyDataPropertyNode') {
              return false;
            }
            else {
              Ext.get('objectName').dom.value = data.node.id;
              var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + data.node.id.split('/')[5] + '</b></td></tr>'
              msg += '</table>'
              Ext.getCmp(formid).body.child('div.property-target' + formid).update(msg)
              return true;
            }
          } //eo notifyDrop
        }); //eo propertydd
        var classTarget = this.body.child('div.class-target' + formid);
        var classdd = new Ext.dd.DropTarget(classTarget, {
          ddGroup: 'refdataGroup',
          notifyDrop: function (classdd, e, data) {
            if (data.node.attributes.type != 'ClassNode') {
              Ext.Msg.show({
                title: 'Invalid Selection',
                msg: 'Please slect a RDL Class...',
                icon: Ext.MessageBox.ERROR,
                buttons: Ext.Msg.CANCEL
              });
              return false;
            }
            Ext.get('classLabel').dom.value = data.node.attributes.record.Label;
            Ext.get('classUrl').dom.value = data.node.attributes.record.Uri;
            var msg = '<table style="font-size:13px"><tr><td>Class Label:</td><td><b>' + data.node.attributes.record.Label + '</b></td></tr>'
            msg += '</table>'
            Ext.getCmp(formid).body.child('div.class-target' + formid).update(msg)
            return true;

          } //eo notifyDrop
        }); //eo propertydd
      }
    });

    var win = new Ext.Window({
      closable: true,
      modal: false,
      layout: 'form',
      title: 'Add new GraphMap to Mapping',
      items: form,
      height: 180,
      width: 430,
      plain: true,
      scope: this
    });

    win.show();
  },

  onClose: function (btn, e) {
    if (btn != undefined) {
      var win = btn.findParentByType('window');
      if (win != undefined)
        win.close();
    }
  },

  onSubmitGraphMap: function (btn, e) {
    var form = btn.findParentByType('form');
    var win = btn.findParentByType('window');
    var objectname = Ext.get('objectName').dom.value;
    var classlabel = Ext.get('classLabel').dom.value;
    var classuri = Ext.get('classUrl').dom.value;
    var graphname = Ext.get('graphName').dom.value;
    var that = this;
    if (form.getForm().isValid())
      Ext.Ajax.request({
        url: 'mapping/addgraphmap',
        method: 'POST',
        params: {
          objectName: objectname,
          classLabel: classlabel,
          classUrl: classuri,
          graphName: graphname
        },
        success: function (result, request) {
          that.onReload();
          win.close();
          Ext.Msg.show({ title: 'Success', msg: 'Added GraphMap to mapping', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
        },
        failure: function (result, request) {
          Ext.Msg.show({ title: 'Failure', msg: 'Failed to Add GraphMap to mapping', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
        }
      })
  },

  onEditGraphMap: function (btn, e) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('opengraphmap', this, node);
  },

  onNewApplication: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('NewApplication', this, node);
  },

  onEditScope: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('EditScope', this, node);
  },

  onEditApplication: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('EditApplication', this, node);
  },

  onDeleteScope: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('DeleteScope', this, node);
  },

  onDeleteApplication: function (btn, ev) {
    var node = this.directoryPanel.getSelectionModel().getSelectedNode();
    this.fireEvent('DeleteApplication', this, node);
  },

  onReload: function (node) {
    //Ext.state.Manager.clear('AdapterManager');
    this.directoryPanel.root.reload();
  },

  onReloadNode: function (node) {
    //Ext.state.Manager.clear('AdapterManager');
    this.directoryPanel.root.reload();
  },

  getNodeBySelectedTab: function (tab) {
    var tabid = tab.id;
    nodeId = tabid.substr(4, tabid.length)  // tabid is "tab-jf23dfj-sd3fas-df33s-s3df"
    return this.getNodeById(nodeId)        // get the NODE using nodeid
  },

  getNodeById: function (nodeId) {
    if (this.directoryPanel.getNodeById(nodeId)) { //if nodeID exists it will find out NODE
      return this.directoryPanel.getNodeById(nodeId)
    } else {
      return false;
    }
  },

  reload: function () {
    this.directoryPanel.root.reload();
  },

  onDoubleClick: function (node) {
    if (node.attributes.type == 'GraphsNode') {
      this.fireEvent('addgraphmap', this, node);
    } else if (node.attributes.type == 'GraphNode') {
      this.fireEvent('opengraphmap', this, node);
    }
  },

  onClick: function (node) {
    try {
      this.propertyPanel.setSource(node.attributes.record);

    } catch (e) {
      //  alert(e);
    }
  }

});


