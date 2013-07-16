Ext.ns('AdapterManager');
/**
* @class AdapterManager.MappingPanel
* @extends Panel
* @author by Gert Jansen van Rensburg
*/
AdapterManager.MappingPanel = Ext.extend(Ext.Panel, {

    height: 300,
    minSize: 150,
    layout: 'border',
    split: true,
    closable: true,
    navigationUrl: null,
    propertyPanel: null,
    mappingPanel: null,
    rootNode: null,
    treeLoader: null,
    scope: null,
    application: null,
    graph: null,
    parentClass: null,
    //  contextButton: null,
    mappingMenu: null,
    graphmapMenu: null,
    templatemapMenu: null,
    rolemapMenu: null,
    classmapMenu: null,
    directoryPanel: null,
    contentPanel: null,
    searchPanel: null,

    iconCls: 'tabsMapping',

    /**
    * initComponent
    * @protected
    */
    initComponent: function () {

        this.tbar = new Ext.Toolbar();
        this.tbar.add(this.buildToolbar());
        //    this.tbar.add(this.contextButton);

        //    this.graphmapMenu = new Ext.menu.Menu();
        //    this.graphmapMenu.add(this.buildGraphmapMenu());

        this.templatemapMenu = new Ext.menu.Menu();
        this.templatemapMenu.add(this.buildTemplateMapMenu());

        this.rolemapMenu = new Ext.menu.Menu();
        this.rolemapMenu.add(this.buildRoleMapMenu());

        this.classmapMenu = new Ext.menu.Menu();
        this.classmapMenu.add(this.buildClassMapMenu());

        this.treeLoader = new Ext.tree.TreeLoader({
            baseParams: {
                type: null,
                id: null,
                range: null,
                index: null,
                graph: this.directoryPanel.getSelectedNode().attributes.id
            },
            url: this.navigationUrl
        });

        this.treeLoader.on("beforeload", function (treeLoader, node) {
            treeLoader.baseParams.type = node.attributes.type;
            treeLoader.baseParams.index = node.attributes.index;
            if (node.attributes.record != undefined) {
                treeLoader.baseParams.id = node.attributes.record.id;
            }
        }, this);

        this.rootNode = new Ext.tree.AsyncTreeNode({
            id: this.scope.Name + "/" + this.application.Name,
            text: 'Mapping',
            expanded: true,
            icon: 'Content/img/16x16/mapping.png',
            type: 'MappingNode'
        });

        this.mappingPanel = new Ext.tree.TreePanel({
            region: 'center',
          //  id: 'Mapping-Panel',
            enableDD: true,
            ddGroup: 'refdataGroup',
            split: true,
            border: true,
            collapseMode: 'mini',
            height: 300,
            layout: 'fit',
            lines: true,
            expandAll: true,
            rootVisible: false,
            pathSeparator: '>',
            lines: true,
            autoScroll: true,
            loader: this.treeLoader,
            root: this.rootNode,
            //bbar: new Ext.ux.StatusBar({ defaultText: 'Ready', statusAlign: 'right' }),
            stateful: true,
            getState: function () {
                var nodes = [];
                this.getRootNode().eachChild(function (child) {
                    //function to store state of tree recursively
                    var storeTreeState = function (node, expandedNodes) {
                        if (node.isExpanded() && node.childNodes.length > 0) {
                            expandedNodes.push(node.getPath());
                            node.eachChild(function (child) {
                                storeTreeState(child, expandedNodes);
                            });
                        }
                    };
                    storeTreeState(child, nodes);
                });

                return {
                    expandedNodes: nodes
                }
            },
            applyState: function (state, isOnClick) {
                var that = this;
                //this.getLoader().on('load', function () {
                if (isOnClick == true) {
                    var nodes = state.expandedNodes;
                    for (var i = 0; i < nodes.length; i++) {
                        if (typeof nodes[i] != 'undefined') {
                            that.expandPath(nodes[i]);
                        }
                    }
                }
                //});
            }
        });

        this.mappingPanel.on('beforenodedrop', this.onBeforeNodedrop, this);
        this.mappingPanel.on('expandnode', this.onExpandNode, this);
        this.mappingPanel.on('contextmenu', this.showContextMenu, this);
        this.mappingPanel.on('click', this.onClick, this);

        this.propertyPanel = new Ext.grid.PropertyGrid({
            title: 'Details',
            region: 'east',
            width: 350,
            split: true,
            stripeRows: true,
            collapsible: true,
            autoScroll: true,
            border: 0,
            frame: false,
            height: 150,
            selModel: new Ext.grid.RowSelectionModel({ singleSelect: true }),
            source: {},
            listeners: {
                beforeedit: function (e) { e.cancel = true; },
                afteredit: function (e) {
                    e.grid.getSelectionModel().selections.items[0].data.value = e.originalValue;
                    e.record.data.value = e.originalValue;
                    e.value = e.originalValue;
                    e.grid.getView().refresh();
                }
            }
        });

        this.items = [
          this.propertyPanel,
          this.mappingPanel
        ];

       // this.on('close', function () {
          //  var thisTreePanel = Ext.getCmp('Mapping-Panel');
           // thisTreePanel = thisTreePanel.findParentByType('tabpanel').getActiveTab().destroy();
       //  });

        var state = Ext.state.Manager.get('mapping-state-' + this.scope.Name + '-' + this.application.Name);

        if (state) {
            if (this.mappingPanel.expandPath(state) == false) {
                Ext.state.Manager.clear('mapping-state-' + this.scope.Name + '-' + this.application.Name);
                this.mappingPanel.root.reload();
            }
        }
        
        // super
        AdapterManager.MappingPanel.superclass.initComponent.call(this);

    },
    
    buildToolbar: function () {
        return [
      {
          text: 'Reload',
          handler: this.onReload,
          icon: 'Content/img/16x16/view-refresh.png',
          scope: this
      },
      {
          text: 'Save',
          handler: this.onSave,
          icon: 'Content/img/16x16/document-save.png',
          scope: this
      }
    ]
    },

    buildTemplateMapMenu: function () {
        return [
          {
              text: 'Delete TemplateMap',
              handler: this.onDeleteTemplateMap,
              icon: 'Content/img/16x16/edit-delete.png',
              scope: this
          }
      ]
    },

    buildRoleMapMenu: function () {
        return [
          {
              text: 'Add/Edit ClassMap',
              handler: this.onAddClassMap,
              icon: 'Content/img/16x16/document-new.png',
              scope: this
          },
          {
              text: 'Make Possessor',
              handler: this.onMakePossessor,
              // icon: '',
              scope: this
          },
          {
              text: 'Map Property',
              handler: this.onMapProperty,
              // icon: '',
              scope: this
          },
          {
              text: 'Map ValueList',
              handler: this.onMapValueList,
              // icon: '',
              scope: this
          },
          {
              text: 'Map Literal',
              handler: this.onMapConstant,
              //icon: '',
              scope: this
          },
          {
              text: 'Reset Mapping',
              handler: this.onResetMapping,
              //icon: '',
              scope: this
          }
      ]
    },

    buildClassMapMenu: function () {
        return [
          {
              text: 'Delete ClassMap',
              handler: this.onDeleteClassMap,
              icon: 'Content/img/16x16/edit-delete.png',
              scope: this
          }/*,
          {
            text: 'Change ClassMap',
            handler: this.onChangeClassMap,
            // icon:'',
            scope: this
          }*/
      ]
    },

    getSelectedNode: function () {
        var node = this.mappingPanel.getSelectionModel().getSelectedNode();
        return node;
    },

    onSave: function (c) {
        var that = this;
        Ext.Ajax.request({
            url: 'mapping/updateMapping',
            method: 'POST',
            params: {
                Scope: that.scope.Name,
                Application: that.application.Name
            },
            success: function (result, request) {
                that.onReload();
                //  var node = that.getSelectedNode();
                //  if (node.expanded == false)
                //    node.expand();
            },
            failure: function (result, request) {
                return false;
            }
        })
    },

    onReload: function () {
        //  this.mappingPanel.root.reload();
        //Ext.state.Manager.clear('mapping-state-' + this.scope.Name + '-' + this.application.Name);
        var panel = this.directoryPanel;
        
        var thisTreePanel = this.mappingPanel; //Ext.getCmp('Mapping-Panel');
      

        //get state from tree
        var state = thisTreePanel.getState();
        panel.body.mask('Loading', 'x-mask-loading');

        thisTreePanel.getLoader().load(thisTreePanel.getRootNode(), function () {
            panel.body.unmask();
            thisTreePanel.applyState(state, true);
        });
    },


    onReloadNode: function (node) {
        node.reload();
    },

    onExpandNode: function (node) {
        Ext.state.Manager.set('mapping-state-' + this.scope.Name + '-' + this.application.Name, node.getPath());
    },
    getParentClass: function (n) {

        if (n.parentNode != undefined) {
            if ((n.parentNode.attributes.type == 'ClassMapNode'
         || n.parentNode.attributes.type == 'GraphMapNode')
         && n.parentNode.attributes.identifier != undefined) {
                this.parentClass = n.parentNode.attributes.identifier;
                this.parentClassIndex = n.parentNode.attributes.index;
                return this.parentClass;
            }
            else {
                this.getParentClass(n.parentNode);
            }
        }

    },
    onBeforeNodedrop: function (e) {
        e.target.expand();

        this.getParentClass(e.target);
        var nodetype, thistype, icn, txt, templateId, rec, parentId, context, classMapIndex;

        if (e.target.attributes.type == 'RoleMapNode') {
            var that = this;
            var targetNode = e.target.attributes;
            var contextParts = targetNode.id.split('/');
            var classId = e.target.parentNode.parentNode.attributes.identifier;
            var classIndex = e.target.parentNode.parentNode.attributes.index;
            var templateIndex = e.target.parentNode.parentNode.indexOf(e.target.parentNode);

            if (targetNode.record.dataType.indexOf('xsd') != 0) {
                e.tree.getEl().mask('Loading...');

                Ext.Ajax.request({
                    url: 'mapping/makereference',
                    method: 'POST',
                    params: {
                        scope: contextParts[0],
                        app: contextParts[1],
                        graph: contextParts[2],
                        classId: classId,
                        templateIndex: templateIndex,
                        roleId: e.target.attributes.record.id,
                        roleName: e.target.attributes.record.name,
                        refClassId: e.dropNode.attributes.record.Uri,
                        refClassLabel: e.dropNode.attributes.record.Label,
                        classIndex: classIndex
                    },
                    success: function (result, request) {
                        e.tree.getEl().unmask();
                        that.onReload();
                    },
                    failure: function (result, request) {
                        e.tree.getEl().unmask();
                        //don't drop it
                        return false;
                    }
                })
            }
            else {
                return false;
            }
        }
        else if (e.data.node.attributes.type == 'TemplateNode') {
            ntype = e.target.attributes.type;
            parentid = e.target.attributes.identifier;
            classMapIndex = e.target.attributes.index;
            thistype = e.data.node.attributes.type;
            icn = 'Content/img/template-map.png';
            txt = e.data.node.attributes.record.Label;
            templateId = e.data.node.attributes.identifier;
            rec = e.data.node.attributes.record;
            context = e.target.id + '/' + txt;
            lf = false;
            var that = this;
            e.tree.getEl().mask('Loading...');
            Ext.Ajax.request({
                url: 'mapping/addtemplatemap',
                method: 'POST',
                params: {
                    nodetype: thistype,
                    parentType: ntype,
                    parentId: parentid,
                    classMapIndex: classMapIndex,
                    id: templateId,
                    ctx: context
                },
                success: function (result, request) {
                    e.tree.getEl().unmask();

                    var error = 'success = False';
                    var index = result.responseText.indexOf(error);

                    if (index != -1) {
                        var msg = result.responseText.substring(index + error.length + 2);
                        showDialog(500, 240, 'Adding Template Error', msg.substring(0, msg.length - 1), Ext.Msg.OK, null);
                    }

                    that.onReload();
                },
                failure: function (result, request) {
                    e.tree.getEl().unmask();
                    return false;
                }
            })
        }
        else {
            return false;
        }

        e.cancel = true; //don't want to remove it from the source
        return true;
    },

    onClose: function (btn, e) {
        if (btn != undefined) {
            var win = btn.findParentByType('window');
            if (win != undefined)
                win.close();
        }
    },

    onDeleteTemplateMap: function () {
        var that = this;
        var node = this.mappingPanel.getSelectionModel().getSelectedNode();
        var index = node.parentNode.indexOf(node);
        that.getParentClass(node);

        Ext.Ajax.request({
            url: 'mapping/deleteTemplateMap',
            method: 'POST',
            params: {
                Scope: this.scope.Name,
                Application: this.application.Name,
                mappingNode: node.id,
                parentIdentifier: that.parentClass,
                identifier: node.attributes.identifier,
                index: index,
                parentClassIndex: that.parentClassIndex
            },
            success: function (result, request) {
                that.onReload();
                //Ext.Msg.show({ title: 'Success', msg: 'Template [' + node.id.split('/')[3] + '] removed from mapping', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
            },
            failure: function (result, request) { }
        })

    },

    onResetMapping: function (node) {
        var that = this;
        var node = this.mappingPanel.getSelectionModel().getSelectedNode();
        var index = node.parentNode.parentNode.indexOf(node.parentNode);

        Ext.Ajax.request({
            url: 'mapping/resetmapping',
            method: 'POST',
            params: {
                mappingNode: node.attributes.id,
                roleId: node.attributes.record.id,
                templateId: node.parentNode.attributes.record.id,
                parentClassId: node.parentNode.parentNode.attributes.identifier,
                index: index,
                parentClassIndex: node.parentNode.parentNode.attributes.index
            },
            success: function (result, request) {
                that.onReload();
                //Ext.Msg.show({ title: 'Success', msg: 'Made [' + node.attributes.id.split('/')[4] + '] possessor role', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
            },
            failure: function (result, request) { }
        })
    },

    onMapConstant: function (node) {

        var mapnode = this.mappingPanel.getSelectionModel().getSelectedNode();
        var formid = 'propertytarget-' + this.scope.Name + '-' + this.application.Name;
        var form = new Ext.form.FormPanel({
            id: formid,
            layout: 'form',
            method: 'POST',
            border: false,
            frame: false,
            bbar: [
                { xtype: 'tbfill' },
                { text: 'Ok', scope: this, handler: this.onSubmitConstantValue },
                { text: 'Cancel', scope: this, handler: this.onClose }
            ],
            items: [
                { fieldLabel: 'Value', name: 'constantValue', xtype: 'textfield', width: 230, value: name, allowBlank: false }
             ]

        });



        var win = new Ext.Window({
            closable: true,
            modal: false,
            layout: 'form',
            title: 'Map Literal to RoleMap',
            items: form,
            // height: 120,
            width: 430,
            plain: true,
            scope: this
        });

        win.show();

    },

    onMapProperty: function (node) {
        var mapnode = this.mappingPanel.getSelectionModel().getSelectedNode();
        var formid = 'propertytarget-' + this.scope.Name + '-' + this.application.Name;
        var form = new Ext.form.FormPanel({
            id: formid,
            layout: 'form',
            method: 'POST',
            border: false,
            frame: false,
            bbar: [
                { xtype: 'tbfill' },
                { text: 'Ok', scope: this, handler: this.onSubmitPropertyMap },
                { text: 'Cancel', scope: this, handler: this.onClose }
            ],
            items: [
                { xtype: 'hidden', name: 'propertyName', id: 'propertyName' },
                { xtype: 'hidden', name: 'relatedObject', id: 'relatedObject' }
             ],
            html: '<div class="property-target' + formid + '" '
                  + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
                  + 'Drop a Property Node here.</div>',

            afterRender: function (cmp) {
                Ext.FormPanel.prototype.afterRender.apply(this, arguments);

                var propertyTarget = this.body.child('div.property-target' + formid);
                var propertydd = new Ext.dd.DropTarget(propertyTarget, {
                    ddGroup: 'propertyGroup',
                    notifyEnter: function (dd, e, data) {
                        if (data.node.attributes.type == 'DataPropertyNode' ||
              data.node.attributes.type == 'KeyDataPropertyNode')
                            return this.dropAllowed;
                        else
                            return this.dropNotAllowed;
                    },
                    notifyOver: function (dd, e, data) {
                        if (data.node.attributes.type == 'DataPropertyNode' ||
              data.node.attributes.type == 'KeyDataPropertyNode')
                            return this.dropAllowed;
                        else
                            return this.dropNotAllowed;
                    },
                    notifyDrop: function (dd, e, data) {
                        if (data.node.attributes.type == 'DataPropertyNode' ||
              data.node.attributes.type == 'KeyDataPropertyNode') {

                            var propPath = data.node.attributes.id.split('/');
                            propPath.splice(0, 5);

                            var propName = propPath.join('.');

                            Ext.get('propertyName').dom.value = propName;

                            var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + propName +
                                '</b></td></tr></table>';

                            Ext.getCmp(formid).body.child('div.property-target' + formid).update(msg);

                            return true;
                        }

                        return fase;
                    } //eo notifyDrop
                }); //eo propertydd
            }
        });

        var win = new Ext.Window({
            closable: true,
            modal: false,
            layout: 'form',
            title: 'Map Data Property to RoleMap',
            items: form,
            // height: 120,
            width: 430,
            plain: true,
            scope: this
        });

        win.show();

    },

    onSubmitConstantValue: function (btn, e) {
        var that = this;
        var form = btn.findParentByType('form');
        var win = btn.findParentByType('window');
        var constantValue = form.getForm().findField('constantValue').getValue();
        var node = this.mappingPanel.getSelectionModel().getSelectedNode();
        var index = node.parentNode.parentNode.indexOf(node.parentNode);

        if (form.getForm().isValid())
            Ext.Ajax.request({
                url: 'mapping/mapconstant',
                method: 'POST',
                params: {
                    constantValue: constantValue,
                    mappingNode: node.attributes.id,
                    classId: node.parentNode.parentNode.attributes.identifier,
                    index: index,
                    classIndex: node.parentNode.parentNode.attributes.index
                },
                success: function (result, request) {
                    win.close();

                    var error = 'success = False';
                    var index = result.responseText.indexOf(error);

                    if (index != -1) {
                        var msg = result.responseText.substring(index + error.length + 2);
                        showDialog(500, 240, 'Mapping Error', msg.substring(0, msg.length - 1), Ext.Msg.OK, null);
                    }
                    else {
                        that.onReload();
                    }
                },
                failure: function (result, request) {
                    //Ext.Msg.show({ title: 'Failure', msg: 'Failed to Map Property to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
                    var message = 'Failed to Map Constant to RoleMap';
                    showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                }
            })
    },

    onSubmitPropertyMap: function (btn, e) {
        var that = this;
        var form = btn.findParentByType('form');
        var win = btn.findParentByType('window');
        var propertyNames = Ext.get('propertyName').dom.value;
        var node = this.mappingPanel.getSelectionModel().getSelectedNode();
        var index = node.parentNode.parentNode.indexOf(node.parentNode);

        if (Ext.get('relatedObject').dom.value != undefined)
            var related = Ext.get('relatedObject').dom.value;

        if (form.getForm().isValid())
            Ext.Ajax.request({
                url: 'mapping/mapproperty',
                method: 'POST',
                params: {
                    propertyName: propertyNames,
                    mappingNode: node.attributes.id,
                    classId: node.parentNode.parentNode.attributes.identifier,
                    relatedObject: related,
                    index: index,
                    classIndex: node.parentNode.parentNode.attributes.index
                },
                success: function (result, request) {
                    win.close();

                    var error = 'success = False';
                    var index = result.responseText.indexOf(error);

                    if (index != -1) {
                        var msg = result.responseText.substring(index + error.length + 2);
                        showDialog(500, 240, 'Mapping Error', msg.substring(0, msg.length - 1), Ext.Msg.OK, null);
                    }
                    else {
                        that.onReload();
                    }
                },
                failure: function (result, request) {
                    //Ext.Msg.show({ title: 'Failure', msg: 'Failed to Map Property to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
                    var message = 'Failed to Map Property to RoleMap';
                    showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                }
            })
    },

    onMapValueList: function (node) {
        var mapnode = this.mappingPanel.getSelectionModel().getSelectedNode();
        var formid = 'valuelisttarget-' + this.scope.Name + '-' + this.application.Name;
        var form = new Ext.form.FormPanel({
            id: formid,
            layout: 'form',
            method: 'POST',
            border: false,
            frame: false,
            bbar: [
        { xtype: 'tbfill' },
        { text: 'Ok', scope: this, handler: this.onSubmitValuelistMap },
        { text: 'Cancel', scope: this, handler: this.onClose }
        ],
            items: [
              { xtype: 'hidden', name: 'objectNames', id: 'objectNames' },
              { xtype: 'hidden', name: 'propertyName', id: 'propertyName' }
             ],
            html: '<div class="property-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a Property Node here.</div>'
          + '<div class="class-target' + formid + '" '
          + 'style="border:1px silver solid;margin:5px;padding:8px;height:20px">'
          + 'Drop a ValueList Node here. </div>',

            afterRender: function (cmp) {
                Ext.FormPanel.prototype.afterRender.apply(this, arguments);

                // drag & drop data property
                var propertyTarget = this.body.child('div.property-target' + formid);
                var propertydd = new Ext.dd.DropTarget(propertyTarget, {
                    ddGroup: 'propertyGroup',
                    notifyEnter: function (dd, e, data) {
                        if (data.node.attributes.type == 'DataPropertyNode' ||
                data.node.attributes.type == 'KeyDataPropertyNode')
                            return this.dropAllowed;
                        else
                            return this.dropNotAllowed;
                    },
                    notifyOver: function (dd, e, data) {
                        if (data.node.attributes.type == 'DataPropertyNode' ||
                data.node.attributes.type == 'KeyDataPropertyNode')
                            return this.dropAllowed;
                        else
                            return this.dropNotAllowed;
                    },
                    notifyDrop: function (dd, e, data) {
                        if (data.node.attributes.type == 'DataPropertyNode' ||
                data.node.attributes.type == 'KeyDataPropertyNode') {

                            var propPath = data.node.attributes.id.split('/');
                            propPath.splice(0, 5);

                            var propName = propPath.join('.');

                            Ext.get('propertyName').dom.value = propName;

                            var msg = '<table style="font-size:13px"><tr><td>Property:</td><td><b>' + propName + '</table>'
                            Ext.getCmp(formid).body.child('div.property-target' + formid).update(msg)
                            return true;
                        }
                    } //eo notifyDrop
                }); //eo propertydd

                // drag & drop value list
                var valueListTarget = this.body.child('div.class-target' + formid);
                var classdd = new Ext.dd.DropTarget(valueListTarget, {
                    ddGroup: 'propertyGroup',
                    notifyEnter: function (dd, e, data) {
                        if (data.node.attributes.type != 'ValueListNode')
                            return this.dropNotAllowed;
                        else
                            return this.dropAllowed;
                    },
                    notifyOver: function (dd, e, data) {
                        if (data.node.attributes.type != 'ValueListNode')
                            return this.dropNotAllowed;
                        else
                            return this.dropAllowed;
                    },
                    notifyDrop: function (classdd, e, data) {
                        if (data.node.attributes.type != 'ValueListNode') {
                            var message = 'Please select a ValueList.';
                            showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                            return false;
                        }
                        Ext.get('objectNames').dom.value = data.node.id;
                        var mapNode = Ext.get('mappingPanel')
                        var msg = '<table style="font-size:13px"><tr><td>Value List:</td><td><b>' + data.node.id.split('/')[4] + '</b></td></tr>'
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
            title: 'Map Valuelist to RoleMap',
            items: form,
            //height: 180,
            width: 430,
            plain: true,
            scope: this
        });

        win.show();

    },

    onSubmitValuelistMap: function (btn, e) {
        var that = this;
        var form = btn.findParentByType('form');
        var win = btn.findParentByType('window');
        var objectname = Ext.get('objectNames').dom.value;
        var propertyNames = Ext.get('propertyName').dom.value;
        var node = this.mappingPanel.getSelectionModel().getSelectedNode();
        var index = node.parentNode.parentNode.indexOf(node.parentNode);

        if (form.getForm().isValid())
            Ext.Ajax.request({
                url: 'mapping/mapvaluelist',
                method: 'POST',
                params: {
                    propertyName: propertyNames,
                    objectNames: objectname,
                    mappingNode: node.attributes.id,
                    classId: node.parentNode.parentNode.attributes.identifier,
                    index: index,
                    classIndex: node.parentNode.parentNode.attributes.index
                },
                success: function (result, request) {
                    var rtext = result.responseText;
                    if (rtext.toUpperCase().indexOf('FALSE') == -1) {
                        that.onReload();
                        win.close();
                    }
                    else {
                        var ind = rtext.indexOf('}');
                        var ine = rtext.indexOf('at');
                        var len = rtext.length - ind - 1;
                        var msg = rtext.substring(ind + 1, ine - 7);
                        showDialog(400, 100, 'Valuelist mapping result - Error', msg, Ext.Msg.OK, null);
                    }
                    //Ext.Msg.show({ title: 'Success', msg: 'Mapped ValueList to Rolemap', icon: Ext.MessageBox.INFO, buttons: Ext.Msg.OK });
                },
                failure: function (result, request) {
                    //Ext.Msg.show({ title: 'Failure', msg: 'Failed to Map ValueList to RoleMap', icon: Ext.MessageBox.ERROR, buttons: Ext.Msg.CANCEL });
                    var message = 'Failed to Map ValueList to RoleMap';
                    showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
                }
            })
    },


    onMakePossessor: function () {
        var that = this;
        var node = this.mappingPanel.getSelectionModel().getSelectedNode();
        var index = node.parentNode.parentNode.indexOf(node.parentNode);

        Ext.Ajax.request({
            url: 'mapping/makePossessor',
            method: 'POST',
            params: {
                mappingNode: node.attributes.id,
                classId: node.parentNode.parentNode.attributes.identifier,
                node: node,
                index: index,
                classIndex: node.parentNode.parentNode.attributes.index
            },
            success: function (result, request) {
                that.onReload();
                //Ext.Msg.show({ title: 'Success', msg: 'Made [' + node.attributes.id.split('/')[4] + '] possessor role', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
            },
            failure: function (result, request) { }
        })
    },

    onAddClassMap: function () {
        var that = this;

        var classMapPanel = new AdapterManager.ClassMapPanel({
            title: 'Add/Edit ClassMap',
            root: this.mappingPanel.root,
            node: this.mappingPanel.getSelectionModel().getSelectedNode(),
            url: 'mapping/addclassmap'
        });

        classMapPanel.on('addClassMapComplete', function () {
            classMapPanel.close();
            that.onReload();
        }, this);

        classMapPanel.show();
    },

    onDeleteClassMap: function (mnode) {
        var that = this;
        var node = this.mappingPanel.getSelectionModel().getSelectedNode();
        var index = node.parentNode.parentNode.parentNode.indexOf(node.parentNode.parentNode);

        Ext.Ajax.request({
            url: 'mapping/deleteclassmap',
            method: 'POST',
            params: {
                mappingNode: node.attributes.id,
                classId: node.attributes.identifier,
                parentClass: node.parentNode.parentNode.parentNode.attributes.identifier,
                parentTemplate: node.parentNode.parentNode.attributes.record.id,
                parentRole: node.parentNode.attributes.record.id,
                index: index,
                parentClassIndex: node.parentNode.parentNode.parentNode.attributes.index
            },
            success: function (result, request) {
                that.onReload();
                //Ext.Msg.show({ title: 'Success', msg: 'Deleted Class Map from Mapping', icon: Ext.MessageBox.INFO, buttons: Ext.MessageBox.OK });
            },
            failure: function (result, request) { }
        })
    },

    onClick: function (node) {
        var templateTypes = ['Qualification', 'Definition']
        var roleTypes = ['Unknown', 'Property', 'Possessor', 'Reference', 'FixedValue', 'DataProperty', 'ObjectProperty'];
        var classLabelKey = 'value label';
        var source = {};

        for (var propName in node.attributes.record) {
            if (propName != 'dataLength') {
                var propValue = node.attributes.record[propName];

                if (propName == 'type') {
                    if (node.attributes.type == 'TemplateMapNode') {
                        propValue = templateTypes[propValue];
                    }
                    else if (node.attributes.type == 'RoleMapNode') {
                        propValue = roleTypes[propValue];
                    }
                }
                else if (propName == 'identifiers') {
                    propName = 'identifier';
                    propValue = propValue.join();
                }

                source[propName] = propValue;
            }
        }

        this.propertyPanel.setSource(source);
    },

    showContextMenu: function (node, event) {
        //  if (node.isSelected()) { 
        var x = event.browserEvent.clientX;
        var y = event.browserEvent.clientY;

        var obj = node.attributes;

        if (obj.type == "MappingNode") {
            this.mappingMenu.showAt([x, y]);
        } else if (obj.type == "TemplateMapNode") {
            this.templatemapMenu.showAt([x, y]);
        } else if (obj.type == "RoleMapNode") {
            this.rolemapMenu.showAt([x, y]);
        } else if (obj.type == "ClassMapNode") {
            this.classmapMenu.showAt([x, y]);
        }
        this.mappingPanel.getSelectionModel().select(node);
        this.onClick(node);
    }
});

  