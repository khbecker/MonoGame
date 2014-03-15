#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
* Copyright 2009-2014 Ethan Lee and the MonoGame Team
*
* Released under the Microsoft Public License.
* See LICENSE for details.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Microsoft.Xna.Framework.Content
{
	public static class ContentExtensions
	{
		public static ConstructorInfo GetDefaultConstructor(this Type type)
		{
			BindingFlags attrs = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
			return type.GetConstructor(attrs, null, new Type[0], null);
		}

		public static PropertyInfo[] GetAllProperties(this Type type)
		{

			// Sometimes, overridden properties of abstract classes can show up even with
			// BindingFlags.DeclaredOnly is passed to GetProperties. Make sure that
			// all properties in this list are defined in this class by comparing
			// its get method with that of it's base class. If they're the same
			// Then it's an overridden property.
			BindingFlags attrs = (
				BindingFlags.NonPublic |
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.DeclaredOnly
			);
			List<PropertyInfo> allProps = type.GetProperties(attrs).ToList();
			PropertyInfo[] props = allProps.FindAll(
				p => p.GetGetMethod(true) == p.GetGetMethod(true).GetBaseDefinition()
			).ToArray();
			return props;
		}

		public static FieldInfo[] GetAllFields(this Type type)
		{
			BindingFlags attrs = (
				BindingFlags.NonPublic |
				BindingFlags.Public |
				BindingFlags.Instance |
				BindingFlags.DeclaredOnly
			);
			return type.GetFields(attrs);
		}
	}
}
