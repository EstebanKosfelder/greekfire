using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TriangleNet.Topology.DCEL;

namespace CGAL
{
    using static DebugLog;
    using static Mathex;



    public partial class KineticTriangle:Face
    {

     



        internal int component = 0;
        public bool IsDead { get; private set; } = false;
        public bool IsDying { get; private set; } = false;


     

        public int WavefrontsCount => Halfedges.Where(h => h.IsConstrain).Count();


        public double Area()
        {

            var p = Vertices.Select(e => e.Point).ToArray();
            return (p[1].X - p[0].X) * (p[2].Y - p[0].Y) - (p[1].Y - p[0].Y) * (p[2].X - p[0].X);
        }

       


        public static int cw(int i)
        { return TriangulationUtils.cw(i); }

        public static int ccw(int i)
        { return TriangulationUtils.ccw(i); }

        public readonly TriangleNet.Geometry.ITriangle OriginalTriangle;


        //friend std::ostream& operator<<(std::ostream& os, const KineticTriangle  const kt);
        //std::string get_name() const {
        //  return "kt" + std::to_string(id);
        //}

        private void assert_is_id(int q) => Debug.Assert(id == q);




        internal void set_dying()
        { IsDying = true; }


        /** Mark this triangle as dead.  May only be called once. */

        public bool IsCollapseEventValid => _eventValid;

        public enum VertexOnSupportingLineType
        { Once, Never, Always };

        //    friend std::ostream& operator<<(std::ostream& os, const KineticTriangle::VertexOnSupportingLineType a);

        internal void set_neighbors(params KineticTriangle?[] n)
        {
            //neighbors[0] = n[0];
            //neighbors[1] = n[1];
            //neighbors[2] = n[2];
        }

        internal void set_wavefronts(params WavefrontEdge[] w)
        {
            //wavefronts[0] = w[0];
            //wavefronts[1] = w[1];
            //wavefronts[2] = w[2];
            InvalidateEvent();
        }
     
        
      






        

        





        //[TODO]
        internal void assert_valid()
        {
//#if !SURF_NDEBUG
            //{{{
            Debug.Assert(!IsDead);
            //DBG_FUNC_BEGIN(//DBG_TRIANGLE_ASSERT_VALID);
            //DBG(//DBG_TRIANGLE_ASSERT_VALID) << this;

            Debug.Assert(Vertices.All(e => e != null)); // "Missing vertex %d", i

            //          Debug.Assert(Halfedges.Where(e=>e.IsConstrain).All(e => (e.Triangle == e.WavefrontEdge.incident_triangle())));// "Wavefront vs. neighbor existence mismatch at %d", i
            Debug.Assert(Halfedges.Where(e => e.IsConstrain).All(e => !e.WavefrontEdge.is_dead()));
            Debug.Assert(Halfedges.Where(e => e.Opposite.IsKinetic).All(e => (e.Opposite.IsKinetic) && e.Opposite.Opposite.Face == e.Face));


            //DBG_FUNC_END(//DBG_TRIANGLE_ASSERT_VALID);
//#endif
        }//}}}




        

        

        /** Compute when the vertex v will move over the supporting line of e (or crash into e).
         *
         * We only call this function when we have a triangle with exactly one constraint.
         *
         * The resulting time might also be now or be in the past.
         */

