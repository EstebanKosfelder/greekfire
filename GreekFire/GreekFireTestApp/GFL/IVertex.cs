
namespace GFL
{
    using Kernel;
    public interface IVertex
    {
      
        string IDPrintable { get; }

        int ID { get; }

        Vector2D Point { get; }
        IHalfedge Halfedge { get; set; }
       

     
    }
}