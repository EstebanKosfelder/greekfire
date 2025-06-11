namespace GFL.Kernel
{
  
    using System;

    //using Double=System.Int64;
    using Double = System.Double;

    public static class MathEx
    {
     

    
        public static int ToInt(this bool value)=>value?1 : 0;
        public static bool IsFinite(this Double d)
        {
            return !double.IsNaN(d) && !double.IsInfinity(d);
        }

        public static (Vector2D c, double r) InnerCircle(params Vector2D[] v)
        {
            var a = (v[1] - v[0]).Len();
            var b = (v[2] - v[1]).Len();
            var c = (v[0] - v[2]).Len();

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
        public static void swap<T>(ref T a, ref T b) { (b, a) = (a, b); }
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

        public static (bool has_real_roots, bool is_square) solve_quadratic(Polynomial1D f, out double x0, out double x1) => f.solve_quadratic(out x0, out x1);

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
        public static double Proporcional(this double from, double to, double time)
        {
            return (to - from) * time + from; // is de same (1 - time) * from + time * to;
        }
     
       
        public static int orientation(Vector2D p, Vector2D q, Vector2D r)
        {
            return orientation(p.X, p.Y, q.X, q.Y, r.X, r.Y);
        }

        public static int
                orientation(double px, double py,
                      double qx, double qy,
                      double rx, double ry)
        {
            return sign_of_determinant(qx - px, qy - py, rx - px, ry - py);
        }

        public static int orientation(Vector2D p, Vector2D q)
        {
            return orientation(p.X, p.Y, q.X, q.Y);
        }

        public static int
                orientation(double ux, double uy, double vx, double vy)
        {
            return sign_of_determinant(ux, uy, vx, vy);
        }


        public static int compare_angle_with_x_axisC2(Vector2D v1, Vector2D v2) =>compare_angle_with_x_axisC2(v1.X,v1.Y,v2.X,v2.Y);

        public static int compare_angle_with_x_axisC2(double dx1, double dy1,
                                      double dx2, double dy2)
        {
            // angles are in [-pi,pi], and the angle between Ox and d1 is compared
            // with the angle between Ox and d2
            int quadrant_1 = (dx1 >= 0) ? (dy1 >= 0 ? 1 : 4)
                : (dy1 >= 0 ? 2 : 3);
            int quadrant_2 = (dx2 >= 0) ? (dy2 >= 0 ? 1 : 4)
                : (dy2 >= 0 ? 2 : 3);
            // We can't use compare(quadrant_1,quadrant_2) because in case
            // of tie, we need additional computation
            if (quadrant_1 > quadrant_2)
                return (int)ESign.LARGER;
            else if (quadrant_1 < quadrant_2)
                return (int)ESign.SMALLER;
            return -sign_of_determinant(dx1, dy1, dx2, dy2);
        }

        public static double
                    determinant(
            double a00, double a01,
            double a10, double a11)
        {
            // First compute the det2x2
            double m01 = a00 * a11 - a10 * a01;
            return m01;
        }

        public static double
                    determinant(
            double a00, double a01, double a02,
            double a10, double a11, double a12,
            double a20, double a21, double a22)
        {
            // First compute the det2x2
            double m01 = a00 * a11 - a10 * a01;
            double m02 = a00 * a21 - a20 * a01;
            double m12 = a10 * a21 - a20 * a11;
            // Now compute the minors of rank 3
            double m012 = m01 * a22 - m02 * a12 + m12 * a02;
            return m012;
        }

        public static double
                    determinant(
            double a00, double a01, double a02, double a03,
            double a10, double a11, double a12, double a13,
            double a20, double a21, double a22, double a23,
            double a30, double a31, double a32, double a33)
        {
            // First compute the det2x2
            double m01 = a10 * a01 - a00 * a11;
            double m02 = a20 * a01 - a00 * a21;
            double m03 = a30 * a01 - a00 * a31;
            double m12 = a20 * a11 - a10 * a21;
            double m13 = a30 * a11 - a10 * a31;
            double m23 = a30 * a21 - a20 * a31;
            // Now compute the minors of rank 3
            double m012 = m12 * a02 - m02 * a12 + m01 * a22;
            double m013 = m13 * a02 - m03 * a12 + m01 * a32;
            double m023 = m23 * a02 - m03 * a22 + m02 * a32;
            double m123 = m23 * a12 - m13 * a22 + m12 * a32;
            // Now compute the minors of rank 4
            double m0123 = m123 * a03 - m023 * a13 + m013 * a23 - m012 * a33;
            return m0123;
        }

        public static double
                    determinant(
            double a00, double a01, double a02, double a03, double a04,
            double a10, double a11, double a12, double a13, double a14,
            double a20, double a21, double a22, double a23, double a24,
            double a30, double a31, double a32, double a33, double a34,
            double a40, double a41, double a42, double a43, double a44)
        {
            // First compute the det2x2
            double m01 = a10 * a01 - a00 * a11;
            double m02 = a20 * a01 - a00 * a21;
            double m03 = a30 * a01 - a00 * a31;
            double m04 = a40 * a01 - a00 * a41;
            double m12 = a20 * a11 - a10 * a21;
            double m13 = a30 * a11 - a10 * a31;
            double m14 = a40 * a11 - a10 * a41;
            double m23 = a30 * a21 - a20 * a31;
            double m24 = a40 * a21 - a20 * a41;
            double m34 = a40 * a31 - a30 * a41;
            // Now compute the minors of rank 3
            double m012 = m12 * a02 - m02 * a12 + m01 * a22;
            double m013 = m13 * a02 - m03 * a12 + m01 * a32;
            double m014 = m14 * a02 - m04 * a12 + m01 * a42;
            double m023 = m23 * a02 - m03 * a22 + m02 * a32;
            double m024 = m24 * a02 - m04 * a22 + m02 * a42;
            double m034 = m34 * a02 - m04 * a32 + m03 * a42;
            double m123 = m23 * a12 - m13 * a22 + m12 * a32;
            double m124 = m24 * a12 - m14 * a22 + m12 * a42;
            double m134 = m34 * a12 - m14 * a32 + m13 * a42;
            double m234 = m34 * a22 - m24 * a32 + m23 * a42;
            // Now compute the minors of rank 4
            double m0123 = m123 * a03 - m023 * a13 + m013 * a23 - m012 * a33;
            double m0124 = m124 * a03 - m024 * a13 + m014 * a23 - m012 * a43;
            double m0134 = m134 * a03 - m034 * a13 + m014 * a33 - m013 * a43;
            double m0234 = m234 * a03 - m034 * a23 + m024 * a33 - m023 * a43;
            double m1234 = m234 * a13 - m134 * a23 + m124 * a33 - m123 * a43;
            // Now compute the minors of rank 5
            double m01234 = m1234 * a04 - m0234 * a14 + m0134 * a24 - m0124 * a34 + m0123 * a44;
            return m01234;
        }

        public static double
                    determinant(
            double a00, double a01, double a02, double a03, double a04,
            double a05,
            double a10, double a11, double a12, double a13, double a14,
            double a15,
            double a20, double a21, double a22, double a23, double a24,
            double a25,
            double a30, double a31, double a32, double a33, double a34,
            double a35,
            double a40, double a41, double a42, double a43, double a44,
            double a45,
            double a50, double a51, double a52, double a53, double a54,
            double a55)
        {
            // First compute the det2x2
            double m01 = a00 * a11 - a10 * a01;
            double m02 = a00 * a21 - a20 * a01;
            double m03 = a00 * a31 - a30 * a01;
            double m04 = a00 * a41 - a40 * a01;
            double m05 = a00 * a51 - a50 * a01;
            double m12 = a10 * a21 - a20 * a11;
            double m13 = a10 * a31 - a30 * a11;
            double m14 = a10 * a41 - a40 * a11;
            double m15 = a10 * a51 - a50 * a11;
            double m23 = a20 * a31 - a30 * a21;
            double m24 = a20 * a41 - a40 * a21;
            double m25 = a20 * a51 - a50 * a21;
            double m34 = a30 * a41 - a40 * a31;
            double m35 = a30 * a51 - a50 * a31;
            double m45 = a40 * a51 - a50 * a41;
            // Now compute the minors of rank 3
            double m012 = m01 * a22 - m02 * a12 + m12 * a02;
            double m013 = m01 * a32 - m03 * a12 + m13 * a02;
            double m014 = m01 * a42 - m04 * a12 + m14 * a02;
            double m015 = m01 * a52 - m05 * a12 + m15 * a02;
            double m023 = m02 * a32 - m03 * a22 + m23 * a02;
            double m024 = m02 * a42 - m04 * a22 + m24 * a02;
            double m025 = m02 * a52 - m05 * a22 + m25 * a02;
            double m034 = m03 * a42 - m04 * a32 + m34 * a02;
            double m035 = m03 * a52 - m05 * a32 + m35 * a02;
            double m045 = m04 * a52 - m05 * a42 + m45 * a02;
            double m123 = m12 * a32 - m13 * a22 + m23 * a12;
            double m124 = m12 * a42 - m14 * a22 + m24 * a12;
            double m125 = m12 * a52 - m15 * a22 + m25 * a12;
            double m134 = m13 * a42 - m14 * a32 + m34 * a12;
            double m135 = m13 * a52 - m15 * a32 + m35 * a12;
            double m145 = m14 * a52 - m15 * a42 + m45 * a12;
            double m234 = m23 * a42 - m24 * a32 + m34 * a22;
            double m235 = m23 * a52 - m25 * a32 + m35 * a22;
            double m245 = m24 * a52 - m25 * a42 + m45 * a22;
            double m345 = m34 * a52 - m35 * a42 + m45 * a32;
            // Now compute the minors of rank 4
            double m0123 = m012 * a33 - m013 * a23 + m023 * a13 - m123 * a03;
            double m0124 = m012 * a43 - m014 * a23 + m024 * a13 - m124 * a03;
            double m0125 = m012 * a53 - m015 * a23 + m025 * a13 - m125 * a03;
            double m0134 = m013 * a43 - m014 * a33 + m034 * a13 - m134 * a03;
            double m0135 = m013 * a53 - m015 * a33 + m035 * a13 - m135 * a03;
            double m0145 = m014 * a53 - m015 * a43 + m045 * a13 - m145 * a03;
            double m0234 = m023 * a43 - m024 * a33 + m034 * a23 - m234 * a03;
            double m0235 = m023 * a53 - m025 * a33 + m035 * a23 - m235 * a03;
            double m0245 = m024 * a53 - m025 * a43 + m045 * a23 - m245 * a03;
            double m0345 = m034 * a53 - m035 * a43 + m045 * a33 - m345 * a03;
            double m1234 = m123 * a43 - m124 * a33 + m134 * a23 - m234 * a13;
            double m1235 = m123 * a53 - m125 * a33 + m135 * a23 - m235 * a13;
            double m1245 = m124 * a53 - m125 * a43 + m145 * a23 - m245 * a13;
            double m1345 = m134 * a53 - m135 * a43 + m145 * a33 - m345 * a13;
            double m2345 = m234 * a53 - m235 * a43 + m245 * a33 - m345 * a23;
            // Now compute the minors of rank 5
            double m01234 = m0123 * a44 - m0124 * a34 + m0134 * a24 - m0234 * a14 + m1234 * a04;
            double m01235 = m0123 * a54 - m0125 * a34 + m0135 * a24 - m0235 * a14 + m1235 * a04;
            double m01245 = m0124 * a54 - m0125 * a44 + m0145 * a24 - m0245 * a14 + m1245 * a04;
            double m01345 = m0134 * a54 - m0135 * a44 + m0145 * a34 - m0345 * a14 + m1345 * a04;
            double m02345 = m0234 * a54 - m0235 * a44 + m0245 * a34 - m0345 * a24 + m2345 * a04;
            double m12345 = m1234 * a54 - m1235 * a44 + m1245 * a34 - m1345 * a24 + m2345 * a14;
            // Now compute the minors of rank 6
            double m012345 = m01234 * a55 - m01235 * a45 + m01245 * a35 - m01345 * a25
                             + m02345 * a15 - m12345 * a05;
            return m012345;
        }
        public static

            int
                sign_of_determinant(double a00, double a01,
                           double a10, double a11)
        {
            var a = (a00 * a11);
            var b = (a10 * a01);
            if (a.AreNear(b)) return 0;
            return (a00 * a11).CompareTo(a10 * a01);
        }
        public static bool AreNear(this double a, double b, double eps = Const.EPSILON)
        {
            return (Math.Abs(a - b) < eps);
        }
        public static bool IsZero(this double a, double eps = Const.EPSILON)
        {
            return (Math.Abs(a ) < eps);
        }
        public static

                int
                    sign_of_determinant(double a00, double a01, double a02,
                               double a10, double a11, double a12,
                               double a20, double a21, double a22)
        {
            return Math.Sign(determinant(a00, a01, a02,
                a10, a11, a12,
                a20, a21, a22));
        }

        public static

                int
                    sign_of_determinant(
            double a00, double a01, double a02, double a03,
            double a10, double a11, double a12, double a13,
            double a20, double a21, double a22, double a23,
            double a30, double a31, double a32, double a33)
        {
            return Math.Sign(determinant(a00, a01, a02, a03,
                a10, a11, a12, a13,
                a20, a21, a22, a23,
                a30, a31, a32, a33));
        }

        public static

                int
                    sign_of_determinant(
            double a00, double a01, double a02, double a03, double a04,
            double a10, double a11, double a12, double a13, double a14,
            double a20, double a21, double a22, double a23, double a24,
            double a30, double a31, double a32, double a33, double a34,
            double a40, double a41, double a42, double a43, double a44)
        {
            return Math.Sign(determinant(a00, a01, a02, a03, a04,
                a10, a11, a12, a13, a14,
                a20, a21, a22, a23, a24,
                a30, a31, a32, a33, a34,
                a40, a41, a42, a43, a44));
        }

        public static

                int
                    sign_of_determinant(
            double a00, double a01, double a02, double a03, double a04,
            double a05,
            double a10, double a11, double a12, double a13, double a14,
            double a15,
            double a20, double a21, double a22, double a23, double a24,
            double a25,
            double a30, double a31, double a32, double a33, double a34,
            double a35,
            double a40, double a41, double a42, double a43, double a44,
            double a45,
            double a50, double a51, double a52, double a53, double a54,
            double a55)
        {
            return Math.Sign(determinant(a00, a01, a02, a03, a04, a05,
                a10, a11, a12, a13, a14, a15,
                a20, a21, a22, a23, a24, a25,
                a30, a31, a32, a33, a34, a35,
                a40, a41, a42, a43, a44, a45,
                a50, a51, a52, a53, a54, a55));
        }

        public static int Sign(double value, double eps=Const.EPSILON)
        {
            if (value.IsZero(eps)) return 0;
            return Math.Sign(value);
            
            
        }
    }
}