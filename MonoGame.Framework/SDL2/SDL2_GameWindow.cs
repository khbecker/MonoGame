#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

// #define THREADED_GL
/* Ah, so I see you've run into some issues with threaded GL...
 * 
 * We use Threading.cs to handle rendering coming from multiple threads, but if
 * you're too wreckless with how many threads are calling the GL, this will
 * hang.
 *
 * With THREADED_GL we instead allow you to run threaded rendering using
 * multiple GL contexts. This is more flexible, but much more dangerous.
 *
 * Also note that this affects Threading.cs! Check THREADED_GL there too.
 *
 * Basically, if you have to enable this, you should feel very bad.
 * -flibit
 */

using System;
using System.ComponentModel;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using SDL2;
using OpenTK.Graphics.OpenGL;

namespace Microsoft.Xna.Framework
{
    public class SDL2_GameWindow : GameWindow
    {
        #region The Game
        
        internal Game Game;
        
        #endregion
        
        #region Internal SDL2 window variables
        
        private IntPtr INTERNAL_sdlWindow;
        private string INTERNAL_sdlWindowTitle;
        
        private SDL.SDL_WindowFlags INTERNAL_sdlWindowFlags_Current;
        private SDL.SDL_WindowFlags INTERNAL_sdlWindowFlags_Next;
        
        private IntPtr INTERNAL_GLContext;
        
        private string INTERNAL_deviceName;
        
        #endregion
        
        #region Internal OpenGL Framebuffer
        
        private int INTERNAL_glFramebuffer;
        private int INTERNAL_glColorAttachment;
        private int INTERNAL_glDepthStencilAttachment;
        private int INTERNAL_glFramebufferWidth;
        private int INTERNAL_glFramebufferHeight;
        
        private DepthFormat INTERNAL_depthFormat;
        
        #endregion
        
        #region Internal Loop Sentinel
        
        private bool INTERNAL_runApplication;
        
        #endregion
        
        #region Private Active XNA Key List
        
        private List<Keys> keys;
        
        #endregion
        
        #region Public Properties
        
        [DefaultValue(false)]
        public override bool AllowUserResizing
        {
            get
            {
                return (INTERNAL_sdlWindowFlags_Current & SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE) != SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            }
            set
            {
                // Note: This can only be used BEFORE window creation!
                if (value)
                {
                    INTERNAL_sdlWindowFlags_Next |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
                }
                else
                {
                    INTERNAL_sdlWindowFlags_Next &= ~SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
                }
            }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                // FIXME: May not be happy since the FBO may be of different size.
                int x = 0, y = 0, w = 0, h = 0;
                SDL.SDL_GetWindowPosition(INTERNAL_sdlWindow, ref x, ref y);
                SDL.SDL_GetWindowSize(INTERNAL_sdlWindow, ref w, ref h);
                return new Rectangle(x, y, w, h);
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get
            {
                // SDL2 has no orientation.
                return DisplayOrientation.LandscapeLeft;
            }
        }
  
        public override IntPtr Handle
        {
            get
            {
                return INTERNAL_sdlWindow;
            }
        }
        
        public override bool IsBorderless
        {
            get
            {
                return (INTERNAL_sdlWindowFlags_Current & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) != SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
            }
            set
            {
                if (value)
                {
                    INTERNAL_sdlWindowFlags_Next |= SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
                }
                else
                {
                    INTERNAL_sdlWindowFlags_Next &= ~SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS;
                }
            }
        }
        
        public override string ScreenDeviceName
        {
            get
            {
                return INTERNAL_deviceName;
            }
        }

        #endregion Properties
        
        #region INTERNAL: GamePlatform Interaction, Properties
        
