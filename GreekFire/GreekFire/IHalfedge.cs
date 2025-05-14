

namespace GFL
{
    using Kernel;

    public interface IHalfedge
    {
        int ID { get; }
        IVertex Vertex { get; set; }
        IHalfedge Next { get; set; }
        IHalfedge Prev { get; set; }
        IFace Face { get; set; }
        IHalfedge Opposite { get; set; }
        bool IsBisector { get; }

        public Vector2D GetNormal()=> (Opposite != null) ? (Vertex.Point - Opposite.Vertex.Point).N: Vector2D.Zero;
        public IEnumerable<IHalfedge> Circulation(int max = 10000)
        {
            IHalfedge curr = this;
            int i = 0;
            if (curr == null)
            {
                throw new InvalidOperationException($"{this} has not Halfedge asigned");

            }
            do
            {

                yield return curr;
                curr = curr.Next;

                if (curr == null)
                {
                    throw new InvalidOperationException($"{this} has not Halfedge asigned");

                }
                if (max < i++)
                {
                    Console.WriteLine($" {this} iterate more than {max} "); break;
                }
            } while (curr != this);

        }

     //   public WaveFront WaveFront => new WaveFront(this.Vertex.Point,this.Opposite.Vertex.Point);
    }

}