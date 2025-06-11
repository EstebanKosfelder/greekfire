using CGAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using static CGAL.DebuggerInfo;
using static CGAL.Mathex;
using Vertex_handle_pair = CGAL.VertexPair;
namespace CGAL
{
    using Segment_2 = CGAL.Segment2;
    using Point_2 = CGAL.Point2;
    using Line_2 = CGAL.Line2;
    using FT = double;
    using Trisegment_2 = CGAL.Trisegment;

    // TODO TOCHECK inverse result;
    public class SplitEventCompare : IComparer<CGAL.Event>
    {
        public SplitEventCompare(StraightSkeletonBuilder aBuilder, CGAL.Vertex aV) { mBuilder = aBuilder; mV = aV; }

        private StraightSkeletonBuilder mBuilder;
        private CGAL.Vertex mV;
        public int Compare(CGAL.Event? aA, CGAL.Event? aB)
        {
            if (aA == null) throw new ArgumentNullException(nameof(aA));
            if (aB == null) throw new ArgumentNullException(nameof(aB));
            if (aA is EdgeEvent) throw new ArgumentException($"can't be {nameof(EdgeEvent)} instance ", nameof(aA));
            if (aB is EdgeEvent) throw new ArgumentException($"can't be {nameof(EdgeEvent)} instance ", nameof(aB));

            CGAL_precondition(aA.type() != CGAL.Event.Type.cEdgeEvent || aB.type() != CGAL.Event.Type.cEdgeEvent);

            CompareResultEnum result;
            if (!(bool)mBuilder.AreEventsSimultaneous(aA, aB))
            {
                CompareResultEnum res = (CompareResultEnum)(int)mBuilder.CompareEvents(aA, aB);
                if (res == CompareResultEnum.EQUAL)
                {

                    return aA.Id.CompareTo(aB.Id);
                }

                return (int)res;
            }

            // There are simultaneous events, we will need to refresh the queue before calling top()
            // see PopNextSplitEvent()
            mV.mHasSimultaneousEvents = true;

            // Priority queue comparison: `A` has higher priority than `B` if `operator()(A, B)` is `false`.
            // We want to give priority to smaller angles, so we must return `false` if the angle is smaller
            // i.e. `true` if the angle is larger
            result = mBuilder.CompareEventsSupportAngles(aA, aB);
            if (result == CompareResultEnum.EQUAL)
            {
                // TO Validate
                throw new NotImplementedException("para revisar si esta bien");
                return aA.GetHashCode().CompareTo(aB.GetHashCode());
            }
            return (int)result;
        }
    }

    public partial class StraightSkeletonBuilder
    {
        private IStraightSkeletonBuilderVisitor? mVisitor;
        List<CGAL.Vertex> mReflexVertices = new List<Vertex>();
        List<Halfedge> mContourHalfedges = new List<Halfedge>();

        List<VertexPair> mSplitNodes = new List<Vertex_handle_pair>();
        List<CGAL.Vertex> mVertexData = new List<Vertex>();
        Event_compare mEventCompare;
        public FT? mFilteringBound = null;
        int mVertexID;

        int mFaceID;
        int mEventID;
        int mStepID;

        FT? mMaxTime;

        PriorityQueue<Event, Event> mPQ;

        private int TrisegmentCount = 0;


        bool CanSafelyIgnoreSplitEventImpl(CGAL.Event lEvent)
        {
            return CanSafelyIgnoreSplitEvent(lEvent);
        }
        void ComputeUpperBoundForValidSplitEventsImpl(CGAL.Vertex aNode,
                                                    IEnumerable<Halfedge> contour_halfedges
                                                   //boost::mpl::bool_<true>

                                                   )
        {
            ComputeFilteringBound(aNode, contour_halfedges);
        }

        void ComputeUpperBoundForValidSplitEvents(CGAL.Vertex aNode,
                                                  IEnumerable<Halfedge> contour_halfedges)
        {
            ComputeUpperBoundForValidSplitEventsImpl(aNode, contour_halfedges
                                                           //,typename CGAL_SS_i::has_Filters_split_events_tag<Traits>::type()
                                                           );
        }