        public bool IsVSync
        {
            get
            {
                int result = -1;
                result = SDL.SDL_GL_GetSwapInterval();
                if (result == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                // NOTE: We can get EXT_swap_control_tear by using -1 here
                if (value)
                {
                    SDL.SDL_GL_SetSwapInterval(1);
                }
                else
                {
                    SDL.SDL_GL_SetSwapInterval(0);
                }
            }
        }
        
        public bool IsGrabbing
        {
            get
            {
                return (SDL.SDL_GetWindowGrab(INTERNAL_sdlWindow) == SDL.SDL_bool.SDL_TRUE);
            }
            set
            {
                if (value)
                {
                    SDL.SDL_SetWindowGrab(INTERNAL_sdlWindow, SDL.SDL_bool.SDL_TRUE);
                }
                else
                {
                    SDL.SDL_SetWindowGrab(INTERNAL_sdlWindow, SDL.SDL_bool.SDL_FALSE);
                }
            }
        }
        
        public bool IsMouseVisible
        {
            get
            {
                return (SDL.SDL_ShowCursor(SDL.SDL_QUERY) == 1);
            }
            set
            {
                SDL.SDL_ShowCursor(value ? 1 : 0);
            }
        }
        
        #endregion
        
        #region INTERNAL: GamePlatform Interaction, Methods
        
        public void INTERNAL_RunLoop()
        {
            // Now that we're in the game loop, this should be safe.
            Game.GraphicsDevice.glFramebuffer = INTERNAL_glFramebuffer;
            
            SDL.SDL_Event evt;
            
            while (INTERNAL_runApplication)
            {
#if !THREADED_GL
                Threading.Run();
#endif
                while (SDL.SDL_PollEvent(out evt) == 1)
                {
                    // TODO: All events...
                    
                    // Keyboard
                    if (evt.type == SDL.SDL_EventType.SDL_KEYDOWN)
                    {
                        Keys key = SDL2_KeyboardUtil.ToXNA(evt.key.keysym.sym);
                        if (!keys.Contains(key))
                        {
                            keys.Add(key);
                        }
                    }
                    else if (evt.type == SDL.SDL_EventType.SDL_KEYUP)
                    {
                        Keys key = SDL2_KeyboardUtil.ToXNA(evt.key.keysym.sym);
                        if (keys.Contains(key))
                        {
                            keys.Remove(key);
                        }
                    }

                    // Various Window Events...
                    else if (evt.type == SDL.SDL_EventType.SDL_WINDOWEVENT)
                    {
                        // Window Focus
                        if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED)
                        {
                            // If we alt-tab away, we lose the 'fullscreen desktop' flag on some WMs
                            SDL.SDL_SetWindowFullscreen(INTERNAL_sdlWindow, (uint) INTERNAL_sdlWindowFlags_Current);
                            
                            // Disable the screensaver when we're back.
                            SDL.SDL_DisableScreenSaver();
                        }
                        else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST)
                        {
                            SDL.SDL_SetWindowFullscreen(INTERNAL_sdlWindow, 0);
                            
                            // Give the screensaver back, we're not that important now.
                            SDL.SDL_EnableScreenSaver();
                        }

                        // Window Resize
                        else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED ||
                                 evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED)
                        {
                            Mouse.INTERNAL_WindowWidth = evt.window.data1;
                            Mouse.INTERNAL_WindowHeight = evt.window.data2;
                        }

                        // Mouse Focus
                        else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_ENTER)
                        {
                            SDL.SDL_DisableScreenSaver();
                        }
                        else if (evt.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE)
                        {
                            SDL.SDL_EnableScreenSaver();
                        }
                    }
                    
                    // Mouse Wheel
                    else if (evt.type == SDL.SDL_EventType.SDL_MOUSEWHEEL)
                    {
                        Mouse.INTERNAL_MouseWheel += evt.wheel.y;
                    }
                    
                    // Quit
                    else if (evt.type == SDL.SDL_EventType.SDL_QUIT)
                    {
                        INTERNAL_runApplication = false;
                        break;
                    }
                }
                if (keys.Contains(Keys.LeftAlt) && keys.Contains(Keys.F4))
                {
                    INTERNAL_runApplication = false;
                }
                Keyboard.SetKeys(keys);
                Game.Tick();
            }
            
            // We out.
            Game.Exit();
            
            GL.DeleteFramebuffer(INTERNAL_glFramebuffer);
            GL.DeleteTexture(INTERNAL_glColorAttachment);
            GL.DeleteTexture(INTERNAL_glDepthStencilAttachment);
            
