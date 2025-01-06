using System.Runtime.InteropServices;

namespace Native;

internal static class DynamicLoader
{
    #if Windows
    internal static class Loader
    {
        
        [DllImport(Platform.LoaderLibrary, CallingConvention = Platform.DefaultCallingConvention, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibrary(string libraryName);

        [DllImport(Platform.LoaderLibrary, CallingConvention = Platform.DefaultCallingConvention, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport(Platform.LoaderLibrary, CallingConvention = Platform.DefaultCallingConvention, SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);
        
        internal static IntPtr Load(string name)
        {
            return LoadLibrary(name);
        }

        internal static bool Free(IntPtr handle)
        {
            return FreeLibrary(handle);
        }

        internal static T? GetSymbol<T>(IntPtr handle, string name) where T : Delegate
        {
            return Marshal.GetDelegateForFunctionPointer(GetProcAddress(handle, name), typeof(T)) as T;
        }
    }
    #else
    internal class Loader
    {
        [DllImport(Platform.LoaderLibrary, CallingConvention = Platform.DefaultCallingConvention, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr dlopen(string filename, int flags);

        [DllImport(Platform.LoaderLibrary, CallingConvention = Platform.DefaultCallingConvention, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport(Platform.LoaderLibrary, CallingConvention = Platform.DefaultCallingConvention, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int dlclose(IntPtr handle);
        
        internal static IntPtr Load(string name)
        {
            return dlopen(name, 0);
        }

        internal static bool Free(IntPtr handle)
        {
            return dlclose(handle) > 0;
        }

        internal static T? GetSymbol<T>(IntPtr handle, string name) where T : Delegate
        { 
            return Marshal.GetDelegateForFunctionPointer(dlsym(handle, name), typeof(T)) as T;
        }
    }
    #endif
}