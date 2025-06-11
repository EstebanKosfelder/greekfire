using CGAL;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using FT = System.Double;
using static CGAL.DebuggerInfo;
using System.Text;

namespace CGAL
{
    public enum Trisegment_collinearity
    {
        TRISEGMENT_COLLINEARITY_01,
        TRISEGMENT_COLLINEARITY_12,
        TRISEGMENT_COLLINEARITY_02,
        TRISEGMENT_COLLINEARITY_ALL,
        TRISEGMENT_COLLINEARITY_NONE
    }
    public enum SEED_ID
    { LEFT, RIGHT, THIRD };


    public class Trisegment
    {
        public  static Trisegment NULL { get; private set; }
        static Trisegment()
            {
            NULL = new Trisegment(Segment2.NULL, 1, Segment2.NULL, 1, Segment2.NULL, 1, Trisegment_collinearity.TRISEGMENT_COLLINEARITY_ALL, -1);
            }


        private int mID;
        private Segment2[] mE;
        private FT[] mW;
        private Trisegment_collinearity mCollinearity;
        private byte mCSIdx, mNCSIdx;

        private Trisegment mChildL = Trisegment.NULL;
        private Trisegment mChildR = Trisegment.NULL;

        // this is the potential child of e2-e0, if it exists. It is used only in the configuration
        // of e0 and e2 collinear as the common child gives where the bisector starts (as it is not
        // necessarily the middle of the gap between e2 and e0).
        private Trisegment mChildT = Trisegment.NULL;

        public Trisegment(Segment2 aE0
                   , FT aW0
                   , Segment2 aE1
                   , FT aW1
                   , Segment2 aE2
                   , FT aW2
                   , Trisegment_collinearity aCollinearity
                   , int aID
                   )

        {
            mE = new Segment2[3];
            mW = new FT[3];
            Id = aID;
            mCollinearity = aCollinearity;

            mE[0] = aE0;
            mE[1] = aE1;
            mE[2] = aE2;

            mW[0] = aW0;
            mW[1] = aW1;
            mW[2] = aW2;

            switch (mCollinearity)
            {
                case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_01:
                    mCSIdx = 0; mNCSIdx = 2; break;

                case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_12:
                    mCSIdx = 1; mNCSIdx = 0; break;

                case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_02:
                    mCSIdx = 0; mNCSIdx = 1; break;

                case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_ALL:
                    mCSIdx = mNCSIdx = byte.MaxValue; break;

                case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_NONE:
                    mCSIdx = mNCSIdx = byte.MaxValue; break;
            }
        }

        public int Id { get; set; }
        public Rational? OffsetLinesIsecTimes { get; internal set; } = null;
        public Point2? OffsetLinesIsecPoint { get; internal set; }

        public int id() => Id;
        

        public Trisegment_collinearity collinearity()
        { return mCollinearity; }

        public Segment2 e(int idx)
        { CGAL_precondition(idx < 3); return mE[idx]; }

        public Segment2 e0()
        { return e(0); }

        public Segment2 e1()
        { return e(1); }

        public Segment2 e2()
        { return e(2); }

        public FT w(int idx)
        { CGAL_precondition(idx < 3); return mW[idx]; }

        public FT w0()
        { return w(0); }

        public FT w1()
        { return w(1); }

        public FT w2()
        { return w(2); }

        // If 2 out of the 3 edges are collinear they can be reclassified as 1 collinear edge (any of the 2) and 1 non-collinear.
        // These methods returns the edges according to that classification.
        // PRECONDITION: Exactly 2 out of 3 edges are collinear
        public Segment2 collinear_edge()
        { return e(mCSIdx); }

        public Segment2 non_collinear_edge()
        { return e(mNCSIdx); }

        public Segment2 other_collinear_edge()
        {
            switch (mCollinearity)
            {
                case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_01:
                    return e(1);

                case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_12:
                    return e(2);

                case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_02:
                    return e(2);

                default:
                    CGAL_assertion(false);
                    return e(0); // arbitrary, meaningless value because a  is expected
            }
        }

        public FT collinear_edge_weight()
        { return w(mCSIdx); }

        public FT non_collinear_edge_weight()
        { return w(mNCSIdx); }

        public FT other_collinear_edge_weight()
        {
            switch (mCollinearity)
            {
                case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_01:
                    return w(1);

                case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_12:
                    return w(2);

                case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_02:
                    return w(2);

                default:
                    CGAL_assertion(false);
                    return w(0); // arbitrary, meaningless value because a  is expected
            }
        }

