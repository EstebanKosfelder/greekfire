using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CGAL.Mathex;
using static CGAL.DebuggerInfo;
using S = CGAL.Segment2;
using FT = double;
using Point_2 = CGAL.Point2;
using System.ComponentModel.Design;


namespace CGAL
{
    using static Mathex;
    public partial class StraightSkeletonBuilder
    {
       

        public  UncertainBool are_edges_parallelC2(Segment2 e0, Segment2 e1)
        {
            var s = certified_sign_of_determinant2x2(e0.target().x() - e0.source().x()
                                                                , e0.target().y() - e0.source().y()
                                                                , e1.target().x() - e1.source().x()
                                                                , e1.target().y() - e1.source().y()
                                                               );

            return (s == (int)CompareResultEnum.EQUAL);
        }

        public  bool are_edges_collinear(S e0, S e1)
        {
            return ((e1.source() == e0.source()) || (e1.source() == e0.target()) || collinear(e0.source(), e0.target(), e1.source()))
              && ((e1.target() == e0.source()) || (e1.target() == e0.target()) || (collinear(e0.source(), e0.target(), e1.target())));
        }

        //inline
        public  bool are_parallel_edges_equally_oriented(S e0, S e1)
        {
            return angle(e0.source(), e0.target(),
                         e1.source(), e1.target()) == 1/*ACUTE*/;
        }

        public  bool are_edges_orderly_collinear(S e0, S e1)
        {
            return are_edges_collinear(e0, e1) && are_parallel_edges_equally_oriented(e0, e1);
        }

        public  Trisegment_collinearity trisegment_collinearity_no_exact_constructions(S e0, S e1, S e2)
        {
            // 'are_edges_orderly_collinear()' is used to harmonize coefficients, but if the kernel is inexact
            // we could also have that are_edges_orderly_collinear() returns false, but the computed coefficients
            // are identical. In that case, we want to return that there is a collinearity, otherwise the internal
            // computations (even the exact ones) will fail.

            Line2 l0 = Mathex.validate(compute_normalized_line_coeffC2(e0));
            Line2 l1 = Mathex.validate(compute_normalized_line_coeffC2(e1));
            Line2 l2 = Mathex.validate(compute_normalized_line_coeffC2(e2));

            bool is_01 = (l0.a() == l1.a()) && (l0.b() == l1.b()) && (l0.c() == l1.c());
            bool is_02 = (l0.a() == l2.a()) && (l0.b() == l2.b()) && (l0.c() == l2.c());
            bool is_12 = (l1.a() == l2.a()) && (l1.b() == l2.b()) && (l1.c() == l2.c());

            CGAL_STSKEL_TRAITS_TRACE($"coeff equalities: {is_01} {is_02} {is_12}");

            if (is_01 & !is_02 & !is_12)
                return Trisegment_collinearity.TRISEGMENT_COLLINEARITY_01;
            else if (is_02 & !is_01 & !is_12)
                return Trisegment_collinearity.TRISEGMENT_COLLINEARITY_02;
            else if (is_12 & !is_01 & !is_02)
                return Trisegment_collinearity.TRISEGMENT_COLLINEARITY_12;
            else if (!is_01 & !is_02 & !is_12)
                return Trisegment_collinearity.TRISEGMENT_COLLINEARITY_NONE;
            else
                return Trisegment_collinearity.TRISEGMENT_COLLINEARITY_ALL;
        }

        /////

        //// Attempted to use std::hypot (https://github.com/CGAL/cgal/commit/a1845691d5d8055978662cd95059c6d3f94c17a2)
        //// but did not notice any gain, and even observed some regressions in the tests.

        //inexact_sqrt_implementation(const NT& n, CGAL::Null_functor)
        //{
        //        /*
        //    typedef CGAL::Interval_nt < false > IFT;
        //    typename IFT::Protector protector;

        //    CGAL::NT_converter<NT, IFT> to_ift;
        //    IFT sqrt_ift = sqrt(to_ift(n));
        //    CGAL_STSKEL_TRAITS_TRACE("sqrt's interval  sqrt_ift.inf()   sqrt_ift.sup());
        //    CGAL_STSKEL_TRAITS_TRACE("interval delta  sqrt_ift.sup() - sqrt_ift.inf());

        //    return NT(to_double(sqrt_ift));
        //        */
        //}

        //template<typename NT, typename Sqrt>
        //typename Sqrt::result_type
        //inexact_sqrt_implementation(const NT& nt, Sqrt sqrt)
        //{
        //    CGAL_STSKEL_TRAITS_TRACE("sqrt( typeid(NT).name() )");
        //    return sqrt(nt);
        //}

        //template<typename NT>
        //decltype(auto) inexact_sqrt(const NT& nt)
        //{
        //    // the initial version of this function was using Algebraic_category
        //    // for the dispatch but some ring type (like Gmpz) provides a Sqrt
        //    // functor even if not being Field_with_sqrt.
        //    typedef CGAL::Algebraic_structure_traits<NT> AST;
        //    typedef typename AST::Sqrt Sqrt;
        //    return inexact_sqrt_implementation(nt, Sqrt());
        //}

