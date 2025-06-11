using System.Text.RegularExpressions;
using System;
using System.Diagnostics;
using GFL.Kernel;

namespace CGAL
{
    public enum LineIntersectionType
    {
        ONE,   /* the two lines intersect in one point */
        ALL,   /* the two lines are parallel and coincide */
        NONE,  /* the two liens are parallel and distinct */
    };

   public enum VertexType
    {
        Contour,
        Wavefront
    }
    public partial class Vertex
    {
        public static int MaxDigit = 1;
        public Point2 intersection = Point2.NAN;
        public bool is_beveling;
        public bool is_infinite;
        public bool is_initial;
        public Point2 pos_zero;
        public double time_start;
        //  public InfiniteSpeedType infinite_speed; /** This wavefront vertex is
        //either between parallel, opposing wavefront elements that have crashed
        //into each other and become collinear, or it is between neighboring
        //wavefront edges that have become collinear yet have different weights. */
        public Vector2 velocity;

        internal EAngle angle;
        internal WavefrontEdge?[] incident_wavefront_edges;
        private bool has_stopped_ = false;
        private bool is_degenerate_ = false;
        private Vertex?[] next_vertex_ = new Vertex[2];
        private Point2 point_2;
        private Point2 pos_stop_;
        private Vertex?[] prev_vertex_ = new Vertex[2];
        private Polynomial1D px_, py_;
        private double time_stop_;
        [Obsolete("use Id")]
        public int ID => Id;
       

        public bool IsConvexOrStraight => angle != EAngle.Reflex;
        public bool IsNotConvex => this.IsConvex;
        public bool IsNotReflex => this.IsReflex;
        public bool IsReflexOrStraight => angle != EAngle.Convex;
        public Point2 pos_start { get => Point; set => this.Point = value; }
        public VertexType Type { get; internal set; }

       


        public override string ToString()
        {
            if( Type == VertexType.Wavefront)
            {
                return $"kv:{IDPrintable}{angle.ToString()[0]}";
            }
            return $"cv:{IDPrintable}{sskAnge}";
        }

        public Vector2 compute_velocity(
                  Point2 pos_zero,
                  WavefrontSupportingLine a,
                  WavefrontSupportingLine b,
                  EAngle angle)
        {
            Vector2 result;

            if (angle != EAngle.Straight)
            {
                Line2 la = a.line_at_one();
                Line2 lb = b.line_at_one();

                var lit = Mathex.intersection(la, lb);
                if (lit.Result != Intersection.Intersection_results.POINT)
                {
                    // CANNOTHAPPEN_MSG << "No point intersection between WavefrontEmittingEdges at offset 1.  Bad.";
                    Debug.Assert(false);
                    throw new Exception("No point intersection between WavefrontEmittingEdges at offset 1.  Bad.");
                }
                intersection = lit.Points[0];
                result = new Vector2(pos_zero, intersection);
            }
            else
            {
                //DBG(DBG_KT) << "a:" << CGAL_vector(a.normal);
                //DBG(DBG_KT) << "b:" << CGAL_vector(b.normal);

                if (Mathex.orientation(a.l.to_vector(), b.l.to_vector().perpendicular(OrientationEnum.CLOCKWISE)) == OrientationEnum.RIGHT_TURN)
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

        public Point2 DebugPointAt(double time)
        {
            if (HasStopped) return pos_stop_;
            return PointAt(time);
        }

        // protected static partial InfiniteSpeedType get_infinite_speed_type(WavefrontEdge a, WavefrontEdge b, ESign angle);
        // public bool is_straight() const { return angle == STRAIGHT; }

        [Obsolete]
        public bool has_stopped() { return has_stopped_; }

        public bool HasStopped => has_stopped_;

        public WavefrontEdge incident_wavefront_edge(int i)
        {
            Debug.Assert(i <= 1);
            return incident_wavefront_edges[i];
        }

        public Vertex next_vertex(int side)
        {
            Debug.Assert(side <= 1);
            return next_vertex_[side];
        }

        public Point2 PointAt(double t)
        {
            Debug.Assert(!has_stopped_ || t <= time_stop_);
            Debug.Assert(!is_infinite);
            return (pos_zero + (velocity * t));
        }

        public Point2 p_at_draw(double t)
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
                                   : (pos_zero + velocity * t);
        }

        public Point2 pos_stop() { return pos_stop_; }

        public Vertex prev_vertex(int side)
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

        //}
        public void set_incident_wavefront_edge(int i, WavefrontEdge e)
        {
            Debug.Assert(i <= 1);
            Debug.Assert(incident_wavefront_edges[i] != null);
            Debug.Assert(e != null);
            Debug.Assert(incident_wavefront_edges[i].SupportLine == e.SupportLine);
            incident_wavefront_edges[i] = e;
        }

        public void stop(double t)
        {
            Debug.Assert(!has_stopped_);

            time_stop_ = t;
            pos_stop_ = PointAt(t);
            has_stopped_ = true;

            if (pos_stop_ == pos_start)
            {
                is_degenerate_ = true;
                Debug.Assert(time_stop_ == time_start);
            };
        }

        public void stop(double t, Point2 p)
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
           
            //#endif
        }


        // ==================== functions maintaining the DCEL =====================

        /** set the successor in the DCEL
         *
         * Also update their prev (or next) pointer depending on whether we have
         * head_to_tail set to true or not.
         */
        internal bool is_degenerate() { return is_degenerate_; }

        internal void link_tail_to_tail(Vertex other)
        {
            Debug.Assert(prev_vertex_[0] == null);
            Debug.Assert(other.prev_vertex_[1] == null);
            prev_vertex_[0] = other;
            other.prev_vertex_[1] = this;
            //DBG(DBG_KT) << " For " << this << " prev_vertex_[0] := " << other;
            //DBG(DBG_KT) << " For " << other << " prev_vertex_[1] := " << this;
        }

        internal void set_next_vertex(int side, Vertex next, bool head_to_tail = true)
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

        
    }

}