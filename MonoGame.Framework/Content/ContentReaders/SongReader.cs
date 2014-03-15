#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
* Copyright 2009-2014 Ethan Lee and the MonoGame Team
*
* Released under the Microsoft Public License.
* See LICENSE for details.
*/
#endregion

using System;
using System.IO;

using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Framework.Content
{
	internal class SongReader : ContentTypeReader<Song>
	{
#if SDL2
		static string[] supportedExtensions = new string[] { ".flac", ".ogg" };
#else
		static string[] supportedExtensions = new string[] { ".mp3" };
#endif

		internal static string Normalize(string fileName)
		{
			return Normalize(fileName, supportedExtensions);
		}

		protected internal override Song Read(ContentReader input, Song existingInstance)
		{
			var path = input.ReadString();
			if (!String.IsNullOrEmpty(path))
			{
				const char notSeparator = '\\';
				var separator = Path.DirectorySeparatorChar;
				path = path.Replace(notSeparator, separator);
				// Get a uri for the asset path using the file:// schema and no host
				var src = new Uri(
					"file:///" +
					input.AssetName.Replace(notSeparator, separator)
				);
				// Add the relative path to the external reference
				var dst = new Uri(src, path);
				// The uri now contains the path to the external reference within
				// the content manager
				// Get the local path and skip the first character
				// (the path separator)
				path = dst.LocalPath.Substring(1);
				// Adds the ContentManager's RootDirectory
				path = Path.Combine(
					input.ContentManager.RootDirectoryFullPath,
					path
				);
			}
			var durationMs = input.ReadObject<int>();
			return new Song(path, durationMs);
		}
	}
}
