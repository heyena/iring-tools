// Copyright (c) 2009, ids-adi.org /////////////////////////////////////////////
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
using System.Xml.Serialization;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;


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
      return Serialize<T>(graph, Encoding.UTF8, false);
    }

    public static string SerializeDataContract<T>(T graph)
    {
      return Serialize<T>(graph, Encoding.UTF8);
    }    

    public static string Serialize<T>(T graph, Encoding encoding)
    {
      return Serialize<T>(graph, encoding, true);
    }

    public static string Serialize<T>(T graph, bool useDataContractSerializer)
    {
      return Serialize<T>(graph, Encoding.UTF8, useDataContractSerializer);
    }

    public static string Serialize<T>(T graph, Encoding encoding, bool useDataContractSerializer)
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
          serializer.Serialize(writer, graph);
        }
        writer.Close();

        xml = builder.ToString();
       
        return xml;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while serializing " + typeof(T).Name + ".", exception);
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
        throw new Exception("Error while serializing stream.", exception);
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
        return stream;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while serializing " + typeof(T).Name + "to stream.", exception);
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
        throw new Exception("Error while deserializing " + typeof(T).Name + ".", exception);
      }
    }

    public static T DeserializeFromStream<T>(Stream stream)
    {
      return DeserializeFromStream<T>(stream, true);
    }

    public static T DeserializeFromStream<T>(Stream stream, bool useDataContractSerializer)
    {
      T graph;
      XmlReader reader = null;
      try
      {
        reader = XmlReader.Create(stream);
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
        
        return graph;
      }
      catch (Exception exception)
      {
        throw new Exception("Error while deserializing stream to " + typeof(T).Name + ".", exception);
      }
      finally
      {
        reader.Close();
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
        throw new Exception("Error while deserializing string to stream.", exception);
      }
      finally
      {
        writer.Close();
      }
    }

    public static string NameSafe(string name)
    {
      return Regex.Replace(name, @"^\d*|\W", "");
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

    public static List<T> GetEnumValues<T>()
    {
      var type = typeof(T);
      if (!type.IsEnum)
        throw new ArgumentException("Type '" + type.Name + "' is not an enum");

      return (
        from field in type.GetFields(BindingFlags.Public | BindingFlags.Static)
        where field.IsLiteral
        select (T)field.GetValue(type)).ToList();
    }

    public static List<string> GetEnumStrings<T>()
    {
      var type = typeof(T);
      if (!type.IsEnum)
        throw new ArgumentException("Type '" + type.Name + "' is not an enum");

      return (
        from field in type.GetFields(BindingFlags.Public | BindingFlags.Static)
        where field.IsLiteral
        select field.Name).ToList();
    }

    public static string TitleCase(string value)
    {
      string returnValue = String.Empty;
      string[] words = value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

      foreach (string word in words)
      {
        returnValue += word.Substring(0, 1).ToUpper();

        if (word.Length > 1)
          returnValue += word.Substring(1).ToLower();
      }

      return returnValue;
    }
  }
}
