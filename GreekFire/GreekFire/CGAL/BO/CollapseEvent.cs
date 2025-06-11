using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CGAL
{
    public class CollapseEvent : IComparable<CollapseEvent>
    {
        public KineticTriangle t { get; private set; }

        public EventData EventData => t.CurrentEvent ?? throw new NullReferenceException($"{t} sin Collapse Event Data");
        public double Time => EventData.Time;

        public void Handle(GreekFireBuilder builder) => EventData.Handle(builder);

        public CollapseType Type => EventData.Type;

        public CollapseEvent(KineticTriangle triangle, double now)
        {
            Debug.Assert(triangle != null);
            Debug.Assert(!triangle.IsCollapseEventValid);
            t = triangle;
            t.GetCollapse(now);
            Debug.Assert(t.CurrentEvent != null);
            Debug.Assert(EventData.Triangle == t);
        }

        //}
        public override string ToString()
        {
            return $"{t} {base.ToString()}";
        }

        public override bool Equals(object? obj)
        {
            if (obj is EventData otherEventData)
            {
                Debug.Assert(false);
                return this.EventData.Equals(otherEventData);
            }

            if (obj is CollapseEvent other)
            {
                return t.Id == other.t.Id && this.EventData.Equals(other.EventData);
            }

            return false;
        }

        public void UpdateCollapse(double now)
        {
          //  Debug.Assert(!t.IsCollapseEventValid);
            if (!t.IsCollapseEventValid)
            {
                t.GetCollapse(now);
            }
        }

        public int CompareTo(CollapseEvent? other)
        {
            Debug.Assert(other != null);
            var result = EventData.CompareTo(other.EventData);
            if (result != 0) return result;
            return t.Id.CompareTo(other?.t.Id);
        }

        //public static bool operator <(CollapseEvent left, CollapseEvent right)
        //{
        //    return left.CompareTo(right) < 0;
        //}

        //public static bool operator <=(CollapseEvent left, CollapseEvent right)
        //{
        //    return left.CompareTo(right) <= 0;
        //}

        //public static bool operator >(CollapseEvent left, CollapseEvent right)
        //{
        //    return left.CompareTo(right) > 0;
        //}

        //public static bool operator >=(CollapseEvent left, CollapseEvent right)
        //{
        //    return left.CompareTo(right) >= 0;
        //}

        //public static bool operator ==(CollapseEvent left, CollapseSpec right)
        //{
        //    if (left != null)
        //    {
        //        return left.Equals(right);
        //    }
        //    else if (right == null)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //public static bool operator !=(CollapseEvent left, CollapseSpec right)
        //{
        //    return !(left == right);
        //}
    }
}