namespace GFL
{
    public interface IFace
    {
        int ID { get;}
        IHalfedge Halfedge{ get; set; }
    }

}