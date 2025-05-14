namespace GFL.Kernel
{
    //using Double=System.Int64;

  
        public enum ESign
        {
            SMALLER = -1,
            EQUAL = 0,
            LARGER = 1,

            NEGATIVE = -1,
            ZERO = 0,
            POSITIVE = 1,

            LEFT_TURN = -1,
            COLLINEAR = 0,
            RIGHT_TURN = 1,

            CONVEX = LEFT_TURN,
            STRAIGHT = COLLINEAR,
            REFLEX = RIGHT_TURN,

            COUNTERCLOCKWISE = 1,
            CLOCKWISE = -1,

            ON_NEGATIVE_SIDE = -1,
            ON_ORIENTED_BOUNDARY = 0,
            ON_POSITIVE_SIDE = 1
        }
    
}