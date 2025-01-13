namespace GLGen;

public class AssemblyGenerator
{
    /*
     * TODO
     * This class will generate the assembly file with the parsed OpenGL Spec XML
     * The file will have code to load the native symbol loading libraries on OSX, Windows & Linux using the
     * NativeLibrary class if possible and DLLImportResolution
     *
     * Add code to load core functions and opengl specific functions
     *
     * Generate the typedefs types using `using` aliases
     * Generate the enumerations
     * Generate the callback handles to be assigned the loaded callbacks
     * Generate delegates to cast the callbacks too
     * Generate the C# functions that will proxy execution to the loaded function pointers
     * Generate an init/load function to load all the callbacks whenever the user wants
     * I could do enumerations for the types so we can get strong typing if I am feeling it I guess
     */
}