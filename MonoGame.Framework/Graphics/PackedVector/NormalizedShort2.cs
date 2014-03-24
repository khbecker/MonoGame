#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	public struct NormalizedShort2 : IPackedVector<uint>, IEquatable<NormalizedShort2>
    {
        #region Public Properties

        [CLSCompliant(false)]
        public uint PackedValue
        {
            get
            {
                return short2Packed;
            }
            set
            {
                short2Packed = value;
            }
        }

        #endregion

        #region Private Variables

        private uint short2Packed;

        #endregion

        #region Public Constructors

        public NormalizedShort2(Vector2 vector)
		{
            short2Packed = PackInTwo(vector.X, vector.Y);
		}

        public NormalizedShort2(float x, float y)
		{
            short2Packed = PackInTwo(x, y);
		}

        #endregion

        #region Public Methods

        public Vector2 ToVector2()
        {
            const float maxVal = 0x7FFF;

            var v2 = new Vector2();
            v2.X = ((short)(short2Packed & 0xFFFF)) / maxVal;
            v2.Y = (short)(short2Packed >> 0x10) / maxVal;
            return v2;
        }

        #endregion

        #region Private Methods

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            short2Packed = PackInTwo(vector.X, vector.Y);
        }

        Vector4 IPackedVector.ToVector4()
        {
            const float maxVal = 0x7FFF;

            var v4 = new Vector4(0, 0, 0, 1);
            v4.X = ((short)((short2Packed >> 0x00) & 0xFFFF)) / maxVal;
            v4.Y = ((short)((short2Packed >> 0x10) & 0xFFFF)) / maxVal;
            return v4;
        }

        #endregion

        #region Public Static Operators and Override Methods

        public static bool operator !=(NormalizedShort2 a, NormalizedShort2 b)
		{
			return !a.Equals (b);
		}

        public static bool operator ==(NormalizedShort2 a, NormalizedShort2 b)
		{
			return a.Equals (b);
		}

        public override bool Equals (object obj)
		{
            return (obj is NormalizedShort2) && Equals((NormalizedShort2)obj);
		}

        public bool Equals(NormalizedShort2 other)
		{
            return short2Packed.Equals(other.short2Packed);
		}

		public override int GetHashCode ()
		{
			return short2Packed.GetHashCode();
		}

		public override string ToString ()
		{
            return short2Packed.ToString("X");
		}

        #endregion

        #region Private Static Methods

        private static uint PackInTwo (float vectorX, float vectorY)
		{
			const float maxPos = 0x7FFF;
            const float minNeg = -maxPos;

			// clamp the value between min and max values
            // Round rather than truncate.
            var word2 = (uint)((int)MathHelper.Clamp((float)Math.Round(vectorX * maxPos), minNeg, maxPos) & 0xFFFF);
            var word1 = (uint)(((int)MathHelper.Clamp((float)Math.Round(vectorY * maxPos), minNeg, maxPos) & 0xFFFF) << 0x10);

			return (word2 | word1);
		}

        #endregion
    }
}
