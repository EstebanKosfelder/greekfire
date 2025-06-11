

namespace CGAL
{
    using System.Diagnostics;
    using static DebugLog;


    public class SplitEventData : EdgeEventData
    {
        public SplitEventData(Halfedge edge, double time) : base(edge, CollapseType.SplitCollapse, time)
        {
        }
public override void Handle(GreekFireBuilder builder)
        {
            Log($"Handle {this}");
            //DBG_FUNC_BEGIN(//DBG_KT_EVENT);
            //DBG(//DBG_KT_EVENT) << evnt;

            Debug.Assert(this.Type == CollapseType.SplitOrFlipRefine);
            KineticTriangle t = this.Triangle;
            double time = this.Time;
            var edge = this.Edge;
            //DBG(//DBG_KT_EVENT2) << " t:  " << &t;
            Vertex v = edge.Vertex;

            WavefrontEdge e = edge.WavefrontEdge;
            WavefrontEdge eb = v.incident_wavefront_edge(0);
            WavefrontEdge ea = v.incident_wavefront_edge(1);
            Debug.Assert(ea.vertex(0) == v);
            Debug.Assert(eb.vertex(1) == v);

            //TODO
            Debug.Assert(edge.Prev.Opposite.Face is KineticTriangle);
            Debug.Assert(edge.Next.Opposite.Face is KineticTriangle);

            KineticTriangle na = (KineticTriangle)edge.Opposite.Prev.Face;
            KineticTriangle nb = (KineticTriangle)edge.Opposite.Next.Face;

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
                var o0 = (Mathex.orientation(e.SupportLine.l.to_vector(), e0));
                var o1 = (Mathex.orientation(e.SupportLine.l.to_vector(), e1));

                Debug.Assert(o0 != OrientationEnum.LEFT_TURN);
                Debug.Assert(o1 != OrientationEnum.RIGHT_TURN);
            }

            var pos = v.pos_stop();
            // Stop the vertex
            v.stop(time);

            // Split edge e into parts,
            var new_edges = e.Split(builder.WavefrontEdges);
            WavefrontEdge nea = new_edges.Left;
            WavefrontEdge neb = new_edges.Right;

            // Create new wavefront vertices with the new edges
            Vertex nva = new Vertex(builder.WavefrontVertices.Count, pos, time, nea, ea, true);
            builder.WavefrontVertices.Add(nva);
            Vertex nvb = new Vertex(builder.WavefrontVertices.Count, pos, time, eb, neb, true);
            builder.WavefrontVertices.Add(nvb);
            // And set these new vertices on the edges.
            nea.vertex(0).set_incident_wavefront_edge(1, nea);
            nea.set_wavefrontedge_vertex(1, nva);
            neb.set_wavefrontedge_vertex(0, nvb);
            neb.vertex(1).set_incident_wavefront_edge(0, neb);

            // And update prev/next for the DCEL that is the wavefront vertices
            v.set_next_vertex(0, nvb);
            v.set_next_vertex(1, nva);
            nva.link_tail_to_tail(nvb);

           
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
            builder.assert_valid( time);