        public Trisegment child_l()
        { return mChildL; }

        public Trisegment child_r()
        { return mChildR; }

        public Trisegment child_t()
        { return mChildT; }

        public void set_child_l(Trisegment aChild)
        { mChildL = aChild; }

        public void set_child_r(Trisegment aChild)
        { mChildR = aChild; }

        public void set_child_t(Trisegment aChild)
        { mChildT = aChild; }

        // Indicates which of the seeds is collinear for a normal collinearity case.
        // PRECONDITION: The collinearity is normal.
        public SEED_ID degenerate_seed_id()
        {
            Trisegment_collinearity c = collinearity();

            return c == Trisegment_collinearity.TRISEGMENT_COLLINEARITY_01 ? SEED_ID.LEFT : c == Trisegment_collinearity.TRISEGMENT_COLLINEARITY_12 ? SEED_ID.RIGHT : SEED_ID.THIRD;
        }

        public override string ToString()
        {
            return this.ToString(0);
        }

        public string ToString(int aDepth)
        {
            var lPadding = new String(' ', aDepth * 2);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{lPadding}T{this.Id}");
            sb.AppendLine($"{lPadding}E{this.e1().Id} E{this.e2().Id}");
            sb.AppendLine($"{lPadding}{this.e0()} w = {this.w0()};");
            sb.AppendLine($"{lPadding}{this.e1()} w = {this.w0()};");
            sb.AppendLine($"{lPadding}{this.e2()} w = {this.w0()};");
            sb.AppendLine($"{lPadding}\tCollinearity: {this.collinearity()}");
            sb.AppendLine($"{lPadding}");
            return sb.ToString();
        }

        public static string recursive_print(Trisegment aTriPtr, int aDepth)
        {
            string lPadding = new string('_', 2 * aDepth);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();

            if (aTriPtr != Trisegment.NULL)
            {
                sb.Append(aTriPtr.ToString(aDepth));

                if (aTriPtr.child_l() != Trisegment.NULL)
                {
                    sb.Append($"{lPadding}left child:");
                    sb.Append(recursive_print(aTriPtr.child_l(), aDepth + 1));
                }

                if (aTriPtr.child_r() != Trisegment.NULL)
                {
                    sb.Append($"{lPadding}left child:");
                    sb.Append(recursive_print(aTriPtr.child_r(), aDepth + 1));
                }

                if (aTriPtr.child_t() != Trisegment.NULL)
                {
                    sb.Append($"{lPadding}third child::");
                    sb.Append(recursive_print(aTriPtr.child_t(), aDepth + 1));
                }
            }
            else
            {
                sb.Append("{null}");
            }
            return sb.ToString();
        }

#if USING_CACHE
        public  void ResetCache()
        {
            OffsetLinesIsecPoint = null;
            OffsetLinesIsecTimes = null;
        }
#endif

        //public class Trisegment
        //{
        //    //public:

        //    //  typedef Segment_2 Segment_2;

        //    //    typedef intrusive_ptr<Trisegment_2> Self_ptr ;

        //    public Trisegment_collinearity Collinearity { get; private set; }
        //    public Segment E0 { get; private set; }
        //    public Segment E1 { get; private set; }
        //    public Segment E2 { get; private set; }

        //    public FT W0 { get; private set; }
        //    public FT W1 { get; private set; }
        //    public FT W2 { get; private set; }

        //    public int CSIdx { get; private set; }
        //    public int NCSIdx { get; private set; }

        //    public Trisegment ChildL { get; set; }
        //    public Trisegment ChildR { get; set; }

        //    public Trisegment(Segment aE0
        //           , Segment aE1
        //           , Segment aE2
        //           , FT aW0=1, FT aW1=1, FT aW2=1)

        //    {
        //        Trisegment_collinearity aCollinearity;

        //        bool is_01 = Segment.AreEdgesOrderlyCollinear(aE0, aE1);
        //        bool is_02 = Segment.AreEdgesOrderlyCollinear(aE0, aE2);
        //        bool is_12 = Segment.AreEdgesOrderlyCollinear(aE1, aE2);

        //        if (is_01 && !is_02 && !is_12) aCollinearity = Trisegment_collinearity.TRISEGMENT_COLLINEARITY_01;
        //        else if (is_02 && !is_01 && !is_12) aCollinearity = Trisegment_collinearity.TRISEGMENT_COLLINEARITY_02;
        //        else if (is_12 && !is_01 && !is_02) aCollinearity = Trisegment_collinearity.TRISEGMENT_COLLINEARITY_12;
        //        else if (!is_01 && !is_02 && !is_12) aCollinearity = Trisegment_collinearity.TRISEGMENT_COLLINEARITY_NONE;
        //        else aCollinearity = Trisegment_collinearity.TRISEGMENT_COLLINEARITY_ALL;

