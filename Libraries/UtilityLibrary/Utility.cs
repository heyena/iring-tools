﻿// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
// All rights reserved.
//------------------------------------------------------------------------------
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the ids-adi.org nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//------------------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY ids-adi.org ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL ids-adi.org BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Xml.Linq;
using System.Linq;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Ionic.Zip;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;


namespace org.iringtools.utility
{
  public class StringEncoder : StringWriter
  {
    Encoding encoding;

    public StringEncoder(StringBuilder builder, Encoding encoding)
      : base(builder)
    {
      this.encoding = encoding;
    }

    public StringEncoder(StringBuilder builder)
      : base(builder)
    {}

    public override Encoding Encoding
    {
      get { return encoding; }
    }
  }

  public static class Utility
  {
    public static bool isLdapConfigured = false;

    public static R Transform<T, R>(object graph, string stylesheetUri)
    {
      return Transform<T, R>((T)graph, stylesheetUri, null, true);
    }

    public static R Transform<T, R>(object graph, string stylesheetUri, bool useDataContractSerializer)
    {
      return Transform<T, R>((T)graph, stylesheetUri, null, useDataContractSerializer);
    }

    public static R Transform<T, R>(T graph, string stylesheetUri)
    {
      return Transform<T, R>(graph, stylesheetUri, null, true);
    }

    public static R Transform<T, R>(T graph, Stream stylesheet)
    {
      return Transform<T, R>(graph, stylesheet, null, true);
    }

    public static R Transform<T, R>(T graph, string stylesheetUri, bool useDataContractSerializer)
    {
      return Transform<T, R>(graph, stylesheetUri, null, useDataContractSerializer);
    }

    public static R Transform<T, R>(T graph, Stream stylesheet, bool useDataContractSerializer)
    {
      return Transform<T, R>(graph, stylesheet, null,  useDataContractSerializer);
    }

    public static R Transform<T, R>(T graph, string stylesheetUri, XsltArgumentList arguments)
    {
      return Transform<T, R>(graph, stylesheetUri, arguments, true);
    }

    public static R Transform<T, R>(T graph, Stream stylesheet, XsltArgumentList arguments)
    {
      return Transform<T, R>(graph, stylesheet, arguments, true);
    }

    public static R Transform<T, R>(T graph, string stylesheetUri, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      FileStream stream;

      try
      {
        stream = new FileStream(stylesheetUri, FileMode.Open);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while loading stylesheet " + stylesheetUri + ".", exception);
      }

      return Transform<T, R>(graph, stream, arguments, useDataContractSerializer);
    }

    public static R Transform<T, R>(T graph, string stylesheetUri, XsltArgumentList arguments, bool useDataContractSerializer, bool useDataContractDeserializer)
    {
      FileStream stream;

      try
      {
        stream = new FileStream(stylesheetUri, FileMode.Open);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while loading stylesheet " + stylesheetUri + ".", exception);
      }

      return Transform<T, R>(graph, stream, arguments, useDataContractSerializer, useDataContractDeserializer);
    }

    public static R Transform<T, R>(T graph, Stream stylesheet, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      return Transform<T, R>(graph, stylesheet, arguments, useDataContractSerializer, useDataContractSerializer);
    }

    public static R Transform<T, R>(T graph, Stream stylesheet, XsltArgumentList arguments, bool useDataContractSerializer, bool useDataContractDeserializer)
    {
      string xml;
      try
      {
        xml = Serialize<T>(graph, useDataContractSerializer);

        xml = Transform(xml, stylesheet, arguments);

        R resultGraph = Deserialize<R>(xml, useDataContractDeserializer);

        return resultGraph;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while transforming " + typeof(T).Name + " to " + typeof(R).Name + ".", exception);

      }
    }

    public static Stream Transform<T>(T graph, string stylesheetUri)
    {
      return Transform<T>(graph, stylesheetUri, null, true);
    }

    public static Stream Transform<T>(T graph, string stylesheetUri, bool useDataContractSerializer)
    {
      return Transform<T>(graph, stylesheetUri, null, useDataContractSerializer);
    }

    public static Stream Transform<T>(T graph, string stylesheetUri, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      FileStream stream;

      try
      {
        stream = new FileStream(stylesheetUri, FileMode.Open);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while loading stylesheet " + stylesheetUri + ".", exception);
      }

      return Transform<T>(graph, stream, arguments, useDataContractSerializer);
    }

    public static Stream Transform<T>(T graph, Stream stylesheet, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      string xml;
      try
      {
        xml = Serialize<T>(graph, useDataContractSerializer);

        xml = Transform(xml, stylesheet, arguments);

        Stream resultGraph = DeserializeToStream(xml);

        return resultGraph;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while transforming " + typeof(T).Name + " to stream.", exception);

      }
    }

    public static R Transform<R>(Stream graph, string stylesheetUri)
    {
      return Transform<R>(graph, stylesheetUri, null, true);
    }

    public static R Transform<R>(Stream graph, Stream stylesheet)
    {
      return Transform<R>(graph, stylesheet, null, true);
    }

    public static R Transform<R>(Stream graph, string stylesheetUri, bool useDataContractSerializer)
    {
      return Transform<R>(graph, stylesheetUri, null, useDataContractSerializer);
    }

    public static R Transform<R>(Stream graph, Stream stylesheet, bool useDataContractSerializer)
    {
      return Transform<R>(graph, stylesheet, null, useDataContractSerializer);
    }

    public static R Transform<R>(Stream graph, string stylesheetUri, XsltArgumentList arguments)
    {
      return Transform<R>(graph, stylesheetUri, arguments, true);
    }

    public static R Transform<R>(Stream graph, Stream stylesheet, XsltArgumentList arguments)
    {
      return Transform<R>(graph, stylesheet, arguments, true);
    }

    public static R Transform<R>(Stream graph, string stylesheetUri, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      FileStream stream;

      try
      {
        stream = new FileStream(stylesheetUri, FileMode.Open);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while loading stylesheet " + stylesheetUri + ".", exception);
      }

      return Transform<R>(graph, stream, arguments, useDataContractSerializer);
    }

