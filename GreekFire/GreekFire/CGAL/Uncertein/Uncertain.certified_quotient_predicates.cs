using CGAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static CGAL.DebuggerInfo;

using static CGAL.UncertainExtensions;
using NT = double;
namespace CGAL
{
    public static partial class Mathex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_rational_is_positive(Rational x)
        {
            UncertainCompareResult signum = certified_sign(x.num);
            UncertainCompareResult sigden = certified_sign(x.den);
            UncertainCompareResult zero = (UncertainCompareResult)0;
            return ((signum != zero).And(signum == sigden));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_rational_is_negative(Rational x)
        {
            UncertainCompareResult signum = certified_sign(x.num);
            UncertainCompareResult sigden = certified_sign(x.den);
            UncertainCompareResult zero = (UncertainCompareResult)(0);

            return ( (signum != zero).And(signum != sigden));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_rational_is_zero(Rational x)
        {
            return certified_is_zero(x.num);
        }


        //CGAL_MEDIUM_INLINE
        public static UncertainCompareResult certified_rational_sign(Rational x)
        {
            // No assumptions on the sign of  den  are made

            return certified_sign(x.num) * certified_sign(x.den);
        }


        //CGAL_MEDIUM_INLINE
        public static UncertainCompareResult certified_rational_compare(Rational x, Rational y)
        {
            UncertainCompareResult r = UncertainCompareResult.indeterminate;

            // No assumptions on the sign of  den  are made

            // code assumes that -1 == - 1;


            UncertainCompareResult xnumsign = certified_sign(x.num);
            UncertainCompareResult xdensign = certified_sign(x.den);
            UncertainCompareResult ynumsign = certified_sign(y.num);
            UncertainCompareResult ydensign = certified_sign(y.den);

            if (is_certain(xnumsign)
               && is_certain(xdensign)
               && is_certain(ynumsign)
               && is_certain(ydensign)
               )
            {
                int xsign = (int)xnumsign * (int)xdensign;
                int ysign = (int)ynumsign * (int)ydensign;
                if (xsign == 0) return (UncertainCompareResult)( - ysign);
                if (ysign == 0) return (UncertainCompareResult)(xsign);
                // now (x != 0) && (y != 0)
                int diff = xsign - ysign;
                if (diff == 0)
                {
                    int msign = (int)xdensign * (int)ydensign;
                    NT leftop = x.num * y.den * msign;
                    NT rightop = y.num * x.den * msign;
                    r = certified_compare(leftop, rightop);
                }
                else
                {
                    r = (xsign < ysign) ? (UncertainCompareResult)( -1) : (UncertainCompareResult) 1;
                }
            }

            return r;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_zero(Rational n)
        {
            return certified_rational_is_zero(n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_positive(Rational n)
        {
            return certified_rational_is_positive(n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainBool certified_is_negative(Rational n)
        {
            return certified_rational_is_negative(n);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainCompareResult certified_sign(Rational n)
        {
            return certified_rational_sign(n);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UncertainCompareResult certified_compare(Rational n1, Rational n2)
        {
            return certified_rational_compare(n1, n2);
        }
    }
} // end namespace CGAL
