using GFL.Kernel;
using System.Diagnostics;

namespace GFL
{
    /*
std.ostream & operator <<(std.ostream& os, const CollapseType a);
    */

    public class CollapseSpec : IComparable<CollapseSpec>
    {
        public static int COUNTER_NT_cmp;

        private CollapseType type_;
        protected double time_;

        /* extra info */

        // for all collapses listed in requires_relevant_edge(), such as CONSTRAINT_COLLAPSE
        private int relevant_edge_ = -1;

        // for VERTEX_MOVES_OVER_SPOKE
        private double secondary_key_; // Higher number is more important

        // - for flip events: longest spoke: squared length
        // - for weighted infinite one: higher edge speed (if faster edge wins)
        public void Assign(CollapseSpec other)
        {
            type_ = other.type_;
            time_ = other.time_;
            relevant_edge_ = other.relevant_edge_;
            secondary_key_ = other.secondary_key_;
        }
        private static bool requires_relevant_edge(CollapseType type)
        {
            return
               type == CollapseType.FACE_HAS_INFINITELY_FAST_VERTEX_WEIGHTED ||
               type == CollapseType.CONSTRAINT_COLLAPSE ||
               type == CollapseType.SPOKE_COLLAPSE ||
               type == CollapseType.SPLIT_OR_FLIP_REFINE ||
               type == CollapseType.VERTEX_MOVES_OVER_SPOKE ||
               type == CollapseType.CCW_VERTEX_LEAVES_CH ||
               false;
        }

        public static bool requires_relevant_edge_plus_secondary_key(CollapseType type)
        {
            return
               type == CollapseType.FACE_HAS_INFINITELY_FAST_VERTEX_WEIGHTED ||
               type == CollapseType.VERTEX_MOVES_OVER_SPOKE ||
               false;
        }

        public readonly int component;

        public bool requires_relevant_edge()
        {
            return requires_relevant_edge(type_);
        }

        public bool requires_relevant_edge_plus_secondary_key()
        {
            return requires_relevant_edge_plus_secondary_key(type_);
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

        protected CollapseSpec(CollapseSpec other)
        {
            type_ = other.type_;
            time_ = other.time_;
            relevant_edge_ = other.relevant_edge_;
            secondary_key_ = other.secondary_key_;
        }

        public CollapseSpec(int p_component, CollapseType type = CollapseType.UNDEFINED)

        {
            type_ = type;
            component = p_component;
            Debug.Assert(type_ == CollapseType.NEVER);
        }

        public CollapseSpec(int p_component, CollapseType type, double time)
        {
            type_ = type;
            time_ = time;
            component = p_component;

            Debug.Assert(type_ != CollapseType.UNDEFINED);
            Debug.Assert(type_ != CollapseType.NEVER);
            Debug.Assert(!requires_relevant_edge(type_));
        }

        public CollapseSpec(int p_component, CollapseType type, double time, int relevant_edge)

        {
            type_ = type;
            time_ = time;
            relevant_edge_ = relevant_edge;

            component = p_component;

            Debug.Assert(requires_relevant_edge(type_));
            Debug.Assert(!requires_relevant_edge_plus_secondary_key(type_));
            Debug.Assert(0 <= relevant_edge_ && relevant_edge_ < 3);
        }

        public CollapseSpec(int p_component,
                        CollapseType type,
                        double time,
                        int relevant_edge,
                        double secondary_key)
        {
            type_ = type;
            time_ = time;
            relevant_edge_ = relevant_edge;
            secondary_key_ = secondary_key;

            component = p_component;

            Debug.Assert(requires_relevant_edge(type_));
            Debug.Assert(requires_relevant_edge_plus_secondary_key(type_));
            Debug.Assert(0 <= relevant_edge_ && relevant_edge_ < 3);
        }

        public CollapseSpec(int p_component,
                         EdgeCollapseSpec edge_collapse,
                         int relevant_edge)

        {
            type_ = edge_collapse.type() == EdgeCollapseType.FUTURE ? CollapseType.CONSTRAINT_COLLAPSE :
                    edge_collapse.type() == EdgeCollapseType.ALWAYS ? CollapseType.CONSTRAINT_COLLAPSE :
                                                                     CollapseType.NEVER;
            time_ = type_ == CollapseType.CONSTRAINT_COLLAPSE ? edge_collapse.time() : Const.CORE_ZERO;
            relevant_edge_ = type_ == CollapseType.CONSTRAINT_COLLAPSE ? relevant_edge : 0;
            component = (p_component);

            Debug.Assert(edge_collapse.type() == EdgeCollapseType.FUTURE ||
                   edge_collapse.type() == EdgeCollapseType.ALWAYS ||
                   edge_collapse.type() == EdgeCollapseType.NEVER ||
                   edge_collapse.type() == EdgeCollapseType.PAST);
            Debug.Assert(0 <= relevant_edge_ && relevant_edge_ < 3);
        }

        /*
        CollapseSpec(const CollapseSpec &o)
          : type_(o.type_)
          , time_(o.time_)
        {}
        */

        public CollapseType type()
        { return type_; }

        public double time()
        { return time_; }

        public double get_printable_time()
        { return time_; }

        public double get_printable_secondary_key()
        { return secondary_key_; }

        public int relevant_edge()
        {
            Debug.Assert(requires_relevant_edge());
            Debug.Assert(0 <= relevant_edge_ && relevant_edge_ < 3);
            return relevant_edge_;
        }

        public bool allows_refinement_to(CollapseSpec o)
        {
            Debug.Assert(time_ == o.time_);
            if (type_ == CollapseType.SPLIT_OR_FLIP_REFINE)
            {
                if (o.type_ == CollapseType.VERTEX_MOVES_OVER_SPOKE ||
                    o.type_ == CollapseType.SPOKE_COLLAPSE)
                {
                    if (relevant_edge_ != o.relevant_edge_)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static int compare_NT(double a, double b)
        {
            ++COUNTER_NT_cmp;
            return a.CompareTo(b);
        }

        public override bool Equals(object? obj)
        {
            bool result = false;
            if (obj is CollapseSpec c)
                result = CompareTo(c) == 0;
            return result;
        }

        public int CompareTo(CollapseSpec? o)
        {
            Debug.Assert(type_ != CollapseType.UNDEFINED);
            Debug.Assert(o.type_ != CollapseType.UNDEFINED);

            if (type_ == CollapseType.NEVER)
            {
                if (o.type_ == CollapseType.NEVER)
                {
                    return (int)ESign.EQUAL;
                }
                else
                {
                    return (int)ESign.LARGER;
                }
            }
            else if (o.type_ == CollapseType.NEVER)
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
    };
}