using System.Runtime.InteropServices;

namespace HunterPie.Core.Definitions
{
    [StructLayout(LayoutKind.Sequential)]
    public struct sVector3
    {
        public float X;
        public float Y;
        public float Z;
    }

    public class Vector3
    {

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Z { get; private set; }

        /// <summary>
        /// Updates the current Vector values
        /// </summary>
        /// <param name="vec">sVector3 with the position values</param>
        public void Update(sVector3 vec)
        {
            X = vec.X;
            Y = vec.Y;
            Z = vec.Z;
        }

        public override string ToString() => $"X: {X}, Y: {Y}, Z: {Z}";

    }
}
