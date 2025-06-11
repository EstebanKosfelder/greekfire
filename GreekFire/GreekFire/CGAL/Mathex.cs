

using FT = System.Double;

namespace CGAL
{
    using Vector_2 = Vector2;

    public static partial class Mathex
    {
        public const FT EPS = 1e-9;
        public const FT EPS2 = EPS* EPS;
        public static FT PI => Math.PI;

        public static bool are_near(FT a, FT b, FT eps = EPS)
        {
            if (!FT.IsRealNumber(a) || !FT.IsRealNumber(b)) throw new Exception();
            return (Math.Abs(a - b) < eps);
        }

        public static bool are_near(Point2 a, Point2 b, FT eps = EPS)
        {
            return are_near(a.X, b.X, eps) && are_near(a.Y, b.Y, eps);
        }

        public static bool AreNear(this FT a, FT b, FT eps = EPS)
        {
            if (!FT.IsRealNumber(a) || !FT.IsRealNumber(b)) throw new Exception();
            return (Math.Abs(a - b) < eps);
        }

        public static void Assert(bool v)
        {
            if (!v)
                throw new Exception();
        }

        public static void Assert(bool v, string message)
        {
            if (!v)
                throw new Exception(message);
        }

        public static int compare(FT a, FT b, FT eps = EPS)
        {
            return are_near(a, b, eps) ? 0 : a.CompareTo(b);
        }

        //CompareResultEnum?
        public static UncertainCompareResult compare_isec_anglesC2(in Vector_2 aBV1, in Vector_2 aBV2,
                                                  in Vector_2 aLV, in Vector_2 aRV)
        {
            UncertainCompareResult rResult = UncertainCompareResult.indeterminate;
            Vector_2 lBisectorDirection = aBV2 - aBV1;
            FT lLNorm = inexact_sqrt(compute_scalar_product_2(aLV, aLV));
            FT lRNorm = inexact_sqrt(compute_scalar_product_2(aRV, aRV));

            if ((bool)(Not(certified_is_positive(lLNorm))
                   .Or(Not(certified_is_positive(lRNorm)))))
                return rResult;

            var aLVn = aLV / lLNorm;
            var aRVn = aRV / lRNorm;

            FT lLSp = compute_scalar_product_2(lBisectorDirection, aLVn);
            FT lRSp = compute_scalar_product_2(lBisectorDirection, aRVn);

            // Smaller if the scalar product is larger, so swapping
            rResult = certified_compare(lRSp, lLSp);

            return rResult;
        }

        public static UncertainCompareResult compare_ss_event_angles_2(in Vector_2 aBV1, in Vector_2 aBV2,
                                                  in Vector_2 aLV, in Vector_2 aRV)
        {
            return compare_isec_anglesC2(aBV1, aBV2, aLV, aRV);

        }

        public static int compare_x(Point2 p, Point2 q, FT eps = EPS)
        {
            return compare(p.X, q.X, eps);
        }

        public static int compare_xy(Point2 p, Point2 q, FT eps = EPS)
        {

            var phx = p.hx();
            var phy = p.hy();
            var phw = p.hw();
            var qhx = q.hx();
            var qhy = q.hy();
            var qhw = q.hw();

            var pV = phx * qhw;
            var qV = qhx * phw;
            if (are_near(pV, qV))
            {
                pV = phy * qhw;
                qV = qhy * phw;
            }
            return compare(pV, qV);
        }

        public static int compare_y(Point2 p, Point2 q, FT eps = EPS)
        {
            return compare(p.Y, q.Y, eps);
        }

        public static Polynomial1D compute_determinant(Polynomial1D x0, Polynomial1D y0,
                          Polynomial1D x1, Polynomial1D y1,
                          Polynomial1D x2, Polynomial1D y2)
        {
            var result = (
              (x0 * y1
              + x1 * y2
              + x2 * y0
              )
              -
              (y0 * x1
              + y1 * x2
              + y2 * x0
              )
            );
            return result;
        }

        public static FT compute_scalar_product_2(CGAL.Vector2 a, CGAL.Vector2 b) => a.X * b.X + a.Y * b.Y;