#if THREADED_GL
            SDL.SDL_GL_DeleteContext(Threading.BackgroundContext.context);
#endif

            /* Some window managers might try to minimize the window as we're
             * destroying it. This looks pretty stupid and could cause problems,
             * so set this hint right before we destroy everything.
             * -flibit
             */
            SDL.SDL_SetHintWithPriority(
                SDL.SDL_HINT_VIDEO_MINIMIZE_ON_FOCUS_LOSS,
                "0",
                SDL.SDL_HintPriority.SDL_HINT_OVERRIDE
            );
            
            SDL.SDL_GL_DeleteContext(INTERNAL_GLContext);
            
            SDL.SDL_DestroyWindow(INTERNAL_sdlWindow);

            // This _should_ be the last SDL call we make...
            SDL.SDL_Quit();
        }
        
        public void INTERNAL_SwapBuffers()
        {
            Rectangle windowRect = ClientBounds;
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, INTERNAL_glFramebuffer);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BlitFramebuffer(
                0, 0, INTERNAL_glFramebufferWidth, INTERNAL_glFramebufferHeight,
                0, 0, windowRect.Width, windowRect.Height,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Linear
            );
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            SDL.SDL_GL_SwapWindow(INTERNAL_sdlWindow);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, INTERNAL_glFramebuffer);
        }
        
        public void INTERNAL_Destroy()
        {
            INTERNAL_runApplication = false;
        }
        
        #endregion
  
        #region Constructor
        
        public SDL2_GameWindow()
        {
            // This _should_ be the first SDL call we make...
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

            INTERNAL_runApplication = true;
            
            // Initialize Active Key List
            keys = new List<Keys>();
            
            INTERNAL_sdlWindowFlags_Next = (
                SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL |
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN |
                SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS |
                SDL.SDL_WindowFlags.SDL_WINDOW_MOUSE_FOCUS
            );
            AllowUserResizing = false;
            
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_RED_SIZE, 8);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_GREEN_SIZE, 8);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_BLUE_SIZE, 8);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_ALPHA_SIZE, 8);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DEPTH_SIZE, 24);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_STENCIL_SIZE, 8);
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);
            
            INTERNAL_sdlWindowTitle = "MonoGame-SDL2 Window";
            
            INTERNAL_sdlWindow = SDL.SDL_CreateWindow(
                INTERNAL_sdlWindowTitle,
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                800,
                600,
                INTERNAL_sdlWindowFlags_Next
            );
            
            INTERNAL_sdlWindowFlags_Current = INTERNAL_sdlWindowFlags_Next;
            
            // Disable the screensaver.
            SDL.SDL_DisableScreenSaver();
            
            // We hide the mouse cursor by default.
            IsMouseVisible = false;
            
            // Initialize OpenGL
            INTERNAL_GLContext = SDL.SDL_GL_CreateContext(INTERNAL_sdlWindow);
            OpenTK.Graphics.GraphicsContext.CurrentContext = INTERNAL_GLContext;
            OpenTK.Graphics.OpenGL.GL.LoadAll();
            
            // We default to VSync being on.
            IsVSync = true;
            
#if THREADED_GL
            // Create a background context
            SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_SHARE_WITH_CURRENT_CONTEXT, 1);
            Threading.WindowInfo = INTERNAL_sdlWindow;
            Threading.BackgroundContext = new GL_ContextHandle()
            {
                context = SDL.SDL_GL_CreateContext(INTERNAL_sdlWindow)
            };
            
            // Make the foreground context current.
            SDL.SDL_GL_MakeCurrent(INTERNAL_sdlWindow, INTERNAL_GLContext);
