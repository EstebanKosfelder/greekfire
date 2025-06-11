using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static CGAL.DebuggerInfo;
using static CGAL.Mathex;
using Segment_2 = CGAL.Segment2;
using Point_2 = CGAL.Point2;
using Line_2 = CGAL.Line2;
using FT = double;
using Trisegment_2 = CGAL.Trisegment;
using CGAL;
using System.Runtime.InteropServices.JavaScript;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;


namespace CGAL
{
    public partial class StraightSkeletonBuilder
    {
        StraightSkeleton mSSkel;
        public StraightSkeletonBuilder(FT? aMaxTime = null, IStraightSkeletonBuilderVisitor? aVisitor = null)

        {
            mVisitor = aVisitor;
            mEventCompare = new Event_compare(this);
            mVertexID = 0;
          
            mFaceID = 0;
            mEventID = 0;
            mStepID = 0;
            mMaxTime = aMaxTime;
            mPQ = new PriorityQueue<Event, Event>(mEventCompare);
            mSSkel = new StraightSkeleton();

            if (mMaxTime != null)
            {
                CGAL_STSKEL_BUILDER_TRACE(4, $"Init with mMaxTime = {mMaxTime}");
            }
        }





        private void InsertEventInPQ(Event aEvent)
        {
            mPQ.Enqueue(aEvent, aEvent);
            CGAL_STSKEL_BUILDER_TRACE(4, "Enqueue: {aEvent}");
        }

        private Event PopEventFromPQ()
        {
            Event rR = mPQ.Dequeue();
            return rR;
        }

        // Tests whether there is an edge event between the 3 contour edges defining nodes 'aLnode' and 'aRNode'.
        // If such event exits and is not in the past, it's returned. Otherwise the result is null.
        //
        public Event? FindEdgeEvent(Vertex aLNode, Vertex aRNode, Triedge? aPrevEventTriedge)
        {
            Event? rResult = null;

            CGAL_STSKEL_BUILDER_TRACE(4, $"FindEdgeEvent(), Left/Right Nodes: N{aLNode.Id} N{aRNode.Id}");

            Triedge lTriedge = GetVertexTriedge(aLNode) & GetVertexTriedge(aRNode);

            if (lTriedge.is_valid() && lTriedge != aPrevEventTriedge)
            {
                Trisegment lTrisegment = CreateTrisegment(lTriedge, aLNode, aRNode);

                CGAL_STSKEL_BUILDER_TRACE(4, $"\n[] Considering E{lTrisegment.e0().Id} <<  E{lTrisegment.e1().Id} E{lTrisegment.e2().Id} Collinearity: {lTrisegment.collinearity()}");

                // The 02 collinearity configuration is problematic: 01 or 12 collinearity has a seed position
                // giving the point through which the bisector passes. However, for 02, it is not a given.
                //
                // If the seed exists, the information is passed to the traits as the "third" child of the trisegment.
                // Otherwise, ignore this as it should re-appear when the seed of 02 is created.
                //
                // Note that this is only for edge events; (pseudo-)split events are not concerned.
                if (lTrisegment.collinearity() == Trisegment_collinearity.TRISEGMENT_COLLINEARITY_02)
                {
                    // Check in the SLAV if the seed corresponding to 02 exists
                    var lPrevNode = aLNode.PrevInLAV;
                    CGAL_assertion(GetEdgeStartingAt(lPrevNode) == lTriedge.e0());

                    if (GetEdgeEndingAt(lPrevNode) == lTriedge.e2())
                    {
                        // Note that this can be a contour node and in that case GetTrisegment returns null
                        // and we get the middle point as a seed, but in that case e2 and e0 are consecutive
                        // in the input and the middle point is the common extremity thus things are fine.
                        lTrisegment.set_child_t(GetTrisegment(lPrevNode));
                    }
                    else
                    {
                        var lOrientationS = orientation(lTrisegment.e0().source(), lTrisegment.e0().target(), lTrisegment.e1().source());
                        var lOrientationT = orientation(lTrisegment.e0().source(), lTrisegment.e0().target(), lTrisegment.e1().target());
                        if (lOrientationS != OrientationEnum.LEFT_TURN && lOrientationT != OrientationEnum.LEFT_TURN)
                        {
                            // Reasonning is: if the middle halfedge (e1) is "below" e0 and e2, then there is some
                            // kind of concavity in between e0 and e2. This concavity will resolve itself and either:
                            // - e0 and e2 will never meet, but in that case we would not be here
                            // - e0 and e2 will meet. In that case, we can ignore all the details of the concavity
                            //   and simply consider that in the end, all that matters is the e0, e2, next(e0),
                            //   and prev(e2). In that case, we get two bisectors issued from e0 and e2, and one
                            //   bisector issued from some seed S and splitting next(e0) and prev(e2). This can also
                            //   be seen as two exterior bisectors and one interior bisector of a triangle
                            //   target(e0) -- S - source(e2). It is a known result that these three bisectors
                            //   meet in a single point. Thus, when we get here e0-e1-e2, we know that
                            //   these will meet in a single, existing point, either the left or the right child (the oldest).

                            if ((CompareResultEnum)(int)CompareEvents(aLNode, aRNode) == CompareResultEnum.SMALLER)
                                lTrisegment.set_child_t(GetTrisegment(aRNode));
                            else
                                lTrisegment.set_child_t(GetTrisegment(aLNode));
                        }
                        else
                        {
                            return rResult;
                        }
                    }
                }

                if ((bool)ExistEvent(lTrisegment))
                {
                    CompareResultEnum lLNodeD = (CompareResultEnum)(int)CompareEvents(lTrisegment, aLNode);
                    CompareResultEnum lRNodeD = (CompareResultEnum)(int)CompareEvents(lTrisegment, aRNode);

                    if (lLNodeD != CompareResultEnum.SMALLER && lRNodeD != CompareResultEnum.SMALLER)
                    {
                        rResult = new EdgeEvent(lTriedge, lTrisegment, aLNode, aRNode);

                        mVisitor?.on_edge_event_created(aLNode, aRNode);

                        SetEventTimeAndPoint(rResult);
                    }
                    else
                    {
                        CGAL_STSKEL_BUILDER_TRACE(4, $"Edge event: {lTriedge} is in the past.");
                        CGAL_STSKEL_BUILDER_TRACE(4, $"\tCompared to L={aLNode.Id} ({lLNodeD})");
                        CGAL_STSKEL_BUILDER_TRACE(4, $"\tCompared to R={aRNode.Id} ({lRNodeD})");
                    }
                }
            }
            return rResult;
        }

        PseudoSplitEvent? IsPseudoSplitEvent(Event aEvent, VertexPair aOpp, Site aSite)
        {
            PseudoSplitEvent? rPseudoSplitEvent = null;

            if (aSite != Site.INSIDE)
            {
                if (aEvent is SplitEvent lEvent)
                {
                    Triedge lEventTriedge = lEvent.triedge();
                    Trisegment lEventTrisegment = lEvent.trisegment();
                    Vertex lSeedN = lEvent.seed0();

                    Vertex lOppL = aOpp.first ?? throw new ArgumentException($"{nameof(aOpp.first)} Vertex is null ", nameof(aOpp));
                    Vertex lOppR = aOpp.last ?? throw new ArgumentException($"{nameof(aOpp.last)}Vertex is null ", nameof(aOpp));

                    if (aSite == Site.AT_SOURCE)
                    {
                        Halfedge lOppPrevBorder = GetVertexTriedge(lOppL).e0();

                        if (lEventTriedge.e0() != lOppPrevBorder && lEventTriedge.e1() != lOppPrevBorder)
                        {
                            rPseudoSplitEvent = new PseudoSplitEvent(lEventTriedge, lEventTrisegment, lOppL, lSeedN, true);

                            CGAL_STSKEL_BUILDER_TRACE(1, $"Pseudo-split-event found against {lOppL}");

                            mVisitor?.on_pseudo_split_event_created(lOppL, lSeedN);
                        }
                    }
                    else // aSite == AT_TARGET
                    {
                        Vertex lOppNextN = lOppR.NextInLAV;

                        Halfedge lOppNextBorder = GetVertexTriedge(lOppNextN).e0();

                        if (lEventTriedge.e0() != lOppNextBorder && lEventTriedge.e1() != lOppNextBorder)
                        {
                            rPseudoSplitEvent = new PseudoSplitEvent(lEventTriedge, lEventTrisegment, lSeedN, lOppR, false);

                            CGAL_STSKEL_BUILDER_TRACE(1, $"Pseudo-split-event found against {lOppR}");

                            mVisitor?.on_pseudo_split_event_created(lSeedN, lOppR);
                        }
                    }
                }
                else
                {
                    throw new ArgumentException($" mus be  {nameof(SplitEvent)} if argument {nameof(Site)} != {Site.INSIDE}", nameof(aEvent));
                }
            }

            if (rPseudoSplitEvent != null)
                rPseudoSplitEvent.SetTimeAndPoint(aEvent.time(), aEvent.point());

            return rPseudoSplitEvent;
        }

        // Tests whether there is a split event between the contour edges (aReflexLBorder,aReflexRBorder,aOppositeBorder).
        // If such event exits and is not in the past, it's returned. Otherwise the result is null
        // 'aReflexLBorder' and 'aReflexRBorder' are consecutive contour edges which 'aNode' as the vertex.
        // 'aOppositeBorder' is some other edge in the polygon which, if the event exists, is split by the reflex wavefront.
        //
        // NOTE: 'aNode' can be a skeleton node (an interior split event produced by a previous vertex event). In that case,
        // the 'reflex borders' are not consecutive in the input polygon but they are in the corresponding offset polygon that
        // contains aNode as a vertex.
        //

        void CollectSplitEvent(Vertex aNode, Triedge aTriedge)
        {
            CGAL_STSKEL_BUILDER_TRACE(3, $"Collect SplitEvent for N{aNode.Id} triedge: {aTriedge}");

            if (IsOppositeEdgeFacingTheSplitSeed(aNode, aTriedge.e2()))
            {
                Trisegment lTrisegment = CreateTrisegment(aTriedge, aNode);

                if (lTrisegment.collinearity() != Trisegment_collinearity.TRISEGMENT_COLLINEARITY_02 && (bool)ExistEvent(lTrisegment))
                {
                    if ((CompareResultEnum)(int)CompareEvents(lTrisegment, aNode) != CompareResultEnum.SMALLER)
                    {
                        Event lEvent = new SplitEvent(aTriedge, lTrisegment, aNode);

                        // filter split event
                        if (CanSafelyIgnoreSplitEvent(lEvent))
                            return;

                        mVisitor?.on_split_event_created(aNode);

#if DEBUG
                        SetEventTimeAndPoint(lEvent);
#endif
                        AddSplitEvent(aNode, lEvent);
                    }
                }
            }
        }

        // Tests the reflex wavefront emerging from 'aNode' against the other contour edges in search for split events.
        public
        void CollectSplitEvents(Vertex aNode, Triedge? aPrevEventTriedge)
        {
            // lLBorder and lRBorder are the consecutive contour edges forming the reflex wavefront.
            Triedge lTriedge = GetVertexTriedge(aNode);

            Halfedge lLBorder = lTriedge.e0();
            Halfedge lRBorder = lTriedge.e1();

            CGAL_STSKEL_BUILDER_TRACE(3, "Finding SplitEvents for N{aNode.Id} LBorder: E{lLBorder.Id} RBorder: E{lRBorder.Id}");

            ComputeUpperBoundForValidSplitEvents(aNode, mContourHalfedges);

            foreach (var lOpposite in mContourHalfedges)
            {
                if (lOpposite != lLBorder && lOpposite != lRBorder)
                {
                    Triedge lEventTriedge = new Triedge(lLBorder, lRBorder, lOpposite);

                    if (lEventTriedge != aPrevEventTriedge)
                    {
                        CollectSplitEvent(aNode, lEventTriedge);
                    }
                }
            }

            CGAL_STSKEL_BUILDER_TRACE(4, $"#Split Events={aNode.SplitEvents.Count}");
        }