            //DBG_FUNC_END(//DBG_KT_EVENT);
        }
    }
    public class SplitOrFlipRefineEventData : EdgeEventData
    {

       

        public SplitOrFlipRefineEventData(Halfedge edge, double time) : base(edge, CollapseType.SplitOrFlipRefine, time)
        {
            T = (KineticTriangle)Edge.Face;
            N = (KineticTriangle)getNeighborEdge.Face;
            Debug.Assert(Edge != null);
            Debug.Assert(T != null);
            Debug.Assert(N != null);

            tc = Tbc.Vertex;
            ta = Tca.Vertex;
            tb = Tab.Vertex;
            nc = Nbc.Vertex;

            Debug.Assert(Na == Tb);
            Debug.Assert(Nb == Ta);

        }


        public Point2 Pa => Ta.PointAt(Time);
        public Point2 Pb => Tb.PointAt(Time);

        public Point2 Pc => Tc.PointAt(Time);

        ///<summary>
        /// Check if a vertex is moving faster or slower relative to an edge.
        ///
        /// If the vertex is in front of the edge, and the edge is faster
        /// (POSITIVE), the edge may eventually catch up, causing a split
        /// or flip event.
        ///
        /// If the vertex is faster then the edge (i.e. if the edge is slower,
        /// NEGATIVE), and if the vertex is behind, v might overtake the edge
        /// causing a flip event.
        ///
        /// If the edge is slower, return -1.  If it's faster +1.  If the same speed, 0.
        ///
        /// (As a result, if they move in opposite directions, then e is "faster" and +1 is returned.)
        ///</summary>
        private static int EdgeIsFasterThanVertex(Vertex v, WavefrontSupportingLine e)
        {

            // Let n be some normal to e,
            // let s be the speed (well, velocity) vector of the vertex v.
            // let w be the weight (speed) of e.
            Vector2 n = e.normal_direction;

            Vector2 s = v.velocity;
            double w = e.weight;
           
            // Then s.n is the length of the projection of s onto n, times the length of n.
            // Per time unit, v and e will approach each other by (w - s.n/|n|).
            double scaled_edge_speed = w * n.Length();
            double scaled_vertex_speed = s.Dot(n);

            double speed_approach = scaled_edge_speed - scaled_vertex_speed;
            int sign = Mathex.sign(speed_approach);

            return sign;
        }
        public override void Handle(GreekFireBuilder builder)
        {

            LogIndent();

            Log($"Handle {this}");

            Debug.Assert(this.Type == CollapseType.SplitOrFlipRefine);

            KineticTriangle t = this.Triangle;
            double time = (this.Time);

            var edge = this.Edge;


            Debug.Assert(edge.IsConstrain);
            Debug.Assert(!edge.Next.IsConstrain);
            Debug.Assert(!edge.Prev.IsConstrain);

            var wavefrontEdge = edge.WavefrontEdge;
            Debug.Assert(wavefrontEdge != null);

            Log($"t :{t}");
            Log($"to:{edge.Next.Opposite.Face}");



            Vertex a = Ta;
            Vertex b = Tb;
            Vertex c = Tc;



            Debug.Assert(!c.HasStopped);

            var pA = this.Pa;
            var pB =this.Pb;
            var pC = this.Pc;
            var SegABatTime = new Segment2(pA, pB);
            //var isCollineal = s.has_on(pos);
            //Debug.Assert(isCollineal);

            /* 3 cases:
             * (1) the vertex v is outside of the segment s, on the supporting line,
             * (2) it's at one of the endpoints of s,
             * (1) or v is on the interior of the edge.
             */

            double sqLen_AB = Mathex.squared_distance(pA, pB);
            double sqLen_CA = Mathex.squared_distance(pC, pA);
            double sqLen_BC = Mathex.squared_distance(pB, pC);
            double longestSpoke;
            Log($"c: {pC}");
            Log($"a: {pA}");
            Log($"b: {pB}");
            Log($"l^2 ab: {sqLen_AB}");
            Log($"l^2 ca: {sqLen_CA}");
            Log($"l^2 bc: {sqLen_BC}");


            var pANearPC = pA.AreNear(pC);
            var pBNearPC = pB.AreNear(pC);
            var isCollinear = SegABatTime.CollinearHasOn(pC);

            if (!(pANearPC || pBNearPC ||isCollinear ))
            {

                // case 1
                Log($"CASE 1:pC is not colineal posa-posb ");
                Log("A potential split evnt is actually a flip evnt.  Maybe refinement should have prevented that?");
                Log("Re-classifying as flip evnt as v is not on the constrained segment.");

                Debug.Assert(EdgeIsFasterThanVertex(Tc, wavefrontEdge.SupportLine) != (int)ESign.Negative);

                // there are basically two types of flip events involving constrained triangles.
                // One is where the vertex is coming towards the supporting line of the constraint
                // edge e and is passing left or right of of e.  The other is where (the
                // supporting line of an edge e overtakes a vertex.
                
                // They are generally handled identically, however the assertions are slightly different
                // ones as these cases differ in which of v's incident edges is relevant.
                
                var e = (wavefrontEdge.SupportLine.l.to_vector());
                var e0 = (Tb.incident_wavefront_edge(0).SupportLine.l.to_vector());
                var e1 = (Tb.incident_wavefront_edge(1).SupportLine.l.to_vector());
                var o0 = (Mathex.orientation(e, e0));
                var o1 = (Mathex.orientation(e, e1));

                Halfedge flipEdge;

                /* Figure out which side of the constraint edge we're on. */
                if (sqLen_AB < sqLen_BC)
                {
                    //DBG(//DBG_KT_EVENT) << "(v, va) is the longest spoke, so vb moves over that.";
                    longestSpoke = sqLen_CA;


                    if (!(sqLen_CA > sqLen_AB))
                    {
                      //     Debug.Assert(sqLen_CA > sqLen_AB); /* Check that CA is the longest spoke */
                    }
                    Debug.Assert(Tb == edge.Prev.Prev.Vertex);
                    Debug.Assert(b.IsReflexOrStraight);

                    // If we come from behind, we don't really care about the first of these things in the discunjtion,
                    // if we face it head on, we don't care about the second.  hmm. 
                    Debug.Assert(o1 != OrientationEnum.RIGHT_TURN || (Tc.IsReflexOrStraight && o0 != OrientationEnum.RIGHT_TURN));

                    //TODO  ckeck
                    flipEdge = Tbc;
                }
                else // (AB) > (CB) 
                {
                    //DBG(//DBG_KT_EVENT) << "(v, vb) is the longest spoke, so va moves over that.";
                    longestSpoke = sqLen_BC;
                    Debug.Assert(!sqLen_AB.AreNear( sqLen_BC,Mathex.EPS2)); /* they really shouldn't be able to be equal. */


                    if (!(sqLen_AB > sqLen_CA))
                    {
                     //   Debug.Assert(sqLen_AB > sqLen_CA); /* Check that v to vb is the longest spoke */
                    }
                    Debug.Assert(a.IsReflexOrStraight);

                    /* If we come from behind, we don't really care about the first of these things in the discunjtion,
                     * if we face it head on, we don't care about the second.  hmm. */
                    Debug.Assert(o0 != OrientationEnum.LEFT_TURN || (Tc.IsReflexOrStraight && o1 != OrientationEnum.LEFT_TURN));
                    //TODO  ckeck

                    flipEdge = Tab;
                };
                var vertexMovesOverSpokeEventData = new VertexMovesOverSpokeEventData(flipEdge, longestSpoke, time);
                var evnt = this.Triangle.RefineCollapseSpec( vertexMovesOverSpokeEventData);
                Log($"Refining to {evnt}");
                builder.EventQueue.NeedsUpdate(t, true);
            }
            else if (pANearPC)
            {
                Log($"CASE 2: pos == posa");
                var spokeEventData = new SpokeCollapseEventData(this.Tab, time);
                var evnt = t.RefineCollapseSpec(spokeEventData);
                Log($"v is incident to va Refining to {evnt}");
                builder.EventQueue.NeedsUpdate(t, true);
            }
            else if (pBNearPC)
            {
                Log($"CASE 2: pos == posb");
                var spokeEventData = new SpokeCollapseEventData(this.Tbc, time);
                var e = t.RefineCollapseSpec(spokeEventData);
                Log($"v is incident to vb Refining to {e}");
                builder.EventQueue.NeedsUpdate(t, true);
            }
            else
            {
                Log($"CASE3 We have a real split this.");
                var SplitEventData = new SplitEventData( this.Edge, time);
                var e = t.RefineCollapseSpec( SplitEventData);
                builder.EventQueue.NeedsUpdate(t, true);

            }

            LogUnindent();

        }

      


        public Halfedge Tca => Edge;
        public Halfedge Tab => Edge.Next;
        public Halfedge Tbc => Edge.Prev;

        public Halfedge Nca => Nab.Next;
        public Halfedge Nab => Tab.Opposite;
        public Halfedge Nbc => Nab.Prev;

        public override KineticTriangle Triangle => T; 

        private Halfedge getNeighborEdge => Edge.Next;
        public KineticTriangle T { get; private set; }
        public KineticTriangle N { get; private set; }

        public Vertex Tc => tc;
        public Vertex Ta => ta;
        public Vertex Tb => tb;
        public Vertex Ne => nc;
        public Vertex Na => Tb;
        public Vertex Nb => Ta;

        private Vertex tc;
        private Vertex ta;
        private Vertex tb;
        private Vertex nc;
    }






   


}