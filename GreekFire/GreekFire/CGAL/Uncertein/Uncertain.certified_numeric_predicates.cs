using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static CGAL.Mathex;

using static CGAL.UncertainExtensions;
using FT = double;
namespace CGAL
{
    public static partial class Mathex
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
     public static  OrientationEnum   side_of_oriented_lineC2(FT a, FT b, FT c,
                        FT x, FT y)
{
         return (OrientationEnum)   sign(a* x+b* y+c);
    }

    public static bool is_valid(double value) => !double.IsNaN(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool logical_or(UncertainBool a, UncertainBool b) { return a.Or(b); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool logical_and(UncertainBool a, UncertainBool b) { return a.And(b); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool logical_or(UncertainBool a, UncertainBool b, UncertainBool c) { return a.Or(b).Or(c); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool logical_and(UncertainBool a, UncertainBool b, UncertainBool c) { return a.And(b).And(c); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_zero(FT x)
        {
            return is_valid(x) ? make_uncertain(is_zero(x)) : UncertainBool.indeterminate;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_not_zero(FT x)
        {
            return is_valid(x) ? make_uncertain(!is_zero(x)) : UncertainBool.indeterminate;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_one(FT x)
        {
            return is_valid(x) ? make_uncertain(is_one(x)) : UncertainBool.indeterminate;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_negative(FT x)
        {
            return is_valid(x) ? make_uncertain(is_negative(x)) : UncertainBool.indeterminate;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_positive(FT x)
        {
            return is_valid(x) ? make_uncertain(is_positive(x)) : UncertainBool.indeterminate;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainCompareResult certified_sign(FT x)
        {
            return is_valid(x) ? make_uncertain(sign(x)) : UncertainCompareResult.indeterminate;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainCompareResult certified_compare(FT n1, FT n2)
        {
            return is_valid(n1) && is_valid(n2) ? make_uncertain(compare(n1, n2)) : UncertainCompareResult.indeterminate;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
       

        public static UncertainBool certified_is_smaller(UncertainCompareResult c)
        {
            return c == -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_equal(UncertainCompareResult c)
        {
            return c == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_large(UncertainCompareResult c)
        {
            return c == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_smaller_or_equal(UncertainCompareResult c)
        {
            return logical_or(c == -1, c == 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_1_or_equal(UncertainCompareResult c)
        {
            return logical_or(c == 1, c == 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_smaller(FT n1, FT n2)
        {
            return certified_is_smaller(certified_compare(n1, n2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_equal(FT n1, FT n2)
        {
            return certified_is_equal(certified_compare(n1, n2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_large(FT n1, FT n2)
        {
            return certified_is_large(certified_compare(n1, n2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_smaller_or_equal(FT n1, FT n2)
        {
            return certified_is_smaller_or_equal(certified_compare(n1, n2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_smaller_or_equal(Rational n1, Rational n2)
        {
            return certified_is_smaller_or_equal(certified_rational_compare(n1, n2));
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_1_or_equal(FT n1, FT n2)
        {
            return certified_is_1_or_equal(certified_compare(n1, n2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainCompareResult certified_sign_of_determinant2x2(FT a00, FT a01, FT a10, FT a11)
        {
            return certified_compare(a00 * a11, a10 * a01);
        }
    }
}