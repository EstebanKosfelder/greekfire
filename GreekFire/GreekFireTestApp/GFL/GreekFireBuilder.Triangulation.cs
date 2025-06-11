using GFL.Kernel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GFL
{
    using static Debugger.DebugLog;
   public partial class GreekFireBuilder
    {
        private uint event_ctr_ = 0;
        private bool finalized = false;

        private double time = 0.0;
        private double last_event_time = 0.0;
        private double increment = 0.0005;
        private int current_component = -1;
        private int last_event_component = -1;

        int[] event_type_counter = new int[(int)(CollapseType.Never)];

        int max_triangles_per_edge_event = 0;
        int avg_triangles_per_edge_event_sum = 0;
        int avg_triangles_per_edge_event_ctr = 0;

        int max_triangles_per_split_event = 0;
        int avg_triangles_per_split_event_sum = 0;
        int avg_triangles_per_split_event_ctr = 0;


        int events_per_current_event_time = 0;
        int max_events_per_time = 0;
        int avg_events_per_time_sum = 0;
        int avg_events_per_time_ctr = 0;


      
        private List<KineticTriangle> CheckRefinement = new List<KineticTriangle>();
        private List<bool> TidxInCheckRefinement = new List<bool>();

        public  void update_event_timing_stats(double now)
        {
            if (now != this.last_event_time)
            {
                this.max_events_per_time = Math.Max(max_events_per_time, this.events_per_current_event_time);
                this.avg_events_per_time_sum += this.events_per_current_event_time;
                ++this.avg_events_per_time_ctr;

                this.last_event_time = now;
                this.events_per_current_event_time = 1;
            }
            else
            {
                ++this.events_per_current_event_time;
            }
        }

        private static double handle_event_last_time = 0;
        private static int handle_event_count = 0;

        public void HandleEvent(CollapseEvent evnt)
        {
            Log("");
            Log($"{nameof(HandleEvent)}");
            Log($"{evnt}");
            LogIndent();
           
            Debug.Assert(!finalized);

            double time = evnt.Time;

            if (time <= handle_event_last_time)
            {
                ++handle_event_count;
                if (handle_event_count > 10000)
                {
                    throw new Exception("In double loop at line ");
                }
            }
            else
            {
                handle_event_count = 0;
                handle_event_last_time = time;
            };

            Debug.Assert(KineticTriangles[evnt.t.ID] == evnt.t);

            Debug.Assert(CheckRefinement.Count == 0);
            Debug.Assert((evnt == evnt.t.GetCollapse(time)));

            ++event_type_counter[(int)(CollapseType.Undefined)];
            ++event_type_counter[(int)(evnt.Type)];
            update_event_timing_stats(time);

            

            switch (evnt.Type)
            {
                case CollapseType.TriangleCollapse:
                    handle_triangle_collapse_event(evnt);
                    break;

                case CollapseType.ConstraintCollapse:
                    HandleConstraintEvent(evnt);
                    break;

                //case CollapseType.FaceHasInfinitelyFastOpposing:
                //    handle_face_with_infintely_fast_opposing_vertex(evnt);
                //    break;

                //case CollapseType.FaceHasInfinitelyFastVertexWeighted:
                //    handle_face_with_infintely_fast_weighted_vertex(evnt);
                //    break;

                case CollapseType.SpokeCollapse:
                    handle_spoke_collapse_event(evnt);
                    break;

                case CollapseType.SplitOrFlipRefine:
                    HandleSplitOrFlipRefineEvent(evnt);
                    break;

                case CollapseType.VertexMovesOverSpoke:
                    HandleVertexMovesOverSpokeEvent(evnt);
                    break;

                case CollapseType.CcwVertexLeavesCh:
                    handle_ccw_vertex_leaves_ch_event(evnt);
                    break;
                /*
                case CollapseType.GENERIC_FLIP_EVENT:
                  Debug.Assert(false);
                  exit(1);
                  break;
                */
                case CollapseType.InvalidEvent:
                case CollapseType.Undefined:
                case CollapseType.Never:
                    throw new Exception($"Should not get evnt {evnt} to handle.");
                    break;

                default:
                    throw new Exception($"Unexpected evnt {evnt}");
                    break;
            }
            //DBG(//DBG_KT_REFINE) << evnt << " - done.  doing refinement";
            ProcessCheckRefinementQueue(time);



            LogUnindent();
            
        }




        /** Split vertex
         *
         * For creating bevels (such as when dealing with degree-1 vertices),
         * we need to split triangulation vertices.
         *
         * This function splits a vertex v, as given by a triangle t and vertex index.
         * The triangle is duplicated, and the new triangle is ccw of t at v.
         *
         * Returns a pointer to the new KineticTriangle.  The new edges is between
         * vertices 0 and 1.
         */

        private  KineticTriangle split_vertex(
          KineticTriangle t,
          int i,
          WavefrontEdge new_e,
          WavefrontVertex new_v
        )
        {
            //DBG_FUNC_BEGIN(//DBG_KT_SETUP);
            Debug.Assert(new_e != null);
            Debug.Assert(new_v != null);
            Debug.Assert(t != null);

            /* Split the triangle */
            KineticTriangle new_t = new KineticTriangle(KineticTriangles.Count);
            KineticTriangles.Add( new_t);
        
            //DBG(//DBG_KT_SETUP) << " New triangle " << &new_t;

            new_t.wavefronts[0] = null;
            new_t.wavefronts[1] = new_e;
            new_t.wavefronts[2] = t.wavefronts[TriangulationUtils.cw(i)];
            new_t.neighbors[0] = t;
            new_t.neighbors[1] = null;
            new_t.neighbors[2] = t.neighbors[TriangulationUtils.cw(i)];

            Debug.Assert(new_t.vertices[1] == null);
            Debug.Assert(new_t.vertices[2] == null);
            new_t.set_vertex(0, new_v);
            if (t.vertices[TriangulationUtils.ccw(i)] != null) { new_t.set_vertex(1, t.vertices[TriangulationUtils.ccw(i)]); }
            if (t.vertices[i] != null) { new_t.set_vertex(2, t.vertices[i]); }
            //DBG(//DBG_KT_SETUP) << "  setting vertex to " << new_v << " in " << &new_t << "(" << 0 << ")";

            t.wavefronts[TriangulationUtils.cw(i)] = null;
            t.neighbors[TriangulationUtils.cw(i)] = new_t;

            Debug.Assert((new_t.wavefronts[2] == null) != (new_t.neighbors[2] == null));
            if (new_t.wavefronts[2] != null)
            {
                new_t.wavefronts[2].set_incident_triangle(new_t);
                //DBG(//DBG_KT_SETUP) << "  setting incident triangle for wavefronts[2], " << *new_t.wavefronts[2];
            }
            else
            {
                int pos = new_t.neighbors[2].index(t);
                new_t.neighbors[2].neighbors[pos] = new_t;
                //DBG(//DBG_KT_SETUP) << "  setting neighbor triangle for neighbors[2], " << new_t.neighbors[2];
            }

            if (new_e.incident_triangle() == null)
            {
                new_e.set_incident_triangle(new_t);
                //DBG(//DBG_KT_SETUP) << "  setting incident triangle for current_edge, " << *new_e;
            }

            //DBG(//DBG_KT_SETUP) << " Old triangle details: " << t;
            //DBG(//DBG_KT_SETUP) << " New triangle details: " << &new_t;

            //DBG_FUNC_END(//DBG_KT_SETUP);
            return new_t;
        }

        public void assert_valid(int current_component, double time)
        {
            //#if defined (DEBUG_EXPENSIVE_PREDICATES) && DEBUG_EXPENSIVE_PREDICATES >= 1
            //  //DBG_FUNC_BEGIN(//DBG_KT_EVENT2);
            //  assert_valid();
            //  for (var & t: triangles) {
            //    if (restrict_component_ >= 0) {
            //      /* We only do SK in one component */
            //      if (t.component != restrict_component_) continue;
            //    } else {
            //      /* We do SK in all components, but the time in the others will be earlier/later so triangles will not be right */
            //      if (t.component != current_component) continue;
            //    }
            //    if (t.is_dead()) continue;
            //    if (t.unbounded()) {
            //      // recall that unbounded triangles witness that the vertex ccw of the infinite
            //      // vertex is on the boundary of the CH.
            //      //DBG(//DBG_KT_EVENT2) << "t" << &t;
            //      int idx = t.infinite_vertex_idx();
            //      const KineticTriangle* const n = t.neighbor(cw(idx));
            //      int nidx = n.infinite_vertex_idx();

            //      const WavefrontVertex * const u = t.vertex(cw (idx));
            //      const WavefrontVertex * const v = t.vertex(ccw(idx));
            //      const WavefrontVertex * const V = n.vertex(cw (nidx));
            //      const WavefrontVertex * const w = n.vertex(ccw(nidx));
            //      Debug.Assert(v==V);
            //      Debug.Assert(CGAL.orientation(u.p_at(time),
            //                               v.p_at(time),
            //                               w.p_at(time)) != CGAL.RIGHT_TURN);
            //    } else {
            //      //DBG(//DBG_KT_EVENT2) << "t" << &t;
            //      const Vector2D a(t.vertex(0).p_at(time));
            //      const Vector2D b(t.vertex(1).p_at(time));
            //      const Vector2D c(t.vertex(2).p_at(time));
            //      const double det = compute_determinant(
            //          a.x(), a.y(),
            //          b.x(), b.y(),
            //          c.x(), c.y());
            //      assert_sign(det);
            //      if ((CGAL.orientation(a, b, c) == CGAL.RIGHT_TURN) != (det < 0)) {
            //        LOG(ERROR) << "CGAL is confused about orientation of triangle " << &t << ": determinant is " << CGAL.to_double(det) << "  but CGAL thinks orientation is " << CGAL.orientation(a, b, c);
            //        exit(EXIT_CGAL_ORIENTATION_MISMATCH);
            //      }
            //      Debug.Assert(CGAL.orientation(a, b, c) != CGAL.RIGHT_TURN);
            //    }
            //  }
            //  for (var & v: vertices) {
            //    v.assert_valid();
            //  }
            //  //DBG_FUNC_END(//DBG_KT_EVENT2);
            //#else
            //assert_valid();
            //foreach (var v in vertices)
            //{
            //    v.assert_valid();
            //}
            //#endif
        }

        private void move_constraints_to_neighbor(KineticTriangle t, int idx)
        {
            //CGAL_precondition(! (t.is_constrained(cw(idx)) && t.is_constrained(ccw(idx))) );

            for (int i = 1; i <= 2; ++i)
            {
                if (t.is_constrained(TriangulationUtils.mod3(idx + i)))
                {
                    var n = t.neighbor(TriangulationUtils.mod3(idx + 3 - i));
                    int nidx = n.index(t);
                    //  CGAL_assertion(! n.is_constrained(nidx) );

                    n.move_constraint_from(nidx, t, TriangulationUtils.mod3(idx + i));
                }
            }
        }

        private void modified(KineticTriangle t, bool front=false)
        {
            PutOnCheckRefinement(t, front);
            // In the initial refinement calls, we will
            // not have a queue yet.
            if (EventQueue != null)
            {
                EventQueue.NeedsUpdate(t);
            }
            
            //if (t.unbounded())
            //{
            //    /* t's neighbor also witnesses the common vertex remaining in the wavefront.
            //     * let them know things might have changed.
            //     */
            //    int idx = t.infinite_vertex_idx();
            //    KineticTriangle n = t.neighbor(ccw(idx));
            //    Debug.Assert(n != null);
            //    if (!n.is_dying())
            //    {
            //        n.invalidate_collapse_spec();
            //        if (queue != null)
            //        {
            //            queue.needs_update(n);
            //        }
            //    }
            //}
        
        }

        /** actually performs the flip, Debug.Assert the triangulation is consistent before. */

        private  void DoRawFlip(KineticTriangle t, KineticHalfedge he, double time, bool allow_collinear)
        {
            Debug.Assert(t != null);
         

            Debug.Assert(!he.IsConstrain);

            var str = t.ToString();
            //KineticTriangle n =  t.neighbor(edge_idx);
            KineticTriangle n = he.Neighbor;

            if(!(he.Opposite_ is KineticHalfedge hn)){
                throw new NullReferenceException($"{he} oppesite is not {nameof(KineticHalfedge)} ");
            }
            
            //WavefrontVertex v = t.vertex(edge_idx);
            //WavefrontVertex v1 = t.vertex(TriangulationUtils.ccw(edge_idx));
            //WavefrontVertex v2 = t.vertex(TriangulationUtils.cw(edge_idx));

            WavefrontVertex v = he.Prev.Vertex;
            WavefrontVertex v1 = he.Vertex; 
            WavefrontVertex v2 = he.Next.Vertex;

            // WavefrontVertex o = n.vertex(nidx);

            WavefrontVertex o = hn.Prev.Vertex;

            //Debug.Assert(v1 == n.vertex(TriangulationUtils.cw(nidx)));
            //Debug.Assert(v2 == n.vertex(TriangulationUtils.ccw(nidx)));

            Debug.Assert(v1 == hn.Next.Vertex);
            Debug.Assert(v2 == hn.Vertex);



            /* We may also call this for two unbounded triangles.  However, right now we
             * only call this in one very specific way, so v1 is always the infinite and
             * v1 the finite one, and v, v1, and o are collinear on the CH boundary right
             * now.
             */
            //Debug.Assert(!v.is_infinite);
            //Debug.Assert(!v2.is_infinite);

            //if (v1.is_infinite)
            //{
            //    Vector2D pos_v = v.p_at(time);
            //    Vector2D pos_v2 = v2.p_at(time);
            //    Vector2D pos_o = o.p_at(time);
            //    Debug.Assert( MathEx.orientation(pos_v, pos_o, pos_v2) == (int) ETurn.Collinear);
            //}
            //else
            {
                Vector2D pos_v = v.p_at(time);
                Vector2D pos_v1 = v1.p_at(time);
                Vector2D pos_v2 = v2.p_at(time);
                Debug.Assert(MathEx.orientation(pos_v1, pos_v2, pos_v) != (int) ETurn.Right); // v may be on line(v1,v2)

                //if (o.is_infinite)
                //{
                //    /* Flipping to an unbounded triangle. */
                //    // nothing to do here.
                //}
                //else
                {
                    Vector2D pos_o = o.p_at(time);

                   //DBG(//DBG_KT_EVENT2) << " o(v,o,v1):  " << CGAL.orientation(pos_v, pos_o, pos_v1);
                    //DBG(//DBG_KT_EVENT2) << " o(v,o,v2):  " << CGAL.orientation(pos_v, pos_o, pos_v2);
                    //DBG(//DBG_KT_EVENT2) << " o(v1,v2,o):  " << CGAL.orientation(pos_v1, pos_v2, pos_o);

                    if ((allow_collinear || true))
                    {
                        Debug.Assert(MathEx.orientation(pos_v, pos_o, pos_v1) != (int)ETurn.Left);
                        Debug.Assert(MathEx.orientation(pos_v, pos_o, pos_v2) != (int)ETurn.Right);
                    }
                    else
                    {
                        Debug.Assert(MathEx.orientation(pos_v, pos_o, pos_v1) == (int)ETurn.Right);
                        Debug.Assert(MathEx.orientation(pos_v, pos_o, pos_v2) == (int)ETurn.Left);
                    }
                    Debug.Assert(MathEx.orientation(pos_v1, pos_v2, pos_o) != (int)ETurn.Left); // The target triangle may be collinear even before.
                }
            }

            // not strictly necessary for flipping purpuses, but we probably
            // should not end up here if this doesn't hold:
    //        Debug.Assert(!t.is_constrained(TriangulationUtils.cw(edge_idx)) || !t.is_constrained(TriangulationUtils.ccw(edge_idx)) || allow_collinear);
            Debug.Assert(!he.IsConstrain || !he.Next.IsConstrain || allow_collinear);
            
            t.DoRawFlip(he);
        }

        
        ///<summary>
        /// perform a flip, marking t and its neighbor as modified. 
        ///</summary>
        private void DoFlip(KineticTriangle t, KineticHalfedge he,  double time, bool allow_collinear = false)
        {
            Debug.Assert(t != null);
            Debug.Assert(!he.IsConstrain);


            //    KineticTriangle n = t.neighbor(idx);
            KineticTriangle n = he.Neighbor;

            DoRawFlip(t,  he,  time, allow_collinear);
            modified(t, true);
            modified(n);
        }

        /** process a flip evnt, checking all the assertions hold.  Calls do_flip(). */

        private  void DoFlipEvent(double time, KineticTriangle t, KineticHalfedge he)
        {
            //DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            //DBG(//DBG_KT_EVENT) << &t << "; flip edge " << edge_idx;

            if(he == null) throw new ArgumentNullException(nameof(he));

            Debug.Assert(!he.Next.IsConstrain);

            Vector2D[] p =  he.Halfedges.Select(e=>e.Vertex.p_at(time)).ToArray();
         
            double[] squared_lengths = new[]{ (p[1]-p[2]).L2_2D(),
                                              (p[2]-p[0]).L2_2D(),
                                               (p[0]-p[1]).L2_2D() };
            Debug.Assert(squared_lengths[0] > squared_lengths[1]);
            Debug.Assert(squared_lengths[0] > squared_lengths[2]);
            Debug.Assert(squared_lengths[1] != 0.0);
            Debug.Assert(squared_lengths[2] != 0.0);

            DoFlip(t, he, time);
            //DBG_FUNC_END(//DBG_KT_EVENT);
        }



        private void RefineTriangulation(KineticTriangle kt, double time)
        {
            //DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            //DBG(//DBG_KT_EVENT) << t;
            Log("");
            Log($"{nameof(RefineTriangulation)}");
            LogIndent();
            Console.WriteLine($"{kt.ID} Refine Tri");

            Debug.Assert(kt != null);
            kt.assert_valid();

            KineticTriangle t = kt;

            // if (!t.unbounded())
            {
               

                var heReflexs = t.Halfedges.Where(e => e.Vertex.IsReflex).ToArray();
                var num_reflex = heReflexs.Length;
                switch (num_reflex)
                {
                    case 0:
                        break;

                    case 1:
                        {

                            var he = heReflexs[0];


                            //int reflex_idx = (!v0.is_convex_or_straight()) ? 0 :
                            //                 (!v1.is_convex_or_straight()) ? 1 :
                            //                                                  2;


                            Debug.Assert(he.Prev.Vertex.IsConvexOrStraight);
                            Debug.Assert(he.Next.Vertex.IsConvexOrStraight);

                            if ( he.IsConstrain) break;
                            

                        
                            var ho = he.OppositeKineticHalfedge;
                            var n = (KineticTriangle)ho.Triangle;
                            var o = (WavefrontVertex)ho.Prev.Vertex;


                            // If on of the corners of the quadrilateral is actually straight (it won't be reflex), do not flip */
                            

                            //if (t.is_constrained(cw (reflex_idx)) && n.is_constrained(ccw(idx_in_n)) && t.vertex(ccw(reflex_idx)).is_reflex_or_straight()) break;

                            WavefrontVertex v = he.Prev.Vertex;
                            WavefrontVertex va = he.Vertex;
                            WavefrontVertex vb = he.Next.Vertex;
                          
                            for (int i = 0; i <= 1; ++i)
                            {
                                Debug.Assert(v.wavefronts()[i] != null);
                                Debug.Assert(va.wavefronts()[i] != null);
                                Debug.Assert(vb.wavefronts()[i] != null);
                                Debug.Assert(o.wavefronts()[i] != null);

                                Debug.Assert(v.wavefronts()[i].vertex(1 - i) == v);
                                Debug.Assert(va.wavefronts()[i].vertex(1 - i) == va);
                                Debug.Assert(vb.wavefronts()[i].vertex(1 - i) == vb);
                                Debug.Assert(o.wavefronts()[i].vertex(1 - i) == o);
                            }
                            /* If on of the corners of the quadrilateral is actually straight (it won't be reflex), do not flip */
                            if (v.wavefronts()[1].vertex(1) == va && va.IsReflexOrStraight) break;
                            if (v.wavefronts()[0].vertex(0) == vb && vb.IsReflexOrStraight) break;
                            /* Either at v, or at the opposite vertex */
                            Log($"   o.wavefronts()[0].vertex(0) { o.wavefronts()[0].vertex(0)}");
                            Log($"   o.wavefronts()[0].vertex(1) { o.wavefronts()[0].vertex(1)}");
                            Log($"   o.wavefronts()[1].vertex(0) { o.wavefronts()[1].vertex(0)}");
                            Log($"   o.wavefronts()[1].vertex(1) { o.wavefronts()[1].vertex(1)}");
                            if (o.wavefronts()[0].vertex(0) == va && va.IsReflexOrStraight) break;
                            if (o.wavefronts()[1].vertex(1) == vb && vb.IsReflexOrStraight) break;

                            Log($"  Flipping {t.ID} along {he.Next}");
                            Log($"   t:  {t}");
                            Log($"   n:  {n}");
                            Log($"   v:  {v} - {v .p_at(time)}");
                            Log($"   va: {va} - {va.p_at(time)}");
                            Log($"   vb: {vb} - {vb.p_at(time)}");
                            Log($"   o : {o}  - {o .p_at(time)}");

                            DoFlip(kt, he, time);
                            break;
                        };
                    case 2:
                        break;

                    case 3:
                        break;

                        //default:
                        //  Debug.Assert(false);
                }
            }

            LogUnindent();
        }


       

        private  void RefineTriangulationInitial()
        {

             
           TidxInCheckRefinement.AddRange(new bool[KineticTriangles.Count]) ;

            foreach (var t in KineticTriangles)
            {
                // Do _one_ refinement of each triangle.  If it actually does change
                // anything, then refine_triangulation() will have put the triangle
                // onto the check_refinement queue.  If not, no harm done.
                RefineTriangulation(t, 0.0);
            }
           ProcessCheckRefinementQueue(0.0);
        }

        private void ProcessCheckRefinementQueue(double time)
        {
            Log("");
            Log($"{nameof(ProcessCheckRefinementQueue)}");


            while (CheckRefinement.Count > 0)
            {
                KineticTriangle t = CheckRefinementPop();
                RefineTriangulation(t, time);
            }
            Log($"end {nameof(ProcessCheckRefinementQueue)}");

        }

        private KineticTriangle CheckRefinementPop()
        {
            KineticTriangle t = CheckRefinement.First();
            CheckRefinement.RemoveAt(0);
            Debug.Assert(TidxInCheckRefinement[t.ID]);
            TidxInCheckRefinement[t.ID] = false;
            return t;
        }

        private void PutOnCheckRefinement(KineticTriangle t, bool front)
        {
            Debug.Assert(t != null);
            Debug.Assert(TidxInCheckRefinement.Count > t.ID);

            if (TidxInCheckRefinement[t.ID]) return;

            TidxInCheckRefinement[t.ID] = true;
            if (front)
            {
                CheckRefinement.Insert(0, t);
            }
            else
            {
                CheckRefinement.Add(t);
            }
        }

        /** deal with spoke collapse
         *
         * The spoke at edge_idx collapsed, the incident vertices have been stopped
         * already.
         *
         * See if this triangle collapses entirely, or if its 3rd vertex (v) is elsewhere.
         *
         * If the triangle collapses entirely, we may have zero, one, or two constraints.
         * If it does not, we still may have zero or one constraint.
         * (Or two, if there is an infinitely fast vertex as v.)
         */

        private void do_spoke_collapse_part2(KineticHalfedge he, double time)
        {
            //DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            //DBG(//DBG_KT_EVENT) << "t  " << &t;

            // int num_constraints =  t.is_constrained(0).ToInt() + t.is_constrained(1).ToInt() + t.is_constrained(2).ToInt();
            KineticTriangle t= he.Triangle;
            WavefrontVertex v = he.Prev.Vertex;// t.vertices[edge_idx];
            WavefrontVertex va = he.Vertex;// t.vertices[TriangulationUtils.ccw(edge_idx)];
            WavefrontVertex vb = he.Next.Vertex; // t.vertices[TriangulationUtils.cw(edge_idx)];
            Debug.Assert(va.has_stopped());
            Debug.Assert(vb.has_stopped());
            Debug.Assert(va.pos_stop() == vb.pos_stop());
            Debug.Assert(!v.has_stopped());

            throw new NotImplementedException();
            /*

     //TODO       Debug.Assert(t.neighbors[edge_idx] == null);

            var posa = va.pos_stop();

            if (!v.is_infinite && v.p_at(time) == posa)
            {
                t.set_dying();
                v.stop(time);
                // triangle collapses completely 
                //DBG(//DBG_KT_EVENT) << "triangle collapses completely as spoke collapses; spoke is " << edge_idx;
                //LOG(WARNING) << __FILE__ << ":" << __LINE__ << " " << "untested code path.";

                int num_neighbors = 0;
                for (int i = 1; i <= 2; ++i)
                {
                    int edge = TriangulationUtils.mod3(edge_idx + i);
                    //DBG(//DBG_KT_EVENT) << " at edge " << edge;
                    if (t.neighbors[edge] != null)
                    {
                        //DBG(//DBG_KT_EVENT) << "  we have a neighbor";
                        num_neighbors++;

                        KineticTriangle n = t.neighbors[edge];
                        int idx_in_n = n.index(t);

                        t.neighbors[edge] = null;
                        n.neighbors[idx_in_n] = null;

                        do_spoke_collapse_part2(n, idx_in_n, time);
                    }
                    else
                    {
                        //DBG(//DBG_KT_EVENT) << "  we have no neighbor";
                        //LOG(WARNING) << __FILE__ << ":" << __LINE__ << " " << "untested code path: DECL linking.";
                        t.vertices[TriangulationUtils.ccw(edge)].set_next_vertex(1, t.vertices[TriangulationUtils.cw(edge)], false);
                        if (t.wavefront(edge) != null)
                        {
                            t.wavefront(edge).set_dead();
                        }
                    }
                }
                EventQueue.needs_dropping(t);
            }
            else
            {
                throw new NotFiniteNumberException();
                bool call_constraint_collapse = true;
                
                //if (v.infinite_speed != InfiniteSpeedType.NONE)
                //{
                //    int i_1 = EventQueue mod3(edge_idx + 1);
                //    int i_2 = EventQueuemod3(edge_idx + 2);
                //    if (t.neighbors[i_1] == null && t.neighbors[i_2] == null)
                //    {
                //        //DBG(//DBG_KT_EVENT) << "triangle is fully constraint and has an infinitely fast vertex.";

                //        t.set_dying();
                //        v.stop(time, posa);
                //        for (int i = 1; i <= 2; ++i)
                //        {
                //            int edge = TriangulationUtils.mod3(edge_idx + i);
                //            //    LOG(WARNING) << __FILE__ << ":" << __LINE__ << " " << "untested code path: DECL linking.";
                //            t.vertices[TriangulationUtils.ccw(edge)].set_next_vertex(1, t.vertices[TriangulationUtils.cw(edge)], false);
                //            if (t.wavefront(edge) != null)
                //            {
                //                t.wavefront(edge).set_dead();
                //            }
                //        }
                //        call_constraint_collapse = false;
                //        EventQueue.needs_dropping(t);
                //    }
                //}
                
                if (call_constraint_collapse)
                {
                    // triangle does not collapse completely 
                    //DBG(//DBG_KT_EVENT) << "triangle does not collapse completely";
                    do_constraint_collapse_part2(t, edge_idx, time);
                }
            }
            */
            //DBG_FUNC_END(//DBG_KT_EVENT);
        }

        private void handle_spoke_collapse_event(CollapseEvent evnt)
        {
            //DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            //DBG(//DBG_KT_EVENT) << evnt;

            Debug.Assert(evnt.Type == CollapseType.SpokeCollapse);
            KineticTriangle t = KineticTriangles[evnt.t.ID];
            double time = evnt.Time;
          
            var he = evnt.RelevantEdge;
            if (he == null) throw new GFLException($"{evnt} has no relevantEdge ");

            Debug.Assert(!he.IsConstrain);

            WavefrontVertex va = he.Vertex;// t.vertices[TriangulationUtils.ccw(edge_idx)];
            WavefrontVertex vb = he.Next.Vertex;// evnt.RelevantEdge.t.vertices[TriangulationUtils.cw(edge_idx)];
            va.stop(time);
            vb.stop(time);
            Debug.Assert(va.pos_stop() == vb.pos_stop());

            //KineticTriangle n = he.Neighbor;//.neighbors[edge_idx];
            //Debug.Assert(n != null);
        //    int idx_in_n = n.index(t);
            var nhe = he.OppositeKineticHalfedge;


            throw new NotImplementedException();
            //TODO
            //t.neighbors[edge_idx] = null;
            //n.neighbors[idx_in_n] = null;

            do_spoke_collapse_part2( he, time);
            do_spoke_collapse_part2( nhe, time);

            // update prev/next for the DCEL that is the wavefront vertices
            /* actually, nothing to do here, the two do_spoke_collapse_part2 calls did everything. */
            //LOG(WARNING) << __FILE__ << ":" << __LINE__ << " " << "untested code path: DECL linking.";

            assert_valid(t.component, time);

            //DBG_FUNC_END(//DBG_KT_EVENT);
        }

        private  void handle_triangle_collapse_event(CollapseEvent evnt)
        {
            //DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            //DBG(//DBG_KT_EVENT) << evnt;

            Debug.Assert(evnt.Type == CollapseType.TriangleCollapse);
            KineticTriangle t = evnt.t;
            double time = evnt.Time;

           // int num_constraints = t.is_constrained(0).ToInt() + t.is_constrained(1).ToInt() + t.is_constrained(2).ToInt();
            //DBG(//DBG_KT_EVENT) << "have " << num_constraints << " constraints";

            t.set_dying();

            foreach( var v in t.Vertices)
            {
                 v.stop(time);
                //DBG(//DBG_KT_EVENT) << "v[" << i << "]: " << t.vertices[i];
            }
            //TODO
            //for (int i = 1; i < 3; ++i)
            //{
            //    Debug.Assert(t.vertices[0].p_at(time) == t.vertices[i].p_at(time));
            //    Debug.Assert(t.vertices[0].pos_stop() == t.vertices[i].pos_stop());
            //}
            foreach (var he in t.Halfedges)
            {
                if (he.IsConstrain)
                {
                    //TODO
                    //Debug.Assert(he.WavefrontEdge ==  t.vertex(TriangulationUtils.ccw(i)).incident_wavefront_edge(1));
                    //Debug.Assert(t.wavefront(i) == t.vertex(TriangulationUtils.cw(i)).incident_wavefront_edge(0));

                    he.WavefrontEdge.set_dead();

                    // update prev/next for the DCEL that is the wavefront vertices
                    throw new NotImplementedException();
                   //TODO
                   //t.vertices[TriangulationUtils.cw(i)].set_next_vertex(0, t.vertices[TriangulationUtils.ccw(i)], false);
                }
                else
                {
                    // from the other triangle's point of view, a spoke collapsed.  deal with that.
                    var nhe = he.OppositeKineticHalfedge;
                    //KineticTriangle n = t.neighbors[i];
                    //Debug.Assert(n != null);
                    //int idx_in_n = n.index(t);

                    //t.neighbors[i] = null;
                    //n.neighbors[idx_in_n] = null;

                    do_spoke_collapse_part2( nhe, time);
                }
            }
            EventQueue.needs_dropping(t);

            assert_valid(t.component, time);

            //DBG_FUNC_END(//DBG_KT_EVENT);
        }

        /** Handle the 2nd part of a collapse of one constraint from a constraint evnt.
         *
         * After the old kinetic vertices are stopped, this creates a new kv, and updates
         * all incident triangles.
         *
         * We may also call this when a spoke collapses, in which case t.wavefront(edge_idx)
         * is null, but it will also not have a neighbor there anymore.
         */

        //[TODO]
        private void do_constraint_collapse_part2(KineticTriangle t, KineticHalfedge edge, double time)
        {


            //DBG_FUNC_BEGIN(//DBG_KT_EVENT);

            //WavefrontVertex va = t.vertices[ccw(edge_idx)];
            //WavefrontVertex vb = t.vertices[cw(edge_idx)];
            //Debug.Assert(va.has_stopped());
            //Debug.Assert(vb.has_stopped());
            //Debug.Assert(va.pos_stop() == vb.pos_stop());

            //var pos = va.pos_stop();

            //t.set_dying();
            //move_constraints_to_neighbor(t, edge_idx);

            //if (t.wavefront(edge_idx) != null)
            //{
            //    /* constraint collapsed */
            //    Debug.Assert(t.wavefront(edge_idx) == va.incident_wavefront_edge(1));
            //    Debug.Assert(t.wavefront(edge_idx) == vb.incident_wavefront_edge(0));
            //}
            //else
            //{
            //    /* spoke collapsed */
            //    Debug.Assert(t.neighbor(edge_idx) == null);
            //}
            //WavefrontEdge ea = va.incident_wavefront_edge(0);
            //WavefrontEdge eb = vb.incident_wavefront_edge(1);

            //KineticTriangle na = t.neighbors[cw(edge_idx)];
            //KineticTriangle nb = t.neighbors[ccw(edge_idx)];

            //WavefrontVertex v = vertices.make_vertex(pos, time, ea, eb);
            ////DBG(//DBG_KT) << " va: " << va;
            ////DBG(//DBG_KT) << " vb: " << vb;
            //va.set_next_vertex(0, v);
            //vb.set_next_vertex(1, v);

            //{
            //    int affected_triangles = 0;

            //    //DBG(//DBG_KT_EVENT) << "updating vertex in affected triangles";
            //    AroundVertexIterator end = incident_faces_end();
            //    AroundVertexIterator i = incident_faces_iterator(&t, ccw(edge_idx));
            //    //DEBUG_STMT(bool first = true);
            //    //DBG(//DBG_KT_EVENT) << " ccw:";
            //    for (--i; i != end; --i)
            //    {
            //        Debug.Assert(!first || na == &*i); DEBUG_STMT(first = false);
            //        (*i).set_vertex(i.v_in_t_idx(), v);
            //        modified(&*i);
            //        ++affected_triangles;
            //    };

            //    i = incident_faces_iterator(&t, cw(edge_idx));
            //    //DEBUG_STMT(first = true);
            //    //DBG(//DBG_KT_EVENT) << " cw:";
            //    for (++i; i != end; ++i)
            //    {
            //        Debug.Assert(!first || nb == &*i); DEBUG_STMT(first = false);
            //        (*i).set_vertex(i.v_in_t_idx(), v);
            //        modified(&*i);
            //        ++affected_triangles;
            //    }

            //    max_triangles_per_edge_event = Math.Max(max_triangles_per_edge_event, affected_triangles);
            //    avg_triangles_per_edge_event_sum += affected_triangles;
            //    ++avg_triangles_per_edge_event_ctr;
            //};

            ////DBG(//DBG_KT_EVENT) << " cw:";
            //if (na != null)
            //{
            //    var idx = na.index(t);
            //    Debug.Assert(na.index(v) == ccw(idx));
            //    na.neighbors[idx] = nb;
            //}
            //if (nb != null)
            //{
            //    var idx = nb.index(t);
            //    Debug.Assert(nb.index(v) == cw(idx));
            //    nb.neighbors[idx] = na;
            //}

            //if (t.wavefront(edge_idx) != null)
            //{
            //    /* if it was a constraint collapse rather than a spoke collapse */
            //    t.wavefront(edge_idx).set_dead();
            //}
            //queue.needs_dropping(t);

            //DBG_FUNC_END(//DBG_KT_EVENT);
        }

        private void HandleConstraintEvent(CollapseEvent evnt)
        {
            Log("");
            Log($"{MethodBase.GetCurrentMethod()?.Name}");
            LogIndent();


            Debug.Assert(evnt.Type == CollapseType.ConstraintCollapse);
            KineticTriangle t = KineticTriangles[evnt.t.ID];
            double time = evnt.Time;
            var edge = evnt.RelevantEdge;

            Debug.Assert(edge.IsConstrain);
            var a = edge.WavefrontEdge.GetCollapse(t.component, time, edge);
            Debug.Assert(evnt == edge.WavefrontEdge.GetCollapse(t.component, time, edge)  );
            

            WavefrontVertex va = edge.Vertex ;
            WavefrontVertex vb = edge.Next.Vertex;
            WavefrontVertex vc = edge.Prev.Vertex;
            va.stop(time);
            vb.stop(time);
            Log($"va: {va.p_at(time)}");
            Log($"vb: {vb.p_at(time)}");
            Log($"vc: {vc.p_at(time)}");

            Debug.Assert(va.p_at(time).AreNear(vb.p_at(time)));

            // update prev/next for the DCEL that is the wavefront vertices
            va.set_next_vertex(1, vb, false);

            do_constraint_collapse_part2(t, edge, time);

            assert_valid(t.component, time);

            LogUnindent();
        }

        //TODO
        private void handle_split_event(CollapseEvent evnt)
        {
            //DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            //DBG(//DBG_KT_EVENT) << evnt;

            Debug.Assert(evnt.Type == CollapseType.SplitOrFlipRefine);
            KineticTriangle  t = KineticTriangles[evnt.t.ID];
            double time= evnt.Time;
            var edge = evnt.RelevantEdge;
            //DBG(//DBG_KT_EVENT2) << " t:  " << &t;
            WavefrontVertex v = edge.Vertex;

            WavefrontEdge e = edge.WavefrontEdge;
            WavefrontEdge eb = v.incident_wavefront_edge(0);
            WavefrontEdge ea = v.incident_wavefront_edge(1);
            Debug.Assert(ea.vertex(0) == v);
            Debug.Assert(eb.vertex(1) == v);

            //TODO
            Debug.Assert(edge.Prev.Opposite_.Face is KineticTriangle);
            Debug.Assert(edge.Next.Opposite_.Face is KineticTriangle);

            KineticTriangle na = edge.Opposite_.Prev_.Face as KineticTriangle;
            KineticTriangle nb = edge.Opposite_.Next_.Face as KineticTriangle;

            { /* So, we have a wavefront edge e and we have an opposite reflex vertex v.
               * Assert that the vertex can actually hit the edge, i.e., check if the
               * wavefront edges e0 and e1 incident at v point away from e when
               * starting at v.
               *
               * The edges we store with v are directed such that e0 points towards v
               * and e1 away from it.
               *
               * The orientation needed for an evnt thus is that e and e0 form a right
               * turn, and e and e1 form a left turn.
               *
               * If either are collinear, things can still happen.
               */

                var e0 = eb.SupportLine.l.to_vector();
                var e1 = ea.SupportLine.l.to_vector(); 
                int o0 = (MathEx.orientation(e.SupportLine.l.to_vector(), e0));
                int o1 = (MathEx.orientation(e.SupportLine.l.to_vector(), e1));

                Debug.Assert(o0 != (int)ETurn.Left);
                Debug.Assert(o1 != (int)ETurn.Right);
            }

            Vector2D pos = v.pos_stop();
            // Stop the vertex
            v.stop(time);

            // Split edge e into parts,
            var new_edges = e.split( this.WavefrontEdges);
            WavefrontEdge nea = new_edges.Left;
            WavefrontEdge neb = new_edges.Right;

            // Create new wavefront vertices with the new edges
            WavefrontVertex nva = new WavefrontVertex(this.WavefrontVertices.Count, pos, time, nea, ea, true);
            this.WavefrontVertices.Add(nva);
            WavefrontVertex nvb = new WavefrontVertex(this.WavefrontVertices.Count, pos, time, eb, neb, true);
            this.WavefrontVertices.Add(nvb);
            // And set these new vertices on the edges.
            nea.vertex(0).set_incident_wavefront_edge(1, nea);
            nea.set_wavefrontedge_vertex(1, nva);
            neb.set_wavefrontedge_vertex(0, nvb);
            neb.vertex(1).set_incident_wavefront_edge(0, neb);

            // And update prev/next for the DCEL that is the wavefront vertices
            v.set_next_vertex(0, nvb);
            v.set_next_vertex(1, nva);
            nva.link_tail_to_tail(nvb);

            throw new NotImplementedException();
            //t.set_dying();
            //{
            //    int affected_triangles = 0;

            //    AroundVertexIterator end = incident_faces_end();
            //    AroundVertexIterator i = incident_faces_iterator(t, edge_idx);
            //    KineticTriangle lasta = null;
            //    //DBG(//DBG_KT_EVENT2) << " split: updating vertex on a side:";
            //    for (++i; i != end; ++i)
            //    {
            //        //DBG(//DBG_KT_EVENT2) << " split:   updating vertex on a side in " << &*i;
            //        (*i).set_vertex(i.v_in_t_idx(), nva);
            //        modified(&*i);
            //        lasta = &*i;
            //        ++affected_triangles;
            //    };
            //    Debug.Assert(lasta);
            //    Debug.Assert(lasta.wavefront(cw(lasta.index(nva))));
            //    Debug.Assert(lasta.wavefront(cw(lasta.index(nva))).vertex(0) == nva);

            //    i = incident_faces_iterator(&t, edge_idx);
            //    KineticTriangle* lastb = null;
            //    //DBG(//DBG_KT_EVENT2) << " split: updating vertex on b side: ";
            //    for (--i; i != end; --i)
            //    {
            //        //DBG(//DBG_KT_EVENT2) << " split:   updating vertex on b side in " << &*i;
            //        (*i).set_vertex(i.v_in_t_idx(), nvb);
            //        modified(&*i);
            //        lastb = &*i;
            //        ++affected_triangles;
            //    }

            //    max_triangles_per_split_event = (std.max)(max_triangles_per_split_event, affected_triangles);
            //    avg_triangles_per_split_event_sum += affected_triangles;
            //    ++avg_triangles_per_split_event_ctr;

            //    Debug.Assert(lastb);
            //    Debug.Assert(lastb.wavefront(ccw(lastb.index(nvb))));
            //    Debug.Assert(lastb.wavefront(ccw(lastb.index(nvb))).vertex(1) == nvb);

            //    Debug.Assert(na.index(nva) == cw(na.index(&t)));
            //    na.set_wavefront(na.index(&t), nea);

            //    Debug.Assert(nb.index(nvb) == ccw(nb.index(&t)));
            //    nb.set_wavefront(nb.index(&t), neb);

            //    //DBG(//DBG_KT_EVENT2) << " nea:" << *nea;
            //    //DBG(//DBG_KT_EVENT2) << " neb:" << *neb;
            //    //DBG(//DBG_KT_EVENT2) << " ea: " << *ea;
            //    //DBG(//DBG_KT_EVENT2) << " eb: " << *eb;
            //    //DBG(//DBG_KT_EVENT2) << " na: " << na;
            //    //DBG(//DBG_KT_EVENT2) << " nb: " << nb;

            //    na.assert_valid();
            //    nb.assert_valid();
            //    lasta.assert_valid();
            //    lastb.assert_valid();
            //}

            //queue.needs_dropping(&t);
            assert_valid(t.component, time);

            //DBG_FUNC_END(//DBG_KT_EVENT);
        }

        /* handle a split evnt, or refine this as a flip evnt.
         *
         * A triangle with exactly one constraint, e, has the opposite vertex v moving
         * onto the supporting line of e.
         *
         * This can be a split evnt, in which case we handle it right here,
         * or it can be a flip evnt, in which case we pump it down the line
         * so we can first deal with other, real split events or higher-priority flip
         * events.
         */

        private void HandleSplitOrFlipRefineEvent(CollapseEvent evnt)
        {

            LogIndent();
            Log($"{MethodBase.GetCurrentMethod()?.Name}");

            Debug.Assert(evnt.Type == CollapseType.SplitOrFlipRefine);

            KineticTriangle t = KineticTriangles[evnt.t.ID];
            double time = (evnt.Time);
            
            var edge = evnt.RelevantEdge;


            Debug.Assert(edge.IsConstrain);
            Debug.Assert(!edge.Next.IsConstrain);
            Debug.Assert(!edge.Prev.IsConstrain);

            var wavefrontEdge = edge.WavefrontEdge;
            edge = edge.Prev; 
            Log($"t :{t}");
            Log($"to:{edge.Next.OppositeKineticHalfedge.Triangle}");



            WavefrontVertex v =  edge.Vertex;
            WavefrontVertex va = edge.Next.Vertex;
            WavefrontVertex vb = edge.Prev.Vertex;  
            Debug.Assert(!v.has_stopped());

            var posa = va.p_at(time);
            var posb = vb.p_at(time);
            var pos = v.p_at(time);
            var s = new Line2D(posa, posb);
            //var isCollineal = s.has_on(pos);
            //Debug.Assert(isCollineal);

            /* 3 cases:
             * (1) the vertex v is outside of the segment s, on the supporting line,
             * (2) it's at one of the endpoints of s,
             * (1) or v is on the interior of the edge.
             */

            double sq_length_constraint = (posa-posb).L2_2D();
            double sq_length_v_to_va = (pos - posa).L2_2D();
            double sq_length_v_to_vb = (pos - posb).L2_2D(); 
            double longest_spoke;
            Log($"v.pos:  {v} ");
            Log($"va.pos: {posa}");
            Log($"vb.pos: {posb}");
            Log($"sqlength s   : {sq_length_constraint}");
            Log($"sqlength v-va: {sq_length_v_to_va}");
            Log($"sqlength v-vb: {sq_length_v_to_vb}");
            if (!s.has_on(pos))
            {

                // case 1
                Log($"CASE 1:pos is not colineal posa-posb ");
                Log("A potential split evnt is actually a flip evnt.  Maybe refinement should have prevented that?");
                Log("Re-classifying as flip evnt as v is not on the constrained segment.");

                Debug.Assert(KineticTriangle.edge_is_faster_than_vertex(edge.Vertex, wavefrontEdge.SupportLine) !=(int) ESign.Negative);

                /** there are basically two types of flip events involving constrained triangles.
                 * One is where the vertex is coming towards the supporting line of the constraint
                 * edge e and is passing left or right of of e.  The other is where (the
                 * supporting line of an edge e overtakes a vertex.
                 *
                 * They are generally handled identically, however the assertions are slightly different
                 * ones as these cases differ in which of v's incident edges is relevant.
                 */
                var e = (wavefrontEdge.SupportLine.l.to_vector());
                var e0 = (edge.Vertex.incident_wavefront_edge(0).SupportLine.l.to_vector());
                var e1 = (edge.Vertex.incident_wavefront_edge(1).SupportLine.l.to_vector());
                int o0 = (MathEx.orientation(e, e0));
                int o1 = (MathEx.orientation(e, e1));
     
                KineticHalfedge flipEdge;

                /* Figure out which side of the constraint edge we're on. */
                if (sq_length_v_to_va > sq_length_v_to_vb)
                {
                    //DBG(//DBG_KT_EVENT) << "(v, va) is the longest spoke, so vb moves over that.";
                    longest_spoke = sq_length_v_to_va;

                    Debug.Assert(sq_length_v_to_va > sq_length_constraint); /* Check that v to va is the longest spoke */
                    Debug.Assert(vb ==  edge.Prev.Vertex);
                    Debug.Assert(vb.IsReflexOrStraight);

                    /* If we come from behind, we don't really care about the first of these things in the discunjtion,
                     * if we face it head on, we don't care about the second.  hmm. */
                    Debug.Assert(o1 != (int)ETurn.Right || (v.IsReflexOrStraight && o0 != (int)ETurn.Right));

                    //TODO  ckeck
                    flipEdge = evnt.RelevantEdge.Prev;
                }
                else /* (pos, posa) < (pos, posb) */
                {
                    //DBG(//DBG_KT_EVENT) << "(v, vb) is the longest spoke, so va moves over that.";
                    longest_spoke = sq_length_v_to_vb;
                    Debug.Assert(sq_length_v_to_va != sq_length_v_to_vb); /* they really shouldn't be able to be equal. */

                    Debug.Assert(sq_length_v_to_vb > sq_length_constraint); /* Check that v to vb is the longest spoke */
                    Debug.Assert(va == edge.Next.Vertex);
                    Debug.Assert(va.IsReflexOrStraight);

                    /* If we come from behind, we don't really care about the first of these things in the discunjtion,
                     * if we face it head on, we don't care about the second.  hmm. */
                    Debug.Assert(o0 != (int)ETurn.Left|| (v.IsReflexOrStraight && o1 != (int)ETurn.Left));
                    //TODO  ckeck
                 
                    flipEdge = evnt.RelevantEdge.Next;
                };
                CollapseSpec c = t.RefineCollapseSpec(new CollapseSpec(t.component, CollapseType.VertexMovesOverSpoke, time, flipEdge.Prev, longest_spoke));
                Log($"Refining to {c}");
                EventQueue.NeedsUpdate(t, true);
            }
            else if (pos == posa)
            {
                Log($"CASE 2: pos == posa");
                CollapseSpec c = t.RefineCollapseSpec(new CollapseSpec(t.component, CollapseType.SpokeCollapse, time, edge.Prev));
                Log($"v is incident to va Refining to {c}");
                EventQueue.NeedsUpdate(t, true);
            }
            else if (pos == posb)
            {
                Log($"CASE 2: pos == posb");
                CollapseSpec c = t.RefineCollapseSpec(new CollapseSpec(t.component, CollapseType.SpokeCollapse, time, edge.Next));
                Log($"v is incident to vb Refining to {c}");
                EventQueue.NeedsUpdate(t, true);
            }
            else
            {
                Log($"CASE3 We have a real split evnt.");
                handle_split_event(evnt);
            }

            LogUnindent();
        }

        private void HandleVertexMovesOverSpokeEvent(CollapseEvent evnt)
        {

            LogIndent();
            Log($"{MethodBase.GetCurrentMethod()?.Name}");

         
            Debug.Assert(evnt.Type == CollapseType.VertexMovesOverSpoke);
            KineticTriangle t = evnt.t;

            DoFlipEvent(evnt.Time, t, evnt.RelevantEdge);

            LogUnindent();
        }

        private void handle_ccw_vertex_leaves_ch_event(CollapseEvent evnt)
        {

            LogIndent();
            Log($"{MethodBase.GetCurrentMethod()?.Name}");



            //DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            //DBG(//DBG_KT_EVENT) << evnt;
            throw new NotImplementedException();
            //Debug.Assert(evnt.type() == CollapseType.CcwVertexLeavesCh);
            //KineticTriangle t = triangles[evnt.t.ID];
            //int idx = evnt.relevant_edge(); /* finite edge idx == infinite vertex idx */

            //Debug.Assert((int)idx == t.infinite_vertex_idx());
            //do_flip(t, TriangulationUtils.cw(idx), evnt.time());

            //DBG_FUNC_END(//DBG_KT_EVENT);

            LogUnindent();
        }




        private void handle_face_with_infintely_fast_opposing_vertex(CollapseEvent evnt)
        {
            throw new NotFiniteNumberException();

            ////DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            ////DBG(//DBG_KT_EVENT) << evnt;

            //Debug.Assert(evnt.type() == CollapseType.FaceHasInfinitelyFastVertexWeighted);
            //KineticTriangle tref = triangles[evnt.t.id];
            //KineticTriangle t = tref;
            //double time = evnt.time();

            ////DBG(//DBG_KT_EVENT2) << "t:  " << t;

            //int num_constraints = t.is_constrained(0).ToInt() + t.is_constrained(1).ToInt() + t.is_constrained(2).ToInt();
            //int num_fast = (t.vertex(0).infinite_speed != InfiniteSpeedType.NONE).ToInt() +
            //                    (t.vertex(1).infinite_speed != InfiniteSpeedType.NONE).ToInt() +
            //                    (t.vertex(2).infinite_speed != InfiniteSpeedType.NONE).ToInt();
            //Debug.Assert(num_fast >= 1);
            //Debug.Assert(num_fast < 3);
            //if (num_constraints == 3)
            //{
            //    //DBG(//DBG_KT_EVENT2) << "infinitely fast triangle with 3 constraints.";
            //    t.set_dying();

            //    Vector2D p = Vector2D.NaN;
            //    bool first = true;
            //    for (int i = 0; i < 3; ++i)
            //    {
            //        if (t.vertex(i).infinite_speed != InfiniteSpeedType.NONE) continue;
            //        t.vertices[i].stop(time);
            //        if (first)
            //        {
            //            p = t.vertices[i].pos_stop();
            //            first = false;
            //        }
            //        else
            //        {
            //            Debug.Assert(p == t.vertices[i].pos_stop());
            //        }
            //    }
            //    Debug.Assert(!first);
            //    for (int i = 0; i < 3; ++i)
            //    {
            //        if (t.vertex(i).infinite_speed == InfiniteSpeedType.NONE) continue;
            //        t.vertices[i].stop(time, p);
            //    }

            //    for (int i = 0; i < 3; ++i)
            //    {
            //        t.wavefront(i).set_dead();
            //    }

            //    // update prev/next for the DCEL that is the wavefront vertices
            //    for (int i = 0; i < 3; ++i)
            //    {
            //        t.vertices[i].set_next_vertex(0, t.vertices[cw(i)], false);
            //    }
            //    //LOG(WARNING) << __FILE__ << ":" << __LINE__ << " " << "untested code path: DECL linking.";

            //    queue.needs_dropping(t);
            //}
            //else
            //{
            //    //DBG(//DBG_KT_EVENT2) << "infinitely fast triangle with fewer than 3 constraints.";
            //    Debug.Assert(num_fast <= 2);
            //    int t_fast_idx = t.infinite_speed_opposing_vertex_idx();
            //    WavefrontVertex v = t.vertices[t_fast_idx];
            //    //DBG(//DBG_KT_EVENT2) << "infinitely fast vertex: " << t_fast_idx;

            //    AroundVertexIterator faces_it = incident_faces_iterator(t, t_fast_idx);
            //    AroundVertexIterator most_cw_triangle = faces_it.most_cw();

            //    //DBG(//DBG_KT_EVENT2) << "cw most triangle: " << most_cw_triangle;

            //    /* Flip away any spoke at the infinitely fast vertex. */
            //    /* ================================================== */
            //    //DBG(//DBG_KT_EVENT2) << "flipping all spokes away from " << most_cw_triangle.t().vertex( most_cw_triangle.v_in_t_idx() );
            //    List<KineticTriangle> mark_modified = new List<KineticTriangle>();
            //    while (true)
            //    {
            //        int nidx = ccw(most_cw_triangle.v_in_t_idx());
            //        if (most_cw_triangle.t().is_constrained(nidx))
            //        {
            //            break;
            //        }
            //        KineticTriangle n = most_cw_triangle.t().neighbor(nidx);
            //        //DBG(//DBG_KT_EVENT2) << "- flipping: " << most_cw_triangle.t();
            //        //DBG(//DBG_KT_EVENT2) << "  towards:  " << n;
            //        do_raw_flip(most_cw_triangle.t(), nidx, time, true);
            //        mark_modified.Add(n);
            //    }
            //    //DBG(//DBG_KT_EVENT2) << "flipping done; " << most_cw_triangle;

            //    t = most_cw_triangle.t();
            //    //DBG(//DBG_KT_EVENT2) << "most cw triangle is: " << t;
            //    int vidx_in_tc = most_cw_triangle.v_in_t_idx();

            //    Debug.Assert(t.vertex(vidx_in_tc) == v);
            //    Debug.Assert(t.is_constrained(cw(vidx_in_tc)));
            //    Debug.Assert(t.is_constrained(ccw(vidx_in_tc)));

            //    /* Figure out which edge to retire */
            //    /* =============================== */
            //    WavefrontVertex v_cw = t.vertices[cw(vidx_in_tc)];
            //    WavefrontVertex v_ccw = t.vertices[ccw(vidx_in_tc)];

            //    WavefrontEdge l = t.wavefront(ccw(vidx_in_tc));
            //    WavefrontEdge r = t.wavefront(cw(vidx_in_tc));

            //    //DBG(//DBG_KT_EVENT) << "l is " << *l;
            //    //DBG(//DBG_KT_EVENT) << "r is " << *r;

            //    //DBG(//DBG_KT_EVENT) << "v     is " << v;
            //    //DBG(//DBG_KT_EVENT) << "v_cw  is " << v_cw;
            //    //DBG(//DBG_KT_EVENT) << "v_ccw is " << v_ccw;

            //    Debug.Assert((v_cw.infinite_speed == InfiniteSpeedType.NONE) ||
            //           (v_ccw.infinite_speed == InfiniteSpeedType.NONE));

            //    int collapse = 0;
            //    WavefrontVertex o = null;
            //    bool spoke_collapse = false;
            //    /* collapse the shorter edge, or the edge to the non-fast vertex (i.e, the one opposite of the fast). */
            //    if (v_cw.infinite_speed != InfiniteSpeedType.NONE)
            //    {
            //        collapse = cw(vidx_in_tc);
            //        o = v_ccw;
            //        //DBG(//DBG_KT_EVENT) << "v_cw has infinite speed";
            //    }
            //    else if (v_ccw.infinite_speed != InfiniteSpeedType.NONE)
            //    {
            //        collapse = ccw(vidx_in_tc);
            //        o = v_cw;
            //        //DBG(//DBG_KT_EVENT) << "v_ccw has infinite speed";
            //    }
            //    else
            //    {
            //        var pos = v.p_at(time);
            //        var poscw = v_cw.p_at(time);
            //        var posccw = v_ccw.p_at(time);
            //        double sq_length_v_to_vcw = squared_distance(pos, poscw);
            //        double sq_length_v_to_vccw = squared_distance(pos, posccw);
            //        if (sq_length_v_to_vcw < sq_length_v_to_vccw)
            //        {
            //            collapse = ccw(vidx_in_tc);
            //            o = v_cw;
            //            //DBG(//DBG_KT_EVENT) << "sq_length_v_to_vcw < sq_length_v_to_vccw";
            //        }
            //        else if (sq_length_v_to_vcw > sq_length_v_to_vccw)
            //        {
            //            collapse = cw(vidx_in_tc);
            //            o = v_ccw;
            //            //DBG(//DBG_KT_EVENT) << "sq_length_v_to_vcw > sq_length_v_to_vccw";
            //        }
            //        else
            //        {
            //            //DBG(//DBG_KT_EVENT) << "sq_length_v_to_vcw == sq_length_v_to_vccw";
            //            spoke_collapse = true;
            //        }
            //    }
            //    if (spoke_collapse)
            //    {
            //        //DBG(//DBG_KT_EVENT) << "both edges incident to the infinitely fast vertex have the same length";
            //        t.set_dying();

            //        v_cw.stop(time);
            //        v_ccw.stop(time);
            //        Debug.Assert(v_cw.pos_stop() == v_ccw.pos_stop());

            //        v.stop(time, v_cw.pos_stop());
            //        Debug.Assert(t.wavefront(cw(vidx_in_tc)) != null);
            //        Debug.Assert(t.wavefront(ccw(vidx_in_tc)) != null);
            //        t.wavefront(cw(vidx_in_tc)).set_dead();
            //        t.wavefront(ccw(vidx_in_tc)).set_dead();

            //        KineticTriangle n = t.neighbors[vidx_in_tc];
            //        Debug.Assert(n != null);
            //        int idx_in_n = n.index(t);

            //        t.neighbors[vidx_in_tc] = null;
            //        n.neighbors[idx_in_n] = null;

            //        // update prev/next for the DCEL that is the wavefront vertices
            //        v.set_next_vertex(0, v_cw, false);
            //        v.set_next_vertex(1, v_ccw, false);

            //        Debug.Assert(!t.is_dead());
            //        do_spoke_collapse_part2(n, idx_in_n, time);

            //        Debug.Assert(!t.is_dead());
            //        queue.needs_dropping(t);
            //    }
            //    else
            //    {
            //        //DBG(//DBG_KT_EVENT) << "collapse " << collapse;
            //        //DBG(//DBG_KT_EVENT) << "v " << v;
            //        //DBG(//DBG_KT_EVENT) << "o " << o;
            //        Debug.Assert((t.index(v) + t.index(o) + collapse) == 3);

            //        o.stop(time);
            //        v.stop(time, o.pos_stop());

            //        // update prev/next for the DCEL that is the wavefront vertices
            //        Debug.Assert(o == v_ccw || o == v_cw);
            //        v.set_next_vertex(o == v_ccw ? 1 : 0, o, false);
            //        //LOG(WARNING) << __FILE__ << ":" << __LINE__ << " " << "untested code path: DECL linking.";

            //        do_constraint_collapse_part2(t, collapse, time);
            //    }
            //    foreach (KineticTriangle tm in mark_modified)
            //    {
            //        if (!tm.is_dead())
            //        {
            //            modified(tm);
            //        }
            //        else
            //        {
            //            //DBG(//DBG_KT_EVENT) << "Not marking " << tm << " as modified because it is dead already.";
            //            Debug.Assert(spoke_collapse);
            //        }
            //    }
            //}
            //assert_valid(t.component, time);

            //DBG_FUNC_END(//DBG_KT_EVENT);
        }

        // TODO
        private void handle_face_with_infintely_fast_weighted_vertex(CollapseEvent evnt)
        {
            throw new NotFiniteNumberException();
            ////DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            ////DBG(//DBG_KT_EVENT) << evnt;

            //Debug.Assert(evnt.type() == CollapseType.FACE_HAS_INFINITELY_FAST_VERTEX_WEIGHTED);
            //KineticTriangle tref = triangles[evnt.t.id];
            //KineticTriangle t = tref;
            //double time(evnt.time());
            //int edge_idx = evnt.relevant_edge();

            ////DBG(//DBG_KT_EVENT2) << "t:  " << t;

            //Debug.Assert((t.vertex(0).infinite_speed == InfiniteSpeedType.WEIGHTED) ||
            //       (t.vertex(1).infinite_speed == InfiniteSpeedType.WEIGHTED) ||
            //       (t.vertex(2).infinite_speed == InfiniteSpeedType.WEIGHTED));

            ///* Find vertex with the fastest incident edge,
            // * This will then gobble up one incident slower edge. */
            //Debug.Assert(t.wavefront(edge_idx));
            //Debug.Assert(t.wavefronts[edge_idx].vertex(0) == t.vertices[ccw(edge_idx)]);
            //Debug.Assert(t.wavefronts[edge_idx].vertex(1) == t.vertices[cw (edge_idx)]);

            //Debug.Assert((t.vertices[ccw(edge_idx)].infinite_speed == InfiniteSpeedType.WEIGHTED) ||
            //       (t.vertices[cw (edge_idx)].infinite_speed == InfiniteSpeedType.WEIGHTED));

            //WavefrontEdge winning_edge = t.wavefront(edge_idx);

            //WavefrontVertex v_fast;
            //KineticTriangle most_cw_triangle;
            //int idx_fast_in_most_cw_triangle;
            //int winning_edge_idx_in_v;
            ////DBG(//DBG_KT_EVENT2) << " edge_idx:  " << edge_idx;
            ////DBG(//DBG_KT_EVENT2) << " t.vertices[    edge_idx ]:  " << t.vertices[    edge_idx ];
            ////DBG(//DBG_KT_EVENT2) << " t.vertices[ccw(edge_idx)]:  " << t.vertices[ccw(edge_idx)];
            ////DBG(//DBG_KT_EVENT2) << " t.vertices[cw (edge_idx)]:  " << t.vertices[cw (edge_idx)];

            ///* If both vertices are of type InfiniteSpeedType.WEIGHTED, pick one. */
            //if (t.vertices[ccw(edge_idx)].infinite_speed == InfiniteSpeedType.WEIGHTED) {
            //  /* The left vertex of the edge is the one in question. */
            //  v_fast = t.vertices[ccw(edge_idx)];
            //  winning_edge_idx_in_v = 1;
            //  most_cw_triangle = t;
            //  idx_fast_in_most_cw_triangle = ccw(edge_idx);
            //} else {
            //  /* The right vertex of the edge is the one in question,
            //   * find the triangle that is incident to the other edge */
            //  v_fast = t.vertices[cw(edge_idx)];
            //  winning_edge_idx_in_v = 0;
            //  Debug.Assert(t.wavefronts[edge_idx].vertex(1).wavefronts()[0] == t.wavefront(edge_idx));

            //  most_cw_triangle = t.wavefronts[edge_idx].vertex(1).wavefronts()[1].incident_triangle();
            //  idx_fast_in_most_cw_triangle = most_cw_triangle.index(v_fast);
            //};

            //if (t.vertices[edge_idx].is_infinite) {
            //  //DBG(//DBG_KT_EVENT2) << "Unbounded triangle";
            //  int nidx = ccw(idx_fast_in_most_cw_triangle);
            //  KineticTriangle *n = most_cw_triangle.neighbor( nidx);
            //  Debug.Assert(n.unbounded());
            //  int idx_in_n = n.index(most_cw_triangle);
            //  Debug.Assert(n.vertices[cw(idx_in_n)].is_infinite);

            //  //DBG(//DBG_KT_EVENT2) << "- flipping: " << most_cw_triangle;
            //  //DBG(//DBG_KT_EVENT2) << "  towards:  " << n;

            //  do_raw_flip(most_cw_triangle, nidx, time, true);
            //  modified(n);
            //} else {
            //  //DBG(//DBG_KT_EVENT2) << "Bounded triangle";

            //  /* The triangle should not have collapsed yet. */
            //  /*
            //   * Actually it may have
            //  {
            //    //DBG(//DBG_KT_EVENT2) << "most_cw_triangle is " << most_cw_triangle;
            //    const Vector2D pos_v0 = most_cw_triangle.vertex(0).p_at(time);
            //    const Vector2D pos_v1 = most_cw_triangle.vertex(1).p_at(time);
            //    const Vector2D pos_v2 = most_cw_triangle.vertex(2).p_at(time);
            //    Debug.Assert(CGAL.orientation(pos_v0, pos_v1, pos_v2) == CGAL.LEFT_TURN);
            //  }
            //  */

            //  /* Flip away any spoke at the infinitely fast vertex. */
            //  //DBG(//DBG_KT_EVENT2) << "flipping all spokes away from " << v_fast;
            //  AroundVertexIterator flipping_triangle = incident_faces_iterator(most_cw_triangle, idx_fast_in_most_cw_triangle);
            //  int nidx_in_most_cw_triangle = ccw(idx_fast_in_most_cw_triangle);
            //  while (true) {
            //    Debug.Assert(most_cw_triangle.vertices[idx_fast_in_most_cw_triangle] == v_fast);
            //    if (most_cw_triangle.is_constrained(nidx_in_most_cw_triangle)) {
            //      break;
            //    }

            //    Vector2D pos_v0 = flipping_triangle.t().vertex( ccw(flipping_triangle.v_in_t_idx()) ).p_at(time);
            //    Vector2D pos_v1 = flipping_triangle.t().vertex( cw (flipping_triangle.v_in_t_idx()) ).p_at(time);
            //    if (flipping_triangle.t() != most_cw_triangle) {
            //      /* We already delayed flipping at least once.  Let's see if we can go back */
            //      int nidx = cw(flipping_triangle.v_in_t_idx()); /* previous guy, the one cw */
            //      KineticTriangle n = flipping_triangle.t().neighbor( nidx );
            //      int idx_in_n = n.index(flipping_triangle.t());
            //      Debug.Assert(n.vertex( ccw (idx_in_n) ) == flipping_triangle.t().vertex( ccw (flipping_triangle.v_in_t_idx()) ) );
            //      Vector2D pos_v2 = n.vertex(idx_in_n).p_at(time);

            //      if (CGAL.orientation(pos_v2, pos_v0, pos_v1) != CGAL.RIGHT_TURN) {
            //        //DBG(//DBG_KT_EVENT2) << "- We can go back, and do a flip: " << flipping_triangle.t();
            //        ++flipping_triangle;
            //        continue; /* We will flip in the next iteration. */
            //      } else {
            //        //DBG(//DBG_KT_EVENT2) << "- We cannot go back just yet";
            //      }
            //    };

            //    /* Check if we can flip to the neighbor ccw. */
            //    int nidx = ccw(flipping_triangle.v_in_t_idx()); /* next guy, the one ccw */
            //    KineticTriangle *n = flipping_triangle.t().neighbor( nidx );
            //    int idx_in_n = n.index(flipping_triangle.t());

            //    Debug.Assert(n.vertex( cw (idx_in_n) ) == flipping_triangle.t().vertex( cw (flipping_triangle.v_in_t_idx()) ) );
            //    const Vector2D pos_v2 = n.vertex(idx_in_n).p_at(time);
            //    if (CGAL.orientation(pos_v0, pos_v1, pos_v2) == CGAL.RIGHT_TURN) {
            //      /* No, not right now.  Try in the next ccw triangle. */
            //      //DBG(//DBG_KT_EVENT2) << "- not flipping right now: " << flipping_triangle.t();
            //      --flipping_triangle;
            //    } else {
            //      //DBG(//DBG_KT_EVENT2) << "- flipping: " << flipping_triangle.t();
            //      //DBG(//DBG_KT_EVENT2) << "  towards:  " << n;
            //      do_raw_flip(flipping_triangle.t(), nidx, time, true);
            //      modified(n);
            //    }
            //  }
            //  //DBG(//DBG_KT_EVENT2) << "flipping done; " << most_cw_triangle;
            //}

            //Debug.Assert(winning_edge == v_fast.wavefronts()[winning_edge_idx_in_v]);
            //const WavefrontEdge * const losing_edge = v_fast.wavefronts()[1-winning_edge_idx_in_v];
            //Debug.Assert(v_fast == losing_edge.vertex(winning_edge_idx_in_v));
            //WavefrontVertex* o = losing_edge.vertex(1-winning_edge_idx_in_v);

            ////DBG(//DBG_KT_EVENT) << "v_fast " << v_fast;
            ////DBG(//DBG_KT_EVENT) << "o      " << o;
            ////DBG(//DBG_KT_EVENT) << "most_cw_triangle:  " << most_cw_triangle;
            ////DBG(//DBG_KT_EVENT) << "winning edge at v: " << winning_edge_idx_in_v;

            //if (o.infinite_speed != InfiniteSpeedType.NONE) {
            //  Debug.Assert(o.infinite_speed == InfiniteSpeedType.WEIGHTED);
            //  o.stop(time, o.pos_start);
            //} else {
            //  o.stop(time);
            //}
            //v_fast.stop(time, o.pos_stop());

            //// update prev/next for the DCEL that is the wavefront vertices
            //v_fast.set_next_vertex(1-winning_edge_idx_in_v, o, false);

            //do_constraint_collapse_part2(*most_cw_triangle, most_cw_triangle.index(losing_edge), time);

            ////DBG_FUNC_END(//DBG_KT_EVENT);
        }




    }

}
