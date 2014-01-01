﻿/*
 tdogl.Shader
 
 Copyright 2012 Thomas Dalling - http://tomdalling.com/
 C# Port is made by Vyacheslav Zeronov - zeronsix@gmail.com
 C# Port is based on Pencil.Gaming library by Antonie Blom - https://github.com/antonijn/Pencil.Gaming

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
 */
using System;
using System.IO;
using Pencil.Gaming.Graphics;

namespace tdogl
{
    /// <summary>
    /// Represents a compiled OpenGL shader.
    /// </summary>
    public class Shader: IDisposable
    {
        /// <summary>
        /// Creates a shader from a text file.
        /// </summary>
        /// <param name="filePath">The path to the text file containing the shader source.</param>
        /// <param name="shaderType">Same as the argument to glCreateShader. For example ShaderType.VertexShader
        ///                          or ShaderType.FragmentShader.</param>
        /// <returns></returns>
        public static Shader ShaderFromFile(string filePath, ShaderType shaderType)
        {
            string text = File.ReadAllText(filePath);
            Shader shader = new Shader(text, shaderType);
            return shader;
        }

        /// <summary>
        /// Creates a shader from a string of shader source code.
        /// </summary>
        /// <param name="shaderCode">The source code for the shader.</param>
        /// <param name="shaderType">Same as the argument to glCreateShader. For example ShaderType.VertexShader
        ///                          or ShaderType.FragmentShader.</param>
        public Shader(string shaderCode, ShaderType shaderType)
        {
            _object = 0;
            _refCount = 0;

            //create the shader object
            _object = GL.CreateShader(shaderType);
            if (_object == 0)
                throw new Exception("glCreateShader failed");

            //set the source code
            GL.ShaderSource(_object, shaderCode);

            // compile
            GL.CompileShader(_object);

            // throw exception if compile error occurred
            int status;
            GL.GetShader(_object, ShaderParameter.CompileStatus, out status);
            if (status == 0) {
                string msg = "Compile failure in shader: ";

                string strInfoLog;
                GL.GetShaderInfoLog((int)_object, out strInfoLog);
                msg += strInfoLog;

                GL.DeleteShader(_object);
                _object = 0;
                throw new Exception(msg);
            }

            _refCount = 1;
        }

        /// <summary>
        /// The shader's object ID, as returned from glCreateShader
        /// </summary>
        public uint GlObject
        {
            get { return _object; }
        }

        // tdogl::Shader objects can be copied and assigned because they are reference counted
        // like a shared pointer
        public Shader(Shader other)
        {
            _object = other._object;
            _refCount = other._refCount;
        }

        public void Dispose()
        {
            Release();
        }

        private uint _object;
        private uint _refCount;

        private void Retain()
        {
            _refCount += 1;
        }

        private void Release()
        {
            _refCount -= 1;
            if (_refCount == 0) {
                GL.DeleteShader(_object);
                _object = 0;
            }
        }
    }
}
