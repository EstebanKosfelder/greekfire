namespace CGAL
{
    public enum EdgeCollapseType
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefine = 1,
        /// <summary>
        /// endpoints moving away from another
        /// </summary>
        Past,
        /// <summary>
        /// endpoints moving towards one another
        /// </summary>
        Future,
        /// <summary>
        /// endpoints moving moving in parrallel and conincident
        /// </summary>
        Always,
        /// <summary>
        /// endpoints moving moving in parrallel but not conincident
        /// </summary>
        Never,
    };
    // std.ostream & operator <<(std.ostream& os, const CollapseSpec& s);
}