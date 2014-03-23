#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2014 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#endregion

namespace Microsoft.Xna.Framework.Graphics
{

	public sealed class ModelMeshPartCollection : ReadOnlyCollection<ModelMeshPart>
	{
		public ModelMeshPartCollection(IList<ModelMeshPart> list)
			: base(list)
		{

		}
	}

	//// Summary:
	////     Represents a collection of ModelMeshPart objects.
	//public sealed class ModelMeshPartCollection : ReadOnlyCollection<ModelMeshPart>
	//{
	//    internal ModelMeshPartCollection()
	//        : base(new List<ModelMeshPart>())
	//    {
	//    }

	//    // Summary:
	//    //     Returns a ModelMeshPartCollection.Enumerator that can iterate through a ModelMeshPartCollection.
	//    public ModelMeshPartCollection.Enumerator GetEnumerator() { throw new NotImplementedException(); }

	//    // Summary:
	//    //     Provides the ability to iterate through the bones in an ModelMeshPartCollection.
	//    public struct Enumerator : IEnumerator<ModelMeshPart>, IDisposable, IEnumerator
	//    {

	//        // Summary:
	//        //     Gets the current element in the ModelMeshPartCollection.
	//        public ModelMeshPart Current { get { throw new NotImplementedException(); } }

	//        // Summary:
	//        //     Immediately releases the unmanaged resources used by this object.
	//        public void Dispose() { throw new NotImplementedException(); }
	//        //
	//        // Summary:
	//        //     Advances the enumerator to the next element of the ModelMeshPartCollection.
	//        public bool MoveNext() { throw new NotImplementedException(); }

	//        #region IEnumerator Members

	//        object IEnumerator.Current
	//        {
	//            get { throw new NotImplementedException(); }
	//        }

	//        public void Reset()
	//        {
	//            throw new NotImplementedException();
	//        }

	//        #endregion
	//    }
	//}
}
