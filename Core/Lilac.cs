// See https://aka.ms/new-console-template for more information
using Native;

namespace Core;

public static class Lilac
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Starting Application!");
        DynamicLibrary library = new DynamicLibrary("kernel32.dll");
        Console.WriteLine(library.IsInvalid);
        Console.WriteLine("Ending Application!");
    }
}