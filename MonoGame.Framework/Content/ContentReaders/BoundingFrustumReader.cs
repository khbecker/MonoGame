#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Content
{
	internal class BoundingFrustumReader : ContentTypeReader<BoundingFrustum>
	{
		internal BoundingFrustumReader()
		{
		}

		protected internal override BoundingFrustum Read(
			ContentReader input,
			BoundingFrustum existingInstance
		) {
			return new BoundingFrustum(input.ReadMatrix());
		}
	}
}