        // Finds and enques all the new potential events produced by the vertex wavefront emerging from 'aNode' (which can be a reflex wavefront).
        // This new events are simply stored in the priority queue, not processed.
        private void CollectNewEvents(Vertex aNode, Triedge? aPrevEventTriedge)
        {
            // A Straight Skeleton is the trace of the 'grassfire propagation' that corresponds to the inward move of all the vertices
            // of a polygon along their angular bisectors.
            // Since vertices are the common endpoints of contour edges, the propagation corresponds to contour edges moving inward,
            // shrinking and expanding as neccesasry to keep the vertices along the angular bisectors.
            // At each instant in time the current location of vertices (and edges) describe the current 'Offset polygon'
            // (with at time zero corresponds to the input polygon).
            //
            // An 'edge wavefront' is a moving contour edge.
            // A 'vertex wavefront' is the wavefront of two consecutive edge wavefronts (sharing a moving vertex).
            //
            // An 'Event' is the collision of 2 wavefronts.
            // Each event changes the topology of the shrinking polygon; that is, at the event, the current polygon differs from the
            // immediately previous polygon in the number of vertices.
            //
            // If 2 vertex wavefronts sharing a common edge collide, the event is called an edge event. At the time of the event, the current
            // polygon doex not have the common edge anymore, and the two vertices become one. This new 'skeleton' vertex generates a new
            // vertex wavefront which can further collide with other wavefronts, producing for instance, more edge events.
            //
            // If a reflex vertex wavefront collide with an edge wavefront, the event is called a split event. At the time of the event, the current
            // polygon is split in two unconnected polygons, each one containing a portion of the edge hit and split by the reflex wavefront.
            //
            // If 2 reflex wavefronts collide with each other, the event is called a vertex event. At the time of the event, the current polygon
            // is split in two unconnected polygons. Each one contains a different combination of the colliding reflex edges. That is, if the
            // wavefront (edgea,edgeb) collides with (edgec,edged), the two resulting polygons will contain (edgea,edgec) and (edgeb,edged).
            // Furthermore, one of the new vertices can be a reflex vertex generating a reflex wavefront which can further produces more split
            // or vertex events (or edge events of course).
            //
            // Each vertex wavefront (reflex or not) results in one and only one event from a set of possible events.
            // It can result in a edge event against the vertex wavefronts emerging from the adjacent vertices (in the current polygon, not
            // in the input polygon); or it can result in a split event (or vertex event) against any other wavefront in the rest of
            // current polygon.

            // Adjacent vertices in the current polygon containing aNode (called LAV)
            Vertex lPrev = aNode.PrevInLAV;
            Vertex lNext = aNode.NextInLAV;

            CGAL_STSKEL_BUILDER_TRACE
              (2, $"Collecting new events generated by N{aNode.Id} at {aNode.point()} (Prev: N{lPrev.Id} Next: N{lNext.Id})");

            if (IsReflex(aNode))
                CollectSplitEvents(aNode, aPrevEventTriedge);

            Event? lLEdgeEvent = FindEdgeEvent(lPrev, aNode, aPrevEventTriedge);
            Event? lREdgeEvent = FindEdgeEvent(aNode, lNext, aPrevEventTriedge);
            CGAL_STSKEL_BUILDER_TRACE(2, $"Done Left {(lLEdgeEvent == null ? "Found" : "Not Found")}");

            CGAL_STSKEL_BUILDER_TRACE(2, $"Done Right {(lREdgeEvent == null ? "Found" : "Not Found")}");

            if (lLEdgeEvent != null)
                InsertEventInPQ(lLEdgeEvent);

            if (lREdgeEvent != null)
                InsertEventInPQ(lREdgeEvent);
        }

        // Handles the special case of two simultaneous edge events, that is, two edges
        // collapsing along the line/point were they meet at the same time.
        // This occurs when the bisector emerging from vertex 'aA' is defined by the same pair of
        // contour edges as the bisector emerging from vertex 'aB' (but in opposite order).
        //
        public
        void HandleSimultaneousEdgeEvent(Vertex aA, Vertex aB)
        {
            CGAL_STSKEL_BUILDER_TRACE(2, $"Handling simultaneous EdgeEvent between N{aA.Id} and N{aB.Id}");

            mVisitor?.on_anihiliation_event_processed(aA, aB);

            Halfedge lOA = aA.primary_bisector();
            Halfedge lOB = aB.primary_bisector();
            Halfedge lIA = lOA.Opposite;
            Halfedge lIB = lOB.Opposite;

            Vertex lOAV = lOA.Vertex;
            Vertex lIAV = lIA.Vertex;
            Vertex lOBV = lOB.Vertex;
            // Vertex lIBV = lIB.Vertex ;

            CGAL_STSKEL_BUILDER_TRACE(2,
                $"OA: B{lOA.Id} V{lOA.Vertex.Id}",
                $"IA: B{lOA.Id} V{lIA.Vertex.Id}",
                $"OB: B{lOA.Id} V{lOB.Vertex.Id}",
                $"IB: B{lOA.Id} V{lIB.Vertex.Id}");

            SetIsProcessed(aA);
            SetIsProcessed(aB);
            GLAV_remove(aA);
            GLAV_remove(aB);

            CGAL_STSKEL_BUILDER_TRACE(3, $"N{aA.Id} processed\nN{aB.Id} processed");

            Halfedge lOA_Prev = lOA.Prev;
            Halfedge lIA_Next = lIA.Next;

            Halfedge lOB_Prev = lOB.Prev;
            Halfedge lIB_Next = lIB.Next;

            CGAL_STSKEL_BUILDER_TRACE(2, $"OA_Prev: B{lOA_Prev.Id} V{lOA_Prev.Vertex.Id}",
                                           $"IA_Next: B{lIA_Next.Id} V{lIA_Next.Vertex.Id}",
                                           $"OB_Prev: B{lOB_Prev.Id} V{lOB_Prev.Vertex.Id}",
                                           $"IB_Next: B{lIB_Next.Id} V{lIB_Next.Vertex.Id}"
                                      );

            // For weighted skeletons of polygons with holes, one can have a skeleton face wrap
            // around an input hole. In that case, we have a configuration of two fictous vertices
            // meeting from each side after going around the hole, as well as the fictous vertex
            // from the hole edge where the simultaneous event occurs. We then have to close the "strait"
            // and ensure that the contour edge corresponding to the wrapping face still has
            // a fictous vertex to continue its progression beyond the hole.

            Halfedge lIA_Prev = lIA.Prev;
            CGAL_STSKEL_BUILDER_TRACE(2, $"lIA_Prev: B{lIA_Prev.Id} V{lIA_Prev.Vertex.Id}");

            if (lIA_Prev != lOB)
            {
                CGAL_STSKEL_BUILDER_TRACE(2, "Closing A-strait N{lOBV.Id} N{lIA_Prev.Vertex.Id}");

                Halfedge lOB_Next = lOB.Next;
                CrossLinkFwd(lIA_Prev, lOB_Next);

                lOB_Next.Vertex.NextInLAV = lIA_Prev.Prev.Vertex;
                lIA_Prev.Prev.Vertex.PrevInLAV= lOB_Next.Vertex;
            }

            Halfedge lIB_Prev = lIB.Prev;
            CGAL_STSKEL_BUILDER_TRACE(2, "lIB_Prev: B{lIB_Prev.Id} V{lIB_Prev.Vertex.Id}");

            if (lIB_Prev != lOA)
            {
                CGAL_STSKEL_BUILDER_TRACE(2, "Closing B-strait N{lOAV.Id} N{lIB_Prev.Vertex.Id}");

                Halfedge lOA_Next = lOA.Next;
                CrossLinkFwd(lIB_Prev, lOA_Next);

                lOA_Next.Vertex.NextInLAV = lIB_Prev.Prev.Vertex;
                lIB_Prev.Prev.Vertex.PrevInLAV= lOA_Next.Vertex;
            }

            // Merge the two bisectors

            CrossLinkFwd(lOB, lIA_Next);
            CrossLinkFwd(lOA_Prev, lIB);

            Link(lOB, aA);

            // The code above corrects the links for vertices aA/aB to the erased halfedges lOA and lIA.
            // However, any of these vertices (aA/aB) may be one of the twin vertices of a split event.
            // If that's the case, the erased halfedge may be be linked to a 'couple' of those vertices.
            // This situation is corrected below:

            if (!lOAV.has_infinite_time() && lOAV != aA && lOAV != aB)
            {
                Link(lOAV, lIB);

                CGAL_STSKEL_BUILDER_TRACE(1, $"N{lOAV.Id} has B{lOA.Id} as its halfedge. Replacing it with B{lIB.Id}");
            }

            if (!lIAV.has_infinite_time() && lIAV != aA && lIAV != aB)
            {
                Link(lIAV, lOB);

                CGAL_STSKEL_BUILDER_TRACE(1, $"N{lIAV.Id} has B{lIA.Id} as its halfedge. Replacing it with B{lOB.Id}");
            }

            CGAL_STSKEL_BUILDER_TRACE(2, $"N{aA.Id} halfedge: B{aA.halfedge().Id}");
            CGAL_STSKEL_BUILDER_TRACE(2, $"N{aB.Id} halfedge: B{aB.halfedge().Id}");

            SetBisectorSlope(aA, aB);

            CGAL_assertion(aA.primary_bisector() == lIB);

            CGAL_STSKEL_BUILDER_TRACE(1, $"Wavefront: E{lIB.defining_contour_edge()?.Id} and E{lIB.Opposite.defining_contour_edge()?.Id} annihilated each other.");

            if (lOAV.has_infinite_time())
            {
                CGAL_STSKEL_BUILDER_TRACE(2, $"Fictitious N{lOAV.Id} erased.");
                EraseNode(lOAV);
            }

            if (lOBV.has_infinite_time())
            {
                CGAL_STSKEL_BUILDER_TRACE(2, $"Fictitious N{lOBV.Id} erased.");
                EraseNode(lOBV);
            }

            CGAL_STSKEL_BUILDER_TRACE(1, $"B{lOA.Id} and B{lIA.Id} erased.");
            EraseBisector(lOA); // `edges_erase(h)` removes `h` and `h.Opposite`
        }

        // Returns true if the skeleton edges 'aA' and 'aB' are defined by the same pair of contour edges (but possibly in reverse order)
        //
        public
        bool AreBisectorsCoincident(Halfedge aA, Halfedge aB)
        {
            CGAL_STSKEL_BUILDER_TRACE(3, $"Testing for simultaneous EdgeEvents between B{aA.Id} and B{aB.Id}");

            Halfedge? lA_LBorder = aA.defining_contour_edge();
            Halfedge? lA_RBorder = aA.Opposite.defining_contour_edge();
            Halfedge? lB_LBorder = aB.defining_contour_edge();
            Halfedge? lB_RBorder = aB.Opposite.defining_contour_edge();

            CGAL_STSKEL_BUILDER_TRACE(3, $"aA = {aA}");
            CGAL_STSKEL_BUILDER_TRACE(3, $"aB = {aB}");
            CGAL_STSKEL_BUILDER_TRACE(3, $"lA_LBorder = {lA_LBorder}");
            CGAL_STSKEL_BUILDER_TRACE(3, $"lA_RBorder = {lA_RBorder}");
            CGAL_STSKEL_BUILDER_TRACE(3, $"lB_LBorder = {lB_LBorder}");
            CGAL_STSKEL_BUILDER_TRACE(3, $"lB_RBorder = {lB_RBorder}");

            return (lA_LBorder == lB_LBorder && lA_RBorder == lB_RBorder)
                   || (lA_LBorder == lB_RBorder && lA_RBorder == lB_LBorder);
        }

        public
        void UpdatePQ(Vertex aNode, Triedge? aPrevEventTriedge)
        {
            Vertex lPrev = aNode.PrevInLAV;
            Vertex lNext = aNode.NextInLAV;

            CGAL_STSKEL_BUILDER_TRACE(3, $"Updating PQ for N{aNode.Id} Prev N{lPrev.Id} Next N{lNext.Id}");
            CGAL_STSKEL_BUILDER_TRACE(3, $"Respective positions {aNode.point()} Prev {lPrev.point()} Next {lNext.point()}");

            Halfedge lOBisector_P = lPrev.primary_bisector();
            Halfedge lOBisector_C = aNode.primary_bisector();
            Halfedge lOBisector_N = lNext.primary_bisector();

            // @todo it's pointless to collect for both the left and the right for contour nodes
            if (AreBisectorsCoincident(lOBisector_C, lOBisector_P))
                HandleSimultaneousEdgeEvent(aNode, lPrev);
            else if (AreBisectorsCoincident(lOBisector_C, lOBisector_N))
                HandleSimultaneousEdgeEvent(aNode, lNext);
            else
                CollectNewEvents(aNode, aPrevEventTriedge);
        }
        