        //template<typename NT>
        //Quotient<NT>
        //inexact_sqrt(const Quotient<NT>& q)
        //{
        //    return { inexact_sqrt(q.numerator() * q.denominator()), abs(q.denominator()) };
        //}

        //template<typename NT>
        //Lazy_exact_nt<NT>
        //inexact_sqrt(const Lazy_exact_nt<NT>& lz)
        //{
        //    return inexact_sqrt(exact(lz));
        //}

        // Given an oriented 2D straight line segment 'e', computes the normalized coefficients (a,b,c)
        // of the supporting line, and weights them with 'aWeight'.
        //
        // POSTCONDITION: [a,b] is the leftward normal vector.
        // POSTCONDITION: In case of overflow, an empty optional<> is returned.

        

        //compute_normalized_line_coeffC2(Segment2 e)
        //    {
        //

        //        if (aCaches.mCoeff_cache.IsCached(e.mID))
        //            return aCaches.mCoeff_cache.Get(e.mID);

        //        boost::optional<Line_2> rRes = compute_normalized_line_coeffC2(e);

        //        aCaches.mCoeff_cache.Set(e.mID, rRes);

        //        return rRes;
        //    }

        // @todo weightless coefficients are stored because we use them sometimes weighted, and sometimes
        // inversely weighted (filtering bound). Should we store them weighted also for speed reasons?
        //CACHE
     public    Line2? compute_weighted_line_coeffC2(Segment2 e, FT aWeight)
        {
            CGAL_precondition(is_finite(aWeight) && is_positive(aWeight));

            try
            {
                Line2 l = Mathex.validate(compute_normalized_line_coeffC2(e /*, aCaches*/));

                FT a = l.a() * aWeight;
                FT b = l.b() * aWeight;
                FT c = l.c() * aWeight;

                CGAL_STSKEL_TRAITS_TRACE($"\n~~ Weighted line coefficients for E{e.Id} {e} weight = {aWeight} a = {a} nb = {b} nc = {c}");

                if (!is_finite(a) || !is_finite(b) || !is_finite(c))
                    return null;
                return new Line2(a, b, c);
            }
            catch (ArithmeticException ex)
            {
                Console.Error.WriteLine(ex);
                return null;
            }
        }


        // Given two oriented straight line segments e0 and e1 such that e-next follows e-prev, returns
        // the coordinates of the midpoint of the segment between e-prev and e-next.
        // NOTE: the edges can be oriented e0.e1 or e1.e0
        //
        // POSTCONDITION: In case of overflow an empty optional is returned.
        //

        public Point2? compute_oriented_midpoint(Segment2 e0, Segment2 e1)
        {
            CGAL_STSKEL_TRAITS_TRACE($"Computing oriented midpoint between: e0 {e0} e1 {e1}");

            FT delta01 = squared_distance(e0.target(), e1.source());
            if (is_finite(delta01) && Mathex.is_zero(delta01))
                return e0.target();

            FT delta10 = squared_distance(e1.target(), e0.source());
            if (is_finite(delta10) &&  Mathex.is_zero(delta10))
                return e1.target();

            bool ok = false;
            Point_2 mp = new Point_2();

            if (is_finite(delta01) && is_finite(delta10))
            {
                if (delta01 <= delta10)
                    mp = midpoint(e0.target(), e1.source());
                else
                    mp = midpoint(e1.target(), e0.source());

                CGAL_STSKEL_TRAITS_TRACE($"mp= p2str(mp)");

                ok = is_finite(mp.x()) && is_finite(mp.y());
            }

            return ok ? mp : null;
        }


        //
        // Constructs a Trisegment_2 which stores 3 oriented straight line segments e0,e1,e2 along with their collinearity.
        //
        // NOTE: If the collinearity cannot be determined reliably, a null trisegment is returned.
        //

        
        public Trisegment construct_trisegment(Segment2 e0,
                       FT w0,
                       Segment2 e1,
                       FT w1,
                       Segment2 e2,
                       FT w2
            )
        {
            CGAL_STSKEL_TRAITS_TRACE($"\n~~  Construct trisegment ");
            CGAL_STSKEL_TRAITS_TRACE($"Segments E{e0.Id}  E{e1.Id}  E{e2.Id}");

            Trisegment_collinearity lCollinearity = trisegment_collinearity_no_exact_constructions(e0, e1, e2);

            return new Trisegment(e0, w0, e1, w1, e2, w2, lCollinearity, this.TrisegmentCount++);
        }


        public Rational squared_distance_from_point_to_lineC2(FT px, FT py, FT sx, FT sy, FT tx, FT ty)
        {
            FT ldx = tx - sx;
            FT ldy = ty - sy;
            FT rdx = sx - px;
            FT rdy = sy - py;

            FT n = square(ldx * rdy - rdx * ldy);
            FT d = square(ldx) + square(ldy);

            return new Rational(n, d);
        }


