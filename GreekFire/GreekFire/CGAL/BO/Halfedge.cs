using System;
using System.Diagnostics;
using System.Text;
using TriangleNet;
using TriangleNet.Topology.DCEL;
using static CGAL.Mathex;


namespace CGAL
{
    using static DebugLog;  
    public class Halfedge
    {
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string r = "";

            if (IsBorder)
            {
                sb.Append((Id % 2 == 1) ? "c" : "o");
            }
            else if (IsConstrain)
            {
                sb.Append("w");
            }
            else if (IsKinetic)
            {
                sb.Append("k");
            }

            {
                if (Slope == SignEnum.POSITIVE)
                {
                    sb.Append(IsInnerBisector ? "r" : "R");
                }
                if (Slope == SignEnum.ZERO)
                {
                    sb.Append(IsInnerBisector ? "l" : "L");
                }
                else
                {
                    sb.Append("?");
                }
            }
            sb.Append($" {this.Opposite?.Vertex}->{Vertex} F{Face.Id} ");

            return sb.ToString();
        }

        public static Halfedge NULL { get; private set; }

        static Halfedge()
        {
            NULL = new Halfedge(-1);
        }

        public Halfedge(int aID, SignEnum aSlope = SignEnum.ZERO) : this(aID, false, aSlope)
        {
        }

        public Halfedge(int aID, bool isKinetic, SignEnum aSlope = SignEnum.ZERO)
        {
            Id = aID;
            Face = Face.NULL;
            Vertex = Vertex.NULL;
            Slope = aSlope;
            Opposite = NULL;
            Next = NULL;
            Prev = NULL;
            IsKinetic = isKinetic;
        }

        public virtual IEnumerable<Halfedge> Halfedges => Circulation(IsKinetic ? 3 : 100000);

        public IEnumerable<Halfedge> Circulation(int max = 100000)
        {
            Halfedge curr = this;
            int i = 0;
            if (curr == null)
            {
                throw new InvalidOperationException($"{this} has not Halfedge asigned");
            }
            do
            {
                yield return curr;
                curr = curr.Next;

                if (curr == null)
                {
                    throw new InvalidOperationException($"{this} has not Halfedge asigned");
                }
                if (max < i++)
                {
                    throw new GFLException($" {this} iterate more than {max} "); break;
                }
            } while (curr != this);
        }

        public double SquaredLength(double time)
        {
            return Mathex.squared_distance(Vertex.PointAt(time), Prev.Vertex.PointAt(time));
        }

        public Halfedge CrossLink(Halfedge next)
        {
            //Debug.Assert(prev.Opposite_.Vertex_ == next.Vertex_);
            //  Debug.Assert(next.Opposite_.Vertex_ == prev.Vertex_);

            this.Next = next;
            
            next.Prev = this;
            return next;
        }

        public Vertex vertex() => validate(this.Vertex);

        public double weight() => Weight;

        public Halfedge Opposite { get; set; }
        public Halfedge Next { get; set; }

        public SignEnum Slope { get; set; }
        public Halfedge Prev { get; set; }
        public Vertex Vertex { get; set; }
        public Face Face { get; set; }

        public List<Vertex> LAVList { get; private set; } = new List<Vertex>();

        private WavefrontEdge? _wavefrontEdge;

        public WavefrontEdge WavefrontEdge
        {
            get { Debug.Assert(_wavefrontEdge != null); return _wavefrontEdge; }
            set
            {
                if (!( Face is KineticTriangle triangle)) throw new InvalidCastException($"{Face} is not {nameof(KineticTriangle)}");
                
                _wavefrontEdge = value;
                triangle.InvalidateEvent();
            }
        }
        public void RemoveWavefrontEdge()
        {
            _wavefrontEdge = null;
            if(! (Face is KineticTriangle triangle)) throw new InvalidCastException($"{Face} is not {nameof(KineticTriangle)}");  
            triangle.InvalidateEvent();
        }

        public bool IsWavefrontEdge => _wavefrontEdge != null;

        public Vector2 Vector
        {
            get
            {
                Point2 s = Opposite.Vertex.Point;
                Point2 t = Vertex.Point;

                return validate(new Vector2(s, t));
            }
        }

