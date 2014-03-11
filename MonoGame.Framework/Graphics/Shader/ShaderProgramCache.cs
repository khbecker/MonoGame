﻿#if OPENGL

using System;
using System.Collections.Generic;

#if SDL2
using OpenTK.Graphics.OpenGL;
#elif PSM
using Sce.PlayStation.Core.Graphics;
#elif WINRT

#else
using OpenTK.Graphics.ES20;
#if IOS || ANDROID
using ActiveUniformType = OpenTK.Graphics.ES20.All;
using ShaderType = OpenTK.Graphics.ES20.All;
using ProgramParameter = OpenTK.Graphics.ES20.All;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{

    internal struct ShaderProgramInfo
    {
        public int program;
        public int posFixupLoc;
    }

    /// <summary>
    /// This class is used to Cache the links between Vertex/Pixel Shaders and Constant Buffers.
    /// It will be responsible for linking the programs under OpenGL if they have not been linked
    /// before. If an existing link exists it will be resused.
    /// </summary>
    internal class ShaderProgramCache : IDisposable
    {
        private readonly Dictionary<int, ShaderProgramInfo> _programCache = new Dictionary<int, ShaderProgramInfo>();
        bool disposed;

        ~ShaderProgramCache()
        {
            Dispose(false);
        }

        /// <summary>
        /// Clear the program cache releasing all shader programs.
        /// </summary>
        public void Clear()
        {
            foreach (var pair in _programCache)
            {
                if (GL.IsProgram(pair.Value.program))
                {
                    GL.DeleteProgram(pair.Value.program);
                }
            }
            _programCache.Clear();
        }

        public ShaderProgramInfo GetProgramInfo(Shader vertexShader, Shader pixelShader)
        {
            // TODO: We should be hashing in the mix of constant 
            // buffers here as well.  This would allow us to optimize
            // setting uniforms to only when a constant buffer changes.

            var key = vertexShader.HashKey | pixelShader.HashKey;
            if (!_programCache.ContainsKey(key))
            {
                // the key does not exist so we need to link the programs
                Link(vertexShader, pixelShader);    
            }

            return _programCache[key];
        }        

        private void Link(Shader vertexShader, Shader pixelShader)
        {
            // NOTE: No need to worry about background threads here
            // as this is only called at draw time when we're in the
            // main drawing thread.
            var program = GL.CreateProgram();

            GL.AttachShader(program, vertexShader.GetShaderHandle());

            GL.AttachShader(program, pixelShader.GetShaderHandle());

            //vertexShader.BindVertexAttributes(program);

            GL.LinkProgram(program);

            GL.UseProgram(program);

            vertexShader.GetVertexAttributeLocations(program);

            pixelShader.ApplySamplerTextureUnits(program);

            var linked = 0;

#if GLES
			GL.GetProgram(program, ProgramParameter.LinkStatus, ref linked);
#else
            GL.GetProgram(program, ProgramParameter.LinkStatus, out linked);
#endif
            if (linked == 0)
            {
#if !GLES
                var log = GL.GetProgramInfoLog(program);
                Console.WriteLine(log);
#endif
                GL.DetachShader(program, vertexShader.GetShaderHandle());
                GL.DetachShader(program, pixelShader.GetShaderHandle());
                GL.DeleteProgram(program);
                throw new InvalidOperationException("Unable to link effect program");
            }

            ShaderProgramInfo info;
            info.program = program;
            info.posFixupLoc = GL.GetUniformLocation(program, "posFixup");

            _programCache.Add(vertexShader.HashKey | pixelShader.HashKey, info);

            ConstantBuffer.FlushUniformLocationCache();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    Clear();
                disposed = true;
            }
        }
    }
}

#endif // OPENGL
