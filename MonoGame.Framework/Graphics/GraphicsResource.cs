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

namespace Microsoft.Xna.Framework.Graphics
{	
	public abstract class GraphicsResource : IDisposable
	{
		bool disposed;
        
        // Resources may be added to and removed from the list from many threads.
        static object resourcesLock = new object();

        // Use WeakReference for the global resources list as we do not know when a resource
        // may be disposed and collected. We do not want to prevent a resource from being
        // collected by holding a strong reference to it in this list.
        static List<WeakReference> resources = new List<WeakReference>();

        // The GraphicsDevice property should only be accessed in Dispose(bool) if the disposing
        // parameter is true. If disposing is false, the GraphicsDevice may or may not be
        // disposed yet.
		GraphicsDevice graphicsDevice;

		internal GraphicsResource()
        {
            lock (resourcesLock)
            {
                resources.Add(new WeakReference(this));
            }
        }

        ~GraphicsResource()
        {
            // Pass false so the managed objects are not released
            // FIXME: How, I say, how in the fuck was this supposed to work? -flibit
            // Dispose(false);
        }

        /// <summary>
        /// Called before the device is reset. Allows graphics resources to 
        /// invalidate their state so they can be recreated after the device reset.
        /// Warning: This may be called after a call to Dispose() up until
        /// the resource is garbage collected.
        /// </summary>
        internal protected virtual void GraphicsDeviceResetting()
        {

        }

        internal static void DoGraphicsDeviceResetting()
        {
            lock (resourcesLock)
            {
                foreach (var resource in resources)
                {
                    var target = resource.Target;
                    if (target != null)
                        (target as GraphicsResource).GraphicsDeviceResetting();
                }

                // Remove references to resources that have been garbage collected.
                resources.RemoveAll(wr => !wr.IsAlive);
            }
        }

        /// <summary>
        /// Dispose all graphics resources remaining in the global resources list.
        /// </summary>
        internal static void DisposeAll()
        {
            lock (resourcesLock)
            {
                foreach (var resource in resources)
                {
                    var target = resource.Target;
                    if (target != null)
                        (target as IDisposable).Dispose();
                }
                resources.Clear();
            }
        }

		public void Dispose()
        {
            // Dispose of managed objects as well
            Dispose(true);
            // Since we have been manually disposed, do not call the finalizer on this object
            GC.SuppressFinalize(this);
        }
		
        /// <summary>
        /// The method that derived classes should override to implement disposing of managed and native resources.
        /// </summary>
        /// <param name="disposing">True if managed objects should be disposed.</param>
        /// <remarks>Native resources should always be released regardless of the value of the disposing parameter.</remarks>
        protected virtual void Dispose(bool disposing)
        {
            // FIXME: What was this? No, really, what? -flibit
            //if (!disposed)
            //{
                //if (disposing)
                //{
                    // Release managed objects
                    // ...
                //}

                // Release native objects
                // ...

                // Do not trigger the event if called from the finalizer
                if (disposing && Disposing != null)
                    Disposing(this, EventArgs.Empty);

                // Remove from the global list of graphics resources
                lock (resourcesLock)
                {
                    resources.Remove(new WeakReference(this));
                }

                graphicsDevice = null;
                disposed = true;
            //}
        }

		public event EventHandler<EventArgs> Disposing;
		
		public GraphicsDevice GraphicsDevice
		{
			get
			{
				return graphicsDevice;
			}

            internal set
            {
                graphicsDevice = value;
            }
		}
		
		public bool IsDisposed
		{
			get
			{
				return disposed;
			}
		}
		
		public string Name { get; set; }
		
		public Object Tag { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? base.ToString() : Name;
        }
	}
}

