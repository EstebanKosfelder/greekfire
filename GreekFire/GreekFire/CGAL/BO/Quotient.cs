namespace CGAL
{
    public struct Quotient
    {
        public double Num { get; private set; }
        public double Den { get; private set; }

        public Quotient(double num, double den = 1)
        {
            if (den == 0)
            {
                throw new ArgumentException("Denominator cannot be zero.");
            }

            Num = num;
            Den = den;
            Simplify();
        }

        public Quotient(double n) : this(n, 1) { }

        public static Quotient operator +(Quotient q1, Quotient q2)
        {
            double num = q1.Num * q2.Den + q2.Num * q1.Den;
            double den = q1.Den * q2.Den;
            return new Quotient(num, den);
        }

        public static Quotient operator -(Quotient q1, Quotient q2)
        {
            double num = q1.Num * q2.Den - q2.Num * q1.Den;
            double den = q1.Den * q2.Den;
            return new Quotient(num, den);
        }

        public static Quotient operator *(Quotient q1, Quotient q2)
        {
            double num = q1.Num * q2.Num;
            double den = q1.Den * q2.Den;
            return new Quotient(num, den);
        }

        public static Quotient operator /(Quotient q1, Quotient q2)
        {
            if (q2.Num == 0)
            {
                throw new ArgumentException("Cannot divide by zero.");
            }

            double num = q1.Num * q2.Den;
            double den = q1.Den * q2.Num;
            return new Quotient(num, den);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is Quotient other)) return false;
            

            return   Num * other.Den == Den * other.Num;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Num, Den);
        }

        public override string ToString()
        {
            return $"{Num}/{Den}";
        }

        private void Simplify()
        {
            double gcd = GCD(Num, Den);
            Num /= gcd;
            Den /= gcd;
        }

        private static double GCD(double a, double b)
        {
            while (b != 0)
            {
                double temp = b;
                b = a % b;
                a = temp;
            }

            return a;
        }
    }
}