using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using static CGAL.DebuggerInfo;
using static CGAL.Mathex;
namespace CGAL
{
    using Segment_2 = CGAL.Segment2;
    using Point_2 = CGAL.Point2;
    using Line_2 = CGAL.Line2;
    using FT = double;
    using Trisegment_2 = CGAL.Trisegment;

    public  partial class StraightSkeletonBuilder

    {
        // Just like the uncertified collinear() returns true IFF r lies in the line p.q
        // NOTE: r might be in the ray from p or q containing q or p, that is, there is no ordering implied, just that
        // the three points are along the same line, in any order.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  UncertainBool certified_collinearC2(in Point2 p, in Point2 q, in Point2 r)
        {
            return certified_is_equal((q.x() - p.x()) * (r.y() - p.y()), (r.x() - p.x()) * (q.y() - p.y()));
        }

        // Just like the uncertified collinear_are_ordered_along_lineC2() returns true IFF, given p,q,r along the same line,
        // q is in the closed segment [p,r].

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  UncertainBool certified_collinear_are_ordered_along_lineC2(in Point2 p,
                                                                      in Point2 q,
                                                                      in Point2 r)
        {
            if (certainly(p.x() < q.x())) return (UncertainBool)!(r.x() < q.x());
            if (certainly(q.x() < p.x())) return (UncertainBool)!(q.x() < r.x());
            if (certainly(p.y() < q.y())) return (UncertainBool)!(r.y() < q.y());
            if (certainly(q.y() < p.y())) return (UncertainBool)!(q.y() < r.y());

            if ((p.x() == q.x()) && (p.y() == q.y())) return (UncertainBool)true;

            return UncertainBool.indeterminate;
        }

        // Returns true IFF segments e0,e1 share the same supporting line

        public  UncertainBool are_edges_collinearC2(in Segment2 e0, in Segment2 e1)
        {
            return certified_collinearC2(e0.source(), e0.target(), e1.source())
              .And(certified_collinearC2(e0.source(), e0.target(), e1.target()));
        }

        // Returns true IFF the supporting lines for segments e0,e1 are parallel (or the same)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  UncertainBool are_edges_parallelC2(in Segment2 e0, in Segment2 e1)
        {
            UncertainCompareResult s = certified_sign_of_determinant2x2(e0.target().x() - e0.source().x()
                                                                , e0.target().y() - e0.source().y()
                                                                , e1.target().x() - e1.source().x()
                                                                , e1.target().y() - e1.source().y()
                                                               );

            return (s == (UncertainCompareResult)(int)(CompareResultEnum.EQUAL));
        }

        // Returns true IFF the parallel segments are equally oriented.
        // Precondition: the segments must be parallel.
        // the three points are along the same line, in any order.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  UncertainBool are_parallel_edges_equally_orientedC2(in Segment2 e0, in Segment2 e1)
        {
            //calcule
            return certified_sign(compute_scalar_product_2( new Vector2(e0.source(),e0.target() ), new Vector2 (e1.source(),e1.target() ))) == 1;
        }

