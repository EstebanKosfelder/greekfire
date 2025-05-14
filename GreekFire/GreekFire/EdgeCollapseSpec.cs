using System.Diagnostics;

namespace GFL
{
    /*
std.ostream & operator <<(std.ostream& os, const EdgeCollapseType a);
    */

    public class EdgeCollapseSpec
    {
        private EdgeCollapseType type_;
        private double time_;

        public EdgeCollapseSpec(EdgeCollapseType type = EdgeCollapseType.UNDEFINED)

        {
            type_ = type;
            Debug.Assert(type_ == EdgeCollapseType.PAST ||
                   type_ == EdgeCollapseType.NEVER);
        }

        public EdgeCollapseSpec(EdgeCollapseType type, double time)

        {
            type_ = type;
            time_ = time;
            Debug.Assert(type_ == EdgeCollapseType.FUTURE ||
                   type_ == EdgeCollapseType.ALWAYS);
        }

        public EdgeCollapseType type() { return type_; }
        public double time() { return time_; }
        public double get_printable_time() { return time_; }

        public int CompareTo(EdgeCollapseSpec? other)
        {
           return time_.CompareTo(other.time_);
        }
    };
    // std.ostream & operator <<(std.ostream& os, const CollapseSpec& s);
}