/*
 * File: Scripts/AM/view/directory/DirectoryTree.js
 *
 * This file was generated by Sencha Architect version 2.2.2.
 * http://www.sencha.com/products/architect/
 *
 * This file requires use of the Ext JS 4.1.x library, under independent license.
 * License of Sencha Architect does not include license for Ext JS 4.1.x. For more
 * details see http://www.sencha.com/license or contact license@sencha.com.
 *
 * This file will be auto-generated each and everytime you save your project.
 *
 * Do NOT hand edit this file.
 */

Ext.define('AM.view.directory.DirectoryTree', {
  extend: 'Ext.tree.Panel',
  alias: 'widget.directorytree',

  stateId: 'directory-treestate',
  stateful: true,
  bodyStyle: 'background:#fff;padding:4px',
  store: 'DirectoryTreeStore',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      stateEvents: [
        'temcollapse',
        'itemexpand'
      ],
      viewConfig: {
        plugins: [
          Ext.create('Ext.tree.plugin.TreeViewDragDrop', {
            dragField: 'text',
            ddGroup: 'propertyGroup',
            dragGroup: 'propertyGroup',
            dragText: '{0}',
            enableDrop: false
          })
        ]
      },
      listeners: {
        itemclick: {
          fn: me.onClick,
          scope: me
        }
      }
    });

    me.callParent(arguments);
  },

  onClick: function(dataview, record, item, index, e, eOpts) {

    var me = this;
    try {
      var obj = record.store.getAt(index);
      var pan = dataview.up('panel').up('panel');
      prop = pan.down('propertygrid');
      prop.setSource(obj.data.property);
    } catch (e) {
      //  alert(e);
    }
  },

  getState: function() {

    var me = this;
    var nodes = [], state = me.callParent();
    me.getRootNode().eachChild(function (child) {
      // function to store state of tree recursively 
      var storeTreeState = function (node, expandedNodes) {
        if (node.isExpanded() && node.childNodes.length > 0) {
          expandedNodes.push(node.getPath('text'));

          node.eachChild(function (child) {
            storeTreeState(child, expandedNodes);
          });
        }
      };
      storeTreeState(child, nodes);
    });
    Ext.apply(state, {
      expandedNodes: nodes
    });
    return state;
  },

  applyState: function(state) {

    var me = this;
    var nodes = state.expandedNodes || [],
      len = nodes.length;
    me.collapseAll();
    Ext.each(nodes, function (path) {
      me.expandPath(path, 'text');
    });
    me.callParent(arguments);
  },

  onReload: function(content) {

    var me = this;
    var node = me.getSelectedNode();
    if (!node)
    node = me.getRootNode(); 

    var nodeInternalId = node.internalId;
    var context;
    var endpoint;
    var baseUrl;
    if(nodeInternalId && nodeInternalId.split('/').length>1){
      context = nodeInternalId.split('/')[0];
      endpoint= nodeInternalId.split('/')[1];
    }
    /*if (node.parentNode){ 
    context = node.parentNode.data.text;
    if(node.parentNode.parentNode!=null)  
    endpoint= node.parentNode.parentNode.data.text;
    //baseUrl = node.data.record.BaseUrl;
  }*/

  var dataRecord = node.data.record;
  var store = me.store;
  var path;
  var panel = me.up();
  var state = me.getState();
  var dirNode;
  if(me.body!=undefined)
  me.body.mask('Loading...', 'x-mask-loading');

  dbInfo = null; 
  dbDict = null;
  if (node) {
    if(context!=undefined && endpoint!=undefined){
      Ext.Ajax.request({
        url: 'AdapterManager/DBDictionary',//'NHibernate/DBDictionary',
        method: 'POST',
        timeout: 6000000,
        params: {
          scope: context,
          app: endpoint,
          baseUrl: baseUrl
        },
        success: function (response, request) {
          dbDict = Ext.JSON.decode(response.responseText);
          if(dbDict) {
            var cstr = dbDict.ConnectionString;
            if(cstr) {
              var nhibernateTreeObject = Ext.widget('nhibernatetree');
              //node.data.record.dbDict = dbDict;
              //dbInfo = me.getConnStrParts(cstr, node);//me.getConnStringParts(cstr, dirNode);
              //var selectTableNames = me.setTableNames(dbDict);
              dbInfo = nhibernateTreeObject.getConnStrParts(cstr, node);
            }
          }
        },
        failure: function (response, request) {
          //var dataObjPanel = content.items.map[contextName + '.' + endpoint + '.-nh-config'];;
        }
      });
    }

    store.load({
      callback: function (records, options, success) {
        var nodes = state.expandedNodes || [],
          len = nodes.length;
        if(len>0)
        me.collapseAll();
        Ext.each(nodes, function (path) {
          me.expandPath(path, 'text');
        });
        //me.body.unmask();
        if(me.body!=undefined)
        me.body.unmask();


        if(content!=undefined)
        content.body.unmask();
      }

    });
    store.on('beforeload', function (store, action) {
      //alert('beforeload...');
      dirNode = store.getNodeById(nodeInternalId);
      if(dirNode!=undefined){
        if(dirNode.data.record!=undefined){
          //if(dirNode.data.record.dbInfo == undefined)
          //dirNode.data.record.dbInfo = dbInfo;//dataRecord.dbInfo;
          //if(dirNode.data.record.dbDict == undefined)
          //dirNode.data.record.dbDict = dbDict;//dataRecord.dbDict;
        }

      }


    }, me);
    store.on('load', function (store, action) {
      //alert('afterload...');
      if(dbInfo == null && dataRecord!=undefined) 
      dbInfo = dataRecord.dbInfo;

      dirNode = store.getNodeById(nodeInternalId);
      if(dirNode!=undefined){
        if(dirNode.data.record!=undefined){
          if(dirNode.data.record.dbInfo == undefined)
          dirNode.data.record.dbInfo = dbInfo;//dataRecord.dbInfo;
          if(dirNode.data.record.dbDict == undefined)
          dirNode.data.record.dbDict = dbDict;//dataRecord.dbDict;
        }

      }


    }, me);
  }
  },

  getSelectedNode: function() {
    var me = this;
    var selected = me.getSelectionModel().getSelection();
    return selected[0];
  }

});