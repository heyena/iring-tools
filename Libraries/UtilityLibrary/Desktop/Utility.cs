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
        settings.ProhibitDtd = false;
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

    public static string ToCSharpType(string xsdType)
    {
      string type = (xsdType.StartsWith("xsd:") || xsdType.StartsWith("XSD:")) ? xsdType.Substring(4) : xsdType;
      
      switch (type.ToLower())
      {
        case "boolean": return "Boolean";
        case "byte": return "SByte";
        case "date": return "DateTime";
        case "datetime": return "DateTime";
        case "decimal": return "Decimal";
        case "double": return "Double";
        case "float": return "Single";
        case "int": return "Int32";
        case "integer": return "Decimal";
        case "long": return "Int64";
        case "short": return "Int16";
        case "string": return "String";
        case "time": return "DateTime";
        default: return xsdType;
      }
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
  }
}
