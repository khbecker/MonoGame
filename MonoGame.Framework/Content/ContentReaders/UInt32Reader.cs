#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

using System;
namespace Microsoft.Xna.Framework.Content
{
	internal class UInt32Reader : ContentTypeReader<uint>
	{
		internal UInt32Reader()
		{
		}

		protected internal override uint Read(
			ContentReader input,
			uint existingInstance
		) {
			return input.ReadUInt32();
		}
	}
}
