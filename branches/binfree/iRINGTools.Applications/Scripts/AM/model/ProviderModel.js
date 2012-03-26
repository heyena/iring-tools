Ext.define('AM.model.ProviderModel', {
    extend: 'Ext.data.Model',
    fields: ['Provider'],
    proxy: {
      type: 'ajax',
      timeout: 600000,
      url: 'NHibernate/DBProviders',
      extraParams: {
        baseUrl: null
      },
      reader: {
          type: 'json',
          root: 'items'
      }
  }
});