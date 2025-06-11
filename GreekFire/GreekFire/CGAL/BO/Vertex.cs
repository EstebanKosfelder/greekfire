using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;
using FT = System.Double;

namespace CGAL
{
    public partial class Vertex
    {

       
        internal string IDPrintable
        {
            get
            {
                var r = Id.ToString();
                if (r.Length < Vertex.MaxDigit) r = r.PadLeft(MaxDigit);
                return r;

            }
        }

        public string sskAnge => IsReflex ? "r" : IsStraight ? "s" : "c";

        internal void _setMaxDigit(int id)=>MaxDigit = Math.Max(id.ToString().Length,MaxDigit);
        static Vertex()
        {
            NULL = new Vertex(-1);
        }

        // Infinite vertex
        public Vertex(int id, Point2 point, VertexType type)
            : this(id, point, 0,  type , VertexFlags.None ) { }

        public Vertex(int id) : this(id, Point2.NAN, FT.NaN,VertexType.Wavefront, VertexFlags.HasInfiniteTimeBit)
        {
        }

        public Vertex(int id, Point2 point, FT time, bool isSplit, bool isInfiniteTime)
            : this(id, point, time, VertexType.Wavefront, (isSplit ? VertexFlags.IsSplitBit : 0) | (isInfiniteTime ? VertexFlags.HasInfiniteTimeBit : 0)) { }

        public Vertex(int id, Point2 point, FT time,VertexType type, VertexFlags flag)
          

        {
            Id = id;
            Point = point;
            Time = time;
            Type = type;
            Flags = flag;

            IsSplitBit = flag.HasFlag(VertexFlags.IsSplitBit);
            IsReflex = (false);
            IsStraight = (false);
            IsProcessed = (false);
            IsExcluded = (false);
            PrevInLAV = Vertex.NULL;
            NextInLAV = Vertex.NULL;
            NextSplitEventInMainPQ = (false);
            EventTrieger = Triedge.NULL;
        }
        public Vertex(int id, Point2 position, WavefrontEdge left, WavefrontEdge right, bool is_initial, bool is_beveling) 
            :this(id, position, position, left, right, is_initial, is_beveling)
            
        { 
        
        }

        public Vertex(int id, Point2 pos_zero, Point2 pos_start, WavefrontEdge left, WavefrontEdge right, bool is_initial, bool is_beveling)
            :this(id,pos_start,VertexType.Wavefront)

        {
            
            this.pos_zero = pos_zero;
            this.incident_wavefront_edges = new WavefrontEdge[] { left, right };
            this.is_initial = is_initial;
            this.is_beveling = is_beveling;
            this.angle = ((left != null && right != null) ? (EAngle)Mathex.orientation(left.SupportLine.l.to_vector(), right.SupportLine.l.to_vector()) : EAngle.Straight);
            this.IsReflex = angle == EAngle.Reflex;
            this.IsStraight = angle == EAngle.Straight;
            velocity = (left != null && right != null) ? compute_velocity(pos_zero, left.SupportLine, right.SupportLine, angle) : Vector2.NaN;
            px_ = new Polynomial1D(pos_zero.X, velocity.X);
            py_ = new Polynomial1D(pos_zero.Y, velocity.Y);
            
            next_vertex_ = new Vertex?[] { null, null };
            prev_vertex_ = new Vertex?[] { null, null };



        }

        public Vertex(int id, Point2 pos, double time, WavefrontEdge left, WavefrontEdge right, bool from_split =false)
        {

            //DBG_FUNC_BEGIN(DBG_KT);
            //DBG(DBG_KT) << "a:" << *a << " " << CGAL_line(a.l().l);
            //DBG(DBG_KT) << "b:" << *b << " " << CGAL_line(b.l().l);

            if (!from_split)
            {
                Debug.Assert(left.vertex(1) != null && left.vertex(1).has_stopped());
                Debug.Assert(right.vertex(0) != null && right.vertex(0).has_stopped());
            }

            Point2 pos_zero;


            Intersection lit = Mathex.intersection(left.SupportLine.l, right.SupportLine.l);
            switch (lit.Result)
            {
                case Intersection.Intersection_results.LINE:
                    pos_zero = pos - (compute_velocity(Vector2.Zero, left.SupportLine, right.SupportLine, EAngle.Straight) * time);
                    break;
                // fall through
                case Intersection.Intersection_results.POINT:
                    pos_zero = lit.Points[0];
                    break;
                case Intersection.Intersection_results.NO_INTERSECTION:
                    //  DBG(DBG_KT) << "No intersection at time 0 between supporting lines of wavefrontedges.  Parallel wavefronts crashing (or wavefronts of different speeds becoming collinear).";
                    pos_zero = pos;
                    break;
                default:
                    // CANNOTHAPPEN_MSG << "Fell through switch which should cover all cases.";
                    Debug.Assert(false);
                    throw new Exception("Fell through switch which should cover all cases.");

            }

            this.pos_zero = pos_zero;
            this.pos_start = pos_start;
            this.incident_wavefront_edges = new WavefrontEdge[] { left, right };
            this.is_initial = false;
            this.is_beveling = true;
            this.angle = ((left != null && right != null) ? (EAngle)Mathex.orientation(left.SupportLine.l.to_vector(), right.SupportLine.l.to_vector()) : EAngle.Straight);
            this.IsReflex = angle == EAngle.Reflex;
            this.IsStraight = angle == EAngle.Straight;

            velocity = (left != null && right != null) ? compute_velocity(pos_zero, left.SupportLine, right.SupportLine, angle) : Vector2.NaN;
            px_ = new Polynomial1D(pos_zero.X, velocity.X);
            py_ = new Polynomial1D(pos_zero.Y, velocity.Y);
            //  skeleton_dcel_halfedge_ = new SkeletonDCELHalfedge?[] { null, null };
            next_vertex_ = new Vertex?[] { null, null };
            prev_vertex_ = new Vertex?[] { null, null };



            //      Debug.Assert((lit == LineIntersectionType.NONE) == (v.infinite_speed != InfiniteSpeedType.NONE));

            Debug.Assert(this.PointAt(time) == pos);
            //DBG_FUNC_END(DBG_KT);

        }
        [Flags]
        public enum VertexFlags
        { None = 0x0, IsSplitBit = 0x01, HasInfiniteTimeBit = 0x02 }

