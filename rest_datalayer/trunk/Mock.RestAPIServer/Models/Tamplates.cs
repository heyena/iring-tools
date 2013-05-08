using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mock.RestAPIServer.Models
{
    public class EndPoint
    {
        public string baseurl = "http://localhost:50637";
        public string schemaurl = "/api/schema/{resource}";
        public  IList<SingleEndPoint> resources;

        public EndPoint()
        {
            resources = new List<SingleEndPoint>();
            resources.Add(new SingleEndPoint() { resource = "Function", url = "/API/Function" });
            resources.Add(new SingleEndPoint() { resource = "Project", url = "/API/Projetc" });
        }


    }


    public class SingleEndPoint
    {
        public string resource { get; set; }
        public string  url { get; set; }
    }

    public class ObjectDefination
    {
        public string type;
        public string size;
    }

    public enum DataType
    {
        @string,
        number
    }

    public class Links
    {
        public IList<string> key;
        public string self;
        public IList<object> relation;
    }

    public class Function
    {
        public int Id;
        public string Name;
        
    }

    public class Project
    {
        public int Id;
        public string Name;
        public string Description;
        public DateTime UpdateOn;

    }

    public static class Utility
    {
        public static IList<Function> GetFunctions()
        {
            IList<Function> list = new List<Function>();
            list.Add(new Function() { Id = 1, Name = "function1" });
            list.Add(new Function() { Id = 2, Name = "function2" });
            list.Add(new Function() { Id = 3, Name = "GENERAL MANAGEMENT" });
            list.Add(new Function() { Id = 4, Name = "function4" });

            return list;
        }

        public static IList<Project> GetProjects()
        {
            IList<Project> list = new List<Project>();
            list.Add(new Project() { Id = 11, Name = "Project1", Description = "Description1", UpdateOn = System.DateTime.Now });
            list.Add(new Project() { Id = 12, Name = "Project2", Description = "Description2", UpdateOn = System.DateTime.Now });
            list.Add(new Project() { Id = 13, Name = "Project3", Description = "Description3", UpdateOn = System.DateTime.Now });
            list.Add(new Project() { Id = 14, Name = "Project4", Description = "Description4", UpdateOn = System.DateTime.Now });
            list.Add(new Project() { Id = 15, Name = "Project5", Description = "Description5", UpdateOn = System.DateTime.Now });

            return list;
        }
    }

    public class GenericObject<T>
    {

        public int total;
        public int limit;
        public IList<T> Items;
    }      
}