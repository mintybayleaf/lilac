// See https://aka.ms/new-console-template for more information

using System.Reflection;

namespace GLGen;

public static class GLAD
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Application!");
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"specs/gl.xml");
        var glSpec = GLXML.from(path);
        foreach (var e in glSpec.Enums)
        {
            var key = e.Key;
            var value = e.Value;
            foreach (var glEnum in value)
            {
                Console.WriteLine($"Group: {key} => {glEnum}");
            }
        }
        Console.WriteLine(glSpec.Enums.Keys.Count);
        Console.WriteLine("Ending Application!");
    }
}