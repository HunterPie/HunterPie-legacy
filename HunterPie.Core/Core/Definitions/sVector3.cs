using System;
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

    /// <summary>
    /// Represents a 3D vector with the position of an element in a 3D space
    /// </summary>
    public class Vector3
    {

        /// <summary>
        /// X coordinate in a 3D space
        /// </summary>
        public float X { get; private set; }

        /// <summary>
        /// Y coordinate in a 3D space
        /// </summary>
        public float Y { get; private set; }

        /// <summary>
        /// Z coordinate in a 3D space
        /// </summary>
        public float Z { get; private set; }

        /// <summary>
        /// Initialized a new Vector3 object
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="z">Z Coordinate</param>
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

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

        /// <summary>
        /// Calculates the distance between two <see cref="Vector3"/>
        /// </summary>
        /// <param name="other">Another Vector3</param>
        /// <returns></returns>
        public float Distance(Vector3 other)
        {
            float dx = (float)Math.Pow(X - other.X, 2);
            float dy = (float)Math.Pow(Y - other.Y, 2);
            float dz = (float)Math.Pow(Z - other.Z, 2);
            return (float)Math.Sqrt(dx + dy + dz);
        }

        #region Overloading & Overriding

        /// <summary>
        /// Compares two <see cref="Vector3"/> and returns true if they're equal
        /// </summary>
        /// <param name="left">A Vector3 to compare</param>
        /// <param name="right">Another Vector3 to compare</param>
        /// <returns>True if their coordinates are equal, false otherwise</returns>
        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return (left.X == right.X && left.Y == right.Y && left.Z == right.Z);
        }

        /// <summary>
        /// Compares two <see cref="Vector3"/> and returns true if they're different
        /// </summary>
        /// <param name="left">A Vector3 to compare</param>
        /// <param name="right">Another Vector3 to compare</param>
        /// <returns>True if their coordinates are different, false otherwise</returns>
        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Multiplies two <see cref="Vector3"/>
        /// </summary>
        /// <param name="left">A Vector3 to multiply</param>
        /// <param name="right">Another Vector3 to multiply</param>
        /// <returns>A new Vector3 with the multiplied coordinates</returns>
        public static Vector3 operator *(Vector3 left, Vector3 right)
        {
            return new Vector3(left.X * right.X, left.Y * right.Y, left.Z * right.Z);
        }

        public override string ToString() => $"<Vector3 (X: {X}, Y: {Y}, Z: {Z})>";
        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
