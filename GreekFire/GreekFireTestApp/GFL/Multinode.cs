namespace CGAL
{
    public class Multinode
    {
        public Multinode(Halfedge b, Halfedge e)
        {
            begin = b;
            end = e;
            v = b.Vertex;
            size = 0;
        }

        public List<Halfedge> haldedges = new List<Halfedge>();
        public CGAL.Vertex v;
        public Halfedge begin;
        public Halfedge end;

        public int size;
        public List<Halfedge> bisectors_to_relink =new List<Halfedge>();
        public List<Halfedge> bisectors_to_remove = new List<Halfedge>();
        public List<CGAL.Vertex> nodes_to_remove = new List<Vertex>();
    }
}