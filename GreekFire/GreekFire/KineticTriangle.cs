using GFL.Kernel;
using System.Diagnostics;

namespace GFL
{
    public class KineticTriangle : IFace
    {


        public KineticTriangle(int id)
        {
            ID = id;
        }


        public int ID { get; private set; }
        private int id => ID;
        private int component = 0;
        public IEnumerable<Vector2D>  Normals => Halfedge.Circulation(3).Select(h => h.GetNormal());
        public IEnumerable<IVertex> Vertices => Halfedge.Circulation(3).Select(h => h.Vertex);
        public IEnumerable<KineticHalfedge> Halfedges => this.Halfedge.Circulation(3).Cast<KineticHalfedge>();

        public (Vector2D c, double r) InnerCircle()
        {
            return MathEx.InnerCircle(this.Vertices.Select(v => v.Point).ToArray());

        }

        public IHalfedge Halfedge { get; set; }

        public override string ToString()
        {
            return $"T{ID}-{string.Join(" -> ", Halfedge.Circulation().Select(h => h))}";
        }

        public Rect2D Bounds()
        {
            Rect2D rect = new Rect2D(vertices.Select(v => v.Point));

            return rect;
        }



        //  friend class KineticTriangulation;


      

        private bool is_dead_ = false;
        private bool is_dying_ = false;
        public HalfEdge3 Edges = null;
        public WavefrontVertex3 vertices = new WavefrontVertex3 ();
        public WavefrontEdge3 wavefronts = new WavefrontEdge3();
        public KineticTriangle3 neighbors = new KineticTriangle3();






        private CollapseSpec collapse_spec;
        private bool collapse_spec_valid = false;
#if !SURF_NDEBUG
        private WavefrontVertex3 collapse_spec_computed_with_vertices = new WavefrontVertex3();

#endif





        public static int cw(int i)
        { return TriangulationUtils.cw(i); }

        public static int ccw(int i)
        { return TriangulationUtils.ccw(i); }

        public readonly TriangleNet.Geometry.ITriangle OriginalTriangle;



     

      
        
        /** Check if this side has a constraint.
         *
         * This usually implies it has no neighbor, except during construction and while
         * manipulating the triangulation.  Outside of methods, it should always hold
         * that we have exactly either neighbor[i] or constraint[i].
         */


        internal WavefrontVertex vertex(int i) { Debug.Assert(i < 3); return vertices[i]; }



        //friend std::ostream& operator<<(std::ostream& os, const KineticTriangle  const kt);
        //std::string get_name() const {
        //  return "kt" + std::to_string(id);
        //}

        private void assert_is_id(int q)=>Debug.Assert(id == q); 

        public CollapseSpec get_collapse(double time_now)
        {
            Debug.Assert(!is_dead_);
            Debug.Assert(!is_dying_);
            if (!collapse_spec_valid)
            {
                collapse_spec = compute_collapse(time_now);
                collapse_spec_valid = true;
#if !SURF_NDEBUG
                set_to_cur_wf_vertices(collapse_spec_computed_with_vertices);
#endif
            };
            return get_cached_collapse();
        }

        internal CollapseSpec refine_collapse_spec(CollapseSpec c)
        {
            throw new NotImplementedException();
            //            Debug.Assert(collapse_spec_valid);
            //#if !SURF_NDEBUG
            //            assert_cur_wf_vertices(collapse_spec_computed_with_vertices);
            //#endif

            //            Debug.Assert(collapse_spec.allows_refinement_to(c));

            //            collapse_spec = std::forward<CollapseSpec>(c);
            //            return collapse_spec;
        }

        private CollapseSpec get_cached_collapse()
        {
            Debug.Assert(!is_dying_);
            Debug.Assert(collapse_spec_valid);
#if !SURF_NDEBUG
            assert_cur_wf_vertices(collapse_spec_computed_with_vertices);
#endif
            return collapse_spec;
        }

        internal void invalidate_collapse_spec()
        {
            Debug.Assert(!is_dead_);
            collapse_spec_valid = false;
#if !SURF_NDEBUG
            invalidate_cur_wf_vertices(collapse_spec_computed_with_vertices);
#endif
            /*
            _is_squeezed = false;
            determinant_valid = false;
            invalidate_cur_wf_vertices(determinantComputedWithWavefrontVertices);
            */
        }


        internal void set_dying() 
        { is_dying_ = true; }

    


        public bool is_dead() => is_dead_;

        /* called by EventQ */

        public bool is_dying() => is_dying_;

        /** Mark this triangle as dead.  May only be called once. */

     

        public bool is_collapse_spec_valid() => collapse_spec_valid;

       

        public enum VertexOnSupportingLineType     { ONCE, NEVER, ALWAYS };

        //    friend std::ostream& operator<<(std::ostream& os, const KineticTriangle::VertexOnSupportingLineType a);

       

        internal  void set_neighbors(params KineticTriangle?[] n)
        {

            neighbors[0] = n[0];
            neighbors[1] = n[1];
            neighbors[2] = n[2];

        }

        internal void set_wavefronts( params WavefrontEdge[] w)
        {
            wavefronts[0] = w[0];
            wavefronts[1] = w[1];
            wavefronts[2] = w[2];
            invalidate_collapse_spec();
        }

        internal  bool has_neighbor(KineticTriangle needle) => neighbors[0] == needle || neighbors[1] == needle || neighbors[2] == needle;

        internal bool has_vertex(WavefrontVertex needle) => vertices[0] == needle || vertices[1] == needle || vertices[2] == needle;

        internal bool has_wavefront(WavefrontEdge needle) => wavefronts[0] == needle || wavefronts[1] == needle || wavefronts[2] == needle;

        internal int index(KineticTriangle needle)
        {
            // SRF_precondition(has_neighbor(needle));

            int idx =
              (neighbors[0] == needle) ? 0 :
              (neighbors[1] == needle) ? 1 :
              2;
            return idx;
        }

        internal int index(WavefrontVertex needle)
        {
            // SRF_precondition(has_vertex(needle));

            int idx =
              (vertices[0] == needle) ? 0 :
              (vertices[1] == needle) ? 1 :
              2;
            return idx;
        }

        internal int index(WavefrontEdge needle)
        {
            //SRF_precondition(has_wavefront(needle));

            int idx =
              (wavefronts[0] == needle) ? 0 :
              (wavefronts[1] == needle) ? 1 :
              2;
            return idx;
        }

        internal KineticTriangle neighbor(int i) =>neighbors[i];
        

        internal bool is_constrained(int i) =>wavefronts[i] != null;
        

        internal WavefrontEdge wavefront(int i)=>wavefronts[i];
        

#if !SURF_NDEBUG