        public bool AssertVertex()
        {
            var result = (Prev.Vertex == Opposite.Vertex);
            if (!result)
                Log($"ER: Prev:{Prev.Vertex.Id} Opp{Opposite.Vertex.Id} =>{Vertex.Id} ");
            return result;
        }

        public Direction2 Direction => new Direction2(Vector);
        public int Id { get; internal set; }

        public int id() => Id;

        public bool IsBorder { get => (this.Face == null || this.Face.Outside != -1) || Face.Halfedge == this; }
        public bool IsBisector => !this.IsBorder;

        public bool IsKinetic { get; private set; }

        public bool IsInnerBisector => !this.Vertex.IsContour() && !this.Opposite.Vertex.IsContour();
        public bool HasNullSegment => this.Vertex.has_null_point() || this.Opposite.Vertex.has_null_point();

        public bool IsValid => this != Halfedge.NULL;
        public Segment2 Segment => new Segment2(Vertex.Point, Opposite.Vertex.Point);

        public double Weight { get; internal set; } = 1;

        internal bool is_border() => IsBorder;

        internal bool is_inner_bisector() => IsInnerBisector;

        internal bool has_null_segment => HasInfiniteTime;

        internal bool is_bisector() => IsBisector;

        public bool IsCollapse { get; set; }

        public string ID
        {
            get
            {
                string r = "";
                if (IsBorder)
                {
                    r = (Id % 2 == 1) ? "C" : "O";
                }
                else
                {
                    if (Slope == SignEnum.POSITIVE)
                    {
                        r = IsInnerBisector ? "r" : "R";
                    }
                    if (Slope == SignEnum.ZERO)
                    {
                        r = IsInnerBisector ? "l" : "L";
                    }
                    else
                    {
                        r = "?";
                    }
                }
                r += String.Format("{0}", Face != Face.NULL ? Face.Id.ToString() : IsBorder ? Opposite.Face.Id.ToString() : "-");
                return r;
            }
        }

        public bool HasInfiniteTime => this.Vertex.has_infinite_time();

        public bool IsConstrain => this.IsKinetic && !this.Opposite.IsKinetic;

        private Vector2? normal { get; set; }

        public Vector2 Normal { get; internal set; }

        public KineticTriangle Neighbor
        {
            get
            {
                Debug.Assert(this.Opposite != null);
                Debug.Assert(this.Opposite.Face is KineticTriangle);
                return (KineticTriangle)this.Opposite.Face;
            }
        }

        public bool HasWavefrontEdge => IsConstrain;

        public Vector2 GetNormal()
        {
            if (normal != null) return normal.Value;
            if (!this.IsValid) throw new IndeterminateValueException($"H{Id}- is null)");
            if (!this.Next.IsValid) throw new IndeterminateValueException($"H{Id}- Opposite is null)");

            if (HasInfiniteTime)
                throw new IndeterminateValueException($"H{Id}-{this} infinite ");

            if (Opposite.HasInfiniteTime)
                throw new IndeterminateValueException($"H{Id}-{this} oposite infinite");

            normal = new Vector2(Vertex.Point, Next.Vertex.Point).Normal();
            return normal.Value;
        }

        internal Halfedge defining_contour_edge()
        { return this.Face.halfedge(); }

        internal SignEnum slope()
        { return Slope; }

        internal void set_opposite(Halfedge h)
        { Opposite = h; }

        internal void set_next(Halfedge h)
        { Next = h; }

        internal void set_prev(Halfedge h)
        { Prev = h; }

        public void set_vertex(Vertex w)
        {
            Vertex = w;
        }

        internal void set_face(Face g)
        { Face = g; }

        internal void set_slope(SignEnum aSlope)
        { Slope = aSlope; }

        internal void reset_id(int aID)
        { Id = aID; }

        internal void set_weight(double lWeight)
        {
            Weight = lWeight;
        }

        internal bool AssertFaces()
        {
          bool  result =  this.Halfedges.All(h => h.Face == this.Face);
            if (!result)
            {

            }
            return result;
        }

        internal string LogVIds => $"{Prev.Vertex.Id}(o{Opposite.Vertex.Id})->v{Vertex.Id}";
      
    }
}