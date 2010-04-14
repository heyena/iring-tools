using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.adapter.semantic
{
  public interface ISemanticEngine
  {
    List<string> GetIdentifiersFromTripleStore(string graphName);

    void RefreshQuery(DataTransferObject dto);

    void RefreshDelete(string graphName, string identifier);

    void DumpStoreData(string xmlPath);

    void ClearStore();

  }
}
