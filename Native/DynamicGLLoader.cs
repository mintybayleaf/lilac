using System.Runtime.InteropServices;

namespace Native;

internal static class OpenGLLoader
{
    internal static class Loader
    {
        [DllImport(Platform.OpenGLLibrary, CallingConvention = Platform.DefaultCallingConvention, EntryPoint = Platform.GLEntryPoint, SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr glGetProcAddress(string name);
        
        // TODO: Update loader functions to fallback to Loader.GetProcAddress from the OpenGL Handle
        
        /// <summary>
        /// The OpenGL document (https://www.khronos.org/opengl/wiki/Load_OpenGL_Functions) says we should
        /// use the default OpenGL Loader functions and fallback to looking for the exported DLL symbols on the
        /// OpenGL DLL itself for older compatibiltity mode functions (on some systems) like windows
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T? GetSymbol<T>(string name) where T : Delegate
        {
            return Marshal.GetDelegateForFunctionPointer(glGetProcAddress(name), typeof(T)) as T;
        }
    }
}