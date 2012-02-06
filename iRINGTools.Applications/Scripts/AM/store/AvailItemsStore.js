﻿Ext.define('AM.store.ProviderStore', {
    extend: 'Ext.data.Store',
    model: 'AM.model.AvailItemsModel',
    autoLoad: true,
    autoDestroy: true,

    proxy: {
        type: 'ajax',
        timeout: 600000,
        url: 'NHibernate/DBProviders',
        reader: {
            type: 'json',
            root: 'items'//,
            //            idProperty: 'Provider',
            //            successProperty: 'success'
        }
    }
});