        private void set_to_cur_wf_vertices(WavefrontVertex3 arr)
        {
            for (int i = 0; i < 3; ++i)
            {
                arr[i] = vertices[i];
            }
        }

        private void invalidate_cur_wf_vertices(WavefrontVertex3 arr)
        {
            for (int i = 0; i < 3; ++i)
            {
                arr[i] = null;
            }
        }

        private void assert_cur_wf_vertices(WavefrontVertex3 arr)
        {
            for (int i = 0; i < 3; ++i)
            {
                Debug.Assert(arr[i] == vertices[i]);
            }
        }

#endif

        private void set_neighbor(int idx, KineticTriangle n)
        {
            // CGAL_precondition(idx < 3);
            neighbors[idx] = n;
            Debug.Assert(n == null || n.component == component);
        }

        /** return the index of one vertex with infinite speed.
         */

        //public int infinite_speed_opposing_vertex_idx()
        //{ // {{{
        //  //Debug.Assert(unbounded());
        //    Debug.Assert((vertex(0).infinite_speed == InfiniteSpeedType.OPPOSING).ToInt() +
        //           (vertex(1).infinite_speed == InfiniteSpeedType.OPPOSING).ToInt() +
        //           (vertex(2).infinite_speed == InfiniteSpeedType.OPPOSING).ToInt() >= 1);
        //    int idx =
        //      (vertex(0).infinite_speed == InfiniteSpeedType.OPPOSING) ? 0 :
        //      (vertex(1).infinite_speed == InfiniteSpeedType.OPPOSING) ? 1 :
        //      2;
        //    return idx;
        //} // }}}

        //public partial int infinite_vertex_idx()
        //{ // {{{
        //  //Debug.Assert(unbounded());
        //    Debug.Assert(((vertex(0).is_infinite ? 1 : 0) + (vertex(1).is_infinite ? 1 : 0) + (vertex(2).is_infinite ? 1 : 0) == 1));
        //    int idx =
        //      (vertex(0).is_infinite) ? 0 :
        //      (vertex(1).is_infinite) ? 1 :
        //      2;
        //    return idx;
        //} // }}}

        //public partial bool unbounded()
        //{ /// {{{
        //    return (vertex(0).is_infinite ||
        //           vertex(1).is_infinite ||
        //           vertex(2).is_infinite);
        //} // }}}

#if !SURF_NDEBUG
        internal void assert_valid()
        { //{{{
            Debug.Assert(!is_dead_);
            //DBG_FUNC_BEGIN(//DBG_TRIANGLE_ASSERT_VALID);
            //DBG(//DBG_TRIANGLE_ASSERT_VALID) << this;
            for (int i = 0; i < 3; ++i)
            {
                //DBG(//DBG_TRIANGLE_ASSERT_VALID) << "- v" << i << ": " << vertices[i].details();
            }
            for (int i = 0; i < 3; ++i)
            {
                Debug.Assert(vertices[i] != null); // "Missing vertex %d", i
                Debug.Assert((neighbors[i] != null) == (wavefronts[i] == null)); // "Wavefront vs. neighbor existence mismatch at %d", i

                if (neighbors[i] != null)
                {
                    /* Not a constraint. */
                    var n = neighbors[i];
                    //DBG(//DBG_TRIANGLE_ASSERT_VALID) << "- " << i << ": - checking neighbor " << n;
                    Debug.Assert(n.has_neighbor(this)); // "Neighborhood relation inconsistent"
                    int n_idx = n.index(this);
                    //DBG(//DBG_TRIANGLE_ASSERT_VALID) << "     - checking edge/vertex match: 1  " << vertices[cw(i)] << " vs " << n.vertices[ccw(n_idx)];
                    //DBG(//DBG_TRIANGLE_ASSERT_VALID) << "     - checking edge/vertex match: 2  " << vertices[ccw(i)] << " vs " << n.vertices[cw(n_idx)];
                    Debug.Assert(vertices[cw(i)] == n.vertices[ccw(n_idx)]); // "Edge vertex mismatch"
                    Debug.Assert(vertices[ccw(i)] == n.vertices[cw(n_idx)]); // "Edge vertex mismatch"
                }
                else
                {
                    //DBG(//DBG_TRIANGLE_ASSERT_VALID) << "- " << i << ": checking wavefront";
                    Debug.Assert(wavefronts[i] != null);
                    //DBG(//DBG_TRIANGLE_ASSERT_VALID) << "- " << i << ":                   : " << *wavefronts[i];
                    Debug.Assert(wavefronts[i].incident_triangle() != null);
                    Debug.Assert(wavefronts[i].incident_triangle() == this);
                    Debug.Assert(!wavefronts[i].is_dead());

                    /* a wavefront has vertices */
                    Debug.Assert(wavefronts[i].vertex(0) == vertices[ccw(i)]);
                    Debug.Assert(wavefronts[i].vertex(1) == vertices[cw(i)]);

                    /* a vertex has wavefronts */
                    Debug.Assert(vertices[ccw(i)].wavefronts()[1] == wavefronts[i]);
                    Debug.Assert(vertices[cw(i)].wavefronts()[0] == wavefronts[i]);

                    // XXX check if input edge oriented correctly (same as constraint)
                }
            }
            //DBG_FUNC_END(//DBG_TRIANGLE_ASSERT_VALID);
        }//}}}
#endif

        private CollapseSpec compute_collapse(double time_now)
        { 
             var  result = compute_collapse_bounded(time_now);
            
            return result;
        } 

        

