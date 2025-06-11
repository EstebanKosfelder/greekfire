using GFL.Kernel;
using System.Text;

namespace GFL
{
    public partial class KineticTriangle : IFace
    {
        public static int MaxDigit = 1;
        private string IDPrintable
        {
            get
            {
                var r = ID.ToString();
                if (r.Length < MaxDigit) r= r.PadLeft(MaxDigit);
                return r;

            }
        }

        public KineticTriangle(int id)
        {
            ID = id;
        }


        public int ID { get; private set; }
        private int id => ID;
     
        public IEnumerable<Vector2D>  Normals => Halfedge.Circulation(3).Select(h => h.GetNormal());
        public IEnumerable<WavefrontVertex> Vertices => Halfedge.Circulation(3).Select(h => h.Vertex_).Cast<WavefrontVertex>();
        public IEnumerable<KineticTriangle?> Neighbors => Halfedge.Circulation(3).Select(h => h.Opposite_.Face as KineticTriangle);

        public IEnumerable<WavefrontEdge?> Wavefronts => Halfedge.Circulation(3).Select(h => h.Opposite_ as WavefrontEdge);
        public IEnumerable<KineticHalfedge> Halfedges => this.Halfedge.Circulation(3).Cast<KineticHalfedge>();

        public (Vector2D c, double r) InnerCircle()
        {
            return MathEx.InnerCircle(this.Vertices.Select(v => v.Point).ToArray());

        }

        public IHalfedge Halfedge { get; set; }


        public override string ToString()
        {

           
            StringBuilder sb = new StringBuilder();
            sb.Append($"kt{this.IDPrintable}");
            sb.Append($" wv[{string.Join(',', Vertices.Select(e =>$"{e.IDPrintable} {e.angle.ToString().ToLower()[0]}"))}]");
            sb.Append($"; n[{string.Join(',', Neighbors.Select(e => (e != null) ? e.IDPrintable : "*"))}]");
            sb.Append($"; wf[{string.Join(',',Wavefronts.Select(e => (e != null) ? e.ID.ToString() : "*"))}]");
            return sb.ToString();
        }


        //  friend class KineticTriangulation;





    }
}