#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
* Copyright 2009-2014 Ethan Lee and the MonoGame Team
*
* Released under the Microsoft Public License.
* See LICENSE for details.
*/
#endregion

namespace Microsoft.Xna.Framework.Content
{
	class BoundingBoxReader : ContentTypeReader<BoundingBox>
	{
		protected internal override BoundingBox Read(
			ContentReader input,
			BoundingBox existingInstance
		) {
			var result = new BoundingBox(
				input.ReadVector3(),
				input.ReadVector3()
			);
			return result;
		}
	}
}