     public  IEnumerable<SsbStepResult> CreateInitialEvents()
        {
            CGAL_STSKEL_BUILDER_TRACE(0, "Creating initial events...");

           

            foreach (Vertex v in mSSkel.Vertices)
            {
                if (!v.has_infinite_time())
                {
                    UpdatePQ(v, null);
                    mVisitor?.on_initial_events_collected(v, IsReflex(v), IsDegenerate(v));
                }
            }
           
            yield return new CreateInitialEventsSsbStepResult(null); 
        }

       

        public IEnumerable<SsbStepResult> HarmonizeSpeeds()
        {
           
            CGAL_STSKEL_BUILDER_TRACE(2, "Harmonize speeds...");
          
            Stopwatch wt = new Stopwatch();
            wt.Start();
           
              // Collinear input edges might not have the exact same speed if an inexact square root is used.
              // This might cause some inconsistencies in time, resulting in invalid skeletons. Therefore,
              // if the square root is not exact, we enforce that collinear input edges have the same speed,
              // by making them use the same line coefficients (which determines the speed of the front).
              //
              // That is achieved by creating a set of input edges, with two input edges being equal if they are collinear.
              // If a new input edge is not successfully inserted into the same, it takes the line coefficients
              // of the representative of this class.

              bool comparer (Halfedge lLH, Halfedge lRH)
              {
                Direction2 lLD = CreateDirection(lLH) ;
                Direction2 lRD = CreateDirection(lRH) ;
                CompareResultEnum rRes = (CompareResultEnum) compare_angle_with_x_axis(lLD, lRD) ;

                if ( rRes == CompareResultEnum.EQUAL) // parallel
                {
                  if ( orientation(lLH.Vertex.point(),
                                   lLH.Opposite.Vertex.point(),
                                   lRH.Vertex.point()) ==  OrientationEnum.COLLINEAR)
                    return false; // collinear

                  // parallel but not collinear, order arbitrarily (but consistently)
                  return compare_xy(lLH.Vertex.point(), lRH.Vertex.point())== (int) CompareResultEnum.SMALLER ;
                }
                else
                {
                  // not parallel
                  return ( rRes == CompareResultEnum.SMALLER) ;
                }
              } ;
             /*
              typedef std::set<Halfedge, decltype(comparer)> Ordered_halfedges;
              Ordered_halfedges lOrdered_halfedges(comparer);

              typename CGAL_SS_i::Get_protector<Gt>::type protector;
              CGAL_USE(protector);

              for( Face_iterator fit = mSSkel.SSkel::Base::faces_begin(); fit != mSSkel.SSkel::Base::faces_end(); ++fit)
              {
                Halfedge lBorder = fit.halfedge() ;
                auto lRes = lOrdered_halfedges.insert(lBorder);
                if(!lRes.second) // successful insertion (i.e., not collinear to any previously inserted halfedge)
                {
                  CGAL_STSKEL_BUILDER_TRACE(4, "Harmonize " << lBorder.Id << " with " << (*lRes.first).Id ) ;
                  InitializeLineCoeffs(lBorder.Id, (*lRes.first).Id);
                }
                else
                {
                  const Segment_2 lBS = CreateSegment(lBorder);
                  InitializeLineCoeffs(lBS);
                }
              }
            */
            wt.Stop();
            yield return new HarmonizeSpeedsSsbStepResult(null);
        }

        public IEnumerable<SsbStepResult> InitPhase()
        {
            Stopwatch wt = new Stopwatch();
     //         CreateContourBisectors();
            foreach (var e in HarmonizeSpeeds()) yield return e;
            foreach (var e in  CreateInitialEvents()) yield return e;
            mVisitor?.on_initialization_finished();
            
           
        }

        Vertex ConstructEdgeEventNode(EdgeEvent aEvent)
        {
           
            Stopwatch wt = new Stopwatch();
            wt.Start();
            CGAL_STSKEL_BUILDER_TRACE(2, "Creating EdgeEvent Node");

            Vertex lLSeed = aEvent.seed0();
            Vertex lRSeed = aEvent.seed1();

            Vertex lNewNode = mSSkel.Add(new Vertex(mVertexID++, aEvent.point(), aEvent.time(), false, false));
            InitVertexData(lNewNode);

            GLAV_push_back(lNewNode);

            SetTrisegment(lNewNode, aEvent.trisegment());

            SetIsProcessed(lLSeed);
            SetIsProcessed(lRSeed);
            GLAV_remove(lLSeed);
            GLAV_remove(lRSeed);

            Vertex lLPrev = lLSeed.PrevInLAV;
            Vertex lRNext = lRSeed.NextInLAV;

            lNewNode.PrevInLAV= lLPrev;
            lLPrev.NextInLAV = lNewNode;

            lNewNode.NextInLAV = lRNext;
            lRNext.PrevInLAV = lNewNode;

            CGAL_STSKEL_BUILDER_TRACE(2, $"New Node: N{lNewNode.Id} at {lNewNode.point()}",
                                          $"N{lLSeed.Id} removed from LAV",
                                          $"N{lRSeed.Id} removed from LAV");

            return lNewNode;
        }

        VertexPair LookupOnSLAV(Halfedge aBorder, Event aEvent, out Site rSite)
        {
            VertexPair rResult = new VertexPair(Vertex.NULL, Vertex.NULL);

            bool lFound = false;
            rSite = Site.NAN;
            // Vertex lSeed = aEvent.seed0();

            CGAL_STSKEL_BUILDER_TRACE(3, "Looking up for E{aBorder.Id}. P={aEvent.point()}");

            foreach (Vertex v in GetHalfedgeLAVList(aBorder))
            {
                Triedge lTriedge = GetVertexTriedge(v);

                Vertex lPrevN = v.PrevInLAV;
                Vertex lNextN = v.NextInLAV;

                if (lTriedge.e0() == aBorder)
                {
                    Halfedge lPrevBorder = GetEdgeEndingAt(lPrevN);
                    Halfedge lNextBorder = GetEdgeEndingAt(lNextN);

                    CGAL_STSKEL_BUILDER_TRACE(3, $"Subedge found in SLAV: N{lPrevN.Id}.N{v.Id} (E{lPrevBorder.Id}.E{aBorder.Id}.E{lNextBorder.Id})");

                    OrientedSideEnum lLSide = (OrientedSideEnum)(int)EventPointOrientedSide(aEvent, lPrevBorder, aBorder, lPrevN, false);
                    OrientedSideEnum lRSide = (OrientedSideEnum)(int)EventPointOrientedSide(aEvent, aBorder, lNextBorder, v, true);

                    if (lLSide != OrientedSideEnum.ON_POSITIVE_SIDE && lRSide != OrientedSideEnum.ON_NEGATIVE_SIDE)
                    {
                        if (lLSide != OrientedSideEnum.ON_ORIENTED_BOUNDARY || lRSide != OrientedSideEnum.ON_ORIENTED_BOUNDARY)
                        {
                            rSite = (lLSide == OrientedSideEnum.ON_ORIENTED_BOUNDARY ? Site.AT_SOURCE
                                                                                      : (lRSide == OrientedSideEnum.ON_ORIENTED_BOUNDARY ? Site.AT_TARGET
                                                                                                                                          : Site.INSIDE
                                                                                        )
                                    );
                            lFound = true;
                            rResult = new VertexPair(lPrevN, v);

                            CGAL_STSKEL_BUILDER_TRACE(3, $"Split point found at the {(rSite == Site.AT_SOURCE ? "SOURCE vertex" : (rSite == Site.AT_TARGET ? "TARGET vertex" : "strict inside"))} of the offset edge {lPrevN} {v}");

                            break;
                        }
                        else
                        {
                            CGAL_STSKEL_BUILDER_TRACE(3, "Opposite edge collapsed to a point");
                        }
                    }
                }
            }


  if ( !handle_assigned(rResult.first) )
  {
    if ( !lFound )
    {
      CGAL_STSKEL_BUILDER_TRACE(1,"Split event is no longer valid. Opposite edge vanished.");
    }
    else
    {
      CGAL_STSKEL_BUILDER_TRACE(1,"Split event is no longer valid. Point not inside the opposite edge's offset zone.");
    }
  }


            return rResult;
        }

        public VertexPair ConstructSplitEventNodes(SplitEvent aEvent, Vertex aOppR)
        {
            VertexPair rResult;

            CGAL_STSKEL_BUILDER_TRACE(2, "Creating SplitEvent Nodes");

            Vertex lOppL = aOppR.PrevInLAV;

            Vertex lNewNodeA = mSSkel.Add(new Vertex(mVertexID++, aEvent.point(), aEvent.time(), true, false));
            Vertex lNewNodeB = mSSkel.Add(new Vertex(mVertexID++, aEvent.point(), aEvent.time(), true, false));

            InitVertexData(lNewNodeA);
            InitVertexData(lNewNodeB);
            SetTrisegment(lNewNodeA, aEvent.trisegment());
            SetTrisegment(lNewNodeB, aEvent.trisegment());

            GLAV_push_back(lNewNodeA);
            GLAV_push_back(lNewNodeB);

            Vertex lSeed = aEvent.seed0();

            SetIsProcessed(lSeed);
            GLAV_remove(lSeed);

            CGAL_STSKEL_BUILDER_TRACE(2, $"N{lNewNodeA.Id} and $N{lNewNodeB.Id} inserted into LAV.");

            Vertex lPrev = lSeed.PrevInLAV;
            Vertex lNext = lSeed.NextInLAV;

            lPrev.NextInLAV = lNewNodeA;
            lNewNodeA.PrevInLAV = lPrev;

            lNewNodeA.NextInLAV = aOppR;
            aOppR.PrevInLAV = lNewNodeA;

            lOppL.NextInLAV = lNewNodeB;
            lNewNodeB.PrevInLAV = lOppL;

            lNewNodeB.NextInLAV = lNext;
            lNext.PrevInLAV = lNewNodeB;

            CGAL_STSKEL_BUILDER_TRACE(2, $"N{lSeed.Id} removed from LAV");

            rResult = new VertexPair(lNewNodeA, lNewNodeB);

            mSplitNodes.push_back(rResult);

            return rResult;
        }

        public VertexPair ConstructPseudoSplitEventNodes(PseudoSplitEvent aEvent)
        {
            VertexPair rResult;

            CGAL_STSKEL_BUILDER_TRACE(2, "Creating PseudoSplitEvent Nodes");

            Vertex lLSeed = aEvent.seed0();
            Vertex lRSeed = aEvent.seed1();

            Vertex lNewNodeA = mSSkel.Add(new Vertex(mVertexID++, aEvent.point(), aEvent.time(), true, false));
            Vertex lNewNodeB = mSSkel.Add(new Vertex(mVertexID++, aEvent.point(), aEvent.time(), true, false));

            GLAV_push_back(lNewNodeA);
            GLAV_push_back(lNewNodeB);

            InitVertexData(lNewNodeA);
            InitVertexData(lNewNodeB);
            SetTrisegment(lNewNodeA, aEvent.trisegment());
            SetTrisegment(lNewNodeB, aEvent.trisegment());

            SetIsProcessed(lLSeed);
            SetIsProcessed(lRSeed);
            GLAV_remove(lLSeed);
            GLAV_remove(lRSeed);

            Vertex lLPrev = lLSeed.PrevInLAV;
            Vertex lLNext = lLSeed.NextInLAV;
            Vertex lRPrev = lRSeed.PrevInLAV;
            Vertex lRNext = lRSeed.NextInLAV;

            lNewNodeA.PrevInLAV = lLPrev;
            lLPrev.NextInLAV = lNewNodeA;

            lNewNodeA.NextInLAV = lRNext;
            lRNext.PrevInLAV = lNewNodeA;

            lNewNodeB.PrevInLAV = lRPrev;
            lRPrev.NextInLAV = lNewNodeB;

            lNewNodeB.NextInLAV= lLNext;
            lLNext.PrevInLAV = lNewNodeB;

            CGAL_STSKEL_BUILDER_TRACE(2, $"NewNodeA: N{lNewNodeA.Id} at {lNewNodeA.point()}",
                                         $"NewNodeB: N{lNewNodeB.Id} at {lNewNodeB.point()}",
                                         $"N{lLSeed.Id} removed from LAV",
                                         $"N{lRSeed.Id} removed from LAV"
                                     );

            rResult = new VertexPair(lNewNodeA, lNewNodeB);

            mSplitNodes.push_back(rResult);

            return rResult;
        }

