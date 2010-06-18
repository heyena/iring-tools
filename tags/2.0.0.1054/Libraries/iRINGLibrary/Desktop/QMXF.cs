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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Text;

namespace org.ids_adi.qmxf
{
    [DataContract]
    [XmlRoot(ElementName = "qmxf", Namespace = "http://ns.ids-adi.org/qxf/model#")]
    public class QMXF
    {
        public QMXF()
        {
            this.classDefinitions = new List<ClassDefinition>();
            this.templateDefinitions = new List<TemplateDefinition>();
            this.templateQualifications = new List<TemplateQualification>();
            this.licenses = new List<License>();
        }

        [DataMember]
        [XmlElement(ElementName = "class-definition")]
        public List<ClassDefinition> classDefinitions { get; set; }

        [DataMember]
        [XmlElement(ElementName = "template-definition")]
        public List<TemplateDefinition> templateDefinitions { get; set; }

        [DataMember]
        [XmlElement(ElementName = "template-qualification")]
        public List<TemplateQualification> templateQualifications { get; set; }

        [DataMember]
        [XmlElement(ElementName = "license")]
        public List<License> licenses { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "timestamp")]
        public string timestamp { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "license-ref")]
        public string licenseRef { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "targetRepository")]
        public string targetRepository { get; set; }


    }

    [DataContract]
    public class TemplateDefinition
    {
        public TemplateDefinition()
        {
            this.name = new List<QMXFName>();
            this.description = new List<Description>();
            this.suggestedDesignation = new List<SuggestedDesignation>();
            this.status = new List<Status>();
            this.textualDefinition = new List<TextualDefinition>();
            this.roleDefinition = new List<RoleDefinition>();
            this.repositoryName = string.Empty;
        }

        [DataMember]
        [XmlElement(ElementName = "repository")]
        public string repositoryName { get; set; }

        [DataMember]
        [XmlElement(ElementName = "suggested-designation")]
        public List<SuggestedDesignation> suggestedDesignation { get; set; }

        [DataMember]
        [XmlElement(ElementName = "designation")]
        public Designation designation { get; set; }

        [DataMember]
        [XmlElement(ElementName = "name")]
        public List<QMXFName> name { get; set; }

        [DataMember]
        [XmlElement(ElementName = "description")]
        public List<Description> description { get; set; }

        [DataMember]
        [XmlElement(ElementName = "textual-definition")]
        public List<TextualDefinition> textualDefinition { get; set; }

        [DataMember]
        [XmlElement(ElementName = "status")]
        public List<Status> status { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "id")]
        public string identifier { get; set; }

        [DataMember]
        [XmlElement(ElementName = "role-definition")]
        public List<RoleDefinition> roleDefinition { get; set; }
    }

    [DataContract]
    public class ClassDefinition
    {
        public ClassDefinition()
        {
          this.name = new List<QMXFName>();
          this.description = new List<Description>();
          this.classification = new List<Classification>();
          this.specialization = new List<Specialization>();
          this.status = new List<Status>();
          this.suggestedDesignation = new List<SuggestedDesignation>();
          this.textualDefinition = new List<TextualDefinition>();
          this.repositoryName = string.Empty;
        }

        [DataMember]
        [XmlElement(ElementName = "repository")]
        public string repositoryName { get; set; }

        [DataMember]
        [XmlElement(ElementName = "entity-type")]
        public EntityType entityType { get; set; }

        [DataMember]
        [XmlElement(ElementName = "classification")]
        public List<Classification> classification { get; set; }

        [DataMember]
        [XmlElement(ElementName = "specialization")]
        public List<Specialization> specialization { get; set; }

        [DataMember]
        [XmlElement(ElementName = "suggested-designation")]
        public List<SuggestedDesignation> suggestedDesignation { get; set; }

        [DataMember]
        [XmlElement(ElementName = "designation")]
        public Designation designation { get; set; }

        [DataMember]
        [XmlElement(ElementName = "name")]
        public List<QMXFName> name { get; set; }

        [DataMember]
        [XmlElement(ElementName = "description")]
        public List<Description> description { get; set; }

        [DataMember]
        [XmlElement(ElementName = "textual-definition")]
        public List<TextualDefinition> textualDefinition { get; set; }

        [DataMember]
        [XmlElement(ElementName = "status")]
        public List<Status> status { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "id")]
        public string identifier { get; set; }

    }

    [DataContract]
    public class TemplateQualification
    {
        public TemplateQualification()
        {
          this.name = new List<QMXFName>();
          this.description = new List<Description>();
          this.suggestedDesignation = new List<SuggestedDesignation>();
          this.designation = new List<Designation>();
          this.status = new List<Status>();
          this.textualDefinition = new List<TextualDefinition>();
          this.roleQualification = new List<RoleQualification>();
        }

        [DataMember]
        [XmlElement(ElementName = "suggested-designation")]
        public List<SuggestedDesignation> suggestedDesignation { get; set; }

        [DataMember]
        [XmlElement(ElementName = "designation")]
        public List<Designation> designation { get; set; }

        [DataMember]
        [XmlElement(ElementName = "name")]
        public List<QMXFName> name { get; set; }

        [DataMember]
        [XmlElement(ElementName = "description")]
        public List<Description> description { get; set; }

        [DataMember]
        [XmlElement(ElementName = "textual-definition")]
        public List<TextualDefinition> textualDefinition { get; set; }

        [DataMember]
        [XmlElement(ElementName = "status")]
        public List<Status> status { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "id")]
        public string identifier { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "qualifies")]
        public string qualifies { get; set; }

        [DataMember]
        [XmlElement(ElementName = "role-qualification")]
        public List<RoleQualification> roleQualification { get; set; }

    }

    [DataContract]
    public class License
    {
        public License()
        {
            this.licenseTerms = new List<LicenseTerms>();
        }

        [DataMember]
        [XmlElement(ElementName = "license-terms")]
        public List<LicenseTerms> licenseTerms { get; set; }
    }

    [DataContract]
    public class LicenseTerms
    {
        public LicenseTerms()
        {
            this.licenseText = new List<LicenseText>();
        }

        [DataMember]
        [XmlElement(ElementName = "license-text")]
        public List<LicenseText> licenseText { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "id")]
        public string identifier { get; set; }

    }

    [DataContract]
    public class LicenseText
    {
        public LicenseText()
        {
            this.formal = "true";
        }

        [DataMember]
        [XmlAttribute]
        public string lang { get; set; }

        [DataMember]
        [XmlText]
        public string value { get; set; }

        [DataMember]
        [XmlAttribute]
        public string formal { get; set; }
    }

    [DataContract]
    public class EntityType
    {
        [DataMember]
        [XmlAttribute]
        public string reference { get; set; }
    }

    [DataContract]
    public class Classification
    {
        [DataMember]
        [XmlAttribute]
        public string reference { get; set; }

        [DataMember]
        [XmlAttribute]
        public string label { get; set; }

        public static IComparer<Classification> sortAscending()
        {
            return (IComparer<Classification>)new sortAscendingHelper();
        }

        private class sortAscendingHelper : IComparer<Classification>
        {
            int IComparer<Classification>.Compare(Classification cd1, Classification cd2)
            {
                return string.Compare(cd1.label, cd2.label);
            }
        }
    }

    [DataContract]
    public class Specialization
    {
        [DataMember]
        [XmlAttribute]
        public string reference { get; set; }

        [DataMember]
        [XmlAttribute]
        public string label { get; set; }

        public static IComparer<Specialization> sortAscending()
        {
            return (IComparer<Specialization>)new sortAscendingHelper();
        }

        private class sortAscendingHelper : IComparer<Specialization>
        {
            int IComparer<Specialization>.Compare(Specialization s1, Specialization s2)
            {
                return string.Compare(s1.label, s2.label);
            }
        }
    }

    [DataContract]
    public class SuggestedDesignation
    {
      [DataMember]
      [XmlText]
      public string value { get; set; }
    }

    [DataContract]
    public class QMXFName
    {
        //TODO: Ignore for now
        [DataMember]  
        [XmlAttribute]
        public string lang { get; set; }

        [DataMember]
        [XmlText]
        public string value { get; set; }
    }

    [DataContract]
    public class Designation
    {
        [DataMember]
        [XmlText]
        public string value { get; set; }
    }

    [DataContract]
    public class Description
    {
        //TODO: Ignore for now
        [DataMember]
        [XmlAttribute]
        public string lang { get; set; }

        //TODO: Ignore for now
        [DataMember]
        [XmlAttribute]
        public string contentType { get; set; }

        //TODO: Ignore for now
        [DataMember]
        [XmlAttribute]
        public string parseType { get; set; }

        [DataMember]
        [XmlText]
        public string value { get; set; }
    }

    [DataContract]
    public class TextualDefinition
    {
        public TextualDefinition()
        {
            this.lang = "en-GB";
        }

        [DataMember]
        [XmlAttribute]
        public string lang { get; set; }

        [DataMember]
        [XmlText]
        public string value { get; set; }
    }

    [DataContract]
    public class RuleSet
    {
        [DataMember]
        [XmlAttribute]
        public string contentType { get; set; }

        [DataMember]
        [XmlAttribute]
        public string dlType { get; set; }

        [DataMember]
        [XmlAttribute]
        public string parseType { get; set; }

        [DataMember]
        [XmlText]
        public string value { get; set; }
    }

    [DataContract]
    public class RoleDefinition
    {
        public RoleDefinition()
        {
          this.name = new List<QMXFName>();
          this.suggestedDesignation = new List<SuggestedDesignation>();
          //this.designation = new Designation();
          this.description = new Description();
          //this.range = "http://www.w3.org/2000/01/rdf-schema#Class";
          //this.minimum = "1";
          //this.maximum = "1";
          //this.inverseMinimum = "0";
          //this.inverseMaximum = "unbounded";
        }

        [DataMember]
        [XmlElement(ElementName = "name")]
        public List<QMXFName> name { get; set; }

        [DataMember]
        [XmlElement(ElementName = "suggested-designation")]
        public List<SuggestedDesignation> suggestedDesignation { get; set; }

        [DataMember]
        [XmlElement(ElementName = "designation")]
        public Designation designation { get; set; }

        [DataMember]
        [XmlElement(ElementName = "description")]
        public Description description { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "id")]
        public string identifier { get; set; }
       
        [DataMember]
        [XmlAttribute(AttributeName = "range")]
        public string range { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "minimum")]
        public string minimum { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "maximum")]
        public string maximum { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "inverse-minimum")]
        public string inverseMinimum { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "inverse-maximum")]
        public string inverseMaximum { get; set; }

        public static IComparer<RoleDefinition> sortAscending()
        {
            return (IComparer<RoleDefinition>)new sortAscendingHelper();
        }

        private class sortAscendingHelper : IComparer<RoleDefinition>
        {
            int IComparer<RoleDefinition>.Compare(RoleDefinition rd1, RoleDefinition rd2)
            {
                return string.Compare(rd1.name[0].value, rd2.name[0].value);
            }
        }
    }

    [DataContract]
    public class QMXFValue
    {
        [DataMember]
        [XmlText]
        public string text { get; set; }
        
        [DataMember]
        [XmlAttribute]
        public string lang { get; set; }

        [DataMember]
        [XmlAttribute]
        public string reference { get; set; }

        [DataMember]
        [XmlAttribute]
        public string As { get; set; }
    }

    [DataContract]
    public class RoleQualification
    {
        public RoleQualification()
        {
          this.name = new List<QMXFName>();
          this.description = new List<Description>();
          //this.value = new Value();
          //this.minimum = "1";
          //this.maximum = "1";
          //this.inverseMinimum = "0";
          //this.inverseMaximum = "unbounded";
        }

        [DataMember]
        [XmlElement(ElementName = "name")]
        public List<QMXFName> name { get; set; }

        [DataMember]
        [XmlElement(ElementName = "value")]
        public QMXFValue value { get; set; }

        [DataMember]
        [XmlElement(ElementName = "description")]
        public List<Description> description { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "qualifies")]
        public string qualifies { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "range")]
        public string range { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "minimum")]
        public string minimum { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "maximum")]
        public string maximum { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "inverse-minimum")]
        public string inverseMinimum { get; set; }
      
        [DataMember]
        [XmlAttribute(AttributeName = "inverse-maximum")]
        public string inverseMaximum { get; set; }

        public static IComparer<RoleQualification> sortAscending()
        {
            return (IComparer<RoleQualification>)new sortAscendingHelper();
        }

        private class sortAscendingHelper : IComparer<RoleQualification>
        {
            int IComparer<RoleQualification>.Compare(RoleQualification rq1, RoleQualification rq2)
            {
                return string.Compare(rq1.name[0].value, rq2.name[0].value);
            }
        }
    }

    [DataContract]
    public class Status
    {
        public Status()
        {
            this.authority = "http://rdl.rdlfacade.org/data#R6569332477";
            this.Class = "http://rdl.rdlfacade.org/data#R3732211754";
        }

        [DataMember]
        [XmlAttribute(AttributeName = "authority")]
        public string authority { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "from")]
        public string from { get; set; }

        [DataMember]
        [XmlAttribute(AttributeName = "to")]
        public string to { get; set; }
    }

}
