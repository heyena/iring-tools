package org.iringtools.utility;

public class Utility
  {
	/*  public static <T, R> R Transform(Object graph, String stylesheetUri)
		{
		  return Transform<T, R>((T)graph, stylesheetUri, null, true);
		}
	  
	public static <T, R> R Transform(Object graph, String stylesheetUri, boolean useDataContractSerializer)
	{
	  return Transform<T, R>((T)graph, stylesheetUri, null, useDataContractSerializer);
	}

	public static <T, R> R Transform(T graph, String stylesheetUri)
	{
	  return Transform<T, R>(graph, stylesheetUri, null, true);
	}

	public static <T, R> R Transform(T graph, Stream stylesheet)
	{
	  return Transform<T, R>(graph, stylesheet, null, true);
	}

	public static <T, R> R Transform(T graph, String stylesheetUri, boolean useDataContractSerializer)
	{
	  return Transform<T, R>(graph, stylesheetUri, null, useDataContractSerializer);
	}

	public static <T, R> R Transform(T graph, Stream stylesheet, boolean useDataContractSerializer)
	{
	  return Transform<T, R>(graph, stylesheet, null, useDataContractSerializer);
	}

	public static <T, R> R Transform(T graph, String stylesheetUri, XsltArgumentList arguments)
	{
	  return Transform<T, R>(graph, stylesheetUri, arguments, true);
	}
	
	public static <T, R> R Transform(T graph, Stream stylesheet, XsltArgumentList arguments)
	{
	  return Transform<T, R>(graph, stylesheet, arguments, true);
	}

	public static <T, R> R Transform(T graph, String stylesheetUri, XsltArgumentList arguments, boolean useDataContractSerializer)
	{
	  FileStream stream;

	  try
	  {
		stream = new FileStream(stylesheetUri, FileMode.Open);
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while loading stylesheet " + stylesheetUri + ".", exception);
	  }

	  return Transform<T, R>(graph, stream, arguments, useDataContractSerializer);
	}

	public static <T, R> R Transform(T graph, String stylesheetUri, XsltArgumentList arguments, boolean useDataContractSerializer, boolean useDataContractDeserializer)
	{
	  FileStream stream;

	  try
	  {
		stream = new FileStream(stylesheetUri, FileMode.Open);
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while loading stylesheet " + stylesheetUri + ".", exception);
	  }

	  return Transform<T, R>(graph, stream, arguments, useDataContractSerializer, useDataContractDeserializer);
	}

	public static <T, R> R Transform(T graph, Stream stylesheet, XsltArgumentList arguments, boolean useDataContractSerializer)
	{
	  return Transform<T, R>(graph, stylesheet, arguments, useDataContractSerializer, useDataContractSerializer);
	}

	public static <T, R> R Transform(T graph, Stream stylesheet, XsltArgumentList arguments, boolean useDataContractSerializer, boolean useDataContractDeserializer)
	{
	  String xml;
	  try
	  {
		xml = Serialize<T>(graph, useDataContractSerializer);

		xml = Transform(xml, stylesheet, arguments);

		R resultGraph = Deserialize<R>(xml, useDataContractDeserializer);

		return resultGraph;
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while transforming " + T.class.Name + " to " + R.class.Name + ".", exception);

	  }
	}

	public static <T> Stream Transform(T graph, String stylesheetUri)
	{
	  return Transform<T>(graph, stylesheetUri, null, true);
	}

	public static <T> Stream Transform(T graph, String stylesheetUri, boolean useDataContractSerializer)
	{
	  return Transform<T>(graph, stylesheetUri, null, useDataContractSerializer);
	}

	public static <T> Stream Transform(T graph, String stylesheetUri, XsltArgumentList arguments, boolean useDataContractSerializer)
	{
	  FileStream stream;

	  try
	  {
		stream = new FileStream(stylesheetUri, FileMode.Open);
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while loading stylesheet " + stylesheetUri + ".", exception);
	  }

	  return Transform<T>(graph, stream, arguments, useDataContractSerializer);
	}

	public static <T> Stream Transform(T graph, Stream stylesheet, XsltArgumentList arguments, boolean useDataContractSerializer)
	{
	  String xml;
	  try
	  {
		xml = Serialize<T>(graph, useDataContractSerializer);

		xml = Transform(xml, stylesheet, arguments);

		Stream resultGraph = DeserializeToStream(xml);

		return resultGraph;
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while transforming " + T.class.Name + " to stream.", exception);

	  }
	}

	public static <R> R Transform(Stream graph, String stylesheetUri)
	{
	  return Transform<R>(graph, stylesheetUri, null, true);
	}

	public static <R> R Transform(Stream graph, Stream stylesheet)
	{
	  return Transform<R>(graph, stylesheet, null, true);
	}

	public static <R> R Transform(Stream graph, String stylesheetUri, boolean useDataContractSerializer)
	{
	  return Transform<R>(graph, stylesheetUri, null, useDataContractSerializer);
	}

	public static <R> R Transform(Stream graph, Stream stylesheet, boolean useDataContractSerializer)
	{
	  return Transform<R>(graph, stylesheet, null, useDataContractSerializer);
	}

	public static <R> R Transform(Stream graph, String stylesheetUri, XsltArgumentList arguments)
	{
	  return Transform<R>(graph, stylesheetUri, arguments, true);
	}

	public static <R> R Transform(Stream graph, Stream stylesheet, XsltArgumentList arguments)
	{
	  return Transform<R>(graph, stylesheet, arguments, true);
	}

	public static <R> R Transform(Stream graph, String stylesheetUri, XsltArgumentList arguments, boolean useDataContractSerializer)
	{
	  FileStream stream;

	  try
	  {
		stream = new FileStream(stylesheetUri, FileMode.Open);
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while loading stylesheet " + stylesheetUri + ".", exception);
	  }

	  return Transform<R>(graph, stream, arguments, useDataContractSerializer);
	}

	public static <R> R Transform(Stream graph, Stream stylesheet, XsltArgumentList arguments, boolean useDataContractSerializer)
	{
	  String xml;
	  try
	  {
		xml = SerializeFromStream(graph);

		xml = Transform(xml, stylesheet, arguments);

		R resultGraph = Deserialize<R>(xml, useDataContractSerializer);

		return resultGraph;
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while transforming " + Stream.class + " to " + R.class.Name + ".", exception);

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

    public static <T> void Write(T graph, String path)
	{
	  Write<T>(graph, path, true);
	}
	public static <T> void Write(T graph, String path, boolean useDataContractSerializer)
	{
	  FileStream stream = null;
	  XmlDictionaryWriter writer = null;
	  try
	  {
		stream = new FileStream(path, FileMode.Create, FileAccess.Write);
		writer = XmlDictionaryWriter.CreateTextWriter(stream);


		if (useDataContractSerializer)
		{
		  DataContractSerializer serializer = new DataContractSerializer(T.class);
		  serializer.WriteObject(writer, graph);
		}
		else
		{
		  XmlSerializer serializer = new XmlSerializer(T.class);
		  serializer.Serialize(writer, graph);
		}

	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while writing " + T.class.Name + " to " + path + ".", exception);
	  }
	  finally
	  {
		if (writer != null)
		{
		  writer.Close();
		}
		if (stream != null)
		{
		  stream.Close();
		}
	  }
	}

	public static <T> MemoryStream Write(T graph, boolean useDataContractSerializer)
	{
	  try
	  {
		MemoryStream stream = new MemoryStream();
		XmlDictionaryWriter writer = XmlDictionaryWriter.CreateTextWriter(stream);


		if (useDataContractSerializer)
		{
		  DataContractSerializer serializer = new DataContractSerializer(T.class);
		  serializer.WriteObject(writer, graph);
		}
		else
		{
		  XmlSerializer serializer = new XmlSerializer(T.class);
		  serializer.Serialize(writer, graph);
		}

		//writer.Close();
		stream.Seek(0, SeekOrigin.Begin);
		return stream;
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while writing " + T.class.Name + " to memory stream.", exception);
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

    public static <T> T Read(String path)
	{
	  return Read<T>(path, true);
	}

	public static <T> T Read(String path, boolean useDataContractSerializer)
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
		  DataContractSerializer serializer = new DataContractSerializer(T.class);
		  graph = (T)serializer.ReadObject(reader, true);
		}
		else
		{
		  XmlSerializer serializer = new XmlSerializer(T.class);
		  graph = (T)serializer.Deserialize(reader);
		}

		return graph;
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while reading " + T.class.Name + " from " + path + ".", exception);
	  }
	  finally
	  {
		if (reader != null)
		{
			reader.Close();
		}
		if (stream != null)
		{
			stream.Close();
		}
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

    public static <T> T DeserializeDataContract(String xml)
	{
	  return Deserialize<T>(xml, true);
	}

    public static <T> T DeserializeXml(String xml)
	{
	  return Deserialize<T>(xml, false);
	}

	public static <T> String SerializeXml(T graph)
	{
	  return Serialize<T>(graph, Encoding.UTF8, false);
	}

	public static <T> String SerializeDataContract(T graph)
	{
	  return Serialize<T>(graph, Encoding.UTF8);
	}

	public static <T> String Serialize(T graph, Encoding encoding)
	{
	  return Serialize<T>(graph, encoding, true);
	}

	public static <T> String Serialize(T graph, boolean useDataContractSerializer)
	{
	  return Serialize<T>(graph, Encoding.UTF8, useDataContractSerializer);
	}

	public static <T> String Serialize(T graph, Encoding encoding, boolean useDataContractSerializer)
	{
	  String xml;
	  try
	  {
		StringBuilder builder = new StringBuilder();
		TextWriter encoder = new StringEncoder(builder, encoding);
		XmlWriter writer = XmlWriter.Create(encoder);

		if (useDataContractSerializer)
		{
		  DataContractSerializer serializer = new DataContractSerializer(T.class);
		  serializer.WriteObject(writer, graph);
		}
		else
		{
		  XmlSerializer serializer = new XmlSerializer(T.class);
		  serializer.Serialize(writer, graph);
		}
		writer.Close();

		xml = builder.toString();

		return xml;
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while serializing " + T.class.Name + ".", exception);
	  }
	}

	public static <T> XElement SerializeToXElement(T graph)
	{
	  try
	  {
		XmlSerializer ser = new XmlSerializer(T.class);
		XDocument doc = new XDocument();

		XmlWriter xw = doc.CreateWriter();
		try
		{
		  ser.Serialize(xw, graph);
		  xw.Close();
		}
		finally
		{
			xw.dispose();
		}

		return doc.Root;
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while serializing " + T.class.Name + ".", exception);
	  }
	}

	public static String SerializeFromStream(Stream graph)
	{
	  try
	  {
		StreamReader reader = new StreamReader(graph, Encoding.UTF8);
		String value = reader.ReadToEnd();
		return value;
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while serializing stream.", exception);
	  }
	}
	public static <T> MemoryStream SerializeToMemoryStream(T graph)
	{
	  return SerializeToMemoryStream(graph, true);
	}

	public static <T> MemoryStream SerializeToMemoryStream(T graph, boolean useDataContractSerializer)
	{
	  try
	  {
		MemoryStream stream = new MemoryStream();

		if (useDataContractSerializer)
		{
		  DataContractSerializer serializer = new DataContractSerializer(T.class);
		  serializer.WriteObject(stream, graph);
		}
		else
		{
		  XmlSerializer serializer = new XmlSerializer(T.class);
		  serializer.Serialize(stream, graph);
		}
		return stream;
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while serializing " + T.class.Name + "to stream.", exception);
	  }
	}

	public static <T> T Deserialize(String xml, boolean useDataContractSerializer)
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
		  DataContractSerializer serializer = new DataContractSerializer(T.class);

		  graph = (T)serializer.ReadObject(reader, false);
		}
		else
		{
		  XmlSerializer serializer = new XmlSerializer(T.class);
		  graph = (T)serializer.Deserialize(reader);
		}
		return graph;
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while deserializing " + T.class.Name + ".", exception);
	  }
	}

	public static <T> T DeserializeFromXElement(XElement element)
	{
	  try
	  {
		XmlReader reader = element.CreateReader();
		XmlSerializer serializer = new XmlSerializer(T.class);
		return (T)serializer.Deserialize(reader);
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while deserializing " + T.class.Name + ".", exception);
	  }
	}

	public static <T> T DeserializeFromStream(Stream stream)
	{
	  return DeserializeFromStream<T>(stream, true);
	}

	public static <T> T DeserializeFromStream(Stream stream, boolean useDataContractSerializer)
	{
	  T graph;
	  XmlDictionaryReader reader = null;
	  try
	  {
		reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
		if (useDataContractSerializer)
		{
		  DataContractSerializer serializer = new DataContractSerializer(T.class);
		  graph = (T)serializer.ReadObject(reader, true);
		}
		else
		{
		  XmlSerializer serializer = new XmlSerializer(T.class);
		  graph = (T)serializer.Deserialize(reader);
		}

		return graph;
	  }
	  catch (RuntimeException exception)
	  {
		throw new RuntimeException("Error while deserializing stream to " + T.class.Name + ".", exception);
	  }
	  finally
	  {
		if (reader != null)
		{
			reader.Close();
		}
	  }
	}

    public static Stream DeserializeToStream(String graph)
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

    public static String ToJson<T>(T obj)
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

	public static <T> T CloneDataContractObject(T obj)
	{
	  String xml = SerializeDataContract<T>(obj);
	  return DeserializeDataContract<T>(xml);
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
*/
    public static String sqlTypeToCSharpType(String sqlType)
    {
    	
        if(sqlType.equalsIgnoreCase("bit")){return "Boolean";}
        else if(sqlType.equalsIgnoreCase("byte")){ return "Byte";}
        else if(sqlType.equalsIgnoreCase("char")){ return "Char";}
        else if(sqlType.equalsIgnoreCase("nchar")){ return "String";}
        else if(sqlType.equalsIgnoreCase("character")){ return "Char";}
        else if(sqlType.equalsIgnoreCase("varchar")){ return "String";}
        else if(sqlType.equalsIgnoreCase("varchar2")){ return "String";}
        else if(sqlType.equalsIgnoreCase("nvarchar")){return "String";}
        else if(sqlType.equalsIgnoreCase("nvarchar2")){ return "String";}
        else if(sqlType.equalsIgnoreCase("text")){ return "String";}
        else if(sqlType.equalsIgnoreCase("ntext")){ return "String";}
        else if(sqlType.equalsIgnoreCase("xml")){ return "String";}
        else if(sqlType.equalsIgnoreCase("date")){ return "DateTime";}
        else if(sqlType.equalsIgnoreCase("datetime")){ return "DateTime";}
        else if(sqlType.equalsIgnoreCase("smalldatetime")){ return "DateTime";}
        else if(sqlType.equalsIgnoreCase("time")){ return "DateTime";}
        else if(sqlType.equalsIgnoreCase("timestamp")){ return "DateTime";}
        else if(sqlType.equalsIgnoreCase("dec")){ return "Double";}
        else if(sqlType.equalsIgnoreCase("decimal")){ return "Decimal";}
        else if(sqlType.equalsIgnoreCase("money")){ return "Double";}
        else if(sqlType.equalsIgnoreCase("smallmoney")){ return "Double";}
        else if(sqlType.equalsIgnoreCase("numeric")){ return "Double";}
        else if(sqlType.equalsIgnoreCase("float")){ return "Single";}
        else if(sqlType.equalsIgnoreCase("real")){ return "Double";}
        else if(sqlType.equalsIgnoreCase("int")){ return "Int32";}
        else if(sqlType.equalsIgnoreCase("integer")){ return "Int32";}
        else if(sqlType.equalsIgnoreCase("bigint")){ return "Int64";}
        else if(sqlType.equalsIgnoreCase("smallint")){ return "Int16";}
        else if(sqlType.equalsIgnoreCase("tinyint")){ return "Int16";}
        else if(sqlType.equalsIgnoreCase("number")){ return "Decimal";}
        else if(sqlType.equalsIgnoreCase("long")){ return "Int64";}
        else if(sqlType.equalsIgnoreCase("clob")){ return "String";}
        else if(sqlType.equalsIgnoreCase("blob")){ return "String";}
        else{
        	return "String";
        }
        
    
    }
    
    public static String nameSafe(String name)
	{
	  return name.replaceAll("^\\d*|\\W", "");
	}

/*
    
    public static <O, T> void SearchAndInsert(java.util.ArrayList<O> list, O element, T Comparer)
	{
		java.util.Comparator<O> Comp = (java.util.Comparator<O>)Comparer;
		int index = list.BinarySearch(element, Comp);

		if (index < 0)
		{
			list.add(~index, element);
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
*/  }