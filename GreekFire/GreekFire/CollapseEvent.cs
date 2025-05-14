
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFL
{
    public class CollapseEvent : CollapseSpec,IComparable<CollapseEvent>
    {
        public KineticTriangle t;
        public CollapseEvent(KineticTriangle t, double now) : base(t.get_collapse(now))
        {

            this.t = t;

        }

        public int CompareTo(CollapseEvent? other)
        {
          return base.CompareTo(other);
          
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

        
public void update_collapse(double now) {
 // DBG_FUNC_BEGIN(DBG_EVENTQ);

    Assign(t.get_collapse(now) );

   //     DBG_FUNC_END(DBG_EVENTQ);
    }


    }
}



