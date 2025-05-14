namespace GFL
{
    public class BisectorHalfedge : Halfedge
    {
        public BisectorHalfedge(int aID, SignEnum aSlope = SignEnum.ZERO) : base(aID, aSlope)
        {
        }
        public override string ToString() => $"B{base.ToString()}";
    }
}

