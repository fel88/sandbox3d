﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RenderTool
{
    public class Shader
    {
        public static string ReadResourceTxt(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fr1 = assembly.GetManifestResourceNames().First(z => z.Contains(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(fr1))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public Shader(string v1, string v2)
        {
            // 2. compile shaders
            int vertex, fragment;
            // vertex shader
            string vShaderCode = ReadResourceTxt(v1);
            string fShaderCode = ReadResourceTxt(v2);
            vertex = GL.CreateShader(ShaderType.VertexShader);

            GL.ShaderSource(vertex, vShaderCode);
            GL.CompileShader(vertex);
            var err=checkCompileErrors(vertex, "VERTEX");
            if (err != "") throw new Exception("VERTEX shader error: " + err);
            // fragment Shader
            fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, fShaderCode);
            GL.CompileShader(fragment);
             err = checkCompileErrors(fragment, "FRAGMENT");
            if (err != "") throw new Exception("FRAGMENT shader error: " + err);
            // shader Program
            ID = GL.CreateProgram();
            GL.AttachShader(ID, vertex);
            GL.AttachShader(ID, fragment);
            GL.LinkProgram(ID);
             err = checkCompileErrors(ID, "PROGRAM");
            if (err != "") throw new Exception("PROGRAM shader error: " + err);
            // delete the shaders as they're linked into our program now and no longer necessary
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
        }
        int ID;
        string checkCompileErrors(int shader, string type)
        {
            int success;
            string infoLog;
            if (type != "PROGRAM")
            {
                GL.GetShader(shader, ShaderParameter.CompileStatus, out success);
                if (success == 0)
                {
                    GL.GetShaderInfoLog(shader, out infoLog);
                    return infoLog;
                    //std::cout << "ERROR::SHADER_COMPILATION_ERROR of type: " << type << "\n" << infoLog << "\n -- --------------------------------------------------- -- " << std::endl;
                }
            }
            else
            {
                GL.GetProgram(shader, GetProgramParameterName.LinkStatus, out success);
                if (success == 0)
                {
                    GL.GetProgramInfoLog(shader, out infoLog);
                    return infoLog;
                    //  std::cout << "ERROR::PROGRAM_LINKING_ERROR of type: " << type << "\n" << infoLog << "\n -- --------------------------------------------------- -- " << std::endl;
                }
            }
            return string.Empty;
        }
        internal void setInt(string v1, int v2)
        {
            GL.Uniform1(GL.GetUniformLocation(ID, v1), v2);
        }
        
        internal void setFloat(string v1, float v2)
        {
            GL.Uniform1(GL.GetUniformLocation(ID, v1), v2);
        }
        internal void setMat4(string v, Matrix4 projection)
        {
            GL.UniformMatrix4(GL.GetUniformLocation(ID, v), false, ref projection);
        }

        internal void setVec3(string v, Vector3 newPos)
        {
            GL.Uniform3(GL.GetUniformLocation(ID, v), ref newPos);
        }
        internal void setVec2(string v, Vector2 newPos)
        {
            GL.Uniform2(GL.GetUniformLocation(ID, v), ref newPos);
        }

        internal void use()
        {
            GL.UseProgram(ID);
        }
    }
}