        private static  CollapseSpec event_that_will_not_happen(int component, double time_now, Polynomial_1 determinant)
        {
            //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
            CollapseSpec result = new CollapseSpec(component);

# if DEBUG_COLLAPSE_TIMES
            double collapse_time;
            bool has_collapse = get_generic_collapse_time(time_now, determinant, collapse_time);
            if (has_collapse)
            {
                result = CollapseSpec(component, CollapseType.INVALID_EVENT, collapse_time);
                //DBG(//DBG_TRIANGLE_TIMING) << "Putting something into the priority queue anyway at determinant's correct zero.";
            }
            else
            {
                result = CollapseSpec(component, CollapseType.NEVER);
            }
#else
            result = new CollapseSpec(component, CollapseType.NEVER);
#endif

            //DBG(//DBG_TRIANGLE) << "returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        }

        /** Check if a vertex is moving faster or slower relative to an edge.
         *
         * If the vertex is in front of the edge, and the edge is faster
         * (POSITIVE), the edge may eventually catch up, causing a split
         * or flip event.
         *
         * If the vertex is faster then the edge (i.e. if the edge is slower,
         * NEGATIVE), and if the vertex is behind, v might overtake the edge
         * causing a flip event.
         *
         * If the edge is slower, return -1.  If it's faster +1.  If the same speed, 0.
         *
         * (As a result, if they move in opposite directions, then e is "faster" and +1 is returned.)
         */
        public static  int edge_is_faster_than_vertex(WavefrontVertex v, WavefrontSupportingLine e)
        {
            //DBG_FUNC_BEGIN(//DBG_TRIANGLE_TIMING2);

            /* Let n be some normal to e,
             * let s be the speed (well, velocity) vector of the vertex v.
             * let w be the weight (speed) of e.
             */
            Vector2D n = e.normal_direction;
            Vector2D s = v.velocity;
            double w = e.weight;
            /* Then s.n is the length of the projection of s onto n, times the length of n.
             * Per time unit, v and e will approach each other by (w - s.n/|n|).
             */

            double scaled_edge_speed = w * n.Len_2D();
            double scaled_vertex_speed = s.DotXY(n);

            double speed_approach = scaled_edge_speed - scaled_vertex_speed;
            int sign = MathEx.Sign(speed_approach);

            //DBG(//DBG_TRIANGLE_TIMING2) << "returning " << sign;
            //DBG_FUNC_END(//DBG_TRIANGLE_TIMING2);
            return sign;
        }

        /** Compute when the vertex v will move over the supporting line of e (or crash into e).
         *
         * We only call this function when we have a triangle with exactly one constraint.
         *
         * The resulting time might also be now or be in the past.
         */

        public static (double, VertexOnSupportingLineType) get_time_vertex_on_supporting_line(WavefrontVertex v, WavefrontSupportingLine e)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE_TIMING2);
            double collapse_time;
            VertexOnSupportingLineType vertex_on_line_type;

            /* Let n be some normal to e,
             * let P be some point on e (at time zero),
             * let s be the speed (well, velocity) vector of the vertex v.
             * let Q be the location of v at time zero, and
             * let w be the weight (speed) of e.
             */
            var n = (e.normal_direction);
            var P = (e.l.point());
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
            var PQ = Q-P;
            double scaled_distance = PQ.Dot(n);
            double scaled_edge_speed = w * n.Len_2D();
            double scaled_vertex_speed = s.Dot( n);
            double scaled_speed_approach = scaled_edge_speed - scaled_vertex_speed;
            /*
            */
            //DBG(//DBG_TRIANGLE_TIMING2) << " -- n: " << CGAL_vector(n);
            //DBG(//DBG_TRIANGLE_TIMING2) << " -- P: " << CGAL_point(P);
            //DBG(//DBG_TRIANGLE_TIMING2) << " -- s: " << CGAL_vector(s);
            //DBG(//DBG_TRIANGLE_TIMING2) << " -- Q: " << CGAL_point(Q);
            //DBG(//DBG_TRIANGLE_TIMING2) << " -- w: " << to_double(w);
            //DBG(//DBG_TRIANGLE_TIMING2) << " -- PQ: " << CGAL_vector(PQ);
            //DBG(//DBG_TRIANGLE_TIMING2) << " -- num (∝ distance      ): " << to_double(scaled_distance);
            //DBG(//DBG_TRIANGLE_TIMING2) << " -- den (∝ approach speed): " << to_double(scaled_speed_approach);

            if (scaled_speed_approach == 0)
            {
                collapse_time = 0.0;
                if (scaled_distance == 0)
                {
                    vertex_on_line_type = VertexOnSupportingLineType.ALWAYS;
                }
                else
                {
                    vertex_on_line_type = VertexOnSupportingLineType.NEVER;
                }
            }
            else
            {
                collapse_time = scaled_distance / scaled_speed_approach;
                vertex_on_line_type = VertexOnSupportingLineType.ONCE;
            }

            //DBG(//DBG_TRIANGLE_TIMING2) << "returning " << to_double(collapse_time) << " with VertexOnSupportingLineType " << vertex_on_line_type;
            //DBG_FUNC_END(//DBG_TRIANGLE_TIMING2);
            return (collapse_time, vertex_on_line_type);
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

        /** find potential split or flip_event.
         *
         * We have a triangle with exactly one constraint, e, and opposite vertex v.
         * We don't like the collapse of e.  This can happen if e has
         * two parallel endpoints (so it does not collapse) or it witnesses
         * the wrong root/zero of the determinant polynomial of degree two.
         *
         * So check
         *  - if v crashes into e, or
         *  - if a vertex incident to e moves over a spoke (as v moves over the
         *    supporting line of e).
         */
        private CollapseSpec compute_split_or_flip_event_bounded_constrained_1(double time_now, int c_idx, Polynomial_1 determinant)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
          //DBG(//DBG_TRIANGLE) << this;
            CollapseSpec result = new CollapseSpec(component);

            Debug.Assert(c_idx < 3);
            Debug.Assert(wavefronts[c_idx] != null);
            Debug.Assert(wavefronts[cw(c_idx)] == null);
            Debug.Assert(wavefronts[ccw(c_idx)] == null);

