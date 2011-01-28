﻿Ext.ns('AdapterManager');
/**
* @class AdapterManager.DirectoryPanel
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
    DirectoryPanel: null,

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

        this.DirectoryPanel = new Ext.tree.TreePanel({
            region: 'center',
            collapseMode: 'mini',
            height: 200,
            layout: 'fit',
            border: false,

            rootVisible: true,
            lines: true,
            //singleExpand: true,
            useArrows: true,

            loader: new Ext.tree.TreeLoader({
                dataUrl: this.navigationUrl
            }),

            root: {
                nodeType: 'async',
                text: 'Scopes',
                expanded: true,
                draggable: false,
                icon: 'Content/img/internet-web-browser.png',
                id: 'src',
                type: 'Scope'
            }

        });

        this.items = [
      this.DirectoryPanel
    ];

        //this.DirectoryPanel.getSelectionModel().on('selectionchange', this.onSelectionChange, this, this);
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
        var node = this.DirectoryPanel.getSelectionModel().getSelectedNode();
        var formType = "newForm";
        this.fireEvent('create', this, node, formType);
    },

    onOpen: function (btn, ev) {
        var node = this.DirectoryPanel.getSelectionModel().getSelectedNode();
        var formType = "editForm";
        this.fireEvent('open', this, node, formType);
    },

    onRemove: function (btn, ev) {
        var node = this.DirectoryPanel.getSelectionModel().getSelectedNode();
        this.fireEvent('remove', this, node);

    },

    onRefresh: function (node) {
        Ext.state.Manager.clear('AdapterManager');
        this.DirectoryPanel.root.reload();
    },

    getNodeBySelectedTab: function (tab) {
      // alert("1");
        var tabid = tab.id;
        nodeId = tabid.substr(4, tabid.length)  // tabid is "tab-jf23dfj-sd3fas-df33s-s3df"
        return this.getNodeById(nodeId)        // get the NODE using nodeid
    },

    getNodeById: function (nodeId) {
      //  alert("2");
        if (this.DirectoryPanel.getNodeById(nodeId)) { //if nodeID exists it will find out NODE
            return this.DirectoryPanel.getNodeById(nodeId)
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
        { fieldLabel: txtLableName, name: 'appName', allowBlank: true, xtype: 'textfield', width: 250, value: name },
        { fieldLabel: txtLableDescription, name: 'description', allowBlank: true, xtype: 'textarea', width: 250, value: scopeDescription }

        ];
    }



});