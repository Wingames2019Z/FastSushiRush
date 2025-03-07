using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    // based off of code from https://forum.unity.com/threads/new-ui-and-line-drawing.253772/page-2
    // and the Unity UI Extensions project https://bitbucket.org/UnityUIExtensions/unity-ui-extensions/wiki/About
    [AddComponentMenu("UI/Line Renderer (UI)")]
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    public class UILineRenderer : MaskableGraphic
    {
        private enum SegmentType
        {
            Start,
            Middle,
            End,
            Full,
        }

        public enum JoinType
        {
            Bevel,
            Miter
        }

        public enum BezierType
        {
            None,
            Quick,
            Basic,
            Improved
        }

        private const float MIN_MITER_JOIN = 15 * Mathf.Deg2Rad;

        // A bevel 'nice' join displaces the vertices of the line segment instead of simply rendering a
        // quad to connect the endpoints. This improves the look of textured and transparent lines, since
        // there is no overlapping.
        private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;

        private static Vector2 UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_TOP_CENTER_LEFT, UV_TOP_CENTER_RIGHT, UV_BOTTOM_CENTER_LEFT, UV_BOTTOM_CENTER_RIGHT, UV_TOP_RIGHT, UV_BOTTOM_RIGHT;
        private static Vector2[] startUvs, middleUvs, endUvs, fullUvs;

        /// <summary>
        /// The specified Material used by this Image. The default Material is used instead if one wasn't specified.
        /// </summary>
        public override Material material
        {
            get
            {
                if (m_Material != null)
                    return m_Material;
#if UNITY_EDITOR
                if (Application.isPlaying && activeSprite && activeSprite.associatedAlphaSplitTexture != null)
                    return defaultETC1GraphicMaterial;
#else
 
                if (activeSprite && activeSprite.associatedAlphaSplitTexture != null)
                    return defaultETC1GraphicMaterial;
#endif

                return defaultMaterial;
            }

            set
            {
                base.material = value;
            }
        }

        static protected Material s_ETC1DefaultUI = null;
        static public Material defaultETC1GraphicMaterial
        {
            get
            {
                if (s_ETC1DefaultUI == null)
                    s_ETC1DefaultUI = Canvas.GetETC1SupportedCanvasMaterial();
                return s_ETC1DefaultUI;
            }
        }

        /// <summary>
        /// Image's texture comes from the UnityEngine.Image.
        /// </summary>
        public override Texture mainTexture
        {
            get
            {
                if (activeSprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }
                    return s_WhiteTexture;
                }

                return activeSprite.texture;
            }
        }

        [SerializeField] private Sprite m_Sprite;
        public Sprite sprite { get { return m_Sprite; } set { if (SetClass(ref m_Sprite, value)) GeneratedUVs(); SetAllDirty(); } }

        [NonSerialized]
        private Sprite m_OverrideSprite;

        public Sprite overrideSprite
        {
            get { return activeSprite; }
            set
            {
                if (SetClass(ref m_OverrideSprite, value))
                {
                    GeneratedUVs();
                    SetAllDirty();
                }
            }
        }

        private Sprite activeSprite { get { return m_OverrideSprite != null ? m_OverrideSprite : sprite; } }

        [SerializeField, Tooltip("Initial control points to draw lines between; by default in UI mesh space but tweak it with useRectAsNormalizedSpace")]
        internal Vector2[] m_points;
        [SerializeField, Tooltip("Segments to be drawn after Bezier pass, an array of points")]
        internal List<Vector2[]> m_segments;
        [SerializeField, Tooltip("Use relative normalized bounds of the Rect Transform (0.0 - 1.0) or screen space coordinates?")]
        internal bool useRectAsNormalizedSpace;

        [SerializeField, Tooltip("Thickness of the line")]
        internal float lineThickness = 2;

        [SerializeField, Tooltip("Do the points identify a single long continuous line, or split pairs of lines?")]
        internal bool drawPointsAsPairs;
        [SerializeField, Tooltip("Add end caps to each line\nMultiple caps when used with Line List")]
        internal bool lineCaps;

        public float LineThickness
        {
            get { return lineThickness; }
            set { lineThickness = value; SetAllDirty(); }
        }

        public bool RelativeSize
        {
            get { return useRectAsNormalizedSpace; }
            set { useRectAsNormalizedSpace = value; SetAllDirty(); }
        }

        public bool LineList
        {
            get { return drawPointsAsPairs; }
            set { drawPointsAsPairs = value; SetAllDirty(); }
        }

        public bool LineCaps
        {
            get { return lineCaps; }
            set { lineCaps = value; SetAllDirty(); }
        }

        [Tooltip("The type of Join used between lines, Square/Mitre or Curved/Bevel")]
        public JoinType LineJoins = JoinType.Bevel;

        [Tooltip("Bezier method to apply to line for smoothing, requires at least 4 points")]
        public BezierType BezierMode = BezierType.None;

        [SerializeField, Tooltip("Resolution of the Bezier curve, more segments = more smoothness = more vertices")]
        internal int bezierSegmentsPerCurve = 10;

        public int BezierSegmentsPerCurve
        {
            get { return bezierSegmentsPerCurve; }
            set { bezierSegmentsPerCurve = value; }
        }

        [HideInInspector]
        public bool drivenExternally = false;


        /// <summary>
        /// Points to be drawn in the line.
        /// </summary>
        public Vector2[] Points
        {
            get
            {
                return m_points;
            }

            set
            {
                if (m_points == value)
                    return;
                m_points = value;
                SetAllDirty();
            }
        }

        /// <summary>
        /// List of Segments to be drawn.
        /// </summary>
        public List<Vector2[]> Segments
        {
            get
            {
                return m_segments;
            }

            set
            {
                m_segments = value;
                SetAllDirty();
            }
        }

        protected UILineRenderer()
        {
            useLegacyMeshGeneration = false;
        }

        /// <summary>
        /// Update the renderer's material.
        /// </summary>
        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();

            // check if this sprite has an associated alpha texture (generated when splitting RGBA = RGB + A as two textures without alpha)

            if (activeSprite == null)
            {
                canvasRenderer.SetAlphaTexture(null);
                return;
            }

            Texture2D alphaTex = activeSprite.associatedAlphaSplitTexture;

            if (alphaTex != null)
            {
                canvasRenderer.SetAlphaTexture(alphaTex);
            }
        }

        private void PopulateMesh(VertexHelper vh, Vector2[] pointsToDraw)
        {
            //If Bezier is desired, pick the implementation
            if (BezierMode != BezierType.None && pointsToDraw.Length > 3)
            {
                BezierPath bezierPath = new BezierPath();

                bezierPath.SetControlPoints(pointsToDraw);
                bezierPath.SegmentsPerCurve = bezierSegmentsPerCurve;
                List<Vector2> drawingPoints;
                switch (BezierMode)
                {
                    case BezierType.Basic:
                        drawingPoints = bezierPath.GetDrawingPoints0();
                        break;
                    case BezierType.Improved:
                        drawingPoints = bezierPath.GetDrawingPoints1();
                        break;
                    default:
                        drawingPoints = bezierPath.GetDrawingPoints2();
                        break;
                }

                pointsToDraw = drawingPoints.ToArray();
            }

            // scale based on the size of the rect or use absolute, this is switchable
            var sizeX = !useRectAsNormalizedSpace ? 1 : rectTransform.rect.width;
            var sizeY = !useRectAsNormalizedSpace ? 1 : rectTransform.rect.height;
            var offsetX = -rectTransform.pivot.x * sizeX;
            var offsetY = -rectTransform.pivot.y * sizeY;

            // Generate the quads that make up the wide line
            var segments = new List<UIVertex[]>();
            if (drawPointsAsPairs)
            {
                for (var i = 1; i < pointsToDraw.Length; i += 2)
                {
                    var start = pointsToDraw[i - 1];
                    var end = pointsToDraw[i];
                    start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
                    end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

                    if (lineCaps)
                    {
                        segments.Add(CreateLineCap(start, end, SegmentType.Start));
                    }

                    segments.Add(CreateLineSegment(start, end, SegmentType.Middle, segments.Count > 1 ? segments[segments.Count - 2] : null));

                    if (lineCaps)
                    {
                        segments.Add(CreateLineCap(start, end, SegmentType.End));
                    }
                }
            }
            else
            {
                for (var i = 1; i < pointsToDraw.Length; i++)
                {
                    var start = pointsToDraw[i - 1];
                    var end = pointsToDraw[i];
                    start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
                    end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

                    if (lineCaps && i == 1)
                    {
                        segments.Add(CreateLineCap(start, end, SegmentType.Start));
                    }

                    segments.Add(CreateLineSegment(start, end, SegmentType.Middle));

                    if (lineCaps && i == pointsToDraw.Length - 1)
                    {
                        segments.Add(CreateLineCap(start, end, SegmentType.End));
                    }
                }
            }

            // Add the line segments to the vertex helper, creating any joins as needed
            for (var i = 0; i < segments.Count; i++)
            {
                if (!drawPointsAsPairs && i < segments.Count - 1)
                {
                    var vec1 = segments[i][1].position - segments[i][2].position;
                    var vec2 = segments[i + 1][2].position - segments[i + 1][1].position;
                    var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

                    // Positive sign means the line is turning in a 'clockwise' direction
                    var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

                    // Calculate the miter point
                    var miterDistance = lineThickness / (2 * Mathf.Tan(angle / 2));
                    var miterPointA = segments[i][2].position - vec1.normalized * miterDistance * sign;
                    var miterPointB = segments[i][3].position + vec1.normalized * miterDistance * sign;

                    var joinType = LineJoins;
                    if (joinType == JoinType.Miter)
                    {
                        // Make sure we can make a miter join without too many artifacts.
                        if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_MITER_JOIN)
                        {
                            segments[i][2].position = miterPointA;
                            segments[i][3].position = miterPointB;
                            segments[i + 1][0].position = miterPointB;
                            segments[i + 1][1].position = miterPointA;
                        }
                        else
                        {
                            joinType = JoinType.Bevel;
                        }
                    }

                    if (joinType == JoinType.Bevel)
                    {
                        if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_BEVEL_NICE_JOIN)
                        {
                            if (sign < 0)
                            {
                                segments[i][2].position = miterPointA;
                                segments[i + 1][1].position = miterPointA;
                            }
                            else
                            {
                                segments[i][3].position = miterPointB;
                                segments[i + 1][0].position = miterPointB;
                            }
                        }

                        var join = new UIVertex[] { segments[i][2], segments[i][3], segments[i + 1][0], segments[i + 1][1] };
                        vh.AddUIVertexQuad(join);
                    }
                }

                vh.AddUIVertexQuad(segments[i]);
            }
            if (vh.currentVertCount > 64000)
            {
                Debug.LogError("Max Verticies size is 64000, current mesh vertcies count is [" + vh.currentVertCount + "] - Cannot Draw");
                vh.Clear();
                return;
            }

        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (m_points != null && m_points.Length > 0)
            {
                GeneratedUVs();
                vh.Clear();

                PopulateMesh(vh, m_points);

            }
            else if (m_segments != null && m_segments.Count > 0)
            {
                GeneratedUVs();
                vh.Clear();

                for (int s = 0; s < m_segments.Count; s++)
                {
                    Vector2[] pointsToDraw = m_segments[s];
                    PopulateMesh(vh, pointsToDraw);
                }
            }


        }

        private UIVertex[] CreateLineCap(Vector2 start, Vector2 end, SegmentType type)
        {
            if (type == SegmentType.Start)
            {
                var capStart = start - ((end - start).normalized * lineThickness / 2);
                return CreateLineSegment(capStart, start, SegmentType.Start);
            }
            else if (type == SegmentType.End)
            {
                var capEnd = end + ((end - start).normalized * lineThickness / 2);
                return CreateLineSegment(end, capEnd, SegmentType.End);
            }

            Debug.LogError("Bad SegmentType passed in to CreateLineCap. Must be SegmentType.Start or SegmentType.End");
            return null;
        }

        private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end, SegmentType type, UIVertex[] previousVert = null)
        {
            Vector2 offset = new Vector2((start.y - end.y), end.x - start.x).normalized * lineThickness / 2;

            Vector2 v1 = Vector2.zero;
            Vector2 v2 = Vector2.zero;
            if (previousVert != null)
            {
                v1 = new Vector2(previousVert[3].position.x, previousVert[3].position.y);
                v2 = new Vector2(previousVert[2].position.x, previousVert[2].position.y);
            }
            else
            {
                v1 = start - offset;
                v2 = start + offset;
            }

            var v3 = end + offset;
            var v4 = end - offset;
            //Return the VDO with the correct uvs
            switch (type)
            {
                case SegmentType.Start:
                    return SetVbo(new[] { v1, v2, v3, v4 }, startUvs);
                case SegmentType.End:
                    return SetVbo(new[] { v1, v2, v3, v4 }, endUvs);
                case SegmentType.Full:
                    return SetVbo(new[] { v1, v2, v3, v4 }, fullUvs);
                default:
                    return SetVbo(new[] { v1, v2, v3, v4 }, middleUvs);
            }
        }

        protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
        {
            UIVertex[] vbo = new UIVertex[4];
            for (int i = 0; i < vertices.Length; i++)
            {
                var vert = UIVertex.simpleVert;
                vert.color = color;
                vert.position = vertices[i];
                vert.uv0 = uvs[i];
                vbo[i] = vert;
            }
            return vbo;
        }

        protected void GeneratedUVs()
        {
            if (activeSprite != null)
            {
                var outer = Sprites.DataUtility.GetOuterUV(activeSprite);
                var inner = Sprites.DataUtility.GetInnerUV(activeSprite);
                UV_TOP_LEFT = new Vector2(outer.x, outer.y);
                UV_BOTTOM_LEFT = new Vector2(outer.x, outer.w);
                UV_TOP_CENTER_LEFT = new Vector2(inner.x, inner.y);
                UV_TOP_CENTER_RIGHT = new Vector2(inner.z, inner.y);
                UV_BOTTOM_CENTER_LEFT = new Vector2(inner.x, inner.w);
                UV_BOTTOM_CENTER_RIGHT = new Vector2(inner.z, inner.w);
                UV_TOP_RIGHT = new Vector2(outer.z, outer.y);
                UV_BOTTOM_RIGHT = new Vector2(outer.z, outer.w);
            }
            else
            {
                UV_TOP_LEFT = Vector2.zero;
                UV_BOTTOM_LEFT = new Vector2(0, 1);
                UV_TOP_CENTER_LEFT = new Vector2(0.5f, 0);
                UV_TOP_CENTER_RIGHT = new Vector2(0.5f, 0);
                UV_BOTTOM_CENTER_LEFT = new Vector2(0.5f, 1);
                UV_BOTTOM_CENTER_RIGHT = new Vector2(0.5f, 1);
                UV_TOP_RIGHT = new Vector2(1, 0);
                UV_BOTTOM_RIGHT = Vector2.one;
            }


            startUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_CENTER_LEFT, UV_TOP_CENTER_LEFT };
            middleUvs = new[] { UV_TOP_CENTER_LEFT, UV_BOTTOM_CENTER_LEFT, UV_BOTTOM_CENTER_RIGHT, UV_TOP_CENTER_RIGHT };
            endUvs = new[] { UV_TOP_CENTER_RIGHT, UV_BOTTOM_CENTER_RIGHT, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };
            fullUvs = new[] { UV_TOP_LEFT, UV_BOTTOM_LEFT, UV_BOTTOM_RIGHT, UV_TOP_RIGHT };
        }

        private int GetSegmentPointCount()
        {
            if (Segments?.Count > 0)
            {
                int pointCount = 0;
                foreach (var segment in Segments)
                {
                    pointCount += segment.Length;
                }
                return pointCount;
            }
            return Points.Length;
        }

        /// <summary>
        /// Get the Vector2 position of a line index
        /// </summary>
        /// <remarks>
        /// Positive numbers should be used to specify Index and Segment
        /// </remarks>
        /// <param name="index">Requied Index of the point, starting from point 1</param>
        /// <param name="segmentIndex">(optional) Required Segment the point is held in, Starting from Segment 1</param>
        /// <returns>Vector2 position of the point within UI Space</returns>
        public Vector2 GetPosition(int index, int segmentIndex = 0)
        {
            if (segmentIndex > 0)
            {
                return Segments[segmentIndex - 1][index - 1];
            }
            else if (Segments.Count > 0)
            {
                var segmentIndexCount = 0;
                var indexCount = index;
                foreach (var segment in Segments)
                {
                    if (indexCount - segment.Length > 0)
                    {
                        indexCount -= segment.Length;
                        segmentIndexCount += 1;
                    }
                    else
                    {
                        break;
                    }
                }
                return Segments[segmentIndexCount][indexCount - 1];
            }
            else
            {
                return Points[index - 1];
            }
        }

        /// <summary>
        /// Get the Vector2 position of a line within a specific segment
        /// </summary>
        /// <param name="index">Requied Index of the point, starting from point 1</param>
        /// <param name="segmentIndex"> Required Segment the point is held in, Starting from Segment 1</param>
        /// <returns>Vector2 position of the point within UI Space</returns>
        public Vector2 GetPositionBySegment(int index, int segment)
        {
            return Segments[segment][index - 1];
        }

        /// <summary>
        /// Get the closest point between two given Vector2s from a given Vector2 point
        /// </summary>
        /// <param name="p1">Starting postion</param>
        /// <param name="p2">End position</param>
        /// <param name="p3">Desired / Selected point</param>
        /// <returns>Closest Vector2 position of the target within UI Space</returns>
        public Vector2 GetClosestPoint(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            Vector2 from_p1_to_p3 = p3 - p1;
            Vector2 from_p1_to_p2 = p2 - p1;
            float dot = Vector2.Dot(from_p1_to_p3, from_p1_to_p2.normalized);
            dot /= from_p1_to_p2.magnitude;
            float t = Mathf.Clamp01(dot);
            return p1 + from_p1_to_p2 * t;
        }

        bool SetClass<T>(ref T currentValue, T newValue) where T : class
        {
            if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
                return false;

            currentValue = newValue;
            return true;
        }


        /**
        This class demonstrates the code discussed in these two articles:
        http://devmag.org.za/2011/04/05/bzier-curves-a-tutorial/
        http://devmag.org.za/2011/06/23/bzier-path-algorithms/
        Use this code as you wish, at your own risk. If it blows up
        your computer, makes a plane crash, or otherwise cause damage,
        injury, or death, it is not my fault.
        @author Herman Tulleken, dev.mag.org.za
        */

        /**
            Class for representing a Bezier path, and methods for getting suitable points to
            draw the curve with line segments.
        */
        class BezierPath
        {
            public int SegmentsPerCurve = 10;
            public float MINIMUM_SQR_DISTANCE = 0.01f;

            // This corresponds to about 172 degrees, 8 degrees from a straight line
            public float DIVISION_THRESHOLD = -0.99f;

            private List<Vector2> controlPoints;

            private int curveCount; //how many bezier curves in this path?

            /**
                Constructs a new empty Bezier curve. Use one of these methods
                to add points: SetControlPoints, Interpolate, SamplePoints.
            */
            public BezierPath()
            {
                controlPoints = new List<Vector2>();
            }

            /**
                Sets the control points of this Bezier path.
                Points 0-3 forms the first Bezier curve, points
                3-6 forms the second curve, etc.
            */
            public void SetControlPoints(List<Vector2> newControlPoints)
            {
                controlPoints.Clear();
                controlPoints.AddRange(newControlPoints);
                curveCount = (controlPoints.Count - 1) / 3;
            }

            public void SetControlPoints(Vector2[] newControlPoints)
            {
                controlPoints.Clear();
                controlPoints.AddRange(newControlPoints);
                curveCount = (controlPoints.Count - 1) / 3;
            }

            /**
                Returns the control points for this Bezier curve.
            */
            public List<Vector2> GetControlPoints()
            {
                return controlPoints;
            }


            /**
                Calculates a Bezier interpolated path for the given points.
            */
            public void Interpolate(List<Vector2> segmentPoints, float scale)
            {
                controlPoints.Clear();

                if (segmentPoints.Count < 2)
                {
                    return;
                }

                for (int i = 0; i < segmentPoints.Count; i++)
                {
                    if (i == 0) // is first
                    {
                        Vector2 p1 = segmentPoints[i];
                        Vector2 p2 = segmentPoints[i + 1];

                        Vector2 tangent = (p2 - p1);
                        Vector2 q1 = p1 + scale * tangent;

                        controlPoints.Add(p1);
                        controlPoints.Add(q1);
                    }
                    else if (i == segmentPoints.Count - 1) //last
                    {
                        Vector2 p0 = segmentPoints[i - 1];
                        Vector2 p1 = segmentPoints[i];
                        Vector2 tangent = (p1 - p0);
                        Vector2 q0 = p1 - scale * tangent;

                        controlPoints.Add(q0);
                        controlPoints.Add(p1);
                    }
                    else
                    {
                        Vector2 p0 = segmentPoints[i - 1];
                        Vector2 p1 = segmentPoints[i];
                        Vector2 p2 = segmentPoints[i + 1];
                        Vector2 tangent = (p2 - p0).normalized;
                        Vector2 q0 = p1 - scale * tangent * (p1 - p0).magnitude;
                        Vector2 q1 = p1 + scale * tangent * (p2 - p1).magnitude;

                        controlPoints.Add(q0);
                        controlPoints.Add(p1);
                        controlPoints.Add(q1);
                    }
                }

                curveCount = (controlPoints.Count - 1) / 3;
            }

            /**
                Sample the given points as a Bezier path.
            */
            public void SamplePoints(List<Vector2> sourcePoints, float minSqrDistance, float maxSqrDistance, float scale)
            {
                if (sourcePoints.Count < 2)
                {
                    return;
                }

                Stack<Vector2> samplePoints = new Stack<Vector2>();

                samplePoints.Push(sourcePoints[0]);

                Vector2 potentialSamplePoint = sourcePoints[1];

                int i = 2;

                for (i = 2; i < sourcePoints.Count; i++)
                {
                    if (
                        ((potentialSamplePoint - sourcePoints[i]).sqrMagnitude > minSqrDistance) &&
                        ((samplePoints.Peek() - sourcePoints[i]).sqrMagnitude > maxSqrDistance))
                    {
                        samplePoints.Push(potentialSamplePoint);
                    }

                    potentialSamplePoint = sourcePoints[i];
                }

                //now handle last bit of curve
                Vector2 p1 = samplePoints.Pop(); //last sample point
                Vector2 p0 = samplePoints.Peek(); //second last sample point
                Vector2 tangent = (p0 - potentialSamplePoint).normalized;
                float d2 = (potentialSamplePoint - p1).magnitude;
                float d1 = (p1 - p0).magnitude;
                p1 = p1 + tangent * ((d1 - d2) / 2);

                samplePoints.Push(p1);
                samplePoints.Push(potentialSamplePoint);


                Interpolate(new List<Vector2>(samplePoints), scale);
            }

            /**
                Caluclates a point on the path.
               
                @param curveIndex The index of the curve that the point is on. For example,
                the second curve (index 1) is the curve with controlpoints 3, 4, 5, and 6.
               
                @param t The paramater indicating where on the curve the point is. 0 corresponds
                to the "left" point, 1 corresponds to the "right" end point.
            */
            public Vector2 CalculateBezierPoint(int curveIndex, float t)
            {
                int nodeIndex = curveIndex * 3;

                Vector2 p0 = controlPoints[nodeIndex];
                Vector2 p1 = controlPoints[nodeIndex + 1];
                Vector2 p2 = controlPoints[nodeIndex + 2];
                Vector2 p3 = controlPoints[nodeIndex + 3];

                return CalculateBezierPoint(t, p0, p1, p2, p3);
            }

            /**
                Gets the drawing points. This implementation simply calculates a certain number
                of points per curve.
            */
            public List<Vector2> GetDrawingPoints0()
            {
                List<Vector2> drawingPoints = new List<Vector2>();

                for (int curveIndex = 0; curveIndex < curveCount; curveIndex++)
                {
                    if (curveIndex == 0) //Only do this for the first end point.
                                         //When i != 0, this coincides with the
                                         //end point of the previous segment,
                    {
                        drawingPoints.Add(CalculateBezierPoint(curveIndex, 0));
                    }

                    for (int j = 1; j <= SegmentsPerCurve; j++)
                    {
                        float t = j / (float)SegmentsPerCurve;
                        drawingPoints.Add(CalculateBezierPoint(curveIndex, t));
                    }
                }

                return drawingPoints;
            }

            /**
                Gets the drawing points. This implementation simply calculates a certain number
                of points per curve.
                This is a lsightly different inplementation from the one above.
            */
            public List<Vector2> GetDrawingPoints1()
            {
                List<Vector2> drawingPoints = new List<Vector2>();

                for (int i = 0; i < controlPoints.Count - 3; i += 3)
                {
                    Vector2 p0 = controlPoints[i];
                    Vector2 p1 = controlPoints[i + 1];
                    Vector2 p2 = controlPoints[i + 2];
                    Vector2 p3 = controlPoints[i + 3];

                    if (i == 0) //only do this for the first end point. When i != 0, this coincides with the end point of the previous segment,
                    {
                        drawingPoints.Add(CalculateBezierPoint(0, p0, p1, p2, p3));
                    }

                    for (int j = 1; j <= SegmentsPerCurve; j++)
                    {
                        float t = j / (float)SegmentsPerCurve;
                        drawingPoints.Add(CalculateBezierPoint(t, p0, p1, p2, p3));
                    }
                }

                return drawingPoints;
            }

            /**
                This gets the drawing points of a bezier curve, using recursive division,
                which results in less points for the same accuracy as the above implementation.
            */
            public List<Vector2> GetDrawingPoints2()
            {
                List<Vector2> drawingPoints = new List<Vector2>();

                for (int curveIndex = 0; curveIndex < curveCount; curveIndex++)
                {
                    List<Vector2> bezierCurveDrawingPoints = FindDrawingPoints(curveIndex);

                    if (curveIndex != 0)
                    {
                        //remove the fist point, as it coincides with the last point of the previous Bezier curve.
                        bezierCurveDrawingPoints.RemoveAt(0);
                    }

                    drawingPoints.AddRange(bezierCurveDrawingPoints);
                }

                return drawingPoints;
            }

            List<Vector2> FindDrawingPoints(int curveIndex)
            {
                List<Vector2> pointList = new List<Vector2>();

                Vector2 left = CalculateBezierPoint(curveIndex, 0);
                Vector2 right = CalculateBezierPoint(curveIndex, 1);

                pointList.Add(left);
                pointList.Add(right);

                FindDrawingPoints(curveIndex, 0, 1, pointList, 1);

                return pointList;
            }


            /**
                @returns the number of points added.
            */
            int FindDrawingPoints(int curveIndex, float t0, float t1,
                List<Vector2> pointList, int insertionIndex)
            {
                Vector2 left = CalculateBezierPoint(curveIndex, t0);
                Vector2 right = CalculateBezierPoint(curveIndex, t1);

                if ((left - right).sqrMagnitude < MINIMUM_SQR_DISTANCE)
                {
                    return 0;
                }

                float tMid = (t0 + t1) / 2;
                Vector2 mid = CalculateBezierPoint(curveIndex, tMid);

                Vector2 leftDirection = (left - mid).normalized;
                Vector2 rightDirection = (right - mid).normalized;

                if (Vector2.Dot(leftDirection, rightDirection) > DIVISION_THRESHOLD || Mathf.Abs(tMid - 0.5f) < 0.0001f)
                {
                    int pointsAddedCount = 0;

                    pointsAddedCount += FindDrawingPoints(curveIndex, t0, tMid, pointList, insertionIndex);
                    pointList.Insert(insertionIndex + pointsAddedCount, mid);
                    pointsAddedCount++;
                    pointsAddedCount += FindDrawingPoints(curveIndex, tMid, t1, pointList, insertionIndex + pointsAddedCount);

                    return pointsAddedCount;
                }

                return 0;
            }



            /**
                Caluclates a point on the Bezier curve represented with the four controlpoints given.
            */
            private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
            {
                float u = 1 - t;
                float tt = t * t;
                float uu = u * u;
                float uuu = uu * u;
                float ttt = tt * t;

                Vector2 p = uuu * p0; //first term

                p += 3 * uu * t * p1; //second term
                p += 3 * u * tt * p2; //third term
                p += ttt * p3; //fourth term

                return p;

            }
        }


    }
}
