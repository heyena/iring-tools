using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using org.iringtools.utility;
using System.Diagnostics;
using System.Text;
using System.CodeDom.Compiler;

namespace WidgetService
{
    public class WidgetProvider
    {
        private string NAMESPACE_PREFIX = "WidgetService";
       
        private string _namespace = String.Empty;

        private IndentedTextWriter _dataObjectWriter = null;
        private StringBuilder _dataObjectBuilder = null;


        public WidgetProvider()
        {
        }

        public List<Sample> Generate()
        {
            string path = "D:\\Project\\Datalayers\\Samples\\WidgetService\\WidgetService\\Widget.xml";
            
            XmlTextReader reader = new XmlTextReader(path);
            reader.WhitespaceHandling = WhitespaceHandling.None;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(reader);

            try
            {

                _namespace = NAMESPACE_PREFIX;

                _dataObjectBuilder = new StringBuilder();
                _dataObjectWriter = new IndentedTextWriter(new StringWriter(_dataObjectBuilder), "  ");


                // _dataObjectWriter.WriteLine(Utility.GeneratedCodeProlog);
                _dataObjectWriter.WriteLine("using System;");
                _dataObjectWriter.WriteLine("using System.Collections.Generic;");
//                _dataObjectWriter.WriteLine("using Iesi.Collections.Generic;");
                _dataObjectWriter.WriteLine("using org.iringtools.library;");
                _dataObjectWriter.WriteLine();
                _dataObjectWriter.WriteLine("namespace {0}", _namespace);
                _dataObjectWriter.WriteLine("{"); // begin namespace block
                _dataObjectWriter.Indent++;


                _dataObjectWriter.WriteLine("public class {0} : IDataObject", Path.GetFileNameWithoutExtension(path));
                _dataObjectWriter.WriteLine("{"); // begin class block
                _dataObjectWriter.Indent++;
                _dataObjectWriter.WriteLine();

                foreach (XmlElement docElement in xmlDoc.DocumentElement)
                {
                    if (_dataObjectBuilder.ToString().Contains(docElement.Name.ToUpper()) != true)
                    {
                        _dataObjectWriter.WriteLine("public virtual List<" + docElement.Name.ToUpper() + "> " + docElement.Name.ToUpper() + " { get; set; }");

                    }

                }
                _dataObjectWriter.WriteLine();
                foreach (XmlElement docElement in xmlDoc.DocumentElement)
                {

                    if (_dataObjectBuilder.ToString().Contains("class " + docElement.Name.ToUpper()) != true)
                    {
                        _dataObjectWriter.WriteLine("public class {0}", docElement.Name.ToUpper());
                        _dataObjectWriter.WriteLine("{"); // begin class block
                        _dataObjectWriter.Indent++;

                        foreach (XmlElement element in docElement)
                        {

                            _dataObjectWriter.WriteLine("public string " + element.Name.ToUpper() + ";");

                        }
                        foreach (System.Xml.XmlAttribute Attributes in docElement.Attributes)
                        {

                            _dataObjectWriter.WriteLine("public string " + Attributes.Name.ToUpper() + ";");

                        }

                        _dataObjectWriter.Indent--;
                        _dataObjectWriter.WriteLine("}");
                        _dataObjectWriter.WriteLine();
                    }
                }
                _dataObjectWriter.WriteLine("public virtual object GetPropertyValue(string propertyName)");
                _dataObjectWriter.WriteLine("{"); // begin virtual function
                _dataObjectWriter.Indent++;
                _dataObjectWriter.WriteLine("switch (propertyName)");
                _dataObjectWriter.WriteLine("{");// begin switch statement
                _dataObjectWriter.Indent++;

                foreach (XmlElement docElement in xmlDoc.DocumentElement)
                {
                    if (_dataObjectBuilder.ToString().Contains("return " + docElement.Name.ToUpper()) != true)
                    {
                        _dataObjectWriter.WriteLine("case \"{0}\": return {0};", docElement.Name.ToUpper());

                    }

                }
                _dataObjectWriter.WriteLine("default: throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
                _dataObjectWriter.WriteLine("}");
                _dataObjectWriter.Indent--;
                _dataObjectWriter.WriteLine("}");
                _dataObjectWriter.Indent--;
                _dataObjectWriter.WriteLine();

                // Implements SetPropertyValue of IDataObject
                _dataObjectWriter.WriteLine();
                _dataObjectWriter.WriteLine("public virtual void SetPropertyValue(string propertyName, object value)");
                _dataObjectWriter.WriteLine("{");
                _dataObjectWriter.Indent++;
                _dataObjectWriter.WriteLine("switch (propertyName)");
                _dataObjectWriter.Write("{");
                _dataObjectWriter.Indent++;

                foreach (XmlElement docElement in xmlDoc.DocumentElement)
                {
                    if (_dataObjectBuilder.ToString().Contains(string.Format("if (value != null) {0}", docElement.Name.ToUpper())) != true)
                    {
                        _dataObjectWriter.WriteLine("case \"{0}\":", docElement.Name.ToUpper());
                        _dataObjectWriter.Indent++;

                        _dataObjectWriter.WriteLine("if (value != null) {0} =  Convert.ToString(value);", docElement.Name.ToUpper());
                        _dataObjectWriter.WriteLine("break;");
                        _dataObjectWriter.Indent--;
                    }
                    
                }
                _dataObjectWriter.WriteLine("default:");
                _dataObjectWriter.Indent++;
                _dataObjectWriter.WriteLine("throw new Exception(\"Property [\" + propertyName + \"] does not exist.\");");
                _dataObjectWriter.Indent--;
                _dataObjectWriter.Indent--;
                _dataObjectWriter.WriteLine("}");
                _dataObjectWriter.Indent--;
                _dataObjectWriter.WriteLine("}");

                #region generate GetRelatedObjects method
                _dataObjectWriter.WriteLine();
                _dataObjectWriter.WriteLine(@"public virtual IList<IDataObject> GetRelatedObjects(string relatedObjectType)");
                _dataObjectWriter.WriteLine("{");
                _dataObjectWriter.Indent++;
                _dataObjectWriter.WriteLine("switch (relatedObjectType)");
                _dataObjectWriter.WriteLine("{");
                _dataObjectWriter.Indent++;


                _dataObjectWriter.WriteLine("default:");
                _dataObjectWriter.Indent++;
                _dataObjectWriter.WriteLine("throw new Exception(\"Related object [\" + relatedObjectType + \"] does not exist.\");");
                _dataObjectWriter.Indent--;
                _dataObjectWriter.WriteLine("}");
                _dataObjectWriter.Indent--;
                _dataObjectWriter.WriteLine("}");
                #endregion

                _dataObjectWriter.Indent--;
                _dataObjectWriter.WriteLine("}"); // end class block

                _dataObjectWriter.Indent--;
                _dataObjectWriter.WriteLine("}"); // end namespace block                

               
                string sourceCode = _dataObjectBuilder.ToString();

                #region Writing memory data to disk

                Utility.WriteString(sourceCode, "D:\\Project\\Datalayers\\Samples\\WidgetService\\WidgetService\\" + Path.GetFileNameWithoutExtension(path)+ ".cs", Encoding.ASCII);
                
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating application entities " + ex);
            }

            //Close off the connection to the file.


            reader.Close();

            return new List<Sample>() { new Sample() { Id = 1, Name = "Hello" } };
        }

    }
}