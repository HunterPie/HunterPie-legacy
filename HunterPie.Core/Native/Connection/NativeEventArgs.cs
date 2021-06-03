using System.Runtime.InteropServices;
using HunterPie.Native.Connection.Packets;

namespace HunterPie.Native.Connection
{
    public class NativeEventArgs
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct NetworkData
        {
            [FieldOffset(0)]
            public S_QUEUE_INPUT input;
            [FieldOffset(0)]
            public S_DEAL_DAMAGE damage;
        }

        public NetworkData Data { get; }

        public NativeEventArgs(object data)
        {
            
        }

        public NativeEventArgs(S_QUEUE_INPUT data)
        {
            Data = new NetworkData
            {
                input = data
            };
        }

        public NativeEventArgs(S_DEAL_DAMAGE data)
        {
            Data = new NetworkData
            {
                damage = data
            };
        }

        public static explicit operator S_QUEUE_INPUT(NativeEventArgs args) => args.Data.input;
        public static explicit operator S_DEAL_DAMAGE(NativeEventArgs args) => args.Data.damage;
    }
}
