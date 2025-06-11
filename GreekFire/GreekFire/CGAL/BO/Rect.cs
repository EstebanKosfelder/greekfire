using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace CGAL
{

    public struct Interval
    {
        public double Min, Max;

        public Interval(double value)
        {
            Min = Max = value;
        }

        public Interval(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public Interval(Interval value)
        {
            Min = value.Min;
            Max = value.Max;
        }

        //      public Interval(Linear l) : this(l.Start, l.End) { }
        public Interval(IEnumerable<double> items)
        {
            Min = Double.MaxValue;
            Max = Double.MinValue;
            foreach (var item in items)
            {
                this.Extend(item);
            }
        }

       public IEnumerable<Point2> GetCorners(Point2 min, Point2 max)
        {
            yield return new Point2(min.X, min.Y); // bottom-left
            yield return new Point2(max.X, min.Y); // bottom-right
            yield return new Point2(max.X, max.Y); // top-right
            yield return new Point2(min.X, max.Y); // top-left
        }
        public static Interval Infinite() => new Interval(Double.MinValue, Double.MaxValue);


        public static Interval NaN() => new Interval(Double.MaxValue, Double.MinValue);

        public static Interval Unit() => new Interval(0, 1);

        #region interfaces

        public double From
        {
            get { return Min; }
            set
            {
                if (value >= Max)
                {
                    Debug.Assert(value < Max, "Posible Bug Valor Minimo mayor que el Maximo");
                }
                Min = value;
            }
        }



        public double Middle { get { return (Min + Max) / 2; } set { Max = (value * 2) - Min; } }


        public double Size { get { return Max - Min; } set { Max = Min + value; } }

        public double To
        {
            get { return Max; }
            set
            {
                if (value <= Min)
                {
                    Debug.Assert(value > Min, "Posible Bug Valor Maximo es menor el minimo");
                }
                Max = value;
            }
        }

        public double this[int i]
        {
            get { Debug.Assert(i < 2); return i == 0 ? Min : Max; }
            set
            {
                Debug.Assert(i < 2);
                if (i == 0)
                    Min = value;
                else
                    Max = value;
            }
        }
        #endregion interfaces

        public bool AreNear(Interval b, double eps = Mathex.EPS)
        {
            return this.Min.AreNear(b.Min, eps) && this.Max.AreNear(b.Max, eps);
        }

        public double Clamp(double val)
        {
            if (val < Min) return Min;
            if (val > Max) return Max;
            return val;
        }

        public bool Contains(double val, double eps = Mathex.EPS)
        {
            return (Min.AreNear(val, eps) || Min < val) &&
                (Max.AreNear(val, eps) || val < Max);
        }

        public bool Contains(Interval val, double eps = Mathex.EPS)
        {
            return (Min.AreNear(val.Min, eps) || Min < val.Min) &&
                (Max.AreNear(val.Max, eps) || val.Max < Max);
        }

        public override bool Equals(object obj)
        {
            if (obj is Interval)
            {
                Interval other = (Interval)obj;
                return this.Min.Equals(other.Min) && this.Max.Equals(other.Max);
            }
            return false;
        }

        public void Extend(double val)
        {
            if (IsNaN())
            {
                Min = Max = val;
            }
            else
            {
                if (val < Min)
                    Min = val;
                if (val > Max)
                    Max = val; //no else, as we want to handle NaN
            }
        }

        public void Extend(Interval val)
        {
            Extend(val.Min);
            Extend(val.Max);
        }

        public override int GetHashCode()
        {
            long bits = 1L;
            HashCode value = new HashCode();
            value.Add(Min);
            value.Add(Max);
            return value.GetHashCode();

        }

        public bool InteriorContains(double val, double eps = Mathex.EPS) =>
            Min < val && val < Max && !Min.AreNear(val, eps) && !Max.AreNear(val, eps);

        public bool InteriorContains(Interval val, double eps = Mathex.EPS) =>
            Min < val.Min && val.Max < Max && !Min.AreNear(val.Min) && !Max.AreNear(val.Max, eps);

        public bool InteriorIntersects(Interval val, double eps = Mathex.EPS)
        {
            var mx = Math.Max(Min, val.Min);
            var mi = Math.Min(Max, val.Max);
            return mx < mi && !mx.AreNear(mi, eps);
        }

        public bool Intersects(Interval other, double eps = Mathex.EPS)
        {
            return Contains(other.Min, eps) || Contains(other.Max, eps) || other.Contains(this, eps);
        }

        public void IntersectWith(Interval o)
        {
            if (o.IsFinite() && this.IsFinite())
            {
                double u = Math.Max(Min, o.Min);
                double v = Math.Min(Max, o.Max);
                if (u <= v)
                {
                    this.Min = u;
                    this.Max = v;
                    return;
                }
            }
            this.Min = Double.MaxValue;
            this.Max = Double.MinValue;
        }

        public void Invalidate()
        {
            Min = double.MaxValue;
            Max = double.MinValue;
        }

        public bool IsFinite()
        {
            return this.Min.IsFinite() && this.Max.IsFinite() && Min <= Max;
        }

        public bool IsNaN()
        {
            return (double.IsNaN(this.Min) || double.IsNaN(this.Max) || Min > Max);
        }
        public bool IsSingular()
        { return Min == Max; }

        public bool IsZero(double eps = Mathex.EPS)
        {
            return this.Min.IsZero(eps) && this.Max.IsZero(eps);
        }
        public bool LowerContains(double val, double eps = Mathex.EPS)
        { return (Min <= val || Min.AreNear(val, eps)) && val < Max && !Max.AreNear(val, eps); }

        public bool LowerContains(Interval val, double eps = Mathex.EPS)
        { return (Min <= val.Min || Min.AreNear(val.Min, eps)) && val.Max < Max && !Max.AreNear(val.Max, eps); }

        public void Mapmax(Interval I)
        {
            this.SetEnds(this.ValueAt(I.Min), this.ValueAt(I.Max));
        }

        public Interval MapTo(Interval other)
        {
            Interval r = new Interval(this);
            r.SetEnds(ValueAt(other.Min), ValueAt(other.Max));
            return r;
        }

        public double NearestEnd(double val)
        {
            double dmin = Math.Abs(val - Min), dmax = Math.Abs(val - Max);
            return dmin <= dmax ? Min : Max;
        }

        /// Find closest time in [0,1] that maps to the given value. */
        public double NearestTime(double v)
        {
            if (v <= Min) return 0;
            if (v >= Max) return 1;
            return TimeAt(v);
        }

        public Interval Offset(double value)
        {
            double of = value;
            if (value < 0)
            {
                of = (Size / 2) < -value ? -(Size / 2) : value;
            }
            return new Interval(this.Min - of, this.Max + of);
        }

        public Interval Proyect(Interval other)
        {
            double d = other.Size / Size;
            Interval interval = other * d + this.From;
            if (interval.From.IsZero())
                interval.From = 0;
            if (interval.To.AreNear(1.0))
            {
                interval.To = 1;
            }
            return interval;
        }

        public void Remove(double value)
        {
            if (!IsNaN())
            {
                if (!this.InteriorContains(value))
                { Invalidate(); }
            }
        }

        public void Remove(Interval value)
        {
            if (!IsNaN())
            {
                if (Min.AreNear(value.Min))
                {
                    Invalidate();
                }
                else if (Max.Equals(value.Max))
                {
                    Invalidate();
                }
            }
        }

        public void SetEnds(double a, double b)
        {
            if (a <= b)
            {
                Min = a;
                Max = b;
            }
            else
            {
                Min = b;
                Max = a;
            }
        }

        public void Swap(ref Interval other)
        {
            double aux = this.Min;
            this.Min = other.Min;
            other.Min = aux;

            aux = this.Max;
            this.Max = other.Max;
            other.Max = aux;
        }
        public double TimeAt(double v)
        {
            return (v - Min) / Size;
        }

        public override string ToString()
        {
            return string.Format("[{0:##0.0000}  {1:##0.0000}]", this.Min, this.Max);
        }

        public void UnionWith(Interval a)
        {
            if (a.Min < Min) Min = a.Min;
            if (a.Max > Max) Max = a.Max;
        }
        public bool UpperContains(double val, double eps = Mathex.EPS)
        { return Min < val && !Min.AreNear(val, eps) && (val <= Max || Max.AreNear(val, eps)); }

        public bool UpperContains(Interval val, double eps = Mathex.EPS)
        { return Min < val.Min && !Min.AreNear(val.Min, eps) && (val.Max <= Max || Max.AreNear(val.Max, eps)); }

        public double ValueAt(double time)
        {
            return Min.Proporcional(Max, time);
        }

        /** @brief Compute a time value that maps to the given value.
        * The supplied value does not need to be in the interval for this method to work. */
        #region Operators

        public static Interval operator -(Interval a)
        {
            return new Interval(-a.Max, -a.Min);
        }

        public static Interval operator -(Interval a, double b)
        {
            return new Interval(a.Min - b, a.Max - b);
        }

        public static Interval operator -(Interval a, Interval b)
        {
            return new Interval(a.Min - b.Max, a.Max - b.Min);
        }

        public static Interval operator &(Interval a, Interval b)
        {
            var r = new Interval(a);
            r.IntersectWith(b);
            return r;
        }

        public static Interval operator *(Interval a, double b)
        {
            return new Interval(a.Min * b, a.Max * b);
        }

        public static Interval operator *(Interval a, Interval b)
        {
            Interval r = new Interval();
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 2; j++)
                    r.Extend(a[i] * b[j]);
            return r;
        }

        public static Interval operator /(Interval a, double b)
        {
            return new Interval(a.Min / b, a.Max / b);
        }

        public static Interval operator /(Interval a, Interval b)
        {
            return new Interval(a.Min / b.Min, a.Max / b.Max);
        }

        public static Interval operator |(Interval a, Interval b)
        {
            var r = new Interval(a);
            r.UnionWith(b);
            return r;
        }

        public static Interval operator +(Interval a, double b)
        {
            return new Interval(a.Min + b, a.Max + b);
        }
        public static Interval operator +(Interval a, Interval b)
        {
            return new Interval(a.Min + b.Min, a.Max + b.Max);
        }
        #endregion Operators

        #region Comparable operators

        public static bool operator !=(Interval a, Interval b)
        {
            return !(a == b);
        }

        public static bool operator ==(Interval a, Interval b)
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
        #endregion Comparable operators
    }

    public struct Rect2D  //:IObjectBounds
	{


		public Rect2D Bounds { get { return this; } }

		private Interval[] axis;

		public static Rect2D NaN() { return new Rect2D(Interval.NaN(),Interval.NaN()); }
		public static Rect2D Infinite() { return new Rect2D(Interval.Infinite(),Interval.Infinite()); }
		public static Rect2D FromOriginAndSize(Point2 origin, Point2 size){
			return new Rect2D(origin, origin + size);
		}
		public Rect2D (Interval ix,Interval iy ){
			axis = new Interval[] { ix, iy };
		}
		public Rect2D (Rect2D b):this(b[0],b[1]){}
		public Rect2D (IEnumerable<Rect2D> items){
			axis = new Interval[] {Interval.NaN(),Interval.NaN(), Interval.NaN() };
			foreach (var i in items)
			{
				Extend(i);
			}
		}
		public Rect2D (IEnumerable<Point2> items){
			axis = new Interval[] {Interval.NaN(),Interval.NaN(), Interval.NaN() };
			foreach (var i in items)
			{
				Extend(i);
			}
		}
        /*
		public Rect2D (IEnumerable<IObjectBounds> items){
			axis = new Interval[] {Interval.NaN(),Interval.NaN(), Interval.NaN() };
			foreach (var i in items)
			{
				Extend(i.Bounds);
			}
		}
        */

        public IEnumerable<Point2> GetCorners()
        {
            yield return new Point2(Left, Top); // bottom-left
            yield return new Point2(Right, Top); // bottom-right
            yield return new Point2(Right, Bottom); // top-right
            yield return new Point2(Right, Bottom); // top-left
        }
        public Rect2D(Point2 value):this ( new Interval(value.X),new Interval(value.Y)){}
		public Rect2D(Point2 min, Point2 max){
			axis = new Interval[] {Interval.NaN(),Interval.NaN()};
			Extend(min);
			Extend(max);
			//this ( new Interval(min[0],max[0]),new Interval(min[1],max[1]),new Interval(min[2],max[2])){}
		}
		public static implicit operator Rect2D(Point2 p){
			return new Rect2D(p);
		}


		public Interval this [int index] { get { return axis[index]; } set { axis[index] = value; } }
		public Point2 Min
		{
			get
			{
				return new Point2(axis[0].Min,axis[1].Min);
			}
			set
			{
				for (int i = 0; i < 2; i++)
					axis[i].Min = value[i];
			}
		}
		public Point2 Max
		{
			get
			{
				return new Point2(axis[0].Max,axis[1].Max);
			}
			set
			{
				for (int i = 0; i < 2; i++)
					axis[i].Max = value[i];
			}
		}
		public void Invalidate()
		{
			for( int i=0;i<2;i++)
				axis[i] =Interval.NaN();
		}

		public bool IsNaN() {  return axis[0].IsNaN() || axis[1].IsNaN() ;  }

		public Point2 Center { get { return (Size / 2) + Min; } }
		public Point2 Size { get { return (Max - Min); } }
		public double Height { get { return Size.Y ; } }
		public double Width { get { return Size.X ; } }
		

		public double Left { get { return axis[0].Min; } set { axis[0].Min = value; } }
		public double Top { get { return axis[1].Min; } set { axis[1].Min = value; } }
	
		public double Right { get { return axis[0].Max; } set { axis[0].Max = value; } }
		public double Bottom { get { return axis[1].Max; } set { axis[1].Max = value; } }
	

		public double IntersectArea { get { return Math.Max(0, Width) * Math.Max(0, Height); } }
		public double Area { get { return (Width) * (Height); } }
		public double Margin { get { return (Width) + (Height); } }

		public Point2 Clamp(Point2 p){
			return new Point2 (axis [0].Clamp (p.X), axis [1].Clamp (p.Y));
		}

        /*
		public void Extend(IEnumerable<IObjectBounds> items)
		{
			foreach (var o in items){
				Extend(o.Bounds);
			}
		}
        */
		public void Extend(IEnumerable<Point2> items)
		{
			foreach (var o in items){
				Extend(o);
			}
		}
		public void Extend(IEnumerable<Rect2D> items)
		{
			foreach (var o in items){
				Extend(o);
			}
		}
		public void Extend(Rect2D o)
		{
			
			for (int i = 0; i < 2; i++)
				axis[i].Extend(o[i]);
		}
        public void Extend(Point2 o)
        {

            for (int i = 0; i < 2; i++)
                axis[i].Extend(o[i]);
        }
        public IEnumerable<Point2> Extrems(){


			for ( int x = 0 ; x < 2; x++){
				for ( int y = 0 ; y <2; y++){
					
						yield return new Point2 (axis [0] [x], axis [1] [y]);
					
				}
			}
		}


		public Rect2D Offset(double value)
		{
			Rect2D r = new Rect2D(this);

			for (int i = 0; i <2; i++)
				r[i]=r[i].Offset(value);
			return r;
		}
		public Rect2D Offset(Point2 value)
		{

			var result = new Rect2D ();
			result.axis = new Interval[2];

            for ( int i=0;i<2;i++)
			 result.axis[i]=	axis[i].Offset(value[i]);
			return result;

		}


		public void Remove(IEnumerable<Rect2D> items)
		{
			foreach (var o in items){
				Remove(o);
				if (IsNaN() )
					break;
			}
		}

		public void Remove(Point2 p){
			if (IsNaN())
				return;
			for( int i=0;i<2;i++)
				axis[i].Remove(p[i]);
			if (IsNaN())
				Invalidate();
		}

		public void Remove(Rect2D r)
		{
			if (IsNaN())
				return;
			for( int i=0;i<2;i++)
				axis[i].Remove(r[i]);
			if (IsNaN())
				Invalidate();



		}
        /*
		public bool Intersects(IObjectBounds other, double eps = Mathex.EPS){
			return Intersects(other.Bounds,eps);
		}
        */

        public bool Intersects(Rect2D other,double eps = Mathex.EPS )
		{
			if (IsNaN() || other.IsNaN())
				throw new Exception("NaN Bounds");
			for (int i = 0; i <2; i++)
			{
				if (! axis[i].Intersects(other[i],eps))
					return false;
			}
			return true;
		}
        /*
		public bool Contains(IObjectBounds other, double eps = Mathex.EPS){
			return Contains(other.Bounds,eps);
		}
        */
		public bool Contains(Rect2D other, double eps = Mathex.EPS)
		{
			if (IsNaN() || other.IsNaN())
				throw new Exception("NaN Bounds");
			for (int i = 0; i < 2; i++)
			{
				if ( !axis[i].Contains(other[i], eps)) return false;
			}
			return true;

		}
		public bool Contains(Point2 point, double eps = Mathex.EPS)
		{
			if (IsNaN() || point.IsNaN)
				throw new Exception("NaN Bounds");
			for (int i = 0; i <2; i++)
			{
				if ( !axis[i].Contains(point[i], eps)) return false;
			}
			return true;
		}

		public bool InteriorContains(Rect2D other, double eps = Mathex.EPS)
		{
			if (IsNaN() || other.IsNaN())
				throw new Exception("NaN Bounds");
			for (int i = 0; i < 2; i++)
			{
				if ( !axis[i].InteriorContains(other[i], eps)) return false;
			}
			return true;

		}

		public void UnionWith(Rect2D other)
		{


			for (int i = 0; i < 2; i++)
			{
				axis[i].UnionWith(other[i]);
			}



		}

		public void IntersectWith(Rect2D other)
		{

			for (int i = 0; i <2; i++)
			{
				axis[i].IntersectWith(other[i]);
			}

		}

		public double IntersectAreaXY()  { return Math.Max(0, Width) * Math.Max(0, Height); } 
		public double AreaXY()  { return (Width) * (Height); } 
		public double MarginXY()  { return (Width) + (Height); } 



		public override bool Equals(object obj){

			if (obj is Rect2D)
			{
				Rect2D  b = (Rect2D)obj;
				for( int i=0;i<2;i++)
					if ( !axis[i].Equals(b[i])) return false;
				return true;
			}
			return false;
		}




		public double MaxRadiusXY()
		{
			return (Math.Max(Height, Width) * 0.7071);
		}
		public double MaxRadius()
		{
			return (Math.Max(Size.X,Size.Y) * 0.7071);
		}


		#region Operadores

		#region == != Operator
		public static bool operator != (Rect2D A, Point2 B)
		{
			return !A.Equals (B);
		}

		public static bool operator == (Rect2D A, Point2 B)
		{
			return A.Equals (B);
		}
		#endregion

		#region + Operator

		//public static Rect2D operator + (Rect2D A, Point2 B)
		//{
		//	return new Rect2D (A.Min + B, A.Max + B);
		//}

		//public static Rect2D operator + (Rect2D A, double B)
		//{
		//	return new Rect2D (A.Min + B, A.Max + B);
		//}


		#endregion

		#region - Operator

		//public static Rect2D operator - (Rect2D A, Point2 B)
		//{
		//	return new Rect2D (A.Min - B, A.Max - B);
		//}

		//public static Rect2D operator - (Rect2D A, double B)
		//{
		//	return new Rect2D (A.Min - B, A.Max - B);
		//}


		#endregion

		#region * Operator
		//public static Rect2D operator * (Rect2D A, Point2 B)
		//{
		//	return new Rect2D (A.Min * B, A.Max * B);
		//}

		//public static Rect2D operator * (Rect2D A, double B)
		//{
		//	return new Rect2D (A.Min * B, A.Max * B);
		//}


		//public static Rect2D operator * (Rect2D A, Matrix m){
		//	return new Rect2D (A.Extrems().Select (p => p * m));
		//}
		#endregion
		public static Rect2D operator | (Rect2D A, Rect2D B)
		{
			Rect2D R = new Rect2D(A);
			R.UnionWith(B);
			return R;
		}
        public static Rect2D operator &(Rect2D A, Rect2D B)
        {
            Rect2D R = new Rect2D(A);
            R.IntersectWith(B);
            return R;
        }

       



		#endregion



		public override int GetHashCode()
		{

            HashCode hashCode = new HashCode();
			for( int i=0;i<2;i++)
                hashCode.Add(axis[i].GetHashCode());
			return hashCode.ToHashCode();

		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[");
			for (int i = 0; i <2; i++)
				sb.Append(axis[i].ToString());
			sb.Append("]");
			return sb.ToString();
		}

	}
}