        public static Vector2 construct_opposite_vector_2(Vector2 v) => -v;

        public static Direction2 construct_perpendicular_direction_2(Direction2 d, OrientationEnum o)
        {
            DebuggerInfo.CGAL_kernel_precondition(o != OrientationEnum.COLLINEAR);
            if (o == OrientationEnum.COUNTERCLOCKWISE)
                return new Direction2(-d.dy(), d.dx());
            else
                return new Direction2(d.dy(), -d.dx());
        }

        public static FT determinant(FT a00, FT a01, FT a10, FT a11)
        {
            // First compute the det2x2
            FT m01 = a00 * a11 - a10 * a01;
            return m01;
        }

        public static FT determinant(FT a00, FT a01, FT a02, FT a10, FT a11, FT a12, FT a20, FT a21, FT a22)
        {
            // First compute the det2x2
            FT m01 = a00 * a11 - a10 * a01;
            FT m02 = a00 * a21 - a20 * a01;
            FT m12 = a10 * a21 - a20 * a11;
            // Now compute the minors of rank 3
            FT m012 = m01 * a22 - m02 * a12 + m12 * a02;
            return m012;
        }

        public static FT determinant(FT a00, FT a01, FT a02, FT a03, FT a10, FT a11, FT a12, FT a13, FT a20, FT a21, FT a22, FT a23, FT a30, FT a31, FT a32, FT a33)
        {
            // First compute the det2x2
            FT m01 = a10 * a01 - a00 * a11;
            FT m02 = a20 * a01 - a00 * a21;
            FT m03 = a30 * a01 - a00 * a31;
            FT m12 = a20 * a11 - a10 * a21;
            FT m13 = a30 * a11 - a10 * a31;
            FT m23 = a30 * a21 - a20 * a31;
            // Now compute the minors of rank 3
            FT m012 = m12 * a02 - m02 * a12 + m01 * a22;
            FT m013 = m13 * a02 - m03 * a12 + m01 * a32;
            FT m023 = m23 * a02 - m03 * a22 + m02 * a32;
            FT m123 = m23 * a12 - m13 * a22 + m12 * a32;
            // Now compute the minors of rank 4
            FT m0123 = m123 * a03 - m023 * a13 + m013 * a23 - m012 * a33;
            return m0123;
        }

        public static FT determinant(FT a00, FT a01, FT a02, FT a03, FT a04, FT a10, FT a11, FT a12, FT a13, FT a14, FT a20, FT a21, FT a22, FT a23, FT a24, FT a30, FT a31, FT a32, FT a33, FT a34, FT a40, FT a41, FT a42, FT a43, FT a44)
        {
            // First compute the det2x2
            FT m01 = a10 * a01 - a00 * a11;
            FT m02 = a20 * a01 - a00 * a21;
            FT m03 = a30 * a01 - a00 * a31;
            FT m04 = a40 * a01 - a00 * a41;
            FT m12 = a20 * a11 - a10 * a21;
            FT m13 = a30 * a11 - a10 * a31;
            FT m14 = a40 * a11 - a10 * a41;
            FT m23 = a30 * a21 - a20 * a31;
            FT m24 = a40 * a21 - a20 * a41;
            FT m34 = a40 * a31 - a30 * a41;
            // Now compute the minors of rank 3
            FT m012 = m12 * a02 - m02 * a12 + m01 * a22;
            FT m013 = m13 * a02 - m03 * a12 + m01 * a32;
            FT m014 = m14 * a02 - m04 * a12 + m01 * a42;
            FT m023 = m23 * a02 - m03 * a22 + m02 * a32;
            FT m024 = m24 * a02 - m04 * a22 + m02 * a42;
            FT m034 = m34 * a02 - m04 * a32 + m03 * a42;
            FT m123 = m23 * a12 - m13 * a22 + m12 * a32;
            FT m124 = m24 * a12 - m14 * a22 + m12 * a42;
            FT m134 = m34 * a12 - m14 * a32 + m13 * a42;
            FT m234 = m34 * a22 - m24 * a32 + m23 * a42;
            // Now compute the minors of rank 4
            FT m0123 = m123 * a03 - m023 * a13 + m013 * a23 - m012 * a33;
            FT m0124 = m124 * a03 - m024 * a13 + m014 * a23 - m012 * a43;
            FT m0134 = m134 * a03 - m034 * a13 + m014 * a33 - m013 * a43;
            FT m0234 = m234 * a03 - m034 * a23 + m024 * a33 - m023 * a43;
            FT m1234 = m234 * a13 - m134 * a23 + m124 * a33 - m123 * a43;
            // Now compute the minors of rank 5
            FT m01234 = m1234 * a04 - m0234 * a14 + m0134 * a24 - m0124 * a34 + m0123 * a44;
            return m01234;
        }

