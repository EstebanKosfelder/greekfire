namespace GFL
{
    /*
std.ostream & operator <<(std.ostream& os, const EdgeCollapseSpec& s);
    */

    public enum CollapseType
    {
        Undefined = 1,
        /// <summary>
        /// This triangle has a vertex
        /// which is between parallel, opposing wavefront elements
        /// that have crashed into each other and their intersection
        /// is now a line segment.
        /// </summary>
        FaceHasInfinitelyFastOpposing,
        /// <summary>
        /// </summary>
        TriangleCollapse,
        // NEIGHBORING_TRIANGLE_COLLAPSES,  /* do we ever need to handle this */
        /// <summary>
        /// </summary>
        ConstraintCollapse,
        /// <summary>
        /// two non-incident vertices become incident,
        /// splitting the wavefront here.
        ///  UNUSED except in get_generic
        /// </summary>
        SpokeCollapse,
        /// <summary>
        /// vertex moves onto supporting line of constraint,
        /// can refine event type when it comes to it.
        /// </summary>
        SplitOrFlipRefine,
        /// <summary>
        /// This triangle has a vertex which is
        /// between parallel adjacent wavefront elements that have
        /// different weights but move in the same direction.
        /// </summary>
        FaceHasInfinitelyFastVertexWeighted,
        /// <summary>
        /// vertex moves into spoke (triangulation edge interior), flip event
        /// </summary>
        VertexMovesOverSpoke,
        /// <summary>
        /// the ccw vertex of the infinite vertex in an
        /// unbounded triangle leaves the convex hull of the
        /// wavefront polygon

        /// </summary>
        CcwVertexLeavesCh,
        // GENERIC_FLIP_EVENT,
        /// <summary>
        /// The triangle will collapse at this time, but we should
        /// never see this as prior events should have rebuilt the
        /// triangulation in some way.  If this is the next event,
        /// something went wrong.
        /// </summary>
        InvalidEvent,
        /// <summary>
        /// Leave this one last.  It serves also as a counter!
        /// </summary>
        Never,
    };
    // std.ostream & operator <<(std.ostream& os, const CollapseSpec& s);
}