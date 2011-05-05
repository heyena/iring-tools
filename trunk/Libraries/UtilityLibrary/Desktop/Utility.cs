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

    public static void Write<T>(T graph, string path, bool useDataContractSerializer)
    {
      FileStream stream = null;
      XmlDictionaryWriter writer = null;
      try
      {
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
          serializer.Serialize(writer, graph);
        }

      }
      catch (Exception exception)
      {
        throw new Exception("Error while writing " + typeof(T).Name + " to " + path + ".", exception);
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
        throw new Exception("Error while writing " + typeof(T).Name + " to memory stream.", exception);
      }
    }

    public static void WriteStream(Stream graph, string path)
    {
      FileStream stream = null;
      try
      {
        stream = new FileStream(path, FileMode.Create, FileAccess.Write);

        byte[] data = ((MemoryStream)graph).ToArray();

        stream.Write(data, 0, data.Length);
      }
      catch (Exception exception)
      {
        throw new Exception("Error while writing stream to " + path + ".", exception);
      }
      finally
      {
        stream.Close();
      }
    }

    public static T Read<T>(string path)
    {
      return Read<T>(path, true);
    }

    public static T Read<T>(string path, bool useDataContractSerializer)
    {
      T graph;
      FileStream stream = null;
      XmlDictionaryReader reader = null;

      try
      {
        stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());

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
        throw new Exception("Error while reading " + typeof(T).Name + " from " + path + ".", exception);
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
        throw new Exception("Error while reading string from " + path + ".", exception);
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
        throw new Exception("Error while reading Xml from " + path + ".", exception);
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
        throw new Exception("Error while reading stream from " + path + ".", exception);
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

      byte[] buffer = new byte[10000];

      int bytesRead = 0;

      do
      {

        bytesRead = requestStream.Read(buffer, 0, buffer.Length);

        usableStream.Write(buffer, 0, bytesRead);

      } while (bytesRead > 0);

      usableStream.Position = 0;

      return usableStream;
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
      XmlDictionaryReader reader = null;
      try
      {
        reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
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
        if (reader != null) reader.Close();
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
        throw new Exception("Error while writing string to " + path + ".", exception);
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
      string type = (xsdType.StartsWith("xsd:") || xsdType.StartsWith("XSD:")) ? xsdType.Substring(4) : xsdType;

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
        case "string": return "String";
        case "time": return "DateTime";
        default: throw new Exception("XSD type \"" + xsdType + "\" not currently supported.");
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
        case "date": return "DateTime";
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

    public static string NameSafe(string name)
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

    public static string ToXsdDateTime(string dateTime)
    {
      if (String.IsNullOrEmpty(dateTime)) 
        return dateTime;

      DateTime dt = DateTime.Parse(dateTime);
      return dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
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
  }
}
