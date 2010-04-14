using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.iringtools.adapter.projection
{
  public interface IProjectionEngine
  {
    List<string> GetIdentifiers(string graphName);
    //DataTransferObject Get(string graphName, string identifier);
    List<DataTransferObject> GetList(string graphName);
    void Post(DataTransferObject dto);
    void PostList(List<DataTransferObject> dtos);
    void Delete(string graphName, string identifier);
    void DeleteList(string graphName, List<string> identifiers);
    void DeleteAll();
    void Initialize();
    void PersistGraphToStore(string graphName);
    //RDF GetRDF();
  }
}