            /* If all of the vertices are convex, this can't happen. */
            if (!vertices[0].is_reflex_or_straight() &&
                !vertices[1].is_reflex_or_straight() &&
                !vertices[2].is_reflex_or_straight())
            {
                //DBG(//DBG_TRIANGLE_TIMING) << this << " all convex vertices.  Will never see an event.";
                result = event_that_will_not_happen(component, time_now, determinant);
            }
            else
            {
                WavefrontSupportingLine e = new WavefrontSupportingLine(wavefront(c_idx).l().l);
                WavefrontVertex v = vertex(c_idx);
                double collapse_time;
                VertexOnSupportingLineType vertex_on_line_type;
                (collapse_time, vertex_on_line_type) = get_time_vertex_on_supporting_line(v, e);
                switch (vertex_on_line_type)
                {
                    case VertexOnSupportingLineType.ONCE:
                        if (collapse_time > time_now)
                        {
                            //DBG(//DBG_TRIANGLE_TIMING) << " v will hit supporting line of e at time " << to_double(collapse_time);
                            result = new CollapseSpec(component, CollapseType.SPLIT_OR_FLIP_REFINE, collapse_time, c_idx);
                        }
                        else if (collapse_time == time_now)
                        {
                            //DBG(//DBG_TRIANGLE_TIMING) << " v is on the supporting line of e right now " << to_double(collapse_time);
                            if (determinant.degree() == 2)
                            {
                                //DBG(//DBG_TRIANGLE_TIMING) << " determinant degree 2";
                                if (accept_collapse_bounded_constrained_1(collapse_time, determinant, false))
                                {
                                    //DBG(//DBG_TRIANGLE_TIMING) << " Will want to handle this.";
                                    result = new CollapseSpec(component, CollapseType.SPLIT_OR_FLIP_REFINE, collapse_time, c_idx);
                                }
                                else
                                {
                                    //DBG(//DBG_TRIANGLE_TIMING) << " But the triangle is growing.";
                                    result = new CollapseSpec(component, CollapseType.NEVER);
                                }
                            }
                            else
                            {
                                Debug.Assert(determinant.degree() == 1);
                                var sign = (ESign) determinant.sign();

                                //DBG(//DBG_TRIANGLE_TIMING) << " determinant degree 1, sign " << sign;
                                if (sign == ESign.NEGATIVE)
                                {
                                    //DBG(//DBG_TRIANGLE_TIMING) << " Will want to handle this.";
                                    result = new CollapseSpec(component, CollapseType.SPLIT_OR_FLIP_REFINE, collapse_time, c_idx);
                                }
                                else
                                {
                                    //LOG(WARNING) << "Untested code path.";
                                    //DBG(//DBG_TRIANGLE_TIMING) << " But the triangle is growing.";
                                    result = new CollapseSpec(component, CollapseType.NEVER);
                                }
                            }
                        }
                        else
                        {
                            //DBG(//DBG_TRIANGLE_TIMING) << " v will not hit supporting line of e.";
                            result = event_that_will_not_happen(component, time_now, determinant);
                            //     Debug.Assert(result.type() == CollapseType.NEVER); // XXX if this holds, we can drop the event_that_will_not_happen thing
                        }
                        break;

                    case VertexOnSupportingLineType.NEVER:
                        //DBG(//DBG_TRIANGLE_TIMING) << " v will never hit supporting line of e as they have the same speed";
                        result = new CollapseSpec(component, CollapseType.NEVER);
                        break;

                    case VertexOnSupportingLineType.ALWAYS:
                        //DBG(//DBG_TRIANGLE_TIMING) << " v is on the supporting line of e and just as fast.  Event now.";
                        result = new CollapseSpec(component, CollapseType.SPLIT_OR_FLIP_REFINE, time_now, c_idx);
                        break;
                }
            }
            //DBG(//DBG_TRIANGLE) << this << " returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        } // }}}

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
        private static  bool get_generic_collapse_time(double time_now, Polynomial_1 det, out double collapse_time)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE_TIMING2);

            bool result;
            collapse_time = double.NaN;
#if NT_USE_DOUBLE
            static double last_time = 0;
            static int count = 0;
#endif

