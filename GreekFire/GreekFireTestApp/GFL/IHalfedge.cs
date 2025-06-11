

namespace GFL
{
    using Kernel;

    public interface IHalfedge
    {
        int ID { get; }
        IVertex Vertex_ { get; set; }
        IHalfedge Next_ { get; set; }
        IHalfedge Prev_ { get; set; }
        IFace Face { get; set; }
        IHalfedge Opposite_ { get; set; }
      

        public Vector2D GetNormal()=> (Opposite_ != null) ? (Vertex_.Point - Opposite_.Vertex_.Point).N: Vector2D.Zero;
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
                curr = curr.Next_;

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