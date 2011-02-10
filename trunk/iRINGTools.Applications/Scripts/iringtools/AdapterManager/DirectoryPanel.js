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
    border: true,
    split: true,

    navigationUrl: null,
    directoryPanel: null,
    rootNode: null,
    treeLoader: null,

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {

        this.addEvents({
            create: true,
            open: true,
            remove: true,
            refresh: true,
            selectionchange: true

        });

        this.tbar = this.buildToolbar();

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
            draggable: false,
            icon: 'Content/img/internet-web-browser.png',
            type: 'ScopesNode'
        });

        this.directoryPanel = new Ext.tree.TreePanel({
            region: 'center',
            collapseMode: 'mini',
            height: 300,
            layout: 'fit',
            border: false,
            split: true,
            expandAll: true,
            rootVisible: true,
            lines: true,
            autoScroll: true,
            //singleExpand: true,
            useArrows: true,
            loader: this.treeLoader,
            root: this.rootNode

        });

        this.items = [
      this.directoryPanel
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

    buildToolbar: function () {
        return [
      {
          text: 'New',
          handler: this.onCreate,
          //icon: 'Content/img/list-add.png',
          scope: this
      },
      {
          text: 'Open',
          handler: this.onOpen,
          //icon: 'Content/img/document-properties.png',
          scope: this
      },
      {
          text: 'Delete',
          handler: this.onRemove,
          //icon: 'Content/img/list-remove.png',
          scope: this
      }
    ]
    },

    onCreate: function (btn, ev) {
        var node = this.directoryPanel.getSelectionModel().getSelectedNode();
        var formType = "newForm";
        this.fireEvent('create', this, node, formType);
    },

    onOpen: function (btn, ev) {
        var node = this.directoryPanel.getSelectionModel().getSelectedNode();
        var formType = "editForm";
        this.fireEvent('open', this, node, formType);
    },

    onRemove: function (btn, ev) {
        var node = this.directoryPanel.getSelectionModel().getSelectedNode();
        this.fireEvent('remove', this, node);

    },

    onRefresh: function (node) {
        //Ext.state.Manager.clear('AdapterManager');
        this.directoryPanel.root.reload();
    },

    getNodeBySelectedTab: function (tab) {
        // alert("1");
        var tabid = tab.id;
        nodeId = tabid.substr(4, tabid.length)  // tabid is "tab-jf23dfj-sd3fas-df33s-s3df"
        return this.getNodeById(nodeId)        // get the NODE using nodeid
    },

    getNodeById: function (nodeId) {
        //  alert("2");
        if (this.directoryPanel.getNodeById(nodeId)) { //if nodeID exists it will find out NODE
            return this.directoryPanel.getNodeById(nodeId)
        } else {
            return false;
        }
    },


    /**
    * buildform
    * @private
    */
    buildForm: function (node, formType) {

        var name, scopeDescription, txtLableName, txtLableDescription, valparentNodeID;

        // set valiable to Empty String.
        name = "";
        description = "";
        txtLableName = "";
        txtLableDescription = ""
        valparentNodeID = "";

        var obj = node.attributes;

        //Check whether the form need to open in Edit mode or not.
        if (formType == 'editForm') {
            if (node.attributes.type == 'Application') {
                name = node.attributes.ScopeApplication.Name;
                scopeDescription = node.attributes.ScopeApplication.Description;
            }
            else {
                name = node.attributes.Scope.Name;
                scopeDescription = node.attributes.Scope.Description;
            }
        }

        //Update fieldLabel based on node type.
        if (node.attributes.type == 'Application') {
            txtLableName = 'Application Name';
            txtLableDescription = 'Description';
        }
        else {
            txtLableName = 'Scope Name';
            txtLableDescription = 'Description';
        }

        if (node.parentNode != null) {
            valparentNodeID = node.parentNode.id;
            txtLableName = 'Application Name';
            txtLableDescription = 'Description';
        }

        return [
        { xtype: 'hidden', name: 'formType', value: formType },
        { xtype: 'hidden', name: 'nodeID', value: obj['id'] },
        { xtype: 'hidden', name: 'parentNodeID', value: valparentNodeID },
        { fieldLabel: txtLableName, name: 'appName', xtype: 'textfield', width: 250, value: name, allowBlank: false },
        { fieldLabel: txtLableDescription, name: 'description', allowBlank: true, xtype: 'textarea', width: 250, value: scopeDescription }

        ];
    }



});