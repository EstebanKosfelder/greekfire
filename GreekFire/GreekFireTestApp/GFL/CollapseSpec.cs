using GFL.Kernel;
using System.Diagnostics;
using System.Globalization;

namespace GFL
{
    /*
std.ostream & operator <<(std.ostream& os, const CollapseType a);
    */

    public class CollapseSpec : IComparable<CollapseSpec>
    {
        public static int COUNTER_NT_cmp;

        public readonly int component;
        protected double time_;
        private KineticHalfedge _relevantEdge = null;
        // for all collapses listed in requires_relevant_edge(), such as CONSTRAINT_COLLAPSE


        // for VERTEX_MOVES_OVER_SPOKE
        private double secondary_key_;

        private CollapseType type_;
        /* extra info */
        public CollapseSpec(int p_component, CollapseType type = CollapseType.Undefined)

        {
            type_ = type;
            component = p_component;
            //Debug.Assert(type_ == CollapseType.NEVER);
        }

        public CollapseSpec(int p_component, CollapseType type, double time)
        {
            type_ = type;
            time_ = time;
            component = p_component;

            Debug.Assert(type_ != CollapseType.Undefined);
            Debug.Assert(type_ != CollapseType.Never);
            Debug.Assert(!requires_relevant_edge(type_));
        }

        public CollapseSpec(int p_component, CollapseType type, double time, KineticHalfedge relevantEdge)

        {
            


            type_ = type;
            time_ = time;
            _relevantEdge = relevantEdge;
            component = p_component;

            Debug.Assert(requires_relevant_edge(type_));
            Debug.Assert(!requires_relevant_edge_plus_secondary_key(type_));
           

        }

        public CollapseSpec(int p_component,CollapseType type,double time, KineticHalfedge relevantEdge, double secondary_key)
        {
            type_ = type;
            time_ = time;

            _relevantEdge = relevantEdge ;
            secondary_key_ = secondary_key;

            component = p_component;

            Debug.Assert(requires_relevant_edge(type_));
            Debug.Assert(requires_relevant_edge_plus_secondary_key(type_));

        }

        public CollapseSpec(int p_component, EdgeCollapseSpec edge_collapse, KineticHalfedge relevantEdge)

        {
            type_ = edge_collapse.Type == EdgeCollapseType.Future ? CollapseType.ConstraintCollapse :
                    edge_collapse.Type == EdgeCollapseType.Always ? CollapseType.ConstraintCollapse :
                                                                     CollapseType.Never;
            time_ = type_ == CollapseType.ConstraintCollapse ? edge_collapse.Time : Const.CORE_ZERO;

            //TODO ver si se necesita que sea null
            _relevantEdge = type_ == CollapseType.ConstraintCollapse ? relevantEdge : null;


            component = (p_component);

            Debug.Assert(edge_collapse.Type == EdgeCollapseType.Future ||
                   edge_collapse.Type == EdgeCollapseType.Always ||
                   edge_collapse.Type == EdgeCollapseType.Never ||
                   edge_collapse.Type == EdgeCollapseType.Past);
            
        }

        protected CollapseSpec(CollapseSpec other)
        {
            type_ = other.type_;
            time_ = other.time_;
            
            _relevantEdge = other._relevantEdge;
            secondary_key_ = other.secondary_key_;
        }

        public KineticHalfedge RelevantEdge => _relevantEdge;

        // Higher number is more important

        public double Time => time_;

        public CollapseType Type => type_;

        public static bool requires_relevant_edge_plus_secondary_key(CollapseType type)
        {
            return
               type == CollapseType.FaceHasInfinitelyFastVertexWeighted ||
               type == CollapseType.VertexMovesOverSpoke ||
               false;
        }

        public bool allows_refinement_to(CollapseSpec o)
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
                    return (int)ESign.EQUAL;
                }
                else
                {
                    return (int)ESign.LARGER;
                }
            }
            else if (o.type_ == CollapseType.Never)
            {
                return (int)ESign.SMALLER;
            }

            if (component < o.component)
            {
                return (int)ESign.SMALLER;
            }
            else if (component > o.component)
            {
                return (int)ESign.LARGER;
            }

            var c = compare_NT(time_, o.time_);
            if (c == (int)ESign.EQUAL)
            {
                if (type_ < o.type_)
                {
                    c = (int)ESign.SMALLER;
                }
                else if (type_ > o.type_)
                {
                    c = (int)ESign.LARGER;
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
               type == CollapseType.FaceHasInfinitelyFastVertexWeighted ||
               type == CollapseType.ConstraintCollapse ||
               type == CollapseType.SpokeCollapse ||
               type == CollapseType.SplitOrFlipRefine ||
               type == CollapseType.VertexMovesOverSpoke ||
               type == CollapseType.CcwVertexLeavesCh ||
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