using System;
using System.Collections.Generic;
using System.Reflection;

namespace consoleapp
{
    class Program
    {
        static void Main(string[] args)
        {
            var objectSource = new ExampleRequest();
            var objectTemplate = new ExampleRequest
            {
                Name = "testName",
                IsAvailable = true,
                Dictionary = new Dictionary<ulong, string>{
                    {1, "item1"}
                },
                SubItem = new ExampleRequest
                {
                    Name = "subItemName"
                }
            };

            objectSource.CopyFrom(objectTemplate);

            Console.WriteLine("copied ");

            Console.WriteLine(objectSource.Name);
            Console.WriteLine(objectSource.IsAvailable);
            Console.WriteLine(objectSource.Dictionary);
            Console.WriteLine(objectSource.SubItem);
        }
    }

    public class ExampleRequest
    {
        delegate string GetName(string id);
        event GetName OnButtonClick;
        public string Name { get; set; }
        public int Number { get; set; }
        public bool IsAvailable { get; set; }
        public IDictionary<ulong, string> Dictionary { get; set; }
        public ExampleRequest SubItem { get; set; }
    }

    public static class MappingExtension
    {
        public static T CopyFrom<T>(this T source, object template){
            AutoMapper.Mapper.Initialize(config => config.CreateMap<T, T>());
            AutoMapper.Mapper.Map(template, source);
            return source;
        }
    }
}
