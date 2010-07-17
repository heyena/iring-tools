package org.iringtools.adapter;

import java.util.Properties;
import org.iringtools.adapter.library.DiffEngine;
import org.iringtools.adapter.library.DtoDiffEngine;
import org.iringtools.adapter.library.RdfDiffEngine;
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