        void Link(Halfedge aH, CGAL.Face aF) => aH.set_face(aF);

        void Link(Halfedge aH, CGAL.Vertex aV)
        {
            aH.Vertex = aV;
        }

        void Link(CGAL.Vertex aV, Halfedge aH) => aV.Halfedge = aH;

        void CrossLinkFwd(Halfedge aPrev, Halfedge aNext)
        {
            aPrev.Next = aNext;
            aNext.Prev = aPrev;
        }

        void CrossLink(Halfedge aH, CGAL.Vertex aV)
        {
            Link(aH, aV);
            Link(aV, aH);
        }


        public UncertainCompareResult oriented_side_of_event_point_wrt_bisector_2(Trisegment aEvent
                                          , CGAL.Segment2 aE0
                                          , FT aW0
                                          , CGAL.Segment2 aE1
                                          , FT aW1
                                          , Trisegment aE01Event
                                          , bool aE0isPrimary
                                          )
        {
            var rResult = oriented_side_of_event_point_wrt_bisectorC2(aEvent, aE0, aW0, aE1, aW1, aE01Event, aE0isPrimary);
            return rResult;
        }

        // Given an oriented 2D straight line segment 'e', computes the normalized coefficients (a,b,c)
        // of the supporting line, and weights them with 'aWeight'.
        //
        // POSTCONDITION: [a,b] is the leftward normal vector.
        // POSTCONDITION: In case of overflow, an empty optional<> is returned.

        public Line_2? _compute_normalized_line_coeffC2(in Segment_2 e)
        {
            bool finite = true;
            FT a = (0), b = (0), c = (0);

            if (e.source().y() == e.target().y())
            {
                a = 0;
                if (e.target().x() > e.source().x())
                {
                    b = 1;
                    c = -e.source().y();
                }
                else if (e.target().x() == e.source().x())
                {
                    b = 0;
                    c = 0;
                }
                else
                {
                    b = -1;
                    c = e.source().y();
                }

                CGAL_STSKEL_TRAITS_TRACE($"HORIZONTAL line; a={a}, b={b}, c={c}");
            }
            else if (e.target().x() == e.source().x())
            {
                b = 0;
                if (e.target().y() > e.source().y())
                {
                    a = -1;
                    c = e.source().x();
                }
                else if (e.target().y() == e.source().y())
                {
                    a = 0;
                    c = 0;
                }
                else
                {
                    a = 1;
                    c = -e.source().x();
                }

                CGAL_STSKEL_TRAITS_TRACE($"VERTICAL line; a={ a}, b={b}, c={c}");
            }
            else
            {
                FT sa = e.source().y() - e.target().y();
                FT sb = e.target().x() - e.source().x();
                FT l2 = square(sa) + square(sb);

                if (FT.IsFinite(l2))
                {
                    FT l = inexact_sqrt(l2);
                    a = sa / l;
                    b = sb / l;

                    c = -e.source().x() * a - e.source().y() * b;

                    CGAL_STSKEL_TRAITS_TRACE($"GENERIC line; sa={sa}, sb={sb}, nnorm²={l2}, norm={l}, a={a}, b={b}, c={c}");
                }
                else
                {
                    finite = false;
                }
            }

            if (finite)
                if (!FT.IsFinite(a) || !FT.IsFinite(b) || !FT.IsFinite(c))
                    finite = false;

            return finite ? new Line_2(a, b, c) : null;
        }

        public Line_2? compute_normalized_line_coeffC2(Segment_2 e)
        {
            CGAL_STSKEL_TRAITS_TRACE($"\n~~ Unweighted line coefficients for E{e.Id} ");

#if USING_CACHE
            if (e.CacheCoeff !=  null)
                return e.CacheCoeff;
#endif
            Line_2? rRes = _compute_normalized_line_coeffC2(e);

#if USING_CACHE

            e.CacheCoeff = rRes;
#endif

            return rRes;
        }

        //
        // Constructs a Trisegment_2 which stores 3 oriented straight line segments e0,e1,e2 along with their collinearity.
        //
        // NOTE: If the collinearity cannot be determined reliably, a null trisegment is returned.
        //

