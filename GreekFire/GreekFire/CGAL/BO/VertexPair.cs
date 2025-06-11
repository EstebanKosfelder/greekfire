namespace CGAL
{
    public struct  VertexPair
    {

        public static implicit operator (Vertex first, Vertex last)(VertexPair v) => (v.first, v.last);
        public static implicit operator VertexPair((Vertex first, Vertex last) v) => new VertexPair(v.first, v.last);

        public VertexPair (Vertex first, Vertex last)
        {
            this.first = first;
            this.last = last;
        }

        public Vertex first;
        public Vertex last;

        public void Deconstruct(out Vertex first, out Vertex last)
        {
            first = this.first;
            last = this.last;
        }
    }
}