            var sign = (ESign) det.sign();
            //DBG(//DBG_TRIANGLE_TIMING2) << "polynomial has degree " << det.degree() << " and sign " << sign;
            if (det.degree() == 0)
            {
                /*
                 * This should really only happen when collinear points on the convex hull move such that they stay collinear.
                 *
                 * No need to switch things around, then.
                 * If they change order, we should, eventually, catch that elsewhere because the vertices become incident.
                 */
                //LOG(WARNING) << "Have a polynomial of degree zero, Can we catch this sooner?  at time " << to_double(time_now);
                if ( MathEx.Sign(det[0]) == (int) ESign.ZERO)
                {
                    result = true;
                    //LOG(WARNING) << " collapses now (and always) too.";
////# if NT_USE_DOUBLE
//                    if (time_now == last_time)
//                    {
//                        ++count;
//                        if (count > 1000)
//                        {
//                            //LOG(ERROR) << "In double loop at line " << __FILE__ << ":" << __LINE__;
//                            abort();
//                        };
//                    }
//                    else
//                    {
//                        count = 0;
//                        last_time = time_now;
//                    };
////#endif
                }
                else
                {
                    result = false;
                }
            }
            else if (det.degree() == 1)
            {
                Debug.Assert(sign != ESign.ZERO);
                collapse_time = -det[0] / det[1];
                if (collapse_time == time_now)
                {
                    if (sign == ESign.POSITIVE)
                    {
                        //DBG(//DBG_TRIANGLE_TIMING2) << "Triangle area is zero and increasing, not using this collapse time";
                        result = false;
                    }
                    else
                    {
                        //LOG(WARNING) << "Polynomial (of degree 1) has a zero right right now.  Can we catch this sooner?.  at time " << to_double(time_now);
                        //DBG(//DBG_TRIANGLE_TIMING2) << "Triangle area is zero and decreasing, using this collapse time";
# if NT_USE_DOUBLE
                        if (time_now == last_time)
                        {
                            ++count;
                            if (count > 1000)
                            {
                                //LOG(ERROR) << "In double loop at line " << __FILE__ << ":" << __LINE__;
                                abort();
                            };
                        }
                        else
                        {
                            count = 0;
                            last_time = time_now;
                        };
#endif
                        result = true;
                    }
                }
                else if (collapse_time > time_now)
                {
                    Debug.Assert(sign == ESign.NEGATIVE);
                    result = true;
                    //DBG(//DBG_TRIANGLE_TIMING2) << "Triangle area is polynomial of degree one, and zero is in the future.  Using this.";
                }
                else
                {
                    //DBG(//DBG_TRIANGLE_TIMING2) << "Triangle area is polynomial of degree one, and zero is in the past.  Not using this.";
                    Debug.Assert(sign == ESign.POSITIVE);
                    result = false;
                };
            }
            else
            {
                Debug.Assert(det.degree() == 2);
                Debug.Assert(sign != ESign.ZERO);
                //DBG(//DBG_TRIANGLE_TIMING2)
                //<< to_double(det[2]) << ".t^2 + "
                //<< to_double(det[1]) << ".t + "
                //<< to_double(det[0]);

                double x0, x1;
                //DBG(//DBG_TRIANGLE_TIMING2) << "solving quadratic.";
                bool has_real_roots, is_square;
                (has_real_roots, is_square) = MathEx.solve_quadratic(det, out x0, out x1);
                if (!has_real_roots)
                {
                    //DBG(//DBG_TRIANGLE_TIMING2) << "no real solutions.  sign is " << sign;
                    Debug.Assert(sign == ESign.POSITIVE);
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

                    Debug.Assert(sign == ESign.NEGATIVE || sign == ESign.POSITIVE);
                    if (sign == ESign.NEGATIVE)
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



        private  Polynomial_1 compute_determinant_from_vertices(WavefrontVertex v0, WavefrontVertex v1, WavefrontVertex v2)
        {
            Debug.Assert(v0 != null);
            Debug.Assert(v1 != null);
            Debug.Assert(v2 != null);
            return MathEx.compute_determinant(
                    v0.px(), v0.py(),
                    v1.px(), v1.py(),
                    v2.px(), v2.py());
        }

        /** only called for unconstrained triangles, only from compute_flip_event
         *
         * Can return NEVER, TRIANGLE_COLLAPSE, SPOKE_COLLAPSE, or VERTEX_MOVES_OVER_SPOKE.
         */
        private CollapseSpec get_generic_collapse(double time_now, Polynomial_1 determinant)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
          //DBG(//DBG_TRIANGLE) << this;

            Debug.Assert(!is_constrained(0) && !is_constrained(1) && !is_constrained(2));

            CollapseSpec result = new CollapseSpec(component);
            double collapse_time;
            //
            // XXX use is_squared in the counting zero length edges down below
            bool triangle_will_collapse = get_generic_collapse_time(time_now, determinant, out collapse_time);
            if (triangle_will_collapse)
            {
                //DBG(//DBG_TRIANGLE_TIMING) << this << " collapse time is " << to_double(collapse_time);
                Vector2D[] p = new Vector2D[] { vertex(0).p_at(collapse_time),
                           vertex(1).p_at(collapse_time),
                           vertex(2).p_at(collapse_time) };
                double[] squared_lengths = new[]{ 
                                    (p[1]-p[2]).Len_2D(),
                                    (p[2]-p[0]).Len_2D(),
                                    (p[0]-p[1]).Len_2D() };

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
                        result = new CollapseSpec(component, CollapseType.TRIANGLE_COLLAPSE, collapse_time);
                        break;

                    case 1:
                        {
                            int zero_idx = is_zero[0] ? 0 :
                                           is_zero[1] ? 1 :
                                                   2;
                            Debug.Assert(squared_lengths[cw(zero_idx)] == squared_lengths[ccw(zero_idx)]);
                            result = new CollapseSpec(component, CollapseType.SPOKE_COLLAPSE, collapse_time, zero_idx);
                        }
                        break;

                    default:
                        Debug.Assert(cnt_zero == 0);
                        {
                            //DBG(//DBG_TRIANGLE_TIMING2) << this << " sorting lengths.";
                            int i0, i1, i2;
                            (i0, i1, i2) = MathEx.indirect_sort_3(squared_lengths);

                            //DBG(//DBG_TRIANGLE_TIMING2) << this << " edge at collapse time is " << i0 << ".  length: " << to_double(squared_lengths[i0]);
                            //DBG(//DBG_TRIANGLE_TIMING2) << this << " edge at collapse time is " << i1 << ".  length: " << to_double(squared_lengths[i1]);
                            //DBG(//DBG_TRIANGLE_TIMING2) << this << " edge at collapse time is " << i2 << ".  length: " << to_double(squared_lengths[i2]);

                            Debug.Assert(squared_lengths[i1] < squared_lengths[i2]);
                            if (determinant.degree() == 0)
                            {
                                //DBG(//DBG_TRIANGLE_TIMING2) << this << " As determinant has degree zero, use current time as collapse time";
                                collapse_time = time_now;
                            };
                            result = new CollapseSpec(component, CollapseType.VERTEX_MOVES_OVER_SPOKE, collapse_time, i2, squared_lengths[i2]);
                        }
                        break;
                }
            }
            else
            {
                result = new CollapseSpec(component, CollapseType.NEVER);
            }

            //DBG(//DBG_TRIANGLE) << this << " returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        } // }}}

        /** only called for unconstrained triangles */
        private CollapseSpec compute_flip_event(double time_now, Polynomial_1 determinant)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
          //DBG(//DBG_TRIANGLE) << this;

            Debug.Assert(!is_constrained(0) && !is_constrained(1) && !is_constrained(2));
            CollapseSpec result = new CollapseSpec(component);

            bool could_flip = vertices[0].is_reflex_or_straight() ||
                              vertices[1].is_reflex_or_straight() ||
                              vertices[2].is_reflex_or_straight();
            if (!could_flip)
            {
                result = event_that_will_not_happen(component, time_now, determinant);
            }
            else
            {
                result = get_generic_collapse(time_now, determinant);
                switch (result.type())
                {
                    case CollapseType.NEVER:
                        break;

                    case CollapseType.TRIANGLE_COLLAPSE:
                    case CollapseType.SPOKE_COLLAPSE:
                        //LOG(INFO) << "compute_flip_event() found a triangle/spoke collapse: " << result;
                        break;

                    case CollapseType.VERTEX_MOVES_OVER_SPOKE:
                        Debug.Assert(wavefronts[result.relevant_edge()] == null);
                        if (vertices[result.relevant_edge()].is_reflex_or_straight())
                        {
                            // We are good, the vertex is reflex and the edge a spoke.
                        }
                        else
                        {
                            // The vertex is not reflex (or at least collinear), and therefore this is an event that
                            // should never actually happen because we rebuild things until then.
# if DEBUG_COLLAPSE_TIMES
                            result = new CollapseSpec(component, CollapseType.INVALID_EVENT, result.time());
#else
                            result = new CollapseSpec(component, CollapseType.NEVER);
#endif
                        }
                        break;

                    case CollapseType.FACE_HAS_INFINITELY_FAST_VERTEX_OPPOSING:
                    case CollapseType.FACE_HAS_INFINITELY_FAST_VERTEX_WEIGHTED:
                    case CollapseType.SPLIT_OR_FLIP_REFINE:
                    case CollapseType.CONSTRAINT_COLLAPSE:
                    case CollapseType.UNDEFINED:
                    case CollapseType.CCW_VERTEX_LEAVES_CH:
                    case CollapseType.INVALID_EVENT:
                    default:
                        {
                            throw new Exception($"Unexpected result from get_generic_collapse: {result}");

                        }
                }
            }

            //DBG(//DBG_TRIANGLE) << this << " returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        } // }}}

        /** Compute the collapse spec for a bounded triangle with 3 contraints.
         *
         * Since all 3 edges are constrained, this can only be a triangle
         * collapse.  This happens when all 3 edges collapse at the same time.
         */
        private CollapseSpec compute_collapse_bounded_constrained_3(double time_now)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
          //DBG(//DBG_TRIANGLE) << this;

            CollapseSpec candidate = (wavefronts[0].get_collapse(component, time_now, 0));
            for (int i = 1; i < 3; ++i)
            {
                Debug.Assert(candidate == wavefronts[i].get_collapse(component, time_now, i));
            };
            Debug.Assert(candidate.type() == CollapseType.CONSTRAINT_COLLAPSE);
            CollapseSpec result = new CollapseSpec(component, CollapseType.TRIANGLE_COLLAPSE, candidate.time());

            //DBG(//DBG_TRIANGLE) << "returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        } // }}}

        /** Compute the collapse spec for a bounded triangle with 2 contraints.
         *
         * Each (constrained) edge collapse witnesses the vanishing of the triangle
         * and thus one of the roots of the triangle's determinant.
         *
         * If they collapse at the same time, this is a triangle collapse.  If not,
         * then this is an edge event.
         */
        private CollapseSpec compute_collapse_bounded_constrained_2(double time_now)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
          //DBG(//DBG_TRIANGLE) << this;
            CollapseSpec result = new CollapseSpec(component);

            int c1_idx = wavefronts[0] != null ? 0 : 1;
            int c2_idx = wavefronts[2] != null ? 2 : 1;
            Debug.Assert(c1_idx != c2_idx);
            Debug.Assert(wavefronts[c1_idx] != null);
            Debug.Assert(wavefronts[c2_idx] != null);

            //DBG(//DBG_TRIANGLE) << "v0: " << vertices[0].details();
            //DBG(//DBG_TRIANGLE) << "v1: " << vertices[1].details();
            //DBG(//DBG_TRIANGLE) << "v2: " << vertices[2].details();
            //DBG(//DBG_TRIANGLE) << "wavefront idx 1: " << c1_idx;
            //DBG(//DBG_TRIANGLE) << "wavefront idx 2: " << c2_idx;
            //DBG(//DBG_TRIANGLE) << "wavefront 1: " << *wavefronts[c1_idx];
            //DBG(//DBG_TRIANGLE) << "wavefront 2: " << *wavefronts[c2_idx];
            CollapseSpec c1 = (wavefronts[c1_idx].get_collapse(component, time_now, c1_idx));
            CollapseSpec c2 = (wavefronts[c2_idx].get_collapse(component, time_now, c2_idx));
            Debug.Assert(c1.type() == CollapseType.CONSTRAINT_COLLAPSE || c1.type() == CollapseType.NEVER);
            Debug.Assert(c2.type() == CollapseType.CONSTRAINT_COLLAPSE || c2.type() == CollapseType.NEVER);
            //DBG(//DBG_TRIANGLE) << "constraint collapse 1: " << c1;
            //DBG(//DBG_TRIANGLE) << "constraint collapse 2: " << c2;
            if (c1.type() == CollapseType.NEVER)
            {
                result = c2;
            }
            else if (c1.type() == CollapseType.NEVER)
            {
                result = c1;
            }
            else if (c1 == c2)
            { /* both constraints collapse at this time. */
                result = new CollapseSpec(component, CollapseType.TRIANGLE_COLLAPSE, c1.time());
            }
            else
            {
                result = c1.CompareTo(c2) < 1 ? c1 : c2;
            }

            //DBG(//DBG_TRIANGLE) << "returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        } // }}}

        /** check if we like a specific collapse as an event.
         *
         * This checkfs whether the collapse at time is really the next instance
         * that the triangle collapses.
         *
         * The area of the triangle as a function of time is proportional to the
         * determinant and is a quadratic in time.  Together with the sign of the
         * leading coefficient of the determinant we can evaluate the derivative of the
         * determinant at time t to see whether this is the next time the determinant
         * vanishes.
         *
         * Unchecked precondition: right *now* (where we try to find the next event time),
         * the determinant of the triangle is not negative.  That is, it's a valid (or
         * degenerate) triangle.
         *
         * If the leading coefficient of det is
         *  - negative, then this means one triangle collapse was in the past already,
         *    and any event we found must be a real one.
         *  - positive, then we are either before or after the time when the area is
         *    negative.
         *    In such cases, we never want the second time (since it'd mean we came
         *    from an invalid triangulation to begin with), only the first.  So, to
         *    verify if the collapse time is the first or the second event, we
         *    look at the sign of the determinant's derivative evaluated at t.
         *    If it's negative, the collapse is the first instance of the triangle
         *    collapsing, otherwise, if the derivative at t is positive, the collapse
         *    is the second instance that the triangle collapses and we'll have
         *    to look for a real event prior.
         *    If the derivative is zero, then this is the only event this triangle
         *    will ever see.  Handle it.
         */
        private static bool accept_collapse_bounded_constrained_1(double collapse_time, Polynomial_1 determinant, bool collapse_time_is_edge_collapse)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
            bool result;

            Debug.Assert(determinant.degree() == 2);
            var determinant_sign =(ESign) determinant.sign();
            Debug.Assert(determinant_sign != ESign.ZERO);

            if (determinant_sign == ESign.NEGATIVE)
            {
                //DBG(//DBG_TRIANGLE) << "Sign is negative, event must be good.  (One collapse was in the past, there only is one more.)";
                result = true;
            }
            else
            {
                Debug.Assert(determinant_sign == ESign.POSITIVE);
                //DBG(//DBG_TRIANGLE) << "Sign is positive, checking if we got the first or second event.";

                Polynomial_1 derivative = determinant.differentiate();
                double derivative_at_collapse = derivative.evaluate( collapse_time);
                //DBG(//DBG_TRIANGLE) << "derivative(t): " << to_double(derivative_at_collapse);

                switch ((ESign)MathEx.Sign(derivative_at_collapse))
                {
                    case ESign.ZERO:
                        //DBG(//DBG_TRIANGLE) << "Derivative is zero.  If an edge collapses right now, then either the triangle collapses entirely, or the 3rd vertex moves over our supporting line right now.  Of course it could also just be that the vertices are collinear exactly once.";
                        if (collapse_time_is_edge_collapse)
                        {
                            //DBG(//DBG_TRIANGLE) << "At any rate, this is an edge collapse and the only event the triangle will ever see.  Handle it.";
                        }
                        else
                        {
                            //DBG(//DBG_TRIANGLE) << "At any rate, since the sign of the determinant is positive, the triangle has positive area after this event, and this is not an edge collapse: we do not need to do anything here.";
                        }
                        result = collapse_time_is_edge_collapse;
                        break;

                    case ESign.NEGATIVE:
                        //DBG(//DBG_TRIANGLE) << "Derivative is negative.  This is the first time the triangle collapses.  We want it.";
                        result = true;
                        break;

                    case ESign.POSITIVE:
                        //DBG(//DBG_TRIANGLE) << "Derivative is positive.  This is the second time the triangle collapses.  This triangle MUST change before the first time it collapses.";
                        result = false;
                        break;

                    default:
                        throw new Exception("Fell through switch which should cover all cases.");
                }
            }

            //DBG(//DBG_TRIANGLE) << "returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        } // }}}

        /** Compute the collapse spec for a bounded triangle with 1 contraint.
         *
         */
        private CollapseSpec compute_collapse_bounded_constrained_1(double time_now)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
          //DBG(//DBG_TRIANGLE) << this;
          //DBG(//DBG_TRIANGLE) << " v0 " << vertex(0).details();
          //DBG(//DBG_TRIANGLE) << " v1 " << vertex(1).details();
          //DBG(//DBG_TRIANGLE) << " v2 " << vertex(2).details();
            CollapseSpec result = new CollapseSpec(component);

            // XXX only compute the determinant if we need it
            //
            Polynomial_1 determinant = (compute_determinant_from_vertices(vertex(0), vertex(1), vertex(2)));
            //DBG(//DBG_TRIANGLE) << "det(time) " << to_double(evaluate(determinant, time_now));
            Debug.Assert(determinant.evaluate( time_now) >= 0.0);

            int c_idx = wavefronts[0] != null ? 0 :
                        wavefronts[1] != null ? 1 :
                                              2;
            WavefrontEdge wf = wavefronts[c_idx];
            if (wf.parallel_endpoints(time_now))
            {
                EdgeCollapseSpec edge_collapse = wf.get_edge_collapse(time_now);
                //DBG(//DBG_TRIANGLE) << "Edge endpoints are parrallel;  collapse is " << edge_collapse << "; det degree is " << determinant.degree();
                if (edge_collapse.type() == EdgeCollapseType.ALWAYS)
                {
                    /* Edge collapses right now */
                    result = new CollapseSpec(component, edge_collapse, c_idx);
                }
                else if (determinant.degree() == 1)
                {
                    result = compute_split_or_flip_event_bounded_constrained_1(time_now, c_idx, determinant);
                }
                else
                {
                    result = new CollapseSpec(component, CollapseType.NEVER);
                }
            }
            else
            {
                CollapseSpec candidate = wf.get_collapse(component, time_now, c_idx);
                //DBG(//DBG_TRIANGLE) << "Edge collapse is " << candidate << "; determinant degree is " << determinant.degree();
                Debug.Assert(candidate.type() == CollapseType.CONSTRAINT_COLLAPSE || candidate.type() == CollapseType.NEVER);

                if (determinant.degree() == 2)
                { // The edge could collapse, or we could flip/split
                    bool have_collapse = (candidate.type() == CollapseType.CONSTRAINT_COLLAPSE) && accept_collapse_bounded_constrained_1(candidate.time(), determinant, true);

                    if (have_collapse)
                    {
                        //DBG(//DBG_TRIANGLE) << "We like the edge collapse.";
                        result = candidate;
                    }
                    else
                    {
                        //DBG(//DBG_TRIANGLE) << "We did not like the edge collapse.  Hunt for the real event.";
                        result = compute_split_or_flip_event_bounded_constrained_1(time_now, c_idx, determinant);
                    }
                }
                else
                {
                    Debug.Assert(determinant.degree() <= 1);
                    if (candidate.type() == CollapseType.NEVER)
                    {
                        //DBG(//DBG_TRIANGLE) << "Determinant: " << determinant;
                        //DBG(//DBG_TRIANGLE) << "Determinant degree < 2 and non-parallel endpoints of the constraint which will never collapse (so would have collapsed in the past).";
                        result = compute_split_or_flip_event_bounded_constrained_1(time_now, c_idx, determinant);
                    }
                    else
                    {
                        Debug.Assert(candidate.type() == CollapseType.CONSTRAINT_COLLAPSE);
                        //DBG(//DBG_TRIANGLE) << "Determinant degree < 2 and non-parallel endpoints of the constraint which will collapse.  We will use the constraint collapse.";
                        result = candidate;
                    }
                }
            }

            //DBG(//DBG_TRIANGLE) << "returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        } // }}}

        /** Compute the collapse spec for a bounded triangle with 0 contraints.
         *
         * Such a triangle can either see a "meet" event, where two non-incident
         * vertices become incident, or it can see a flip event where a reflex
         * vertex moves over a triangluation spoke.
          // XXX do meet events.

         */
        private CollapseSpec compute_collapse_bounded_constrained_0(double time_now)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
          //DBG(//DBG_TRIANGLE) << this;

            Polynomial_1 determinant = compute_determinant_from_vertices(vertex(0), vertex(1), vertex(2));
            var result = compute_flip_event(time_now, determinant);

            //DBG(//DBG_TRIANGLE) << "returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        } // }}}

        private CollapseSpec compute_collapse_bounded(double time_now)
        { /// {{{
            //DBG_FUNC_BEGIN(//DBG_TRIANGLE);
            //DBG(//DBG_TRIANGLE) << this;
            CollapseSpec result;

            /** See notes on classification from 20180731 */
            int num_wavefronts = (wavefronts[0] != null).ToInt() + (wavefronts[1] != null).ToInt() + (wavefronts[2] != null).ToInt();
            //DBG(//DBG_TRIANGLE) << "Have " << num_wavefronts << " constrained edge(s).";
            switch (num_wavefronts)
            {
                case 3:
                    result = compute_collapse_bounded_constrained_3(time_now);
                    break;

                case 2:
                    result = compute_collapse_bounded_constrained_2(time_now);
                    break;

                case 1:
                    result = compute_collapse_bounded_constrained_1(time_now);
                    break;

                case 0:
                    result = compute_collapse_bounded_constrained_0(time_now);
                    break;

                default:
                    throw new Exception($"Invalid number of constrained edges: {num_wavefronts}");

            }
            Debug.Assert(result.type() != CollapseType.UNDEFINED);

            //DBG(//DBG_TRIANGLE) << "returning " << result;
            //DBG_FUNC_END(//DBG_TRIANGLE);
            return result;
        } // }}}

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
            Debug.Assert(!is_dead_);
            for (int i = 0; i < 3; ++i)
            {
                if (neighbors[i] != null)
                {
                    Debug.Assert(!neighbors[i].has_neighbor(this));
                }
                if (wavefronts[i] != null)
                {
                    Debug.Assert(wavefronts[i].incident_triangle() != this || wavefronts[i].is_dead());
                }
            }
            is_dead_ = true;
        } // }}}

