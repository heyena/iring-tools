﻿using System;
namespace org.iringtools.utility
{
  public interface IWebHttpClient
  {
    T Get<T>(string relativeUri, bool useDataContractSerializer);
    T Get<T>(string relativeUri);
    string GetMessage(string relativeUri);
    string Post<T>(string relativeUri, T requestEntity, bool useDataContractSerializer);
    R Post<T, R>(string relativeUri, T requestEntity, bool useDataContractSerializer);
    R Post<T, R>(string relativeUri, T requestEntity);
    T PostMessage<T>(string relativeUri, string requestMessage, bool useDataContractSerializer);
    void PostMultipartMessage(string relativeUri, System.Collections.Generic.List<MultiPartMessage> requestMessages, ref string response);
    string PutJson(string relativeUri, string requestMessage);
    string PostJson(string relativeUri, string requestMessage);
    string GetJson(string relativeUri);

  }
}
