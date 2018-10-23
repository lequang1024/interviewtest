using System;
using System.Collections.Generic;
using System.Reflection;

namespace consoleapp
{
    class Program
    {
        static void Main(string[] args)
        {
            var copiedObject = new ExampleRequest();
            var objectTemplate = new ExampleRequest
            {
                BaseName = "base1",
                Name = "testName",
                Bool = true,
                Number = 13,
                NumberArray = new[] { 1, 3, 2 },
                NumberRefArray = new List<ulong> { 1, 3, 2 },
                Dictionary = new Dictionary<ulong, string>{
                    {1, "item1"}
                },
                SubItem = new ExampleRequest
                {
                    Name = "testNameSub",
                    Bool = true,
                    Number = 14,
                    NumberArray = new[] { 4, 5, 6 },
                    NumberRefArray = new List<ulong> { 7, 8, 9 },
                    Dictionary = new Dictionary<ulong, string>{
                        {133, "item1Sub"}
                    },
                    SubItem = new ExampleRequest
                    {
                        BaseName = "wa",
                        Name = "testNameSubSub",
                        Bool = true,
                        Number = 156,
                        NumberArray = new[] { 4, 5, 6, 7, 8, 9 },
                        NumberRefArray = new List<ulong> { 7, 8, 9, 4, 5, 6 },
                        Dictionary = new Dictionary<ulong, string>{
                            {133, "item1SubSub"},
                         },
                        RequestArray = new[] {
                             new ExampleRequest{
                                 Name="reqinArray1"
                             },
                               new ExampleRequest{
                                 Name="reqinArray2"
                             },
                        }
                    },
                },
            };
            objectTemplate.BaseName = "changedBaseName";

            copiedObject.CopyFrom(objectTemplate);

            Console.WriteLine("source--------------------------------------------------------- \n");
            WriteObjectToConsole(objectTemplate);
            Console.WriteLine();
            Console.WriteLine("copied--------------------------------------------------------- \n");
            WriteObjectToConsole(copiedObject);
            Console.WriteLine();
            Console.WriteLine("references--------------------------------------------------------- \n");
            Console.WriteLine(Object.ReferenceEquals(objectTemplate.SubItem, copiedObject.SubItem));
            Console.WriteLine(Object.ReferenceEquals(objectTemplate.Dictionary, copiedObject.Dictionary));
            Console.WriteLine(Object.ReferenceEquals(objectTemplate.NumberRefArray, copiedObject.NumberRefArray));
            Console.WriteLine(Object.ReferenceEquals(objectTemplate.BaseName, copiedObject.BaseName));
            Console.WriteLine(Object.ReferenceEquals(objectTemplate.SubItem.SubItem, copiedObject.SubItem.SubItem));
            Console.WriteLine(Object.ReferenceEquals(objectTemplate.SubItem.SubItem.RequestArray, copiedObject.SubItem.SubItem.RequestArray));

        }

        static void WriteObjectToConsole(ExampleRequest request)
        {
            Console.WriteLine();
            Console.WriteLine(request.BaseName);
            Console.WriteLine(request.Name);
            Console.WriteLine(request.Bool);
            Console.WriteLine(request.Number);
            if (request.NumberArray != null)
                Console.WriteLine(string.Join(",", request.NumberArray));
            if (request.NumberRefArray != null)
                Console.WriteLine(string.Join(",", request.NumberRefArray));
            if (request.Dictionary != null)
                Console.WriteLine(request.Dictionary);
            if (request.SubItem != null)
            {
                Console.WriteLine("Subitem------------------- \n");
                WriteObjectToConsole(request.SubItem);

            }
            if (request.RequestArray != null)
            {
                Console.WriteLine("Request in array------------------- \n");
                foreach (var requestInArr in request.RequestArray)
                {
                    WriteObjectToConsole(requestInArr);
                }

            }
            Console.WriteLine();
        }
    }

    public class ExampleRequest : ExampleRequestBase
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public bool Bool { get; set; }
        public int[] NumberArray { get; set; }
        public ExampleRequest[] RequestArray { get; set; }
        public List<ulong> NumberRefArray { get; set; }
        public IDictionary<ulong, string> Dictionary { get; set; }
        public ExampleRequest SubItem { get; set; }
    }
    public abstract class ExampleRequestBase
    {
        string _baseName = "privateBaseName";
        public string BaseName { get { return _baseName; } set { _baseName = value; } }
    }
}

namespace System
{
    public static class ObjectExtensions
    {
        private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

        private static void RecursiveCopy(Object destinationObject, Object originalObject)
        {
            if (originalObject == null) destinationObject = null;
            var typeToReflect = originalObject.GetType();
            if (IsPrimitive(destinationObject.GetType()))
            {
                destinationObject = originalObject;
                return;
            }
            CopyFields(originalObject, destinationObject, typeToReflect);
            CopyBaseTypeFields(originalObject, destinationObject, typeToReflect);
        }

        private static void CopyBaseTypeFields(object originalObject, object cloneObject, Type typeToReflect)
        {
            if (typeToReflect.BaseType != null)
            {
                CopyBaseTypeFields(originalObject, cloneObject, typeToReflect.BaseType);
                CopyFields(originalObject, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
            }
        }

        private static void CopyFields(object originalObject, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
        {
            foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
            {
                var originalFieldValue = fieldInfo.GetValue(originalObject);
                if (originalFieldValue == null)
                {
                    continue;
                }
                object clonedFieldValue;
                if (IsPrimitive(originalFieldValue.GetType()))
                {
                    clonedFieldValue = originalFieldValue;
                }
                else
                {
                    clonedFieldValue = CloneMethod.Invoke(originalFieldValue, null);
                }
                fieldInfo.SetValue(cloneObject, clonedFieldValue);
                RecursiveCopy(clonedFieldValue, originalFieldValue);
            }
        }

        static bool IsPrimitive(this Type type)
        {
            if (type == typeof(String)) return true;
            return (type.IsValueType & type.IsPrimitive);
        }
        public static void CopyFrom<T>(this T destination, T source)
        {
            RecursiveCopy(destination, source);
        }
    }
}