        internal void set_vertex(int i, WavefrontVertex v)
        { // {{{
            Debug.Assert(i < 3);
            Debug.Assert(v != null);
            vertices[i] = v;
            if (is_constrained(cw(i)))
            {
                Debug.Assert(wavefronts[cw(i)] != null);
                wavefronts[cw(i)].set_wavefrontedge_vertex(0, v);
            }
            if (is_constrained(ccw(i)))
            {
                Debug.Assert(wavefronts[ccw(i)] != null);
                wavefronts[ccw(i)].set_wavefrontedge_vertex(1, v);
            }
            invalidate_collapse_spec();
        } // }}}

        private void set_wavefront(int idx, WavefrontEdge e)
        { // {{{
            //CGAL_precondition(idx < 3);
            Debug.Assert(e != null);

            Debug.Assert(wavefronts[idx] == null);
            Debug.Assert(neighbors[idx] != null);
            Debug.Assert(neighbors[idx].is_dying());
            Debug.Assert(e.incident_triangle() == neighbors[idx]);

            wavefronts[idx] = e;
            neighbors[idx] = null;
            e.set_incident_triangle(this);
        } // }}}

        internal void move_constraint_from(int idx, KineticTriangle src, int src_idx)
        { /// {{{
           // CGAL_precondition(idx < 3 && src_idx < 3);

            Debug.Assert(src.is_dying());
            Debug.Assert(!is_constrained(idx));
            Debug.Assert(src.wavefronts[src_idx].incident_triangle() == src);
            wavefronts[idx] = src.wavefronts[src_idx];

            // we already need to share one vertex with the origin, which will go away.
            Debug.Assert(wavefronts[idx].vertex(0) == vertices[ccw(idx)] ||
                   wavefronts[idx].vertex(1) == vertices[cw(idx)]);
            Debug.Assert(has_neighbor(src));
            Debug.Assert(idx == index(src));

            wavefronts[idx].set_wavefrontedge_vertex(0, vertices[ccw(idx)]);
            wavefronts[idx].set_wavefrontedge_vertex(1, vertices[cw(idx)]);
            wavefronts[idx].set_incident_triangle(this);

            src.wavefronts[src_idx] = null;
            invalidate_collapse_spec();
        } // }}}

