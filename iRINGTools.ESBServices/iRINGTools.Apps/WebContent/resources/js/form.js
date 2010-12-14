/*
 * @File Name : form.js
 * @Path : resources/js
 * @Using Lib : Ext JS Library 3.2.1(lib/ext-3.2.1)
 *
 * This file intended to generate the forms for editing previous content or generate new one
 * It contains different functions those can be called to display form
 */

function showCentralEditForms(node) {
  /* 01. Start The Edit Form Component */

  // 01. Edit Form
        
        var obj = node.attributes
        var properties = node.attributes.items
        var nId = obj['id']

        if ('children' in obj != true) { // restrict to generate form those have children

                var list_items = '{'
                     +'xtype:"hidden",'//<--hidden field
                     +'name:"nodeID",' //name of the field sent to the server
                     +'value:"'+obj['id']+'"' //value of the field
                     +'},'
                     +'{'
                     +'xtype:"hidden",'//<--hidden field
                     +'name:"parentNodeID",' //parent node id
                     +'value:"'+node.parentNode.id+'"' //value of the field
                     +'}';

                /*
                 * Generate the fields items dynamically
                 */
                for ( var i = 0; i < properties.length; i++) {

                    var fname=properties[i].name
                    var xtype=''
                    var vtype=''
                    switch(fname){
                        case "Description":
                            xtype= 'xtype : "textarea"'
                            break;
                         case "Read Only" :
                         case "Writable":
                             xtype= 'xtype : "combo",triggerAction: "all", mode: "local", store: ["true","false"],  displayField:"'+properties[i].value+'", width: 120'
                             
                             break;
                         default:
                            xtype= 'xtype : "textfield"'
                            vtype= 'vtype : "uniquename",' // custom validation
                    }

                     list_items = list_items+',{'+xtype+',' +vtype + 'fieldLabel:"' + properties[i].name
                     + '",name:"'
                     + properties[i].name
                     + '",allowBlank:false, blankText:"This Field is required !", value:"'
                     +properties[i].value+'"}'

               }

                list_items = eval('[' + list_items + ']')

                // generate form for editing purpose
                var edit_form = new Ext.FormPanel({
                        labelWidth : 100, // label settings here cascade unless
                        url:'postFederation',
                        method: 'POST',
                        border : false, // removing the border of the form
                        id : 'frmEdit' + nId,
                        frame : true,
                        //autoHeight: true,
                        layout:'fit',
                        closable : true,
                        defaults : {
                          width : 230
                          //msgTarget: 'under'
                        },
                        //defaultType : 'textfield',
                        items : list_items,     // binding with the fields list
                        buttonAlign : 'left', // buttons aligned to the left
                        buttons : [ {
                                text : 'Save',
                                handler: function(){
                                    edit_form.getForm().submit({
                                        success: function(f,a){
                                            Ext.Msg.alert('Success', 'It worked');
                                        },
                                        failure: function(f,a){
                                            Ext.Msg.alert('Warning', 'Error');
                                        }
                                    });
                                }
                        }, {
                                text: 'Reset',
                                handler: function(){
                                    edit_form.getForm().reset();
                                }
                        } ],
                       autoDestroy:false,
                       listeners: {
                        close: function(){
                            // check number of tabs in panel to make disabled the centerPanel if its the last tab has been closed.
                            if((Ext.getCmp('centerPanel').items.length) ==1){
                                Ext.getCmp('centerPanel').disable();                                
                             }
                            }
                        }
                });

                // fill the data in all fields of form
                //edit_form.getForm().setValues(node.attributes.properties);

                // display the form in the center panel as a tab
                Ext.getCmp('centerPanel').enable();
                Ext.getCmp('centerPanel').add(Ext.apply(edit_form, {
                        id : 'tab-' + obj['id'],
                         deferredRender: false,
                        title : node.parentNode.text + ' : ' + obj['text'],
                        closable : true,
                        layout:'form'
                })).show();
        }// end of if
}


function addNewForm(node) {
  /* 01. Start The Add New Form Component */

  // 01. Add New Form
}