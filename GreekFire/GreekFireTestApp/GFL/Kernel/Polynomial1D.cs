
using System;
using System.Diagnostics;

namespace GFL.Kernel
{



    public struct Polynomial1D
    {
        double[] a_ = new double[3];
        int degree_;
        public Polynomial1D(double a0 = 0.0, double a1 = 0.0, double a2 = 0.0)
        {
            a_ = new[] { a0, a1, a2 };
            degree_ = (a2 != Const.CORE_ZERO) ? 2 : (a1 != Const.CORE_ZERO) ? 1 : 0;
        }

        public int Degree { get=>degree_;  set => degree_ = value; }
        [Obsolete]
        public int degree() => degree_;
        public int sign() => Math.Sign(a_[degree_]);
        public double this[int index] => a_[index];

        public Polynomial1D differentiate() => new Polynomial1D(a_[1], 2 * a_[2]);

       public  double evaluate(double at) => a_[2] * (at * at) + a_[1] * at + a_[0];

        public static Polynomial1D operator *(Polynomial1D m, Polynomial1D o)
        {
            Debug.Assert(m.degree_ + o.degree_ <= 2);
            return new Polynomial1D(
                m.a_[0] * o.a_[0],
                m.a_[0] * o.a_[1] + m.a_[1] * o.a_[0],
                m.a_[0] * o.a_[2] + m.a_[1] * o.a_[1] + m.a_[2] * o.a_[0]);
        }
        public static Polynomial1D operator +(Polynomial1D m, Polynomial1D o)
        {
            return new Polynomial1D(m.a_[0] + o.a_[0], m.a_[1] + o.a_[1], m.a_[2] + o.a_[2]);
        }
        public static Polynomial1D operator -(Polynomial1D m, Polynomial1D o)
        {
            return new Polynomial1D(m.a_[0] - o.a_[0], m.a_[1] - o.a_[1], m.a_[2] - o.a_[2]);
        }

        public override string ToString()
        {
            switch (degree_)
            {
                case 0: return $"({a_[0]})";
                case 1: return $"({a_[1]}*x +{a_[0]})";
                case 2: return $"({a_[2]}*x^2  +{a_[1]}*x +{a_[0]})";
                default: return $"Degree Err {degree_} ";
            }
        }

     

        public (bool has_real_roots, bool is_square) solve_quadratic(out double x0, out double x1)
        {
            (bool, bool) res;
            Debug.Assert(degree_ == 2);

            double a = (a_[2]);
            double b = (a_[1]);
            double c = (a_[0]);
            //LOG(DEBUG) << " d: " << CGAL::to_double(a) << "t^2 + "
            //                    << CGAL::to_double(b) << "t + "
            //                    << CGAL::to_double(c);

            /* CGAL can factorize polynomials, but only square-free ones,
             * which doesn't help us much, alas.
             */
            double radicand = (b * b - 4.0 * a * c);
            if (radicand == 0)
            {
                x0 = x1 = (-b) / (2.0 * a);
                res = (true, true);
            }
            else if (radicand > 0)
            {
                double root = Math.Sqrt(radicand);
                double divisor = (2.0 * a);
                int sign = this.sign();
                if (sign == (int)ESign.Positive)
                {
                    x0 = (-b - root) / divisor;
                    x1 = (-b + root) / divisor;
                }
                else
                {
                    Debug.Assert(sign == (int)ESign.Negative);
                    x1 = (-b - root) / divisor;
                    x0 = (-b + root) / divisor;
                }
                //LOG(DEBUG) << " x0: " << CGAL::to_double(x0);
                //LOG(DEBUG) << " x1: " << CGAL::to_double(x1);
                //LOG(DEBUG) << " x0: " << x0;
                //LOG(DEBUG) << " x1: " << x1;
                res = (true, false);
            }
            else
            {
                x0 = x1 = double.NaN;
                res = (false, false);
            }

            return res;
        }




    }
}