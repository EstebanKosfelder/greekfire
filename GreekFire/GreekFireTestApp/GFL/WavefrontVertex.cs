using System.Text.RegularExpressions;
using System;
using System.Diagnostics;
using GFL.Kernel;

namespace GFL
{
    public enum LineIntersectionType
    {
        ONE,   /* the two lines intersect in one point */
        ALL,   /* the two lines are parallel and coincide */
        NONE,  /* the two liens are parallel and distinct */
    };

  
    public class WavefrontVertex : IVertex
    {
        public static int MaxDigit = 1;
        public Vector2D intersection = Vector2D.NaN;

        public bool is_beveling;

        public bool is_infinite;

        public bool is_initial;

        public Vector2D pos_zero;

        public double time_start;

        //  public InfiniteSpeedType infinite_speed; /** This wavefront vertex is
        //either between parallel, opposing wavefront elements that have crashed
        //into each other and become collinear, or it is between neighboring
        //wavefront edges that have become collinear yet have different weights. */
        public Vector2D velocity;

        internal EAngle angle;

        internal WavefrontEdge?[] incident_wavefront_edges;

        private bool has_stopped_ = false;

        private bool is_degenerate_ = false;

        private WavefrontVertex?[] next_vertex_ = new WavefrontVertex[2];

        private Vector2D point_2;

        private Vector2D pos_stop_;

        private WavefrontVertex?[] prev_vertex_ = new WavefrontVertex[2];

        private Polynomial1D px_, py_;

        private double time_stop_;

        public WavefrontVertex(int id, Vector2D position, WavefrontEdge left, WavefrontEdge right, bool is_initial, bool is_beveling) :
                   this(id, position, position, left, right, is_initial, is_beveling)
        { }

        public WavefrontVertex(int id, Vector2D pos_zero, Vector2D pos_start, WavefrontEdge left, WavefrontEdge right, bool is_initial, bool is_beveling)
        {
            this.ID = id;
            this.pos_zero = pos_zero;
            this.pos_start = pos_start;
            this.incident_wavefront_edges = new WavefrontEdge[] { left, right };
            this.is_initial = is_initial;
            this.is_beveling = is_beveling;
            this.angle = ((left != null && right != null) ? (EAngle)MathEx.orientation(left.l().l.to_vector(), right.l().l.to_vector()) : EAngle.Straight);
            velocity = (left != null && right != null) ? compute_velocity(pos_zero, left.SupportLine, right.SupportLine, angle) : Vector2D.NaN;
            px_ = new Polynomial1D(pos_zero.X, velocity.X);
            py_ = new Polynomial1D(pos_zero.Y, velocity.Y);
            //  skeleton_dcel_halfedge_ = new SkeletonDCELHalfedge?[] { null, null };
            next_vertex_ = new WavefrontVertex?[] { null, null };
            prev_vertex_ = new WavefrontVertex?[] { null, null };



        }

