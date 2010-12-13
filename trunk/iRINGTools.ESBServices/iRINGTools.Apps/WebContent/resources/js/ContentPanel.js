Ext.ns('FederationManager');
/**
* @class FederationManager.ContentPanel
* @extends Panel
* @author by Ritu Garg
*/
FederationManager.ContentPanel = Ext.extend(Ext.Panel, {
	title: 'ContentPanel',
	layout: 'card',
	activeItem: 0,
        //deferredRender: false,
	dataForm: null,
        configData:null,
	url: null,

  /**
  * initComponent
  * @protected
  */
  initComponent: function () {

  	this.addEvents({

      close:true
      //next: true,
      //prev: true
    });
  	
      this.dataForm = new Ext.FormPanel({
      labelWidth : 100, // label settings here cascade unless
                        url:this.url,
                        method: 'POST',
                        border : false, // removing the border of the form
                        //id : 'frmEdit' + nId,
                        frame : true,
                        //autoHeight: true,
                        layout:'fit',
                        closable : true,
                        defaults : {
                          width : 230
                          //msgTarget: 'under'
                        },
                        //defaultType : 'textfield',
                        items : this.configData,     // binding with the fields list
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

  	this.items = [
  		this.dataForm
  	];

    // super
    FederationManager.ContentPanel.superclass.initComponent.call(this);

  }

});