        public bool IsProcessed(Event aEvent)
        {
            CGAL_STSKEL_BUILDER_TRACE(4, $"Event is processed? V{aEvent.seed0().Id}: {IsProcessed(aEvent.seed0())} V{aEvent.seed1().Id}: IsProcessed(aEvent.seed1()) ");

            return IsProcessed(aEvent.seed0()) || IsProcessed(aEvent.seed1());
        }

        public
        bool IsValidEvent(Event aEvent)
        {
            if (IsProcessed(aEvent))
                return false;

            SetEventTimeAndPoint(aEvent);

            if (aEvent is EdgeEvent edgeEvent)
            {
                return IsValidEdgeEvent(edgeEvent);
            }
            else if (aEvent is SplitEvent splitEvent)
            {
                Halfedge lOppEdge = aEvent.triedge().e2();
                Site lSite;
                VertexPair lOpp = LookupOnSLAV(lOppEdge, aEvent, out lSite);

                if (handle_assigned(lOpp.first))
                {
                    var lPseudoSplitEvent = IsPseudoSplitEvent(aEvent, lOpp, lSite);
                    if (lPseudoSplitEvent != null)
                    {
                        return IsValidPseudoSplitEvent(lPseudoSplitEvent);
                    }
                    else
                    {
                        return IsValidSplitEvent(splitEvent);
                    }
                }
                else
                {
                    return false;
                }
            }
            else if (aEvent is PseudoSplitEvent pseudoSplitEvent)
            {
                return IsValidPseudoSplitEvent(pseudoSplitEvent);
            }
            else
            {
                throw new Exception($"{aEvent} in this point must by a {nameof(PseudoSplitEvent)}");
            }
        }

        bool IsValidEdgeEvent(EdgeEvent aEvent)
        {
            Vertex lLSeed = aEvent.seed0();
            Vertex lRSeed = aEvent.seed1();

            Vertex lPrevLSeed = lLSeed.PrevInLAV;
            Vertex lNextRSeed = lRSeed.NextInLAV;

            if (lPrevLSeed != lNextRSeed)
            {
                Halfedge lPrevE0 = GetEdgeEndingAt(lPrevLSeed);
                Halfedge lE0 = aEvent.triedge().e0();
                Halfedge lE2 = aEvent.triedge().e2();
                Halfedge lNextE2 = GetEdgeStartingAt(lNextRSeed);

                CGAL_STSKEL_BUILDER_TRACE(3, $"PrevLSeed=N{lPrevLSeed.Id} PrevE0=E{lPrevE0.Id}");
                CGAL_STSKEL_BUILDER_TRACE(3, $"NextRSeed=N{lNextRSeed.Id} NextE2=E{lNextE2.Id}");

                CGAL_STSKEL_BUILDER_TRACE(3, $"- Check left E{lPrevE0.Id} E{lE0.Id} N{lPrevLSeed.Id}");
                OrientedSideEnum lLSide = (OrientedSideEnum)(int)EventPointOrientedSide(aEvent, lPrevE0, lE0, lPrevLSeed, false); // replace aE0isPrimary for false
                bool lLSideOK = (lLSide != OrientedSideEnum.ON_POSITIVE_SIDE);
                if (!lLSideOK)
                {
                    CGAL_STSKEL_BUILDER_TRACE(3, $"Invalid edge event on the left side: {aEvent.triedge()} NewNode is before E{lE0.Id}  source N{lPrevLSeed.Id}");

                    return false;
                }

                CGAL_STSKEL_BUILDER_TRACE(3, $"- Check right E{lE2.Id} E{lNextE2.Id} N{lNextRSeed.Id}");
                OrientedSideEnum lRSide = (OrientedSideEnum)(int)EventPointOrientedSide(aEvent, lE2, lNextE2, lNextRSeed, true);// replace aE0isPrimary for false

                bool lRSideOK = (lRSide != OrientedSideEnum.ON_NEGATIVE_SIDE);
                if (!lRSideOK)
                {
                    CGAL_STSKEL_BUILDER_TRACE(3, $"Invalid edge event on the right side: {aEvent.triedge()} NewNode is past E{lE2.Id} target N{lNextRSeed.Id}");
                }

                return lRSideOK; // lLSideOK is `true` if we are here
            }
            else
            {
                // Triangle collapse. No need to test explicitly.
                return true;
            }
        }

        void HandleEdgeEvent(EdgeEvent lEvent)
        {
            CGAL_STSKEL_BUILDER_TRACE(2, "\n== Edge event.");

            if (IsValidEdgeEvent(lEvent))
            {
                Vertex lLSeed = lEvent.seed0();
                Vertex lRSeed = lEvent.seed1();

                CGAL_STSKEL_BUILDER_TRACE(3, "valid event.");

                CGAL_STSKEL_BUILDER_TRACE(4, $"TriEdge e0 = {lEvent.triedge().e0()} ");
                CGAL_STSKEL_BUILDER_TRACE(4, $"TriEdge e1 = {lEvent.triedge().e1()} ");
                CGAL_STSKEL_BUILDER_TRACE(4, $"TriEdge e2 = {lEvent.triedge().e2()} ");

                Vertex lNewNode = ConstructEdgeEventNode(lEvent);

                Halfedge lLOBisector = lLSeed.primary_bisector();
                Halfedge lROBisector = lRSeed.primary_bisector();
                Halfedge lLIBisector = lLOBisector.Opposite;
                Halfedge lRIBisector = lROBisector.Opposite;

                Vertex lRIFicNode = lROBisector.Vertex;
                Vertex lLOFicNode = lLOBisector.Vertex;

                CrossLink(lLOBisector, lNewNode);

                Link(lROBisector, lNewNode);

                CrossLinkFwd(lROBisector, lLIBisector);

                Halfedge lDefiningBorderA = validate(lNewNode.halfedge().defining_contour_edge());
                Halfedge lDefiningBorderB = validate(lNewNode.halfedge().Opposite.Prev.Opposite.defining_contour_edge());
                Halfedge lDefiningBorderC = validate(lNewNode.halfedge().Opposite.Prev.defining_contour_edge());

                Triedge lTri = new Triedge(lDefiningBorderA, lDefiningBorderB, lDefiningBorderC);
                SetVertexTriedge(lNewNode, lTri);

                SetBisectorSlope(lLSeed, lNewNode);
                SetBisectorSlope(lRSeed, lNewNode);

                CGAL_STSKEL_BUILDER_TRACE(1, $"{lRSeed.halfedge().defining_contour_edge()} collapsed.");
                CGAL_STSKEL_BUILDER_TRACE(3, $"fictitious node along collapsed face is N{lRIFicNode.Id} between {lROBisector} and lLIBisector");

                if (lLOFicNode.has_infinite_time())
                {
                    CGAL_STSKEL_BUILDER_TRACE(3, "Creating new Edge Event's Bisector");

                 
                    CreateNewEdge(out var lNOBisector, out var lNIBisector);

                    CrossLinkFwd(lNOBisector, lLOBisector.Next);
                    CrossLinkFwd(lRIBisector.Prev, lNIBisector);

                    CrossLink(lNOBisector, lLOFicNode);

                    SetBisectorSlope(lNOBisector, SignEnum.POSITIVE);
                    SetBisectorSlope(lNIBisector, SignEnum.NEGATIVE);

                    CrossLinkFwd(lNIBisector, lRIBisector);
                    CrossLinkFwd(lLOBisector, lNOBisector);

                    Link(lNOBisector, lLOBisector.Face);
                    Link(lNIBisector, lRIBisector.Face);

                    Link(lNIBisector, lNewNode);

                    CGAL_STSKEL_BUILDER_TRACE(2, $"{lNewNode},{lTri}");
                    CGAL_STSKEL_BUILDER_TRACE(2, $"O{lNOBisector}");
                    CGAL_STSKEL_BUILDER_TRACE(2, $"I{lNIBisector}");

                    CGAL_STSKEL_BUILDER_TRACE(2, $"Fictitious N{lRIFicNode.Id} erased.");
                    EraseNode(lRIFicNode);

                    SetupNewNode(lNewNode);

                    UpdatePQ(lNewNode, lEvent.triedge());

                    mVisitor?.on_edge_event_processed(lLSeed, lRSeed, lNewNode);
                }
                else
                {
                    CGAL_STSKEL_BUILDER_TRACE(2, $"{lNewNode},{lTri}.",
                                                  $"This is a multiple node (A node with these defining edges already exists in the LAV)"
                                             );
                }

                CGAL_STSKEL_BUILDER_TRACE(1, $"Wavefront: {wavefront2str(lNewNode)}");
            }
        }

        bool IsValidSplitEvent(SplitEvent aEvent)
        {
            return true;
        }

        void HandleSplitEvent(SplitEvent lEvent, VertexPair aOpp)
        {
            CGAL_STSKEL_BUILDER_TRACE(2, "Split event.");

            if (IsValidSplitEvent(lEvent))
            {
                CGAL_STSKEL_BUILDER_TRACE(3, "valid event.");

                Vertex lSeed = lEvent.seed0();

                Vertex lOppL = validate(aOpp.first);
                Vertex lOppR = validate(aOpp.last);
                //CGAL_USE(lOppL);

                Halfedge lOppOBisector_R = lOppR.primary_bisector();
                Halfedge lOppIBisector_L = lOppOBisector_R.Next;

                Vertex lOppFicNode = lOppOBisector_R.Vertex;
                //(void)lOppFicNode; // variable may be unused

                CGAL_STSKEL_BUILDER_TRACE(4, $"TriEdge e0 = {lEvent.triedge().e0()}");
                CGAL_STSKEL_BUILDER_TRACE(4, $"TriEdge e1 = {lEvent.triedge().e1()}");
                CGAL_STSKEL_BUILDER_TRACE(4, $"TriEdge e2 = {lEvent.triedge().e2()}");
                CGAL_STSKEL_BUILDER_TRACE(4, $"lOppL = {lOppL}");
                CGAL_STSKEL_BUILDER_TRACE(4, $"lOppR = {lOppR}");
                CGAL_STSKEL_BUILDER_TRACE(4, $"lOppLPrimary = {lOppL.primary_bisector()}");
                CGAL_STSKEL_BUILDER_TRACE(4, $"lOppRPrimary = {lOppR.primary_bisector()}");

                CGAL_STSKEL_BUILDER_TRACE(2, $"Split face: N{lOppR.Id}.B{lOppOBisector_R.Id}.N{lOppFicNode.Id}.B{lOppIBisector_L.Id}.N{lOppL.Id}");

                CGAL_STSKEL_BUILDER_TRACE(2, $"fictitious node for right half of opposite edge: N{lOppFicNode.Id}");

                CGAL_assertion(lOppFicNode.has_infinite_time());

                Halfedge lOppBorder = lEvent.triedge().e2();

                Vertex lNewNode_L, lNewNode_R;
                (lNewNode_L, lNewNode_R) = ConstructSplitEventNodes(lEvent, lOppR);

                // Triedge lTriedge = aEvent.triedge();

                // Halfedge lReflexLBorder = lTriedge.e0();
                // Halfedge lReflexRBorder = lTriedge.e1();



                CreateNewEdge(out var lNOBisector_L, out var lNIBisector_L);
                CreateNewEdge(out var lNOBisector_R, out var lNIBisector_R);

                Halfedge lXOBisector = lSeed.primary_bisector();
                Halfedge lXIBisector = lXOBisector.Opposite;

                Halfedge lXONextBisector = lXOBisector.Next;
                Halfedge lXIPrevBisector = lXIBisector.Prev;

                Vertex lXOFicNode = lXOBisector.Vertex;
                CGAL_assertion(lXOFicNode.has_infinite_time());

                CGAL_STSKEL_BUILDER_TRACE(2, $"fictitious node for left reflex face: N{lXOFicNode.Id}");
                CGAL_STSKEL_BUILDER_TRACE(2, $"fictitious node for right reflex face: N{lXIPrevBisector.Vertex.Id}");

                Link(lNewNode_L, lXOBisector);
                Link(lNewNode_R, lNIBisector_L);

                Link(lXOBisector, lNewNode_L);

                Link(lNOBisector_L, lXOBisector.Face);
                Link(lNIBisector_L, lOppBorder.Face);
                Link(lNOBisector_R, lOppBorder.Face);
                Link(lNIBisector_R, lXIBisector.Face);

                Link(lNIBisector_L, lNewNode_R);
                Link(lNIBisector_R, lNewNode_R);

                Link(lNOBisector_L, lXOFicNode);

                CrossLinkFwd(lXOBisector, lNOBisector_L);
                CrossLinkFwd(lNOBisector_L, lXONextBisector);
                CrossLinkFwd(lXIPrevBisector, lNIBisector_R);
                CrossLinkFwd(lNIBisector_R, lXIBisector);
                CrossLinkFwd(lOppOBisector_R, lNIBisector_L);
                CrossLinkFwd(lNIBisector_L, lNOBisector_R);
                CrossLinkFwd(lNOBisector_R, lOppIBisector_L);

                SetBisectorSlope(lSeed, lNewNode_L);

                Vertex lNewFicNode = mSSkel.Add(new Vertex(mVertexID++));

                InitVertexData(lNewFicNode);
                CGAL_assertion(lNewFicNode.has_infinite_time());
                CrossLink(lNOBisector_R, lNewFicNode);

                SetBisectorSlope(lNOBisector_L, SignEnum.POSITIVE);
                SetBisectorSlope(lNIBisector_L, SignEnum.NEGATIVE);
                SetBisectorSlope(lNOBisector_R, SignEnum.POSITIVE);
                SetBisectorSlope(lNIBisector_R, SignEnum.NEGATIVE);

                CGAL_STSKEL_BUILDER_TRACE(2, $"(New) fictitious node for left half of opposite edge: N{lNewFicNode.Id}");

                Halfedge lNewNode_L_DefiningBorderA = validate(lNewNode_L.halfedge().defining_contour_edge());
                Halfedge lNewNode_L_DefiningBorderB = validate(lNewNode_L.halfedge().Opposite.Prev.Opposite.defining_contour_edge());
                Halfedge lNewNode_L_DefiningBorderC = validate(lNewNode_L.halfedge().Opposite.Prev.defining_contour_edge());
                Halfedge lNewNode_R_DefiningBorderA = validate(lNewNode_R.halfedge().defining_contour_edge());
                Halfedge lNewNode_R_DefiningBorderB = validate(lNewNode_R.halfedge().Opposite.Prev.Opposite.defining_contour_edge());
                Halfedge lNewNode_R_DefiningBorderC = validate(lNewNode_R.halfedge().Opposite.Prev.defining_contour_edge());

                Triedge lTriL = new Triedge(lNewNode_L_DefiningBorderA, lNewNode_L_DefiningBorderB, lNewNode_L_DefiningBorderC);
                Triedge lTriR = new Triedge(lNewNode_R_DefiningBorderA, lNewNode_R_DefiningBorderB, lNewNode_R_DefiningBorderC);

                SetVertexTriedge(lNewNode_L, lTriL);
                SetVertexTriedge(lNewNode_R, lTriR);

                CGAL_STSKEL_BUILDER_TRACE(2, $"L{lNewNode_L} {lTriL}",
                                            $"R{lNewNode_R} {lTriR}",
                                            $"OL{lNOBisector_L}",
                                            $"IL{lNIBisector_L}",
                                            $"OR{lNOBisector_R}",
                                            $"IR{lNIBisector_R}"
                                         );

                CGAL_STSKEL_BUILDER_TRACE(1, $"Wavefronts:\n {wavefront2str(lNewNode_L)}\n  {wavefront2str(lNewNode_R)}");

                SetupNewNode(lNewNode_L);
                SetupNewNode(lNewNode_R);

                UpdatePQ(lNewNode_L, lEvent.triedge());
                UpdatePQ(lNewNode_R, lEvent.triedge());

                mVisitor?.on_split_event_processed(lSeed, lNewNode_L, lNewNode_R);
            }
        }

