using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Modules;
using org.iringtools.library;

namespace org.ids_adi.iring.referenceData
{
    public class ReferenceDataModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ReferenceDataSettings>().ToSelf().InSingletonScope();
        }
    }
}