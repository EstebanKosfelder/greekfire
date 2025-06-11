

using System;
using static GFL.GreekFireUtils;
namespace GFL
{

    public class Face:IFace {

        public static Face NULL { get; private set; } 
        
        static Face()
        {
            NULL = new Face(-1);
        }

        public Face( int id,int outSide = -1)
        {
            ID = id;
            Outside = outSide;
            
        }


        public IHalfedge Halfedge { get; set; } = null;

    
        public int ID { get; private set; }
        public int Outside { get; private set; }

        [Obsolete]
        internal IHalfedge halfedge() => Halfedge;

        [Obsolete]
        internal void set_halfedge(HalfedgeBase aHE) { Halfedge = aHE; }

        internal void reset_id(int aID) { ID = aID; }

        public override string ToString()
        {
            return $"F{ID} Os:{Outside} H:{Halfedge.ID}";
        }

    }

    public static partial class GreekFireUtils
    {
        public static bool IsValid(Face face) => face != null && face != Face.NULL;  
    }

}