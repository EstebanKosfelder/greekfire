namespace GFL.Kernel
{
    public class Line2D
    {
        private double _a, _b, _c;

        public Line2D()
        {
            _a = _b = _c = 0;
        }

        public Line2D(double a, double b, double c)
        {
            _a = a;
            _b = b;
            _c = c;
        }

        public Vector2D to_vector() => new Vector2D(A, B);

        // Devuelve un punto sobre la línea
        public (double x, double y) PointOnLine()
        {
            // Encuentra un punto arbitrario sobre la línea
            if (Math.Abs(B) > 1e-9)
            {
                double x = 1.0;
                double y = -(A * x + C) / B;
                return (x, y);
            }
            else
            {
                double y = 1.0;
                double x = -(B * y + C) / A;
                return (x, y);
            }
        }

        // Devuelve vector normal unitario apuntando "hacia adentro"
        public (double dx, double dy) NormalDirection()
        {
            double length = Math.Sqrt(A * A + B * B);
            return (A / length, B / length);
        }

        // Mueve la línea una unidad en dirección normal
        public Line2D LineAtOne()
        {
            var (nx, ny) = NormalDirection();

            // La nueva línea está desplazada en dirección normal
            // Ax + By + (C - (A*nx + B*ny)) = 0
            double delta = A * nx + B * ny;
            double newC = C - delta;

            return new Line2D(A, B, newC);
        }

        public Line2D(Vector2D p, Vector2D q)
        {
            if (p.Y == q.Y)
            {
                _a = 0;
                if (q.X > p.X)
                {
                    _b = 1;
                    _c = -p.Y;
                }
                else if (q.X == p.X)
                {
                    _b = 0;
                    _c = 0;
                }
                else
                {
                    _b = -1;
                    _c = p.Y;
                }
            }
            else if (q.X == p.X)
            {
                _b = 0;
                if (q.Y > p.Y)
                {
                    _a = -1;
                    _c = p.X;
                }
                else if (q.Y == p.Y)
                {
                    _a = 0;
                    _c = 0;
                }
                else
                {
                    _a = 1;
                    _c = -p.X;
                }
            }
            else
            {
                _a = p.Y - q.Y;
                _b = q.X - p.X;
                _c = -p.X * _a - p.Y * _b;
            }
        }

        //public Line_2(Vector_2 p, Direction d)
        //{
        //    _a = -d.DY;
        //    _b = d.DX;
        //    _c = p.X * d.DY - p.Y * d.DX;
        //}

        public double GetY(double x)
        {
            return ((_a * x + _c) / (-_b));
        }

        public double GetX(double y)
        {
            return ((_b * y + _c) / (-_a));
        }
        public Vector2D point() => IsVertical ? new Vector2D(GetX(0.0), 0.0) : new Vector2D(0.0, GetY(0.0));
        public bool IsVertical
        { get { return _b.IsZero(); } }

        public bool IsHorizontal
        { get { return _a.IsZero(); } }

        public Vector2D Point(double distance)
        {
            Vector2D result;
            if (_b.IsZero())
            {
                result = new Vector2D((-_b - _c) / _a + distance * _b, 1 - distance * _a);
            }
            else
            {
                result = new Vector2D(1 + distance * _b, -(_a + _c) / _b - distance * _a);
            }
            return result;
        }

        public double A
        { get { return _a; } }

        public double B
        { get { return _b; } }

        public double C
        { get { return _c; } set { _c = value; } }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
                return true;
            if (!(obj is Line2D))
                return false;
            Line2D l = obj as Line2D;
            return (this.A == A && this.B == l.B && this.C == l.C);
        }

        public static bool operator ==(Line2D a, Line2D b)
        {
            if (System.Object.ReferenceEquals(a, null))
            {
                if (System.Object.ReferenceEquals(b, null))
                {
                    return true;
                }
                return false;
            }
            else if (System.Object.ReferenceEquals(b, null))
            {
                return false;
            }
            return a.Equals(b);
        }

        public static bool operator !=(Line2D a, Line2D b)
        {
            return !(a == b);
        }

        public bool has_on(Vector2D point) => has_on_boundary(point);

        public Boolean has_on_boundary(Vector2D p)
        {
            return oriented_side(p) == (int)ESign.ON_ORIENTED_BOUNDARY;
        }

        public bool is_vertical() => B == 0;

        public int oriented_side(Vector2D p)
        {
            return Math.Sign(A * p.X + B * p.Y + C);
        }

        public override int GetHashCode()
        {
            HashCode hc = new HashCode();
            hc.Add(A);
            hc.Add(B);
            hc.Add(C);
            return hc.ToHashCode();
        }

        /*
        Line_2 	transform( Aff_transformation_2 &t)
        {
            return Line_2(t.transform(point(0)),
                          t.transform(direction()));
        }

        Line_2
            opposite()
        {
            return R().ruct_opposite_Line2_object()(*this);
        }

        Direction_2
            direction()
        {
            return R().ruct_direction_2_object()(*this);
        }

        Vector_2
            to_vector()
        {
            return R().ruct_vector_2_object()(*this);
        }
 */

        public Line2D perpendicular(Vector2D p)
        {
            return new Line2D(-_b, _a, _b * p.X - _a * p.Y);
        }
        //public Line_2 perpendicular(Point2D p)
        //{
        //    return new Line_2(-_b, _a, _b * p.X - _a * p.Y);
        //}

        public bool isReflex(Line2D other)
        {
            return ((this.A * other.B) < (other.A * this.B));
        }

        public double Distance(Vector2D p)
        {
            return _a * p.X + _b * p.Y + _c;
        }

        public int SideOfOriented(Vector2D p)
        {
            return Math.Sign(Distance(p));
        }

        public Line2D Translate(Vector2D v)
        {
            double newC = C - A * v.X - B * v.Y;
            return new Line2D(A, B, newC);
        }

        public Vector2D projection(Vector2D p)
        {
            double x = 0, y = 0;
            // Original old version
            if (A.IsZero())
            { // horizontal line
                x = p.X;
                y = -C / B;
            }
            else if (B.IsZero())
            { // vertical line
                x = -C / A;
                y = p.Y;
            }
            else
            {
                double a2 = A * A;
                double b2 = B * B;
                double d = a2 + b2;
                x = (A * (B * p.Y - C) - p.X * b2) / d;
                y = (B * (C - A * p.X) + p.Y * a2) / d;
            }
            return new Vector2D(x, y);
        }

        public override string ToString()
        {
            return string.Format("[L[{0},{1},{2}]", A, B, C);
        }

        internal Line2D opposite()
        {
            return new Line2D(-A, -B, -C);
        }

        public struct Direction : IComparer<Direction>, IComparable<Direction>
        {
            public Direction(double x, double y)
            {
                DX = x;
                DY = y;
            }

            public Direction(Vector2D v)
                : this(v.X, v.Y)
            {
            }

            public double DX;
            public double DY;

            public double this[int index]
            {
                get
                {
                    if (!((index == 0) || (index == 1)))
                    {
                        throw new IndexOutOfRangeException();
                    }

                    return (index == 0) ? DX : DY;
                }
                set
                {
                    if (!((index == 0) || (index == 1)))
                    {
                        throw new IndexOutOfRangeException();
                    }
                    if (index == 0)
                        DX = value;
                    else
                        DY = value;
                }
            }

            public int Dimension
            {
                get { return 2; }
            }

            public override bool Equals(Object obj)
            {
                if (obj is Direction other) return DX.Equals(other.DX) && DY.Equals(other.DY);
                return false;
            }

            public static bool operator ==(Direction a, Direction b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(Direction a, Direction b)
            {
                return !(a == b);
            }

            public static bool operator <(Direction a, Direction b)
            {
                return a.CompareTo(b) < 0;
            }

            public static bool operator <=(Direction a, Direction b)
            {
                return a.CompareTo(b) < 1;
            }

            public static bool operator >(Direction a, Direction b)
            {
                return a.CompareTo(b) > 0;
            }

            public static bool operator >=(Direction a, Direction b)
            {
                return a.CompareTo(b) > -1;
            }

            public static Direction operator -(Direction a)
            {
                return new Direction(-a.DX, -a.DY);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            #region IComparer implementation

            public int Compare(Direction x, Direction y)
            {
                return MathEx.compare_angle_with_x_axisC2(x.DX, x.DY, y.DX, y.DY);
            }

            #endregion IComparer implementation

            #region IComparable implementation

            public int CompareTo(Direction other)
            {
                return MathEx.compare_angle_with_x_axisC2(this.DX, this.DY, other.DX, other.DY);
            }

            #endregion IComparable implementation

            public static bool Counterclockwise_in_between(Direction p, Direction q, Direction r)
            {
                if (q < p)
                    return (p < r) || (r <= q);
                else
                    return (p < r) && (r <= q);
            }

            public static bool Counterclockwise_at_or_in_between_2(Direction p, Direction q, Direction r)
            {
                return p == q || p == r || Counterclockwise_in_between(p, q, r);
            }
        }

       
    }

}