        public static FT determinant(FT a00, FT a01, FT a02, FT a03, FT a04, FT a05, FT a10, FT a11, FT a12, FT a13, FT a14, FT a15, FT a20, FT a21, FT a22, FT a23, FT a24, FT a25, FT a30, FT a31, FT a32, FT a33, FT a34, FT a35, FT a40, FT a41, FT a42, FT a43, FT a44, FT a45, FT a50, FT a51, FT a52, FT a53, FT a54, FT a55)
        {
            // First compute the det2x2
            FT m01 = a00 * a11 - a10 * a01;
            FT m02 = a00 * a21 - a20 * a01;
            FT m03 = a00 * a31 - a30 * a01;
            FT m04 = a00 * a41 - a40 * a01;
            FT m05 = a00 * a51 - a50 * a01;
            FT m12 = a10 * a21 - a20 * a11;
            FT m13 = a10 * a31 - a30 * a11;
            FT m14 = a10 * a41 - a40 * a11;
            FT m15 = a10 * a51 - a50 * a11;
            FT m23 = a20 * a31 - a30 * a21;
            FT m24 = a20 * a41 - a40 * a21;
            FT m25 = a20 * a51 - a50 * a21;
            FT m34 = a30 * a41 - a40 * a31;
            FT m35 = a30 * a51 - a50 * a31;
            FT m45 = a40 * a51 - a50 * a41;
            // Now compute the minors of rank 3
            FT m012 = m01 * a22 - m02 * a12 + m12 * a02;
            FT m013 = m01 * a32 - m03 * a12 + m13 * a02;
            FT m014 = m01 * a42 - m04 * a12 + m14 * a02;
            FT m015 = m01 * a52 - m05 * a12 + m15 * a02;
            FT m023 = m02 * a32 - m03 * a22 + m23 * a02;
            FT m024 = m02 * a42 - m04 * a22 + m24 * a02;
            FT m025 = m02 * a52 - m05 * a22 + m25 * a02;
            FT m034 = m03 * a42 - m04 * a32 + m34 * a02;
            FT m035 = m03 * a52 - m05 * a32 + m35 * a02;
            FT m045 = m04 * a52 - m05 * a42 + m45 * a02;
            FT m123 = m12 * a32 - m13 * a22 + m23 * a12;
            FT m124 = m12 * a42 - m14 * a22 + m24 * a12;
            FT m125 = m12 * a52 - m15 * a22 + m25 * a12;
            FT m134 = m13 * a42 - m14 * a32 + m34 * a12;
            FT m135 = m13 * a52 - m15 * a32 + m35 * a12;
            FT m145 = m14 * a52 - m15 * a42 + m45 * a12;
            FT m234 = m23 * a42 - m24 * a32 + m34 * a22;
            FT m235 = m23 * a52 - m25 * a32 + m35 * a22;
            FT m245 = m24 * a52 - m25 * a42 + m45 * a22;
            FT m345 = m34 * a52 - m35 * a42 + m45 * a32;
            // Now compute the minors of rank 4
            FT m0123 = m012 * a33 - m013 * a23 + m023 * a13 - m123 * a03;
            FT m0124 = m012 * a43 - m014 * a23 + m024 * a13 - m124 * a03;
            FT m0125 = m012 * a53 - m015 * a23 + m025 * a13 - m125 * a03;
            FT m0134 = m013 * a43 - m014 * a33 + m034 * a13 - m134 * a03;
            FT m0135 = m013 * a53 - m015 * a33 + m035 * a13 - m135 * a03;
            FT m0145 = m014 * a53 - m015 * a43 + m045 * a13 - m145 * a03;
            FT m0234 = m023 * a43 - m024 * a33 + m034 * a23 - m234 * a03;
            FT m0235 = m023 * a53 - m025 * a33 + m035 * a23 - m235 * a03;
            FT m0245 = m024 * a53 - m025 * a43 + m045 * a23 - m245 * a03;
            FT m0345 = m034 * a53 - m035 * a43 + m045 * a33 - m345 * a03;
            FT m1234 = m123 * a43 - m124 * a33 + m134 * a23 - m234 * a13;
            FT m1235 = m123 * a53 - m125 * a33 + m135 * a23 - m235 * a13;
            FT m1245 = m124 * a53 - m125 * a43 + m145 * a23 - m245 * a13;
            FT m1345 = m134 * a53 - m135 * a43 + m145 * a33 - m345 * a13;
            FT m2345 = m234 * a53 - m235 * a43 + m245 * a33 - m345 * a23;
            // Now compute the minors of rank 5
            FT m01234 = m0123 * a44 - m0124 * a34 + m0134 * a24 - m0234 * a14 + m1234 * a04;
            FT m01235 = m0123 * a54 - m0125 * a34 + m0135 * a24 - m0235 * a14 + m1235 * a04;
            FT m01245 = m0124 * a54 - m0125 * a44 + m0145 * a24 - m0245 * a14 + m1245 * a04;
            FT m01345 = m0134 * a54 - m0135 * a44 + m0145 * a34 - m0345 * a14 + m1345 * a04;
            FT m02345 = m0234 * a54 - m0235 * a44 + m0245 * a34 - m0345 * a24 + m2345 * a04;
            FT m12345 = m1234 * a54 - m1235 * a44 + m1245 * a34 - m1345 * a24 + m2345 * a14;
            // Now compute the minors of rank 6
            FT m012345 = m01234 * a55 - m01235 * a45 + m01245 * a35 - m01345 * a25
                             + m02345 * a15 - m12345 * a05;
            return m012345;
        }

