
using System.Diagnostics;
using System.Globalization;

namespace CGAL
{
    /*
std.ostream & operator <<(std.ostream& os, const CollapseType a);
    */

    public class CollapseSpec : IComparable<CollapseSpec>
    {
        public static int COUNTER_NT_cmp;

        public readonly int component;
        protected double time_;
        private Halfedge _relevantEdge = null;
        // for all collapses listed in requires_relevant_edge(), such as CONSTRAINT_COLLAPSE


        // for VERTEX_MOVES_OVER_SPOKE
        private double secondary_key_;

        private CollapseType type_;
        /* extra info */

        public EventData EventData { get; private set; }

        public CollapseSpec(EventData eventData )

        {
            EventData = eventData;
            type_ = EventData.Type;
            time_ = eventData.Time;
            component = 0;
            //Debug.Assert(type_ == CollapseType.NEVER);
        }

        public CollapseSpec( CollapseType type, double time)
        {
            type_ = type;
            time_ = time;
          

            Debug.Assert(type_ != CollapseType.Undefined);
            Debug.Assert(type_ != CollapseType.Never);
            Debug.Assert(!requires_relevant_edge(type_));
        }

        public CollapseSpec( CollapseType type, double time, Halfedge relevantEdge)

        {
            


            type_ = type;
            time_ = time;
            _relevantEdge = relevantEdge;
            

            Debug.Assert(requires_relevant_edge(type_));
            Debug.Assert(!requires_relevant_edge_plus_secondary_key(type_));
           

        }

        public CollapseSpec(CollapseType type,double time, Halfedge relevantEdge, double secondary_key)
        {
            type_ = type;
            time_ = time;

            _relevantEdge = relevantEdge ;
            secondary_key_ = secondary_key;

           

            Debug.Assert(requires_relevant_edge(type_));
            Debug.Assert(requires_relevant_edge_plus_secondary_key(type_));

        }

        //public CollapseSpec( EdgeCollapseSpec edge_collapse, Halfedge relevantEdge)

        //{
        //    type_ = edge_collapse.Type == EdgeCollapseType.Future ? CollapseType.ConstraintCollapse :
        //            edge_collapse.Type == EdgeCollapseType.Always ? CollapseType.ConstraintCollapse :
        //                                                             CollapseType.Never;
        //    time_ = type_ == CollapseType.ConstraintCollapse ? edge_collapse.Time : 0.0;

        //    //TODO ver si se necesita que sea null
        //    _relevantEdge = type_ == CollapseType.ConstraintCollapse ? relevantEdge : null;


         

        //    Debug.Assert(edge_collapse.Type == EdgeCollapseType.Future ||
        //           edge_collapse.Type == EdgeCollapseType.Always ||
        //           edge_collapse.Type == EdgeCollapseType.Never ||
        //           edge_collapse.Type == EdgeCollapseType.Past);
            
        //}

        protected CollapseSpec(CollapseSpec other)
        {
            type_ = other.type_;
            time_ = other.time_;
            
            _relevantEdge = other._relevantEdge;
            secondary_key_ = other.secondary_key_;
        }

        public Halfedge RelevantEdge => _relevantEdge;

        // Higher number is more important

        public double Time => time_;

        public CollapseType Type => type_;

        public static bool requires_relevant_edge_plus_secondary_key(CollapseType type)
        {
            return
             /*  type == CollapseType.FaceHasInfinitelyFastVertexWeighted ||*/
               type == CollapseType.VertexMovesOverSpoke ||
               false;
        }