        // Given 3 oriented straight line segments: e0, e1, e2
        // returns the OFFSET DISTANCE (n/d) at which the offsetted lines
        // intersect at a single point, IFF such intersection exist.
        // If the lines intersect to the left, the returned distance is positive.
        // If the lines intersect to the right, the returned distance is negative.
        // If the lines do not intersect, for example, for collinear edges, or parallel edges but with the same orientation,
        // returns 0 (the actual distance is undefined in this case, but 0 is a useful return)
        //
        // NOTE: The result is a explicit rational number returned as a tuple (num,den); the caller must check that den!=0 manually
        // (a predicate for instance should return indeterminate in this case)
        //
        // PRECONDITION: None of e0, e1 and e2 are collinear (but two of them can be parallel)
        //
        // POSTCONDITION: In case of overflow an empty optional is returned.
        //
        // NOTE: The segments (e0,e1,e2) are stored in the argument as the trisegment st.event()
        //

        public  Rational? compute_normal_offset_lines_isec_timeC2(Trisegment tri)
        {
            CGAL_STSKEL_TRAITS_TRACE("\n~~ Computing normal offset lines isec time ");
            CGAL_STSKEL_TRAITS_TRACE($"Event:\n{tri}");

            FT num = 0, den = 0;

            // DETAILS:
            //
            // An offset line is given by:
            //
            //   a*x(t) + b*y(t) + c - t = 0
            //
            // were 't > 0' being to the left of the line.
            // If 3 such offset lines intersect at the same offset distance, the intersection 't',
            // or 'time', can be computed solving for 't' in the linear system formed by 3 such equations.
            // The result is :
            //
            // sage: var('a0 b0 c0 a1 b1 c1 a2 b2 c2 x y t w0 w1 w2')
            // (a0, b0, c0, a1, b1, c1, a2, b2, c2, x, y, t, w0, w1, w2)
            // sage:
            // sage: eqw0 = w0*a0*x + w0*b0*y + w0*c0 - t == 0
            // sage: eqw1 = w1*a1*x + w1*b1*y + w1*c1 - t == 0
            // sage: eqw2 = w2*a2*x + w2*b2*y + w2*c2 - t == 0
            // sage:
            // sage: solve([eqw0,eqw1,eqw2], x,y,t)
            //   x ==  (((c1*w1 - c2*w2)*b0 - (b1*w1 - b2*w2)*c0)*w0 - (b2*c1*w2 - b1*c2*w2)*w1) / (((b1*w1 - b2*w2)*a0 - (a1*w1 - a2*w2)*b0)*w0 - (a2*b1*w2 - a1*b2*w2)*w1),
            //   y == -(((c1*w1 - c2*w2)*a0 - (a1*w1 - a2*w2)*c0)*w0 - (a2*c1*w2 - a1*c2*w2)*w1) / (((b1*w1 - b2*w2)*a0 - (a1*w1 - a2*w2)*b0)*w0 - (a2*b1*w2 - a1*b2*w2)*w1),
            //   t == -((b2*c1*w2 - b1*c2*w2)*a0*w1 - (a2*c1*w2 - a1*c2*w2)*b0*w1 + (a2*b1*w2 - a1*b2*w2)*c0*w1)*w0/(((b1*w1 - b2*w2)*a0 - (a1*w1 - a2*w2)*b0)*w0 - (a2*b1*w2 - a1*b2*w2)*w1)

            bool ok = false;

            try
            {
                Line2 l0 = validate(compute_weighted_line_coeffC2(tri.e0(), tri.w0()));
                Line2 l1 = validate(compute_weighted_line_coeffC2(tri.e1(), tri.w1()));
                Line2 l2 = validate(compute_weighted_line_coeffC2(tri.e2(), tri.w2()));

                CGAL_STSKEL_TRAITS_TRACE($"coeffs E{tri.e0().Id} [{l0.a()} {l0.b()} {l0.c()}]",
                                         $"coeffs E{tri.e1().Id} [{l1.a()} {l1.b()} {l1.c()}]",
                                         $"coeffs E{tri.e2().Id} [{l2.a()} {l2.b()} {l2.c()}]");

                num = (l2.a() * l0.b() * l1.c())
                     - (l2.a() * l1.b() * l0.c())
                     - (l2.b() * l0.a() * l1.c())
                     + (l2.b() * l1.a() * l0.c())
                     + (l1.b() * l0.a() * l2.c())
                     - (l0.b() * l1.a() * l2.c());

                den = (-l2.a() * l1.b())
                     + (l2.a() * l0.b())
                     + (l2.b() * l1.a())
                     - (l2.b() * l0.a())
                     + (l1.b() * l0.a())
                     - (l0.b() * l1.a());

                ok = is_finite(num) && is_finite(den);
                CGAL_STSKEL_TRAITS_TRACE($"Event time (normal): n={num} d={den} n/d={num/den}");
            }
            catch (ArithmeticException e)
            {
                Console.Error.WriteLine(e);
            }

            return ok ? new Rational(num, den) : null;
        }

        
        