        public static (int, int, int) indirect_sort_3<T>(T[] t) where T : IComparable
        {
            int i0 = 0;
            int i1 = 1;
            int i2 = 2;
            if (t[i0].CompareTo(t[i1]) > 0) swap(ref i0, ref i1);
            if (t[i1].CompareTo(t[i2]) > 0) swap(ref i1, ref i2);
            if (t[i0].CompareTo(t[i1]) > 0) swap(ref i0, ref i1);
            return (i0, i1, i2);
        }

        public static FT inexact_sqrt(FT a) => Math.Sqrt(a);

        public static (Point2 c, double r) InnerCircle(params Point2[] v)
        {
            var a = distance(v[1], v[0]);
            var b = distance(v[2], v[1]);
            var c = distance(v[0], v[2]);

            //var t = (v[0] + v[1] + v[2]);
            //var center = t / 3.0;
            // Calculate the semiperimeter
            var s = (a + b + c) / 2.0;

            // Calculate the area using Heron's formula
            var area = Math.Sqrt(s * (s - a) * (s - b) * (s - c));

            // Calculate the inradius
            var inradius = area / s;

            // Calculate the incenter using weighted average of vertices
            var center = (v[2] * a + v[0] * b + v[1] * c) / (a + b + c);

            return (center, inradius);
        }

        public static Intersection intersection(Ray2 ray, Segment2 seg) => new Ray2Segment2Intersection(ray, seg);

        public static Intersection intersection(Line2 a, Line2 b) => new Line_2_Line_2_pair(a, b);

        public static bool is_finite(FT a) => FT.IsFinite(a);

        public static bool is_positive(FT a) => sign(a) == 1;

        public static bool IsFinite(this FT a) => is_finite(a);

        public static bool IsZero(this FT a, double eps = EPS) => is_zero(a, eps);

        public static Point2 midpoint(in Point2 a, in Point2 b) => (a + b) * 0.5;

        public static int orientation(Point2 u, Point2 v)
        {
            return sign_of_determinant(u.X, u.Y, v.X, v.Y);
        }