        /** Flip edge <idx> of this triangle.
         *
         * Let t be v, v1, v2.  (where v is the vertex at idx),
         * and let our neighbor opposite v be o, v2, v1.
         *
         * Then after the flipping, this triangle will be v, o, v2,
         * and the neighbor will be o, v, v1.
         */
        private void do_raw_flip_inner(int egde_idx)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE | //DBG_TRIANGLE_FLIP);
          //DBG(//DBG_TRIANGLE | //DBG_TRIANGLE_FLIP) << this;
          //DBG(//DBG_TRIANGLE_FLIP) << this << " * flipping at idx " << idx;
            Debug.Assert(!is_constrained(egde_idx));
            KineticTriangle n = neighbors[egde_idx];
            int nidx = n.index(this);
            //DBG(//DBG_TRIANGLE_FLIP) << "   neighbor " << n << " with nidx " << nidx;

            WavefrontVertex v = vertices[egde_idx];
            WavefrontVertex v1 = vertices[ccw(egde_idx)];
            WavefrontVertex v2 = vertices[cw(egde_idx)];
            WavefrontVertex o = n.vertices[nidx];
            Debug.Assert(v1 == n.vertex(cw(nidx)));
            Debug.Assert(v2 == n.vertex(ccw(nidx)));

            // set new triangles
            vertices[ccw(egde_idx)] = o;
            n.vertices[ccw(nidx)] = v;