        public
        void SetupNewNode(Vertex aNode)
        {
            // In an edge-edge annihilation the current polygon becomes a two-node degenerate chain collapsed into a single point
            if (aNode.PrevInLAV != aNode.NextInLAV)
            {
                Halfedge lLE = GetEdgeEndingAt(aNode);
                Halfedge lRE = GetEdgeStartingAt(aNode);

                Vector2 lLV = CreateVector(lLE);
                Vector2 lRV = CreateVector(lRE);

                OrientationEnum lOrientation = orientation(lLV, lRV);
                if (lOrientation == OrientationEnum.COLLINEAR)
                {
                    SetIsDegenerate(aNode);
                    CGAL_STSKEL_BUILDER_TRACE(1, $"COLLINEAR *NEW* vertex: {aNode} (E{lLE.Id},E{lRE.Id})");
                }
                else if (lOrientation == OrientationEnum.RIGHT_TURN)
                {
                    mReflexVertices.push_back(aNode);
                    SetIsReflex(aNode);
                    CGAL_STSKEL_BUILDER_TRACE(1, "Reflex *NEW* vertex: N{aNode} (E{lLE.Id},E{lRE.Id})");
                }
            }
        }

        bool counterclockwise_at_or_in_between_2(in Direction2 p, in Direction2 q, in Direction2 r)
        {
            return p == q || p == r || counterclockwise_in_between_2(p, q, r);
        }

        bool IsValidPseudoSplitEvent(PseudoSplitEvent aEvent)
        {
            Vertex lSeed0 = aEvent.seed0();
            Vertex lSeed1 = aEvent.seed1();

            CGAL_STSKEL_BUILDER_TRACE(3, $"Checking for tangleness...");
            CGAL_STSKEL_BUILDER_TRACE(3, $"lSeed0 = {lSeed0}");
            CGAL_STSKEL_BUILDER_TRACE(3, $"lSeed1 = {lSeed1}");

            Halfedge lEL0 = GetEdgeEndingAt(lSeed0);
            Halfedge lER0 = GetEdgeStartingAt(lSeed0);

            Halfedge lEL1 = GetEdgeEndingAt(lSeed1);
            Halfedge lER1 = GetEdgeStartingAt(lSeed1);

            CGAL_STSKEL_BUILDER_TRACE(3, $"lEL0 = {lEL0}");
            CGAL_STSKEL_BUILDER_TRACE(3, $"lER0 = {lER0}");
            CGAL_STSKEL_BUILDER_TRACE(3, $"lEL1 = {lEL1}");
            CGAL_STSKEL_BUILDER_TRACE(3, $"lER1 = {lER1}");

            Direction2 lDL0 = -CreateDirection(lEL0);
            Direction2 lDL1 = -CreateDirection(lEL1);
            Direction2 lDR0 = CreateDirection(lER0);
            Direction2 lDR1 = CreateDirection(lER1);

            bool lV01Degenerate = (lDL0 == lDR1);
            bool lV10Degenerate = (lDL1 == lDR0);

            CGAL_STSKEL_BUILDER_TRACE(3, $"Validating pseudo-split event. Resulting re-connection: ",
                                     $"E{lEL0.Id} [DL0:{lDL0}].E{lER1.Id} [DR1:{lDR1}]{(lV01Degenerate ? " (degenerate)" : "")}",
                                     $"E{lEL1.Id} [DL1:{lDL1}].E{lER0.Id} [DR0:{lDR0}]{(lV10Degenerate ? " (degenerate)" : "")}"
                                     );

            bool lTangled;

            if (!lV01Degenerate)
            {
                bool lEL1V_Tangled = counterclockwise_at_or_in_between_2(lDL1, lDR1, lDL0);
                bool lER0V_Tangled = counterclockwise_at_or_in_between_2(lDR0, lDR1, lDL0);

                CGAL_STSKEL_BUILDER_TRACE(3, $"lV01Degenerate not degenerate, CCW DL1,DR1,DL0 = {lEL1V_Tangled}");
                CGAL_STSKEL_BUILDER_TRACE(3, $"lV01Degenerate not degenerate, CCW DR0,DR1,DL0 = {lER0V_Tangled}");

                lTangled = lEL1V_Tangled || lER0V_Tangled;
            }
            else if (!lV10Degenerate)
            {
                bool lEL0V_Tangled = counterclockwise_at_or_in_between_2(lDL0, lDR0, lDL1);
                bool lER1V_Tangled = counterclockwise_at_or_in_between_2(lDR1, lDR0, lDL1);

                CGAL_STSKEL_BUILDER_TRACE(3, $"lV10Degenerate not degenerate, CCW DL0,DR0,DL1 = {lEL0V_Tangled}");
                CGAL_STSKEL_BUILDER_TRACE(3, $"lV10Degenerate not degenerate, CCW DR1,DR0,DL1 = {lER1V_Tangled}");

                lTangled = lEL0V_Tangled || lER1V_Tangled;
            }
            else
            {
                CGAL_STSKEL_BUILDER_TRACE(3, $"Both degenerate, tangled = {(lDL0 == lDL1)}");

                lTangled = (lDL0 == lDL1);
            }

            CGAL_STSKEL_BUILDER_TRACE_IF(lTangled, 3, "Tangled profile. Pseudo-split event is invalid");

            return !lTangled;
        }

        void HandlePseudoSplitEvent(PseudoSplitEvent lEvent)
        {
            CGAL_STSKEL_BUILDER_TRACE(2, "Pseudo split event.");

            if (IsValidPseudoSplitEvent(lEvent))
            {
                CGAL_STSKEL_BUILDER_TRACE(3, "valid event.");

                Vertex lLSeed = lEvent.seed0();
                Vertex lRSeed = lEvent.seed1();

                CGAL_STSKEL_BUILDER_TRACE(4, $"TriEdge e0 = {lEvent.triedge().e0()}");
                CGAL_STSKEL_BUILDER_TRACE(4, $"TriEdge e1 = {lEvent.triedge().e1()}");
                CGAL_STSKEL_BUILDER_TRACE(4, $"TriEdge e2 = {lEvent.triedge().e2()}");
                CGAL_STSKEL_BUILDER_TRACE(4, $"LSeed = {lLSeed}");
                CGAL_STSKEL_BUILDER_TRACE(4, $"LReed = {lRSeed}");

                Vertex lNewNode_L, lNewNode_R;
                (lNewNode_L, lNewNode_R) = ConstructPseudoSplitEventNodes(lEvent);

                CreateNewEdge(out var lNBisector_LO, out var lNBisector_LI);
                CreateNewEdge(out var lNBisector_RO, out var lNBisector_RI);



                Halfedge lSBisector_LO = lLSeed.primary_bisector();
                Halfedge lSBisector_LI = lSBisector_LO.Opposite;

                Halfedge lSBisector_RO = lRSeed.primary_bisector();
                Halfedge lSBisector_RI = lSBisector_RO.Opposite;

                Halfedge lSBisector_LO_Next = lSBisector_LO.Next;
                Halfedge lSBisector_RO_Next = lSBisector_RO.Next;
                Halfedge lSBisector_LI_Prev = lSBisector_LI.Prev;
                Halfedge lSBisector_RI_Prev = lSBisector_RI.Prev;

                Vertex lFicNod_SLO = lSBisector_LO.Vertex;
                //CGAL_assertion_code(Vertex lFicNod_SLI = lSBisector_LI_Prev.Vertex;) // unused
                Vertex lFicNod_SRO = lSBisector_RO.Vertex;
                //CGAL_assertion_code(Vertex lFicNod_SRI = lSBisector_RI_Prev.Vertex;) // unused

                CGAL_assertion(lFicNod_SLO.has_infinite_time());
                // CGAL_assertion( lFicNod_SLI.has_infinite_time() ) ;
                CGAL_assertion(lFicNod_SRO.has_infinite_time());
                // CGAL_assertion( lFicNod_SRI.has_infinite_time() ) ;

                Link(lNBisector_LO, lSBisector_LO.Face);
                Link(lNBisector_LI, lSBisector_RI.Face);
                Link(lNBisector_RO, lSBisector_RO.Face);
                Link(lNBisector_RI, lSBisector_LI.Face);

                CrossLink(lSBisector_LO, lNewNode_L);
                CrossLink(lSBisector_RO, lNewNode_R);

                CrossLink(lNBisector_LO, lFicNod_SLO);
                CrossLink(lNBisector_RO, lFicNod_SRO);

                SetBisectorSlope(lNBisector_LO, SignEnum.POSITIVE);
                SetBisectorSlope(lNBisector_LI, SignEnum.NEGATIVE);
                SetBisectorSlope(lNBisector_RO, SignEnum.POSITIVE);
                SetBisectorSlope(lNBisector_RI, SignEnum.NEGATIVE);

                Link(lNBisector_LI, lNewNode_L);
                Link(lNBisector_RI, lNewNode_R);

                Link(lNewNode_L, lSBisector_LO);
                Link(lNewNode_R, lSBisector_RO);

                CrossLinkFwd(lSBisector_LO, lNBisector_LO);
                CrossLinkFwd(lNBisector_LO, lSBisector_LO_Next);
                CrossLinkFwd(lSBisector_LI_Prev, lNBisector_RI);
                CrossLinkFwd(lNBisector_RI, lSBisector_LI);
                CrossLinkFwd(lSBisector_RI_Prev, lNBisector_LI);
                CrossLinkFwd(lNBisector_LI, lSBisector_RI);
                CrossLinkFwd(lSBisector_RO, lNBisector_RO);
                CrossLinkFwd(lNBisector_RO, lSBisector_RO_Next);

                SetBisectorSlope(lLSeed, lNewNode_L);
                SetBisectorSlope(lRSeed, lNewNode_R);

                Halfedge lNewNode_L_DefiningBorderA = validate(lNewNode_L.halfedge().defining_contour_edge());
                Halfedge lNewNode_L_DefiningBorderB = validate(lNewNode_L.halfedge().Next.Opposite.defining_contour_edge());
                Halfedge lNewNode_L_DefiningBorderC = validate(lNewNode_L.halfedge().Opposite.Prev.defining_contour_edge());
                Halfedge lNewNode_R_DefiningBorderA = validate(lNewNode_R.halfedge().defining_contour_edge());
                Halfedge lNewNode_R_DefiningBorderB = validate(lNewNode_R.halfedge().Next.Opposite.defining_contour_edge());
                Halfedge lNewNode_R_DefiningBorderC = validate(lNewNode_R.halfedge().Opposite.Prev.defining_contour_edge());

                Triedge lTriL = new Triedge(lNewNode_L_DefiningBorderA, lNewNode_L_DefiningBorderB, lNewNode_L_DefiningBorderC);
                Triedge lTriR = new Triedge(lNewNode_R_DefiningBorderA, lNewNode_R_DefiningBorderB, lNewNode_R_DefiningBorderC);

                SetVertexTriedge(lNewNode_L, lTriL);
                SetVertexTriedge(lNewNode_R, lTriR);

                CGAL_STSKEL_BUILDER_TRACE(2, $"L{lNewNode_L},{lTriL}",
                                             $"R{lNewNode_R},{lTriR}",
                                             $"OL{lNBisector_LO}",
                                             $"IL{lNBisector_LI}",
                                             $"OR{lNBisector_RO}",
                                             $"IR{lNBisector_RI}"
                                         );

                CGAL_STSKEL_BUILDER_TRACE(1, "Wavefronts:", wavefront2str(lNewNode_L), wavefront2str(lNewNode_R));

                SetupNewNode(lNewNode_L);
                SetupNewNode(lNewNode_R);

                UpdatePQ(lNewNode_L, lEvent.triedge());
                UpdatePQ(lNewNode_R, lEvent.triedge());

                mVisitor?.on_pseudo_split_event_processed(lLSeed, lRSeed, lNewNode_L, lNewNode_R);
            }
        }

