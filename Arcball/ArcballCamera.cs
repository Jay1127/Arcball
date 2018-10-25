using Jay.Geometry.Transformation;
using Jay.Geometry.Util;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arcball
{
    /// <summary>
    /// Arcball camera representation
    /// </summary>
    public class ArcballCamera
    {
        /// <summary>
        /// camera view matrix
        /// </summary>
        public Matrix4 ViewMatrix => Scaling * Rotation * Translation;

        /// <summary>
        /// center of sphere
        /// </summary>
        public Vector3 Center { get; private set; }

        /// <summary>
        /// translation matrix
        /// </summary>
        public Matrix4 Translation { get; private set; }

        /// <summary>
        /// rotation matrix
        /// </summary>
        public Matrix4 Rotation { get; private set; }

        /// <summary>
        /// scaling matrix
        /// </summary>
        public Matrix4 Scaling { get; private set; }

        /// <summary>
        /// screen's width
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// screen's height
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// pan sensitivity
        /// (to be changed)
        /// </summary>
        private readonly float _panSensitivity = 5;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="center">center of sphere</param>
        /// <param name="screenWidth">screen's width</param>
        /// <param name="screenHeight">screen's height</param>
        public ArcballCamera(Vector3 center, int screenWidth, int screenHeight)
        {
            this.Center = center;
            this.Width = screenWidth;
            this.Height = screenHeight;
        }

        /// <summary>
        /// Calculate dafualt camera position(simple case)
        /// </summary>
        public void InitDefaultPosition(float radius)
        {
            var position = new Vector3(Center.X, Center.Y, Center.Z + radius);
            Translation = Matrix4.LookAt(position, Center, Vector3.UnitY);
            Rotation = Matrix4.Identity;
            Scaling = Matrix4.Identity;
        }

        /// <summary>
        /// Pan camera
        /// </summary>
        /// <param name="dv">movement vector</param>
        public void Pan(Vector3 dv)
        {
            var translation = new Translation3D(dv / _panSensitivity);
            Translation *= translation.Matrix;
        }

        /// <summary>
        /// Rotate camera by pivot(pivot is sphere center)
        /// </summary>
        /// <param name="scrStartPoint">screen point(mouse start)</param>
        /// <param name="srcEndPoint"></param>
        public void Rotate(Vector2 scrStartPoint, Vector2 srcEndPoint)
        {
            Vector3 arcStartPoint = MapSphereCoordinate(scrStartPoint);
            Vector3 arcEndPoint = MapSphereCoordinate(srcEndPoint);

            Vector3 rotateAxis = Vector3.Cross(arcStartPoint, arcEndPoint);
            float angle = Vector3.CalculateAngle(arcStartPoint, arcEndPoint);

            // pivot 중심 회전
             var rotation = new Rotation3D(MathUtil.ToDegrees(angle), rotateAxis, Center);
            Rotation *= rotation.Matrix;
        }

        /// <summary>
        /// Zoom camera(not consider view volume size)
        /// </summary>
        /// <param name="zoomFactor">zoom magnitude</param>
        public void Zoom(float zoomFactor)
        {            
            zoomFactor = zoomFactor > 0 ? 1.1f : 0.9f;

            var scaling = new Scaling3D(zoomFactor, Center);
            Scaling *= scaling.Matrix;
        }

        /// <summary>
        /// Map Screen Coordinates to Sphere Coordinate[-1 ~ 1]
        /// </summary>
        /// <param name="screenPt">screen coordinate</param>
        /// <returns>sphere coordinate</returns>
        private Vector3 MapSphereCoordinate(Vector2 screenPt)
        {
            Vector3 coord = new Vector3();

            var diameter = Math.Min(Width, Height);

            coord.X = (2 * screenPt.X - diameter) / diameter;
            coord.Y = -(2 * screenPt.Y - diameter) / diameter;            // 좌표축 반대라서 대칭

            float length_squared = coord.X * coord.X + coord.Y * coord.Y;

            if (length_squared <= 1.0)
            {
                coord.Z = (float)Math.Sqrt(1.0 - length_squared);
            }
            else
            {
                coord = Vector3.Normalize(coord);
                coord.Z = 0.0f;                
            }

            return coord;
        }
    }
}
