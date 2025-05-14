namespace GFL
{
    public enum EdgeCollapseType
    {
        /// <summary>
        /// Undefined
        /// </summary>
        UNDEFINED = 1,
        /// <summary>
        /// endpoints moving away from another
        /// </summary>
        PAST,
        /// <summary>
        /// endpoints moving towards one another
        /// </summary>
        FUTURE,
        /// <summary>
        /// endpoints moving moving in parrallel and conincident
        /// </summary>
        ALWAYS,
        /// <summary>
        /// endpoints moving moving in parrallel but not conincident
        /// </summary>
        NEVER,
    };
    // std.ostream & operator <<(std.ostream& os, const CollapseSpec& s);
}