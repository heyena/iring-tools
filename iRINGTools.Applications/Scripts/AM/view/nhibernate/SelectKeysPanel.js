Ext.define('AM.view.nhibernate.SelectKeysPanel', {
    extend: 'Ext.form.Panel',
    alias: 'widget.selectkeyspanel',
    border: false,
    autoScroll: true,
    bodyStyle: 'background:#eee;padding:10px 10px 0px 10px',
    labelWidth: 160,
    defaults: { anchor: '100%' },

    initComponent: function () {
        this.items = [{
            xtype: 'label',
            fieldLabel: 'Select Keys',
            itemCls: 'form-title',
            labelSeparator: ''
        }, keysItemSelector];

        this.tbar = new Ext.Toolbar({
            items: [{
                xtype: 'tbspacer',
                width: 4
            }, {
                xtype: 'tbbutton',
                icon: 'Content/img/16x16/apply.png',
                text: 'Apply',
                tooltip: 'Apply the current changes to the data objects tree',
                handler: function (f) {
                    var selectedValues = keysItemSelector.toMultiselect.store.data.items;
                    var keysNode = keysItemSelector.treeNode;
                    var propertiesNode = keysNode.parentNode.childNodes[1];

                    for (var i = 0; i < keysNode.childNodes.length; i++) {
                        var found = false;

                        for (var j = 0; j < selectedValues.length; j++) {
                            if (selectedValues[j].data.text.toLowerCase() == keysNode.childNodes[i].text.toLowerCase()) {
                                found = true;
                                break;
                            }
                        }

                        if (!found) {
                            if (keysNode.childNodes[i].attributes.properties)
                                var properties = keysNode.childNodes[i].attributes.properties;
                            else if (keysNode.childNodes[i].attributes.attributes.properties)
                                var properties = keysNode.childNodes[i].attributes.attributes.properties;

                            if (properties) {
                                properties['isNullable'] = true;
                                delete properties.keyType;

                                propertyNode.appendChild({
                                    text: keysNode.childNodes[i].text,
                                    type: "dataProperty",
                                    leaf: true,
                                    iconCls: 'treeProperty',
                                    properties: properties
                                });

                                //propertyNode.iconCls = 'treeProperty';
                                //propertiesNode.appendChild(propertyNode);
                                keysNode.removeChild(keysNode.childNodes[i], true);
                                i--;
                            }
                        }
                    }

                    var nodeChildren = new Array();
                    for (var j = 0; j < keysNode.childNodes.length; j++)
                        nodeChildren.push(keysNode.childNodes[j].text);

                    for (var j = 0; j < selectedValues.length; j++) {
                        var found = false;
                        for (var i = 0; i < nodeChildren.length; i++) {
                            if (selectedValues[j].data.text.toLowerCase() == nodeChildren[i].toLowerCase()) {
                                found = true;
                                break;
                            }
                        }

                        if (!found) {
                            var newKeyNode;

                            for (var jj = 0; jj < propertiesNode.childNodes.length; jj++) {
                                if (propertiesNode.childNodes[jj].text.toLowerCase() == selectedValues[j].data.text.toLowerCase()) {
                                    var properties = propertiesNode.childNodes[jj].attributes.properties;
                                    properties.keyType = 'assigned';
                                    properties.nullable = false;

                                    propertiesNode.appendChild({
                                        text: selectedValues[j].data.text,
                                        type: "keyProperty",
                                        leaf: true,
                                        iconCls: 'treeKey',
                                        hidden: false,
                                        properties: properties
                                    });
                                    //newKeyNode.iconCls = 'treeKey';
                                    propertiesNode.removeChild(propertiesNode.childNodes[jj], true);
                                    break;
                                }
                            }

                            if (newKeyNode) {
                                keysNode.appendChild(newKeyNode);
                                if (keysNode.expanded == false)
                                    keysNode.expand();
                            }
                        }
                    }
                }
            }, {
                xtype: 'tbspacer',
                width: 4
            }, {
                xtype: 'tbbutton',
                icon: 'Content/img/16x16/edit-clear.png',
                text: 'Reset',
                tooltip: 'Reset to the latest applied changes',
                handler: function (f) {
                    var availItems = setItemSelectorAvailValues(node);
                    var selectedItems = setItemSelectorselectedValues(node);
                    if (keysItemSelector.fromMultiselect.store.data) {
                        keysItemSelector.fromMultiselect.reset();
                        keysItemSelector.fromMultiselect.store.removeAll();
                    }

                    keysItemSelector.fromMultiselect.store.loadData(availItems);
                    keysItemSelector.fromMultiselect.store.commitChanges();

                    if (keysItemSelector.toMultiselect.store.data) {
                        keysItemSelector.toMultiselect.reset();
                        keysItemSelector.toMultiselect.store.removeAll();
                    }

                    keysItemSelector.toMultiselect.store.loadData(selectedItems);
                    keysItemSelector.toMultiselect.store.commitChanges();
                }
            }]
        });
        editPane.add(keysSelectorPanel);
        var panelIndex = editPane.items.indexOf(keysSelectorPanel);
        editPane.getLayout().setActiveItem(panelIndex);
    }
});