        Trisegment_2 construct_trisegment(in Segment_2 e0,
                              FT w0,
                              Segment_2 e1,
                              FT w1,
                              Segment_2 e2,
                              FT w2,
                              int id)
        {
            CGAL_STSKEL_TRAITS_TRACE($"~~  Construct ");
            CGAL_STSKEL_TRAITS_TRACE($"Segments E{e0.Id} E{e1.Id} E{e2.Id}");

            Trisegment_collinearity lCollinearity = trisegment_collinearity_no_exact_constructions(e0, e1, e2);

            return new Trisegment_2(e0, w0, e1, w1, e2, w2, lCollinearity, id);
        }

        public FT squared_distance(Point_2 a, Point_2 b) => a.X * b.X + a.Y * b.Y;
        // Given two oriented straight line segments e0 and e1 such that e-next follows e-prev, returns
        // the coordinates of the midpoint of the segment between e-prev and e-next.
        // NOTE: the edges can be oriented e0.e1 or e1.e0
        //
        // POSTCONDITION: In case of overflow an empty optional is returned.
        //

        public Point_2? compute_oriented_midpoint(in Segment_2 e0, Segment_2 e1)
        {
            CGAL_STSKEL_TRAITS_TRACE($"Computing oriented midpoint between:\ne0: {e0}\ne1: {e1}");

            FT delta01 = squared_distance(e0.target(), e1.source());
            if (FT.IsFinite(delta01) && Mathex.is_zero(delta01))
                return e0.target();

            FT delta10 = squared_distance(e1.target(), e0.source());
            if (FT.IsFinite(delta10) && Mathex.is_zero(delta10))
                return e1.target();

            bool ok = false;
            Point_2 mp = new Point_2(0, 0);

            if (FT.IsFinite(delta01) && FT.IsFinite(delta10))
            {
                if (delta01 <= delta10)
                    mp = midpoint(e0.target(), e1.source());
                else
                    mp = midpoint(e1.target(), e0.source());

                CGAL_STSKEL_TRAITS_TRACE($"\nmp={mp}");

                ok = FT.IsFinite(mp.x()) && FT.IsFinite(mp.y());
            }

            return ok ? mp : null;
        }

