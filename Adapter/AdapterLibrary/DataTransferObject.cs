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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.ServiceModel;
using System.Linq;

namespace org.iringtools.adapter
{
    public class DataTransferObject
    {
        public List<ClassObject> classObjects { get; set; }

        public List<ClassObject> GetClassObjects(string classId)
        {
            List<ClassObject> classObjectList = new List<ClassObject>();
            foreach (ClassObject classObject in this.classObjects)
            {
                if (classObject.classId == classId)
                {
                    classObjectList.Add(classObject);
                }
            }
            return classObjectList;
        }
    }

    public class ClassObject
    {
        public string classId { get; set; }
        public string identifier { get; set; }
        public List<TemplateObject> templateObjects  { get; set; }

        public List<TemplateObject> GetTemplateObjects(string templateId)
        {
            List<TemplateObject> templateObjectList = new List<TemplateObject>();
            foreach (TemplateObject templateObject in this.templateObjects)
            {
                if (templateObject.templateId == templateId)
                {
                    templateObjectList.Add(templateObject);
                }
            }
            return templateObjectList;
        }
    }

    public class TemplateObject
    {
        public string templateId { get; set; }
        public string name { get; set; }
        public List<RoleObject> roleObjects { get; set; }
    }

    public class RoleObject
    {
        public string roleId { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public string reference { get; set; }
    }
}
 