        //
        // Given 3 oriented straight line segments: e0, e1, e2 and the corresponding offsetted segments: e0*, e1* and e2*,
        // returns the point of the left or right seed (offset vertex) (e0*,e1*) or (e1*,e2*)
        //
        // If the current event (defined by e0,e1,e2) is a propagated event, that is, it follows from a previous event,
        // the seeds are skeleten nodes and are given by non-null trisegments.
        // If the current event is an initial event the seeds are contour vertices and are given by null trisegmets.
        //
        // If a seed is a skeleton node, its point has to be computed from the trisegment that defines it.
        // That trisegment is exactly the trisegment tree that defined the previous event which produced the skeleton node
        // (so the trisegment tree is basically a lazy representation of the seed point).
        //
        // If a seed is a contour vertex, its point is then simply the target endpoint of e0 or e1 (for the left/right seed).
        //
        // This method returns the specified seed point (left or right)
        //
        // NOTE: Split events involve 3 edges but only one seed, the left (that is, only e0*,e1* is connected before the event).
        // The trisegment tree for a split event has always a null right child even if the event is not an initial event
        // (in which case its left child won't be null).
        // If you ask for the right child point for a trisegment tree corresponding to a split event you will just get e1.target()
        // which is nonsensical for a non initial split event.
        //
        // NOTE: There is an abnormal collinearity case which occurs when e0 and e2 are collinear.
        // In this case, these lines do not correspond to an offset vertex (because e0* and e2* are never consecutive before the event),
        // so the degenerate seed is neither the left or the right seed. In this case, the SEED ID for the degenerate pseudo seed is UNKNOWN.
        // If you request the point of such degenerate pseudo seed the oriented midpoint between e0 and e2 is returned.
        //

        public  Point_2? compute_seed_pointC2(Trisegment tri, SEED_ID sid)
        {
            Point_2? p = null;

            switch (sid)
            {
                case SEED_ID.LEFT:

                    p = tri.child_l() != Trisegment.NULL ? construct_offset_lines_isecC2(tri.child_l()) // this can recurse
                                       : compute_oriented_midpoint(tri.e0(), tri.e1());
                    break;

                case SEED_ID.RIGHT:

                    p = tri.child_r() != Trisegment.NULL ? construct_offset_lines_isecC2(tri.child_r()) // this can recurse
                                       : compute_oriented_midpoint(tri.e1(), tri.e2());
                    break;

                case SEED_ID.THIRD:

                    p = tri.child_t() != Trisegment.NULL ? construct_offset_lines_isecC2(tri.child_t()) // this can recurse
                                       : compute_oriented_midpoint(tri.e0(), tri.e2());

                    break;
            }

            return p;
        }

        //
        // Given the trisegment tree for an event which is known to have a normal collinearity returns the seed point
        // of the degenerate seed.
        // A normal collinearity occurs when e0,e1 or e1,e2 are collinear.

        //CACHE//
        public  Point_2? construct_degenerate_seed_pointC2(Trisegment tri)
        {
            return compute_seed_pointC2(tri, tri.degenerate_seed_id());
        }
        //CACHE//
        public  Rational? compute_artifical_isec_timeC2(Trisegment tri)
        {
            CGAL_STSKEL_TRAITS_TRACE("\n~~  Computing artificial isec time ");
            CGAL_STSKEL_TRAITS_TRACE("Event:\n tri");

            CGAL_precondition(tri.e0() == tri.e1());
            CGAL_precondition(tri.child_l() != Trisegment.NULL);

            try
            {
                Line2 l0 = validate(compute_weighted_line_coeffC2(tri.e0(), tri.w0()));

                Segment2 contour_seg = tri.e0();
                Direction2 perp_dir = new Direction2(contour_seg.source().y() - contour_seg.target().y(), contour_seg.target().x() - contour_seg.source().x());
                Point_2 seed = validate(construct_offset_lines_isecC2(tri.child_l()));

                Ray2 ray = new Ray2(seed, perp_dir);
                Segment2 opp_seg = tri.e2();

                // Compute the intersection point and evalute the time from the line equation of the contour edge
                var inter_res = intersection(ray, opp_seg);

                FT t;

                if (inter_res.Result == Intersection.Intersection_results.SEGMENT)
                {
                    var points = inter_res.Points;

                    // get the segment extremity closest to the seed
                    var idx = ((CompareResultEnum)compare_distance_2(seed, points[0], points[1])) == CompareResultEnum.SMALLER ? 0 : 1;
                    t = l0.a() * points[idx].x() + l0.b() * points[idx].y() + l0.c();
                }
                else if (inter_res.Result == Intersection.Intersection_results.POINT)
                {
                    Point_2 inter_pt = inter_res.Points[0];
                    if (!is_finite(inter_pt.x()) || !is_finite(inter_pt.y()))
                        return null;
                    t = l0.a() * inter_pt.x() + l0.b() * inter_pt.y() + l0.c();
                }
                else
                {
                    return new Rational(0, 0);
                }

                bool ok = is_finite(t);
                return ok ? new Rational(t, 1.0) : null;
            }
            catch (ArithmeticException ex)
            {
                Console.Error.WriteLine(ex);
            }
            return null;
        }