        public (double, VertexOnSupportingLineType) GetTimeVertexOnSupportingLine(Vertex v, WavefrontSupportingLine e)
        {
            Log("");
            Log($"{MethodBase.GetCurrentMethod()?.Name}");
            LogIndent();
            VertexOnSupportingLineType vertexOnSupportingLineType = VertexOnSupportingLineType.Once;
            double collapse_time = 0.0;


            /* Let n be some normal to e,
             * let P be some point on e (at time zero),
             * let s be the speed (well, velocity) vector of the vertex v.
             * let Q be the location of v at time zero, and
             * let w be the weight (speed) of e.
             */
            var n = (e.normal_direction);
            var P = (e.l.PointOnLine());
            var s = (v.velocity);
            var Q = (v.pos_zero);
            double w = (e.weight);
            /* Then PQ.n is the length of the projection of PQ onto n, times the length of n.
             * Likewise, s.n is the length of the projection of s onto n, times the length of n.
             * Per time unit, v and e will approach each other by (w - s.n/|n|).
             *
             * So v will hit (the supporting line of) e at time t := PQ.n/|n| / (w - s.n/|n|) ==
             * == PQ.n / (w |n| - s.n)
             */
            var PQ = new Vector2(P, Q);
            double scaled_distance = PQ.Dot(n);
            double scaled_edge_speed = w * n.Length();
            double scaled_vertex_speed = s.Dot(n);
            double scaled_speed_approach = scaled_edge_speed - scaled_vertex_speed;



            Log($" -- e: {e}");
            Log($" -- n: {n}");
            Log($" -- P: {P}");
            Log($" -- s: {s}");
            Log($" -- Q: {Q}");
            Log($" -- w: {w}");
            Log($" -- PQ:{PQ}");
            Log($" -- num (∝ distance      ): {scaled_distance}");
            Log($" -- den (∝ approach speed): {scaled_speed_approach}");

            if ( scaled_speed_approach.IsZero())
            {
                if (scaled_distance.IsZero())
                    vertexOnSupportingLineType = VertexOnSupportingLineType.Always;
                else
                    vertexOnSupportingLineType = VertexOnSupportingLineType.Never;
            }
            else
            {
                vertexOnSupportingLineType = VertexOnSupportingLineType.Once;

                collapse_time = scaled_distance / scaled_speed_approach;


                if (collapse_time.IsZero())
                    collapse_time = 0.0;
            }
            Log($"returning {collapse_time}  with VertexOnSupportingLineType {vertexOnSupportingLineType}");
            LogUnindent();

            return (collapse_time, vertexOnSupportingLineType);





        } // }}}

#if false
/** determine if split or flip event
 *
 * The triangle has one constraint, e, and opposite vertex v
 * move onto the supporting line of e at time collapse_time.
 *
 * Determine if this is a split or a flip event.
 */
CollapseSpec
KineticTriangle.
determine_split_or_flip_bounded_constrained_1(double collapse_time, int c_idx){
  CollapseSpec result(component);
  result = CollapseSpec(component, CollapseType.SPLIT_OR_FLIP_REFINE, collapse_time, c_idx);
  return result;
}
#endif



        /** Learn when the triangle will collapse from just the determinant, with no
         * respect to combinatorics or other geometry.
         *
         * The determinant polynomial passed should be (positively proportional to) the
         * triangle's (signed) area.
         *
         * It must be of at most degree 2 (which is the case in our use).
         *
         *
         * We return whether this triangle will ever collapse (now or in the future),
         * along with the collapse time in the collapse_time argument.  Collapses in the past
         * are disregarded.
         *
         * Additionally, we note some extra information that helps the calling function classify the
         * type of collapse.
         */

        // NT_USE_DOUBLE
        static double generic_collapse_last_time = 0;
        static int generic_collapse_count = 0;
        //endif

        public static bool GetGenericCollapseTime(double time_now, Polynomial1D det, out double collapse_time)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE_TIMING2);

            bool result;
            collapse_time = double.NaN;




