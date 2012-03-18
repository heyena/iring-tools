Ext.define('AM.controller.Mapping', {
  extend: 'Ext.app.Controller',
  views: [
    'common.PropertyPanel',
    'common.ContentPanel',
    'common.CenterPanel',
    'mapping.MappingPanel',
    'mapping.MappingTree',
    'mapping.MapProperty',
    'mapping.ClassmapForm',
    'mapping.MapValuelist'
  ],
  stores: [],
  models: [
     'MappingModel'
  ],
  refs: [
        {
          ref: 'dirTree',
          selector: 'viewport > directorypanel > directorytree'
        },
        {
          ref: 'mainContent',
          selector: 'viewport > centerpanel > contentpanel'
        },
        {
          ref: 'mappingPanel',
          selector: 'mappingtree'
        }
  ],
  init: function () {
    this.control({
      'menu button[action=newgraph]': {
        click: this.onNewGraph
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
      }
    });
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
  }
});