        public WavefrontVertex(int id, Vector2D pos, double time, WavefrontEdge left, WavefrontEdge right, bool from_split)
        {

            //DBG_FUNC_BEGIN(DBG_KT);
            //DBG(DBG_KT) << "a:" << *a << " " << CGAL_line(a.l().l);
            //DBG(DBG_KT) << "b:" << *b << " " << CGAL_line(b.l().l);

            if (!from_split)
            {
                Debug.Assert(left.vertex(1) != null && left.vertex(1).has_stopped());
                Debug.Assert(right.vertex(0) != null && right.vertex(0).has_stopped());
            }

            Vector2D pos_zero;
            LineIntersectionType lit;
            (lit, pos_zero) = compute_intersection(left.l().l, right.l().l);
            switch (lit)
            {
                case LineIntersectionType.ALL:
                    pos_zero = pos - (Vector2D)compute_velocity(Vector2D.ORIGIN, left.SupportLine, right.SupportLine, EAngle.Straight) * time;
                    break;
                // fall through
                case LineIntersectionType.ONE:
                    pos_zero = pos - (Vector2D)compute_velocity(Vector2D.ORIGIN, left.SupportLine, right.SupportLine, EAngle.Straight) * time;
                    break;
                case LineIntersectionType.NONE:
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
            this.angle = ((left != null && right != null) ? (EAngle)MathEx.orientation(left.SupportLine.l.to_vector(), right.SupportLine.l.to_vector()) : EAngle.Straight);
            velocity = (left != null && right != null) ? compute_velocity(pos_zero, left.SupportLine, right.SupportLine, angle) : Vector2D.NaN;
            px_ = new Polynomial1D(pos_zero.X, velocity.X);
            py_ = new Polynomial1D(pos_zero.Y, velocity.Y);
            //  skeleton_dcel_halfedge_ = new SkeletonDCELHalfedge?[] { null, null };
            next_vertex_ = new WavefrontVertex?[] { null, null };
            prev_vertex_ = new WavefrontVertex?[] { null, null };



            //      Debug.Assert((lit == LineIntersectionType.NONE) == (v.infinite_speed != InfiniteSpeedType.NONE));

            Debug.Assert(this.p_at(time) == pos);
            //DBG_FUNC_END(DBG_KT);

        }

        public int Degree => throw new NotImplementedException();

        public IHalfedge Halfedge { get; set; }

        public int ID { get; set; }

        public string IDPrintable
        {
            get
            {
                var r = ID.ToString();
                if (r.Length < MaxDigit) r = r.PadLeft(MaxDigit);
                return r;

            }
        }
        public bool IsConvex => this.angle == EAngle.Convex;
        public bool IsConvexOrStraight => angle != EAngle.Reflex;
        public bool IsNotConvex => this.angle != EAngle.Convex;
        public bool IsNotReflex => this.angle != EAngle.Reflex;
        public bool IsReflex => this.angle == EAngle.Reflex;
        public bool IsReflexOrStraight => angle != EAngle.Convex;

        public Vector2D Point { get; set; }

        public Vector2D pos_start { get => Point; set => this.Point = value; }

        public Vector2D compute_velocity(
          Vector2D pos_zero,
          WavefrontSupportingLine a,
          WavefrontSupportingLine b,
          EAngle angle)
        {
            Vector2D result;

            if (angle != EAngle.Straight)
            {
                Line2D la = a.line_at_one();
                Line2D lb = b.line_at_one();

                (var lit, var intersect) = compute_intersection(la, lb);
                if (lit != LineIntersectionType.ONE)
                {
                    // CANNOTHAPPEN_MSG << "No point intersection between WavefrontEmittingEdges at offset 1.  Bad.";
                    Debug.Assert(false);
                    throw new Exception("No point intersection between WavefrontEmittingEdges at offset 1.  Bad.");
                }
                intersection = intersect;
                result = (Vector2D)(intersect - pos_zero);
            }
            else
            {
                //DBG(DBG_KT) << "a:" << CGAL_vector(a.normal);
                //DBG(DBG_KT) << "b:" << CGAL_vector(b.normal);

                if (MathEx.orientation(a.l.to_vector(), b.l.to_vector().Perpendicular(ESign.CLOCKWISE)) == (int)ETurn.Right)
                {
                    /* They are in the same direction */
                    if (a.normal == b.normal)
                    {
                        result = a.normal;
                    }
                    else
                    {
                        throw new Exception("collinear incident wavefront edges with different speeds.");

                    }
                }
                else
                {
                    throw new Exception("This is an infinitely fast vertex.  We should not be in compute_velocity().");
                }
            }
            return result;
        }

        public Vector2D DebugPointAt(double time)
        {
            if (this.has_stopped()) return pos_stop_;
            return p_at(time);
        }

        // protected static partial InfiniteSpeedType get_infinite_speed_type(WavefrontEdge a, WavefrontEdge b, ESign angle);
        // public bool is_straight() const { return angle == STRAIGHT; }
        public bool has_stopped() { return has_stopped_; }

        public WavefrontEdge incident_wavefront_edge(int i)
        {
            Debug.Assert(i <= 1);
            return incident_wavefront_edges[i];
        }

        public WavefrontVertex next_vertex(int side)
        {
            Debug.Assert(side <= 1);
            return next_vertex_[side];
        }

        public Vector2D p_at(double t)
        {
            Debug.Assert(!has_stopped_ || t <= time_stop_);
            Debug.Assert(!is_infinite);
            return (Vector2D)(pos_zero + (Vector2D)(velocity * t));
        }

        public Vector2D p_at_draw(double t)
        {
            Debug.Assert(!is_infinite);
            bool return_stop_pos;

            if (has_stopped_)
            {
                if (t < time_stop_)
                {
                    return_stop_pos = false;
                }
                else
                {
                    return_stop_pos = true;
                };
            }
            else
            {
                return_stop_pos = false;
            }

            return return_stop_pos ? pos_stop_
                                   : (pos_zero + (Vector2D)velocity * t);
        }

        public Vector2D pos_stop() { return pos_stop_; }

        public WavefrontVertex prev_vertex(int side)
        {
            Debug.Assert(side <= 1);
            return prev_vertex_[side];
        }

        public Polynomial1D px()
        {
            Debug.Assert(!is_infinite);
            Debug.Assert(!has_stopped_);
            return px_;
        }

        //  std.string details() const;
        public Polynomial1D py()
        {
            Debug.Assert(!is_infinite);
            Debug.Assert(!has_stopped_);
            return py_;
        }

        public void set_incident_wavefront_edge(int i, WavefrontEdge e)
        {
            Debug.Assert(i <= 1);
            Debug.Assert(incident_wavefront_edges[i] != null);
            Debug.Assert(e != null);
            Debug.Assert(incident_wavefront_edges[i].l() == e.l());
            incident_wavefront_edges[i] = e;
        }

        public void stop(double t)
        {
            Debug.Assert(!has_stopped_);

            time_stop_ = t;
            pos_stop_ = p_at(t);
            has_stopped_ = true;

            if (pos_stop_ == pos_start)
            {
                is_degenerate_ = true;
                Debug.Assert(time_stop_ == time_start);
            };
        }

        public void stop(double t, Vector2D p)
        {
            Debug.Assert(!has_stopped_);

            time_stop_ = t;
            pos_stop_ = p;
            has_stopped_ = true;

            Debug.Assert(time_stop_ == time_start);

            if (pos_stop_ == pos_start)
            {
                is_degenerate_ = true;
            };
        }

        //  friend std.ostream& operator<<(std.ostream& os, const WavefrontVertex.LineIntersectionType t);
        public double time_stop() { return time_stop_; }

        public override string ToString()
        {
            return $"kv:{IDPrintable}{angle.ToString()[0]}";
        }
         /* if pos_stop == pos_start */
        //  private SkeletonDCELHalfedge?[] skeleton_dcel_halfedge_;

        /* wavefront vertices form a doubly-linked edge list to represent
         * the boundary of their left(0) and right(1) incident faces.
         *
         * prev points to the wavefront vertex earlier in time, next to
         * the one later in time, so when traversing a face, care needs
         * to be taken at each arc (i.e. wavefront-vertex) wrt direction.
         */
        //public WavefrontVertex( int id,
        //            Vector2D p_pos_zero,
        //            Vector2D p_pos_start,
        //            double p_time_start,
        //            WavefrontEdge? a = null,
        //            WavefrontEdge? b = null,
        //            bool p_is_initial = false,
        //            bool p_is_beveling = false
        //            ) 

        //{


        //    //  id = kvctr++;

        //    pos_zero = (p_pos_zero);
        //    pos_start = (p_pos_start);
        //    time_start = (p_time_start);
        //    incident_wavefront_edges = new WavefrontEdge?[] { a, b };
        //    angle = (a != null && b != null) ? MathEx.orientation(a.l().l.to_vector(), b.l().l.to_vector()) : (int) ESign.STRAIGHT;
        //    is_initial = (p_is_initial);
        //    is_beveling = (p_is_beveling);
        //    is_infinite = (p_is_infinite);
        //    infinite_speed = get_infinite_speed_type(a, b, angle);
        //    velocity = ((a != null && b != null && (infinite_speed == InfiniteSpeedType.NONE)) ? compute_velocity(pos_zero, a.l(), b.l(), angle) : Vector2D.NaN);
        //    px_ = ((infinite_speed != InfiniteSpeedType.NONE) ? new Polynomial_1(0) : new Polynomial_1(pos_zero.X, velocity.X));
        //    py_ = ((infinite_speed != InfiniteSpeedType.NONE) ? new Polynomial_1(0) : new Polynomial_1(pos_zero.Y, velocity.Y));
        //  //  skeleton_dcel_halfedge_ = new SkeletonDCELHalfedge?[] { null, null };
        //    next_vertex_ = new WavefrontVertex?[] { null, null };
        //    prev_vertex_ = new WavefrontVertex?[] { null, null };


        //    // Debug.Assert(!!a == !!b);
        //}



        /** type of intersections for two lines */
        //KineticTriangle const * const * triangles() const { return incident_triangles; };
        public WavefrontEdge[] wavefronts() { return incident_wavefront_edges; }
        /** return the position of this vertex for drawing purposes.
         *
         * If the time to draw is later than the stop position, return
         * the stop position.
         *
         * If the time to draw is prior to the start position, no such
         * special handling is done and we return the location where
         * the vertex would have been such that it is at the start position
         * at the start time given its velocity.
         */
        internal void assert_valid()
        {
            //#if !SURF_NDEBUG

            Debug.Assert(is_initial || !is_beveling); // !initial => !beveling   <=>  !!initial v !beveling
            for (int i = 0; i < 2; ++i)
            {
                if (is_initial)
                {
                    Debug.Assert(prev_vertex_[i] == null || is_beveling);
                }
                else
                {
                    Debug.Assert(prev_vertex_[i] != null);
                }
                Debug.Assert(!has_stopped() ^ next_vertex_[i] != null);
            }
            // #endif

        }

        internal bool is_degenerate() { return is_degenerate_; }

        internal void link_tail_to_tail(WavefrontVertex other)
        {
            Debug.Assert(prev_vertex_[0] == null);
            Debug.Assert(other.prev_vertex_[1] == null);
            prev_vertex_[0] = other;
            other.prev_vertex_[1] = this;
            //DBG(DBG_KT) << " For " << this << " prev_vertex_[0] := " << other;
            //DBG(DBG_KT) << " For " << other << " prev_vertex_[1] := " << this;
        }

        internal void set_next_vertex(int side, WavefrontVertex next, bool head_to_tail = true)
        {
            Debug.Assert(side <= 1);
            Debug.Assert(next != null);
            Debug.Assert(has_stopped());
            //DBG(DBG_KT) << " for " << this << " next_vertex_[" << side << "] is " << next_vertex_[side];
            //DBG(DBG_KT) << " for " << this << " next_vertex_[" << side << "] := " << next;
            Debug.Assert(next_vertex_[side] == null);
            next_vertex_[side] = next;

            if (head_to_tail)
            {
                //DBG(DBG_KT) << " +for " << next << " prev_vertex_[" << side << "] is " << next.prev_vertex_[side];
                //DBG(DBG_KT) << " +for " << next << " prev_vertex_[" << side << "] := " << this;
                Debug.Assert(next.prev_vertex_[side] == null);
                next.prev_vertex_[side] = this;
            }
            else
            {
                /* head to head */
                //DBG(DBG_KT) << " +for " << next << " next_vertex_[" << 1 - side << "] is " << next.next_vertex_[1 - side];
                //DBG(DBG_KT) << " +for " << next << " next_vertex_[" << 1 - side << "] := " << this;
                Debug.Assert(next.next_vertex_[1 - side] == null);
                next.next_vertex_[1 - side] = this;
            };
        }

        protected static (LineIntersectionType, Vector2D) compute_intersection(Line2D a, Line2D b)
        {


            var i = new LineLineIntersector(a, b);
            var it = i.intersection_type();
            switch (it)
            {
                case LineLineIntersectionResult.LINE: return (LineIntersectionType.ALL, Vector2D.NaN);
                case LineLineIntersectionResult.POINT: return (LineIntersectionType.ONE, new Vector2D(i.point.X, i.point.Y));

            }
            return (LineIntersectionType.NONE, Vector2D.NaN);


        }
        /*
        public
            friend inline std.ostream& operator<<(std.ostream& os, const WavefrontVertex * const kv) {
              if (kv) {
                os << "kv";
        DEBUG_STMT(os << kv.id);
        os << (kv.angle == CONVEX ? "c" :
               kv.angle == REFLEX ? "r" :
               kv.angle == STRAIGHT ? "=" :
                                       "XXX-INVALID-ANGLE")
           << kv.infinite_speed
           << (kv.has_stopped_ ? "s" : "");
              } else
        {
            os << "kv*";
        }
        return os;
            }
        */
        // ==================== functions maintaining the DCEL =====================

        /** set the successor in the DCEL
         *
         * Also update their prev (or next) pointer depending on whether we have
         * head_to_tail set to true or not.
         */
        /** join two wavefront vertices, tail-to-tail.
         *
         * This is used after a split event.  Is called at a, where a's left (ccw)
         * side is towards the split edge, i.e. where prev_vertex[0] is null.
         *
         * This is also used to link initial vertices while beveling.  Going
         * clockwise about a vertex, this is called at each kinetc vertex with
         * the previous one as an argument.
         */
        //SkeletonDCELHalfedge skeleton_dcel_halfedge(int i)
        //{
        //    Debug.Assert(i <= 1);

        //    Debug.Assert(!is_degenerate());
        //    return skeleton_dcel_halfedge_[i];
        //}

        //internal void set_skeleton_dcel_halfedge(int i, SkeletonDCELHalfedge he)
        //{
        //    Debug.Assert(i <= 1);

        //    Debug.Assert(!is_degenerate());
        //    skeleton_dcel_halfedge_[i] = he;
        //}
    }

}