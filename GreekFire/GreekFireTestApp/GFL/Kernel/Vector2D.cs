using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFL.Kernel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.Json.Serialization;


    //using Double=System.Int64;
    using Double = System.Double;


   
        public struct Vector2D

        {
        public static Double NanValue = Double.NaN;

        [JsonInclude]
        public Double X { get; set; }

        [JsonInclude]
        public Double Y { get; set; }

        public Vector2D(Double x = 0, Double y = 0)
        {
            this.X = x;
            this.Y = y;
        }

        public Vector2D(IEnumerable<double> values)
                : this()
        {
            int i = 0;
            foreach (var value in values)
            {
                this[i] = value;

                if (i == 2)
                    break;
                i++;
            }
        }

        public static Vector2D NaN => new Vector2D(Double.NaN, Double.NaN);

        public static Vector2D ORIGIN => new Vector2D(1, 1);

        public static Vector2D Zero => new Vector2D(0, 0);

        public static Vector2D ZERO => new Vector2D(0, 0);

        [JsonIgnore]
        public Vector2D CCW => new Vector2D(-this.Y, this.X);

        [JsonIgnore]
        public Vector2D CW => new Vector2D(this.Y, -this.X);

        [JsonIgnore]
        public bool IsNaN => double.IsNaN(X) || double.IsNaN(Y);
        [JsonIgnore]
        public Vector2D N => this / this.Len();

        [JsonIgnore]
        public int Sign
        {
            get
            {
                return Math.Sign(X) * Math.Sign(Y);
            }
        }

        [JsonIgnore]
        public Double this[int index]
        {
            get
            {
                if (index < 0 || index > 1)
                {
                    throw new IndexOutOfRangeException();
                }

                return (index == 0) ? X : Y;
            }
            set
            {
                if (index < 0 || index > 2)
                {
                    throw new IndexOutOfRangeException();
                }
                if (index == 0)
                    X = value;
                else
                    Y = value;
            }
        }

        public static bool Collineal(Vector2D p, Vector2D q, Vector2D r, double eps = Const.EPSILON)
        {
            Vector2D qp = q - p;
            Vector2D rp = r - p;

            return (qp.X * rp.Y).AreNear(rp.X * qp.Y, eps);
        }

        public static Vector2D Identity(int dim)
        {
            var p = new Vector2D();
            p[dim] = 1;
            return p;
        }

        public static implicit operator Vector2D(in (double x, double y) value) => new Vector2D(value.x, value.y);
        public static Vector2D Lli(Vector2D p1, Vector2D p2, Vector2D p3, Vector2D p4)
        {
            var n = (p3 - p4) * p1.CrossXY(p2) - (p1 - p2) * p3.CrossXY(p4);
            var d = (p1 - p2).CrossXY(p3 - p4);
            if (d == 0)
            {
                return Vector2D.NaN;
            }
            return n / d;
        }

        public static Vector2D Max(Vector2D a, Vector2D b)
        {
            if (a.IsNaN)
                return b;
            if (b.IsNaN)
                return a;
            return new Vector2D(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        }

        public static Vector2D Min(Vector2D a, Vector2D b)
        {
            if (a.IsNaN)
                return b;
            if (b.IsNaN)
                return a;
            return new Vector2D(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
        }

        public static Vector2D operator -(Vector2D A)
        {
            if (A.IsNaN)
            {
                return Vector2D.NaN;
            }
            return new Vector2D(-A.X, -A.Y);
        }

        public static Vector2D operator -(Vector2D A, Vector2D B)
        {
            if (A.IsNaN || B.IsNaN)
            {
                return Vector2D.NaN;
            }
            return new Vector2D(A.X - B.X, A.Y - B.Y);
        }

        public static Vector2D operator -(Vector2D A, Double B)
        {
            if (A.IsNaN)
            {
                return Vector2D.NaN;
            }
            return new Vector2D(A.X - B, A.Y - B);
        }

        public static bool operator !=(Vector2D a, Vector2D b)
        {
            return !(a == b);
        }

        public static Vector2D operator *(Vector2D A, Vector2D B)
        {
            if (A.IsNaN || B.IsNaN)
            {
                return Vector2D.NaN;
            }
            return new Vector2D(A.X * B.X, A.Y * B.Y);
        }

        public static Vector2D operator *(Vector2D A, Double B)
        {
            if (A.IsNaN)
            {
                return Vector2D.NaN;
            }
            return new Vector2D(A.X * B, A.Y * B);
        }

        public static Vector2D operator /(Vector2D A, Vector2D B)
        {
            return new Vector2D(A.X / B.X, A.Y / B.Y);
        }

        public static Vector2D operator /(Vector2D A, Double B)
        {
            return new Vector2D(A.X / B, A.Y / B);
        }

        public static Vector2D operator +(Vector2D A, Vector2D B)
        {
            if (A.IsNaN || B.IsNaN)
            {
                return Vector2D.NaN;
            }
            return new Vector2D(A.X + B.X, A.Y + B.Y);
        }

        public static Vector2D operator +(Vector2D A, Double B)
        {
            if (A.IsNaN)
            {
                return Vector2D.NaN;
            }
            return new Vector2D(A.X + B, A.Y + B);
        }

        public static bool operator ==(Vector2D a, Vector2D b)
        {
            return a.Equals(b);
        }

        public static ESign Orientation(Vector2D p, Vector2D q, Vector2D r, double eps = Const.EPSILON)
        {
            //return orientationC2(p.x(), p.y(), q.x(), q.y(), r.x(), r.y());
            Vector2D qp = q - p;
            Vector2D rp = r - p;
            double a = (qp.X * rp.Y);
            double b = (rp.X * qp.Y);

            if (a.AreNear(b, eps))
            {
                return ESign.Zero;
            }

            return (ESign)(a).CompareTo(b);
        }

        public static ESign Orientation(Vector2D p, Vector2D q)
        {
            //return orientationC2(p.x(), p.y(), q.x(), q.y(), r.x(), r.y());

            int result = (p.X * q.Y).CompareTo(q.X * p.Y);

            return (ESign)result;
        }

        public static double signedArea(Vector2D p0, Vector2D p1, Vector2D p2)
        {
            Vector2D d0 = (p0 - p2);
            Vector2D d1 = (p1 - p2);
            return d0.X * d1.Y - d1.X * d0.Y;
        }

        public Double Angle()
        { return Math.Atan2(this.Y, this.X); }

        public double Angle(Vector2D v1, Vector2D v2)
        {
            var d1 = v1 - this;
            var d2 = v2 - this;
            return Math.Atan2(d1.Cross(d2), d1.Dot(d2));
        }

        public Double angle_between(Vector2D b)
        {
            return Math.Atan2(this.CrossXY(b), this.DotXY(b));
        }

        public String AngleDebug(Vector2D v1, Vector2D v2)
        {
            var d1 = v1 - this;
            var d2 = v2 - this;
            return string.Format(" {0:0.000} {1:0.000}", d1.Cross(d2), d1.Dot(d2));
        }

        public bool AreNear(Vector2D p, double eps = Const.EPSILON)
        {
            return X.AreNear(p.X, eps) && Y.AreNear(p.Y, eps);
        }

        public int CompareXY(Vector2D other, double eps = Const.EPSILON)
        {
            if (this.X.AreNear(other.X, eps))
            {
                if (this.Y.AreNear(other.Y, eps))
                {
                    return 0;
                }
                else return (this.Y.CompareTo(other.Y));
            }
            else return this.X.CompareTo(other.X);
        }

        public int CompareYX(Vector2D other, double eps = Const.EPSILON)
        {
            if (this.Y.AreNear(other.Y, eps))
            {
                if (this.X.AreNear(other.X, eps))
                {
                    return 0;
                }
                else return (this.X.CompareTo(other.X));
            }
            else return this.Y.CompareTo(other.Y);
        }

        public Double Cross(Vector2D other)
        {
            // cross = sin(a) * this.Len * other.Len
            return this.X * other.Y - this.Y * other.X;
        }

        public Double CrossXY(Vector2D other)
        {
            // cross = sin(a) * this.Len * other.Len
            return this.X * other.Y - this.Y * other.X;
        }

        public void Deconstruct(out double x, out double y)
        { x = X; y = Y; }
            /*
            public static Vector2D operator *(Vector2D a, Matrix m)
            {
                Vector2D r = new Vector2D();

                for (int i = 0; i < 3; i++)
                {
                    r[i] = (a * m.Col(i)).Sum();
                }
                r += m.Translation;
                return r;
            }

            public static Vector2D operator /(Vector2D a, Matrix m)
            {
                return a * m.Inverse();
            }
            */
            public Double Dot(Vector2D other)
            {
                // dot = cos(a) * this.Len * other.Len
                return this.X * other.X + this.Y * other.Y;
            }

            public Vector2D DotCross(Vector2D v1, Vector2D v2)
            {
                var d1 = v1 - this;
                var d2 = v2 - this;
                return new Vector2D(d1.Dot(d2), d1.Cross(d2));
            }

            public Double DotXY(Vector2D other)
            {
                // dot = cos(a) * this.Len * other.Len
                return this.X * other.X + this.Y * other.Y;
            }

            public override readonly bool Equals(Object obj)
            {
                if (obj is Vector2D other) return X.Equals(other.X) && Y.Equals(other.Y);
                return false;
            }

            public IEnumerator<double> GetEnumerator()
            {
                var a = new double[] { X, Y };
                return a.AsEnumerable().GetEnumerator();
            }

         

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public Vector2D Inverse()
            {
                return new Vector2D(X.IsZero() ? X : 1 / X, Y.IsZero() ? Y : 1 / Y);
            }

            public AnglePosition IsBettweenTwoAngles(Vector2D firstA, Vector2D middleA, Vector2D lastA, double eps = Const.EPSILON)
            {
                AnglePosition result = AnglePosition.OutSide;

                var centerPoint = this;

                var last = centerPoint.DotCross(firstA, lastA);
                var middle = centerPoint.DotCross(firstA, middleA);

                if (middle.X.IsZero(eps)) middle.X = 0.0d;
                if (middle.Y.IsZero(eps)) middle.Y = 0.0d;

                if (middle.Y == 0.0d && middle.X >= 0)
                {
                    if (last.X.IsZero(eps)) last.X = 0.0d;
                    if (last.Y.IsZero(eps)) last.Y = 0.0d;

                    //middle is  0º

                    result = result | AnglePosition.Inside;
                    result = result | AnglePosition.OnFirst;
                    if (last.Y == 0.0d && (last.X >= 0))
                    {
                        result = result | AnglePosition.OnLast;
                    }
                }
                else
                {
                    // middle is not First Angle , middle != 0º

                    if (last.X.IsZero(eps)) last.X = 0.0d;
                    if (last.Y.IsZero(eps)) last.Y = 0.0d;

                    int sgCosLast = Math.Sign(last.X);

                    int sgSinLast = Math.Sign(last.Y);
                    int sgCosMiddle = Math.Sign(middle.X);
                    int sgSinMiddle = Math.Sign(middle.Y);

                    if (sgCosLast != sgCosMiddle || sgSinLast != sgSinMiddle)
                    {
                        //   middle and last are not in the same cuadrant

                        if (sgSinMiddle != sgSinLast)
                        {
                            // the Sin are diferent
                            //    sin  =0  0º | 180º
                            //    sin =1  >0º <180º
                            //    sin =-1  >180º <360º

                            //    SinM  SinCos  R
                            //    0      1      0
                            //    0     -1      1
                            //    1      0      1
                            //    1     -1      1
                            //   -1      0      0
                            //    1      0      1

                            if (sgSinLast == -1 || sgSinMiddle == 1)
                            {
                                result = result | AnglePosition.Inside;
                            }
                        }
                        else
                        {
                            //the 2 angles are on  0º<.180º | 180º<-.360º
                            // sCy == sBy && sCx !=sBx

                            int sM = -sgCosMiddle * sgSinMiddle;
                            int sL = -sgCosLast * sgSinLast;
                            if (sM < sL)
                            {
                                result = result | AnglePosition.Inside;
                            }
                        }
                    }
                    else
                    {
                        // sBx == sCx || sBy == sCy
                        if (sgCosLast == 0)
                        {
                            result = result | AnglePosition.Inside;
                            result = result | AnglePosition.OnLast;
                        }
                        else
                        {
                            double tanLast = last.Y / last.X;
                            double tanMiddle = middle.Y / middle.X;

                            if (tanMiddle.AreNear(tanLast, eps))
                            {
                                result = result | AnglePosition.Inside;
                                result = result | AnglePosition.OnLast;
                            }
                            else if (tanMiddle.CompareTo(tanLast) < 0)
                            {
                                result = result | AnglePosition.Inside; ;
                            }
                        }
                    }
                }
                return result;
            }

            public bool IsBetween(Vector2D left, Vector2D right)
            {
                if (left.AreNear(right) || left.AreNear(this) || right.AreNear(this))
                    return false;
                else if (!left.X.AreNear(left.X))
                    return (this.X > left.X) == (this.X < right.X);
                else
                    return (this.Y > left.Y) == (this.Y < right.Y);
            }

            public bool IsFinite()
            {
                return !IsNaN && X.IsFinite() && Y.IsFinite();
            }

            public bool IsIdentity(int axis, double eps = Const.EPSILON)
            {
                return ((axis == 0 && X.AreNear(1.0d, eps)) || (axis != 0 && X.IsZero(eps))) &&
                    ((axis == 1 && Y.AreNear(1.0d, eps)) || (axis != 1 && Y.IsZero(eps)));
            }

            public bool IsZero(double eps = Const.EPSILON)
            {
                return X.IsZero(eps) && Y.IsZero(eps);
            }

            public Double L2()
            { return this.X * this.X + this.Y * this.Y; }

            public Double L2_2D()
            { return this.X * this.X + this.Y * this.Y; }

            public Double Len()
            { return Math.Sqrt(this.L2()); }

        public Double Len_2D()
        { return Math.Sqrt(this.L2_2D()); }

        public Vector2D Lerp(Vector2D other, double time)
        {
            return (other - this) * time + this; // is de same (1 - time) * from + time * to;
        }

        public Vector2D Lerp(double time)
        {
            return (this) * time;
        }

        public Vector2D Max(Vector2D other)
        {
            return Max(this, other);
        }

        public double Max()
        {
            return Math.Max(X, Y);
        }

        public Vector2D Middle(Vector2D p) => (this + p) / 2d;

        public Vector2D Min(Vector2D other)
        {
            return Min(this, other);
        }

        public double Normal() => Len();
        public Vector2D Perpendicular(ESign o)
        {
            if (o == ESign.Zero) throw new ArgumentOutOfRangeException(nameof(o), "No puede ser Zero");
            if (o == ESign.COUNTERCLOCKWISE)
                return new Vector2D(-Y, X);
            else
                return new Vector2D(Y, -X);
        }

        public Double Proporcional(Vector2D other)
        {
            return (other.X != 0 && this.X != 0) ? other.X / this.X : (other.Y != 0) ? other.Y / this.Y : 0d;
        }

        public double Time(Vector2D other)
        {
            return this.Dot(other) / this.L2_2D();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "(x:{0:###0.000000000;-##0.000000000}, y:{1:###0.000000000;-##0.000000000})", X, Y);
        }

        public Vector2D Unit() => N;
    }
    }