            //DBG(//DBG_TRIANGLE_FLIP) << "   * t  " << this;
            //DBG(//DBG_TRIANGLE_FLIP) << "     n  " << n;

            //DBG(//DBG_TRIANGLE_FLIP) << "     na " << n.neighbors[cw(idx)];
            //DBG(//DBG_TRIANGLE_FLIP) << "     nb " << neighbors[cw(idx)];

            // fix neighborhood relations and fix/move constraints
            // - neighbors of t and n that see their neighbor
            //   change from n to t or t to n.
            neighbors[egde_idx] = n.neighbors[cw(nidx)];
            wavefronts[egde_idx] = n.wavefronts[cw(nidx)];
            n.neighbors[nidx] = neighbors[cw(egde_idx)];
            n.wavefronts[nidx] = wavefronts[cw(egde_idx)];

            // - pair up t and n
            n.neighbors[cw(nidx)] = this;
            n.wavefronts[cw(nidx)] = null;
            neighbors[cw(egde_idx)] = n;
            wavefronts[cw(egde_idx)] = null;

            //DBG(//DBG_TRIANGLE_FLIP) << "   * t  " << this;
            //DBG(//DBG_TRIANGLE_FLIP) << "     n  " << n;

            Debug.Assert((neighbors[egde_idx] != null) == (wavefronts[egde_idx] == null));
            Debug.Assert((n.neighbors[nidx] != null) == (n.wavefronts[nidx] == null));

            if (is_constrained(egde_idx))
            {
                wavefronts[egde_idx].set_incident_triangle(this);
            }
            else
            {
                var i = neighbors[egde_idx].index(n);
                neighbors[egde_idx].neighbors[i] = this;
            }
            if (n.wavefronts[nidx] != null)
            {
                n.wavefronts[nidx].set_incident_triangle(n);
            }
            else
            {
                var i = n.neighbors[nidx].index(this);
                n.neighbors[nidx].neighbors[i] = n;
            }

            //DBG_FUNC_END(//DBG_TRIANGLE | //DBG_TRIANGLE_FLIP);
        } // }}}

        /** Flip edge <idx> of this triangle.
         *
         * Run do_raw_flip_inner() and asserts validity after
         * and invalidates collapse specs.
         */
        internal void do_raw_flip(int edge_idx)
        { // {{{
          //DBG_FUNC_BEGIN(//DBG_TRIANGLE | //DBG_TRIANGLE_FLIP);
            KineticTriangle n = neighbors[edge_idx];

            do_raw_flip_inner(edge_idx);

            assert_valid();
            n.assert_valid();

            invalidate_collapse_spec();
            n.invalidate_collapse_spec();

            //DBG_FUNC_END(//DBG_TRIANGLE | //DBG_TRIANGLE_FLIP);
        } // }}}

        //std.ostream & operator <<(std.ostream& os,  KineticTriangle kt)
        //{ // {{{
        //    if (kt)
        //    {
        //        char sep[3] = { ',', ',', ';' };
        //        char sep2[3] = { ',', ',', ';' };

        //        os << kt.get_name()
        //           << " (@"
        //           << (void*)kt << "; ";
        //        for (int i = 0; i < 3; ++i)
        //        {
        //            os << kt.vertices[i] << sep[i];
        //        }
        //        os << " ";
        //        for (int i = 0; i < 3; ++i)
        //        {
        //            if (kt.neighbors[i])
        //            {
        //                os << kt.neighbors[i].get_name() << sep2[i];
        //            }
        //            else
        //            {
        //                os << "*" << sep[i];
        //            };
        //        };
        //        os << " ";
        //        for (int i = 0; i < 3; ++i)
        //        {
        //            if (kt.wavefronts[i])
        //            {
        //                os << *kt.wavefronts[i] << sep2[i];
        //            }
        //            else
        //            {
        //                os << "*" << sep2[i];
        //            };
        //        };
        //        os << " c" << kt.component << ")";
        //    }
        //    else
        //    {
        //        os << "kt*";
        //    }
        //    return os;
        //} // }}}

        //std.ostream & operator <<(std.ostream& os,  KineticTriangle.VertexOnSupportingLineType a)
        //{ // {{{
        //    switch (a)
        //    {
        //        case KineticTriangle.VertexOnSupportingLineType.ONCE:
        //            os << "ONCE";
        //            break;
        //        case KineticTriangle.VertexOnSupportingLineType.ALWAYS:
        //            os << "ALWAYS";
        //            break;
        //        case KineticTriangle.VertexOnSupportingLineType.NEVER:
        //            os << "NEVER";
        //            break;
        //    }
        //    return os;
        //} // }}}
    }
}