        void HandleSplitOrPseudoSplitEvent(Event aEvent)
        {
            Halfedge lOppEdge = aEvent.triedge().e2();

            CGAL_STSKEL_BUILDER_TRACE(2, "Split or Pseudo split event.");
            CGAL_STSKEL_BUILDER_TRACE(3, "Opposite edge: {lOppEdge}");

            Site lSite;

            VertexPair lOpp = LookupOnSLAV(lOppEdge, aEvent, out lSite);

            if (handle_assigned(lOpp.first))
            {
                var lPseudoSplitEvent = IsPseudoSplitEvent(aEvent, lOpp, lSite);
                if (lPseudoSplitEvent != null)
                    HandlePseudoSplitEvent(lPseudoSplitEvent);
                else if (aEvent is SplitEvent splitEvent)
                    HandleSplitEvent(splitEvent, lOpp);
                else throw new Exception("No Handler");
            }
        }

        void InsertNextSplitEventInPQ(Vertex v)
        {
            CGAL_STSKEL_BUILDER_TRACE(2, $"Insert split event from N{v.Id}, {v.SplitEvents.Count} potential splits");

            var lSplitEvent = PopNextSplitEvent(v);
            if (lSplitEvent != null)
                InsertEventInPQ(lSplitEvent);
        }

        void InsertNextSplitEventsInPQ()
        {
            CGAL_STSKEL_BUILDER_TRACE(2, "Insert next split events...");
            foreach (var v in mReflexVertices)
                if (!IsProcessed(v))
                    InsertNextSplitEventInPQ(v);
        }

   public    IEnumerable<SsbStepResult>  Propagate()
        {
            CGAL_STSKEL_BUILDER_TRACE(0, "Propagating events...");
            mVisitor?.on_propagation_started();
            
            for (; ; )
            {
                
                InsertNextSplitEventsInPQ();

                if (mPQ.Count > 0)
                {
#if CGAL_SLS_PRINT_QUEUE_BEFORE_EACH_POP
      CGAL_STSKEL_BUILDER_TRACE(4, "MAIN QUEUE -------------------------------------------------- ");
      CGAL_STSKEL_BUILDER_TRACE(4, "Queue size: " << mPQ.size());
      auto mpq = mPQ;
      while(!mpq.empty())
      {
        Event event = mpq.top();
        mpq.pop();
        CGAL_STSKEL_BUILDER_TRACE(4, *event);
      }
      CGAL_STSKEL_BUILDER_TRACE(4, "END MAIN QUEUE --------------------------------------------- ");
#endif

                    Event lEvent = PopEventFromPQ();

                    if (lEvent is EdgeEvent edgeEvent)
                        AllowNextSplitEvent(edgeEvent.seed0());

                    CGAL_STSKEL_BUILDER_TRACE(3, " ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ ");
                    CGAL_STSKEL_BUILDER_TRACE(3, $"\tS{mStepID} Tentative Event: {lEvent}");

                    

                    if (!IsProcessed(lEvent))
                    {
                        CGAL_STSKEL_BUILDER_TRACE(1, $"\tS{mStepID} Event: {lEvent}");

                        SetEventTimeAndPoint(lEvent);
                        
                        switch (lEvent)
                        {
                            case EdgeEvent edge: HandleEdgeEvent(edge); break;
                            case SplitEvent split: HandleSplitOrPseudoSplitEvent(split); break;
                            case PseudoSplitEvent pseudo: HandlePseudoSplitEvent(pseudo); break;
                            default: CGAL_assertion(false); break;
                        }

                        ++mStepID;
                        yield return new EnventProcessStepResult(null, lEvent);
                    }
                    else
                    {
                        CGAL_STSKEL_BUILDER_TRACE(3, "\nAlready processed");
                    }
                }
                else break;
            }

            mVisitor?.on_propagation_finished();
            yield break;
        }

        void MergeSplitNodes(VertexPair aSplitNodes)
        {
            Vertex lLNode, lRNode;
            (lLNode, lRNode) = aSplitNodes;

            Halfedge lIBisectorL1 = lLNode.primary_bisector().Opposite;
            Halfedge lIBisectorR1 = lRNode.primary_bisector().Opposite;
            Halfedge lIBisectorL2 = lIBisectorL1.Next.Opposite;
            Halfedge lIBisectorR2 = lIBisectorR1.Next.Opposite;

            CGAL_STSKEL_BUILDER_TRACE(2,
                       $"Merging SplitNodes: (L) N{lLNode.Id} and (R) N{lRNode.Id}",
                       $"  LOut: B{lLNode.primary_bisector().Id}",
                       $"  ROut: B{lRNode.primary_bisector().Id}",
                       $"  LIn1: B{lIBisectorL1.Id}",
                       $"  RIn1: B{lIBisectorR1.Id}",
                       $"  LIn2: B{lIBisectorL2.Id}",
                       $"  RIn2: B{lIBisectorR2.Id}"
                       );

            if (lIBisectorL1.Vertex == lRNode)
                lIBisectorL1.Vertex = lLNode;

            if (lIBisectorR1.Vertex == lRNode)
                lIBisectorR1.Vertex = lLNode;

            if (lIBisectorL2.Vertex == lRNode)
                lIBisectorL2.Vertex = lLNode;

            if (lIBisectorR2.Vertex == lRNode)
                lIBisectorR2.Vertex = lLNode;

            CGAL_STSKEL_BUILDER_TRACE(2
                                  , $"  N{lRNode.Id} removed."
                                  , $"  LIn1 B{lIBisectorL1.Id} now linked to N {lIBisectorL1.Vertex.Id}"
                                  , $"  RIn1 B{lIBisectorR1.Id} now linked to N {lIBisectorR1.Vertex.Id}"
                                  , $"  LIn2 B{lIBisectorL2.Id} now linked to N {lIBisectorL2.Vertex.Id}"
                                  , $"  RIn2 B{lIBisectorR2.Id} now linked to N {lIBisectorR2.Vertex.Id}"
                                 );

            EraseNode(lRNode);
        }

        public void EraseNode(Vertex aNode)
        {
            aNode.reset_id__internal__(-aNode.Id);
            mSSkel.Remove(aNode);
        }

        //#if CGAL_STRAIGHT_SKELETON_ENABLE_TRACE

        void TraceMultinode(string pre, Halfedge b, Halfedge e)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(pre);
            sb.Append($"before: B{b.Prev.Id} N{b.Prev.Vertex.Id} Pt: {b.Prev.Vertex.point()}\n");
            do
            {
                sb.Append($"B{b.Id} N{b.Vertex.Id} Pt: {b.Vertex.point()}\n");
                b = b.Next;
            }
            while (b != e);

            sb.Append($"after: B{b.Id} N{b.Vertex.Id} Pt: {b.Vertex.point()}\n");

            CGAL_STSKEL_BUILDER_TRACE(0, sb.ToString());
        }

        double angle_wrt_X(in Point2 a, in Point2 b)
        {
            double dx = (b.x() - a.x());
            double dy = (b.y() - a.y());
            double atan = Math.Atan2(dy, dx);
            double rad = atan >= 0.0 ? atan : 2.0 * PI + atan;
            double deg = rad * 180.0 / PI;
            return deg;
        }

        void TraceFinalBisectors(Vertex v, IEnumerable<Halfedge> cb)
        {
            foreach (var c in cb)
            {
                double phi = angle_wrt_X(c.Vertex.point(), c.Opposite.Vertex.point());

                CGAL_STSKEL_BUILDER_TRACE(2, $"  N{v.Id} in=B{c.Id} E{c.defining_contour_edge().Id}  out=B{c.Opposite.Id} E{c.Opposite.defining_contour_edge().Id} phi={phi}");
            }
        }

        bool ValidateFinalBisectorsAfterMerge(Vertex v, IEnumerable<Halfedge> cb)
        {
            bool rOK = true;

            foreach (var c in cb)
            {
                if (c.defining_contour_edge() != c.Prev.defining_contour_edge())
                    rOK = false;
            }
            return rOK;
        }

        void RelinkBisectorsAroundMultinode(Vertex v0, IEnumerable<Halfedge> aLinks)
        {
            CGAL_assertion(aLinks.Count() > 0);

            CGAL_STSKEL_BUILDER_TRACE(4, $"Relinking {aLinks.Count()} bisectors around N{v0.Id}");

            // Connect the bisectors with each other following the CCW ordering

            Halfedge first_he = aLinks.First();
            Halfedge prev_he = first_he;

            first_he.Vertex = v0;

            Halfedge prev_he_opp;
            foreach (var he in aLinks.Skip(1))
            {
                he.Vertex = v0;

                prev_he_opp = prev_he.Opposite;

                he.Next = prev_he_opp;
                prev_he_opp.Prev = he;

                CGAL_STSKEL_BUILDER_TRACE(4, $"Relinking B{he.Id}.B{prev_he_opp.Id}");

                prev_he = he;
            }

            prev_he_opp = prev_he.Opposite;

            first_he.Next = prev_he_opp;
            prev_he_opp.Prev = first_he;

            CGAL_STSKEL_BUILDER_TRACE(4, $"Relinking B{first_he.Id}.B{prev_he_opp.Id}");

            // Reset the main halfedge for v0
            v0.Halfedge = first_he;

            TraceFinalBisectors(v0, v0.Circulation());

            DebuggerInfo.CGAL_postcondition(ValidateFinalBisectorsAfterMerge(v0, v0.Circulation()));
        }