        // Given 3 oriented straight line segments: e0, e1, e2
        // such that two and only two of these edges are collinear, not necessarily consecutive but with the same orientaton;
        // returns the OFFSET DISTANCE (n/d) at which a line perpendicular to the collinear edge passing through
        // the degenerate seed point intersects the offset line of the non collinear edge
        //
        // NOTE: The result is a explicit rational number returned as a tuple (num,den); the caller must check that den!=0 manually
        // (a predicate for instance should return indeterminate in this case)
        //
        // POSTCONDITION: In case of overflow an empty optional is returned.
        //
        //CACHE//
        public  Rational? compute_degenerate_offset_lines_isec_timeC2(Trisegment tri)
        {
            if (tri.e0() == tri.e1()) // marker for artificial bisectors: they have the same face on both sides
                return compute_artifical_isec_timeC2(tri);

            CGAL_STSKEL_TRAITS_TRACE("\n~~  Computing degenerate offset lines isec time");
            CGAL_STSKEL_TRAITS_TRACE($"Event:\n {tri}");

            // DETAILS:
            //
            // For simplicity, assume e0,e1 are the collinear edges.
            //
            //   (1)
            //   The bisecting line of e0 and e1 is a line perpendicular to e0 (and e1)
            //   which passes through 'q', the degenerate offset vertex (e0*,e1*).
            //   This "degenerate" bisecting line is given by:
            //
            //     B0(t) = p + t*[l0.a,l0.b]
            //
            //   where p is the projection of q along l0 and l0.a,l0.b are the _normalized_ line coefficients for e0 (or e1 which is the same)
            //   Since [a,b] is a _unit_ vector pointing perpendicularly to the left of e0 (and e1);
            //   any point B0(k) is at a distance k from the line supporting e0 and e1.
            //
            //   (2)
            //   The bisecting line of e0 and e2 is given by the following SEL
            //
            //    l0.a*x(t) + l0.b*y(t) + l0.c - t = 0
            //    l2.a*x(t) + l2.b*y(t) + l2.c - t = 0
            //
            //   where (l0.a,l0.b,l0.c) and (l2.a,l2.b,l2.c) are the normalized line coefficientes of e0 and e2, resp.
            //
            //     B1(t) = [x(t),y(t)]
            //
            //   (3)
            //   These two bisecting lines B0(t) and B1(t) intersect (if they do) in a single point 'r' whose distance
            //   to the lines supporting the 3 edges is exactly 't' (since those expressions are precisely parametrized in a distance)
            //   Solving the following vectorial equation:
            //
            //     [x(y),y(t)] = p + t*[l0.a,l0.b]
            //
            //   for t gives the result we want.
            //
            //
            //   (4)
            //   With weights, the above equations become:
            //
            //   sage: eq0 = w0*a0*x + w0*b0*y + w0*c0 - t == 0
            //   sage: eq2 = w2*a2*x + w2*b2*y + w2*c2 - t == 0
            //
            //   sage: solve([eq0,eq2], x,y)
            //     [[x == -(b2*t*w2 - (b2*c0*w2 - b0*c2*w2 + b0*t)*w0)/((a2*b0*w2 - a0*b2*w2)*w0),
            //       y ==  (a2*t*w2 - (a2*c0*w2 - a0*c2*w2 + a0*t)*w0)/((a2*b0*w2 - a0*b2*w2)*w0) ]]
            //
            //   sage: x0 = -(b2*t*w2 - (b2*c0*w2 - b0*c2*w2 + b0*t)*w0)/((a2*b0*w2 - a0*b2*w2)*w0)
            //   sage: eqb0 = px + t * a0 / w0 - x0 == 0
            //   sage: solve(eqb0, t)
            //     [t == -(b2*c0 - b0*c2 - (a2*b0 - a0*b2)*px)*w0*w2/(b0*w0 - (a0*a2*b0 - (a0^2 - 1)*b2)*w2) ]
            //
            //   sage: y0 = (a2*t*w2 - (a2*c0*w2 - a0*c2*w2 + a0*t)*w0)/((a2*b0*w2 - a0*b2*w2)*w0)
            //   sage: eqb1 = py + t * b0 / w0 - y0 == 0
            //   sage: solve(eqb1, t)
            //     [t == -(a2*c0 - a0*c2 + (a2*b0 - a0*b2)*py)*w0*w2/(a0*w0 + (a2*b0^2 - a0*b0*b2 - a2)*w2)]

            bool ok = false;

            try
            {
                Line2 l0 = validate(compute_weighted_line_coeffC2(tri.collinear_edge(), tri.collinear_edge_weight()));
                Line2 l1 = validate(compute_weighted_line_coeffC2(tri.other_collinear_edge(), tri.other_collinear_edge_weight()));
                Line2 l2 = validate(compute_weighted_line_coeffC2(tri.non_collinear_edge(), tri.non_collinear_edge_weight()));

                Point2 q = validate(construct_degenerate_seed_pointC2(tri));

                CGAL_STSKEL_TRAITS_TRACE($"\tCE ID:{tri.collinear_edge().Id}  w:{tri.collinear_edge_weight()}");

                CGAL_STSKEL_TRAITS_TRACE($"\tOCE ID:{tri.other_collinear_edge().Id}  w:{tri.other_collinear_edge_weight()}");

                CGAL_STSKEL_TRAITS_TRACE($"\tNCE ID:{tri.non_collinear_edge().Id}  w:{tri.non_collinear_edge_weight()}");

                CGAL_STSKEL_TRAITS_TRACE($"\tLabc [{l0.a()} {l0.b()} {l0.c()} [{l1.a()} {l1.b()} {l1.c()}] [{l2.a()} {l2.b()} {l2.c()} ]");

                line_project_point(l0.a(), l0.b(), l0.c(), q.x(), q.y(), out var px, out var py);
                CGAL_STSKEL_TRAITS_TRACE($"Seed point:  {q} .\nProjected seed point: ({px} ,{py}) )");

                if (tri.collinear_edge_weight() == tri.other_collinear_edge_weight())
                {
                    FT l0a = l0.a();
                    FT l0b = l0.b();
                    FT l0c = l0.c();
                    FT l2a = l2.a();
                    FT l2b = l2.b();
                    FT l2c = l2.c();

                    // Since l0 and l1 are parallel, we cannot solve the system using:
                    //   l0a*x + l0b*y + l0c = 0 (1)
                    //   l1a*x + l1b*y + l1c = 0
                    //   l2a*x + l2b*y + l2c = 0
                    // Instead, we use the equation of the line orthogonal to l0 (and l1).
                    // However, rephrasing
                    //   l0a*x + l0b*y + l0c = 0
                    // to
                    //   [x, y] = projected_seed + t * N
                    // requires the norm (l0a² + l0b²) to be exactly '1', which likely isn't the case
                    // if we are using inexact square roots. In that case, the norm behaves similarly
                    // to a weight (i.e. speed), and the speed is inverted in the alternate front formulation.
                    // Equation (1) rewritten with weights is:
                    //   w*l0a*x + w*l0b*y + w*l0c = 0
                    // with l0a² + l0b² ~= 1. Extracting the numerical error, we have:
                    //   w'*l0a'*x + w'*l0b'*y + w'*l0c' = 0,
                    // with l0a'² + l0b'² = 1.
                    // The orthogonal displacement is rephrased to:
                    //   [x, y] = projected_seed + t / w' * N,
                    // with w' = weight * (l0a² + l0b²).
                    FT sq_w0 = square(l0a) + square(l0b); // l0a and l0b are already *weighted* coefficients

                    FT num = (0), den = (0);
                    if (!Mathex.is_zero(l0b)) // Non-vertical
                    {
                        num = ((l2a * l0b - l0a * l2b) * px - l2b * l0c + l0b * l2c) * sq_w0;
                        den = l0a * l0a * l2b - l2b * sq_w0 + l0b * sq_w0 - l0a * l2a * l0b;

                        CGAL_STSKEL_TRAITS_TRACE($"Event time (degenerate, non-vertical) n={num}  d={den}  n/d= {new Rational(num, den)}");
                    }
                    else
                    {
                        // l0b = 0, and all sq_w0 disappear
                        num = -l0a * l2b * py - l0a * l2c + l2a * l0c;
                        den = l2a - l0a;

                        CGAL_STSKEL_TRAITS_TRACE($"Event time (degenerate, vertical)  n={num}  d={den}  n/d= {new Rational(num, den)}");
                    }

                    ok = is_finite(num) && is_finite(den);
                    return ok ? new Rational(num, den) : null;
                }
                else
                {
                    // l0 and l1 are collinear but with different speeds, so there cannot be an event.
                    CGAL_STSKEL_TRAITS_TRACE("Event times (degenerate, inequal norms)");
                    CGAL_STSKEL_TRAITS_TRACE("-. Returning 0/0 (no event)");
                    // if we return boost::none, exist_offset_lines_isec2() will think it's a numerical error
                    return new Rational(0, 0);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }

            return null;
        }

        //
        // Calls the appropriate function depending on the collinearity of the edges.
        //

        public  Rational? compute_offset_lines_isec_timeC2(Trisegment tri)
        {
            CGAL_STSKEL_TRAITS_TRACE("compute_offset_lines_isec_timeC2 {tri.Id} ) [ typeid(FT).name() ");


            if (tri.OffsetLinesIsecTimes != null) return tri.OffsetLinesIsecTimes;
            //    if (aCaches.mTime_cache.IsCached(tri.Id))
            //      return aCaches.mTime_cache.Get(tri.Id);

            CGAL_precondition(tri.collinearity() != Trisegment_collinearity.TRISEGMENT_COLLINEARITY_ALL);

            Rational? rRes = tri.collinearity() == Trisegment_collinearity.TRISEGMENT_COLLINEARITY_NONE ? compute_normal_offset_lines_isec_timeC2(tri)
                                                                    : compute_degenerate_offset_lines_isec_timeC2(tri);

            tri.OffsetLinesIsecTimes = rRes;

            //  aCaches.mTime_cache.Set(tri.Id, rRes);

            return rRes;
        }

        // Given 3 oriented line segments e0, e1 and e2
        // such that their offsets at a certain distance intersect in a single point,
        // returns the coordinates (x,y) of such a point.
        //
        // PRECONDITIONS:
        // None of e0, e1 and e2 are collinear (but two of them can be parallel)
        // The line coefficients must be normalized: a²+b²==1 and (a,b) being the leftward normal vector
        // The offsets at a certain distance do intersect in a single point.
        //
        // POSTCONDITION: In case of overflow an empty optional is returned.
        //

        //CACHE
        public  Point_2? construct_normal_offset_lines_isecC2(Trisegment tri)
        {
            CGAL_STSKEL_TRAITS_TRACE("\n~~ Computing normal offset lines isec point ");
            CGAL_STSKEL_TRAITS_TRACE("Event:\n tri");

            FT x = 0, y = 0;

            bool ok = false;
            try
            {
                Line2 l0 = validate(compute_weighted_line_coeffC2(tri.e0(), tri.w0()));
                Line2 l1 = validate(compute_weighted_line_coeffC2(tri.e1(), tri.w1()));
                Line2 l2 = validate(compute_weighted_line_coeffC2(tri.e2(), tri.w2()));

                FT den = l0.a() * l2.b() - l0.a() * l1.b() - l1.a() * l2.b() + l2.a() * l1.b() + l0.b() * l1.a() - l0.b() * l2.a();

                CGAL_STSKEL_TRAITS_TRACE($"\tden={den}");

                if (!Mathex.is_zero(den))
                {
                    FT numX = l0.b() * l2.c() - l0.b() * l1.c() - l1.b() * l2.c() + l2.b() * l1.c() + l1.b() * l0.c() - l2.b() * l0.c();
                    FT numY = l0.a() * l2.c() - l0.a() * l1.c() - l1.a() * l2.c() + l2.a() * l1.c() + l1.a() * l0.c() - l2.a() * l0.c();

                    CGAL_STSKEL_TRAITS_TRACE($"\tnumX={numX}\n\tnumY={numY}");

                    if (is_finite(den) && is_finite(numX) && is_finite(numY))
                    {
                        ok = true;

                        x = numX / den;
                        y = -numY / den;

                        CGAL_STSKEL_TRAITS_TRACE($"\n\tx={x} \n\ty={y}");
                    }
                }

                return ok ? new Point_2(x, y) : null;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
            return null;
        }

        // Given a contour halfedge and a bisector halfedge, constructs the intersection
        // between the line orthogonal to the contour halfedge through a given seed and the bisector halfedge.
        // This is an artificial vertex added to recover simply-connectedness of a skeleton face
        // in weighted skeletons of polygons with holes.

        //CACHE//
        public  Point_2? construct_artifical_isecC2(Trisegment tri)
        {
            CGAL_STSKEL_TRAITS_TRACE($"\n~~  Computing artificial isec point ");
            CGAL_STSKEL_TRAITS_TRACE($"Event:\n {tri}");

            CGAL_precondition(tri.e0() == tri.e1());
            CGAL_precondition(tri.child_l() != Trisegment.NULL);

            try
            {
                Segment2 contour_seg = tri.e0();
                Direction2 perp_dir = new Direction2(contour_seg.source().y() - contour_seg.target().y(), contour_seg.target().x() - contour_seg.source().x());
                Point_2 seed = validate(construct_offset_lines_isecC2(tri.child_l()));
                Ray2 ray = new Ray2(seed, perp_dir);

                Segment2 opp_seg = tri.e2();
                var inter_res = intersection(ray, opp_seg);

                if (inter_res.Result == Intersection.Intersection_results.POINT)
                {
                    var inter_pt = inter_res.Points[0];
                    bool ok = is_finite(inter_pt.x()) && is_finite(inter_pt.y());
                    return ok ? inter_pt : null;
                }
                else if (inter_res.Result == Intersection.Intersection_results.POINT)
                {
                    // get the segment extremity closest to the seed
                    Point_2 pt = ((CompareResultEnum)compare_distance_2(seed, inter_res.Points[0], inter_res.Points[1])) == CompareResultEnum.SMALLER ? inter_res.Points[0] : inter_res.Points[1];
                    return pt;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
            return null;
        }

        // Given 3 oriented line segments e0, e1 and e2
        // such that their offsets at a certain distance intersect in a single point,
        // returns the coordinates (x,y) of such a point.
        // two and only two of the edges are collinear, not necessarily consecutive but with the same orientaton
        //
        // PRECONDITIONS:
        // The line coefficients must be normalized: a²+b²==1 and (a,b) being the leftward normal vector
        // The offsets at a certain distance do intersect in a single point.
        //
        // POSTCONDITION: In case of overflow an empty optional is returned.
        //
        // See detailed computations in compute_degenerate_offset_lines_isec_timeC2()
        //CACHE
        public  Point_2? construct_degenerate_offset_lines_isecC2(Trisegment tri)
        {
            if (tri.e0() == tri.e1()) // marker for artificial bisectors: they have the same face on both sides
                return construct_artifical_isecC2(tri);

            CGAL_STSKEL_TRAITS_TRACE("\n~~ Computing degenerate offset lines isec point ");
            CGAL_STSKEL_TRAITS_TRACE("Event:\n {tri}");

            FT x = 0, y = 0;

            bool ok = false;
            try
            {
                Line2 l0 = validate(compute_weighted_line_coeffC2(tri.collinear_edge(), tri.collinear_edge_weight()));
                Line2 l2 = validate(compute_weighted_line_coeffC2(tri.non_collinear_edge(), tri.non_collinear_edge_weight()));
                Point2 q = validate(construct_degenerate_seed_pointC2(tri));

                var res = (CompareResultEnum)compare(tri.collinear_edge_weight(), tri.other_collinear_edge_weight());
                if (res == CompareResultEnum.EQUAL)
                {
                    FT px, py;
                    line_project_point(l0.a(), l0.b(), l0.c(), q.x(), q.y(), out px, out py);

                    CGAL_STSKEL_TRAITS_TRACE($"Degenerate, equal weights  {tri.collinear_edge_weight()}");

                    CGAL_STSKEL_TRAITS_TRACE($"Seed point:  {q} . Projected seed point: ({px} ,{py}) ");
                    FT l0a = l0.a();
                    FT l0b = l0.b();
                    FT l0c = l0.c();
                    FT l2a = l2.a();
                    FT l2b = l2.b();
                    FT l2c = l2.c();

                    // See details in compute_degenerate_offset_lines_isec_timeC2()
                    FT sq_w0 = square(l0a) + square(l0b);

                    // Note that "* sq_w0" is removed from the numerator expression.
                    //
                    // This is because the speed is inverted while representing the front
                    // progression using the orthogonal normalized vector [l0a, l0b]: P = Q + t/w * V with V normalized.
                    // However, here l0a & l0b are not normalized but *weighted* coeff, so we need to divide by w0².
                    // Hence we can just avoid multiplying by w0² in the numerator in the first place.
                    FT num, den;
                    if (!Mathex.is_zero(l0.b())) // Non-vertical
                    {
                        num = ((l2a * l0b - l0a * l2b) * px - l2b * l0c + l0b * l2c) ;// * sq_w0  ;
                        den = l0a * l0a * l2b - l2b * sq_w0 + l0b * sq_w0 - l0a * l2a * l0b;
                    }
                    else
                    {
                        num = ((l2a * l0b - l0a * l2b) * py - l0a * l2c + l2a * l0c); //  * sq_w0  ;
                        den = l0a * l0b * l2b - l0b * l0b * l2a + l2a * sq_w0 - l0a * sq_w0;
                    }

                    if (!Mathex.is_zero(den) && is_finite(den) && is_finite(num))
                    {
                        x = px + l0a * num / den;
                        y = py + l0b * num / den;

                        ok = is_finite(x) && is_finite(y);
                    }
                }
                else
                {
                    CGAL_STSKEL_TRAITS_TRACE($"Degenerate, different weights {tri.collinear_edge_weight()} and {tri.other_collinear_edge_weight()}");

                    FT l0a = l0.a(); FT l0b = l0.b(); FT l0c = l0.c();
                    FT l2a = l2.a(); FT l2b = l2.b(); FT l2c = l2.c();

                    // The line parallel to l0 (and l1) passing through q is: l0a*x + l0b*y + lambda = 0, with
                    FT lambda = -l0a * q.x() - l0b * q.y();

                    // The bisector between l0 (l1) and l2 is:
                    //  l0a*x + l0b*y + l0c - t = 0
                    //  l2a*x + l2b*y + l2c - t = 0

                    // The intersection point is thus:
                    //  l0a*x + l0b*y + l0c - t = 0
                    //  l2a*x + l2b*y + l2c - t = 0
                    //  l0a*x + l0b*y + lambda = 0

                    // const FT t = l0c - lambda ; // (3) - (1)
                    FT den = l2a * l0b - l0a * l2b;

                    if (!Mathex.is_zero(den) && is_finite(den))

                        x = (l0b * l0c - l0b * (l2c + lambda) + l2b * lambda) / den;
                    y = -(l0a * l0c - l0a * (l2c + lambda) + l2a * lambda) / den;
                }
                CGAL_STSKEL_TRAITS_TRACE($"Degenerate {(Mathex.is_zero(l0.b()) ? "(vertical)" : "")}  event point:  x={x} y={y}");

                return ok ? new Point2(x, y) : null;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            return null;
        }

        // Calls the appropriate function depending on the collinearity of the edges.
        //CACHE//
        public  Point_2? construct_offset_lines_isecC2(Trisegment tri)
        {
            CGAL_STSKEL_TRAITS_TRACE("construct_offset_lines_isecC2( tri.Id ) [ typeid(FT).name() ");


            if (tri.OffsetLinesIsecPoint != null) return tri.OffsetLinesIsecPoint;
            //if (aCaches.mPoint_cache.IsCached(tri.Id))
            //    return aCaches.mPoint_cache.Get(tri.Id);

            CGAL_precondition(tri.collinearity() != Trisegment_collinearity.TRISEGMENT_COLLINEARITY_ALL);

            Point_2? rRes = tri.collinearity() == Trisegment_collinearity.TRISEGMENT_COLLINEARITY_NONE ? construct_normal_offset_lines_isecC2(tri)
                                                                    : construct_degenerate_offset_lines_isecC2(tri);

            //  aCaches.mPoint_cache.Set(tri.Id, rRes);

            tri.OffsetLinesIsecPoint = rRes;

            return rRes;
        }
  
    
    }
}