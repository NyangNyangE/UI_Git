using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace PanicCall
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class Marshaling
    {
        public static void StructToBytes(object obj, ref byte[] packet)
        {
            int size = Marshal.SizeOf(obj);
            packet = new byte[size];
            IntPtr buffer = Marshal.AllocHGlobal(size + 1);
            Marshal.StructureToPtr(obj, buffer, false);
            Marshal.Copy(buffer, packet, 0, size);
            Marshal.FreeHGlobal(buffer);
        }

        public static void BytesToStructure(byte[] bValue, ref object obj, Type t)
        {
            int size = Marshal.SizeOf(t);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            Marshal.Copy(bValue, 0, buffer, size);
            obj = Marshal.PtrToStructure(buffer, t);
            Marshal.FreeHGlobal(buffer);
        }
    }
}
