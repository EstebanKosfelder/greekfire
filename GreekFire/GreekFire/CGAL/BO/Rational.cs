using System;
using FT = System.Double;
using static CGAL.Mathex;
namespace CGAL
{
    public struct Rational // :IComparable<Rational>
    {
        public Rational(FT aN, FT aD)
        {
            num = (aN); den = (aD);
        }
        public static implicit operator FT ( Rational r) { return r.num / r.den; }

        internal FT n()
        {
            return num;
        }

        internal FT d()
        {
            return den;
        }

        internal double to_nt() => num / den;

        //CGAL::Quotient<FT> to_quotient() { return CGAL::Quotient<FT>(mN, mD) ; }


        public FT num { get; private set; }
        public FT den { get; private set; }

        // TODO
        public bool IsPositive
        {
            get { return (are_near (num,0)?0:sign(num)) * (are_near(den,0)?0:sign(den)) >0; }
        }

        public bool IsNaN => FT.IsNaN(den) || FT.IsNaN(num); 


    }
}