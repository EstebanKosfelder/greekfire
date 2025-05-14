namespace GFL
{
    /*
std.ostream & operator <<(std.ostream& os, const EdgeCollapseSpec& s);
    */

    public enum CollapseType
    {
        UNDEFINED = 1,
        /// <summary>
        /// This triangle has a vertex
        /// which is between parallel, opposing wavefront elements
        /// that have crashed into each other and their intersection
        /// is now a line segment.
        /// </summary>
        FACE_HAS_INFINITELY_FAST_VERTEX_OPPOSING,
        /// <summary>
        /// </summary>
        TRIANGLE_COLLAPSE,
        // NEIGHBORING_TRIANGLE_COLLAPSES,  /* do we ever need to handle this */
        /// <summary>
        /// </summary>
        CONSTRAINT_COLLAPSE,
        /// <summary>
        /// two non-incident vertices become incident,
        /// splitting the wavefront here.
        ///  UNUSED except in get_generic
        /// </summary>
        SPOKE_COLLAPSE,
        /// <summary>
        /// vertex moves onto supporting line of constraint,
        /// can refine event type when it comes to it.
        /// </summary>
        SPLIT_OR_FLIP_REFINE,
        /// <summary>
        /// This triangle has a vertex which is
        /// between parallel adjacent wavefront elements that have
        /// different weights but move in the same direction.
        /// </summary>
        FACE_HAS_INFINITELY_FAST_VERTEX_WEIGHTED,
        /// <summary>
        /// vertex moves into spoke (triangulation edge interior), flip event
        /// </summary>
        VERTEX_MOVES_OVER_SPOKE,
        /// <summary>
        /// the ccw vertex of the infinite vertex in an
        /// unbounded triangle leaves the convex hull of the
        /// wavefront polygon

        /// </summary>
        CCW_VERTEX_LEAVES_CH,
        // GENERIC_FLIP_EVENT,
        /// <summary>
        /// The triangle will collapse at this time, but we should
        /// never see this as prior events should have rebuilt the
        /// triangulation in some way.  If this is the next event,
        /// something went wrong.
        /// </summary>
        INVALID_EVENT,
        /// <summary>
        /// Leave this one last.  It serves also as a counter!
        /// </summary>
        NEVER,
    };
    // std.ostream & operator <<(std.ostream& os, const CollapseSpec& s);
}