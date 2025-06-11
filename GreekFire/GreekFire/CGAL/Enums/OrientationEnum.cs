namespace CGAL
{
    public enum OrientationEnum : int
    {
        RIGHT_TURN = -1, LEFT_TURN = 1,

        CONVEX = 1, REFLEX =-1,
            CLOCKWISE = -1, COUNTERCLOCKWISE = 1,

        COLLINEAR = 0, COPLANAR = 0, DEGENERATE = 0, STRAIGHT = 0,
        ON_NEGATIVE_SIDE = -1, ON_ORIENTED_BOUNDARY = 0, ON_POSITIVE_SIDE = 1,

    }

    public enum ESign
    {
        COUNTERCLOCKWISE = OrientationEnum.COUNTERCLOCKWISE,
        CLOCKWISE = OrientationEnum.CLOCKWISE,

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
        Left = OrientationEnum.LEFT_TURN,
        Collinear = 0,
        Right = OrientationEnum.RIGHT_TURN,
    }
    public enum EAngle
    {
        Convex = OrientationEnum.CONVEX,
        Straight = OrientationEnum.STRAIGHT,
        Reflex = OrientationEnum.REFLEX,


    }
}

