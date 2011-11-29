Ext.define('AM.store.DirectoryStore', {
  extend: 'Ext.data.TreeStore',
  model: 'AM.model.DirectoryModel',
  clearOnLoad: true,
  root: {
    expanded: true,
    type: 'ScopesNode',
    iconCls: 'scopes',
    text: 'Scopes',
    security: ''
  },

  listeners: {
    beforeload: function (store, operation, options) {
      if (operation.node != undefined) {
        var operationNode = operation.node.data;
        var param = store.proxy.extraParams;

        if (operationNode.type != undefined)
          param.type = operationNode.type;

        if (operationNode.record != undefined && operationNode.record.Related != undefined)
          param.related = operationNode.record.Related;

        if (operationNode.record != undefined) {
          operationNode.leaf = false;

          if (operationNode.record.context)
            param.contextName = operationNode.record.context;

          if (operationNode.record.endpoint)
            param.endpoint = operationNode.record.endpoint;

          if (operationNode.record.securityRole)
            param.security = operationNode.record.securityRole;

          if (operationNode.text != undefined)
            param.text = operationNode.text;
        }
        else if (operationNode.property != undefined) {
          operationNode.leaf = false;
          if (operationNode.property.context)
            param.contextName = operationNode.property.context;

          if (operationNode.property.endpoint)
            param.endpoint = operationNode.property.endpoint;

          if (operationNode.text != undefined)
            param.text = operationNode.text;
        }
      }
    
      
      //        console.log( 'manual load ' + operation.params );
      //        console.log( operation.params );
      //        console.log( 'proxy defined params ' + store.proxy.extraParams );
      //        console.log( store.proxy.extraParams )
    }
  }

});