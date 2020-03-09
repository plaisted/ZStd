using System;
using JetBrains.Annotations;

namespace ImpromptuNinjas.ZStd {

  [PublicAPI]
  public static partial class Extensions {

    internal static UIntPtr EnsureZStdSuccess(this UIntPtr value) {
      if (Native.ZDict.IsError(value) != 0)
        ThrowException(value, Native.ZStd.GetErrorName(value));
      return value;
    }

    internal static UIntPtr EnsureZDictSuccess(this UIntPtr value) {
      if (Native.ZDict.IsError(value) != 0)
        ThrowException(value, Native.ZDict.GetErrorName(value));
      return value;
    }

    private static void ThrowException(UIntPtr value, string message) {
      var code = unchecked(0 - (uint) (ulong) value);
      throw new ZStdException(message, code);
    }

    internal static unsafe int CompareTo(this UIntPtr a, UIntPtr b)
      => sizeof(UIntPtr) == 8
        ? a.ToUInt64().CompareTo(b.ToUInt64())
        : a.ToUInt32().CompareTo(b.ToUInt32());

    internal static bool GreaterThan(this UIntPtr a, UIntPtr b)
      => a.CompareTo(b) > 0;

    internal static bool LessThan(this UIntPtr a, UIntPtr b)
      => a.CompareTo(b) < 0;

    internal static bool GreaterThanOrEqualTo(this UIntPtr a, UIntPtr b)
      => a.CompareTo(b) >= 0;

    internal static bool LessThanOrEqualTo(this UIntPtr a, UIntPtr b)
      => a.CompareTo(b) <= 0;
    internal static unsafe bool EqualTo(this UIntPtr a, int b)
      => sizeof(UIntPtr) == 8
        ? checked((long)a.ToUInt64()) == b
        : a.ToUInt32() == b;

    internal static unsafe bool EqualTo(this UIntPtr a, long b)
      => sizeof(UIntPtr) == 8
        ? checked((long)a.ToUInt64()) == b
        : a.ToUInt32() == b;

  }

}