            var sign = (ESign)det.sign();
            //DBG(//DBG_TRIANGLE_TIMING2) << "polynomial has degree " << det.degree() << " and sign " << sign;
            if (det.Degree == 0)
            {
                /*
                 * This should really only happen when collinear points on the convex hull move such that they stay collinear.
                 *
                 * No need to switch things around, then.
                 * If they change order, we should, eventually, catch that elsewhere because the vertices become incident.
                 */
                //LOG(WARNING) << "Have a polynomial of degree zero, Can we catch this sooner?  at time " << to_double(time_now);
                if (Mathex.sign(det[0]) == (int)ESign.Zero)
                {
                    result = true;
                    //LOG(WARNING) << " collapses now (and always) too.";
                    //# if NT_USE_DOUBLE
                    if (time_now == generic_collapse_last_time)
                    {
                        ++generic_collapse_count;
                        if (generic_collapse_count > 1000)
                        {
                            throw new GFLException("In double loop at line");
                        };
                    }
                    else
                    {
                        generic_collapse_count = 0;
                        generic_collapse_last_time = time_now;
                    };
                    ////#endif
                }
                else
                {
                    result = false;
                }
            }
            else if (det.Degree == 1)
            {
                Debug.Assert(sign != ESign.Zero);
                collapse_time = -det[0] / det[1];
                if (collapse_time == time_now)
                {
                    if (sign == ESign.Positive)
                    {
                        //DBG(//DBG_TRIANGLE_TIMING2) << "Triangle area is zero and increasing, not using this collapse time";
                        result = false;
                    }
                    else
                    {
                        //LOG(WARNING) << "Polynomial (of degree 1) has a zero right right now.  Can we catch this sooner?.  at time " << to_double(time_now);
                        //DBG(//DBG_TRIANGLE_TIMING2) << "Triangle area is zero and decreasing, using this collapse time";

                        if (time_now == generic_collapse_last_time)
                        {
                            ++generic_collapse_count;
                            if (generic_collapse_count > 1000)
                            {
                                throw new GFLException("In double loop at line");
                            };
                        }
                        else
                        {
                            generic_collapse_count = 0;
                            generic_collapse_last_time = time_now;
                        };

                        result = true;
                    }
                }
                else if (collapse_time > time_now)
                {
                    Debug.Assert(sign == ESign.Negative);
                    result = true;
                    //DBG(//DBG_TRIANGLE_TIMING2) << "Triangle area is polynomial of degree one, and zero is in the future.  Using this.";
                }
                else
                {
                    //DBG(//DBG_TRIANGLE_TIMING2) << "Triangle area is polynomial of degree one, and zero is in the past.  Not using this.";
                    Debug.Assert(sign == ESign.Positive);
                    result = false;
                };
            }
            else
            {
                Debug.Assert(det.Degree == 2);
                Debug.Assert(sign != ESign.Zero);
                //DBG(//DBG_TRIANGLE_TIMING2)
                //<< to_double(det[2]) << ".t^2 + "
                //<< to_double(det[1]) << ".t + "
                //<< to_double(det[0]);

                double x0, x1;
                //DBG(//DBG_TRIANGLE_TIMING2) << "solving quadratic.";
                bool has_real_roots, is_square;
                (has_real_roots, is_square) = Mathex.solve_quadratic(det, out x0, out x1);
                if (!has_real_roots)
                {
                    //DBG(//DBG_TRIANGLE_TIMING2) << "no real solutions.  sign is " << sign;
                    Debug.Assert(sign == ESign.Positive);
                    result = false;
                }
                else
                {
                    //DBG(//DBG_TRIANGLE_TIMING2) << "have real solutions (" << to_double(x0) << ", " << to_double(x1) << ").  Checking if we like them.";

                    /*
                    //DBG(//DBG_TRIANGLE_TIMING2) << " - x0:  " << to_double(x0);
                    //DBG(//DBG_TRIANGLE_TIMING2) << " - x1:  " << to_double(x1);
                    //DBG(//DBG_TRIANGLE_TIMING2) << " - now: " << to_double(time_now);
                    */

                    Debug.Assert(sign == ESign.Negative || sign == ESign.Positive);
                    if (sign == ESign.Negative)
                    {
                        //DBG(//DBG_TRIANGLE_TIMING2) << " we like x1: The sign of the determinant is negative.  So the second root must be a valid event.";
                        collapse_time = x1;
                        Debug.Assert(x1 >= time_now);
                        result = true;
                    }
                    else if (x0 >= time_now)
                    {
                        //DBG(//DBG_TRIANGLE_TIMING2) << " we like x0: The sign of the determinant is positive and the first root is not in the past.";
                        collapse_time = x0;
                        result = true;
                    }
                    else
                    {
                        //DBG(//DBG_TRIANGLE_TIMING2) << " we like neither: The sign of the determinant is positive, but the first root is in the past.";
                        result = false;
                    }
                }
            }
            if (result)
            {
                //DBG(//DBG_TRIANGLE) << "returning " << result << " with " << to_double(collapse_time);
            }
            else
            {
                //DBG(//DBG_TRIANGLE) << "returning " << result;
            }
            //DBG_FUNC_END(//DBG_TRIANGLE_TIMING2);
            return result;
        } // }}}

        private Polynomial1D ComputeDeterminantFromVertices(params Vertex[] v)
        {
            Debug.Assert(v[0] != null);
            Debug.Assert(v[1] != null);
            Debug.Assert(v[2] != null);
            return Mathex.compute_determinant(
                    v[0].px(), v[0].py(),
                    v[1].px(), v[1].py(),
                    v[2].px(), v[2].py());
        }

        [Conditional("DEBUG")]
        public void AssertCirculation()
        {
            if (Halfedges.Where( h=> h.Vertex != h.Next.Opposite.Vertex && h.Prev.Vertex == h.Opposite.Vertex).Any())
            {
                Debug.Fail($"{this} inconsistencia error en los link de Halfedges {string.Join("->",Halfedges.Select(h=>h.ToString()))}");
            }
        }
        public double[] SquaredLengths(double time)
        {
            AssertCirculation();    
            return Halfedges.Select(h=>h.SquaredLength(time)).ToArray();
        }

        /** only called for unconstrained triangles, only from compute_flip_event
         *
         * Can return NEVER, TRIANGLE_COLLAPSE, SPOKE_COLLAPSE, or VERTEX_MOVES_OVER_SPOKE.
         */

        private EventData GetGenericCollapse(double time_now, Polynomial1D determinant)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
          //DBG(//DBG_TRIANGLE) << this;

            Debug.Assert(Halfedges.All(h => !h.IsConstrain));

            EventData result  ;
            double collapse_time;
            //
            // XXX use is_squared in the counting zero length edges down below
            bool triangle_will_collapse = GetGenericCollapseTime(time_now, determinant, out collapse_time);
            if (triangle_will_collapse)
            {
                //DBG(//DBG_TRIANGLE_TIMING) << this << " collapse time is " << to_double(collapse_time);
              //  Point2[] p = this.Vertices.Select(v => v.PointAt(collapse_time)).ToArray();
                double[] squared_lengths = this.SquaredLengths(collapse_time);

                //DBG(//DBG_TRIANGLE_TIMING2) << this << " checking for edge lengths zero at time " << to_double(collapse_time);
                bool[] is_zero = new bool[3];
                int cnt_zero = 0;
                if ((is_zero[0] = (squared_lengths[0].IsZero()))) ++cnt_zero;
                if ((is_zero[1] = (squared_lengths[1].IsZero()))) ++cnt_zero;
                if (cnt_zero == 2)
                {
                    Debug.Assert(squared_lengths[2].IsZero());
                    cnt_zero = 3;
                    is_zero[2] = true;
                }
                else if ((is_zero[2] = (squared_lengths[2].IsZero()))) ++cnt_zero;

                for (int i = 0; i < 3; ++i)
                {
                    //DBG(//DBG_TRIANGLE_TIMING2) << this << (is_zero[i] ? "  is zero" : "  is not zero.");
                    //DBG(//DBG_TRIANGLE_TIMING2) << this << "  length: " << to_double(squared_lengths[i]);
                }
          
                switch (cnt_zero)
                {
                    case 3:

                      
                        result = new TriangleCollapseEventData(this, collapse_time);
                        break;

                    case 1:
                        {
                            int zero_idx =  is_zero[0] ? 0 :
                                           is_zero[1] ? 1 :
                                                   2;
                            var zero_edge = this.Halfedges.ElementAt(zero_idx);
                            Debug.Assert(squared_lengths[cw(zero_idx)] == squared_lengths[ccw(zero_idx)]);
                            result = new SpokeCollapseEventData(zero_edge,collapse_time );
                        }
                        break;

                    default:
                        Debug.Assert(cnt_zero == 0);
                        {
                            //DBG(//DBG_TRIANGLE_TIMING2) << this << " sorting lengths.";
                            int i0, i1, i2;
                            (i0, i1, i2) = Mathex.indirect_sort_3(squared_lengths);

                            //DBG(//DBG_TRIANGLE_TIMING2) << this << " edge at collapse time is " << i0 << ".  length: " << to_double(squared_lengths[i0]);
                            //DBG(//DBG_TRIANGLE_TIMING2) << this << " edge at collapse time is " << i1 << ".  length: " << to_double(squared_lengths[i1]);
                            //DBG(//DBG_TRIANGLE_TIMING2) << this << " edge at collapse time is " << i2 << ".  length: " << to_double(squared_lengths[i2]);

                            Debug.Assert(squared_lengths[i1] < squared_lengths[i2]);
                            if (determinant.Degree == 0)
                            {
                                //DBG(//DBG_TRIANGLE_TIMING2) << this << " As determinant has degree zero, use current time as collapse time";
                                collapse_time = time_now;
                            };
                            var edge = this.Halfedges.ElementAt(i2);

                            if (edge.Vertex.IsConvex)
                                result = new VertexMovesOverSpokeEventData(this.Halfedges.ElementAt(i2), squared_lengths[i2], collapse_time);
                            else
                                result = new NeverOrInvalidCollapseEventData(edge, determinant, collapse_time,"${edge.Vertex} is Convex");
                        }
                        break;
                }
            }
            else
            {
                result = new NeverCollapseEventData(this, "never triangle_will_collapse");
            }

            //DBG(//DBG_TRIANGLE) << this << " returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return  result;
        } // }}}

        /** only called for unconstrained triangles */








        /// <summary>
        /// (wavefronts[0] != null).ToInt() + (wavefronts[1] != null).ToInt() + (wavefronts[2] != null).ToInt()
        /// </summary>


        /* unbounded triangles witness two things:
         *   - collapse of their bounded spoke
         *   - one of their vertices leaving the CH boundary.
         *     (the vertex CCW from the infinite vertex, i.e.,
         *      the first vertex when traversing the convex hull in CW order.)
         *      let that vertex be v.
         */
        //private CollapseSpec compute_collapse_unbounded(double time_now)
        //{ // {{{
        //  //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
        //  //DBG(//DBG_TRIANGLE) << this;

        //    CollapseSpec result;
        //    CollapseSpec edge_collapse;

        //    Debug.Assert(unbounded());
        //    Debug.Assert(has_vertex_infinite_speed() == InfiniteSpeedType.NONE);
        //    int idx = infinite_vertex_idx();
        //    if (is_constrained(idx))
        //    {
        //        edge_collapse = wavefronts[idx].get_collapse(component, time_now, idx);
        //        //DBG(//DBG_TRIANGLE_TIMING) << "   Constraint will collapse at " << edge_collapse;
        //    }
        //    else
        //    {
        //        edge_collapse = new CollapseSpec(component, CollapseType.NEVER);
        //        //DBG(//DBG_TRIANGLE_TIMING) << "   not constraint.";
        //    }

        //    KineticTriangle n = neighbor(cw(idx));
        //    Debug.Assert(n.unbounded());
        //    int nidx = n.infinite_vertex_idx();
        //    Debug.Assert(n.neighbor(ccw(nidx)) == this);
        //    Debug.Assert(vertex(ccw(idx)) == n.vertex(cw(nidx)));

        //    // v is the (finite) vertex common to this and n.
        //    if (is_constrained(idx) && n.is_constrained(nidx))
        //    {
        //        //DBG(//DBG_TRIANGLE_TIMING) << "both this and n are constraint -- v cannot leave the CH boundary.  Constraint collapse is the only thing that may happen.";
        //        result = edge_collapse;
        //    }
        //    else if (is_constrained(idx) || n.is_constrained(nidx))
        //    {
        //        //DBG(//DBG_TRIANGLE_TIMING) << "Exactly one of this and n is constraint.";
        //        /* one of this and n is constraint.
        //         */
        //        CollapseSpec vertex_leaves_ch;
        //        WavefrontEdge wfe;
        //        WavefrontVertex v;
        //        if (is_constrained(idx))
        //        {
        //            wfe = wavefront(idx);
        //            v = n.vertex(ccw(nidx));
        //        }
        //        else
        //        {
        //            wfe = n.wavefront(nidx);
        //            v = vertex(cw(idx));
        //        };
        //        WavefrontSupportingLine e = wfe.l();
        //        Debug.Assert(v != null);
        //        Debug.Assert(e != null);

        //        var edge_is_faster = edge_is_faster_than_vertex(v, e);
        //        if (edge_is_faster == ZERO)
        //        {
        //            //DBG(//DBG_TRIANGLE_TIMING) << "   Edge " << *wfe << " is just as fast as the vertex " << v;

        //            double collapse_time;
        //            VertexOnSupportingLineType vertex_on_line_type;
        //            (collapse_time, vertex_on_line_type) = get_time_vertex_on_supporting_line(v, e);
        //            if (vertex_on_line_type == VertexOnSupportingLineType.NEVER)
        //            {
        //                //DBG(//DBG_TRIANGLE_TIMING) << "     but v is not on the supporting line of e";
        //            }
        //            else
        //            {
        //                Debug.Assert(vertex_on_line_type == VertexOnSupportingLineType.ALWAYS);
        //                //DBG(//DBG_TRIANGLE_TIMING) << "     and they are collinear.  And while that is the case, v cannot leave the CH.";
        //                /* v moving into us is witnessed by the neighboring triangle */
        //            }
        //            vertex_leaves_ch = new CollapseSpec(component, CollapseType.NEVER);
        //        }
        //        else if (edge_is_faster == POSITIVE)
        //        {
        //            //DBG(//DBG_TRIANGLE_TIMING) << "   Edge " << *wfe << " is faster than vertex " << v << " - CCW of t will never leave the CH.";
        //            vertex_leaves_ch = new CollapseSpec(component, CollapseType.NEVER);
        //        }
        //        else
        //        {
        //            //DBG(//DBG_TRIANGLE_TIMING) << "   Edge " << *wfe << " is slower than vertex " << v << " - CCW of t will leave the CH.";

        //            double collapse_time;
        //            VertexOnSupportingLineType vertex_on_line_type;
        //            (collapse_time, vertex_on_line_type) = get_time_vertex_on_supporting_line(v, e);
        //            Debug.Assert(vertex_on_line_type == VertexOnSupportingLineType.ONCE);

        //            //DBG(//DBG_TRIANGLE_TIMING) << "   * vertex will move onto supporting line at " << to_double(collapse_time);
        //            //DBG(//DBG_TRIANGLE_TIMING) << "   * now                                    t " << to_double(time_now);
        //            assert_expensive_ge(collapse_time, time_now);
        //            vertex_leaves_ch = new CollapseSpec(component, CollapseType.CCW_VERTEX_LEAVES_CH, collapse_time, idx);
        //        }

        //        //DBG(//DBG_TRIANGLE_TIMING) << "  edge_collapse   : " << edge_collapse;
        //        //DBG(//DBG_TRIANGLE_TIMING) << "  vertex_leaves_ch: " << vertex_leaves_ch;
        //        result = vertex_leaves_ch.CompareTo(edge_collapse) < 1 ? vertex_leaves_ch : edge_collapse;
        //    }
        //    else
        //    {
        //        //DBG(//DBG_TRIANGLE_TIMING) << "Neither this nor n are constraint -- a vertex leaving the CH is the only thing that can happen.";
        //        Debug.Assert(edge_collapse.type() == CollapseType.NEVER);
        //        /* we need to do set up a determinant and solve for zeroes.
        //         */
        //        WavefrontVertex u = vertex(cw(idx));
        //        WavefrontVertex v = vertex(ccw(idx));
        //        WavefrontVertex V = n.vertex(cw(nidx));
        //        WavefrontVertex w = n.vertex(ccw(nidx));
        //        Debug.Assert(v == V);
        //        Debug.Assert(orientation(u.p_at(time_now),
        //                           v.p_at(time_now),
        //                           w.p_at(time_now)) != RIGHT_TURN);
        //        if (u.velocity == v.velocity && v.velocity == w.velocity)
        //        {
        //            //DBG(//DBG_TRIANGLE_TIMING) << "   * all three vertices have the same velocity ";
        //            if (u.pos_zero == v.pos_zero || v.pos_zero == w.pos_zero)
        //            {
        //                //DBG(//DBG_TRIANGLE_TIMING) << "   * at least two vertices are incident. ";
        //                throw new NotImplementedException("Incident vertices case");
        //            }
        //            else
        //            {
        //                //DBG(//DBG_TRIANGLE_TIMING) << "   * Three parallel vertices on the ch";
        //                result = new CollapseSpec(component, CollapseType.NEVER);
        //            }
        //        }
        //        else
        //        {
        //            Polynomial_1 determinant = compute_determinant_from_vertices(u, v, w);
        //            double time;
        //            if (get_generic_collapse_time(time_now, determinant, out time))
        //            {
        //                //DBG(//DBG_TRIANGLE_TIMING) << "   * CCW will leave CH in unconstrained situation at time " << to_double(time);
        //                if (time == time_now)
        //                {
        //                    //LOG(WARNING) << "Rarely exercised code path: unconstraint case with triangle leaving right now.";
        //                }
        //                result = new CollapseSpec(component, CollapseType.CCW_VERTEX_LEAVES_CH, time, idx);
        //            }
        //            else
        //            {
        //                //DBG(//DBG_TRIANGLE_TIMING) << "   * CCW will not leave CH in unconstrained situation";
        //                result = new CollapseSpec(component, CollapseType.NEVER);
        //            }
        //        }
        //    }

        //    //DBG(//DBG_TRIANGLE) << this << " returning " << result;
        //    //DBG_FUNC_END(//DBG_TRIANGLE);
        //    return result;
        //} // }}}

        public void set_dead()
        { // {{{
            Debug.Assert(!IsDead);
            //TODO Assert 
            //foreach (var h in Halfedges )
            //{
            //    if (!h.IsConstrain )
            //    {
            //        Debug.Assert(!h.Opposite.Halfedges.Any(ho=>ho.Face==this));
            //    }
            //    if (wavefronts[i] != null)
            //    {
            //        Debug.Assert(wavefronts[i].incident_triangle() != this || wavefronts[i].is_dead());
            //    }
            //}
            IsDead = true;
        } // }}}

        //[Obsolete]
        //internal void set_vertex(int i, Vertex v)
        //{ // {{{
        //    Debug.Assert(i < 3);
        //    Debug.Assert(v != null);
        //    vertices[i] = v;
        //    if (is_constrained(cw(i)))
        //    {
        //        Debug.Assert(wavefronts[cw(i)] != null);
        //        wavefronts[cw(i)].set_wavefrontedge_vertex(0, v);
        //    }
        //    if (is_constrained(ccw(i)))
        //    {
        //        Debug.Assert(wavefronts[ccw(i)] != null);
        //        wavefronts[ccw(i)].set_wavefrontedge_vertex(1, v);
        //    }
        //    InvalidateCollapseSpec();
        //} // }}}

        //private void set_wavefront(int idx, WavefrontEdge e)
        //{ // {{{
        //    //CGAL_precondition(idx < 3);
        //    Debug.Assert(e != null);

        //    Debug.Assert(wavefronts[idx] == null);
        //    Debug.Assert(neighbors[idx] != null);
        //    Debug.Assert(neighbors[idx].is_dying());
        //    Debug.Assert(e.incident_triangle() == neighbors[idx]);

        //    wavefronts[idx] = e;
        //    neighbors[idx] = null;
        //    e.set_incident_triangle(this);
        //} // }}}

        //internal void move_constraint_from(int idx, KineticTriangle src, int src_idx)
        //{ /// {{{
        //   // CGAL_precondition(idx < 3 && src_idx < 3);

        //    Debug.Assert(src.is_dying());
        //    Debug.Assert(!is_constrained(idx));
        //    Debug.Assert(src.wavefronts[src_idx].incident_triangle() == src);
        //    wavefronts[idx] = src.wavefronts[src_idx];

        //    // we already need to share one vertex with the origin, which will go away.
        //    Debug.Assert(wavefronts[idx].vertex(0) == vertices[ccw(idx)] ||
        //           wavefronts[idx].vertex(1) == vertices[cw(idx)]);
        //    Debug.Assert(has_neighbor(src));
        //    Debug.Assert(idx == index(src));

        //    wavefronts[idx].set_wavefrontedge_vertex(0, vertices[ccw(idx)]);
        //    wavefronts[idx].set_wavefrontedge_vertex(1, vertices[cw(idx)]);
        //    wavefronts[idx].set_incident_triangle(this);

        //    src.wavefronts[src_idx] = null;
        //    invalidate_collapse_spec();
        //} // }}}


        ///** Flip edge <idx> of this triangle.
        // *
        // * Let t be v, v1, v2.  (where v is the vertex at idx),
        // * and let our neighbor opposite v be o, v2, v1.
        // *
        // * Then after the flipping, this triangle will be v, o, v2,
        // * and the neighbor will be o, v, v1.
        // */

        //private void do_raw_flip_inner(int egde_idx)
        //{ // {{{
        //  //DBG_FUNC_BEGIN(//DBG_TRIANGLE | //DBG_TRIANGLE_FLIP);
        //  //DBG(//DBG_TRIANGLE | //DBG_TRIANGLE_FLIP) << this;
        //  //DBG(//DBG_TRIANGLE_FLIP) << this << " * flipping at idx " << idx;
        //    Debug.Assert(!is_constrained(egde_idx));
        //    KineticTriangle n = neighbors[egde_idx];
        //    int nidx = n.index(this);
        //    //DBG(//DBG_TRIANGLE_FLIP) << "   neighbor " << n << " with nidx " << nidx;

        //    WavefrontVertex v = vertices[egde_idx];
        //    WavefrontVertex v1 = vertices[ccw(egde_idx)];
        //    WavefrontVertex v2 = vertices[cw(egde_idx)];
        //    WavefrontVertex o = n.vertices[nidx];
        //    Debug.Assert(v1 == n.vertex(cw(nidx)));
        //    Debug.Assert(v2 == n.vertex(ccw(nidx)));

        //    // set new triangles
        //    vertices[ccw(egde_idx)] = o;
        //    n.vertices[ccw(nidx)] = v;

        //    //DBG(//DBG_TRIANGLE_FLIP) << "   * t  " << this;
        //    //DBG(//DBG_TRIANGLE_FLIP) << "     n  " << n;

        //    //DBG(//DBG_TRIANGLE_FLIP) << "     na " << n.neighbors[cw(idx)];
        //    //DBG(//DBG_TRIANGLE_FLIP) << "     nb " << neighbors[cw(idx)];

        //    // fix neighborhood relations and fix/move constraints
        //    // - neighbors of t and n that see their neighbor
        //    //   change from n to t or t to n.
        //    neighbors[egde_idx] = n.neighbors[cw(nidx)];
        //    wavefronts[egde_idx] = n.wavefronts[cw(nidx)];
        //    n.neighbors[nidx] = neighbors[cw(egde_idx)];
        //    n.wavefronts[nidx] = wavefronts[cw(egde_idx)];

        //    // - pair up t and n
        //    n.neighbors[cw(nidx)] = this;
        //    n.wavefronts[cw(nidx)] = null;
        //    neighbors[cw(egde_idx)] = n;
        //    wavefronts[cw(egde_idx)] = null;

        //    //DBG(//DBG_TRIANGLE_FLIP) << "   * t  " << this;
        //    //DBG(//DBG_TRIANGLE_FLIP) << "     n  " << n;

        //    Debug.Assert((neighbors[egde_idx] != null) == (wavefronts[egde_idx] == null));
        //    Debug.Assert((n.neighbors[nidx] != null) == (n.wavefronts[nidx] == null));

        //    if (is_constrained(egde_idx))
        //    {
        //        wavefronts[egde_idx].set_incident_triangle(this);
        //    }
        //    else
        //    {
        //        var i = neighbors[egde_idx].index(n);
        //        neighbors[egde_idx].neighbors[i] = this;
        //    }
        //    if (n.wavefronts[nidx] != null)
        //    {
        //        n.wavefronts[nidx].set_incident_triangle(n);
        //    }
        //    else
        //    {
        //        var i = n.neighbors[nidx].index(this);
        //        n.neighbors[nidx].neighbors[i] = n;
        //    }

        //    //DBG_FUNC_END(//DBG_TRIANGLE | //DBG_TRIANGLE_FLIP);
        //} // }}}

        ///** Flip edge <idx> of this triangle.
        // *
        // * Run do_raw_flip_inner() and asserts validity after
        // * and invalidates collapse specs.
        // */

        //internal void do_raw_flip(int edge_idx)
        //{ // {{{
        //  //DBG_FUNC_BEGIN(//DBG_TRIANGLE | //DBG_TRIANGLE_FLIP);
        //    KineticTriangle n = neighbors[edge_idx];

        //    do_raw_flip_inner(edge_idx);

        //    assert_valid();
        //    n.assert_valid();

        //    invalidate_collapse_spec();
        //    n.invalidate_collapse_spec();

        //    //DBG_FUNC_END(//DBG_TRIANGLE | //DBG_TRIANGLE_FLIP);
        //} // }}}





        public Rect2D Bounds()
        {
            Rect2D rect = new Rect2D(Vertices.Select(v => v.Point));

            return rect;
        }


    }
}