        public UncertainCompareResult oriented_side_of_event_point_wrt_bisectorC2(Trisegment @event,
                                              Segment_2 e0,
                                              FT w0,
                                              Segment_2 e1,
                                              FT w1,
                                              Trisegment v01_event, // can be null
                                              bool primary_is_0
                                              )
        {
            UncertainCompareResult rResult = UncertainCompareResult.indeterminate;

            try
            {
                Point_2 p = Mathex.validate(construct_offset_lines_isecC2(@event));

                Line_2 l0 = Mathex.validate(compute_weighted_line_coeffC2(e0, w0));
                Line_2 l1 = Mathex.validate(compute_weighted_line_coeffC2(e1, w1));

                CGAL_STSKEL_TRAITS_TRACE($"\n~~ Oriented side of point ");
                CGAL_STSKEL_TRAITS_TRACE($"p = {p} w.r.t. bisector of [E{e0.Id} {e0} {(primary_is_0 ? "*" : "")}, E{e1.Id} {e1}{(primary_is_0 ? "" : "*")}]");

                // Degenerate bisector?
                var are_edge = are_edges_parallelC2(e0, e1);
                if (certainly(are_edge))
                {
                    CGAL_STSKEL_TRAITS_TRACE("Bisector is not angular.");

                    // b01 is degenerate so we don't have an *angular bisector* but a *perpendicular* bisector.
                    // We need to compute the actual bisector line.
                    CGAL_assertion(v01_event != Trisegment_2.NULL || (v01_event == Trisegment_2.NULL && e0.target() == e1.source()));

                    Point_2 v01 = v01_event != Trisegment_2.NULL ? Mathex.validate(construct_offset_lines_isecC2(v01_event))
                                            : e1.source();

                    CGAL_STSKEL_TRAITS_TRACE($"v01={v01} {(v01_event != Trisegment_2.NULL ? " (from skeleton node)" : "")}");

                    // (a,b,c) is a line perpendicular to the primary edge through v01.
                    // If e0 and e1 are collinear this line is the actual perpendicular bisector.
                    //
                    // If e0 and e1 are parallel but not collinear (then necessarily facing each other) this line
                    // is NOT the bisector, but it serves to determine the side of the point (projected along
                    // the primary edge) w.r.t. vertex v01.

                    FT a, b, c;
                    perpendicular_through_pointC2(primary_is_0 ? l0.a() : l1.a()

                                                 , primary_is_0 ? l0.b() : l1.b()

                                                 , v01.x(), v01.y()

                                                 , out a, out b, out c

                                                 );

                    rResult = certified_side_of_oriented_lineC2(a, b, c, p.x(), p.y());

                    CGAL_STSKEL_TRAITS_TRACE($"Point is at {rResult} side of degenerate bisector through v01 {v01}");
                }
                else // Valid (non-degenerate) angular bisector
                {
                    // Scale distance from to the lines.
                    FT sd_p_l0 = Mathex.validate(l0.a() * p.x() + l0.b() * p.y() + l0.c());
                    FT sd_p_l1 = Mathex.validate(l1.a() * p.x() + l1.b() * p.y() + l1.c());

                    CGAL_STSKEL_TRAITS_TRACE($"sd_p_l0 = {sd_p_l0}");
                    CGAL_STSKEL_TRAITS_TRACE($"sd_p_l1 = {sd_p_l1}");

                    var lCmpResult = certified_compare(sd_p_l0, sd_p_l1);
                    if (is_certain(lCmpResult))
                    {
                        var cmpResult = (CompareResultEnum)(int)lCmpResult;
                        CGAL_STSKEL_TRAITS_TRACE($"compare(sd_p_l0, sd_p_l1) = {lCmpResult}");
                        if ((int)lCmpResult == (int)CompareResultEnum.EQUAL)
                        {
                            CGAL_STSKEL_TRAITS_TRACE("Point is exactly at bisector");

                            rResult = (UncertainCompareResult)(int)OrientedSideEnum.ON_ORIENTED_BOUNDARY;
                        }
                        else
                        {
                            var smaller = Mathex.certified_is_smaller(Mathex.validate(l0.a() * l1.b()), Mathex.validate(l1.a() * l0.b()));
                            if (is_certain(smaller))
                            {
                                // Reflex bisector?
                                if ((bool)smaller)
                                {
                                    var osr = ((cmpResult == CompareResultEnum.SMALLER) ? OrientedSideEnum.ON_NEGATIVE_SIDE : OrientedSideEnum.ON_POSITIVE_SIDE);
                                    CGAL_STSKEL_TRAITS_TRACE($"Event point is on {((osr > 0) ? "POSITIVE" : "NEGATIVE")} side of reflex bisector");
                                    rResult = (UncertainCompareResult)(int)osr;
                                }
                                else
                                {
                                    var osr = (cmpResult == CompareResultEnum.LARGER) ? OrientedSideEnum.ON_NEGATIVE_SIDE : OrientedSideEnum.ON_POSITIVE_SIDE;
                                    CGAL_STSKEL_TRAITS_TRACE($"Event point is on {((osr > 0) ? "POSITIVE" : "NEGATIVE")} side of convex bisector");
                                    rResult = (UncertainCompareResult)(int)osr;
                                }
                            }
                        }
                    }
                }
            }
            catch (ArithmeticOverflowException aoe)
            {
                CGAL_STSKEL_TRAITS_TRACE("Unable to compute value due to overflow.");
                DebuggerInfo.ExceptionHandler(aoe);
            }
            catch (UncertainConversionException uce)
            {
                CGAL_STSKEL_TRAITS_TRACE("Indeterminate boolean expression.");
                DebuggerInfo.ExceptionHandler(uce);
            }
            if (!is_certain(rResult))
            {

            }
            return rResult;
        }