        public
        void PreprocessMultinode(Multinode aMN)
        {
            //
            // A Multinode is a run of coincident nodes along a face.
            // Its represented by a pair of halfedges describing a linear profile.
            // The first halfedge in the pair points to the first node in the multinode.
            // Each .Next halfedge in the profile points to a subsequent node.
            // The second halfedge in the pair is past-the-end (it points to the first node around the face that IS NOT part of the multinode)
            //

            // Halfedge oend = validate(aMN.end.Opposite);

            TraceMultinode("Preprocessing multinode:\n", aMN.begin, aMN.end);

            Halfedge h = aMN.begin;

            aMN.bisectors_to_relink.push_back(h);

            // Traverse the profile collecting:
            //  The nodes to be removed from the HDS (all but the first)
            //  The bisectors to be removed from the HDS (each bisector pointing to the next node in the multinode)
            //  The bisectors around each node that must be relinked to the first node (which will be kept in place of the multinode)
            do
            {
                ++aMN.size;
                Halfedge nx = validate(h.Next);
                if (nx != aMN.end)
                    aMN.bisectors_to_remove.push_back(nx);

                // Since each halfedge "h" in this lineal profile corresponds to a single face, all the bisectors around
                // each node which must be relinked are those found ccw between h and h.Next
                Halfedge ccw = h;
                Halfedge ccw_end = validate(h.Next.Opposite);
                for (; ; )
                {
                    ccw = validate(ccw.Opposite.Prev);
                    if (ccw != ccw_end)
                        aMN.bisectors_to_relink.push_back(ccw);
                    else
                        break;
                }

                if (h != aMN.begin)
                    aMN.nodes_to_remove.push_back(h.Vertex);

                h = nx;
            }
            while (h != aMN.end);

            aMN.bisectors_to_relink.push_back(aMN.end.Opposite);
        }

        //
        // Replaces a run of coincident nodes with a single one by removing all but the first, removing node-to-node bisectors and
        // relinking the other bisectors.
        //

        void ProcessMultinode(Multinode aMN, List<Halfedge> rBisectorsToRemove, List<Vertex> rNodesToRemove)

        {
            bool lDoNotProcess = false;

            Halfedge h = aMN.begin;

            do
            {
                if (h.Vertex.has_infinite_time() || IsExcluded(h.Vertex))
                    lDoNotProcess = true;
                h = h.Next;
            }
            while (!lDoNotProcess && h != aMN.end);

            if (!lDoNotProcess)
            {
                TraceMultinode("Processing multinode: ", aMN.begin, aMN.end);

                h = aMN.begin;
                do
                {
                    Exclude(h.Vertex);
                    h = h.Next;
                }
                while (h != aMN.end);

                rBisectorsToRemove.AddRange(aMN.bisectors_to_remove);

                foreach (var vi in aMN.nodes_to_remove)
                    rNodesToRemove.push_back(vi);

                RelinkBisectorsAroundMultinode(aMN.v, aMN.bisectors_to_relink);
            }
        }

        Multinode CreateMultinode(Halfedge begin, Halfedge end)
        {
            return new Multinode(begin, end);
        }

        //
        // Finds coincident skeleton nodes and merge them
        //
        // If moving edges Ei,Ej collide with moving edge Ek causing Ej to collapse, Ei and Ek becomes consecutive and a new
        // polygon vertex (Ei,Ek) appears in the wavefront.
        // If moving edges Ei,Ej collide with moving edge Ek causing Ek to be split in two halves, L(Ek) amd R(Ek) resp, two new
        // polygon vertices appears in the wavefront; namely: (Ei,R(Ek)) and (L(Ek),Ej))
        // If moving edge Ei,Ej collide with both Ek,El simultaneously causing the edges to cross-connect, two new vertices
        // (Ei,Ek) and (El,Ej) appear in the wavefront.
        //
        // In all those 3 cases, each new polygon vertex is represented in the straight skeleton as a skeleton node.
        // Every skeleton node is describing the collision of at least 3 edges (called the "defining edges" of the node)
        // and it has at least 3 incident bisectors, each one pairing 2 out of the total number of defining edges.
        //
        // Any skeleton node has a degree of at least 3, but if more than 3 edges collide simultaneously, the corresponding
        // skeleton node has a higher degree. (the degree of the node is exactly the number of colliding edges)
        //
        // However, the algorithm handles the coallison of 3 edges at a time so each skeleton node initially created
        // has degree exactly 3 so this function which detects higher degree nodes and merge them into a single node
        // of the proper degree is needed.
        //
        // Two skeleton nodes are "coincident" IFF they have 2 defining edges in common and each triedge of edges collide
        // at the same time and point. IOW, 2 nodes are coincident if they represent the simultaneous
        // coallison of exactly 4 edges (the union of 2 triedges with 2 common elements is a set of 4).
        //

        bool MergeCoincidentNodes()
        {
            //
            // NOTE: This code might be executed on a topologically inconsistent HDS, thus the need to check
            // the structure along the way.
            //

            CGAL_STSKEL_BUILDER_TRACE(0, "Merging coincident nodes...");

            // ALGORITHM DESCRIPTION:
            //
            // While circulating the bisectors along the face for edge Ei we find all those edges E* which
            // are or become consecutive to Ei during the wavefront propagation. Each bisector along the face:
            // (Ei,Ea), (Ei,Eb), (Ei,Ec), etc pairs Ei with such other edge.
            // Between one bisector (Ei,Ea) and the next (Ei,Eb) there is skeleton node which represents
            // the collision between the 3 edges (Ei,Ea,Eb).
            // It follows from the pairing that any skeleton node Ni, for example (Ei,Ea,Eb), necessarily
            // shares two edges (Ei and Eb precisely) with any next skeleton node Ni+1 around the face.
            // That is, the triedge of defining edges that correspond to each skeleton node around the face follow this
            // sequence: (Ei,Ea,Eb), (Ei,Eb,Ec), (Ei,Ec,Ed), ...
            //
            // Any 2_ consecutive_ skeleton nodes around a face share 2 out of the 3 defining edges, which is one of the
            // necessary conditions for "coincidence". Therefore, coincident nodes can only come as consecutive along a face
            //

            List<Multinode> lMultinodes = new List<Multinode>();

            foreach (var fit in mSSkel.Faces)
            {
                // 'h' is the first (CCW) skeleton halfedge.
                Halfedge h = validate(validate(fit.halfedge()).Next);

                CGAL_assertion(h.IsBisector);

                // 'last' is the last (CCW) skeleton halfedge
                Halfedge last = validate(fit.halfedge().Prev);

                CGAL_assertion(last.IsBisector);
                CGAL_assertion(last.Vertex.is_contour());

                Halfedge h0 = h;
                Vertex v0 = validate(h0.Vertex);
                CGAL_assertion(handle_assigned(v0));

                if (!v0.has_infinite_time())
                {
                    CGAL_assertion(v0.IsSkeleton);

                    h = validate(h.Next);

                    while (h != last)
                    {
                        Vertex v = validate(h.Vertex);

                        if (!v.has_infinite_time())
                        {
                            CGAL_assertion(v.IsSkeleton);

                            if (!(bool)AreSkeletonNodesCoincident(v0, v))
                            {
                                if (h0.Next != h)
                                    lMultinodes.push_back(CreateMultinode(h0, h));

                                v0 = v;
                                h0 = h;
                            }
                        }

                        h = validate(h.Next);
                    }

                    if (h0.Next != h)
                        lMultinodes.push_back(CreateMultinode(h0, h));
                }
            }

            if (lMultinodes.Count == 0)
                return false;

            //
            // The merging loop removes all but one of the coincident skeleton nodes and the halfedges between them.
            // But it can't physically erase those from the HDS while looping, so the nodes/bisector to erase
            // are collected in these sequences are erased after the merging loop.
            //
            List<Halfedge> lBisectorsToRemove = new List<Halfedge>();
            List<Vertex> lNodesToRemove = new List<Vertex>();

            foreach (var it in lMultinodes)
                PreprocessMultinode(it);

            lMultinodes.Sort(new MultinodeComparer());

            foreach (var it in lMultinodes)
                ProcessMultinode(it, lBisectorsToRemove, lNodesToRemove);

            if (lBisectorsToRemove.Count == 0)
                return false;

            foreach (var hi in lBisectorsToRemove)
            {
                CGAL_STSKEL_BUILDER_TRACE(1, $"B{hi.Id} removed.");
                hi.reset_id(-1);
                mSSkel.edges_erase(hi);
            }

            foreach (var vi in lNodesToRemove)
                EraseNode(vi);

            foreach (var vit in mSSkel.Vertices)
                vit.IsExcluded = false;

            return true;
        }

        // For weighted skeletons of polygons with holes, one can create non-simply-connected skeleton faces.
        // This is a problem both because it is not a valid HDS, and because we walk skeleton face borders
        // in polygon offseting. We add so-called artificial nodes and bisectors to ensure that faces
        // are simply-connected by shooting rays from the topmost vertex of the bisectors of the skeleton
        // of the hole(s).

