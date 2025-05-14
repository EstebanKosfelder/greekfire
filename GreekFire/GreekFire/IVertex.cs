
namespace GFL
{
    using Kernel;
    public interface IVertex
    {
      


        int ID { get; }

        Vector2D Point { get; }
        IHalfedge Halfedge { get; set; }
       

     
    }
}