        //        Collinearity = aCollinearity;

        //        E0 = aE0;
        //        E1 = aE1;
        //        E2 = aE2;
        //        W0 = aW0;
        //        W1 = aW1;
        //        W2 = aW2;
        //        Id = aId;

        //        switch (Collinearity)
        //        {
        //            case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_01:
        //                CSIdx = 0; NCSIdx = 2; break;

        //            case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_12:
        //                CSIdx = 1; NCSIdx = 0; break;

        //            case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_02:
        //                CSIdx = 0; NCSIdx = 1; break;

        //            case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_ALL:
        //                CSIdx = NCSIdx = -1; break;

        //            case Trisegment_collinearity.TRISEGMENT_COLLINEARITY_NONE:
        //                CSIdx = NCSIdx = -1; break;
        //        }
        //    }

        //    internal Trisegment_collinearity collinearity() => Collinearity;

        //    internal Segment e(int idx)
        //    {
        //        Extensions.Precondition(idx < 3); return idx == 0 ? E0 : idx == 1 ? E1 : E2;
        //    }

        //    internal Segment e0()
        //    {
        //        return E0;
        //    }

        //    internal Segment e1()
        //    {
        //        return E1;
        //    }

        //    internal Segment e2()
        //    {
        //        return E2;
        //    }

        //    // If 2 out of the 3 edges are collinear they can be reclassified as 1 collinear edge (any of the 2) and 1 non-collinear.
        //    // These methods returns the edges according to that classification.
        //    // PRECONDITION: Exactly 2 out of 3 edges are collinear
        //    internal Segment collinear_edge() { return e(CSIdx); }

        //    internal Segment non_collinear_edge()
        //    {
        //        return e(NCSIdx);
        //    }

        //    internal Trisegment child_l()
        //    {
        //        return ChildL;
        //    }

        //    internal Trisegment child_r()
        //    {
        //        return ChildR;
        //    }

        //    internal void set_child_l(Trisegment aChild)
        //    {
        //        ChildL = aChild;
        //    }

        //    internal void set_child_r(Trisegment aChild)
        //    {
        //        ChildR = aChild;
        //    }

        //    // Indicates which of the seeds is collinear for a normal collinearity case.
        //    // PRECONDITION: The collinearity is normal.
        //    internal SEED_ID degenerate_seed_id()
        //    {
        //        Trisegment_collinearity c = collinearity();

        //        return c == Trisegment_collinearity.TRISEGMENT_COLLINEARITY_01 ? SEED_ID.LEFT : c == Trisegment_collinearity.TRISEGMENT_COLLINEARITY_12 ? SEED_ID.RIGHT : SEED_ID.UNKNOWN;
        //    }

        //    public override string ToString()
        //    {
        //        return "[" + E0 + " " + E1 + " " + E2 + " " + collinearity() + "]";
        //    }

        //    internal bool exist_offset_lines_isec2(FT? aMaxTime)
        //    {
        //        bool rResult = false;
        //        if (Collinearity != Trisegment_collinearity.TRISEGMENT_COLLINEARITY_ALL)
        //        {
        //            Rational t = compute_offset_lines_isec_time();
        //            bool d_is_zero = t.D.AreNear(0);

        //            if (!d_is_zero)
        //            {
        //                rResult = t.IsPositive;

        //                if (aMaxTime != null)
        //                    rResult = t.CompareTo(new Rational(aMaxTime.Value, 1)) < 0;
        //            }
        //            else
        //            {
        //                rResult = false;
        //            }
        //        }
        //        else
        //        {
        //            rResult = false;
        //        }

        //        return rResult;
        //    }

        //    internal Rational compute_offset_lines_isec_time()
        //    {
        //        Extensions.Precondition(Collinearity != Trisegment_collinearity.TRISEGMENT_COLLINEARITY_ALL);

        //        return Collinearity == Trisegment_collinearity.TRISEGMENT_COLLINEARITY_NONE ?
        //                compute_normal_offset_lines_isec_time()
        //                : compute_degenerate_offset_lines_isec_time();
        //    }

        //    internal Point construct_offset_lines_isec()
        //    {
        //        Extensions.Precondition(this.collinearity() != Trisegment_collinearity.TRISEGMENT_COLLINEARITY_ALL);

