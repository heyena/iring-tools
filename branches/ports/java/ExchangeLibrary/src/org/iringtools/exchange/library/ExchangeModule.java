package org.iringtools.exchange.library;

import java.util.Properties;
import com.google.inject.Binder;
import com.google.inject.Module;
import com.google.inject.name.Names;

public class ExchangeModule implements Module
{
  private Properties properties;
  
  public ExchangeModule(Properties properties)
  {
    this.properties = properties;
  }
  
  @Override
  public void configure(Binder binder)
  {   
    Names.bindProperties(binder, properties);
    binder.bind(DiffEngine.class).annotatedWith(Names.named("rdf")).to(RdfDiffEngine.class);
    binder.bind(DiffEngine.class).annotatedWith(Names.named("dto")).to(DtoDiffEngine.class); 
  }
}
