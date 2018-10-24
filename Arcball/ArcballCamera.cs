using Jay.Geometry.Transformation;
using Jay.Geometry.Util;
using OpenTK;
using System;
using System.Collections.Generic;
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
        /// radius of sphere
        /// </summary>
        public float Radius { get; private set; }

        /// <summary>
        /// translation matrix
        /// </summary>
        public Matrix4 Translation { get; set; }

        /// <summary>
        /// rotation matrix
        /// </summary>
        public Matrix4 Rotation { get; set; }

        /// <summary>
        /// scaling matrix
        /// </summary>
        public Matrix4 Scaling { get; set; }

        /// <summary>
        /// screen's width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// screen's height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// pan sensitivity
        /// (to be changed)
        /// </summary>
        private readonly float _panSensitivity = 5;

        /// <summary>
        /// rotation sensitivity
        /// </summary>
        private readonly float _rotateSensitivity = 2;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="center">center of sphere</param>
        /// <param name="radius">radius of sphere</param>
        /// <param name="viewWidth">screen's width</param>
        /// <param name="viewHeight">screen's height</param>
        public ArcballCamera(Vector3 center, float radius, int viewWidth, int viewHeight)
        {
            this.Center = center;
            this.Radius = radius;
            this.Width = viewWidth;
            this.Height = viewHeight;

            InitDefaultPosition();
        }

        /// <summary>
        /// Calculate dafualt camera position(simple case)
        /// </summary>
        public void InitDefaultPosition()
        {
            var position = new Vector3(Center.X, Center.Y, Center.Z + Radius);
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
            float angle = Vector3.CalculateAngle(arcStartPoint, arcEndPoint) * _rotateSensitivity;

            // pivot 중심 회전
             var rotation = new Rotation3D(MathUtil.ToDegrees(angle), rotateAxis, Center);
            Rotation *= rotation.Matrix;
        }

        public void Zoom(float zoomFactor)
        {            
            zoomFactor = zoomFactor > 0 ? 1.1f : 0.9f;

            var scaling = new Scaling3D(zoomFactor, Center);
            Scaling *= scaling.Matrix;
        }

        private Vector3 MapSphereCoordinate(Vector2 pt)
        {
            Vector3 coord = new Vector3();

            coord.X = (2 * pt.X - Width) / Width;
            coord.Y = -(2 * pt.Y - Height) / Height;            // 좌표축 반대라서 대칭

            coord = Vector3.Clamp(coord, new Vector3(-1, -1, 0), new Vector3(1, 1, 0));

            float length_squared = coord.X * coord.X + coord.Y * coord.Y;

            if (length_squared <= 1.0)
            {
                coord.Z = (float)Math.Sqrt(1.0 - length_squared);
            }
            else
            {
                // checking
                coord = Vector3.Normalize(coord);
                //coord *= (float)(1.0 / Math.Sqrt(length_squared));
                coord.Z = 0.0f;                
            }

            return coord;
        }
    }
}
