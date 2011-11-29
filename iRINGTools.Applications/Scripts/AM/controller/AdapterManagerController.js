﻿Ext.define('AM.controller.AdapterManagerController', {
  extend: 'Ext.app.Controller',
  views: [
        'directory.DirectoryPanel',
        'directory.DirectoryTree',
        'directory.ScopePanel',
        'directory.ApplicationPanel',
        'directory.DataGridPanel',
        'directory.GraphPanel',
        'directory.ValuelistPanel',
        'directory.ValuelistMapPanel',
        'common.PropertyPanel',
        'common.ContentPanel',
        'common.CenterPanel',
        'search.SearchPanel',
        'search.SearchTree',
        'mapping.MappingPanel',
        'mapping.MappingTree',
        'mapping.MapProperty',
        'mapping.ClassmapForm',
        'mapping.MapValuelist'
    ],
  stores: [
        'DirectoryStore'
  //, 'SearchStore'
  // , 'MappingStore'
    ],
  models: [
        'DirectoryModel'
        , 'DataLayerModel'
        , 'DynamicModel'
        , 'SearchModel'
        , 'MappingModel'
    ],
  refs: [
        {
          ref: 'dirTree',
          selector: 'viewport > directorypanel > directorytree'
        },
        {
          ref: 'dirProperties',
          selector: 'viewport > directorypanel > propertypanel'
        },
        {
          ref: 'mainContent',
          selector: 'viewport > centerpanel > contentpanel'
        },
        {
          ref: 'mappingPanel',
          selector: 'mappingtree'
        },
        {
          ref: 'searchPanel',
          selector: 'viewport > centerpanel > searchpanel'
        },
        {
          ref: 'searchProperty',
          selector: 'viewport > centerpanel > searchpanel > propertypanel'
        },
        {
          ref: 'searchContent',
          selector: 'viewport > centerpanel > searchpanel > contentpanel'
        }
    ],
  parentClass: null,
  init: function () {
    this.control({
      'menu button[action=newscope]': {
        click: this.newScope
      },
      'menu button[action=editscope]': {
        click: this.editScope
      },
      'menu button[action=deletescope]': {
        click: this.deleteScope
      },
      'menu button[action=newapp]': {
        click: this.newApplication
      },
      'menu button[action=editapp]': {
        click: this.editApplication
      },
      'menu button[action=deleteapp]': {
        click: this.deleteApplication
      },
      'menu button[action=showdata]': {
        click: this.showDataGrid
      },
      'menu button[action=newgraph]': {
        click: this.onNewGraph
      },
      'menu button[action=refreshfacade]': {
        click: this.onRefreshFacade
      },
      'menu button[action=editgraph]': {
        click: this.onEditGraph
      },
      'menu button[action=deletegraph]': {
        click: this.onDeleteGraph
      },
      'menu button[action=newvaluelist]': {
        click: this.onNewValueList
      },
      'menu button[action=editvaluelist]': {
        click: this.onEditValueList
      },
      'menu button[action=deletevaluelist]': {
        click: this.onDeleteValueList
      },
      'menu button[action=newvaluemap]': {
        click: this.onNewValueMap
      },
      'menu button[action=editvaluemap]': {
        click: this.onEditValueMap
      },
      'menu button[action=deletevaluemap]': {
        click: this.onDeleteValueMap
      },
      'menu button[action=opengraph]': {
        click: this.openGraphMap
      },
      'menu button[action=addclassmap]': {
        click: this.addClassMap
      },
      'menu button[action=mapproperty]': {
        click: this.mapProperty
      },
      'menu button[action=makepossessor]': {
        click: this.makePossessor
      },
      'menu button[action=resetmapping]': {
        click: this.resetMapping
      },
      'menu button[action=templatemapdelete]': {
        click: this.deleteTemplateMap
      },
      'menu button[action=deleteclassmap]': {
        click: this.deleteClassMap
      },
      'menu button[action=mapvaluelist]': {
        click: this.mapValueList
      },
      'button[action=search]': {
        click: this.onSearchRdl
      },
      'directorypanel directorytree': {
        beforeload: this.beforeLoad
      }
    });
  },

  onRefreshFacade: function () {

    var tree = this.getDirTree(),
        node = tree.getSelectedNode();

    tree.getEl().mask('Loading', 'x-mask-loading');
    Ext.Ajax.request({
      url: 'facade/refreshFacade',
      method: 'POST',
      params: {
        scope: node.data.id
      },
      success: function (o) {
        tree.onReload(node);
        tree.getEl().unmask();
      },
      failure: function (f, a) {
        tree.getEl().unmask();
        Ext.Msg.alert('Warning', 'Error Refreshing Facade!!!');
      }
    });
    tree.graphMenu.hide();
  },

  mapValueList: function () {
    var tree = this.getMappingPanel(),
          node = tree.getSelectedNode(),
          graphName = tree.graphName,
          contextName = tree.contextName,
          endpoint = tree.endpoint;
    this.getParentClass(node);
    var conf = {
      tree: tree,
      contextName: contextName,
      endpoint: endpoint,
      graphName: graphName,
      mappingNode: node,
      index: node.parentNode.parentNode.indexOf(node.parentNode),
      classId: this.parentClass
    },

    win = Ext.widget('valuelistform', conf);
    win.on('Save', function () {
      win.destroy();
      tree.onReload(node);
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.destroy();
    }, this);
    win.show();
    tree.rolemapMenu.hide();
  },

  deleteClassMap: function () {
    var tree = this.getMappingPanel(),
          node = tree.getSelectedNode(),
          contextName = tree.contextName,
          endpoint = tree.endpoint,
          graphName = tree.graphName,
          index = node.parentNode.parentNode.parentNode.indexOf(node.parentNode.parentNode);
    Ext.Ajax.request({
      url: 'mapping/deleteclassmap',
      method: 'POST',
      params: {
        contextName: contextName,
        endpoint: endpoint,
        graphName: graphName,
        classId: node.data.identifier,
        className: getLastXString(node.data.id, 1),
        parentClass: node.parentNode.parentNode.parentNode.data.identifier,
        parentTemplate: node.parentNode.parentNode.data.record.id,
        parentRole: node.parentNode.data.record.id,
        index: index
      },
      success: function () {
        tree.onReload();
      },
      failure: function () { }
    });
  },

  deleteTemplateMap: function () {
    var tree = this.getMappingPanel(),
          node = tree.getSelectedNode(),
          index = node.parentNode.indexOf(node),
          contextName = tree.contextName,
          endpoint = tree.endpoint;

    this.getParentClass(node);
    Ext.Ajax.request({
      url: 'mapping/deleteTemplateMap',
      method: 'POST',
      params: {
        contextName: contextName,
        endpoint: endpoint,
        mappingNode: node.data.id,
        parentIdentifier: this.parentClass,
        identifier: node.data.identifier,
        index: index
      },
      success: function () {
        tree.onReload();

      },
      failure: function () { }
    });
    tree.templatemapMenu.hide();
  },

  resetMapping: function () {
    var tree = this.getMappingPanel(),
        node = tree.getSelectedNode(),
        index = node.parentNode.parentNode.indexOf(node.parentNode),
        contextName = tree.contextName,
        endpoint = tree.endpoint,
        graphName = tree.graphName;

    this.getParentClass(node);
    Ext.Ajax.request({
      url: 'mapping/resetmapping',
      method: 'POST',
      params: {
        contextName: contextName,
        endpoint: endpoint,
        roleId: node.data.record.id,
        templateId: node.parentNode.data.record.id,
        parentClassId: node.parentNode.parentNode.data.identifier,
        graphName: graphName,
        index: index
      },
      success: function () {
        tree.onReload();
      },
      failure: function () { }
    });
    tree.rolemapMenu.hide();
  },

  makePossessor: function () {
    var tree = this.getMappingPanel(),
        node = tree.getSelectedNode(),
        index = node.parentNode.parentNode.indexOf(node.parentNode),
        graphName = tree.graphName,
        contextName = tree.contextName,
        endpoint = tree.endpoint;
    Ext.Ajax.request({
      url: 'mapping/makePossessor',
      method: 'POST',
      params: {
        contextName: contextName,
        endpoint: endpoint,
        graphName: graphName,
        roleName: getLastXString(node.data.id, 1),
        classId: node.parentNode.parentNode.data.identifier,
        index: index
      },
      success: function () {
        tree.onReload();
      },
      failure: function () { }
    });
    tree.rolemapMenu.hide();
  },

  addClassMap: function () {
    var tree = this.getMappingPanel(),
        node = tree.getSelectedNode(),
        graphName = tree.graphName,
        contextName = tree.contextName,
        endpoint = tree.endpoint;
    this.getParentClass(node);
    var conf = {
      tree: tree,
      contextName: contextName,
      endpoint: endpoint,
      graphName: graphName,
      mappingNode: node,
      index: node.parentNode.parentNode.indexOf(node.parentNode),
      parentClassId: node.parentNode.parentNode.data.identifier
    };
    var win = Ext.widget('classmapform', conf);
    win.on('Save', function () {
      win.destroy();
      tree.onReload(node);
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.destroy();
    }, this);
    win.show();
    tree.rolemapMenu.hide();
  },

  mapProperty: function () {
    var tree = this.getMappingPanel(),
      node = tree.getSelectedNode(),
      contextName = tree.contextName,
      endpoint = tree.endpoint,
      graphName = tree.graphName,
      conf = {
        tree: tree,
        contextName: contextName,
        endpoint: endpoint,
        graphName: graphName,
        classId: node.parentNode.parentNode.data.identifier,
        mappingNode: node
      },
      win = Ext.widget('propertyform', conf);

    win.on('Save', function () {
      win.destroy();
      tree.onReload(node);
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.destroy();
    }, this);
    win.show();
    tree.rolemapMenu.hide();
  },

  openGraphMap: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        content = this.getMainContent(),
        contextName = node.data.property.context,
        endpoint = node.data.property.endpoint,
        graphName = node.data.text,
        me = this;

    var conf = {
      contextName: contextName,
      endpoint: endpoint,
      record: node,
      graphName: graphName
    },
      maptree = Ext.widget('mappingtree', conf);
    var mapprop = Ext.widget('propertypanel', { title: 'Mapping Details', region: 'east', width: 350, height: 150, split: true, collapsible: true });
    var panconf = {
      title: 'GraphMap - ' + contextName + "." + endpoint + '.' + node.data.text,
      id: 'GraphMap - ' + contextName + "-" + endpoint + '.' + node.data.text,
      height: 300,
      minSize: 150,
      layout: {
        type: 'border',
        padding: 2
      },
      split: true,
      closable: true,
      iconCls: 'tabsMapping',
      items: []
    },

    mappanel = Ext.widget('panel', panconf);
    mappanel.items.add(maptree);
    mappanel.items.add(mapprop);
    maptree.on('beforeitemexpand', function () {
      content.getEl().mask('Loading...');
    }, this);
    maptree.on('load', function () {
      content.getEl().unmask();
    }, this);
    maptree.on('itemexpand', function () {
      content.getEl().unmask();
    }, this);
    maptree.on('itemclick', function (view, model, n, index) {
      var obj = model.store.getAt(index).data;
      if (obj.property != null && obj.property != "") {
        mapprop.setSource(obj.property);
      } else {
        if (obj.record.type != null && !obj.record.roleMaps) {
          var arrStr = '';
          for (var i in obj.record) {
            if (i != 'type' && obj.record[i] != null && obj.record[i] != '') {
              arrStr += i + '=' + obj.record[i] + '&';
            }
          };
          var type = me.getObjectType(obj.record.type);
          arrStr += 'typeDescription=' + type;
          var arr = Ext.Object.fromQueryString(arrStr);
          mapprop.setSource(arr);
        }
        else {
          mapprop.setSource(obj.record);
        }
      }

    });
    var exist = content.items.map[panconf.id];
    if (exist == null) {
      content.add(mappanel).show();
    } else {
      exist.show();
    }
    tree.graphMenu.hide();
  },

  getObjectType: function (type) {
    switch (type) {
      case 0:
        return 'Property';
      case 1:
        return 'Possessor';
      case 2:
        return 'Reference';
      case 3:
        return 'FixedValue';
      case 4:
        return 'DataProperty';
      case 5:
        return 'ObjectProperty';
    }
  },

  onDeleteValueMap: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode();
    Ext.Ajax.request({
      url: 'mapping/deleteValueMap',
      method: 'POST',
      params: {
        mappingNode: node.data.id,
        oldClassUrl: node.data.record.uri
      },
      success: function () {
        tree.onReload();
      },
      failure: function () { }
    });
  },

  onEditValueMap: function () {
    var tree = this.getDirTree(),
      node = tree.getSelectedNode(),
      conf = {
        id: 'tab-' + node.data.id,
        record: node.data.record,
        node: node,
        title: 'Edit Value List \"' + node.data.text + '\"'
      };
    var win = Ext.widget('valuelistmappanel', conf);
    win.on('save', function () {
      win.close();
      tree.onReload(node);
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
    tree.valueListMenu.hide();
  },

  onNewValueMap: function () {
    var tree = this.getDirTree(),
      node = tree.getSelectedNode(),
      conf = {
        id: 'tab-' + node.data.id,
        record: node.data.record,
        node: node,
        title: 'Add new ValueListMap to valueList'
      };
    var win = Ext.widget('valuelistmappanel', conf);
    win.on('save', function () {
      win.close();
      tree.onReload(node);
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
    tree.valueListMenu.hide();
  },

  onDeleteValueList: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode();
    Ext.Ajax.request({
      url: 'mapping/deletevaluelist',
      method: 'POST',
      params: {
        contextName: node.data.property.context,
        endpoint: node.data.property.endpoint,
        valueList: getLastXString(node.id, 1)
      },
      success: function () {
        tree.onReload(node);
      },
      failure: function () { }
    });
    tree.valueListMenu.hide();
  },

  onNewValueList: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'tab-' + node.data.id,
          record: node.data.record,
          node: node,
          state: 'new',
          nodeId: node.data.id,
          title: 'Add Value List Name'
        };
    var win = Ext.widget('valuelistpanel', conf);
    win.on('save', function () {
      win.close();
      tree.onReload();
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
    tree.valueListsMenu.hide();
  },

  onEditValueList: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'tab-' + node.data.id,
          record: node.data.record,
          node: node,
          state: 'edit',
          title: 'Edit Value List \"' + node.data.text + '\"'
        };
    var win = Ext.widget('valuelistpanel', conf);
    win.on('save', function () {
      win.close();
      tree.onReload(node);
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
    tree.valueListMenu.hide();
  },

  onNewGraph: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'tab-' + node.data.id,
          record: node.data.record,
          node: node,
          state: 'new',
          title: 'Add new GraphMap to Mapping',
          iconCls: 'tabsGraph',
          height: 200,
          width: 430
        };
    var win = Ext.widget('graphmapform', conf);
    win.on('save', function () {
      win.close();
      tree.onReload();
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
    tree.graphsMenu.hide();
  },

  onEditGraph: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'tab-' + node.data.id,
          record: node.data.record,
          node: node,
          state: 'edit',
          title: 'Edit Graph Map \"' + node.data.text + '\"',
          iconCls: 'tabsGraph',
          height: 200,
          width: 430
        };
    var win = Ext.widget('graphmapform', conf);
    win.on('save', function () {
      win.close();
      tree.onReload(node);
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
    tree.graphMenu.hide();
  },

  onDeleteGraph: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode();
    Ext.Ajax.request({
      url: 'mapping/deletegraphmap',
      method: 'POST',
      params: {
        contextName: node.data.property.context,
        endpoint: node.data.property.endpoint,
        mappingNode: node.id,
        graphName: getLastXString(node.id, 1)
      },
      success: function () {
        tree.onReload();
      },
      failure: function () { }
    });
    tree.graphMenu.hide();
  },

  onSearchRdl: function () {
    var pan = this.getSearchPanel(),
          searchText = pan.dockedItems.items[1].items.items[0].getValue(),
          isreset = pan.dockedItems.items[1].items.items[2].checked,
          content = this.getSearchContent(),
          propPanel = this.getSearchProperty(),
          value = pan.dockedItems.items[1].items.items[5].value;
    if (!searchText && searchText.length == 0) return;

    var conf = {
      title: searchText,
      id: 'tab_' + searchText
    };
    var tree = Ext.widget('searchtree', conf);
    tree.on('itemexpand', function () {
      content.getEl().unmask();
    }, this);
    tree.on('itemclick', this.onSearchClick, this);
    tree.on('beforeload', function (store, action) {
      content.getEl().mask('Loading...');
      store.proxy.extraParams.type = (action.node.data.type == "" ? 'SearchNode' : action.node.data.type);
      if (searchText != undefined && searchText != '') {
        store.proxy.extraParams.query = searchText;
        store.proxy.extraParams.reset = isreset;
        store.proxy.extraParams.limit = value;
      }
      if (action.node.parentNode && (action.node.data.identifier == null || action.node.data.identifier == '')) {
        store.proxy.extraParams.id = action.node.parentNode.data.identifier;
      } else {
        store.proxy.extraParams.id = action.node.data.identifier;
      }
    }, this);
    tree.on('beforeitemexpand', function () {
      content.getEl().mask('Loading...');
    }, this);
    tree.on('load', function (store, model) {
      content.getEl().unmask();
      if (model.data.type == "ClassNode") {
        try {
          if (model.childNodes.length > 0) {
            propPanel.setSource(model.childNodes[0].data.record);
          }
          else {
            propPanel.setSource(model.data.record);
          }
        } catch (ex) { }
      }
    }, this);

    var exist = content.items.map[conf.id];
    if (exist == null) {
      content.add(tree).show();
    } else {
      exist.show();
    }
    tree.getStore().load();

  },

  onSearchClick: function (view, model, n, idx) {
    var content = this.getSearchContent(),
      propPanel = this.getSearchProperty();
    var node = model.store.getAt(idx);
    try {
      node.on('expand', function () {
        content.getEl().unmask();
      }, this);
      if (node.data.type == "ClassNode" && model.firstChild) {
        propPanel.setSource(model.firstChild.data.record);
      } else {
        propPanel.setSource(node.data.record);
      }
    } catch (e) { }
    node.expand();
  },

  showDataGrid: function () {
    var tree = this.getDirTree(),
        content = this.getMainContent(),
        node = tree.getSelectedNode(),
        context = node.data.property.context,
        endpoint = node.data.property.endpoint,
        graph = node.data.text;
    var conf = {
      title: context + '.' + endpoint + '.' + graph,
      id: context + endpoint + graph + Ext.id(),
      scope: scope,
      //start: 0,
      //limit: 25,
      app: app,
      graph: graph
    };
    var exist = content.items.map[conf.id];
    if (exist) {
      exist.show();
      tree.appDataMenu.hide();
      return true;
    }
    var newtab = Ext.widget('datagridpanel', conf);
    content.add(newtab).show();
    tree.appDataMenu.hide();
    return true;
  },

  newApplication: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'newwin-' + node.data.id,
          path: node.internalId,
          state: 'new',
          record: node.data.record,
          node: node,
          title: 'Add New Endpoint',
          iconCls: 'tabsApplication'
        };
    var win = Ext.widget('applicationform', conf);

    win.on('save', function () {
      win.close();
      tree.onReload();
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
  },

  editApplication: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'tab-' + node.data.id,
          path: node.internalId,
          record: node.data.record,
          node: node,
          state: 'edit',
          node: node.parentNode,
          title: 'Edit Endpint \"' + node.data.text + '\"',
          iconCls: 'tabsApplication'
        };
    var win = Ext.widget('applicationform', conf);

    win.on('save', function () {
      win.close();
      tree.onReload();
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
  },

  deleteApplication: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode();
    Ext.Ajax.request({
      url: 'directory/deleteEntry',
      method: 'POST',
      params: {
        'nodeid': node.data.id
      },
      success: function () {
        tree.onReload();
        if (node.parentNode.expanded == false)
          node.parentNode.expand();
      },
      failure: function () {
        //Ext.Msg.alert('Warning', 'Error!!!');
        var message = 'Error deleting application!';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });
    tree.applicationMenu.hide();
  },

  newScope: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'tab-' + node.data.id,
          record: node.data.record,
          state: 'new',
          path: node.internalId,
          node: node,
          title: 'Add New Folder',
          iconCls: 'tabsScope',
          url: 'directory/folder'
        };
    var win = Ext.widget('scopeform', conf);

    win.on('save', function () {
      win.close();
      tree.onReload();
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();

    win.items.first().getForm().findField('Name').clearInvalid();

  },

  editScope: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode(),
        conf = {
          id: 'tab-' + node.id,
          record: node.data.record,
          state: 'new',
          path: node.id,
          node: node,
          title: 'Edit Folder \"' + node.data.text + '\"',
          iconCls: 'tabsScope',
          url: 'directory/folder'
        };
    var win = Ext.widget('scopeform', conf);

    win.on('save', function () {
      win.close();
      tree.onReload();
      if (node.get('expanded') == false)
        node.expand();
    }, this);

    win.on('Cancel', function () {
      win.close();
    }, this);
    win.show();
  },

  deleteScope: function () {
    var tree = this.getDirTree(),
        node = tree.getSelectedNode();
    Ext.Ajax.request({
      url: 'directory/deleteEntry',
      method: 'POST',
      params: {
        'nodeid': node.data.id
      },
      success: function () {
        tree.onReload(node);
      },
      failure: function () {
        var message = 'Error deleting scope!';
        showDialog(400, 100, 'Warning', message, Ext.Msg.OK, null);
      }
    });

    var editScopePane = Ext.getCmp('editwin-' + node.id);
    if (editScopePane)
      editScopePane.destroy();
  },

  beforeLoad: function (store, rec) {
    store.proxy.extraParams.type = rec.node.data.type;
    if (rec.node.data.record != undefined && rec.node.data.record.Related != undefined) {
      store.proxy.extraParams.related = rec.node.data.record.Related;
    }
  },

  getParentClass: function (n) {
    if (n.parentNode != undefined) {
      if ((n.parentNode.data.type == 'ClassMapNode'
         || n.parentNode.data.type == 'GraphMapNode')
         && n.parentNode.data.identifier != undefined) {
        this.parentClass = n.parentNode.data.identifier;
        return this.parentClass;
      }
      else {
        this.getParentClass(n.parentNode);
      }
    }
    return this.parentClass;
  }
});