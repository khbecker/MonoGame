#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
* Copyright 2009-2014 Ethan Lee and the MonoGame Team
*
* Released under the Microsoft Public License.
* See LICENSE for details.
*/
#endregion

using System;
using Microsoft.Xna.Framework.Graphics;
namespace Microsoft.Xna.Framework.Content
{
	public class VertexDeclarationReader : ContentTypeReader<VertexDeclaration>
	{
		protected internal override VertexDeclaration Read(
			ContentReader reader,
			VertexDeclaration existingInstance
		) {
			var vertexStride = reader.ReadInt32();
			var elementCount = reader.ReadInt32();
			VertexElement[] elements = new VertexElement[elementCount];
			for (int i = 0; i < elementCount; i += 1)
			{
				var offset = reader.ReadInt32();
				var elementFormat = (VertexElementFormat)reader.ReadInt32();
				var elementUsage = (VertexElementUsage)reader.ReadInt32();
				var usageIndex = reader.ReadInt32();
				elements[i] = new VertexElement(
					offset,
					elementFormat,
					elementUsage,
					usageIndex
				);
			}

			// TODO: This process generates alot of duplicate VertexDeclarations
			// which in turn complicates other systems trying to share GPU resources
			// like DX11 vertex input layouts.
			//
			// We should consider caching vertex declarations here and returning
			// previously created declarations when they are in our cache.
			return new VertexDeclaration(vertexStride, elements);
		}
	}
}

