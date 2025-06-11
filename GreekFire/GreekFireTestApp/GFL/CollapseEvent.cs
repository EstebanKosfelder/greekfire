
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GFL
{
    public class CollapseEvent : CollapseSpec,IComparable<CollapseEvent>
    {
        public KineticTriangle t;
        public CollapseEvent(KineticTriangle t, double now) : base(t.GetCollapse(now))
        {

            this.t = t;

        }
        public override string ToString()
        {
            return $"{t} {base.ToString()}";
        }
       
    


        public override bool Equals(object? obj)
        {
            bool result = base.Equals(obj);

            if ( result &&  obj is CollapseEvent evnt )
            {
                result =  t == evnt.t;
            }

            return result;

        }

        
public void UpdateCollapse(double now) {
 // DBG_FUNC_BEGIN(DBG_EVENTQ);

    Assign(t.GetCollapse(now) );

   //     DBG_FUNC_END(DBG_EVENTQ);
    }

        public int CompareTo(CollapseEvent? other)
        {
           return base.CompareTo(other);
        }

        public static bool operator <(CollapseEvent left, CollapseEvent right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(CollapseEvent left, CollapseEvent right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(CollapseEvent left, CollapseEvent right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(CollapseEvent left, CollapseEvent right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator == (CollapseEvent left, CollapseSpec right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(CollapseEvent left, CollapseSpec right)
        {

            return !left.Equals(right);
        }

    }
}



