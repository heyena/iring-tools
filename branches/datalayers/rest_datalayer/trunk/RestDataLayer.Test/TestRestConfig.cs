using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using org.iringtools.library;

namespace RestDataLayer.Test
{
    public class TestRestConfig
    {


        public static void ReadXML(RestConfigSettings objRestDD, string path_to_xml)
        {
            //RestConfigSettings objRestDD = new RestConfigSettings();
            string fileName = @"C:\Users\npandey1\Desktop\RestDataLayer\RestDataLayer.Test\App_Data\GenericDataDictionaryRestful.xml";

            XDocument doc = XDocument.Load(fileName);

            //  reading root elements
            foreach (XElement element in doc.Root.Elements())
            {
                switch (element.Name.LocalName)
                {
                    case "baseUrl":
                        objRestDD.BaseURL = element.Value;
                        break;
                    case "appKey":
                        objRestDD.AppKey = element.Value;
                        break;
                    case "accessToken":
                        objRestDD.AccessToken = element.Value;
                        break;
                    case "contentType":
                        objRestDD.ContentType = element.Value;
                        break;
                    case "enablePaging":
                        objRestDD.EnablePaging = bool.Parse(element.Value);
                        break;
                    case "startPage":
                        objRestDD.StartPage = int.Parse(element.Value);
                        break;
                    case "endPage":
                        objRestDD.EndPage = int.Parse(element.Value);
                        break;
                }
            }

            List<ConfigDataObject> lstConfigDataObject = new List<ConfigDataObject>();

            //  loop through each DataObject of config file
            foreach (XElement element in doc.Descendants("dataObject"))
            {
                ConfigDataObject objDataObject = new ConfigDataObject();
                List<ConfigKeyProperty> lstConfigKeyProperty = new List<ConfigKeyProperty>();


                //  reading Key Property elements for current DataObject
                foreach (XElement el in element.Descendants("keyProperties").Elements())
                {
                    ConfigKeyProperty objKeyProperty = new ConfigKeyProperty();
                    foreach (XNode xnode in el.Nodes())
                    {
                        objKeyProperty.keyPropertyName = ((System.Xml.Linq.XElement)xnode).Value;
                    }
                    lstConfigKeyProperty.Add(objKeyProperty);
                }

                //  assigning Key Property elements for current DataObject
                objDataObject.keyProperties = lstConfigKeyProperty;
                lstConfigKeyProperty = null;

                List<ConfigDataProperty> lstConfigDataProperty = new List<ConfigDataProperty>();

                //  reading Data Property elements for current DataObject
                foreach (XElement el in element.Descendants("dataProperties").Elements())
                {
                    ConfigDataProperty objConfigDataProperty = new ConfigDataProperty();

                    foreach (XNode xnode in el.Nodes())
                    {
                        switch (((System.Xml.Linq.XElement)xnode).Name.LocalName)
                        {
                            case "columnName":
                                objConfigDataProperty.columnName = ((System.Xml.Linq.XElement)xnode).Value;
                                break;
                            case "propertyName":
                                objConfigDataProperty.propertyName = ((System.Xml.Linq.XElement)xnode).Value;
                                break;
                            case "dataType":
                                if (Enum.IsDefined(typeof(DataType), ((System.Xml.Linq.XElement)xnode).Value))
                                    objConfigDataProperty.dataType = (DataType)Enum.Parse(typeof(DataType), ((System.Xml.Linq.XElement)xnode).Value, true);
                                break;
                            case "dataLength":
                                objConfigDataProperty.dataLength = int.Parse(((System.Xml.Linq.XElement)xnode).Value);
                                break;
                            case "isNullable":
                                objConfigDataProperty.isNullable = bool.Parse(((System.Xml.Linq.XElement)xnode).Value);
                                break;
                            case "keyType":
                                if (Enum.IsDefined(typeof(KeyType), ((System.Xml.Linq.XElement)xnode).Value))
                                    objConfigDataProperty.keyType = (KeyType)Enum.Parse(typeof(KeyType), ((System.Xml.Linq.XElement)xnode).Value, true);
                                break;
                        }
                    }
                    lstConfigDataProperty.Add(objConfigDataProperty);
                }

                //  assigning Data Property(s) collection for current DataObject
                objDataObject.dataProperties = lstConfigDataProperty;
                lstConfigDataProperty = null;

                //  assigning Data Propery(s) collection for current DataObject
                lstConfigDataObject.Add(objDataObject);
                objDataObject = null;
            }
            //  assigning DataObject collection for Config object
            objRestDD.dataObjects = lstConfigDataObject;
        }
    }

}
