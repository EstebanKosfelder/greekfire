
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace CGAL
{
    public abstract class EventData:IComparable<EventData>
    {
        static int IDs = 0; 
        public override string ToString()
        {
            return $"({Id}) {Type} T:{Triangle.Id} t:{Time}" ;
        }

        public EventData(CollapseType type,  double time ) 
        {
            Id = IDs++;
            Type = type;
            Time = time;
        }
        public readonly int Id;
        public CollapseType Type;
        public double Time { get; protected set; }
        public abstract KineticTriangle Triangle { get;  }

        public  static EventData CreateEventData(EdgeCollapseSpec edge_collapse, Halfedge edge)
        {

            Debug.Assert(edge_collapse.Type == EdgeCollapseType.Future ||
                       edge_collapse.Type == EdgeCollapseType.Always ||
                       edge_collapse.Type == EdgeCollapseType.Never ||
                       edge_collapse.Type == EdgeCollapseType.Past);

            CollapseType type_ = edge_collapse.Type == EdgeCollapseType.Future ? CollapseType.ConstraintCollapse :
                                 edge_collapse.Type == EdgeCollapseType.Always ? CollapseType.ConstraintCollapse :
                                                                                 CollapseType.Never;
            double time_ = type_ == CollapseType.ConstraintCollapse ? edge_collapse.Time : 0.0;

            KineticTriangle triangle = edge.Face is KineticTriangle tri ? tri : throw new InvalidCastException($"{edge.Face} is not {nameof(KineticTriangle)}");

             switch (edge_collapse.Type )
            {
                case EdgeCollapseType.Future:
                case EdgeCollapseType.Always:
                     return new ConstrainCollapseEventData(edge, time_);
                default:
                    //EdgeCollapseType.Never:
                    // EdgeCollapseType.Past:
                    return new NeverCollapseEventData(triangle,$"Constrain collapse on {edge_collapse.Type}");

            }



        }

       
        public virtual int CompareTo(EventData? other)
        {
            Debug.Assert(other != null);
            Debug.Assert(Type != CollapseType.Undefined);
            Debug.Assert(other.Type != CollapseType.Undefined);
            if (other == null)
            {
                return -1;
            }
              

                if (Type == CollapseType.Never)
                {
                    if (other.Type == CollapseType.Never)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }

                else if (other.Type == CollapseType.Never)
                {
                    return -1;
                }


                var c = Time.CompareTo(other.Time);
                if (c == (int)SignEnum.ZERO)
                {
                    if (Type < other.Type)
                    {
                        c = -1 ;
                    }
                    else if (Type > other.Type)
                    {
                        c = 1;
                    }
                }
                return c;
            
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is EventData other)) return false;
            return this.CompareTo(other) == 0;
        }

        public abstract void Handle(GreekFireBuilder builder);
       
        //public static bool operator == (EventData left, EventData right)
        //{
        //    if (left == null) return right == null;
        //    if (left == null || right == null)
        //    {
        //        return (left == null && right == null);
        //    }
        //    return left.Equals(right);  

        //}
        //public static bool operator !=(EventData? left, EventData? right)
        //{
        //    if (left == null || right == null)
        //    {
        //        return (left == null && right == null);
        //    }
        //    return !(left ==  right);

        //}

        protected void do_spoke_collapse_part2(GreekFireBuilder builder, Halfedge he, double time)
        {
            Debug.Assert(he.IsKinetic);
            //DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            //DBG(//DBG_KT_EVENT) << "t  " << &t;

            // int num_constraints =  t.is_constrained(0).ToInt() + t.is_constrained(1).ToInt() + t.is_constrained(2).ToInt();
            KineticTriangle t = (KineticTriangle)he.Face;
            Vertex v = he.Prev.Vertex;// t.vertices[edge_idx];
            Vertex va = he.Vertex;// t.vertices[TriangulationUtils.ccw(edge_idx)];
            Vertex vb = he.Next.Vertex; // t.vertices[TriangulationUtils.cw(edge_idx)];
            Debug.Assert(va.has_stopped());
            Debug.Assert(vb.has_stopped());
            Debug.Assert(va.pos_stop() == vb.pos_stop());
            Debug.Assert(!v.has_stopped());



            Debug.Assert(he.IsCollapse);

            var posa = va.pos_stop();

            if (v.PointAt(time) == posa)
            {
                t.set_dying();
                v.stop(time);
                // triangle collapses completely
                //DBG(//DBG_KT_EVENT) << "triangle collapses completely as spoke collapses; spoke is " << edge_idx;
                //LOG(WARNING) << __FILE__ << ":" << __LINE__ << " " << "untested code path.";

                int num_neighbors = 0;
                foreach (var h in he.Next.Halfedges.Take(2))
                {

                    // int edge = TriangulationUtils.mod3(edge_idx + i);
                    //DBG(//DBG_KT_EVENT) << " at edge " << edge;
                    if (!h.IsCollapse)
                    {
                        //DBG(//DBG_KT_EVENT) << "  we have a neighbor";
                        num_neighbors++;

                        var nh = h.Opposite;
                        KineticTriangle n = (KineticTriangle)nh.Face;
                        Debug.Assert(n != null);


                        h.IsCollapse = true;
                        nh.IsCollapse = true;

                        do_spoke_collapse_part2(builder, nh, time);
                    }
                    else
                    {
                        //DBG(//DBG_KT_EVENT) << "  we have no neighbor";
                        //LOG(WARNING) << __FILE__ << ":" << __LINE__ << " " << "untested code path: DECL linking.";
                        h.Next.Vertex.set_next_vertex(1, h.Prev.Vertex, false);
                        if (h.IsWavefrontEdge)
                        {
                            h.WavefrontEdge.set_dead();
                        }
                    }
                }
                builder.EventQueue.needs_dropping(t);
            }
            else
            {
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
                    do_constraint_collapse_part2(builder, he, time);
                }
            }
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
            protected  void do_constraint_collapse_part2(GreekFireBuilder builder,Halfedge edge, double time)
            {

           

            Vertex va = edge.Next.Vertex;
            Vertex vb = edge.Prev.Vertex;
            Debug.Assert(va.has_stopped());
            Debug.Assert(vb.has_stopped());
            Debug.Assert(va.pos_stop() == vb.pos_stop());

            var pos = va.pos_stop();

            Triangle.set_dying();
            move_constraints_to_neighbor(edge);

            if (edge.IsWavefrontEdge)
            {
                /* constraint collapsed */
                Debug.Assert(edge.WavefrontEdge == va.incident_wavefront_edge(1));
                Debug.Assert(edge.WavefrontEdge == vb.incident_wavefront_edge(0));
            }
            else
            {
                /* spoke collapsed */
                Debug.Assert(edge.IsCollapse || ! edge.IsKinetic );
            }
            WavefrontEdge ea = va.incident_wavefront_edge(0);
            WavefrontEdge eb = vb.incident_wavefront_edge(1);

            KineticTriangle na = (KineticTriangle) edge.Prev.Opposite.Face;
            KineticTriangle nb = (KineticTriangle) edge.Next.Opposite.Face;

            Vertex v = new Vertex(builder.WavefrontVertices.Count,pos, time, ea, eb);
            builder.WavefrontVertices.Add(v);   
            //DBG(//DBG_KT) << " va: " << va;
            //DBG(//DBG_KT) << " vb: " << vb;
            va.set_next_vertex(0, v);
            vb.set_next_vertex(1, v);
            throw new NotImplementedException(); 
            {
                //int affected_triangles = 0;

                ////DBG(//DBG_KT_EVENT) << "updating vertex in affected triangles";
                //AroundVertexIterator end = incident_faces_end();
                //AroundVertexIterator i = incident_faces_iterator(&Triangle, ccw(edge_idx));
                ////DEBUG_STMT(bool first = true);
                ////DBG(//DBG_KT_EVENT) << " ccw:";
                //for (--i; i != end; --i)
                //{
                //    Debug.Assert(!first || na == &*i); DEBUG_STMT(first = false);
                //    (*i).set_vertex(i.v_in_t_idx(), v);
                //    modified(&*i);
                //    ++affected_triangles;
                //};

                //i = incident_faces_iterator(&Triangle, cw(edge_idx));
                ////DEBUG_STMT(first = true);
                ////DBG(//DBG_KT_EVENT) << " cw:";
                //for (++i; i != end; ++i)
                //{
                //    Debug.Assert(!first || nb == &*i); DEBUG_STMT(first = false);
                //    (*i).set_vertex(i.v_in_t_idx(), v);
                //    modified(&*i);
                //    ++affected_triangles;
                //}

                //max_triangles_per_edge_event = Math.Max(max_triangles_per_edge_event, affected_triangles);
                //avg_triangles_per_edge_event_sum += affected_triangles;
                //++avg_triangles_per_edge_event_ctr;
            };

            //DBG(//DBG_KT_EVENT) << " cw:";
            //if (na != null)
            //{
            //    var idx = na.index(Triangle);
            //    Debug.Assert(na.index(v) == ccw(idx));
            //    na.neighbors[idx] = nb;
            //}
            //if (nb != null)
            //{
            //    var idx = nb.index(Triangle);
            //    Debug.Assert(nb.index(v) == cw(idx));
            //    nb.neighbors[idx] = na;
            //}

            //if (Triangle.wavefront(edge_idx) != null)
            //{
            //    /* if it was a constraint collapse rather than a spoke collapse */
            //    Triangle.wavefront(edge_idx).set_dead();
            //}
            //queue.needs_dropping(Triangle);

            //DBG_FUNC_END(//DBG_KT_EVENT);
            }


        protected void move_constraints_to_neighbor( Halfedge he)
        {
            var ha = he.Next;
            var hb = he.Prev;


             Debug.Assert(! (ha.IsWavefrontEdge && hb.IsWavefrontEdge) );

            if (!ha.IsWavefrontEdge)
            {
                (ha, hb) = (hb, ha);
            }


             if ( ha.IsWavefrontEdge)
            {
                Debug.Assert( !  hb.IsWavefrontEdge );
                var hn = hb.Opposite;
                var n = (KineticTriangle)hn.Face;   
                move_constraint_from(hn , ha);
            }
           
        }

        internal void move_constraint_from(Halfedge hn, Halfedge hsrc)
        {

            KineticTriangle t = (KineticTriangle)hn.Face;
            KineticTriangle src = (KineticTriangle)hsrc.Face; 
            Debug.Assert(t!=null);
            Debug.Assert(src!= null);
            /// {{{
            // CGAL_precondition(idx < 3 && src_idx < 3);
            Debug.Assert(src.IsDying);
            Debug.Assert(!hn.IsWavefrontEdge);
            Debug.Assert(hsrc.WavefrontEdge.IncidentTriangle == src);
            var wavefrontEdge = hn.WavefrontEdge = hsrc.WavefrontEdge;

            //// we already need to share one vertex with the origin, which will go away.
            Debug.Assert(wavefrontEdge.vertex(0) == hn.Next.Vertex ||
                   wavefrontEdge.vertex(1) == hn.Prev.Vertex);
            Debug.Assert(hn.Neighbor == src);
            

            wavefrontEdge.set_wavefrontedge_vertex(0, hn.Next.Vertex);
            wavefrontEdge.set_wavefrontedge_vertex(1, hn.Prev.Vertex);
            wavefrontEdge.set_incident_triangle(t);

            hsrc.RemoveWavefrontEdge();
            hsrc.RemoveWavefrontEdge();

            t.InvalidateEvent();
        } 
    }
}