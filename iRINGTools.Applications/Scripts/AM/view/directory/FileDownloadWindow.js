/*
 * File: Scripts/AM/view/directory/FileDownloadWindow.js
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

Ext.define('AM.view.directory.FileDownloadWindow', {
  extend: 'Ext.window.Window',
  alias: 'widget.filedownloadwindow',

  requires: [
    'AM.view.directory.DownloadGrid'
  ],

  height: 342,
  width: 843,
  layout: {
    type: 'fit'
  },
  title: 'Download File',

  initComponent: function() {
    var me = this;

    Ext.applyIf(me, {
      items: [
        {
          xtype: 'downloadgrid'
        }
      ],
      dockedItems: [
        {
          xtype: 'toolbar',
          dock: 'bottom',
          layout: {
            pack: 'end',
            type: 'hbox'
          },
          items: [
            {
              xtype: 'button',
              handler: function(button, event) {
                button.up().up().close();
              },
              text: 'Cancel'
            }
          ]
        }
      ]
    });

    me.callParent(arguments);
  }

});