        // Returns true IFF segments e0,e1 share the same supporting line but do not overlap except at the vertices, and have the same orientation.
        // NOTE: If e1 goes back over e0 (a degenerate antenna or alley) this returns false.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  UncertainBool are_edges_orderly_collinearC2(in Segment2 e0, in Segment2 e1)
        {
            return are_edges_collinearC2(e0, e1)
            .And(are_parallel_edges_equally_orientedC2(e0, e1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  UncertainCompareResult certified_side_of_oriented_lineC2(FT a, FT b, FT c, FT x, FT y)
        {
            return certified_sign(a * x + b * y + c);
        }

        // Given 3 oriented straight line segments: e0, e1, e2
        // returns true if there exists some positive offset distance 't' for which the
        // leftward-offsets of their supporting lines intersect at a single point.
        //
        // NOTE: This function can handle the case of collinear and/or parallel segments.
        //
        // If two segments are collinear but equally oriented (that is, they share a degenerate vertex) the event exists and
        // is well defined, In that case, the degenerate vertex can be even a contour vertex or a skeleton node. If it is a skeleton
        // node, it is properly defined by the trisegment tree that corresponds to the node.
        // A trisegment tree stores not only the "current event" trisegment but also the trisegments for the left/right seed vertices,
        // recursively in case the seed vertices are skeleton nodes as well.
        // Those seeds are used to determine the actual position of the degenerate vertex in case of collinear edges (since that point is
        // not given by the collinear edges alone)
        //

        public  UncertainBool exist_offset_lines_isec2(Trisegment tri, FT? aMaxTime)
        {
            UncertainBool rResult = UncertainBool.indeterminate;

            CGAL_STSKEL_TRAITS_TRACE("\n~~ Checking existence of an event ");
            CGAL_STSKEL_TRAITS_TRACE($"Event:\n{tri}");

            if (tri.collinearity() != Trisegment_collinearity.TRISEGMENT_COLLINEARITY_ALL)
            {
                CGAL_STSKEL_TRAITS_TRACE((tri.collinearity() == Trisegment_collinearity.TRISEGMENT_COLLINEARITY_NONE ? " normal edges" : " collinear edges"));

                try
                {
                    Rational t = validate(compute_offset_lines_isec_timeC2(tri));

                    UncertainBool d_is_zero = certified_is_zero(t.den);
                    if (! (bool)(d_is_zero))
                    {
                        rResult = certified_is_positive(t);

                        if (aMaxTime.HasValue && certainly(rResult))
                            rResult = certified_is_smaller_or_equal(t, new Rational(aMaxTime.Value, 1));

                        CGAL_STSKEL_TRAITS_TRACE($"Event time: {t}. Event {((bool)rResult ? "exist." : "doesn't exist.")}");
                    }
                    else
                    {
                        CGAL_STSKEL_TRAITS_TRACE("Denominator exactly zero, Event doesn't exist.");
                        rResult = (UncertainBool)false;
                    }
                }
                catch (ArithmeticException ex)
                {
                    CGAL_STSKEL_TRAITS_TRACE("Event time overflowed, event existence is indeterminate.");
                    ExceptionHandler(ex);
                    rResult = (UncertainBool)false;
                }
            }
            else
            {
                CGAL_STSKEL_TRAITS_TRACE("All the edges are collinear. Event doesn't exist.");
                rResult = (UncertainBool)false;
            }

            return rResult;
        }

        // Given 2 triples of oriented straight line segments: (m0,m1,m2) and (n0,n1,n2), such that
        // for each triple there exists distances 'mt' and 'nt' for which the offsets lines (at mt and nt resp.),
        // (m0',m1',m2') and (n0',n1',n2') intersect each in a single point; returns the relative order of mt w.r.t. nt.
        // That is, indicates which offset triple intersects first (closer to the source lines)
        // PRECONDITION: There exists distances mt and nt for which each offset triple intersect at a single point.

        public  UncertainCompareResult compare_offset_lines_isec_timesC2(Trisegment_2 m, Trisegment_2 n)
        {
            CGAL_STSKEL_TRAITS_TRACE("compare_offset_lines_isec_timesC2(\n{m}\n{n}\n)");

            UncertainCompareResult rResult = UncertainCompareResult.indeterminate;

            Rational mt_ = validate(compute_offset_lines_isec_timeC2(m));
            Rational nt_ = validate(compute_offset_lines_isec_timeC2(n));

            try
            {
                if ((bool) certified_is_positive(mt_) && (bool)certified_is_positive(nt_))
                    rResult = certified_compare(mt_, nt_);
            }
            catch (ArithmeticException ex)
            {
                CGAL_STSKEL_TRAITS_TRACE("Event time overflowed, event existence is indeterminate.");
                ExceptionHandler(ex);
            }

            return rResult;
        }

        public  UncertainCompareResult compare_isec_anglesC2(in Vector2 aBV1, in Vector2 aBV2, Vector2 aLV, Vector2 aRV)
        {
            UncertainCompareResult rResult = UncertainCompareResult.indeterminate;
            var lBisectorDirection = aBV2 - aBV1;
            FT lLNorm = inexact_sqrt(compute_scalar_product_2(aLV, aLV));
            FT lRNorm = inexact_sqrt(compute_scalar_product_2(aRV, aRV));

            if (!(bool)certified_is_positive(lLNorm) ||
                !(bool)certified_is_positive(lRNorm))
                return rResult;

            aLV = aLV / lLNorm;
            aRV = aRV / lRNorm;

            FT lLSp = compute_scalar_product_2(lBisectorDirection, aLV);
            FT lRSp = compute_scalar_product_2(lBisectorDirection, aRV);

            // Smaller if the scalar product is larger, so swapping
            rResult = certified_compare(lRSp, lLSp);

            return rResult;
        }

        // Returns true if the point aP is on the positive side of the line supporting the edge
        //

        public  UncertainBool is_edge_facing_pointC2(Point_2? aP, Segment2 aEdge)
        {
            UncertainBool rResult = UncertainBool.indeterminate;
            if (aP.HasValue)
            {
                FT a, b, c;
                line_from_pointsC2(aEdge.source().x(), aEdge.source().y(), aEdge.target().x(), aEdge.target().y(), out a, out b, out c);
                UncertainCompareResult sol= certified_side_of_oriented_lineC2(a, b, c, aP.Value.x(), aP.Value.y());
                rResult= sol == 1;
                
            }
            
            return rResult;
        }

        // Given a triple of oriented straight line segments: (e0,e1,e2) such that their offsets
        // at some distance intersects in a point (x,y), returns true if (x,y) is on the positive side of the line supporting aEdge
        //

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public  UncertainBool is_edge_facing_offset_lines_isecC2(Trisegment_2 tri, Segment2 aEdge)
        {

            return is_edge_facing_pointC2(construct_offset_lines_isecC2(tri), aEdge);
        }

        // Given an event trisegment and two oriented straight line segments e0 and e1, returns the oriented side of the event point
        // w.r.t. the (positive) bisector [e0,e1].
        //
        // The (positive) bisector [e0,e1] is a ray starting at the vertex (e0,e1) (called "v01")
        //
        // If e0,e1 are consecutive in the input polygon, v01 = e0.target() = e1.source().
        // If they are not consecutive in the input polygon they must had become consecutive at some known previous event. In this
        // case, the point of the "v01_event" trisegment intersection determines v01 which is to the position of the very first
        // vertex shared between e0,e1 (at the time they first become consecutive).
        //
        // A point *exactly on* the bisector is an offset vertex (e0*,e1*) (that is, belongs to both e0* and e1*).
        // A point to the positive side of the bisector belongs to e0* but not e1*
        // A point to the negative side of the bisector belongs to e1* but not e0*
        //
        // If e0,e1 intersect in a single point the bisector is an angular bisector.
        //
        // One of e0 or e1 is considered the primary edge.
        //
        // If e0,e1 are parallel a line perpendicular to the primary edge passing through "v01" is used "as bisector".
        //
        // If e0,e1 are collinear then this perpendicular line is a perpendicular bisector of the two segments.
        //
        // If e0,e1 are parallel but in opposite directions then the bisector is an equidistant line parallel to e0 and e1.
        // e0* and e1* overlap and are known to be connected sharing a known vertex v01, which is somewhere along the parallel
        // line which is the bisector of e0 and e1.
        // Given a line perpendicular to e0 through v01, a point to its positive side belongs to e0* while a point to its negative side does not.
        // Given a line perpendicular to e1 through v01, a point to its negative side belongs to e1* while a point to its positive side does not.
        //
        // This predicate is used to determine the validity of a split or edge event.
        //
        // A split event is the collision of a reflex wavefront and some opposite offset edge. Unless the three segments
        // don't actually collide (there is no event), the split point is along the supporting line of the offset edge.
        // Testing its validity amounts to determining if the split point is inside the closed offset segment instead of
        // the two open rays before and after the offset segment endpoints.
        // The offset edge is bounded by its previous and next adjacent edges at the time of the event. Thus, the bisectors
        // of this edge and its previous/next adjacent edges (at the time of the event) determine the offset vertices that
        // bound the opposite edge.
        // If the opposite edge is 'e' and its previous/next edges are "preve"/"nexte" then the split point is inside the offset
        // edge if it is NOT to the positive side of [preve,e] *and* NOT to the negative side o [e,nexte].
        // (so this predicate answers half the question, at one side).
        // If the split point is exactly over any of these bisectors then the split point occurs exactly and one (or both) endpoints
        // of the opposite edge (so it is a pseudo-split event since the opposite edge is not itself split in two halfedges).
        // When this predicate is called to test (prev,e), e is the primary edge but since it is passed as e1, primary_is_0=false.
        // This causes the case of parallel but not collinear edges to return positive when the split point is before the source point of e*
        // (a positive result means invalid split).
        // Likewise, primary_is_0 must be true when testing (e,nexte) to return negative if the split point is past the target endpoint of e*.
        // (in the other cases there is no need to discriminate which is 'e' in the call since the edges do not overlap).
        //
        // An edge event is a collision of three *consecutive* edges, say, e1,e2 and e3.
        // The collision causes e2 (the edge in the middle) to collapse and e1,e3 to become consecutive and form a new vertex.
        // In all cases there is an edge before e1, say e0, and after e3, say e4.
        // Testing for the validity of an edge event amounts to determine that (e1,e3) (the new vertex) is not before (e0,e1) nor
        // past (e3,e4).
        // Thus, and edge event is valid if the new vertex is NOT to the positive side of [e0,e1] *and* NOT to the negative side of [e3,e4].
        //
        // PRECONDITIONS:
        //   There exists a single point 'p' corresponding to the event as given by the trisegment
        //   e0 and e1 are known to be consecutive at the time of the event (even if they are not consecutive in the input polygon)
        //   If e0 and e1 are not consecutive in the input, v01_event is the event that defined their first common offset vertex.
        //   If e0 and e1 are consecutive in the input, v01_event is null.
        //

        public  UncertainCompareResult oriented_side_of_event_point_wrt_bisectorC2(Trisegment_2 @event,
                                                     in Segment2 e0,
                                                     FT w0,
                                                     Segment2 e1,
                                                     FT w1,
                                                     Trisegment_2 v01_event, // can be null
                                                     bool primary_is_0

                                                     )
        {
            UncertainCompareResult rResult = UncertainCompareResult.indeterminate;

            try
            {
                Point_2 p = validate(construct_offset_lines_isecC2(@event));

                Line_2 l0 = validate(compute_weighted_line_coeffC2(e0, w0));
                Line_2 l1 = validate(compute_weighted_line_coeffC2(e1, w1));

                CGAL_STSKEL_TRAITS_TRACE("\n~~ Oriented side of point ");
                CGAL_STSKEL_TRAITS_TRACE($"p = {p} w.r.t. bisector of [E{e0.Id} {e0} {(primary_is_0 ? "*" : "")}, E{e1.Id} {e1} {(primary_is_0 ? "" : "*")}]");

                // Degenerate bisector?
                if (certainly(are_edges_parallelC2(e0, e1)))
                {
                    CGAL_STSKEL_TRAITS_TRACE("Bisector is not angular.");

                    // b01 is degenerate so we don't have an *angular bisector* but a *perpendicular* bisector.
                    // We need to compute the actual bisector line.
                    CGAL_assertion(v01_event != Trisegment.NULL || (v01_event == Trisegment.NULL && e0.target() == e1.source()));

                    Point_2 v01 = v01_event != Trisegment.NULL ? validate(construct_offset_lines_isecC2(v01_event))
                                            : e1.source();

                    CGAL_STSKEL_TRAITS_TRACE($"v01={v01} {(v01_event != Trisegment.NULL ? " (from skeleton node)" : "")}");

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
                    FT sd_p_l0 = validate(l0.a() * p.x() + l0.b() * p.y() + l0.c());
                    FT sd_p_l1 = validate(l1.a() * p.x() + l1.b() * p.y() + l1.c());

                    CGAL_STSKEL_TRAITS_TRACE($"sd_p_l0 = {sd_p_l0}");
                    CGAL_STSKEL_TRAITS_TRACE($"sd_p_l1 = {sd_p_l1}");

                    UncertainCompareResult lCmpResult = certified_compare(sd_p_l0, sd_p_l1);
                    if (is_certain(lCmpResult))
                    {
                        CGAL_STSKEL_TRAITS_TRACE($"compare(sd_p_l0, sd_p_l1) = {lCmpResult}");
                        if ((bool)(lCmpResult == (int)CompareResultEnum.EQUAL))
                        {
                            CGAL_STSKEL_TRAITS_TRACE("Point is exactly at bisector");

                            rResult = (UncertainCompareResult)(int)OrientedSideEnum.ON_ORIENTED_BOUNDARY;
                        }
                        else
                        {
                            var smaller = certified_is_smaller(validate(l0.a() * l1.b()), validate(l1.a() * l0.b()));
                            if (is_certain(smaller))
                            {
                                // Reflex bisector?
                                if ((bool)smaller)
                                {
                                    if ((bool)(lCmpResult == (int)CompareResultEnum.SMALLER))
                                    {
                                        rResult = (UncertainCompareResult)(int)OrientedSideEnum.ON_NEGATIVE_SIDE;
                                    }
                                    else
                                    {
                                        rResult = (UncertainCompareResult)(int)OrientedSideEnum.ON_POSITIVE_SIDE;
                                    }
                                    CGAL_STSKEL_TRAITS_TRACE($"Event point is on {((((int)rResult) > 0) ? "POSITIVE" : "NEGATIVE")} side of reflex bisector");
                                }
                                else
                                {
                                    if ((bool)(lCmpResult == (int)CompareResultEnum.LARGER))
                                    {
                                        rResult = (UncertainCompareResult)(int)OrientedSideEnum.ON_NEGATIVE_SIDE;
                                    }
                                    else
                                    {
                                        rResult = (UncertainCompareResult)(int)OrientedSideEnum.ON_POSITIVE_SIDE;
                                    }
                                    CGAL_STSKEL_TRAITS_TRACE($"Event point is on {(((bool)(rResult > 0)) ? "POSITIVE" : "NEGATIVE")}side of convex bisector");
                                }
                            }
                        }
                    }
                }
            }
            catch (ArithmeticOverflowException aoe)
            {
                DebuggerInfo.ExceptionHandler(aoe);
                CGAL_STSKEL_TRAITS_TRACE("Unable to compute value due to overflow.");
            }
            catch (UncertainConversionException uce)
            {
                DebuggerInfo.ExceptionHandler(uce);
                CGAL_STSKEL_TRAITS_TRACE("Indeterminate boolean expression.");
            }

            return rResult;
        }

    }
}