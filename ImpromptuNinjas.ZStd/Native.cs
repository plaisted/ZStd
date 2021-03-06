using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ImpromptuNinjas.ZStd {

  public static partial class Native {

    private const string LibName = "libzstd";

    internal static string LibPath;

    private static readonly unsafe Lazy<IntPtr> LazyLoadedLib = new Lazy<IntPtr>(() => {
      var asm = typeof(Native).GetAssembly();
      var baseDir = asm.GetLocalCodeBaseDirectory();

      var ptrBits = sizeof(void*) * 8;

      // ReSharper disable once RedundantAssignment
      IntPtr lib = default;

#if NETFRAMEWORK
      LibPath = Path.Combine(baseDir, "libzstd.dll");
      if (!File.Exists(LibPath))
        LibPath = Path.Combine(baseDir, $"libzstd{ptrBits}.dll");
#else
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
        LibPath = Path.Combine(baseDir, "libzstd.dll");
        if (!TryLoad(LibPath, out lib))
          LibPath = Path.Combine(baseDir, "runtimes", ptrBits == 32 ? "win-x86" : "win-x64", "native", "libzstd.dll");
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
        LibPath = Path.Combine(baseDir, "libzstd.dylib");
        if (!TryLoad(LibPath, out lib))
          LibPath = Path.Combine(baseDir, "runtimes", "osx-x64", "native", "libzstd.dylib");
      }
      else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
        LibPath = Path.Combine(baseDir, "libzstd.so");
        if (!TryLoad(LibPath, out lib))
          LibPath = Path.Combine(baseDir, "runtimes", $"{(IsMusl() ? "linux-musl-" : "linux-")}{GetProcArchString()}", "native", "libzstd.so");
      }
      else throw new PlatformNotSupportedException();
#endif

      // ReSharper disable once InvertIf
      if (lib == default && !TryLoad(LibPath, out lib)) {
#if !NETSTANDARD1_1
        if (File.Exists(LibPath))
          throw new UnauthorizedAccessException(LibPath);
#endif
#if !NETFRAMEWORK
        throw new DllNotFoundException(LibPath);
#else
        throw new FileNotFoundException(LibPath + "\n" +
          $"You may need to specify <RuntimeIdentifier>{(ptrBits == 32 ? "win-x86" : "win-x64")}<RuntimeIdentifier> or <RuntimeIdentifier>win<RuntimeIdentifier> in your project file.",
          LibPath);
#endif
      }

      return lib;
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    private static bool TryLoad(string libPath, out IntPtr lib) {
      try {
        lib = NativeLibrary.Load(libPath);
      }
      catch {
        lib = default;
        return false;
      }

      return true;
    }

    public static IntPtr Lib => LazyLoadedLib.Value;

    static Native()
      => NativeLibrary.SetDllImportResolver(typeof(Native).GetAssembly(),
        (name, assembly, path)
          => {
          if (name != LibName)
            return default;

          Debug.Assert(Lib != default);
          return Lib;
        });

    internal static void Init()
      => Debug.Assert(Lib != default);

  }

}
