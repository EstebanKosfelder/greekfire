using GFL.Kernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GFL
{
    







    public partial class WavefrontEdge
    {


        public WavefrontEdge(int iD, Vector2D point1, Vector2D point2)
        {
            
            id = iD;
            Point1 = point1;
            Point2 = point2;
        }

        public int ID =>id;
        public Vector2D Point1 { get; }
        public Vector2D Point2 { get; }


        private static int wavefront_edge_ctr;
        private readonly int id;
        

        private bool is_dead_ = false;        /** stopped propagating */

        /** The left and right wavefront vertex right
                                   *  now.  This changes over the course of the
                                   *  propagation period.
                                   */

        ///<summary>
        /// The left and right wavefront vertex right now.
        /// This changes over the course of the propagation period.
        ///</summary>
                        private WavefrontVertex[] vertices = new WavefrontVertex[2];

        ///<summary>
        ///  The supporting line backing this wavefront vertex.
        ///</summary>
        private WavefrontSupportingLine supporting_line;

        ///<summary>
        /// The triangle incident right now at this wavefront edge.
        ///</summary>
        private KineticTriangle incident_triangle_ = null;


        /// <summary>
        /// Is this wavefront edge one that was
        /// created initially as part of the input or
        /// from beveling (true), or is it the result
        /// of a wavefront edge having been split during the
        /// propagation period (false).
        /// </summary>
        public readonly bool is_initial; 

        /// <summary>
        /// Is this wavefront edge the result of beveling,
        /// and thus degenerate at time zero?
        /// </summary>
        public readonly bool is_beveling;


        /// <summary>
        /// The pointers to the left and right straight
        /// skeleton arcs (==kinetic wavefront vertex).
        /// Only for is_initial wavefront edges, so
        /// we can then find the faces nicely. 
        /// </summary>
        private WavefrontVertex[] initial_vertices = new WavefrontVertex[2];
       

        ///<summary>
        /// The straight skeleton face that this edge traces out (at least partially.
        /// With splitting, multiple edges are needed to trace out a single face.
        ///</summary>
      

        private EdgeCollapseSpec collapse_spec;
        private bool collapse_spec_valid = false;
#if  !SURF_NDEBUG
        private WavefrontVertex[] collapse_spec_computed_with_vertices = new WavefrontVertex[2];
#endif

        /// <summary>
        /// Used when setting up initial wavefront edges for all constraints 
        /// </summary>
        public WavefrontEdge(int id_, Vector2D u, Vector2D v, double weight = 0, KineticTriangle incident_triangle=null)

        {

            id = id_;
            vertices = new WavefrontVertex[] { null, null };
            supporting_line = new WavefrontSupportingLine(u, v, weight);
            incident_triangle_ = (incident_triangle);
            is_initial = (true);
            is_beveling = (false);
            initial_vertices = new WavefrontVertex[] { null, null };
            //skeleton_face = p_skeleton_face;
#if !SURF_NDEBUG
            collapse_spec_computed_with_vertices = new WavefrontVertex[] { null, null };
#endif
            //Debug.Assert((skeleton_face!=null) ^ is_beveling);
        }

        /// <summary>
        /// Used when setting up bevels 
        /// </summary>
//        public WavefrontEdge(WavefrontSupportingLine p_supporting_line)
//        {
//#if !SURF_NDEBUG
//            id = (wavefront_edge_ctr++);
//#endif
//            vertices = new WavefrontVertex[] { null, null };
//            supporting_line = p_supporting_line;
//            incident_triangle_ = null;
//            is_initial = true;
//            is_beveling = true;
//            initial_vertices = new WavefrontVertex[] { null, null };
//            skeleton_face = null;
//# if !SURF_NDEBUG
//            collapse_spec_computed_with_vertices = new WavefrontVertex[] { null, null };
//#endif

//            Debug.Assert(skeleton_face!=null ^ is_beveling);
//        }

        private WavefrontEdge(int id_,WavefrontVertex va,
                          WavefrontVertex vb,
                          WavefrontSupportingLine p_supporting_line,
                          KineticTriangle incident_triangle,
                          /*DcelFace p_skeleton_face,*/
                          bool p_is_beveling)
        {

            id = id_;

            vertices = new WavefrontVertex[]{ va, vb};
            supporting_line = (p_supporting_line);
            incident_triangle_ = (incident_triangle);
            is_initial = (false);
            is_beveling = (p_is_beveling);
            initial_vertices =new WavefrontVertex[]{ null, null };
          //  skeleton_face = (p_skeleton_face);
#if !SURF_NDEBUG
            collapse_spec_computed_with_vertices = new WavefrontVertex[] { null, null };
#endif

        //    Debug.Assert((skeleton_face!=null) ^ is_beveling);
        }

        public void set_dead()
        {
            Debug.Assert(!is_dead_);
            is_dead_ = true;
        }

        public WavefrontSupportingLine l() =>supporting_line; 

        public KineticTriangle incident_triangle()=> incident_triangle_; 

        public bool is_dead()=>is_dead_;



        public void set_vertices(params  WavefrontVertex[] vertices)
        {
            this.vertices = vertices;
        }

        public void set_initial_vertices(params WavefrontVertex[] initial_vertices)
        {
            this.initial_vertices = initial_vertices;
        }

        public WavefrontVertex vertex(int i)
        {
            Debug.Assert(!is_dead_);
            Debug.Assert(i <= 1);
            return vertices[i];
        }

        public WavefrontVertex initial_vertex(int i)
        {
           Debug.Assert(is_initial);
            Debug.Assert(i <= 1);
            return initial_vertices[i];
        }

        public void set_wavefrontedge_vertex(int i, WavefrontVertex v)
        {
            Debug.Assert(!is_dead_);
            Debug.Assert(i <= 1);
            Debug.Assert(v != null);
            vertices[i] = v;
            invalidate_collapse_spec();
        }

        internal void set_initial_vertices()
        {
            Debug.Assert(is_initial);
            for (int i = 0; i <= 1; ++i)
            {
                Debug.Assert(vertices[i] != null);
                Debug.Assert(initial_vertices[i] == null);
                initial_vertices[i] = vertices[i];
            };
        }

        public partial void set_incident_triangle(KineticTriangle incident_triangle);

        public CollapseSpec get_collapse(int component, double time_now, int collapsing_edge)
        {
            assert_edge_sane(collapsing_edge);
            return new CollapseSpec(component, get_edge_collapse(time_now), collapsing_edge);
        }

        public EdgeCollapseSpec get_edge_collapse(double time_now)
        {
            Debug.Assert(!is_dead_);
            Debug.Assert(vertices[0] != null);
            Debug.Assert(vertices[1] != null);
            if (!collapse_spec_valid)
            {
                collapse_spec = compute_collapse(time_now);
                collapse_spec_valid = true;
#if !SURF_NDEBUG
                set_to_cur_wf_vertices(collapse_spec_computed_with_vertices);
#endif
            };
            return get_cached_edge_collapse();
        }

        public partial EdgePtrPair split(List<WavefrontEdge> wavefront_edges);

        public bool parallel_endpoints(double time_now)
        {
            EdgeCollapseSpec e = get_edge_collapse(time_now);
            switch (e.type())
            {
                case EdgeCollapseType.UNDEFINED:
                    Debug.Assert(false);
                    return false;

                case EdgeCollapseType.PAST:
                case EdgeCollapseType.FUTURE:
                    return false;

                case EdgeCollapseType.ALWAYS:
                case EdgeCollapseType.NEVER:
                    return true;
            }
            Debug.Assert(false);
            return false;
        }

#if !SURF_NDEBUG

        private partial void assert_edge_sane(int collapsing_edge);

#else
private void assert_edge_sane(int collapsing_edge) {};
#endif

        private EdgeCollapseSpec get_cached_edge_collapse()
        {
            Debug.Assert(collapse_spec_valid);
#if !SURF_NDEBUG
            assert_cur_wf_vertices(collapse_spec_computed_with_vertices);
#endif
            return collapse_spec;
        }

        private void invalidate_collapse_spec()
        {
            collapse_spec_valid = false;
#if !SURF_NDEBUG
            invalidte_cur_wf_vertices(collapse_spec_computed_with_vertices);
#endif
        }

#if !SURF_NDEBUG

        private void set_to_cur_wf_vertices(WavefrontVertex[] arr)
        {
            for (int i = 0; i < 2; ++i)
            {
                arr[i] = vertices[i];
            }
        }

        private void invalidte_cur_wf_vertices(WavefrontVertex[] arr)
        {
            for (int i = 0; i < 2; ++i)
            {
                arr[i] = null;
            }
        }

        private void assert_cur_wf_vertices(WavefrontVertex[] arr)
        {
            for (int i = 0; i < 2; ++i)
            {
                Debug.Assert(arr[i] != null);
                Debug.Assert(arr[i] == vertices[i]);
            }
        }

#endif

        private partial EdgeCollapseSpec compute_collapse(double time_now);


        /** returns when this edge will collapse.
         *
         * If the two vertices are parallel or moving away from one another,
         * they will NEVER collapse.  Even if the two end-points are
         * conincident right now but are moving away from another, we will consider
         * this as not collapsing.
         *
         * Otherwise, they will collapse at some point in time.  Since
         * we are asking, we assume (and we Debug.Assert() during debugging),
         * that this will be in the future (or now).
         */

        private partial EdgeCollapseSpec compute_collapse(double time_now)
        {
            //DBG_FUNC_BEGIN(DBG_KT);
            //DBG(DBG_KT) << "Computing edge collapse time for " << * this;

            EdgeCollapseSpec res;
            WavefrontVertex wfv0 = vertices[0];
            WavefrontVertex wfv1 = vertices[1];
            Debug.Assert(wfv0 != null);
            Debug.Assert(wfv1 != null);
            Vector2D v0 = wfv0.velocity;
            Vector2D v1 = wfv1.velocity;

            //DBG(DBG_KT) << "v0" << CGAL_vector(v0);
            // DBG(DBG_KT) << "v1" << CGAL_vector(v1);
            var o =(ESign) MathEx.orientation(v0, v1);

            if (o != ESign.LEFT_TURN)
            {
                /* If the two wavefront vertices move away from each other
                 * or in parallel, this edge will never collapse.
                 */
                if (o == ESign.RIGHT_TURN)
                {
                    //  DBG(DBG_KT) << "Orientation is right turn";
                    // let's consider two points that are identical right now but moving away from another as not collapsing.
                    res = new EdgeCollapseSpec(EdgeCollapseType.PAST);
                }
                else
                {
                    // DBG(DBG_KT) << "Orientation is collinear";
                    Debug.Assert(o == ESign.COLLINEAR);

                    /* Previously we computed the distance at time_now.  I wonder why.
                     * If we then claim that the edge is always collapsing, then it should
                     * suffice to compute the distance at t=0. */
                    double sqdist = (wfv0.pos_zero - wfv1.pos_zero).L2();
                    //DBG(DBG_KT) << "sqdist zero: " << CGAL::to_double(sqdist);

                    {
                        Vector2D p0 = (wfv0.p_at(time_now));
                        Vector2D p1 = (wfv1.p_at(time_now));
                        double sqdistnow = (p0-p1).L2();
                        //     DBG(DBG_KT) << "sqdist now : " << sqdistnow;
                        if (sqdist == Const.CORE_ZERO)
                        {
                         //   assert_expensive_eq(sqdist, sqdistnow);
                        }
                        else
                        {
                            Debug.Assert(sqdistnow > Const.CORE_ZERO);
                        }
                    }

                    if (sqdist == Const.CORE_ZERO)
                    {
                        // DBG(DBG_KT) << "Distance is zero now.";
                        res = new EdgeCollapseSpec(EdgeCollapseType.ALWAYS, time_now);
                    }
                    else
                    {
                        //  DBG(DBG_KT) << "Distance is not zero.";
                        res = new EdgeCollapseSpec(EdgeCollapseType.NEVER);
                    }
                }
            }
            else
            {
                // DBG(DBG_KT) << "Orientation is left turn";
                /* Note that, by construction, if you start on the wavefront edge, go out v0,
                 * and go back v1, you end up on the wavefrong edge again.  Or, in other words,
                 * v0-v1 is collinear with the direction of the wavefront edge e.
                 *
                 * Now, we want to know when e=AB collapses.  So we can restrict ourselves to
                 * consider the projection of A+t*v0 and B+t*v1 to e itself.  Once they meet,
                 * the edge collapses.  Equivalently, once the length of projected t*v0 and
                 * projected t*v1 equals the length of e, the edge collapses.
                 *
                 * Let d be the vector AB (i.e., B-A).  The dot product v0.d is the length of
                 * the projected v0 times the length of d.  So v0.d/|d| is the length of the
                 * projected v0.  Likewise, v1.d/|d| is the length of the projected v1.
                 * Thus e will collapse at time t := |d| / ( v0.d/|d| - v1.d/|d| ) ==
                 * == |d|^2 / ( v0.d - v1.d ) == |d|^2 / ( (v0 - v1).d) ==
                 * == d.d /  ( (v0 - v1).d).
                 *
                 * Isn't that interesting?  Note how we compare the length of d projected
                 * onto d with the length of (v0 - v1) projected onto d?  Remember that, as
                 * previously mentioned, by construction, (v0 - v1) is in the same direction
                 * as d.  Thus, this quotient will have the same value regardless of
                 * which line we project both d and (v0 - v1) -- as long as the line is not
                 * orthogonal to (v0 - v1).  So we might just as well project onto (1,0) and
                 * only consider their x-coordinates (if d is not exactly vertical).
                 * So t == d_x / (v0_x - v1_x)
                 *
                 * This also makes sense when looking at it another way.  Consider the points
                 * of the projection of A+t*v0 and B+t*v1 onto e.  Since they are on e, they
                 * will become incident when and only when their x-coordinates is the same.
                 */
                double edge_delta;
                double wfvs_delta;
                if (!supporting_line.l.is_vertical())
                {
                    edge_delta = wfv1.pos_zero.X - wfv0.pos_zero.X;
                    wfvs_delta = v0.X - v1.X;
                }
                else
                { /* l is vertical, do the same with y-coordinates */
                    edge_delta = wfv1.pos_zero.Y - wfv0.pos_zero.Y;
                    wfvs_delta = v0.Y - v1.Y;
                }

                Debug.Assert(edge_delta != 0);
                Debug.Assert(wfvs_delta != 0);
                double time = edge_delta / wfvs_delta;
                //DBG(DBG_KT) << "future edge collapse: " << CGAL::to_double(time);
                //DBG(DBG_KT) << "time_now            : " << CGAL::to_double(time_now);
                Debug.Assert(time > time_now);
                res = new EdgeCollapseSpec(EdgeCollapseType.FUTURE, time);
            }
            //DBG(DBG_KT) << "returning " << res;
            //DBG_FUNC_END(DBG_KT);
            return res;
        }

#if !SURF_NDEBUG

        private partial void assert_edge_sane(int collapsing_edge)
        {
            throw new NotImplementedException();
            //Debug.Assert(0 <= collapsing_edge && collapsing_edge < 3);
            //Debug.Assert(incident_triangle_);
            //Debug.Assert(incident_triangle_.wavefront(collapsing_edge) == this);
            //Debug.Assert(vertices[0]!=null);
            //Debug.Assert(vertices[1] != null);
        }

#endif

        public partial void set_incident_triangle(KineticTriangle incident_triangle)
        {
            Debug.Assert(!is_dead_);
            Debug.Assert(incident_triangle.has_wavefront(this));
            int idx = incident_triangle.index(this);

            incident_triangle_ = incident_triangle;

            Debug.Assert(incident_triangle.vertex(KineticTriangle.ccw(idx)) == vertices[0]);
            Debug.Assert(incident_triangle.vertex(KineticTriangle.cw(idx)) == vertices[1]);
            invalidate_collapse_spec();
        }

        /** Duplicate this edge in the course of a split event.
         *
         * This is just a simple helper that creates two copies of this edge and marks
         * the original as dead.
         *
         * It is the job of the caller (KineticTriangulation) to then give us new vertices.
         */

        public partial EdgePtrPair split(  List<WavefrontEdge> wavefront_edges)
        {
            Debug.Assert(vertices[0] != null);
            Debug.Assert(vertices[1] != null);
            Debug.Assert(vertices[0].incident_wavefront_edge(1) == this);
            Debug.Assert(vertices[1].incident_wavefront_edge(0) == this);
            set_dead();

            wavefront_edges.Add(new WavefrontEdge(wavefront_edges.Count, vertices[0], null, supporting_line, incident_triangle_, is_beveling));
            var pea = wavefront_edges.Last();
            wavefront_edges.Add(new WavefrontEdge(wavefront_edges.Count, null, vertices[1], supporting_line, incident_triangle_, is_beveling));
            var peb = wavefront_edges.Last();

            return new EdgePtrPair(pea, peb);
        }
    }
}