    public static R Transform<R>(Stream graph, Stream stylesheet, XsltArgumentList arguments, bool useDataContractSerializer)
    {
      string xml;
      try
      {
        xml = SerializeFromStream(graph);

        xml = Transform(xml, stylesheet, arguments);

        R resultGraph = Deserialize<R>(xml, useDataContractSerializer);

        return resultGraph;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while transforming " + typeof(Stream) + " to " + typeof(R).Name + ".", exception);

      }
    }

    public static XElement Transform(XElement xml, string stylesheetUri, XsltArgumentList arguments)
    {
      FileStream stream;
      try
      {
        stream = new FileStream(stylesheetUri, FileMode.Open);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while loading stylesheet " + stylesheetUri + ".", exception);
      }
      return Transform(xml, stream, arguments);
    }

    public static string Transform(string xml, string stylesheetUri, XsltArgumentList arguments)
    {
      FileStream stream;
      try
      {
        stream = new FileStream(stylesheetUri, FileMode.Open);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while loading stylesheet " + stylesheetUri + ".", exception);
      }
      return Transform(xml, stream, arguments);
    }

    private static string Transform(string xml, Stream stylesheet, XsltArgumentList arguments)
    {
      StringReader reader = null;
      TextWriter writer = null;
      try
      {
        XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
        XsltSettings xsltSettings = new XsltSettings();
        xsltSettings.EnableDocumentFunction = true;

        XmlUrlResolver stylesheetResolver = new XmlUrlResolver();

        XmlReader stylesheetReader = XmlReader.Create(stylesheet);
        xslCompiledTransform.Load(stylesheetReader, xsltSettings, stylesheetResolver);
        Encoding encoding = xslCompiledTransform.OutputSettings.Encoding;
        
        reader = new StringReader(xml);
        XPathDocument input = new XPathDocument(reader);

        StringBuilder builder = new StringBuilder();
        writer = new StringEncoder(builder, encoding);

        xslCompiledTransform.Transform(input, arguments, writer);

        xml = builder.ToString();
        
        return xml;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while transforming. " + exception);
      }
      finally
      {
        stylesheet.Close();
        reader.Close();
        writer.Close();
      }
    }

    private static XElement Transform(XElement sourceXml, Stream stylesheet, XsltArgumentList arguments)
    {
      XDocument resultXml = new XDocument();
      
      try
      {
        XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
        XsltSettings xsltSettings = new XsltSettings();
        xsltSettings.EnableDocumentFunction = true;

        XmlUrlResolver stylesheetResolver = new XmlUrlResolver();

        XmlReader stylesheetReader = XmlReader.Create(stylesheet);
        xslCompiledTransform.Load(stylesheetReader, xsltSettings, stylesheetResolver);
        Encoding encoding = xslCompiledTransform.OutputSettings.Encoding;

        using (XmlWriter writer = resultXml.CreateWriter())
        {
          xslCompiledTransform.Transform(sourceXml.CreateReader(), arguments, writer);
        }
        return resultXml.Element(resultXml.Root.Name);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while transforming. " + exception);
      }
      finally
      {
        stylesheet.Close();
      }
    }

    public static void Write<T>(T graph, string path)
    {
      Write<T>(graph, path, true);
    }

    public static void WriteJson<T>(T graph, string path, bool useDataContractSerializer)
    {
        try
        {
            string json = String.Empty;
            if (useDataContractSerializer)
            {
                json = SerializeJson<T>(graph, true);
            }
            else
            {
                json = SerializeJson<T>(graph, false);
            }
            WriteString(json, path);
        }
        catch (Exception exception)
        {
            throw new Exception("Error writing [" + typeof(T).Name + "] to " + path + ".", exception);
        }

    }

    public static void Write<T>(T graph, string path, bool useDataContractSerialize)
    {
      Write<T>(graph, path, useDataContractSerialize, null);
    }

    public static void Write<T>(T graph, string path, bool useDataContractSerializer, XmlSerializerNamespaces namespaces)
    {
      FileStream stream = null;
      XmlDictionaryWriter writer = null;
      try
      {
          if (isLdapConfigured && ConfigurationRepository.configTypes.Contains(typeof(T)))
          {
              bool result = false;
              Stream binaryStream = Utility.SerializeToBinaryStream<T>(graph);
              result = ConfigurationRepository.SaveFile<T>(binaryStream, path);
              if (result) 
                  return;
          }

          stream = new FileStream(path, FileMode.Create, FileAccess.Write);
          writer = XmlDictionaryWriter.CreateTextWriter(stream);


          if (useDataContractSerializer)
          {
              DataContractSerializer serializer = new DataContractSerializer(typeof(T));
              serializer.WriteObject(writer, graph);
          }
          else
          {
              XmlSerializer serializer = new XmlSerializer(typeof(T));
              serializer.Serialize(writer, graph, namespaces);
          }

      }
      catch (Exception exception)
      {
          throw new Exception("Error writing [" + typeof(T).Name + "] to " + path + ".", exception);
      }
      finally
      {
        if (writer != null)
          writer.Close();
        if (stream != null)
          stream.Close();    
      }
    }

    public static MemoryStream Write<T>(T graph, bool useDataContractSerializer)
    {
      try
      {
        MemoryStream stream = new MemoryStream();
        XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);


        if (useDataContractSerializer)
        {
          DataContractSerializer serializer = new DataContractSerializer(typeof(T));
          serializer.WriteObject(writer, graph);
        }
        else
        {
          XmlSerializer serializer = new XmlSerializer(typeof(T));
          serializer.Serialize(writer, graph);
        }

        //writer.Close();
        stream.Seek(0, SeekOrigin.Begin);
        return stream;
      }
      catch (Exception exception)
      {
        throw new Exception("Error writing [" + typeof(T).Name + "] to memory stream.", exception);
      }
    }

    public static void WriteBytes(byte[] data, string path)
    {
      FileStream stream = null;
      try
      {
        stream = new FileStream(path, FileMode.Create, FileAccess.Write);

        stream.Write(data, 0, data.Length);
      }
      catch (Exception exception)
      {
        throw new Exception("Error writing bytes to [" + path + "].", exception);
      }
      finally
      {
        stream.Close();
      }
    }

    public static void WriteStream(Stream graph, string path)
    {
      try
      {
        byte[] data = ((MemoryStream)graph).ToArray();

        WriteBytes(data, path);
      }
      catch (Exception exception)
      {
        throw new Exception("Error writing stream to [" + path + "].", exception);
      }
    }