        //        return Collinearity == Trisegment_collinearity.TRISEGMENT_COLLINEARITY_NONE ?
        //              construct_normal_offset_lines_isec()
        //            : construct_degenerate_offset_lines_isec();
        //    }

        //    private Point construct_degenerate_offset_lines_isec()
        //    {
        //        Line l0 = collinear_edge().compute_normalized_line_ceoff();
        //        Line l2 = non_collinear_edge().compute_normalized_line_ceoff();

        //        Point q = compute_degenerate_seed_point();

        //        FT num, den;

        //        var p = l0.line_project_point(q.X, q.Y);

        //        //CGAL_STSKEL_TRAITS_TRACE("Seed point: " << p2str(*q) << ". Projected seed point: (" << n2str(px) << "," << n2str(py) << ")");

        //        if (!l0.B.AreNear(0)) // Non-vertical
        //        {
        //            num = (l2.A * l0.B - l0.A * l2.B) * p.X + l0.B * l2.C - l2.B * l0.C;
        //            den = (l0.A * l0.A - 1) * l2.B + (1 - l2.A * l0.A) * l0.B;
        //        }
        //        else
        //        {
        //            num = (l2.A * l0.B - l0.A * l2.B) * p.Y - l0.A * l2.C + l2.A * l0.C;
        //            den = l0.A * l0.B * l2.B - l0.B * l0.B * l2.A + l2.A - l0.A;
        //        }

        //        if (den.is_finite() && !(den.AreNear(0)) && num.is_finite())
        //        {
        //            return new Point(p.X + l0.A * num / den, p.Y + l0.B * num / den);
        //        }

        //        throw new IndeterminateValueException();
        //    }

        //    private Point construct_normal_offset_lines_isec()
        //    {
        //        Line l0 = E0.compute_normalized_line_ceoff();
        //        Line l1 = E1.compute_normalized_line_ceoff();
        //        Line l2 = E2.compute_normalized_line_ceoff();

        //        FT den = l0.A * l2.B - l0.A * l1.B - l1.A * l2.B + l2.A * l1.B + l0.B * l1.A - l0.B * l2.A;

        //        if (!den.AreNear(0))
        //        {
        //            FT numX = l0.B * l2.C - l0.B * l1.C - l1.B * l2.C + l2.B * l1.C + l1.B * l0.C - l2.B * l0.C;
        //            FT numY = l0.A * l2.C - l0.A * l1.C - l1.A * l2.C + l2.A * l1.C + l1.A * l0.C - l2.A * l0.C;

        //            if (den.is_finite() && numX.is_finite() && numY.is_finite())

        //                return new Point() { X = numX / den, Y = -numY / den };
        //        }

        //        throw new IndeterminateValueException();
        //    }

        //    internal Rational compute_normal_offset_lines_isec_time()
        //    {
        //        FT num = 0.0, den = 0.0;

        //        // DETAILS:
        //        //
        //        // An offset line is given by:
        //        //
        //        //   a*x(t) + b*y(t) + c - t = 0
        //        //
        //        // were 't > 0' being to the left of the line.
        //        // If 3 such offset lines intersect at the same offset distance, the intersection 't',
        //        // or 'time', can be computed solving for 't' in the linear system formed by 3 such equations.
        //        // The result is :
        //        //
        //        //  t = a2*b0*c1 - a2*b1*c0 - b2*a0*c1 + b2*a1*c0 + b1*a0*c2 - b0*a1*c2
        //        //      ---------------------------------------------------------------
        //        //             -a2*b1 + a2*b0 + b2*a1 - b2*a0 + b1*a0 - b0*a1 ;

        //        bool ok = false;

        //        Line l0 = E0.compute_normalized_line_ceoff();
        //        Line l1 = E1.compute_normalized_line_ceoff();
        //        Line l2 = E2.compute_normalized_line_ceoff();
        //        num = (l2.A * l0.B * l1.C)
        //             - (l2.A * l1.B * l0.C)
        //             - (l2.B * l0.A * l1.C)
        //             + (l2.B * l1.A * l0.C)
        //             + (l1.B * l0.A * l2.C)
        //             - (l0.B * l1.A * l2.C);

        //        den = (-l2.A * l1.B)
        //             + (l2.A * l0.B)
        //             + (l2.B * l1.A)
        //             - (l2.B * l0.A)
        //             + (l1.B * l0.A)
        //             - (l0.B * l1.A);

