
using System.Text;

namespace CGAL
{
    public partial class KineticTriangle 
    {
        public static int MaxDigit = 1;
        private string IDPrintable
        {
            get
            {
                var r = Id.ToString();
                if (r.Length < MaxDigit) r= r.PadLeft(MaxDigit);
                return r;

            }
        }

        public KineticTriangle(int id):base(id) {
        }


       
        private int id => Id;
     
        public IEnumerable<Vector2>  Normals => Halfedge.Circulation(3).Select(h =>  h.GetNormal());
        public IEnumerable<Vertex> Vertices => Halfedge.Circulation(3).Select(h => h.Vertex);
        public IEnumerable<KineticTriangle?> Neighbors => Halfedge.Circulation(3).Select(h => h.Opposite.Face as KineticTriangle);

        public IEnumerable<WavefrontEdge?> Wavefronts => Halfedge.Circulation(3).Select(h => h.IsWavefrontEdge?h.WavefrontEdge:null);

      

        public (Point2 c, double r) InnerCircle()
        {
            return Mathex.InnerCircle(this.Vertices.Select(v => v.Point).ToArray());

        }

  


        public override string ToString()
        {

           
            StringBuilder sb = new StringBuilder();
            sb.Append($"kt{this.IDPrintable}");
            if(this.IsCollapseEventValid)
            sb.Append($"{this.CurrentEvent}");
            else
            {
                sb.Append($" mo valid event");
            }

            sb.Append($" wv[{string.Join(',', Vertices.Select(e =>$"{e.IDPrintable} {e.angle.ToString().ToLower()[0]}"))}]");
          //  sb.Append($"; p[{string.Join(',', Vertices.Select(e =>$"{e.Point}"))}]");
            sb.Append($"; n[{string.Join(',', Neighbors.Select(e => (e != null) ? e.IDPrintable : "*"))}]");
            sb.Append($"; wf[{string.Join(',',Wavefronts.Select(e => (e != null) ? e.ID.ToString() : "*"))}]");

            return sb.ToString();
        }

        internal OrientationEnum Orientation()
        {
            return (OrientationEnum) Mathex.sign(this.Area());;
        }


        //  friend class KineticTriangulation;





    }
}