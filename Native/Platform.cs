using System.Runtime.InteropServices;

namespace Native;

internal static class Platform
{
#if Windows
    internal const string LoaderLibrary = "kernel32.dll";
    internal const string GLFWLibrary = "glfw3.dll";
    internal const string OpenGLLibrary = "opengl32.dll";
    internal const CallingConvention DefaultCallingConvention = CallingConvention.Winapi;
    internal const string GLEntryPoint = "wglGetProcAddress";
#elif OSX
    internal const string LoaderLibrary = "libSystem.dylib";
    internal const string GLFWLibrary = "libglfw.3.dylib";
    internal const string OpenGLLibrary = "libGL.dylib";
    internal const CallingConvention DefaultCallingConvention = CallingConvention.Cdecl;
    internal const string GLEntryPoint = "glXGetProcAddress";
#else
    internal const string LoaderLibrary = "libdl.so";
    internal const string GLFWLibrary = "libGL.so";
    internal const string OpenGLLibrary = "libglfw.so";
    internal const CallingConvention DefaultCallingConvention = CallingConvention.Cdecl;
    internal const string GLEntryPoint = "glXGetProcAddress";
#endif
}