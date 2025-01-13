// See https://aka.ms/new-console-template for more information

using System.Reflection;

namespace GLGen;

public static class App
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Application!");
        var glSpecPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"specs/gl.xml");
        var glSpec = GLXml.From(glSpecPath);
    }
}