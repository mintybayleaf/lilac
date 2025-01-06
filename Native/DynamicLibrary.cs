using System.Runtime.InteropServices;

namespace Native;

public class DynamicLibrary(string library) : SafeHandle(IntPtr.Zero, true)
{
        public string Library { get; } = library;
        
        private IntPtr Handle { get; } = DynamicLoader.Loader.Load(library);

        public override bool IsInvalid => Handle == IntPtr.Zero;


        protected override bool ReleaseHandle()
        {
            return !IsInvalid && DynamicLoader.Loader.Free(Handle);
        }

        public T? GetFunction<T>(string symbol) where T : Delegate
        {
            return DynamicLoader.Loader.GetSymbol<T>(Handle, symbol);
        }
}