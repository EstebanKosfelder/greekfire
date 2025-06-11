namespace GFL.Kernel
{
    //using Double=System.Int64;


    public enum ESign
    {
        COUNTERCLOCKWISE = -1,
        CLOCKWISE = 1,

        SMALLER = -1,
        EQUAL = 0,
        LARGER = 1,

        Negative = -1,
        Zero = 0,
        Positive = 1,

      
       



        // ON_NEGATIVE_SIDE = -1,
        ON_ORIENTED_BOUNDARY = 0,
        //  ON_POSITIVE_SIDE = 1
    }

    public enum ETurn
    {
        Left = 1,
        Collinear = 0,
        Right = -1,
    }
    public enum EAngle
    {
        Convex = ETurn.Left,
        Straight = ETurn.Collinear,
        Reflex = ETurn.Right,


    }
}