        public static OrientationEnum orientation(Vector2 u, Vector2 v)
        {
            return (OrientationEnum)sign_of_determinant(u.X, u.Y, v.X, v.Y);
        }

        public static int oriented(FT a, FT eps = EPS)
        {
            if (are_near(a, 0, eps)) return 1;
            return Math.Sign(a);
        }

        //    (OrientationEnum)sign_of_determinant(q.X - p.X, q.Y - p.Y, r.X - p.X, r.Y - p.Y);
        public static void Postcondition(bool v)
        {
            if (!v)
                throw new Exception(); ;
        }

        //public static OrientationEnum orientation(in Point2 p, in Point2 q, in Point2 r) =>
        public static void Precondition_msg(bool v1, string v2)
        {
            Assert(v1, v2);
        }

        public static double Proporcional(this double from, double to, double time)
        {
            return (to - from) * time + from; // is de same (1 - time) * from + time * to;
        }

        public static int side_of(FT a, FT eps = EPS)
        {
            if (are_near(a, 0, eps)) return 0;
            return Math.Sign(a);
        }

        public static int sign(FT vaule, FT eps = EPS) => is_zero(vaule) ? 0 : Math.Sign(vaule);

        public static int sign_of_determinant(FT a00, FT a01, FT a10, FT a11)
        {
            return compare((a00 * a11), (a10 * a01));
        }

        public static int sign_of_determinant(FT a00, FT a01, FT a02, FT a10, FT a11, FT a12, FT a20, FT a21, FT a22)
        {
            return sign(determinant(a00, a01, a02, a10, a11, a12, a20, a21, a22));
        }

        public static int sign_of_determinant(FT a00, FT a01, FT a02, FT a03, FT a10, FT a11, FT a12, FT a13, FT a20, FT a21, FT a22, FT a23, FT a30, FT a31, FT a32, FT a33)
        {
            return sign(determinant(a00, a01, a02, a03, a10, a11, a12, a13, a20, a21, a22, a23, a30, a31, a32, a33));
        }

        public static int sign_of_determinant(FT a00, FT a01, FT a02, FT a03, FT a04, FT a10, FT a11, FT a12, FT a13, FT a14, FT a20, FT a21, FT a22, FT a23, FT a24, FT a30, FT a31, FT a32, FT a33, FT a34, FT a40, FT a41, FT a42, FT a43, FT a44)
        {
            return sign(determinant(a00, a01, a02, a03, a04,
                                                   a10, a11, a12, a13, a14,
                                                   a20, a21, a22, a23, a24,
                                                   a30, a31, a32, a33, a34,
                                                   a40, a41, a42, a43, a44));
        }

        public static int sign_of_determinant(FT a00, FT a01, FT a02, FT a03, FT a04, FT a05, FT a10, FT a11, FT a12, FT a13, FT a14, FT a15, FT a20, FT a21, FT a22, FT a23, FT a24, FT a25, FT a30, FT a31, FT a32, FT a33, FT a34, FT a35, FT a40, FT a41, FT a42, FT a43, FT a44, FT a45, FT a50, FT a51, FT a52, FT a53, FT a54, FT a55)
        {
            return sign(determinant(a00, a01, a02, a03, a04, a05, a10, a11, a12, a13, a14, a15, a20, a21, a22, a23, a24, a25, a30, a31, a32, a33, a34, a35, a40, a41, a42, a43, a44, a45, a50, a51, a52, a53, a54, a55));
        }

        public static (bool has_real_roots, bool is_square) solve_quadratic(Polynomial1D f, out double x0, out double x1) => f.solve_quadratic(out x0, out x1);
        public static FT square(FT value) => value * value;

        public static void swap<T>(ref T a, ref T b) { (b, a) = (a, b); }
        public static FT validate(FT value)
        {
            if (!is_finite(value))
                throw new ArithmeticOverflowException();
            return value;
        }

        public static Point2 validate(Point2 value)
        {
            validate(value.X);
            validate(value.Y);
            return value;
        }

        public static Vector2 validate(Vector2 value)
        {
            validate(value.X);
            validate(value.Y);
            return value;
        }
    }
}