#endif
            
            // Create an FBO, use this as our "backbuffer".
            GL.GenFramebuffers(1, out INTERNAL_glFramebuffer);
            GL.GenTextures(1, out INTERNAL_glColorAttachment);
            GL.GenTextures(1, out INTERNAL_glDepthStencilAttachment);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, INTERNAL_glFramebuffer);
            GL.BindTexture(TextureTarget.Texture2D, INTERNAL_glColorAttachment);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                800,
                600,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedInt,
                IntPtr.Zero
            );
            GL.BindTexture(TextureTarget.Texture2D, INTERNAL_glDepthStencilAttachment);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.DepthComponent16,
                800,
                600,
                0,
                PixelFormat.DepthComponent,
                PixelType.UnsignedByte,
                IntPtr.Zero
            );
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                INTERNAL_glColorAttachment,
                0
            );
            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D,
                INTERNAL_glDepthStencilAttachment,
                0
            );
            GL.BindTexture(TextureTarget.Texture2D, 0);
            INTERNAL_glFramebufferWidth = 800;
            INTERNAL_glFramebufferHeight = 600;
            Mouse.INTERNAL_BackbufferWidth = 800;
            Mouse.INTERNAL_BackbufferHeight = 600;
            
            INTERNAL_depthFormat = DepthFormat.Depth16;
        }
        
        #endregion
        
        #region ScreenDeviceChange
        
        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            // Fullscreen windowflag
            if (willBeFullScreen)
            {
                INTERNAL_sdlWindowFlags_Next |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
            }
            else
            {
                INTERNAL_sdlWindowFlags_Next &= ~SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;
            }
        }
  
        public override void EndScreenDeviceChange(
            string screenDeviceName,
            int clientWidth,
            int clientHeight
        ) {
            EndScreenDeviceChange(
                screenDeviceName,
                clientWidth,
                clientHeight,
                INTERNAL_depthFormat
            );
        }
        
        public void EndScreenDeviceChange(
            string screenDeviceName,
            int clientWidth,
            int clientHeight,
            DepthFormat backbufferFormat
        ) {
            // Set screen device name, not that we use it...
            INTERNAL_deviceName = screenDeviceName;
            
            // Window bounds
            SDL.SDL_SetWindowSize(INTERNAL_sdlWindow, clientWidth, clientHeight);
            
            // Bordered
            // FIXME: May not be needed due to SetWindowFullscreen?
            if ((INTERNAL_sdlWindowFlags_Next & SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS) == SDL.SDL_WindowFlags.SDL_WINDOW_BORDERLESS)
            {
                SDL.SDL_SetWindowBordered(INTERNAL_sdlWindow, SDL.SDL_bool.SDL_FALSE);
            }
            else
            {
                SDL.SDL_SetWindowBordered(INTERNAL_sdlWindow, SDL.SDL_bool.SDL_TRUE);
            }
            
            // Fullscreen (and general Window flags)
            SDL.SDL_SetWindowFullscreen(INTERNAL_sdlWindow, (uint) INTERNAL_sdlWindowFlags_Next);
            
            // Window position
            if (    (INTERNAL_sdlWindowFlags_Current & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) == SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP &&
                    (INTERNAL_sdlWindowFlags_Next & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) == 0 )
            {
                // If exiting fullscreen, just center the window on the desktop.
                SDL.SDL_SetWindowPosition(
                    INTERNAL_sdlWindow,
                    SDL.SDL_WINDOWPOS_CENTERED,
                    SDL.SDL_WINDOWPOS_CENTERED
                );
            }
            else if ((INTERNAL_sdlWindowFlags_Next & SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP) == 0)
            {
                // Try to center the window around the old window position.
                int x = 0;
                int y = 0;
                SDL.SDL_GetWindowPosition(INTERNAL_sdlWindow, ref x, ref y);
                SDL.SDL_SetWindowPosition(
                    INTERNAL_sdlWindow,
                    x + ((INTERNAL_glFramebufferWidth - clientWidth) / 2),
                    y + ((INTERNAL_glFramebufferHeight - clientHeight) / 2)
                );
            }
            
            // Current flags have just been updated.
            INTERNAL_sdlWindowFlags_Current = INTERNAL_sdlWindowFlags_Next;
            
            // Now, update the viewport
            Game.GraphicsDevice.Viewport = new Viewport(
                0,
                0,
                clientWidth,
                clientHeight
            );
            
            // Update our color attachment to the new resolution
            GL.BindTexture(TextureTarget.Texture2D, INTERNAL_glColorAttachment);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                clientWidth,
                clientHeight,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedInt,
                IntPtr.Zero
            );
            
            // Update the depth attachment based on the desired DepthFormat.
            PixelFormat depthPixelFormat;
            PixelInternalFormat depthPixelInternalFormat;
            PixelType depthPixelType;
            FramebufferAttachment depthAttachmentType;
            if (backbufferFormat == DepthFormat.Depth16)
            {
                depthPixelFormat = PixelFormat.DepthComponent;
                depthPixelInternalFormat = PixelInternalFormat.DepthComponent16;
                depthPixelType = PixelType.UnsignedByte;
                depthAttachmentType = FramebufferAttachment.DepthAttachment;
            }
            else if (backbufferFormat == DepthFormat.Depth24)
            {
                depthPixelFormat = PixelFormat.DepthComponent;
                depthPixelInternalFormat = PixelInternalFormat.DepthComponent24;
                depthPixelType = PixelType.UnsignedByte;
                depthAttachmentType = FramebufferAttachment.DepthAttachment;
            }
            else
            {
                depthPixelFormat = PixelFormat.DepthStencil;
                depthPixelInternalFormat = PixelInternalFormat.Depth24Stencil8;
                depthPixelType = PixelType.UnsignedInt248;
                depthAttachmentType = FramebufferAttachment.DepthStencilAttachment;
            }
            
            GL.BindTexture(TextureTarget.Texture2D, INTERNAL_glDepthStencilAttachment);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                depthPixelInternalFormat,
                clientWidth,
                clientHeight,
                0,
                depthPixelFormat,
                depthPixelType,
                IntPtr.Zero
            );
            
            if (backbufferFormat != INTERNAL_depthFormat)
            {
                FramebufferAttachment attach;
                if (INTERNAL_depthFormat == DepthFormat.Depth24Stencil8)
                {
                    attach = FramebufferAttachment.DepthStencilAttachment;
                }
                else
                {
                    attach = FramebufferAttachment.DepthAttachment;
                }
                
                int oldFramebuffer;
                GL.GetInteger(GetPName.FramebufferBinding, out oldFramebuffer);
             
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, INTERNAL_glFramebuffer);
                
                GL.FramebufferTexture2D(
                    FramebufferTarget.Framebuffer,
                    attach,
                    TextureTarget.Texture2D,
                    0,
                    0
                );
                GL.FramebufferTexture2D(
                    FramebufferTarget.Framebuffer,
                    depthAttachmentType,
                    TextureTarget.Texture2D,
                    INTERNAL_glDepthStencilAttachment,
                    0
                );
             
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, oldFramebuffer);
                
                INTERNAL_depthFormat = backbufferFormat;
            }
            
            INTERNAL_glFramebufferWidth = clientWidth;
            INTERNAL_glFramebufferHeight = clientHeight;
            Mouse.INTERNAL_BackbufferWidth = clientWidth;
            Mouse.INTERNAL_BackbufferHeight = clientHeight;
        }
        
        #endregion
        
        #region Sets

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // No-op. SDL2 has no orientation.
        }
        
        protected override void SetTitle(string title)
        {
            INTERNAL_sdlWindowTitle = title;
            SDL.SDL_SetWindowTitle(
                INTERNAL_sdlWindow,
                INTERNAL_sdlWindowTitle
            );
            
            if (System.IO.File.Exists(title + ".bmp"))
            {
                IntPtr icon = SDL.SDL_LoadBMP(title + ".bmp");
                SDL.SDL_SetWindowIcon(INTERNAL_sdlWindow, icon);
            }
        }
  
        #endregion
    }
}