        //        if (!num.is_finite() || !den.is_finite())
        //            throw new IndeterminateValueException();

        //        return new Rational(num, den);
        //    }

        //    internal Point compute_seed_point(SEED_ID sid)
        //    {
        //        switch (sid)
        //        {
        //            case SEED_ID.LEFT:

        //                return child_l() != null ? child_l().construct_offset_lines_isec()  // this can recurse
        //                                   : E0.compute_oriented_midpoint(E1);

        //            case SEED_ID.RIGHT:

        //                return child_r() != null ? child_r().construct_offset_lines_isec() // this can recurse
        //                                   : E1.compute_oriented_midpoint(E2);

        //            case SEED_ID.UNKNOWN:
        //            default:

        //                return E0.compute_oriented_midpoint(E2);
        //        }
        //    }

        //    //
        //    // Given the trisegment tree for an event which is known to have a normal collinearity returns the seed point
        //    // of the degenerate seed.
        //    // A normal collinearity occurs when e0,e1 or e1,e2 are collinear.

        //    internal Point compute_degenerate_seed_point()
        //    {
        //        return compute_seed_point(degenerate_seed_id());
        //    }

        //    internal Rational compute_degenerate_offset_lines_isec_time()
        //    {
        //        // DETAILS:
        //        //
        //        // For simplicity, assume e0,e1 are the collinear edges.
        //        //
        //        //   (1)
        //        //   The bisecting line of e0 and e1 is a line perpendicular to e0 (and e1)
        //        //   which passes through 'q': the degenerate offset vertex (e0*,e1*)
        //        //   This "degenerate" bisecting line is given by:
        //        //
        //        //     B0(t) = p + t*[l0.a,l0.b]
        //        //
        //        //   where p is the projection of q along l0 and l0.a,l0.b are the _normalized_ line coefficients for e0 (or e1 which is the same)
        //        //   Since [a,b] is a _unit_ vector pointing perpendicularly to the left of e0 (and e1);
        //        //   any point B0(k) is at a distance k from the line supporting e0 and e1.
        //        //
        //        //   (2)
        //        //   The bisecting line of e0 and e2 is given by the following SEL
        //        //
        //        //    l0.a*x(t) + l0.b*y(t) + l0.c + t = 0
        //        //    l2.a*x(t) + l2.b*y(t) + l2.c + t = 0
        //        //
        //        //   where (l0.a,l0.b,l0.c) and (l2.a,l2.b,l0.c) are the normalized line coefficientes of e0 and e2 resp.
        //        //
        //        //     B1(t)=[x(t),y(t)]
        //        //
        //        //   (3)
        //        //   These two bisecting lines B0(t) and B1(t) intersect (if they do) in a single point 'p' whose distance
        //        //   to the lines supporting the 3 edges is exactly 't' (since those expressions are precisely parametrized in a distance)
        //        //   Solving the following vectorial equation:
        //        //
        //        //     [x(y),y(t)] = q + t*[l0.a,l0.b]
        //        //
        //        //   for t gives the result we want.
        //        //
        //        //

        //        Line l0 = collinear_edge().compute_normalized_line_ceoff();
        //        Line l2 = non_collinear_edge().compute_normalized_line_ceoff();

        //        Point q = compute_degenerate_seed_point();

        //        FT num = (0.0), den = (0.0);

        //        Point p = l0.line_project_point(q.X, q.Y);

        //        if (!l0.B.AreNear(0)) // Non-vertical
        //        {
        //            num = (l2.A * l0.B - l0.A * l2.B) * p.X + l0.B * l2.C - l2.B * l0.C;
        //            den = (l0.A * l0.A - 1) * l2.B + (1 - l2.A * l0.A) * l0.B;
        //        }
        //        else
        //        {
        //            num = (l2.A * l0.B - l0.A * l2.B) * p.Y - l0.A * l2.C + l2.A * l0.C;
        //            den = l0.A * l0.B * l2.B - l0.B * l0.B * l2.A + l2.A - l0.A;
        //        }

        //        if (num.is_finite() && den.is_finite()) return new Rational(num, den);

        //        throw new IndeterminateValueException();
        //    }

        //    internal Oriented_side oriented_side_of_event_point_wrt_bisector(Segment e0, Segment e1, Trisegment v01_event /* can be null */, bool primary_is_0)
        //    {
        //        Point p = this.construct_offset_lines_isec();
        //        Line l0 = e0.compute_normalized_line_ceoff();
        //        Line l1 = e1.compute_normalized_line_ceoff();

