using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace CGAL
{
    using FT = double;
    using RT = double;
    using ST = int;
    using We = double;
    using static DebuggerInfo;
    using static System.Runtime.InteropServices.JavaScript.JSType;

    public partial class Mathex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void
        midpoint(FT px, FT py,
                    FT qx, FT qy,
                    out FT x, out FT y)
        {
            x = (px + qx) * 0.5;
            y = (py + qy) * 0.5;
        }

        ////CGAL_KERNEL_LARGE_INLINE
        public static void
        circumcenter_translate(FT dqx, FT dqy,
                                 FT drx, FT dry,
                                       out FT dcx, out FT dcy)
        {
            // Given 3 points P, Q, R, this function takes as input:
            // qx-px, qy-py, rx-px, ry-py.  And returns cx-px, cy-py,
            // where (cx, cy) are the coordinates of the circumcenter C.

            // What we do is intersect the bisectors.
            FT r2 = square(drx) + square(dry);
            FT q2 = square(dqx) + square(dqy);
            FT den = 2 * determinant(dqx, dqy, drx, dry);

            // The 3 points aren't collinear.
            // Hopefully, this is already checked at the upper level.
            CGAL_kernel_assertion(!is_zero(den));

            // One possible optimization here is to precompute 1/den, to avoid one
            // division.  However, we lose precision, and it's maybe not worth it (?).
            dcx = determinant(dry, dqy, r2, q2) / den;
            dcy = -determinant(drx, dqx, r2, q2) / den;
        }

        //
        public static void
         circumcenter(FT px, FT py,
                         FT qx, FT qy,
                         FT rx, FT ry,
                         out FT x, out FT y)
        {
            circumcenter_translate(qx - px, qy - py, rx - px, ry - py, out x, out y);
            x += px;
            y += py;
        }

        public static void
        barycenter(FT p1x, FT p1y, FT w1,
                     FT p2x, FT p2y,
                     out FT x, out FT y)
        {
            FT w2 = 1 - w1;
            x = w1 * p1x + w2 * p2x;
            y = w1 * p1y + w2 * p2y;
        }

        public static void
        barycenter(FT p1x, FT p1y, FT w1,
                     FT p2x, FT p2y, FT w2,
                     out FT x, out FT y)
        {
            FT sum = w1 + w2;
            CGAL_kernel_assertion(sum != 0);
            x = (w1 * p1x + w2 * p2x) / sum;
            y = (w1 * p1y + w2 * p2y) / sum;
        }

        public static void
        barycenter(FT p1x, FT p1y, FT w1,
                     FT p2x, FT p2y, FT w2,
                     FT p3x, FT p3y,
                     out FT x, out FT y)
        {
            FT w3 = 1 - w1 - w2;
            x = w1 * p1x + w2 * p2x + w3 * p3x;
            y = w1 * p1y + w2 * p2y + w3 * p3y;
        }

        public static void
        barycenter(FT p1x, FT p1y, FT w1,
                     FT p2x, FT p2y, FT w2,
                     FT p3x, FT p3y, FT w3,
                     out FT x, out FT y)
        {
            FT sum = w1 + w2 + w3;
            CGAL_kernel_assertion(sum != 0);
            x = (w1 * p1x + w2 * p2x + w3 * p3x) / sum;
            y = (w1 * p1y + w2 * p2y + w3 * p3y) / sum;
        }

        public static void
        barycenter(FT p1x, FT p1y, FT w1,
                     FT p2x, FT p2y, FT w2,
                     FT p3x, FT p3y, FT w3,
                     FT p4x, FT p4y,
                     out FT x, out FT y)
        {
            FT w4 = 1 - w1 - w2 - w3;
            x = w1 * p1x + w2 * p2x + w3 * p3x + w4 * p4x;
            y = w1 * p1y + w2 * p2y + w3 * p3y + w4 * p4y;
        }

        public static void
        barycenter(FT p1x, FT p1y, FT w1,
                     FT p2x, FT p2y, FT w2,
                     FT p3x, FT p3y, FT w3,
                     FT p4x, FT p4y, FT w4,
                     out FT x, out FT y)
        {
            FT sum = w1 + w2 + w3 + w4;
            CGAL_kernel_assertion(sum != 0);
            x = (w1 * p1x + w2 * p2x + w3 * p3x + w4 * p4x) / sum;
            y = (w1 * p1y + w2 * p2y + w3 * p3y + w4 * p4y) / sum;
        }

        //
        public static void
        centroid(FT px, FT py,
                    FT qx, FT qy,
                    FT rx, FT ry,
                    out FT x, out FT y)
        {
            x = (px + qx + rx) / 3;
            y = (py + qy + ry) / 3;
        }

        //
        public static void
        centroid(FT px, FT py,
                    FT qx, FT qy,
                    FT rx, FT ry,
                    FT sx, FT sy,
                    out FT x, out FT y)
        {
            x = (px + qx + rx + sx) / 4;
            y = (py + qy + ry + sy) / 4;
        }

        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void
           line_from_points(FT px, FT py,
                              FT qx, FT qy,
                              out FT a, out FT b, out FT c)
        {
            // The horizontal and vertical line get a special treatment
            // in order to make the intersection code robust for doubles
            if (py == qy)
            {
                a = 0;
                if (qx > px)
                {
                    b = 1;
                    c = -py;
                }
                else if (qx == px)
                {
                    b = 0;
                    c = 0;
                }
                else
                {
                    b = -1;
                    c = py;
                }
            }
            else if (qx == px)
            {
                b = 0;
                if (qy > py)
                {
                    a = -1;
                    c = px;
                }
                else if (qy == py)
                {
                    a = 0;
                    c = 0;
                }
                else
                {
                    a = 1;
                    c = -px;
                }
            }
            else
            {
                a = py - qy;
                b = qx - px;
                c = -px * a - py * b;
            }
        }

        //   [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void
        line_from_point_direction(FT px, FT py,
                                    FT dx, FT dy,
                                    out FT a, out FT b, out FT c)
        {
            a = -dy;
            b = dx;
            c = px * dy - py * dx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void
        bisector_of_points(FT px, FT py,
                             FT qx, FT qy,
                             out FT a, out FT b, out FT c)
        {
            a = 2 * (px - qx);
            b = 2 * (py - qy);
            c = square(qx) + square(qy) -
                square(px) - square(py);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void
        bisector_of_lines(FT pa, FT pb, FT pc,
                            FT qa, FT qb, FT qc,
                            out FT a, out FT b, out FT c)
        {
            // We normalize the equations of the 2 lines, and we then add them.
            FT n1 = Math.Sqrt((square(pa) + square(pb)));
            FT n2 = Math.Sqrt((square(qa) + square(qb)));
            a = n2 * pa + n1 * qa;
            b = n2 * pb + n1 * qb;
            c = n2 * pc + n1 * qc;

            // Care must be taken for the case when this produces a degenerate line.
            if (a == 0 && b == 0)
            {
                a = n2 * pa - n1 * qa;
                b = n2 * pb - n1 * qb;
                c = n2 * pc - n1 * qc;
            }
        }

        //   [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT
        line_y_at_x(FT a, FT b, FT c, FT x)
        {
            return (-a * x - c) / b;
        }

        //   [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void
        line_get_point(FT a, FT b, FT c, FT i,
                         out FT x, out FT y)
        {
            if (is_zero(b))
            {
                x = -c / a;
                y = 1 - i * a;
            }
            else
            {
                x = 1 + i * b;
                y = -(a + c) / b - i * a;
            }
        }

        public static void
            perpendicular_through_point(FT la, FT lb,
                                          FT px, FT py,
                                          out FT a, out FT b, out FT c)
        {
            a = -lb;
            b = la;
            c = lb * px - la * py;
        }

        //
        public static void
        line_project_point(FT la, FT lb, FT lc,
                             FT px, FT py,
                             out FT x, out FT y)
        {
            if ((is_zero(la))) // horizontal line
            {
                x = px;
                y = -lc / lb;
            }
            else if ((is_zero(lb))) // vertical line
            {
                x = -lc / la;
                y = py;
            }
            else
            {
                FT a2 = square(la);
                FT b2 = square(lb);
                FT d = a2 + b2;
                x = (b2 * px - la * lb * py - la * lc) / d;
                y = (-la * lb * px + a2 * py - lb * lc) / d;
            }
        }

        //
        public static FT
        squared_radius(FT px, FT py,
                         FT qx, FT qy,
                         FT rx, FT ry,
                         out FT x, out FT y)
        {
            circumcenter_translate(qx - px, qy - py, rx - px, ry - py, out x, out y);
            FT r2 = square(x) + square(y);
            x += px;
            y += py;
            return r2;
        }

        //
        public static FT
        squared_radius(FT px, FT py,
                         FT qx, FT qy,
                         FT rx, FT ry)
        {
            FT x, y;
            circumcenter_translate(qx - px, qy - py, rx - px, ry - py, out x, out y);
            return square(x) + square(y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT squared_distance(FT px, FT py, FT qx, FT qy)
        {
            return square(px - qx) + square(py - qy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT squared_distance(in Point2 p, in Point2 q)
        {
            return squared_distance(p.X, p.Y, q.X, q.Y);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT distance(in Point2 p, in Point2 q)
        {
            return Math.Sqrt(squared_distance(p.X, p.Y, q.X, q.Y));
        }


            //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static FT
        squared_radius(FT px, FT py,
                         FT qx, FT qy)
        {
            return squared_distance(px, py, qx, qy) / 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT scaled_distance_to_line(FT la, FT lb, FT lc,
                                   FT px, FT py)
        {
            // for comparisons, use distance_to_directionsC2 instead
            // since lc is irrelevant
            return la * px + lb * py + lc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT
        scaled_distance_to_direction(FT la, FT lb,
                                        FT px, FT py)
        {
            // scalar product with direction
            return la * px + lb * py;
        }

        //
        public static FT
        scaled_distance_to_line(FT px, FT py,
                                   FT qx, FT qy,
                                   FT rx, FT ry)
        {
            return determinant(px - rx, py - ry, qx - rx, qy - ry);
        }

        public static void
         weighted_circumcenter_translate(RT dqx, RT dqy, RT dqw,
                                           RT drx, RT dry, RT drw,
                                           out RT dcx, out RT dcy)
        {
            // Given 3 points P, Q, R, this function takes as input:
            // qx-px, qy-py,qw-pw,  rx-px, ry-py, rw-pw.  And returns cx-px, cy-py,
            // where (cx, cy) are the coordinates of the circumcenter C.

            // What we do is intersect the radical axis
            RT r2 = square(drx) + square(dry) - drw;
            RT q2 = square(dqx) + square(dqy) - dqw;

            RT den = (2) * determinant(dqx, dqy, drx, dry);

            // The 3 points aren't collinear.
            // Hopefully, this is already checked at the upper level.
            CGAL_assertion(den != 0);

            // One possible optimization here is to precompute 1/den, to avoid one
            // division.  However, we lose precision, and it's maybe not worth it (?).
            dcx = determinant(dry, dqy, r2, q2) / den;
            dcy = -determinant(drx, dqx, r2, q2) / den;
        }

        /*
        //template < class RT >
        public static void
        weighted_circumcenter( RT px, RT py, We pw,
                                 RT qx, RT qy, We qw,
                                 RT rx, RT ry, We rw,
                                 out RT x, out RT y )
        {
          RT dqw = (RT)(qw-pw);
          RT drw = (RT)(rw-pw);

          weighted_circumcenter_translateC2<RT>(qx-px, qy-py, dqw,rx-px, ry-py,drw,x, y);
          x += px;
          y += py;
        }
        */

        public static FT
        power_product(FT px, FT py, FT pw,
                        FT qx, FT qy, FT qw)
        {
            // computes the power product of two weighted points
            FT qpx = qx - px;
            FT qpy = qy - py;
            FT qp2 = square(qpx) + square(qpy);
            return qp2 - pw - qw;
        }
        /*
        public static void
        radical_axis(RT px, RT py,  We pw,
                       RT qx, RT qy, We qw,
                       out RT a, out RT b, out RT c )
        {
          a =  (RT)(2)*(px - qx);
          b =  (RT)(2)*(py - qy);
          c = - square(px) - square(py)
              + square(qx) + square(qy)
              + (RT)(pw) - (RT)(qw);
        }

        */

        public static FT
        squared_radius_orthogonal_circle(FT px, FT py, FT pw,
                                           FT qx, FT qy, FT qw,
                                           FT rx, FT ry, FT rw)
        {
            FT FT4 = (4);
            FT dpx = px - rx;
            FT dpy = py - ry;
            FT dqx = qx - rx;
            FT dqy = qy - ry;
            FT dpp = square(dpx) + square(dpy) - pw + rw;
            FT dqq = square(dqx) + square(dqy) - qw + rw;

            FT det0 = determinant(dpx, dpy, dqx, dqy);
            FT det1 = determinant(dpp, dpy, dqq, dqy);
            FT det2 = determinant(dpx, dpp, dqx, dqq);

            return (square(det1) + square(det2)) /
                                            (FT4 * square(det0)) - rw;
        }

        //
        public static FT
        squared_radius_smallest_orthogonal_circle(FT px, FT py, FT pw,
                                                    FT qx, FT qy, FT qw)
        {
            FT FT4 = (4);
            FT dpz = square(px - qx) + square(py - qy);
            return (square(dpz - pw + qw) / (FT4 * dpz) - qw);
        }
    }

    public static partial class Mathex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool parallel(FT l1a, FT l1b, FT l2a, FT l2b)
        {
            return sign_of_determinant(l1a, l1b, l2a, l2b) == 0;
        }

        public static bool parallel(FT s1sx, FT s1sy, FT s1tx, FT s1ty, FT s2sx, FT s2sy, FT s2tx, FT s2ty)
        {
            return sign_of_determinant(s1tx - s1sx, s1ty - s1sy, s2tx - s2sx, s2ty - s2sy) == 0;
        }

        public static bool equal_line(FT l1a, FT l1b, FT l1c,
             FT l2a, FT l2b, FT l2c)
        {
            if (sign_of_determinant(l1a, l1b, l2a, l2b) != 0)
                return false; // Not parallel.
            ST s1a = sign(l1a);
            if (s1a != 0)
                return s1a == sign(l2a) && sign_of_determinant(l1a, l1c, l2a, l2c) == 0;
            return sign(l1b) == sign(l2b) && sign_of_determinant(l1b, l1c, l2b, l2c) == 0;
        }

        public static ST
        compare_x(FT px,
            FT la, FT lb, FT lc,
            FT ha, FT hb, FT hc)
        {
            // The abscissa of the intersection point is num/den.
            FT num = determinant(lb, lc, hb, hc);
            FT den = determinant(la, lb, ha, hb);
            ST s = sign(den);
            CGAL_kernel_assertion(s != 0);
            return s * compare(px * den, num);
        }

        public static ST
        compare_x(FT la, FT lb, FT lc,
            FT h1a, FT h1b, FT h1c,
            FT h2a, FT h2b, FT h2c)
        {
            /*
            FT num1 = determinant( lb, lc, h1b, h1c);
            FT den1 = determinant( la, lb, h1a, h1b);
            FT num2 = determinant( lb, lc, h2b, h2c);
            FT den2 = determinant( la, lb, h2a, h2b);
            Sign s = Sign (sign(den1) * sign(den2));
            CGAL_kernel_assertion( s != 0 );
            return s * sign_of_determinant(num1, num2, den1, den2);
            */
            FT num1 = determinant(la, lc, h1a, h1c);
            FT num2 = determinant(la, lc, h2a, h2c);
            FT num = determinant(h1a, h1c, h2a, h2c) * lb
                      + determinant(num1, num2, h1b, h2b);
            FT den1 = determinant(la, lb, h1a, h1b);
            FT den2 = determinant(la, lb, h2a, h2b);
            return sign(lb) *
                   sign(num) *
                   sign(den1) *
                   sign(den2);
        }

        public static ST
        compare_x(FT l1a, FT l1b, FT l1c,
            FT h1a, FT h1b, FT h1c,
            FT l2a, FT l2b, FT l2c,
            FT h2a, FT h2b, FT h2c)
        {
            FT num1 = determinant(l1b, l1c, h1b, h1c);
            FT den1 = determinant(l1a, l1b, h1a, h1b);
            FT num2 = determinant(l2b, l2c, h2b, h2c);
            FT den2 = determinant(l2a, l2b, h2a, h2b);
            ST s = sign(den1) * sign(den2);
            CGAL_kernel_assertion(s != 0);
            return s * sign_of_determinant(num1, num2, den1, den2);
        }

        public static ST compare_y_at_x(FT px, FT py,
                 FT la, FT lb, FT lc)
        {
            var s = sign(lb);
            CGAL_kernel_assertion(s != 0);
            return s * sign(la * px + lb * py + lc);
        }

        public static ST compare_y_at_x(FT px,
                 FT l1a, FT l1b, FT l1c,
                 FT l2a, FT l2b, FT l2c)
        {
            ST s = sign(l1b) * sign(l2b);
            CGAL_kernel_assertion(s != 0);
            return s * sign_of_determinant(l2a * px + l2c, l2b, l1a * px + l1c, l1b);
        }

        ////CGAL_KERNEL_LARGE_INLINE
        public static ST compare_y_at_x(
                   FT l1a, FT l1b, FT l1c,
                 FT l2a, FT l2b, FT l2c,
                 FT ha, FT hb, FT hc)
        {
            ST s = sign(hb) * sign_of_determinant(l1a, l1b, l2a, l2b);
            CGAL_kernel_assertion(s != 0);
            return s * sign_of_determinant(l1a, l1b, l1c,
                                              l2a, l2b, l2c,
                                              ha, hb, hc);
        }

        ////CGAL_KERNEL_LARGE_INLINE
        public static ST compare_y_at_x(FT l1a, FT l1b, FT l1c,
                         FT l2a, FT l2b, FT l2c,
                         FT h1a, FT h1b, FT h1c,
                         FT h2a, FT h2b, FT h2c)
        {
            // The abscissa of the intersection point is num/den.
            FT num = determinant(l1b, l1c, l2b, l2c);
            FT den = determinant(l1a, l1b, l2a, l2b);
            ST s = sign(h1b) * sign(h2b) * sign(den);
            CGAL_kernel_assertion(s != 0);
            return s * sign_of_determinant(h2a * num + h2c * den, h2b,
                                                  h1a * num + h1c * den, h1b);
        }

        // forward-declaration of orientationC2, used in compare_y_at_xC2

        public static bool are_ordered(FT a, FT b, FT c)
        {
            FT min = Math.Min(a, c);
            FT max = Math.Max(a, c);
            return min <= b && b <= max;
        }

        // //CGAL_KERNEL_LARGE_INLINE
        public static ST compare_y_at_x(FT px, FT py,
                     FT ssx, FT ssy,
                     FT stx, FT sty)
        {
            // compares the y-coordinates of p and the vertical projection of p on s.
            // Precondition : p is in the x-range of s.

            CGAL_kernel_precondition(are_ordered(ssx, px, stx));

            if (ssx < stx)
                return orientation(px, py, ssx, ssy, stx, sty);
            else if (ssx > stx)
                return orientation(px, py, stx, sty, ssx, ssy);
            else
            {
                if (py < Math.Min(sty, ssy))
                    return -1;
                if (py > Math.Max(sty, ssy))
                    return 1;
                return 0;
            }
        }

        ////CGAL_KERNEL_LARGE_INLINE
        public static ST compare_y_at_x_segment_(FT px,
                          FT s1sx, FT s1sy,
                          FT s1tx, FT s1ty,
                          FT s2sx, FT s2sy,
                          FT s2tx, FT s2ty)
        {
            // compares the y-coordinates of the vertical projections of p on s1 and s2
            // Precondition : p is in the x-range of s1 and s2.
            // - if one or two segments are vertical :
            //   - if the segments intersect, return 0
            //   - if not, return the obvious -1/1.

            CGAL_kernel_precondition(are_ordered(s1sx, px, s1tx));
            CGAL_kernel_precondition(are_ordered(s2sx, px, s2tx));

            if (s1sx != s1tx && s2sx != s2tx)
            {
                FT s1stx = s1sx - s1tx;
                FT s2stx = s2sx - s2tx;

                return compare(s1sx, s1tx) *
                       compare(s2sx, s2tx) *
                       compare(-(s1sx - px) * (s1sy - s1ty) * s2stx,
                                        (s2sy - s1sy) * s2stx * s1stx
                                        - (s2sx - px) * (s2sy - s2ty) * s1stx);
            }
            else
            {
                if (s1sx == s1tx)
                { // s1 is vertical
                    ST c1, c2;
                    c1 = compare_y_at_x(px, s1sy, s2sx, s2sy, s2tx, s2ty);
                    c2 = compare_y_at_x(px, s1ty, s2sx, s2sy, s2tx, s2ty);
                    if (c1 == c2)
                        return c1;
                    return 0;
                }
                // s2 is vertical
                ST c3, c4;
                c3 = compare_y_at_x(px, s2sy, s1sx, s1sy, s1tx, s1ty);
                c4 = compare_y_at_x(px, s2ty, s1sx, s1sy, s1tx, s1ty);
                if (c3 == c4)
                    return -c3;
                return 0;
            }
        }

        public static bool equal_direction(FT dx1, FT dy1,
                          FT dx2, FT dy2)
        {
            return (sign(dx1) == sign(dx2) &&
                               sign(dy1) == sign(dy2) &&
                               sign_of_determinant(dx1, dy1, dx2, dy2) == 0);
        }

        public static ST compare_angle_with_x_axis(FT dx1, FT dy1, FT dx2, FT dy2)
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
                return 1;
            else if (quadrant_1 < quadrant_2)
                return -1;
            return -sign_of_determinant(dx1, dy1, dx2, dy2);
        }
        public static ST compare_angle_with_x_axis(Direction2 d1, Direction2 d2) => compare_angle_with_x_axis(d1.DX, d1.DY, d2.DX, d2.DY);

        public static ST compare_slopes(FT l1a, FT l1b, FT l2a, FT l2b)
        {
            if (is_zero(l1a))  // l1 is horizontal
                return is_zero(l2b) ? -1 : sign(l2a) * sign(l2b);
            if (is_zero(l2a)) // l2 is horizontal
                return is_zero(l1b) ? 1 : -sign(l1a) * sign(l1b);
            if (is_zero(l1b)) return is_zero(l2b) ? 0 : 1;
            if (is_zero(l2b)) return -1;
            var l1_sign = -sign(l1a) * sign(l1b);
            var l2_sign = -sign(l2a) * sign(l2b);

            if (l1_sign < l2_sign) return -1;
            if (l1_sign > l2_sign) return 1;

            if (l1_sign > 0)
                return compare(Math.Abs(l1a * l2b), Math.Abs(l2a * l1b));

            return compare(Math.Abs(l2a * l1b),
                                      Math.Abs(l1a * l2b));
        }

        public static ST compare_slopes(FT s1_src_x, FT s1_src_y, FT s1_tgt_x,
                 FT s1_tgt_y, FT s2_src_x, FT s2_src_y,
                 FT s2_tgt_x, FT s2_tgt_y)
        {
            var cmp_y1 = compare(s1_src_y, s1_tgt_y);
            if (cmp_y1 == 0) // horizontal
            {
                if (compare(s2_src_x, s2_tgt_x) == 0) return -1;
                return -sign(s2_src_y - s2_tgt_y) * sign(s2_src_x - s2_tgt_x);
            }

            var cmp_y2 = compare(s2_src_y, s2_tgt_y);
            if (cmp_y2 == 0)
            {
                if (compare(s1_src_x, s1_tgt_x) == 0) return 1;
                return sign(s1_src_y - s1_tgt_y) * sign(s1_src_x - s1_tgt_x);
            }

            var cmp_x1 = compare(s1_src_x, s1_tgt_x);
            var cmp_x2 = compare(s2_src_x, s2_tgt_x);

            if (cmp_x1 == 0) return cmp_x2 == 0 ? 0 : 1;

            if (cmp_x2 == 0) return -1;

            FT s1_xdiff = s1_src_x - s1_tgt_x;
            FT s1_ydiff = s1_src_y - s1_tgt_y;
            FT s2_xdiff = s2_src_x - s2_tgt_x;
            FT s2_ydiff = s2_src_y - s2_tgt_y;
            var s1_sign = sign(s1_ydiff) * sign(s1_xdiff);
            var s2_sign = sign(s2_ydiff) * sign(s2_xdiff);

            if (s1_sign < s2_sign) return -1;
            if (s1_sign > s2_sign) return 1;

            if (s1_sign > 0)
                return compare(Math.Abs(s1_ydiff * s2_xdiff), Math.Abs(s2_ydiff * s1_xdiff));

            return compare(Math.Abs(s2_ydiff * s1_xdiff), Math.Abs(s1_ydiff * s2_xdiff));
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //        public static ST compare_lexicographically_xy(FT px, FT py,
        //                               FT qx, FT qy)
        //    {
        //            ST c = compare(px, qx);
        //        if (is_indeterminate(c)) return indeterminate<Cmp>();
        //        return (c != 0) ? c : compare(py, qy);
        //    }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ST orientation(FT px, FT py, FT qx, FT qy, FT rx, FT ry)
        {
            return sign_of_determinant(qx - px, qy - py, rx - px, ry - py);
        
        
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static    bool counterclockwise_in_between_2( in Direction2 p, in Direction2 q, in Direction2 r) 
    {
        if (q<p)
            return (p<r )||(r <= q );
        else
            return (p<r )&&(r <= q );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OrientationEnum orientation(in Vector2 u, in Vector2 v) {
      return (OrientationEnum ) sign_of_determinant(u.hx(), u.hy(),v.hx(), v.hy());
    }



        public static OrientationEnum orientation(Point2[] p)
        {
            var phx = p[0].hx();
            var phy = p[0].hy();
            var phw = p[0].hw();
            var qhx = p[1].hx();
            var qhy = p[1].hy();
            var qhw = p[1].hw();
            var rhx = p[2].hx();
            var rhy = p[2].hy();
            var rhw = p[2].hw();

            // | A B |
            // | C D |

            RT A = phx * rhw - phw * rhx;
            RT B = phy * rhw - phw * rhy;
            RT C = qhx * rhw - qhw * rhx;
            RT D = qhy * rhw - qhw * rhy;

            return (OrientationEnum)compare(A * D, B * C);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static OrientationEnum orientation(in Point2 p, in Point2 q, in Point2 r)
        {
            var phx = p.hx();
            var phy = p.hy();
            var phw = p.hw();
            var qhx = q.hx();
            var qhy = q.hy();
            var qhw = q.hw();
            var rhx = r.hx();
            var rhy = r.hy();
            var rhw = r.hw();

            // | A B |
            // | C D |

            RT A = phx * rhw - phw * rhx;
            RT B = phy * rhw - phw * rhy;
            RT C = qhx * rhw - qhw * rhx;
            RT D = qhy * rhw - qhw * rhy;

            return (OrientationEnum) compare(A * D, B * C);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ST orientation(FT ux, FT uy, FT vx, FT vy)
        {
            return sign_of_determinant(ux, uy, vx, vy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ST angle(FT ux, FT uy, FT vx, FT vy)
        {
            return (sign(ux * vx + uy * vy));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ST angle(
                                        FT px, FT py,
                                        FT qx, FT qy,
                                        FT rx, FT ry)
        {
            return (sign((px - qx) * (rx - qx) + (py - qy) * (ry - qy)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ST angle(
                                        FT px, FT py,
                                        FT qx, FT qy,
                                        FT rx, FT ry,
                                        FT sx, FT sy)
        {
            return (sign((px - qx) * (rx - sx) + (py - qy) * (ry - sy)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ST angle(Point2 p, Point2 q, Point2 r, Point2 s) =>

            angle(p.X, p.Y, q.X, q.Y, q.X, r.Y, s.X, s.Y);

        public static bool
        collinear_are_ordered_along_line(FT px, FT py, FT qx, FT qy,FT rx, FT ry)
        {
            if (px < qx) return !(rx < qx);
            if (qx < px) return !(qx < rx);
            if (py < qy) return !(ry < qy);
            if (qy < py) return !(qy < ry);
            return true; // p==q
        }

        public static bool collinear_are_strictly_ordered_along_line(FT px, FT py, FT qx, FT qy, FT rx, FT ry)
        {
            if (px < qx) return (qx < rx);
            if (qx < px) return (rx < qx);
            if (py < qy) return (qy < ry);
            if (qy < py) return (ry < qy);
            return false;
        }

        // //CGAL_KERNEL_LARGE_INLINE
        public static ST side_of_oriented_circle(FT px, FT py,
                                  FT qx, FT qy,
                                  FT rx, FT ry,
                                  FT tx, FT ty)
        {
            //  sign_of_determinant(px, py, px*px + py*py, 1,
            //                         qx, qy, qx*qx + qy*qy, 1,
            //                         rx, ry, rx*rx + ry*ry, 1,
            //                         tx, ty, tx*tx + ty*ty, 1);
            // We first translate so that p is the new origin.
            FT qpx = qx - px;
            FT qpy = qy - py;
            FT rpx = rx - px;
            FT rpy = ry - py;
            FT tpx = tx - px;
            FT tpy = ty - py;
            // The usual 3x3 formula can be simplified a little bit to a 2x2.
            //         - sign_of_determinant(qpx, qpy, square(qpx) + square(qpy),
            //                                  rpx, rpy, square(rpx) + square(rpy),
            //                                  tpx, tpy, square(tpx) + square(tpy)));
            return sign_of_determinant(qpx * tpy - qpy * tpx, tpx * (tx - qx) + tpy * (ty - qy),
                                            qpx * rpy - qpy * rpx, rpx * (rx - qx) + rpy * (ry - qy));
        }

        ////CGAL_KERNEL_LARGE_INLINE

        public static ST? side_of_bounded_circle(FT px, FT py,
                                 FT qx, FT qy,
                                 FT rx, FT ry,
                                 FT tx, FT ty)
        {
            return (side_of_oriented_circle(px, py, qx, qy, rx, ry, tx, ty)
                                           * orientation(px, py, qx, qy, rx, ry));
        }

        public static ST? side_of_bounded_circle(FT px, FT py,
                                 FT qx, FT qy,
                                 FT tx, FT ty)
        {
            // Returns whether T lies inside or outside the circle which diameter is PQ.
            return (compare((tx - px) * (qx - tx), (ty - py) * (ty - qy)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ST cmp_dist_to_point(FT px, FT py,
                            FT qx, FT qy,
                            FT rx, FT ry)
        {
            return compare(squared_distance(px, py, qx, qy),
                                    squared_distance(px, py, rx, ry));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ST compare_distance_2(Point2 p, Point2 q, Point2 r)
        {
            return compare(squared_distance(p, q),
                                    squared_distance(p, r));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool has_larger_dist_to_point(FT px, FT py, FT qx, FT qy, FT rx, FT ry)
        {
            return cmp_dist_to_point(px, py, qx, qy, rx, ry) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool has_smaller_dist_to_point(FT px, FT py, FT qx, FT qy, FT rx, FT ry)
        {
            return cmp_dist_to_point(px, py, qx, qy, rx, ry) == -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ST cmp_signed_dist_to_direction(FT la, FT lb,
                                       FT px, FT py,
                                       FT qx, FT qy)
        {
            return compare(scaled_distance_to_direction(la, lb, px, py), scaled_distance_to_direction(la, lb, qx, qy));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool has_larger_signed_dist_to_direction(FT la, FT lb,
                                              FT px, FT py,
                                              FT qx, FT qy)
        {
            return cmp_signed_dist_to_direction(la, lb, px, py, qx, qy) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool has_smaller_signed_dist_to_direction(FT la, FT lb,
                                               FT px, FT py,
                                               FT qx, FT qy)
        {
            return cmp_signed_dist_to_direction(la, lb, px, py, qx, qy) == -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ST cmp_signed_dist_to_line(FT px, FT py,
                                  FT qx, FT qy,
                                  FT rx, FT ry,
                                  FT sx, FT sy)
        {
            return compare(scaled_distance_to_line(px, py, qx, qy, rx, ry),
                                    scaled_distance_to_line(px, py, qx, qy, sx, sy));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool has_larger_signed_dist_to_line(FT px, FT py,
                                         FT qx, FT qy,
                                         FT rx, FT ry,
                                         FT sx, FT sy)
        {
            return cmp_signed_dist_to_line(px, py, qx, qy, rx, ry, sx, sy) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool has_smaller_signed_dist_to_line(FT px, FT py,
                                          FT qx, FT qy,
                                          FT rx, FT ry,
                                          FT sx, FT sy)
        {
            return cmp_signed_dist_to_line(px, py, qx, qy, rx, ry, sx, sy) == -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ST side_of_oriented_line(FT a, FT b, FT c,
                                FT x, FT y)
        {
            return sign(a * x + b * y + c);
        }

        public static ST compare_power_distance(FT px, FT py, FT pwt,
                                 FT qx, FT qy, FT qwt,
                                 FT rx, FT ry)
        {
            // returns -1 if r is closer to p w.r.t. the power metric
            FT d1 = square(rx - px) + square(ry - py) - pwt;
            FT d2 = square(rx - qx) + square(ry - qy) - qwt;
            return compare(d1, d2);
        }

        public static ST power_side_of_bounded_power_circle(
                                             FT px, FT py, FT pw,
                                             FT qx, FT qy, FT qw,
                                             FT tx, FT ty, FT tw)
        {
            FT dpx = px - qx;
            FT dpy = py - qy;
            FT dtx = tx - qx;
            FT dty = ty - qy;
            FT dpz = square(dpx) + square(dpy);

            return
              (sign(-(square(dtx) + square(dty) - tw + qw) * dpz
                             + (dpz - pw + qw) * (dpx * dtx + dpy * dty)));
        }

        public static OrientedSideEnum power_side_of_oriented_power_circle(FT px, FT py, FT pwt,
                                              FT qx, FT qy, FT qwt,
                                              FT rx, FT ry, FT rwt,
                                              FT tx, FT ty, FT twt)
        {
            // Note: maybe this can be further optimized like the usual in_circle() ?

            // We translate the 4 points so that T becomes the origin.
            FT dpx = px - tx;
            FT dpy = py - ty;
            FT dpz = square(dpx) + square(dpy) - pwt + twt;
            FT dqx = qx - tx;
            FT dqy = qy - ty;
            FT dqz = square(dqx) + square(dqy) - qwt + twt;
            FT drx = rx - tx;
            FT dry = ry - ty;
            FT drz = square(drx) + square(dry) - rwt + twt;

            return (OrientedSideEnum)sign_of_determinant(dpx, dpy, dpz,
                                       dqx, dqy, dqz,
                                       drx, dry, drz);
        }

        public static OrientedSideEnum power_side_of_oriented_power_circle(FT px, FT py, FT pwt,
                                          FT qx, FT qy, FT qwt,
                                          FT tx, FT ty, FT twt)
        {
            // Same translation as above.
            FT dpx = px - tx;
            FT dpy = py - ty;
            FT dpz = square(dpx) + square(dpy) - pwt + twt;
            FT dqx = qx - tx;
            FT dqy = qy - ty;
            FT dqz = square(dqx) + square(dqy) - qwt + twt;

            // We do an orthogonal projection on the (x) axis, if possible.
            ST cmpx = compare(px, qx);
            if (cmpx != 0)
                return (OrientedSideEnum)(cmpx * sign_of_determinant(dpx, dpz, dqx, dqz));

            // If not possible, then on the (y) axis.
            var cmpy = compare(py, qy);
            return (OrientedSideEnum)(cmpy * sign_of_determinant(dpy, dpz, dqy, dqz));
        }

        public static OrientedSideEnum circumcenter_oriented_side_of_oriented_segment(FT ax, FT ay,
                                                         FT bx, FT by,
                                                         FT p0x, FT p0y,
                                                         FT p1x, FT p1y,
                                                         FT p2x, FT p2y)
        {
            FT dX = bx - ax;
            FT dY = by - ay;
            FT R0 = p0x * p0x + p0y * p0y;
            FT R1 = p1x * p1x + p1y * p1y;
            FT R2 = p2x * p2x + p2y * p2y;
            FT denominator = (p1x - p0x) * (p2y - p0y) +
                                   (p0x - p2x) * (p1y - p0y);
            FT det = 2 * denominator * (ax * dY - ay * dX)
                             - (R2 - R1) * (p0x * dX + p0y * dY)
                             - (R0 - R2) * (p1x * dX + p1y * dY)
                             - (R1 - R0) * (p2x * dX + p2y * dY);
            return (OrientedSideEnum)sign(det);
        }
    }

    public static partial class Mathex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void midpointC2(FT px, FT py,
                    FT qx, FT qy,
                   out FT x, out FT y)
        {
            x = (px + qx) / 2;
            y = (py + qy) / 2;
        }

        //CGAL_KERNEL_LARGE_INLINE
        public static void circumcenter_translateC2(FT dqx, FT dqy, FT drx, FT dry, out FT dcx, out FT dcy)
        {
            // Given 3 points P, Q, R, this function takes as input:
            // qx-px, qy-py, rx-px, ry-py.  And returns cx-px, cy-py,
            // where (cx, cy) are the coordinates of the circumcenter C.

            // What we do is intersect the bisectors.
            FT r2 = square(drx) + square(dry);
            FT q2 = square(dqx) + square(dqy);
            FT den = 2 * determinant(dqx, dqy, drx, dry);

            // The 3 points aren't collinear.
            // Hopefully, this is already checked at the upper level.
            CGAL_kernel_assertion(!is_zero(den));

            // One possible optimization here is to precompute 1/den, to avoid one
            // division.  However, we lose precision, and it's maybe not worth it (?).
            dcx = determinant(dry, dqy, r2, q2) / den;
            dcy = -determinant(drx, dqx, r2, q2) / den;
        }

        // CGAL_KERNEL_MEDIUM_INLINE
        public static void circumcenterC2(FT px, FT py, FT qx, FT qy, FT rx, FT ry, out FT x, out FT y)
        {
            circumcenter_translateC2(qx - px, qy - py, rx - px, ry - py, out x, out y);
            x += px;
            y += py;
        }

        public static void barycenterC2(FT p1x, FT p1y, FT w1,
                    FT p2x, FT p2y,
                    out FT x, out FT y)
        {
            FT w2 = 1 - w1;
            x = w1 * p1x + w2 * p2x;
            y = w1 * p1y + w2 * p2y;
        }

        public static void barycenterC2(FT p1x, FT p1y, FT w1,
                    FT p2x, FT p2y, FT w2,
                    out FT x, out FT y)
        {
            FT sum = w1 + w2;
            CGAL_kernel_assertion(sum != 0);
            x = (w1 * p1x + w2 * p2x) / sum;
            y = (w1 * p1y + w2 * p2y) / sum;
        }

        public static void barycenterC2(FT p1x, FT p1y, FT w1,
                    FT p2x, FT p2y, FT w2,
                    FT p3x, FT p3y,
                    out FT x, out FT y)
        {
            FT w3 = 1 - w1 - w2;
            x = w1 * p1x + w2 * p2x + w3 * p3x;
            y = w1 * p1y + w2 * p2y + w3 * p3y;
        }

        public static void barycenterC2(FT p1x, FT p1y, FT w1,
                    FT p2x, FT p2y, FT w2,
                    FT p3x, FT p3y, FT w3,
                    out FT x, out FT y)
        {
            FT sum = w1 + w2 + w3;
            CGAL_kernel_assertion(sum != 0);
            x = (w1 * p1x + w2 * p2x + w3 * p3x) / sum;
            y = (w1 * p1y + w2 * p2y + w3 * p3y) / sum;
        }

        public static void barycenterC2(FT p1x, FT p1y, FT w1,
                    FT p2x, FT p2y, FT w2,
                    FT p3x, FT p3y, FT w3,
                    FT p4x, FT p4y,
                    out FT x, out FT y)
        {
            FT w4 = 1 - w1 - w2 - w3;
            x = w1 * p1x + w2 * p2x + w3 * p3x + w4 * p4x;
            y = w1 * p1y + w2 * p2y + w3 * p3y + w4 * p4y;
        }

        public static void barycenterC2(FT p1x, FT p1y, FT w1,
                    FT p2x, FT p2y, FT w2,
                    FT p3x, FT p3y, FT w3,
                    FT p4x, FT p4y, FT w4,
                    out FT x, out FT y)
        {
            FT sum = w1 + w2 + w3 + w4;
            CGAL_kernel_assertion(sum != 0);
            x = (w1 * p1x + w2 * p2x + w3 * p3x + w4 * p4x) / sum;
            y = (w1 * p1y + w2 * p2y + w3 * p3y + w4 * p4y) / sum;
        }

        // CGAL_KERNEL_MEDIUM_INLINE
        public static void centroidC2(FT px, FT py,
                   FT qx, FT qy,
                   FT rx, FT ry,
                   out FT x, out FT y)
        {
            x = (px + qx + rx) / 3;
            y = (py + qy + ry) / 3;
        }

        // CGAL_KERNEL_MEDIUM_INLINE
        public static void centroidC2(FT px, FT py,
                   FT qx, FT qy,
                   FT rx, FT ry,
                   FT sx, FT sy,
                   out FT x, out FT y)
        {
            x = (px + qx + rx + sx) / 4;
            y = (py + qy + ry + sy) / 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void
        line_from_pointsC2(FT px, FT py,
                          FT qx, FT qy,
                          out FT a, out FT b, out FT c)
        {
            // The horizontal and vertical line get a special treatment
            // in order to make the intersection code robust for doubles
            if (py == qy)
            {
                a = 0;
                if (qx > px)
                {
                    b = 1;
                    c = -py;
                }
                else if (qx == px)
                {
                    b = 0;
                    c = 0;
                }
                else
                {
                    b = -1;
                    c = py;
                }
            }
            else if (qx == px)
            {
                b = 0;
                if (qy > py)
                {
                    a = -1;
                    c = px;
                }
                else if (qy == py)
                {
                    a = 0;
                    c = 0;
                }
                else
                {
                    a = 1;
                    c = -px;
                }
            }
            else
            {
                a = py - qy;
                b = qx - px;
                c = -px * a - py * b;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void
        line_from_point_directionC2(FT px, FT py,
                                   FT dx, FT dy,
                                   out FT a, out FT b, out FT c)
        {
            a = -dy;
            b = dx;
            c = px * dy - py * dx;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT approximate_sqrt(FT v) => Math.Sqrt(v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT sqrt(FT v) => Math.Sqrt(v);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void
        bisector_of_pointsC2(FT px, FT py, FT dx, FT dy, FT qx, FT qy, out FT a, out FT b, out FT c)
        {
            a = 2 * (px - qx);
            b = 2 * (py - qy);
            c = square(qx) + square(qy) -
                square(px) - square(py);
        }

        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static void
        //bisector_of_pointsC2(FT px, FT py, FT dx, FT dy, FT qx, FT qy, out FT a, out FT b, out FT c)
        //{
        //    a = 2 * (px - qx);
        //    b = 2 * (py - qy);
        //    c = square(qx) + square(qy) -
        //        square(px) - square(py);
        //}


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void bisector_of_linesC2(FT pa, FT pb, FT pc,
                          FT qa, FT qb, FT qc,
                          out FT a, out FT b, out FT c)
        {
            // We normalize the equations of the 2 lines, and we then add them.
            FT n1 = approximate_sqrt((square(pa) + square(pb)));
            FT n2 = approximate_sqrt((square(qa) + square(qb)));
            a = n2 * pa + n1 * qa;
            b = n2 * pb + n1 * qb;
            c = n2 * pc + n1 * qc;

            // Care must be taken for the case when this produces a degenerate line.
            if (a == 0 && b == 0)
            {
                a = n2 * pa - n1 * qa;
                b = n2 * pb - n1 * qb;
                c = n2 * pc - n1 * qc;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT line_y_at_xC2(FT a, FT b, FT c, FT x)
        {
            return (-a * x - c) / b;
        }

        // Silence a warning for MSVC 2017
        // > include\cgal\constructions\kernel_ftc2.h(287) :
        // >   warning C4723: potential divide by 0

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void line_get_pointC2(FT a, FT b, FT c, FT i, out FT x, out FT y)
        {
            if (is_zero(b))
            {
                x = -c / a;
                y = 1 - i * a;
            }
            else
            {
                x = 1 + i * b;
                y = -(a + c) / b - i * a;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void perpendicular_through_pointC2(FT la, FT lb,
                                    FT px, FT py,
                                    out FT a, out FT b, out FT c)
        {
            a = -lb;
            b = la;
            c = lb * px - la * py;
        }

        // CGAL_KERNEL_MEDIUM_INLINE
        public static void line_project_pointC2(FT la, FT lb, FT lc, FT px, FT py, out FT x, out FT y)
        {
            if ((is_zero(la))) // horizontal line
            {
                x = px;
                y = -lc / lb;
            }
            else if ((is_zero(lb))) // vertical line
            {
                x = -lc / la;
                y = py;
            }
            else
            {
                FT a2 = square(la);
                FT b2 = square(lb);
                FT d = a2 + b2;
                x = (b2 * px - la * lb * py - la * lc) / d;
                y = (-la * lb * px + a2 * py - lb * lc) / d;
            }
        }

        // CGAL_KERNEL_MEDIUM_INLINE
        public static FT squared_radiusC2(FT px, FT py, FT qx, FT qy, FT rx, FT ry, out FT x, out FT y)
        {
            circumcenter_translateC2(qx - px, qy - py, rx - px, ry - py, out x, out y);
            FT r2 = square(x) + square(y);
            x += px;
            y += py;
            return r2;
        }

        // CGAL_KERNEL_MEDIUM_INLINE
        public static FT squared_radiusC2(FT px, FT py,
               FT qx, FT qy,
               FT rx, FT ry)
        {
            FT x, y;
            circumcenter_translateC2(qx - px, qy - py, rx - px, ry - py, out x, out y);
            return square(x) + square(y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT squared_distanceC2(FT px, FT py, FT qx, FT qy)
        {
            return square(px - qx) + square(py - qy);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT squared_radiusC2(FT px, FT py,
                       FT qx, FT qy)
        {
            return squared_distanceC2(px, py, qx, qy) / 4f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT scaled_distance_to_lineC2(FT la, FT lb, FT lc,
                                 FT px, FT py)
        {
            // for comparisons, use distance_to_directionsC2 instead
            // since lc is irrelevant
            return la * px + lb * py + lc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FT scaled_distance_to_directionC2(FT la, FT lb, FT px, FT py)
        {
            // scalar product with direction
            return la * px + lb * py;
        }

        // CGAL_KERNEL_MEDIUM_INLINE
        public static FT scaled_distance_to_lineC2(FT px, FT py, FT qx, FT qy, FT rx, FT ry)
        {
            return determinant(px - rx, py - ry, qx - rx, qy - ry);
        }

        public static void weighted_circumcenter_translateC2(RT dqx, RT dqy, RT dqw,
                                        RT drx, RT dry, RT drw,
                                       out RT dcx, out RT dcy)
        {
            // Given 3 points P, Q, R, this function takes as input:
            // qx-px, qy-py,qw-pw,  rx-px, ry-py, rw-pw.  And returns cx-px, cy-py,
            // where (cx, cy) are the coordinates of the circumcenter C.

            // What we do is intersect the radical axis
            RT r2 = square(drx) + square(dry) - drw;
            RT q2 = square(dqx) + square(dqy) - dqw;

            RT den = 2 * determinant(dqx, dqy, drx, dry);

            // The 3 points aren't collinear.
            // Hopefully, this is already checked at the upper level.
            CGAL_assertion(den != 0);

            // One possible optimization here is to precompute 1/den, to avoid one
            // division.  However, we lose precision, and it's maybe not worth it (?).
            dcx = determinant(dry, dqy, r2, q2) / den;
            dcy = -determinant(drx, dqx, r2, q2) / den;
        }

        //template < class RT >

        public static void weighted_circumcenterC2(RT px, RT py, We pw,
                               RT qx, RT qy, We qw,
                               RT rx, RT ry, We rw,
                               out RT x, out RT y)
        {
            RT dqw = (RT)(qw - pw);
            RT drw = (RT)(rw - pw);

            weighted_circumcenter_translateC2(qx - px, qy - py, dqw, rx - px, ry - py, drw, out x, out y);
            x += px;
            y += py;
        }

        public static FT power_productC2(FT px, FT py, FT pw, FT qx, FT qy, FT qw)
        {
            // computes the power product of two weighted points
            FT qpx = qx - px;
            FT qpy = qy - py;
            FT qp2 = square(qpx) + square(qpy);
            return qp2 - pw - qw;
        }

        public static void radical_axisC2(RT px, RT py, We pw,
                     RT qx, RT qy, We qw,
                     out RT a, out RT b, out RT c)
        {
            a = (RT)(2) * (px - qx);
            b = (RT)(2) * (py - qy);
            c = -square(px) - square(py)
                + square(qx) + square(qy)
                + (RT)(pw) - (RT)(qw);
        }

        // CGAL_KERNEL_MEDIUM_INLINE
        public static FT
        squared_radius_orthogonal_circleC2(FT px, FT py, FT pw, FT qx, FT qy, FT qw, FT rx, FT ry, FT rw)
        {
            FT FT4 = (4);
            FT dpx = px - rx;
            FT dpy = py - ry;
            FT dqx = qx - rx;
            FT dqy = qy - ry;
            FT dpp = square(dpx) + square(dpy) - pw + rw;
            FT dqq = square(dqx) + square(dqy) - qw + rw;

            FT det0 = determinant(dpx, dpy, dqx, dqy);
            FT det1 = determinant(dpp, dpy, dqq, dqy);
            FT det2 = determinant(dpx, dpp, dqx, dqq);

            return (square(det1) + square(det2)) /
                                            (FT4 * square(det0)) - rw;
        }

        // CGAL_KERNEL_MEDIUM_INLINE
        public static FT squared_radius_smallest_orthogonal_circleC2(FT px, FT py, FT pw, FT qx, FT qy, FT qw)
        {
            FT FT4 = (4);
            FT dpz = square(px - qx) + square(py - qy);
            return (square(dpz - pw + qw) / (FT4 * dpz) - qw);
        }
    }
    public static partial class Mathex
    {
        public static bool collinear_are_ordered_along_line(in Point2 p, in Point2 q, in Point2 r)
        {
            return collinear_are_ordered_along_line(p.X, p.Y, q.X, q.Y, r.X, r.Y);
        }

        public static bool collinear(in Point2 p, in Point2 q, in Point2 r)
        {
            return ( orientation(p, q, r) == OrientationEnum.COLLINEAR);
        }

        public static bool are_ordered_along_line(in Point2 p, in Point2 q, in Point2 r) => collinear(p, q, r) && collinear_are_ordered_along_line(p, q, r);
    }
}