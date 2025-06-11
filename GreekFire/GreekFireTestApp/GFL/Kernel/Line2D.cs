namespace GFL.Kernel
{

    
    public class Line2D_new
    {
        // Ax + By + C = 0
        public readonly double A;
        public readonly double B;
        public readonly double C;

        /// <summary>
        /// Constructor por coeficientes
        /// </summary>
        public Line2D_new(double A, double B, double C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        /// <summary>
        /// Crea una línea desde dos puntos
        /// </summary>
        public static Line2D_new FromTwoPoints(Vector2D p, Vector2D q)
        {
            double A = q.Y - p.Y;
            double B = p.X - q.X;
            double C = q.X * p.Y - p.X * q.Y;
            return new Line2D_new(A, B, C);
        }

        /// <summary>
        /// Crea una línea desde un punto y una dirección (vector director)
        /// </summary>
        public static Line2D_new FromPointAndDirection(Vector2D p, Vector2D direction)
        {
            // La línea pasa por p, tiene vector director direction = (dx, dy)
            // Perpendicular: (-dy, dx)
            double A = -direction.Y;
            double B = direction.X;
            double C = -A * p.X - B * p.Y;
            return new Line2D_new(A, B, C);
        }

        /// <summary>
        /// Devuelve un punto arbitrario sobre la línea
        /// </summary>
        public Vector2D PointOnLine()
        {
            if (Math.Abs(B) > 1e-9)
            {
                return new Vector2D(1.0, -(A * 1.0 + C) / B);
            }
            else if (Math.Abs(A) > 1e-9)
            {
                return new Vector2D(-(B * 1.0 + C) / A, 1.0);
            }
            else
            {
                throw new InvalidOperationException("Línea inválida");
            }
        }

        /// <summary>
        /// Devuelve el vector normal unitario apuntando hacia adentro del wavefront
        /// </summary>
        public Vector2D NormalDirection()
        {
            return new Vector2D(A, B).N;
        }

        /// <summary>
        /// Devuelve la dirección tangente (paralela a la línea)
        /// </summary>
        public Vector2D Direction()
        {
            return new Vector2D(B, -A).N;
        }
        public Vector2D to_vector()=>Direction();

        /// <summary>
        /// Determina si un punto está sobre la línea
        /// </summary>
        public bool HasOn(Vector2D p)
        {
            double val = A * p.X + B * p.Y + C;
            return val.IsZero();
        }
        public double GetY(double x)
        {
            return ((A * x + C) / (-B));
        }

        public double GetX(double y)
        {
            return ((B * y + C) / (-A));
        }
        public bool Intersection(Rect2D b,out Vector2D start,out Vector2D end  )
        {
            Vector2D rp = this.PointOnLine();

         

            // Caso especial: línea vertical
            if (IsVertical)
            {
                if (rp.X < b.Left || rp.X > b.Right)
                {
                    start = end = Vector2D.NaN;
                    return false;
                }

                start = new Vector2D(rp.X, b.Top);
                end = new Vector2D(rp.X, b.Bottom);
                return true;
            }

            // Caso especial: línea horizontal
            if (IsHorizontal)
            {
                if (rp.Y < b.Bottom || rp.Y > b.Top)
                {
                    start = end = Vector2D.NaN;
                    return false;
                }

                start = new Vector2D(b.Left, rp.Y);
                end = new Vector2D(b.Right, rp.Y);
                return true;
            }

            // Caso general: línea oblicua
            List<Vector2D> intersections = new List<Vector2D>();

            // Intersectar con bordes verticales
            double yLeft = GetY(b.Left);
            if (yLeft <= b.Bottom && yLeft >= b.Top)
                intersections.Add(new Vector2D(b.Left, yLeft));

            double yRight = GetY(b.Right);
            if (yRight <= b.Bottom && yRight >= b.Top)
                intersections.Add(new Vector2D(b.Right, yRight));

            // Intersectar con bordes horizontales
            double xTop = GetX(b.Top);
            if (xTop >= b.Left && xTop <= b.Right)
                intersections.Add(new Vector2D(xTop, b.Top));

            double xBottom = GetX(b.Bottom);
            if (xBottom >= b.Left && xBottom <= b.Right)
                intersections.Add(new Vector2D(xBottom, b.Bottom));

            // También revisar esquinas en caso de coincidencia exacta
            foreach (var corner in b.GetCorners())
            {
                if (HasOn(corner))
                    intersections.Add(corner);
            }

            // Si tenemos al menos dos puntos, devolvemos el segmento
            if (intersections.Count >= 2)
            {
                // Eliminar duplicados para evitar puntos repetidos
                var unique = intersections.Distinct().ToList();
                if (unique.Count >= 2)
                {
                    start = unique[0];
                    end = unique[1];
                    return true;
                }
            }

            start = end = Vector2D.NaN;
            return false;

        }








        /// <summary>
        /// Encuentra la intersección con otra línea
        /// </summary>
        public bool Intersect(Line2D_new other, out Vector2D intersection)
        {
            double det = A * other.B - other.A * B;
            if (det.IsZero())
            {
                // Líneas paralelas
                intersection = default;
                return false;
            }

            double x = (B * other.C - other.B * C) / det;
            double y = (other.A * C - A * other.C) / det;


         


            intersection = new Vector2D(x, y);
            return true;






        }


        /// <summary>
        /// Devuelve la orientación relativa de tres puntos
        /// </summary>
        //public static 0 Orientation(Point2D p, Point2D q, Point2D r)
        //{
        //    double area = (q.X - p.X) * (r.Y - p.Y) - (q.Y - p.Y) * (r.X - p.X);
        //    if (Math.Abs(area) < 1e-9) return Orientation.Collinear;
        //    return area > 0 ? Orientation.CounterClockwise : Orientation.Clockwise;
        //}

        /// <summary>
        /// Devuelve perpendicular_direction(), es decir,
        /// el vector normalizado que apunta perpendicularmente a esta línea
        /// </summary>
        public Vector2D PerpendicularDirection()
        {
            return -NormalDirection();
        }

        /// <summary>
        /// Mueve la línea una unidad en su dirección normal
        /// </summary>
        public Line2D_new LineAtOne()
        {
            var n = PerpendicularDirection();
            double delta = A * n.X + B * n.Y;
            return new Line2D_new(A, B, C - delta);
        }
        public bool IsVertical =>B.IsZero(); 

        public bool IsHorizontal  =>A.IsZero(); 
        /// <summary>
        /// Mueve la línea según un vector
        /// </summary>
        public Line2D_new Translate(Vector2D v)
        {
            double newC = C - A * v.X - B * v.Y;
            return new Line2D_new(A, B, newC);
        }

        public override string ToString()
        {
            return $"{A:F4}x + {B:F4}y + {C:F4} = 0";
        }
    }

    public class Line2D
    {

       
        public bool Intersection(Rect2D b, out Vector2D start, out Vector2D end)
        {
            Vector2D rp = this.PointOnLine();



            // Caso especial: línea vertical
            if (IsVertical)
            {
                if (rp.X < b.Left || rp.X > b.Right)
                {
                    start = end = Vector2D.NaN;
                    return false;
                }

                start = new Vector2D(rp.X, b.Top);
                end = new Vector2D(rp.X, b.Bottom);
                return true;
            }

            // Caso especial: línea horizontal
            if (IsHorizontal)
            {
                if (rp.Y < b.Bottom || rp.Y > b.Top)
                {
                    start = end = Vector2D.NaN;
                    return false;
                }

                start = new Vector2D(b.Left, rp.Y);
                end = new Vector2D(b.Right, rp.Y);
                return true;
            }

            // Caso general: línea oblicua
            List<Vector2D> intersections = new List<Vector2D>();

            // Intersectar con bordes verticales
            double yLeft = GetY(b.Left);
            if (yLeft <= b.Bottom && yLeft >= b.Top)
                intersections.Add(new Vector2D(b.Left, yLeft));

            double yRight = GetY(b.Right);
            if (yRight <= b.Bottom && yRight >= b.Top)
                intersections.Add(new Vector2D(b.Right, yRight));

            // Intersectar con bordes horizontales
            double xTop = GetX(b.Top);
            if (xTop >= b.Left && xTop <= b.Right)
                intersections.Add(new Vector2D(xTop, b.Top));

            double xBottom = GetX(b.Bottom);
            if (xBottom >= b.Left && xBottom <= b.Right)
                intersections.Add(new Vector2D(xBottom, b.Bottom));

            // También revisar esquinas en caso de coincidencia exacta
            foreach (var corner in b.GetCorners())
            {
                if (has_on(corner))
                    intersections.Add(corner);
            }

            // Si tenemos al menos dos puntos, devolvemos el segmento
            if (intersections.Count >= 2)
            {
                // Eliminar duplicados para evitar puntos repetidos
                var unique = intersections.Distinct().ToList();
                if (unique.Count >= 2)
                {
                    start = unique[0];
                    end = unique[1];
                    return true;
                }
            }

            start = end = Vector2D.NaN;
            return false;

        }



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

             

       


            //var o = this.SideOfOriented(p );

            // Encuentra un punto arbitrario sobre la línea
            if (B.IsZero())
            {
                double x = 0;
                double y = -(A * x + C) / B;
                return (x, y);
            }
            else
            {
                double y = 0;
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
            var r = A * p.X + B * p.Y + C;
            if (r.IsZero()) 
                return 0;
            return Math.Sign( r);
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
            return string.Format("l(a:{0}, b:{1}, b:{2})", A, B, C);
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