        // Given 2 triples of oriented straight line segments (l0,l1,l2) and (r0,r1,r2), such that
        // the offsets at time 'tl' for triple 'l' intersects in a point (lx,ly) and
        // the offsets at time 'tr' for triple 'r' intersects in a point (rx,ry)
        // returns true if "tl==tr" and "(lx,ly)==(rx,ry)"
        // PRECONDITIONS:
        //   There exists single points at which the offset lines for 'l' and 'r' at 'tl', 'tr' intersect.
        //

        public UncertainBool are_events_simultaneousC2(Trisegment_2 l, Trisegment_2 r)
        {
            var rResult = UncertainBool.indeterminate;

            Rational lt = Mathex.validate(compute_offset_lines_isec_timeC2(l));
            Rational rt = Mathex.validate(compute_offset_lines_isec_timeC2(r));

            try
            {
                if ((bool)(certified_is_positive(lt).And(certified_is_positive(rt))))
                {
                    var equal_times = certified_is_equal(lt, rt);

                    if (is_certain(equal_times))
                    {
                        if ((bool)equal_times)
                        {
                            Point2 li = Mathex.validate(construct_offset_lines_isecC2(l));
                            Point2 ri = Mathex.validate(construct_offset_lines_isecC2(r));

                            rResult = certified_is_equal(li.x(), ri.x()).And(certified_is_equal(li.y(), ri.y()));
                        }
                        else rResult = (UncertainBool)false;
                    }
                }
            }
            catch (ArithmeticOverflowException aoe)
            {
                DebuggerInfo.ExceptionHandler(aoe);
            }
            return rResult;
        }

        Trisegment construct_ss_trisegment_2(Segment2 aS0, FT aW0, Segment2 aS1, FT aW1, Segment2 aS2, FT aW2) => construct_trisegment(aS0, aW0, aS1, aW1, aS2, aW2);

        UncertainBool do_ss_event_exist_2(Trisegment aTrisegment, FT? aMaxTime) => exist_offset_lines_isec2(aTrisegment, aMaxTime);
        UncertainBool is_edge_facing_ss_node_2(Point_2 aContourNode, Segment_2 aEdge) => is_edge_facing_pointC2(aContourNode, aEdge);
        UncertainBool is_edge_facing_ss_node_2(Trisegment aSkeletonNode, Segment_2 aEdge) => is_edge_facing_offset_lines_isecC2(aSkeletonNode, aEdge);
        UncertainCompareResult compare_ss_event_times_2(Trisegment aL, Trisegment aR) => compare_offset_lines_isec_timesC2(aL, aR);

        UncertainCompareResult compare_ss_event_angles_2(Vector2 aBV1, Vector2 aBV2, Vector2 aLV, Vector2 aRV) => compare_isec_anglesC2(aBV1, aBV2, aLV, aRV);

        //UncertainCompareResult  oriented_side_of_event_point_wrt_bisector_2 ( Trisegment  aEvent
        //                              , Segment2   aE0
        //                              , FT         aW0
        //                              , Segment2   aE1
        //                              , FT         aW1
        //                              , Trisegment aE01Event
        //                              , bool       aE0isPrimary
        //                              ) => oriented_side_of_event_point_wrt_bisectorC2(aEvent,aE0,aW0,aE1,aW1,aE01Event,aE0isPrimary) ;

        UncertainBool are_ss_events_simultaneous_2(Trisegment aA, Trisegment aB) => are_events_simultaneousC2(aA, aB);

        (FT, Point_2)? construct_ss_event_time_and_point_2(Trisegment aTrisegment)
        {
            bool lOK = false;

            FT t = 0;
            Point_2 i = new Point2(0, 0);

            Rational? ot = compute_offset_lines_isec_timeC2(aTrisegment);

            if (ot != null && certainly(certified_is_not_zero(ot.Value.den)))
            {
                t = ot.Value.n() / ot.Value.d();

                Point_2? oi = construct_offset_lines_isecC2(aTrisegment);
                if (oi != null)
                {
                    i = oi.Value;
                    lOK = true;
                }
            }

            //    CGAL_STSKEL_ASSERT_CONSTRUCTION_RESULT(lOK,K,"Construct_ss_event_time_and_point_2",aTrisegment);

            return lOK ? (t, i) : null;
        }

