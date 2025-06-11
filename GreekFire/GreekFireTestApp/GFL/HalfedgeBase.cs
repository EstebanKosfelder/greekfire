



using System;
using System.Diagnostics;


namespace GFL
{
    using GFL.Kernel;

    public enum SignEnum : int
    {
        POSITIVE = 1,
        NEGATIVE = -1,
        ZERO = 0
    }


    
    
    public class HalfedgeBase:IHalfedge {





       public  Line2D wavefront_support_lines { get; set; }

        public HalfedgeBase(int aID , SignEnum aSlope = SignEnum.ZERO)
        {
            ID = aID;
            Face = null;
            Vertex_ = null;
            Slope = aSlope;
            Opposite_ = null;
            Next_ = null;
            Prev_ = null;
        }
        public override string ToString()
        {
            return $"H{ID} F{Face?.ID} {Vertex_}"; 
        }
        IHalfedge opposite_;
        private IVertex vertex_;

        public  IHalfedge Opposite_ { 
            get=>opposite_ ;
            set {

               if(value != null && Vertex_.ID == value.Vertex_.ID)
                {
                    Console.WriteLine("Catapum");
                }
                opposite_ = value ;
            } 
        }
       
        
       
        public IHalfedge Next_ { get; set; }

        public SignEnum Slope { get; set; }
        public IHalfedge Prev_ { get; set; }
        public IVertex Vertex_
        {
            get => vertex_; set
            {
                if (value != null && 36 == value.ID)
                {
                    Console.WriteLine("Catapum");
                }
              
                vertex_ = value;
            }
        
        }
        public IFace Face { get; set; }


        public Vector2D Vector
        {
            get
            {
                Debug.Assert(Opposite_!=null);
                Debug.Assert(Opposite_.Vertex_ != null);
                Debug.Assert(Vertex_ != null);

                Vector2D s = Opposite_.Vertex_.Point;
                Vector2D t = Vertex_.Point;

                return (t - s);
            }
        }




      
        public List<Vertex> LAVList { get; private set; } = new List<Vertex>();


      

        //public Direction2 Direction
        //{
        //    get
        //    {

        //        return new Direction2(Vector);
        //    }
        //}
        public int ID { get; internal set; }


        public bool IsBorder =>  throw new NotImplementedException();// { get => (this.Face == null ||/* this.Face.Outside!=-1*/ ) || Face.Halfedge == this; }
      //  public virtual bool IsBisector => !this.HasIsBorder;
     //   public bool IsInnerBisector =>  !this.Vertex.IsContour() && !this.Opposite.Vertex.IsContour();
   //     public bool HasNullSegment => this.Vertex.has_null_point() || this.Opposite.Vertex.has_null_point();

        /*
        public Segment2 Segment => new Segment2(Vertex.Point, Opposite.Vertex.Point);
        */
        public double Weight { get; internal set; } = 1;

        [Obsolete]
        public int id() => ID;

        [Obsolete]
        internal bool is_border() => IsBorder;
      //  [Obsolete]
    //    internal bool is_inner_bisector() => IsInnerBisector;
        //[Obsolete]
        //internal bool has_null_segment => HasInfiniteTime;
        //[Obsolete]
        //internal bool is_bisector() => IsBisector;

        
        //public string ID
        //{
        //    get
        //    {
        //        string r = "";
        //        if (IsBorder)
        //        {
        //            r = (ID % 2 == 1) ? "C" : "O";
        //        } else
        //        {
        //            if (Slope == SignEnum.POSITIVE)
        //            {
        //                r = IsInnerBisector ? "r" : "R";
        //            }
        //            if (Slope == SignEnum.ZERO)
        //            {
        //                r = IsInnerBisector ? "l" : "L";
        //            }
        //            else
        //            {
        //                r = "?";
        //            }

        //        }
        //        r += String.Format("{0}", Face != Face.NULL ? Face.ID.ToString() : IsBorder ? Opposite.Face.ID.ToString() : "-");
        //        return r;
        //    }
        //}







     //   public bool HasInfiniteTime => this.Vertex.has_infinite_time();

      

        //private Vector2? normal { get; set; }

        //public Vector2 Normal { get; internal set; }

        //public Vector2 GetNormal()
        //{
        //    if (normal != null) return normal.Value;
        //    if (!this.IsValid) throw new IndeterminateValueException($"H{Id}- is null)");
        //    if (!this.Opposite.IsValid) throw new IndeterminateValueException($"H{Id}- Opposite is null)");

        //    if (HasInfiniteTime )
        //        throw new IndeterminateValueException($"H{Id}-{this} infinite ");

        //    if ( Opposite.HasInfiniteTime)
        //        throw new IndeterminateValueException($"H{Id}-{this} oposite infinite");

        //    if ( Opposite.normal!=null)
        //    {
        //        return -Opposite.Normal;
        //    }

        //    normal = (Opposite.Vertex.Point - Vertex.Point).GetNormal();
        //    return normal.Value;
        //}

        //[Obsolete]
        //internal Halfedge defining_contour_edge() { return this.Face.halfedge(); }
        //[Obsolete]
        //internal SignEnum slope() { return Slope; }
        //[Obsolete]
        //internal void set_opposite(Halfedge h) { Opposite = h; }
        //[Obsolete]
        //internal void set_next(Halfedge h) { Next = h; }
        //[Obsolete]
        //internal void set_prev(Halfedge h) { Prev = h; }
        //[Obsolete]
        //public void set_vertex(Vertex w) {
        //    Vertex = w;
        //}
        //[Obsolete]
        //internal void set_face(Face g) { Face = g; }
        //[Obsolete]
        //internal void set_slope(SignEnum aSlope) { Slope = aSlope; }
        //[Obsolete]
        //internal void reset_id(int aID) { ID = aID; }
        //[Obsolete]
        //internal void set_weight(double lWeight)
        //{
        //    Weight =lWeight   ;
        //}

      
        
    }

    public static partial class GreekFireUtils
    {
        public static bool IsValid(IHalfedge h) => h != null ;
    }
}

