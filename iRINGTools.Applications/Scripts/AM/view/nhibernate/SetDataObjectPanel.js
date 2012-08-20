Ext.define('AM.view.nhibernate.SetDataObjectPanel', {
  extend: 'Ext.form.Panel',
  alias: 'widget.setdataobjectpanel',
  border: false,
  name: 'dataObject',
  contextName: null,
  endpoint: null,
  node: null,
  tree: null,
  dbDict: null,
  editor: null,
  autoScroll: true,
  monitorValid: true,   
  bodyStyle: 'background:#eee;padding:10px 0px 0px 10px',
  defaults: {
    anchor: '100%',
    xtype: 'textfield',
    labelWidth: 160,
    allowBlank: false
  },
  initComponent: function () {
    var me = this;
    var keyDelimiter;
    var treeNode = this.node;
    var tree = this.tree;
    var dbDict = this.dbDict;
    var editor = this.editor;
    if (treeNode.data.property.keyDelimiter == 'null' || !treeNode.data.property.keyDelimiter || treeNode.data.property.keyDelimiter == undefined)
      keyDelimiter = '_';
    else
      keyDelimiter = treeNode.data.property.keyDelimiter;

    me.items = [{
      xtype: 'label',
      text: 'Data Object',
      cls: 'x-form-item',
      style: 'font-weight:bold;'
    }, {
      name: 'tableName',
      fieldLabel: 'Table Name',
      value: treeNode.data.property.tableName,
      disabled: true
    }, {
      name: 'objectNamespace',
      fieldLabel: 'Object Namespace',
      allowBlank: false,
      value: treeNode.data.property.objectNamespace
    }, {
      name: 'objectName',
      fieldLabel: 'Object Name',
      allowBlank: false,
      validationEvent: "blur",
      regex: new RegExp("^[a-zA-Z_][a-zA-Z0-9_]*$"),
      regexText: '<b>Error</b></br>Invalid Value. A valid value should start with alphabet or "_", and follow by any number of "_", alphabet, or number characters',
      value: treeNode.data.property.objectName
    }, {
      name: 'keyDelimeter',
      fieldLabel: 'Key Delimiter',
      value: keyDelimiter,
      allowBlank: true
    }, {
      name: 'description',
      xtype: 'textarea',
      height: 150,
      fieldLabel: 'Description',
      value: treeNode.data.property.description,
      allowBlank: true
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
        handler: function (f) {
          var form = me.getForm();
          if (treeNode) {
            var treeNodeProps = treeNode.data.property;
            var objectNameField = form.findField('objectName');
            var objNam = objectNameField.getValue();

            if (objectNameField.validate())
              treeNodeProps['objectName'] = objNam;
            else {
              showDialog(400, 100, 'Warning', "Object Name is not valid. A valid object name should start with alphabet or \"_\", and follow by any number of \"_\", alphabet, or number characters", Ext.Msg.OK, null);
              return;
            }

            var oldObjNam = treeNodeProps['objectName'];
            treeNodeProps.tableName = form.findField('tableName').getValue();
            treeNodeProps.objectName = objNam;
            treeNodeProps.keyDelimiter = form.findField('keyDelimeter').getValue();
            treeNodeProps.description = form.findField('description').getValue();

            for (var ijk = 0; ijk < dbDict.dataObjects.length; ijk++) {
              var dataObject = dbDict.dataObjects[ijk];
              if (treeNode.data.text.toUpperCase() != dataObject.objectName.toUpperCase())
                continue;
              dataObject.objectName = objNam;
            }

            treeNode.set('text', objNam);
            var rootNode = tree.getRootNode();

            for (var i = 0; i < rootNode.childNodes.length; i++) {
              var folderNode = rootNode.childNodes[i];
              var folderNodeProp = folderNode.data.property;
              if (folderNode.childNodes[2])
                var relationFolderNode = folderNode.childNodes[2];
              else
                var relationFolderNode = folderNode.attributes.children[2];

              if (!relationFolderNode)
                continue;

              if (relationFolderNode.childNodes)
                var relChildenNodes = relationFolderNode.childNodes;
              else
                var relChildenNodes = relationFolderNode.children;

              if (relChildenNodes) {
                for (var k = 0; k < relChildenNodes.length; k++) {
                  var relationNode = relChildenNodes[k];

                  if (relationNode.text == '')
                    continue;

                  if (relationNode.data)
                    var relationNodeAttr = relationNode.data

                  var relObjNam = relationNodeAttr.relatedObjectName;
                  if (relObjNam.toLowerCase() != objNam.toLowerCase() && relObjNam.toLowerCase() == oldObjNam.toLowerCase())
                    relationNodeAttr.relatedObjectName = objNam;

                  if (relationNodeAttr.relatedObjMap) {
                    var relatedObjPropMap = relationNodeAttr.relatedObjMap;

                    for (var iki = 0; iki < relatedObjPropMap.length; iki++) {
                      if (relatedObjPropMap[iki].relatedObjName.toLowerCase() == oldObjNam.toLowerCase())
                        relatedObjPropMap[iki].relatedObjName = objNam;
                    }
                  }
                }
              }
            }

            var items = editor.items.items;

            for (var i = 0; i < items.length; i++) {
              var relateObjField = items[i].getForm().findField('relatedObjectName');
              if (relateObjField)
                if (relateObjField.getValue().toLowerCase() == oldObjNam.toLowerCase())
                  relateObjField.setValue(objNam);
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
        scope: this,
        handler: function (f) {
          var form = me.getForm();
          if (treeNode.data.property) {
            form.findField('objectName').setValue(treeNode.data.property.objectName);
            form.findField('objectNamespace').setValue(treeNode.data.property.objectNamespace);
            form.findField('keyDelimeter').setValue(treeNode.data.property.keyDelimiter);
            form.findField('description').setValue(treeNode.data.property.description);
          }
        }
      }]
    });
    this.callParent(arguments);
  },




  setActiveRecord: function (record) {
    if (record) {
      this.getForm().setValues(record);
    } else {
      this.getForm().reset();
    }
  }
});


