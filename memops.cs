using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace tomek_cswpf_notes.memoryops
{
    public static class memops
    {

        static unsafe void CpBlk(void* dest, void* src, uint count)
        {
            var local = _cpBlk;
            local(dest, src, count);
        }

        static CopyBlockDelegate GenerateCpBlk()
        {
            var method = new DynamicMethod("CopyBlockIL", typeof(void), new[] { typeof(void*), typeof(void*), typeof(uint) }, typeof(memops));
            var emitter = method.GetILGenerator();
            // emit IL
            emitter.Emit(OpCodes.Ldarg_0);
            emitter.Emit(OpCodes.Ldarg_1);
            emitter.Emit(OpCodes.Ldarg_2);
            emitter.Emit(OpCodes.Cpblk);
            emitter.Emit(OpCodes.Ret);
            // compile to delegate
            return (CopyBlockDelegate)method.CreateDelegate(typeof(CopyBlockDelegate));
        }

        unsafe delegate void CopyBlockDelegate(void* des, void* src, uint bytes);
        static unsafe readonly CopyBlockDelegate _cpBlk = GenerateCpBlk();

        public unsafe static void memcpyblk(IntPtr dst, IntPtr src, uint length)
        {
            CpBlk((void*)dst, (void*)src, length);
        }

        [DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void memcpy(IntPtr Destination, IntPtr Source, uint Length);


    }
}
