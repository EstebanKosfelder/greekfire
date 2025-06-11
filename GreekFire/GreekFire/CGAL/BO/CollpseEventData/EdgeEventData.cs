using System.ComponentModel;
using System.Diagnostics;

namespace CGAL
{
    public abstract class EdgeEventData:EventData
    {
        public override string ToString()
        {
           return $"{base.ToString()} Edge:{Edge.LogVIds} ";
        }
        public EdgeEventData(Halfedge edge, CollapseType type, double time) :
            base(type, time)
        {
            Edge = edge;
            Debug.Assert (Edge.Face!=null && Edge.Face is KineticTriangle );
            _triangle = (KineticTriangle)Edge.Face; 



        }
        private KineticTriangle _triangle;

        public virtual Halfedge Edge { get; protected set; }
        public override KineticTriangle Triangle => _triangle;


        public bool AllowsRefinementTo()
        {
            EventData eventData = this.Triangle.CurrentEvent??throw new NullReferenceException("No existe CollapseEvent");
            Debug.Assert(Time == eventData.Time);


            if (eventData.Type == CollapseType.SplitOrFlipRefine)
            {
                if (!(eventData is SplitOrFlipRefineEventData splitOrFlipRefineEventData))
                    throw new InvalidCastException();

                if (Type == CollapseType.VertexMovesOverSpoke ||
                    Type == CollapseType.SpokeCollapse


                    )
                {

                    if (Edge != splitOrFlipRefineEventData.Edge)
                    {
                        return true;
                    }
                }
            }
            else if (Type == CollapseType.SplitCollapse)
            {
                return true;
            }
            return false;
        }


    }

   
}