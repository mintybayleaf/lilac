// See https://aka.ms/new-console-template for more information

using System.Reflection;

namespace GLGen;

public static class App
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Application!");
        var glSpecPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"specs/gl.xml");
        var glSpec = GLXML.From(glSpecPath);
        Console.WriteLine(glSpec.GLFunctions["glAccum"]);
        Console.WriteLine(string.Join("\t\n", glSpec.GLFunctions["glAccum"].Parameters));
        Console.WriteLine(glSpec.GLEnums["GL_SAMPLE_SHADING"]);
        Console.WriteLine(glSpec.GLEnums["GL_SAMPLE_SHADING"].Value);
        Console.WriteLine(glSpec.GLFeatures);
        foreach (var feature in glSpec.GLFeatures)
        {
            Console.WriteLine(feature.Key + ": " + feature.Value);
            foreach (var f in feature.Value)
            {
                Console.WriteLine(string.Join(",", f.Commands));
                Console.WriteLine(string.Join(",", f.Enums));
            }
        };
        Console.WriteLine("Ending Application!");
    }
}