        public bool AllowsRefinementTo(CollapseSpec o)
        {
            Debug.Assert(time_ == o.time_);
            if (type_ == CollapseType.SplitOrFlipRefine)
            {
                if (o.type_ == CollapseType.VertexMovesOverSpoke ||
                    o.type_ == CollapseType.SpokeCollapse)
                {
                    if (RelevantEdge != o.RelevantEdge)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // - for flip events: longest spoke: squared length
        // - for weighted infinite one: higher edge speed (if faster edge wins)
        public void Assign(CollapseSpec other)
        {
            type_ = other.type_;
            time_ = other.time_;
           
            _relevantEdge = other._relevantEdge;
            secondary_key_ = other.secondary_key_;
        }
        public int CompareTo(CollapseSpec? o)
        {
            Debug.Assert(type_ != CollapseType.Undefined);
            Debug.Assert(o.type_ != CollapseType.Undefined);

            if (type_ == CollapseType.Never)
            {
                if (o.type_ == CollapseType.Never)
                {
                    return (int)SignEnum.ZERO;
                }
                else
                {
                    return (int)SignEnum.POSITIVE;
                }
            }
            else if (o.type_ == CollapseType.Never)
            {
                return (int)SignEnum.NEGATIVE;
            }

            if (component < o.component)
            {
                return (int)SignEnum.NEGATIVE;
            }
            else if (component > o.component)
            {
                return (int)SignEnum.POSITIVE;
            }

            var c = compare_NT(time_, o.time_);
            if (c == (int)SignEnum.ZERO)
            {
                if (type_ < o.type_)
                {
                    c = (int)SignEnum.NEGATIVE;
                }
                else if (type_ > o.type_)
                {
                    c = (int)SignEnum.POSITIVE;
                }
                else if (requires_relevant_edge_plus_secondary_key())
                {
                    c = -compare_NT(secondary_key_, o.secondary_key_);
                }
            }
            return c;
        }

        public override bool Equals(object? obj)
        {
            bool result = false;
            if (obj is CollapseSpec c)
                result = CompareTo(c) == 0;
            return result;
        }

        public double get_printable_secondary_key()
        { return secondary_key_; }

        public double get_printable_time()
        { return time_; }

       

        public bool requires_relevant_edge()
        {
            return requires_relevant_edge(type_);
        }

        public bool requires_relevant_edge_plus_secondary_key()
        {
            return requires_relevant_edge_plus_secondary_key(type_);
        }

        [Obsolete]
        public double time()
        { return time_; }

        public override string ToString()
        {
            return $"{time_.ToString("##0.00000000000000000", CultureInfo.InvariantCulture):F3.9} {type_}";
        }

        [Obsolete]
        public CollapseType type()
        { return type_; }

        private static int compare_NT(double a, double b)
        {
            ++COUNTER_NT_cmp;
            return a.CompareTo(b);
        }

        private static bool requires_relevant_edge(CollapseType type)
        {
            return
             //  type == CollapseType.FaceHasInfinitelyFastVertexWeighted ||
               type == CollapseType.ConstraintCollapse ||
               type == CollapseType.SpokeCollapse ||
               type == CollapseType.SplitOrFlipRefine ||
               type == CollapseType.VertexMovesOverSpoke ||
           //    type == CollapseType.CcwVertexLeavesCh ||
               false;
        }
        /* TODO */
        /*
      public CollapseSpec(const CollapseSpec&) = default;
    public CollapseSpec(CollapseSpec &&) = default;

    CollapseSpec & operator = (CollapseSpec&& o)
    {
        Debug.Assert(component == o.component);

        type_ = std.move(o.type_);
        time_ = std.move(o.time_);
        relevant_edge_ = std.move(o.relevant_edge_);
        secondary_key_ = std.move(o.secondary_key_);
        return *this;
    }

    CollapseSpec & operator =(const CollapseSpec& o)
    {
        Debug.Assert(component == o.component);
        type_ = o.type_;
        time_ = o.time_;
        relevant_edge_ = o.relevant_edge_;
        secondary_key_ = o.secondary_key_;
        return *this;
    }
        */
        /*
        CollapseSpec(const CollapseSpec &o)
          : type_(o.type_)
          , time_(o.time_)
        {}
        */
    };
}