        void EnforceSimpleConnectedness()
        {
            CGAL_STSKEL_BUILDER_TRACE(1, "Ensuring simple connectedness...");

            // Associates to contour halfedges a range of holes, each hole being represented by a halfedge
            // pointing to the vertex farthest from the contour halfedge.
            List<Halfedge>[] skeleton_face_holes = new List<Halfedge>[mContourHalfedges.Count];

            var max_id = mSSkel.Halfedges.Count() - 1;
            //TODO Optimizar con array y usar indice
            HashSet<Halfedge> visited_halfedges = new HashSet<Halfedge>(mSSkel.Halfedges.Count());

            foreach (Halfedge h in mSSkel.Halfedges)
            {
                if (h.IsBorder)
                    continue;

                if (visited_halfedges.Contains(h))
                    continue;

                // Walk the halfedge cycle; if it doesn't contain its contour halfedge, then it is a hole
                // in the skeleton face.

                Halfedge contour_h = validate(h.defining_contour_edge());

                bool is_contour_h_in_boundary = false;

                Halfedge done = h, ih = h;
                do
                {
                    visited_halfedges.Add(h);

                    if (ih == contour_h)
                        is_contour_h_in_boundary = true;

                    ih = ih.Next;
                }
                while (ih != done);

                if (is_contour_h_in_boundary)
                    continue;

                CGAL_STSKEL_BUILDER_TRACE(4, $"Face incident to border E{contour_h.Id} has a hole at E{h.Id}");

                // This is a hole, find the vertex of the hole that is farthest from the defining border
                // as to ensure that the artificial bisectors will not intersect the hole
                Halfedge extreme_h = h;
                ih = h;
                do
                {
                    CompareResultEnum res = (CompareResultEnum)(int)CompareEvents(h.Vertex, extreme_h.Vertex);
                    if (res == CompareResultEnum.LARGER)
                        extreme_h = ih;

                    ih = h.Next;
                }
                while (ih != done);

                CGAL_STSKEL_BUILDER_TRACE(4, $"Extremum: E{extreme_h.Id} V{extreme_h.Vertex.Id}");

                // contour halfedges are the first N *even* halfedges
                skeleton_face_holes[contour_h.Id / 2].push_back(extreme_h);
            }

            // For each face with hole(s), create the extra halfedges to bridge the gap between
            // the skeleton face's border and the holes by shooting a ray from a vertex hole to a halfedge
            //   .first is the source of the ray
            //   .second is th event creating the intersection of the ray with an halfedge

            // Collect first for all faces, apply later because one might split
            List<(Halfedge first, Event second)> artifical_events = new List<(Halfedge first, Event second)>();

            foreach (var contour_hi in mContourHalfedges)
            {
                Halfedge contour_h = contour_hi;
                List<Halfedge> holes = skeleton_face_holes[contour_h.Id / 2];

                if (holes==null || holes.Count == 0)
                    continue;

                Direction2 orth_dir = CreatePerpendicularDirection(contour_h);

                CGAL_STSKEL_BUILDER_TRACE(4, $"E{contour_h.Id} has {holes.Count} hole(s).");

                // First, order the holes such that extreme points (one by hole) are from closest
                // to farthest from the contour border
                holes.Sort(
                          (Halfedge h1, Halfedge h2) => (int)CompareEvents(h1.Vertex, h2.Vertex));

                // Shoot a ray from each hole's extreme point to find the closest halfedge of another hole
                // or of the face's main contour
                for (var ih = 0; ih < holes.Count; ih++)
                {
                    var hole_hi = holes[ih];
                    Halfedge extreme_h = hole_hi;

                    Ray2 r = new Ray2(extreme_h.Vertex.point(), orth_dir);

                    CGAL_STSKEL_BUILDER_TRACE(4, $"Shooting ray from {extreme_h.Vertex.point()}");

                    Event? closest_artificial_event = null;

                    void test_halfedge(Halfedge h)
                    {
                        // @partial_wsls_pwh Don't compute an intersection with a halfedge incident to a fictitious vertex
                        if (h.Vertex.has_infinite_time() || h.Prev.Vertex.has_infinite_time())
                            return;

                        Triedge artificial_triedge = new Triedge(contour_h, contour_h, h);
                        Trisegment artificial_trisegment = CreateTrisegment(artificial_triedge, extreme_h.Vertex);
                        CGAL_assertion(artificial_trisegment.child_l() != Trisegment.NULL);

                        if (!(bool)ExistEvent(artificial_trisegment))
                            return;

                        Event artificial_event = new ArtificialEvent(artificial_triedge, artificial_trisegment, extreme_h.Vertex);
                        if (closest_artificial_event == null ||
                         (int)CompareEvents(artificial_event, closest_artificial_event) == (int)CompareResultEnum.SMALLER)
                        {
                            closest_artificial_event = artificial_event;
                        }
                    };

                    // Seek an intersection with other holes. Only need to check holes that are farther
                    // than the one we are tracing from.

                    for (var jh = ih + 1; jh < holes.Count; jh++)
                    {
                        var hole_hj = holes[jh];

                        // @todo same spatial searching required as in filtering bound computations
                        Halfedge other_hole_done = hole_hj, other_hole_h = hole_hj;
                        do
                        {
                            test_halfedge(other_hole_h);
                            other_hole_h = other_hole_h.Next; // next halfedge of the hole
                        }
                        while (other_hole_h != other_hole_done);

                        // can't break yet: a yet farther hole might have a closer intersection
                    }

                    // Whether an intersection has been found or not, we still need to check with the border
                    // of the face, because it could be pinched and be closer than a hole
                    Halfedge done = contour_h, h = contour_h.Next; // no point checking the contour edge
                    do
                    {
                        test_halfedge(h);
                        h = h.Next;
                    }
                    while (h != done);

                    // @partial_wsls_pwh support partial weighted skeleton of polygons with holes
                    // In partial skeletons, we might the face (and possibly even holes!) are not closed,
                    // so we could potentially not find a halfedge
                    Debug.Assert(closest_artificial_event != null);
                    if (closest_artificial_event != null)
                    {
                        SetEventTimeAndPoint(closest_artificial_event);
                        artifical_events.Add((extreme_h, closest_artificial_event));
                    }
                } // holes
            } // contours

            CGAL_STSKEL_BUILDER_TRACE(2, $"{artifical_events.Count} artificial events to add");

            int artifical_events_comparer((Halfedge first, Event second) e1, (Halfedge first, Event second) e2)
            {
                Event artificial_event_1 = e1.second;
                Event artificial_event_2 = e2.second;

                Halfedge split_h_1 = artificial_event_1.triedge().e2();
                Halfedge split_h_2 = artificial_event_2.triedge().e2();
                Halfedge canonical_split_h_1 = (split_h_1.Id < split_h_1.Opposite.Id) ? split_h_1 : split_h_1.Opposite;
                Halfedge canonical_split_h_2 = (split_h_2.Id < split_h_2.Opposite.Id) ? split_h_2 : split_h_2.Opposite;

                bool is_same_edge = (canonical_split_h_1 == canonical_split_h_2);
                if (!is_same_edge)
                    return canonical_split_h_1.Id.CompareTo(canonical_split_h_2.Id); // arbitrary

                Halfedge contour_h = validate(canonical_split_h_1.defining_contour_edge());

                // @fixme this should be a predicate...
                Point_2 split_p_1 = artificial_event_1.point();
                Point_2 split_p_2 = artificial_event_2.point();

                // TODO TOCHECK
                return (-(int)orientation(contour_h.Vertex.point(), split_p_1, split_p_2));
            }
            // Splits need to be sorted right to left such that consecutive splits do not tangle the HDS
            // Since we can have splits from both side of an edge, we sort events globally
            artifical_events.Sort(artifical_events_comparer);

            // Apply the splits

            //
            //
            //        <----------------                     <---------   <----------
            //            split_h                             split_h | | split_h_prev
            //                                              new_up_h | | new_down_h
            //              .                                        | |
            //            /  \                ----.                /  \
            //extreme_h /     \                          extreme_h /     \
            //        /        \                                 /        \
            //

            foreach (var e in artifical_events)
            {
                Halfedge extreme_h = e.first;
                Event artificial_event = e.second;

                // Halfedge contour_h = artificial_event.triedge().e0() ;

                Halfedge split_h = artificial_event.triedge().e2();
                Halfedge split_h_prev = split_h.Prev;
                Halfedge split_h_next = split_h.Next;
                Halfedge split_h_opp = split_h.Opposite;
                Halfedge split_h_opp_prev = split_h_opp.Prev;
                Halfedge split_h_opp_next = split_h_opp.Next;

                Vertex split_h_sv = split_h_opp.Vertex;
                Vertex split_h_tv = split_h.Vertex;

                Halfedge extreme_h_next = extreme_h.Next;

                Point_2 split_p = artificial_event.point();
                FT split_t = artificial_event.time();

                CGAL_STSKEL_BUILDER_TRACE(4, $"Splitting E{split_h.Id} at pos {split_p} time {split_t}");

                // Create the new "vertical" (w.r.t. the contour edge) halfedges
                CreateNewEdge(out var new_up, out var new_down);
                (var new_up_h, var new_down_h) =(new_up, new_down);
             
                SetBisectorSlope(new_up_h, SignEnum.POSITIVE);
                SetBisectorSlope(new_down_h, SignEnum.NEGATIVE);

                new_down_h.set_vertex(extreme_h.Vertex);
                new_up_h.set_face(extreme_h.Face);
                new_down_h.set_face(extreme_h.Face);

                new_down_h.Next = extreme_h_next;
                extreme_h_next.Prev = new_down_h;
                extreme_h.Next = new_up_h;
                new_up_h.Prev = extreme_h;

                if (split_p == split_h_tv.point())
                {
                    new_up_h.Vertex = split_h_tv;

                    new_up_h.Next = split_h_next;
                    split_h_next.Prev = new_up_h;

                    split_h.Next = new_down_h;
                    new_down_h.Prev = split_h;
                }
                else if (split_p == split_h_sv.point())
                {
                    new_up_h.Vertex = split_h_sv;

                    new_up_h.Next = split_h;
                    split_h.Prev = new_up_h;

                    new_down_h.Prev = split_h_prev;
                    split_h_prev.Next = new_down_h;
                }
                else
                {
                    // Create the new artificial vertex
                    Vertex new_v = mSSkel.Add(new Vertex(mVertexID++, split_p, split_t, false, false));
                    InitVertexData(new_v);

                    // This is not a valid triedge because split_h is not a contour halfedge, but we need
                    // to know which skeleton bisector the line orthogonal to contour_h interscets.
                    // The pair of identical contour halfedges at e0 and e1 is the marker for artifical vertices
                    SetVertexTriedge(new_v, artificial_event.triedge());
                    SetTrisegment(new_v, artificial_event.trisegment());

                    new_up_h.Vertex = new_v;
                    new_v.Halfedge = new_up_h;

                    // Split the halfedge

                  //  Halfedge new_horizontal = new Halfedge(mEdgeID++), new_horizontal_opp = new Halfedge(mEdgeID++); // split new prev
                   
                    CreateNewEdge(out var new_horizontal, out var new_horizontal_opp);

                    var new_hor_h = new_horizontal;

                    SetBisectorSlope(new_hor_h, split_h.slope());
                    SetBisectorSlope(new_hor_h.Opposite, split_h_opp.slope());

                    new_hor_h.set_face(split_h.Face);
                    new_hor_h.Opposite.set_face(split_h_opp.Face);

                    // A skeleton edge might be split from both sides, so we have previously ordered
                    // these artificial vertices from the right to the left, and we want split_h/split_h_opp
                    // to remain on the leftmost when looking from the canonical halfedge's side.
                    // Thus:
                    // - if we are on the canonical halfedge, split_h is on the left after the split
                    // - if we are not on the canonical halfedge, split_h is on the right after the split
                    Halfedge left_h, right_h;
                    if (split_h.Id < split_h_opp.Id)
                    {
                        left_h = split_h;
                        right_h = new_hor_h;
                    }
                    else
                    {
                        left_h = new_hor_h;
                        right_h = split_h;
                    }

                    Halfedge left_h_opp = left_h.Opposite;
                    Halfedge right_h_opp = right_h.Opposite;

                    CGAL_STSKEL_BUILDER_TRACE(4, $"Split actors"
                                              , $"  left_h: H{left_h.Id}"
                                              , $"  right_h: H{right_h.Id}"
                                              , $"  left_h_opp: H{left_h_opp.Id}"
                                              , $"  right_h_opp: H{right_h_opp.Id}"
                                              );

                    // Update the canonical halfedge of the vertices only if they are not contour vertices
                    if (!split_h_sv.is_contour())
                        split_h_sv.Halfedge = right_h_opp;
                    if (!split_h_tv.is_contour())
                        split_h_tv.Halfedge = left_h;

                    // vertex incidences
                    right_h.Vertex = new_v;
                    right_h_opp.Vertex = split_h_sv;
                    left_h.Vertex = split_h_tv;
                    left_h_opp.Vertex = new_v;

                    new_up_h.Next = left_h;
                    left_h.Prev = new_up_h;

                    left_h.Next = split_h_next;
                    split_h_next.Prev = left_h;

                    split_h_opp_prev.Next = left_h_opp;
                    left_h_opp.Prev = split_h_opp_prev;

                    left_h_opp.Next = right_h_opp;
                    right_h_opp.Prev = left_h_opp;

                    right_h_opp.Next = split_h_opp_next;
                    split_h_opp_next.Prev = right_h_opp;

                    split_h_prev.Next = right_h;
                    right_h.Prev = split_h_prev;

                    right_h.Next = new_down_h;
                    new_down_h.Prev = right_h;

                    CGAL_STSKEL_BUILDER_TRACE(4, $"New vertex V{new_v.Id} at position {new_v.point()} [between V{split_h_sv.Id} and V{split_h_tv.Id}] at time {new_v.time()}  [between {split_h_sv.time()} and {split_h_tv.time()}]");
                }
            }
        }

     public  IEnumerable<SsbStepResult>  FinishUp()
        {
            CGAL_STSKEL_BUILDER_TRACE(0, "\n\nFinishing up...");

            mVisitor?.on_cleanup_started();

            foreach (var p in mSplitNodes)
            {
                this.MergeSplitNodes(p);
            }

            // MergeCoincidentNodes() locks all extremities of halfedges that have a vertex involved in a multinode.
            // However, both extremities might have different (combinatorially and geometrically) vertices.
            // With a single pass, it would prevent one of the extremities from being properly simplified.
            // The simplest is to just run it again as the skeleton structure is small compared to the rest
            // of the algorithm.
            for (; ; )
            {
                if (!MergeCoincidentNodes())
                    break;
            }

            // For weighted polygons with holes, some faces might not be simply connected. In this case,
            // add extra, manually constructed bisectors to ensure that this property is present in all faces
            EnforceSimpleConnectedness();

            mVisitor?.on_cleanup_finished();

            // If 'mMaxTime' is sufficiently large, it will be a full skeleton and could be validated as such...
            bool result; 
            if (mMaxTime != null) // might be a partial skeleton
            {
                result =  mSSkel.IsValid(true);
            }
            else
            {
                result= mSSkel.IsValid(false);
            }
            yield return  new FinishUpStepResult(null);
            
        }

       public   IEnumerable<SsbStepResult>       Run()
        {
            foreach( var e in InitPhase()) yield return e;
            //foreach (var e in Propagate())
            //    yield return e;
            foreach (var e in FinishUp()) yield return e; ;
        }
        public StraightSkeleton ConstructSkeleton(bool aNull_if_failed = false)
        {
            bool ok = false;

            try
            {
                foreach (var e in Run())
                {
                };
            }
            catch (Exception e)
            {
                mVisitor?.on_error(e);
                CGAL_STSKEL_BUILDER_TRACE(0, $"EXCEPTION THROWN ({e}) during straight skeleton construction.");
            }

            if (!ok)
            {
                CGAL_STSKEL_BUILDER_TRACE(0, "Invalid result.");

                if (aNull_if_failed)
                    mSSkel = new StraightSkeleton();
            }

            mVisitor?.on_algorithm_finished(ok);

            return mSSkel;
        }
    }
}