    public static T Read<T>(string path)
    {
      return Read<T>(path, true);
    }

    public static T ReadJson<T>(string path, bool useDataContractSerializer)
    {
        T graph;
        MemoryStream stream = new MemoryStream();
        try
        {
            if (useDataContractSerializer)
            {
                stream = ReadStream(path);
                graph = (T)DeserializeFromStreamJson<T>(stream, true);
            }
            else
            {
                stream = ReadStream(path);
                graph = (T)DeserializeFromStreamJson<T>(stream, false);
            }

            return graph;
        }
        catch (Exception exception)
        {
            throw new Exception("Error reading [" + typeof(T).Name + "] from [" + path + "].", exception);
        }
        finally
        {
            stream.Close();
        }
    }

    public static T Read<T>(string path, bool useDataContractSerializer)
    {
      T graph;
      FileStream stream = null;
      XmlDictionaryReader reader = null;

      try
      {
          if (isLdapConfigured && ConfigurationRepository.configTypes.Contains(typeof(T)))
          {
              Stream binaryStream = ConfigurationRepository.GetFile<T>(path);

              if (binaryStream == null && typeof(T).Name == "XElementClone")
                  return default(T);

              if (binaryStream != null) 
              {
                  graph = DeserializeBinaryStream<T>(binaryStream);
                  return graph;
              }//else check the local disk. 
          }

        stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas { MaxStringContentLength = int.MaxValue });

        if (useDataContractSerializer)
        {
          DataContractSerializer serializer = new DataContractSerializer(typeof(T));
          graph = (T)serializer.ReadObject(reader, true);
        }
        else
        {
          XmlSerializer serializer = new XmlSerializer(typeof(T));
          graph = (T)serializer.Deserialize(reader);
        }
        
        //If the file is on disk but the LDAP is configuried then move the files in LDAP consecutively.
        if (isLdapConfigured && ConfigurationRepository.configTypes.Contains(typeof(T)))
        {
            Write<T>(graph, path);
        }

        return graph;
      }
      catch (Exception exception)
      {
        throw new Exception("Error reading [" + typeof(T).Name + "] from [" + path + "].", exception);
      }
      finally
      {
        if (reader != null) reader.Close();
        if (stream != null) stream.Close();
      }
    }

    public static string ReadString(string path)
    {
      StreamReader streamReader = null;      
      try
      {
        streamReader = new StreamReader(path);
        string query = streamReader.ReadToEnd();
        streamReader.Close();
        return query;
      }
      catch (Exception exception)
      {
        throw new Exception("Error reading string from [" + path + "].", exception);
      }
      finally
      {
        if (streamReader != null) streamReader.Close();
      }
    }

    public static string ReadString(Stream stream)
    {
      StreamReader streamReader = null;
      try
      {
        streamReader = new StreamReader(stream);
        string query = streamReader.ReadToEnd();
        streamReader.Close();
        return query;
      }
      catch (Exception exception)
      {
        throw new Exception("Error reading string from stream.", exception);
      }
      finally
      {
        if (streamReader != null) streamReader.Close();
      }
    }

    public static XElement ReadXml(string path)
    {
      try
      {
        XDocument document = XDocument.Load(path);
        return document.Element(document.Root.Name);
      }
      catch (Exception exception)
      {
        throw new Exception("Error reading XML from [" + path + "].", exception);
      }
    }

    public static MemoryStream ReadStream(string path)
    {
      MemoryStream stream = null;
      FileStream fileStream = null;

      try
      {
        fileStream = new FileStream(path, FileMode.Open);
        stream = fileStream.ToMemoryStream();
      }
      catch (Exception exception)
      {
        throw new Exception("Error reading stream from [" + path + "].", exception);
      }
      finally
      {
        if (fileStream != null) fileStream.Close();
      }

      return stream;
    }


    public static MemoryStream ToMemoryStream(this Stream requestStream)
    {
      MemoryStream usableStream = new MemoryStream();
      
      if (requestStream != null)
      {
        byte[] buffer = new byte[4096];
        int bytesRead = 0;

        do
        {
          bytesRead = requestStream.Read(buffer, 0, buffer.Length);
          usableStream.Write(buffer, 0, bytesRead);
        } while (bytesRead > 0);

        usableStream.Position = 0;
      }

      return usableStream;
    }

    public static MemoryStream ToMemoryStream(this string base64String)
    {
      byte[] bytes = Convert.FromBase64String(base64String);
      
      MemoryStream stream = new MemoryStream();
      stream.Write(bytes, 0, bytes.Length);

      return stream;
    }

    public static T DeserializeDataContract<T>(this string xml)
    {
      return Deserialize<T>(xml, true);
    }

    public static T DeserializeXml<T>(this string xml)
    {
      return Deserialize<T>(xml, false);
    }

    public static string SerializeXml<T>(T graph)
    {
        return Serialize<T>(graph, Encoding.UTF8, false, null);
    }

   
    public static string SerializeDataContract<T>(T graph)
    {
      return Serialize<T>(graph, Encoding.UTF8);
    }    

    public static string Serialize<T>(T graph, Encoding encoding)
    {
      return Serialize<T>(graph, encoding, true, null);
    }

    public static string Serialize<T>(T graph, bool useDataContractSerializer)
    {
      return Serialize<T>(graph, Encoding.UTF8, useDataContractSerializer, null);
    }

    public static string Serialize<T>(T graph, Encoding encoding, bool useDataContractSerializer)
    {
      return Serialize<T>(graph, Encoding.UTF8, useDataContractSerializer, null);
    }

    public static string SerializeJson<T>(T graph, bool useDataContractSerializer)
    {
        return SerializeJson<T>(graph, Encoding.UTF8, useDataContractSerializer);
    }

    public static string SerializeJson<T>(T graph, Encoding encoding, bool useDataContractSerializer)
    {
        string jsonString = String.Empty;

        try
        {
            if (!useDataContractSerializer)
            {
                //JavaScriptSerializer serializer = new JavaScriptSerializer();
                //serializer.MaxJsonLength = int.MaxValue;
                //jsonString = serializer.Serialize(graph);

                jsonString = JsonConvert.SerializeObject(graph, new IsoDateTimeConverter());
            }
            else
            {
                MemoryStream stream = new MemoryStream();
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(stream, graph);
                jsonString = Encoding.Default.GetString(stream.ToArray());
                stream.Close();
            }
            return jsonString;
        }
        catch (Exception exception)
        {
            throw new Exception("Error serializing [" + typeof(T).Name + "].", exception);
        }

    }

    public static T DeserializeBinaryStream<T>(Stream stream)
    {
        try
        {
            BinaryFormatter bFormatter = new BinaryFormatter();
            T obj = (T)bFormatter.Deserialize(stream);
            return obj;
        }
        catch (Exception exception)
        {
            throw new Exception("Error deserializing [" + typeof(T).Name + "].", exception);
        }
    }

    public static Stream SerializeToBinaryStream<T>(T obj)
    {
        try
        {
            BinaryFormatter bformatter = new BinaryFormatter();
            System.IO.Stream stream = new System.IO.MemoryStream();
            bformatter.Serialize(stream, (T)obj);

            return stream;
        }
        catch (Exception exception)
        {
            throw new Exception("Error Serializing [" + typeof(T).Name + "].", exception);
        }
    }

    public static MemoryStream SerializeToStreamJSON<T>(T graph, bool useDataContractSerializer)
    {
        return SerializeToStreamJSON<T>(graph, Encoding.UTF8, useDataContractSerializer);
    }
    
    public static MemoryStream SerializeToStreamJSON<T>(T graph, Encoding encoding, bool useDataContractSerializer)
    {
        MemoryStream stream = new MemoryStream();
        if (!useDataContractSerializer)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer{ MaxJsonLength = int.MaxValue };
            string jsonString = serializer.Serialize(graph);
            byte[] byteArray = encoding.GetBytes(jsonString);
            stream = new MemoryStream(byteArray);
        }
        else
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            serializer.WriteObject(stream, graph);
        }
        return stream;
    }

    public static string Serialize<T>(T graph, Encoding encoding, bool useDataContractSerializer, XmlSerializerNamespaces namespaces)
    {
      string xml;
      try
      { 
        StringBuilder builder = new StringBuilder();
        TextWriter encoder = new StringEncoder(builder, encoding);
        XmlWriter writer = XmlWriter.Create(encoder);

        if (useDataContractSerializer)
        {
          DataContractSerializer serializer = new DataContractSerializer(typeof(T));
          serializer.WriteObject(writer, graph);
        }
        else
        {
          XmlSerializer serializer = new XmlSerializer(typeof(T));
          if (namespaces != null && namespaces.Count > 0)
          {
            serializer.Serialize(writer, graph, namespaces);
          }
          else
          {
            serializer.Serialize(writer, graph);
          }
        }
        writer.Close();

        xml = builder.ToString();
       
        return xml;
      }
      catch (Exception exception)
      {
        throw new Exception("Error serializing [" + typeof(T).Name + "].", exception);
      }
    }

    public static XElement SerializeToXElement<T>(T graph)
    {
      try
      {
        XmlSerializer ser = new XmlSerializer(typeof(T));
        XDocument doc = new XDocument();
        
        using (XmlWriter xw = doc.CreateWriter())
        {
          ser.Serialize(xw, graph);
          xw.Close();
        }

        return doc.Root;
      }
      catch (Exception exception)
      {
        throw new Exception("Error serializing [" + typeof(T).Name + "].", exception);
      }
    }

    public static XElement ToXElement<T>(this T graph)
    {
      try
      {
        DataContractSerializer ser = new DataContractSerializer(typeof(T));
        XDocument doc = new XDocument();

        using (XmlWriter xw = doc.CreateWriter())
        {
          ser.WriteObject(xw, graph);
          xw.Close();
        }

        return doc.Root;
      }
      catch (Exception exception)
      {
        throw new Exception("Error serializing [" + typeof(T).Name + "].", exception);
      }
    }

    public static string SerializeFromStream(Stream graph)
    {
      try
      {
        StreamReader reader = new StreamReader(graph, Encoding.UTF8);
        string value = reader.ReadToEnd();
        return value;
      }
      catch (Exception exception)
      {
        throw new Exception("Error serializing stream.", exception);
      }
    }

    public static MemoryStream SerializeToMemoryStream<T>(T graph)
    {
      return SerializeToMemoryStream(graph, true);
    }

    public static MemoryStream SerializeToMemoryStream<T>(T graph, bool useDataContractSerializer)
    {
      try
      {
        MemoryStream stream = new MemoryStream();

        if (useDataContractSerializer)
        {
          DataContractSerializer serializer = new DataContractSerializer(typeof(T));
          serializer.WriteObject(stream, graph);
        }
        else
        {
          XmlSerializer serializer = new XmlSerializer(typeof(T));
          serializer.Serialize(stream, graph);
        }

        stream.Position = 0;

        return stream;
      }
      catch (Exception exception)
      {
        throw new Exception("Error serializing [" + typeof(T).Name + "] to stream.", exception);
      }
    }

    public static T Deserialize<T>(string xml, bool useDataContractSerializer)
    {
      T graph;
      try
      {
        StringReader input = new StringReader(xml);
        XmlReaderSettings settings = new XmlReaderSettings();
        settings.DtdProcessing = DtdProcessing.Ignore;
        XmlReader reader = XmlDictionaryReader.Create(input, settings);

        if (useDataContractSerializer)
        {
          DataContractSerializer serializer = new DataContractSerializer(typeof(T));

          graph = (T)serializer.ReadObject(reader, false);
        }
        else
        {
          XmlSerializer serializer = new XmlSerializer(typeof(T));
          graph = (T)serializer.Deserialize(reader);
        }
        return graph;
      }
      catch (Exception exception)
      {
        throw new Exception("Error deserializing [" + typeof(T).Name + "].", exception);
      }
    }

    public static T DeserializeJson<T>(string jsonString, bool useDataContractSerializer)
    {
        T graph = DeserializeJson<T>(jsonString, Encoding.UTF8, useDataContractSerializer);
        return graph;
    }

    public static T DeserializeFromStreamJson<T>(Stream stream, bool useDataContractSerializer)
    {
        return DeserializeFromStreamJson<T>(stream, Encoding.UTF8, useDataContractSerializer);
    }


    public static T DeserializeFromStreamJson<T>(Stream stream, Encoding encoding, bool useDataContractSerializer)
    {
        T graph;

        if (!useDataContractSerializer)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue;
            string json = ReadString(stream);
            graph = (T)serializer.Deserialize<T>(json);
        }
        else
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            graph = (T)serializer.ReadObject(stream);
        }
        return graph;
    }

    public static T DeserializeJson<T>(string jsonString, Encoding encoding, bool useDataContractSerializer)
    {
        T graph;

        try
        {
            if (!useDataContractSerializer)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = int.MaxValue;
                graph = (T)serializer.Deserialize<T>(jsonString);
            }
            else
            {                
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                byte[] byteArray = encoding.GetBytes(jsonString);
                MemoryStream stream = new MemoryStream(byteArray);
                graph = (T)serializer.ReadObject(stream);
            }
            return graph;
        }
        catch (Exception exception)
        {
            throw new Exception("Error deserializing [" + typeof(T).Name + "].", exception);
        }
    }

    public static T DeserializeFromXElement<T>(XElement element)
    {      
      try
      {
        XmlReader reader = element.CreateReader();
        XmlSerializer serializer = new XmlSerializer(typeof(T));
        return (T)serializer.Deserialize(reader);        
      }
      catch (Exception exception)
      {
        throw new Exception("Error deserializing [" + typeof(T).Name + "].", exception);
      }
    }

    public static T DeserializeFromStream<T>(Stream stream)
    {
      return DeserializeFromStream<T>(stream, true);
    }


    public static T DeserializeFromStream<T>(Stream stream, bool useDataContractSerializer)
    {
      
      T graph;
      
      try
      {
        XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
        quotas.MaxStringContentLength = int.MaxValue;
        quotas.MaxArrayLength = int.MaxValue;

        if (useDataContractSerializer)
        {
          XmlDictionaryReader reader = null;
          try
          {
            reader = XmlDictionaryReader.CreateTextReader(stream, quotas);
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            graph = (T)serializer.ReadObject(reader, true);
          }
          finally
          {
            if (reader != null) reader.Close();
          }
        }
        else
        {
          StreamReader reader = null;
          try
          {
            reader = new StreamReader(stream);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            graph = (T)serializer.Deserialize(reader);
          }
          finally
          {
            if (reader != null) reader.Close();
          }
        }        
        return graph;
      }
      catch (Exception exception)
      {
        throw new Exception("Error deserializing stream to [" + typeof(T).Name + "].", exception);
      }
    }

    public static Stream DeserializeToStream(string graph)
    {
      Stream stream;
      StreamWriter writer = null;
      try
      {
        stream = new MemoryStream();
        writer = new StreamWriter(stream);
        writer.Write(graph);
        writer.Flush();

        return stream;
      }
      catch (Exception exception)
      {
        throw new Exception("Error deserializing string to stream.", exception);
      }
      finally
      {
        writer.Close();
      }
    }

    public static string ToJson<T>(T obj)
    {
      DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
      MemoryStream ms = new MemoryStream();
      serializer.WriteObject(ms, obj);
      return Encoding.Default.GetString(ms.ToArray());
    }

    public static T FromJson<T>(string json)
    {
      MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
      DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
      T obj = (T)serializer.ReadObject(ms);
      ms.Close();
      return obj;
    }

    public static T CloneDataContractObject<T>(T obj)
    {
      string xml = SerializeDataContract<T>(obj);
      return DeserializeDataContract<T>(xml);
    }

    public static T CloneSerializableObject<T>(T obj)
    {
      string xml = Serialize<T>(obj, false);
      return Deserialize<T>(xml, false);
    }

    public static void WriteException(Exception exception, string path)
    {
      string typeName = String.Empty;
      
      StreamWriter streamWriter = new StreamWriter(path, true);
      streamWriter.WriteLine(System.DateTime.UtcNow + " (UTC) - " + exception.Source);
      streamWriter.WriteLine(exception.ToString());
      streamWriter.WriteLine();
      streamWriter.Flush();
      streamWriter.Close();
    }

    public static void WriteString(string value, string path)
    {
      WriteString(value, path, false, Encoding.UTF8);
    }

    public static void WriteString(string value, string path, bool append)
    {
      WriteString(value, path, append, Encoding.UTF8);
    }

    public static void WriteString(string value, string path, Encoding encoding)
    {
      WriteString(value, path, false, encoding);
    }

    public static void WriteString(string value, string path, bool append, Encoding encoding)
    {
      try
      {
        FileStream stream;
        if (append)
        {
          stream = new FileStream(path, FileMode.Append, FileAccess.Write);
        }
        else
        {
          stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        }
        StreamWriter writer = new StreamWriter(stream, encoding);

        writer.Write(value);
        writer.Flush();
        writer.Close();
        stream.Close();
      }
      catch (Exception exception)
      {
        throw new Exception("Error writing string to [" + path + "].", exception);
      }
    }

    public static string ShellExec(string command, string args, bool redirectStdout) 
    {
      String output = String.Empty;
      try 
      {
        Process process = new Process();
        
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = redirectStdout;
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        process.StartInfo.CreateNoWindow = false;
        process.Start();
        
        if (redirectStdout) 
        {
          output = process.StandardOutput.ReadToEnd();
        }
        
        process.WaitForExit();
      }
      catch (Exception exception) 
      {
        output = exception.ToString();
      }

      return output;
    }

    public static void ExecuteSQL(string sql, string connectionString)
    {
      using (SqlConnection connection = new SqlConnection(
                 connectionString))
      {
        SqlCommand command = new SqlCommand(sql, connection);
        command.Connection.Open();
        command.ExecuteNonQuery();
      }
    }

    public static XDocument RemoveNamespace(XDocument xdoc)
    {
      foreach (XElement e in xdoc.Root.DescendantsAndSelf())
      {
        if (e.Name.Namespace != XNamespace.None)
        {
          e.Name = XNamespace.None.GetName(e.Name.LocalName);
        }

        if (e.Attributes().Where(a => a.IsNamespaceDeclaration || a.Name.Namespace != XNamespace.None).Any())
        {
          e.ReplaceAttributes(e.Attributes().Select(a => a.IsNamespaceDeclaration ? null : a.Name.Namespace != XNamespace.None ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value) : a));
        }
      }
      return xdoc;
    }

    public static void Compile(Dictionary<string, string> compilerOptions, CompilerParameters compilerParameters, string[] sources)
    {
      try
      {
        CSharpCodeProvider codeProvider = new CSharpCodeProvider(compilerOptions);
        CompilerResults results = codeProvider.CompileAssemblyFromSource(compilerParameters, sources);

        if (results.Errors.Count > 0)
        {
          StringBuilder errors = new StringBuilder();

          foreach (CompilerError error in results.Errors)
          {
            errors.AppendLine(error.ErrorNumber + ": " + error.ErrorText);
          }

          throw new Exception(errors.ToString());
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static string GeneratedCodeProlog
    {
      get
      {
        return
  @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated.
//     Runtime Version:2.0.50727.3074
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------";
      }
    }

    public static string XsdTypeToCSharpType(string xsdType)
    {
      string type = (xsdType.ToLower().StartsWith("xsd:")) ? xsdType.Substring(4) : xsdType;

      switch (type.ToLower())
      {
        case "boolean": return "Boolean";
        case "byte": return "Byte";
        case "char": return "Char";
        case "character": return "Char";
        case "date": return "DateTime";
        case "datetime": return "DateTime";
        case "decimal": return "Decimal";
        case "double": return "Double";
        case "float": return "Single";
        case "int": return "Int32";
        case "integer": return "Int32";
        case "long": return "Int64";
        case "short": return "Int16";
        case "time": return "DateTime";
        default: return "String";
      }
    }

    public static bool ValidateValueWithXsdType(string xsdType, string value)
    {
      string type = (xsdType.ToLower().StartsWith("xsd:")) ? xsdType.Substring(4) : xsdType;

      //TODO need more improvement in datetime format handling
      switch (type.ToLower())
      {
        case "boolean": { bool temp; return Boolean.TryParse(value, out temp); }
        case "byte": { byte temp; return Byte.TryParse(value, out temp); }
        case "char": { Char temp; return Char.TryParse(value, out temp); }
        case "character": { Char temp; return Char.TryParse(value, out temp); }
        case "decimal": { Decimal temp; return Decimal.TryParse(value, out temp); }
        case "double": { Double temp; return Double.TryParse(value, out temp); }
        case "float": { Single temp; return Single.TryParse(value, out temp); }
        case "int": { Int32 temp; return Int32.TryParse(value, out temp); }
        case "integer": { Int32 temp; return Int32.TryParse(value, out temp); }
        case "long": { Int64 temp; return Int64.TryParse(value, out temp); }
        case "short": { Int16 temp; return Int16.TryParse(value, out temp); }
        case "time":
        case "date":
        case "datetime":
        case "duration":
        case "gday":
        case "gmonth":
        case "gmonthday":
        case "gyear":
        case "gyearmonth":
          { DateTime temp; return DateTime.TryParse(value, out temp); }
        case "string": 
        case "entities":
        case "entity":
        case "id":
        case "idref":
        case "idrefs":
        case "language":
        case "name":
        case "ncname":
        case "nmtoken":
        case "nmtokens":
        case "normalizedstring":
        case "qname":
        case "token":
            { return true; }
        case "anyURI":
            { return true; }
        default: { return false; }
      }
    }

    public static string SqlTypeToCSharpType(string sqlType)
    {
      switch (sqlType.ToLower())
      {
        case "bit": return "Boolean";
        case "byte": return "Byte";
        case "char": return "Char";
        case "nchar": return "String";
        case "character": return "Char";
        case "varchar": return "String";
        case "varchar2": return "String";
        case "nvarchar":return "String";
        case "nvarchar2": return "String";
        case "text": return "String";
        case "ntext": return "String";
        case "xml": return "String";
        case "date": return "Date";
        case "datetime": return "DateTime";
        case "smalldatetime": return "DateTime";
        case "time": return "DateTime";
        case "timestamp": return "DateTime";
        case "dec": return "Double";
        case "decimal": return "Decimal";
        case "money": return "Double";
        case "smallmoney": return "Double";
        case "numeric": return "Double";
        case "float": return "Single";
        case "real": return "Double";
        case "int": return "Int32";
        case "integer": return "Int32";
        case "bigint": return "Int64";
        case "smallint": return "Int16";
        case "tinyint": return "Int16";
        case "number": return "Decimal";
        case "long": return "Int64";
        case "clob": return "String";
        case "blob": return "String";
        default: return "String"; 
      }
    }

    public static string ToSafeName(string name)
    {
      return Regex.Replace(name, @"^\d*|\W", "");
    }    

    public static void SearchAndInsert<O, T>(List<O> list, O element, T Comparer)
    {
        IComparer<O> Comp = (IComparer<O>)Comparer;
        int index = list.BinarySearch(element, Comp);

        if (index < 0)
        {
            list.Insert(~index, element);
        }

    }

    public static string GetQNameFromUri(String uri)
    {
        Uri u;
        if (uri.StartsWith("http"))
        {
            u = new Uri(uri);
            if (uri.Contains("XMLSchema"))
                return "xsd:" + u.Fragment.Substring(1);
            else
                return u.Authority.Split('.')[0] + ":" + u.Fragment.Substring(1);
        }
        else
        {
            throw new Exception(uri + " is not a valid Uri");
        }
    }

    public static string GetIdFromURI(string uri)
    {
      string id = uri;

      if (!String.IsNullOrEmpty(uri))
      {
        if (id.Contains("#"))
        {
          id = id.Substring(id.LastIndexOf("#") + 1);
        }
        else if (id.Contains(":"))
        {
          id = id.Substring(id.LastIndexOf(":") + 1);
        }
      }

      if (id == null) id = String.Empty;

      return id;
    }

    public static XElement GetXElement(this XmlNode node)
    {
      XDocument xDoc = new XDocument();
      XmlWriter xmlWriter = xDoc.CreateWriter();
      node.WriteTo(xmlWriter);
      return xDoc.Root;
    }

    public static XmlNode GetXmlNode(this XElement element)
    {
      using (XmlReader xmlReader = element.CreateReader())
      {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlReader);
        return xmlDoc;
      }
    }

    public static string MD5Hash(string input)
    {
      // calculate MD5 hash from input
      MD5 md5 = MD5.Create();
      byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
      byte[] hash = md5.ComputeHash(inputBytes);

      // convert byte array to hex string
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < hash.Length; i++)
      {
        sb.Append(hash[i].ToString("x2"));
      }

      return sb.ToString();
    }

    public static string ExtractId(string qualifiedId)
    {
      if (String.IsNullOrEmpty(qualifiedId) || !qualifiedId.Contains(":"))
        return qualifiedId;

      return qualifiedId.Substring(qualifiedId.IndexOf(":") + 1);
    }

    public static string TitleCase(string value)
    {
      string returnValue = String.Empty;

      if (!String.IsNullOrEmpty(value))
      {
        string[] words = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string word in words)
        {
          returnValue += word.Substring(0, 1).ToUpper();

          if (word.Length > 1)
            returnValue += word.Substring(1).ToLower();
        }
      }

      return returnValue;
    }
    
    public static string ToXsdDateTime(DateTime dateTime)
    {
      #region DO NOT DELETE - WILL RETROFIT IT LATER
      //DateTimeOffset dtOffset =
      //  new DateTimeOffset(dateTime, TimeZoneInfo.Local.GetUtcOffset(dateTime));
      //string dtOffsetStr = dtOffset.ToString("o");

      //if (dtOffsetStr[dtOffsetStr.Length - 3] == ':')
      //  dtOffsetStr = dtOffsetStr.Remove(dtOffsetStr.Length - 3, 1);

      //return dtOffsetStr;
      #endregion

      // We should not use XmlDateTimeSerializationMode.Utc as this implies we know that the source was a local time to begin with
      // For example if my source data has two columns, EventTime and EventTimeUTC that store an event time with local offset and UCT respectively 
      // then converting the UTC column will apply the server's TZone offset to that data thus corrupting it.
      //
      // Only the original application (Datalayer for that application) could know if a datetime column stores local or UTC information and would be able to make the determination
      // that the timezone should (or should not) be preserved when data is moved to servers in different time zones.
      //
      // so replace: string utcStr = XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Utc); 
      // with:
      //string utcStr = XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Unspecified);

      // temporary restoring the old format to support POMA
      string utcStr = XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Utc);

      if (!utcStr.Contains("."))
        utcStr = utcStr.Replace("Z", ".000-00:00");
      else
        utcStr = utcStr.Replace("Z", "-00:00");

      return utcStr;
    }

    public static string ToXsdDate(DateTime date)
    {
        string utcStr = XmlConvert.ToString(date, XmlDateTimeSerializationMode.Utc);

        if (utcStr.Contains("T"))
            utcStr = utcStr.Substring(0, utcStr.IndexOf("T"));

        return utcStr;
    }

    public static string ToXsdDateTime(string dateTime)
    {
        if (String.IsNullOrEmpty(dateTime))
            return dateTime;

      DateTime dt;
      if (dateTime.Contains("T"))
      {
          dt = DateTime.ParseExact(dateTime,"yyyy-MM-dd'T'HH:mm:ss.000-00:00",CultureInfo.InvariantCulture,
                                             DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
      }
      else
      {
          dt = DateTime.Parse(dateTime);
      }

      return ToXsdDateTime(dt); 
    }

    public static string ToXsdDate(string date)
    {
        if (String.IsNullOrEmpty(date))
            return date;

        DateTime dt = DateTime.Parse(date);
        return ToXsdDate(dt);
    }

    public static DateTime FromXsdDateTime(string dateTime)
    {
        DateTime dt;
        
        try
        {
            dt = DateTime.ParseExact(dateTime, "yyyy-MM-ddTHH:mm:ss.fff-00:00", CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            dt = DateTime.ParseExact(dateTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        return dt;
    }

    public static string GetMimeType(string fileName)
    {
      string mimeType = "application/unknown";
      string ext = System.IO.Path.GetExtension(fileName).ToLower();
      Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
      if (regKey != null && regKey.GetValue("Content Type") != null)
        mimeType = regKey.GetValue("Content Type").ToString();
      return mimeType;
    }

    public static string EncodeTo64(string toEncode)
    {
      if (!String.IsNullOrEmpty(toEncode))
      {
        byte[] toEncodeAsBytes = ASCIIEncoding.ASCII.GetBytes(toEncode);
        return Convert.ToBase64String(toEncodeAsBytes);
      }

      return toEncode;
    }

    public static string DecodeFrom64(string encodedData)
    {
      if (!String.IsNullOrEmpty(encodedData))
      {
        byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
        return ASCIIEncoding.ASCII.GetString(encodedDataAsBytes);
      }

      return encodedData;
    }

    private static string GetMapUri(Properties uriMaps, string uri)
    {
      if (!String.IsNullOrEmpty(uri) && uriMaps != null)
      {
        foreach (string key in uriMaps.Keys)
        {
          if (key.ToLower() == uri.ToLower())
          {
            return uriMaps[key];
          }
        }
      }

      return uri;
    }

    public static string FormAppBaseURI(Properties uriMaps, string baseUri, string app)
    {
      string appBaseUri = String.Empty;

      baseUri = GetMapUri(uriMaps, baseUri);
      appBaseUri = String.Format("{0}{1}", baseUri, HttpUtility.UrlEncode(app));
      
      return appBaseUri;
    }

    public static string FormEndpointBaseURI(Properties uriMaps, string baseUri, string project, string app)
    {
      const string DEFAULT_PROJECT = "all";
      string endpointBaseUri = String.Empty;

      if (project.ToLower() == DEFAULT_PROJECT)
      {
        endpointBaseUri = String.Format("{0}{1}/{2}/", baseUri, DEFAULT_PROJECT, HttpUtility.UrlEncode(app));
        endpointBaseUri = GetMapUri(uriMaps, endpointBaseUri);
      }
      else
      {
        baseUri = GetMapUri(uriMaps, baseUri);
        endpointBaseUri = String.Format("{0}{1}/{2}/", baseUri, HttpUtility.UrlEncode(app), HttpUtility.UrlEncode(project));
      }

      return endpointBaseUri;
    }

    //Converting special character names to character
    public static string ConvertSpecialCharInbound(string identifier, string[] arrSpecialcharlist, string[] arrSpecialcharValue)
    {
        if (arrSpecialcharlist != null && arrSpecialcharValue != null)
        {
            foreach (string str in arrSpecialcharValue)
            {
                if (identifier.Contains(str))
                {
                    identifier = identifier.Replace(str, arrSpecialcharlist[Array.IndexOf(arrSpecialcharValue, str)]);
                }
            }
        }
        return identifier;
    }

    //Converting special character  to character names
    public static string ConvertSpecialCharOutbound(string identifier, string[] arrSpecialcharlist, string[] arrSpecialcharValue)
    {
        if (arrSpecialcharlist != null && arrSpecialcharValue != null)
        {
            foreach (string str in arrSpecialcharlist) 
            {
                if (identifier.Contains(str))
                {
                    identifier = identifier.Replace(str, arrSpecialcharValue[Array.IndexOf(arrSpecialcharlist, str)]);
                }
            }
        }
        return identifier;
    }

    public static MemoryStream Zip(string directory)
    {
      MemoryStream stream = new MemoryStream();

      using (ZipFile zip = new ZipFile())
      {
        zip.AddDirectory(directory, string.Empty);
        zip.Save(stream);
      }

      stream.Position = 0;

      return stream;
    }

    public static void Unzip(Stream zipStream, string targetDirectory)
    {
      zipStream.Position = 0;

      using (ZipFile zip = ZipFile.Read(zipStream))
      {
        zip.ExtractAll(targetDirectory, ExtractExistingFileAction.OverwriteSilently);
        zip.Dispose();
      }
    }

    public static void Unzip(string zipFile, string targetDirectory)
    {
      using (ZipFile zip = ZipFile.Read(zipFile))
      {
        zip.ExtractAll(targetDirectory);
        zip.Dispose();
      }
    }

    public static byte[] GetBytes(string file)
    {
      FileStream fs = File.OpenRead(file);

      try
      {
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, (int)fs.Length);
        fs.Close();
        return bytes;
      }
      finally
      {
        fs.Close();
      }
    }

    public static object Evaluate(string expression)
    {
      try
      {
        string source = string.Format(@"
namespace org.iringtools.dynamic
{{
  public class Evaluator
  {{
    public object Evaluate()
    {{
      return {0};
    }}
  }}
}}", expression);

        CodeDomProvider provider = new CSharpCodeProvider();
        CompilerParameters parameters = new CompilerParameters();

        CompilerResults compiler = provider.CompileAssemblyFromSource(parameters, source);
        if (compiler.Errors.Count > 0)
        {
          return null;
        }

        Assembly assembly = compiler.CompiledAssembly;
        object instance = assembly.CreateInstance("org.iringtools.dynamic.Evaluator");

        Type type = instance.GetType();
        MethodInfo method = type.GetMethod("Evaluate");

        return method.Invoke(instance, null);
      }
      catch
      {
        return null;
      }
    }

    public static bool FileExistInRepository<T>(string path)
    {
        try
        {
            bool result = false;
            if (isLdapConfigured && ConfigurationRepository.configTypes.Contains(typeof(T)))
            {
                result = ConfigurationRepository.FileExists(path);
            }
            return result;
        }
        catch (Exception exception)
        {
            throw new Exception("Error in verifying LDAP repository for [" + typeof(T).Name + "].", exception);
        }
    }

    public static void DeleteFileFromRepository<T>(string path)
    {
        try
        {
            if (isLdapConfigured && ConfigurationRepository.configTypes.Contains(typeof(T)))
            {
                ConfigurationRepository.DeleteEntry(path);
            }
        }
        catch (Exception exception)
        {
            throw new Exception("Error in verifying LDAP repository for [" + typeof(T).Name + "].", exception);
        }
    }

      /// <summary>
      /// It will save an xElement object to a file on the specified path.
      /// </summary>
    public static void SavexElementObject(XElement xElement, string path)
    {
        if (isLdapConfigured && ConfigurationRepository.configTypes.Contains(typeof(XElementClone)))
        {
            XElementClone objSerializedXElement = GetxElementCloneObject(xElement, path);
            Write(objSerializedXElement, path);
            return;
        }
        xElement.Save(path);
    }

    public static XElement GetxElementObject(string path)
    {
        XElement xelement;
        if (isLdapConfigured && ConfigurationRepository.configTypes.Contains(typeof(XElementClone)))
        {
            XElementClone objSerializedXElement = Read<XElementClone>(path);
            if (objSerializedXElement != null)
            {
                xelement = GetxElementFromCloneObject(objSerializedXElement);
                return xelement;
            }
        }

        xelement = XElement.Load(path);
        //If the file is on disk but the LDAP is configuried then move the files in LDAP consecutively.
        if (xelement != null && isLdapConfigured && ConfigurationRepository.configTypes.Contains(typeof(XElementClone)))
        {
            SavexElementObject(xelement, path);
        }
        return xelement;
    }

    /// <summary>
    /// Since xElement is non-serializable so we have to clone it in other class.
    /// </summary>
    public static XElementClone GetxElementCloneObject(XElement xelement, string fileName) 
    {
             XElementClone objxElementAlias = new XElementClone();
             objxElementAlias.fileName = fileName;
             objxElementAlias.data = xelement;
             objxElementAlias.xElementContent = objxElementAlias.data.ToString();
             return objxElementAlias;		 
    }

    public static XElement GetxElementFromCloneObject(XElementClone xelementAlias)
    {
        xelementAlias.data = XElement.Parse(xelementAlias.xElementContent);
        return xelementAlias.data;
    }

    public static void InitializeConfigurationRepository(Type[] types)
    {
        if (ConfigurationRepository.configTypes.Count == 0)
        {
            ConfigurationRepository.GetAppSettings();
            ConfigurationRepository.configTypes.AddRange(types);
        }
    }

    public static bool IsBase64Encoded(string text)
    {
      if (string.IsNullOrWhiteSpace(text))
        return false;

      string pattern = "^([A-Za-z0-9+/]{4})*([A-Za-z0-9+/]{4}|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{2}==)$";
      return Regex.IsMatch(text, pattern);
    }
  }
}
