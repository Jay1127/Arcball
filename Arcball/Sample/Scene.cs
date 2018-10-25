using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcball
{
    class Scene : GameWindow
    {
        ArcballCamera _camera;

        int _vao;
        int _vbo;
        int _ebo;

        Vector3[] _vertices = Teapot.Vertices;
        uint[] _indices = Teapot.Indices;

        Vector3[] _circlePts;

        bool _isMousePosInScreen;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Title = "Arcball";

            // set arcball camera
            CalculateSphereInfo(out Vector3 center, out float radius);
            _camera = new ArcballCamera(center, Width, Height);
            _camera.InitDefaultPosition(radius);

            _circlePts = GetPointsOnCircle(center, 100);

            // generate buffer of teapot model
            GL.GenVertexArrays(1, out _vao);
            GL.BindVertexArray(_vao);

            GL.GenBuffers(1, out _vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * _vertices.Length, _vertices, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out _ebo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, sizeof(int) * _indices.Length, _indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_isMousePosInScreen) return;

            if(e.Mouse.LeftButton == ButtonState.Pressed)
            {
                var prevPos = new Vector2(e.Position.X - e.XDelta, e.Position.Y - e.YDelta);
                var currPos = new Vector2(e.Position.X, e.Position.Y);

                _camera.Rotate(prevPos, currPos);
            }
            else if(e.Mouse.RightButton == ButtonState.Pressed)
            {
                _camera.Pan(new Vector3(e.XDelta, -e.YDelta, 0));
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            if (!_isMousePosInScreen) return;

            _camera.Zoom(e.Delta);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isMousePosInScreen = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isMousePosInScreen = false;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            int viewVolume = 100;

            float aspect = 1.0f * Width / Height;

            GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(-viewVolume * aspect, viewVolume * aspect, -viewVolume, viewVolume, viewVolume, -viewVolume);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            GL.PushMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            var viewMatrix = _camera.ViewMatrix;
            GL.LoadMatrix(ref viewMatrix);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

            // unbind
            GL.BindVertexArray(0);
            GL.PopMatrix();            

            // draw circle
            GL.Begin(PrimitiveType.Lines);
            {
                Array.ForEach(_circlePts, pt => GL.Vertex3(pt));
            }
            GL.End();

            SwapBuffers();
        }

        public Vector3[] GetPointsOnCircle(Vector3 center, float radius)
        {
            var pts = new List<Vector3>();
            var angle = (360 / 180);
            var radian = angle * Math.PI / 180;

            for(int i = 0; i < 180; i++)
            {
                var x = (float)(center.X + radius * Math.Cos(radian * i));
                var y = (float)(center.Y + radius * Math.Sin(radian * i));
                pts.Add(new Vector3(x, y, 0));
            }

            return pts.ToArray();
        }

        /// <summary>
        /// Calculate sphere's center, radius
        /// </summary>
        /// <param name="center">center of sphere</param>
        /// <param name="radius">radius of shpere</param>
        private void CalculateSphereInfo(out Vector3 center, out float radius)
        {
            var maxX = _vertices.Max(v => v.X);
            var maxY = _vertices.Max(v => v.Y);
            var maxZ = _vertices.Max(v => v.Z);
            var minX = _vertices.Min(v => v.X);
            var minY = _vertices.Min(v => v.Y);
            var minZ = _vertices.Min(v => v.Z);

            center = new Vector3((maxX + minX) / 2, (maxY + minY) / 2, (maxZ + minZ) / 2);
            radius = Vector3.Distance(new Vector3(maxX, maxY, maxZ), center) * 2;
        }
    }
}