        public static Vertex NULL { get; private set; }
        public Halfedge EdgeEnding => this.mTriedge.E0;

        public Halfedge EdgeStarting => NextInLAV.EdgeEnding;

        public Triedge EventTrieger { get; internal set; }

        public VertexFlags Flags { get; internal set; }

        public Halfedge Halfedge { get; set; } = Halfedge.NULL;

        public bool HasInfiniteTime => Flags.HasFlag(VertexFlags.HasInfiniteTimeBit);

        private int _id;
        public int Id { 
            get=>_id; 
            internal set {
                _setMaxDigit(value);
                _id = value;
                
            }
        }

        public bool IsConvex { get => !IsReflex && !IsStraight; }

        public bool IsStraight { get; internal set; }

        public bool IsExcluded { get; internal set; }

        public bool IsProcessed { get; internal set; }

        public bool IsReflex { get; internal set; }

        public bool IsSkeleton => Halfedge.IsBisector;

        public bool IsSplitBit { get; internal set; }

        ////#if MYDEBUG
        ////        public Vector2 Bisector = new Vector2(0,0);
        ////#endif
        public bool IsValid => this != Vertex.NULL;

        public bool mHasSimultaneousEvents { get; internal set; }

        public Triedge mTriedge { get; internal set; } = Triedge.NULL;

        public Vertex NextInLAV { get; internal set; }

        public bool NextSplitEventInMainPQ { get; internal set; }

        public Point2 Point { get; private set; }

        public Vertex PrevInLAV { get; internal set; }

        public PriorityQueue<Event, Event> SplitEvents { get; internal set; }

        public FT Time { get; internal set; }

        // Here, E0,E1 corresponds to the vertex (unlike *event* triedges)
        public Trisegment Trisegment { get; internal set; } = Trisegment.NULL;

        internal int Degree => Circulation().Count();

        // Skeleton nodes cache the full trisegment tree that defines the originating event
        public IEnumerable<Halfedge> Circulation()
        {
            Halfedge curr = Halfedge;
            if (curr == Halfedge.NULL) yield break;
            do
            {
                yield return curr;
                curr = curr.Opposite.Prev;
            } while (curr != Halfedge.NULL && curr != Halfedge);
        }

        public bool has_infinite_time() => Flags.HasFlag(VertexFlags.HasInfiniteTimeBit);

        public bool is_contour() => IsContour();

        public bool is_skeleton() => IsSkeleton;

        public bool is_split() => IsSplitBit;

        public bool IsContour()
        {
            return !Halfedge.IsBisector;
        }

        public Point2 point() => Point;

        public Halfedge primary_bisector() => halfedge().Next;

        public void reset_id__internal__(int aID)
        { Id = aID; }

        public void reset_point__internal__(in Point2 aP)
        { Point = aP; }
        internal Triedge event_triedge() => mTriedge;

        internal Halfedge halfedge() => Halfedge;

        // TOCHECK
        internal bool has_null_point()
        {
            return !Point.IsFinite();
        }

      
        internal void reset_id(int aID)
        {
            Id = aID;
        }

        internal void reset_point(Point2 aP)
        {
            Point = aP;
        }

        internal void set_event_triedge(Triedge aTriedge) => mTriedge = aTriedge;

        internal void set_halfedge(Halfedge aHE)
        {
            Halfedge = aHE;
        }
        internal FT time()
        {
            return Time;
        }


      

    }
}