        //        // Degenerate bisector?
        //        if (e0.are_edges_parallel(e1))
        //        {
        //            //   CGAL_STSKEL_TRAITS_TRACE("Bisector is not angular." );

        //            // b01 is degenerate so we don't have an *angular bisector* but a *perpendicular* bisector.
        //            // We need to compute the actual bisector line.
        //            Extensions.Assert(v01_event != null || (v01_event == null && e0.Target.AreNear(e1.Source)));

        //            Point v01 = v01_event != null ? v01_event.construct_offset_lines_isec() : e1.Source;

        //            //CGAL_STSKEL_TRAITS_TRACE("v01=" << p2str(v01) << (v01_event? " (from skelton node)" : "" ) ) ;

        //            // (a,b,c) is a line perpedincular to the primary edge through v01.
        //            // If e0 and e1 are collinear this line is the actual perpendicular bisector.
        //            //
        //            // If e0 and e1 are parallel but not collinear (then neccesarrily facing each other) this line
        //            // is NOT the bisector, but the serves to determine the side of the point (projected along the primary ege) w.r.t vertex v01.

        //            //FT a, b, c;
        //            Line l = Line.perpendicular_through_point(primary_is_0 ? l0.A : l1.A, primary_is_0 ? l0.B : l1.B, v01.X, v01.Y);

        //            return (Oriented_side)l.SideOfOriented(p);

        //            //    CGAL_STSKEL_TRAITS_TRACE("Point is at " << rResult << " side of degenerate bisector through v01 " << p2str(v01));
        //        }
        //        else // Valid (non-degenerate) angular bisector
        //        {
        //            // Scale distance from to the lines.
        //            FT sd_p_l0 = l0.Distance(p).Validate();
        //            FT sd_p_l1 = l1.Distance(p).Validate();

        //            if (sd_p_l0.Validate().AreNear(sd_p_l1.Validate()))
        //            {
        //                return Oriented_side.ON_ORIENTED_BOUNDARY;
        //            }
        //            else
        //            {
        //                if ((l0.A * l1.B).Validate().CompareToEps((l1.A * l0.B).Validate()) < 0)
        //                {
        //                    return sd_p_l0.CompareToEps(sd_p_l1) < 0 ? Oriented_side.ON_NEGATIVE_SIDE : Oriented_side.ON_POSITIVE_SIDE;
        //                }
        //                else
        //                {
        //                    return sd_p_l0.CompareToEps(sd_p_l1) > 0 ? Oriented_side.ON_NEGATIVE_SIDE : Oriented_side.ON_POSITIVE_SIDE;
        //                }
        //            }
        //        }

        //        throw new IndeterminateValueException();
        //    }

        //    internal Comparison_result compare_offset_lines_isec_times(Trisegment n)
        //    {
        //        Rational mt = this.compute_offset_lines_isec_time();
        //        Rational nt = n.compute_offset_lines_isec_time();

        //        if (mt.IsPositive && nt.IsPositive)
        //            return (Comparison_result)mt.CompareTo(nt);

        //        throw new IndeterminateValueException();
        //    }

        //    internal bool are_events_simultaneous( Trisegment r)
        //    {
        //        Rational lt = this.compute_offset_lines_isec_time();
        //        Rational rt = r.compute_offset_lines_isec_time();

        //        if (lt.IsPositive && rt.IsPositive)
        //        {
        //                if (lt.CompareTo(rt)==0)
        //                {
        //                        Point li = construct_offset_lines_isec();
        //                        Point ri = r.construct_offset_lines_isec();

        //                        return li.AreNear(ri);

        //                }
        //                else return  false;
        //        }

        //        throw new IndeterminateValueException();
        //    }

        //    public static Comparison_result CompareEvents(Trisegment aA, Trisegment aB)
        //    {
        //      return  aA.compare_offset_lines_isec_times(aB);

        //    }

        //    internal (FT, Point) Construct_ss_event_time_and_point_2()
        //    {
        //        bool lOK = false;

        //        FT t = (0);
        //        Point i = Point.ORIGIN;

        //        Rational ot = compute_offset_lines_isec_time();

        //        if (!ot.D.AreNear(0))
        //        {
        //            t = ot;

        //            i = construct_offset_lines_isec();
        //            //CGAL_stskel_intrinsic_test_assertion(!is_point_calculation_clearly_wrong(t, i, aTrisegment));

        //            return (t, i);
        //        }
        //        throw new IndeterminateValueException();
        //    }
        //}
    }
    };