        bool CanSafelyIgnoreSplitEvent(Event lEvent)
        {
            // filter event
            if (mFilteringBound == null)
                return false;

            Trisegment tri = lEvent.trisegment();
            Rational? lOptTime = compute_offset_lines_isec_timeC2(tri);

            if (lOptTime != null && lOptTime.Value.to_nt() > mFilteringBound.Value)
            {
                CGAL_STSKEL_TRAITS_TRACE("Ignoring potential split event");

                // avoid filling the cache vectors with times of trisegments that will be removed
#if USING_CACHE
                //      reset_trisegment(tri.Id);
                tri.ResetCache();
#endif
                return true;
            }

            return false;
        }

        // @todo there shouldn't be any combinatorial structures such as vertices in the traits

        void ComputeFilteringBound(Vertex aNode, IEnumerable<Halfedge> contour_halfedges)
        {
            CGAL_STSKEL_TRAITS_TRACE($"Computing filtering bound of V{aNode.Id}");

            mFilteringBound = null;

            // No gain observed on norway while doing it for more than contour nodes
            if (!aNode.IsContour())
                return;

            // get the contour input segments on each side of the bisector spawned ataNode
            var lHL = validate(aNode.halfedge()?.defining_contour_edge());

            var lHR = validate((aNode.IsContour()) ? lHL.Opposite.Prev.Opposite
                                               : aNode.halfedge().Opposite.defining_contour_edge());

            Segment2 lSL = new Segment_2(lHL.Opposite.Vertex.point(),
                                                 lHL.Vertex.point(),
                                                 lHL.Id);
            Segment2 lSR = new Segment2(lHR.Opposite.Vertex.point(),
                                         lHR.Vertex.point(),
                                         lHR.Id);

            Line_2 lL = validate(compute_weighted_line_coeffC2(lSL, lHL.Weight));
            Line_2 lR = validate(compute_weighted_line_coeffC2(lSR, lHR.Weight));

            Vector2 lVL = new Vector2(lL.b(), -lL.a());
            Vector2 lVR = new Vector2(-lR.b(), lR.a());
            Vector2 lVLR = lVL + lVR;
            Point_2 laP = aNode.point();
            Ray2 bisect_ray = new Ray2(laP, lVLR);

            // @todo this should use some kind of spatial searching
            foreach (Halfedge h in contour_halfedges)
            {
                CGAL_assertion((h).Vertex.IsContour() && (h).Opposite.Vertex.IsContour());

                // @todo could be a line as long as we are in a convex area
                Segment_2 s_h = new Segment_2((h).Opposite.Vertex.point(), (h).Vertex.point());

                // we use segments of the input polygon intersected by the bisector and such that
                // they are oriented such that the reflex vertex is on the left side of the segment
                var orient = orientation(s_h.Source, s_h.Target, aNode.point());
                if (!is_certain(orient) || orient != OrientationEnum.LEFT_TURN)
                    continue;

                var inter = intersection(bisect_ray, s_h);
                if (inter != null && !is_certain(inter))
                    continue;

                // See the other function for the equations
                var lSh = new Segment2(s_h.Source, s_h.Target, h.Id);
                Line2 lh = validate(compute_normalized_line_coeffC2(lSh));

                FT lLambda = -(lh.a() * laP.x() + lh.b() * laP.y() + lh.c()) /
                                 (lh.a() * lVLR.x() + lh.b() * lVLR.y());

                Point_2 lP = laP + lVLR;
                FT lBound = lLambda * (lL.a() * lP.x() + lL.b() * lP.y() + lL.c());

                if (!is_finite(lBound) || !is_positive(lBound))
                    continue;

                if (mFilteringBound == null || mFilteringBound > lBound)
                    mFilteringBound = lBound;
            }

            if (mFilteringBound != null)
            {
                CGAL_STSKEL_TRAITS_TRACE("Filtering bound: {mFilteringBound}");
            }
            else
            {
                CGAL_STSKEL_TRAITS_TRACE("Filtering bound: none");
            }
        }
    }
}