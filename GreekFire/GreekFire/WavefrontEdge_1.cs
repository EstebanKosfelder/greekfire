

namespace GFL
{
    using GFL.Kernel;
    public class WavefrontSupportingLine
    {
        // private  using Transformation = CGAL.Aff_transformation_2<Kernel>;

        public Line2D l;

        public double weight;

        /// <summary>
        /// arbitrary length
        /// </summary>
        public Vector2D normal_direction;

        /// <summary>
        /// unit length
        /// </summary>
        public Vector2D normal_unit;

        /// <summary>
        /// weighted
        /// </summary>
        public Vector2D normal;

        public WavefrontSupportingLine(Vector2D u, Vector2D v, double p_weight = 1) : this(new Line2D(u, v), p_weight)
        {
        }

        public WavefrontSupportingLine(Line2D p_l, double p_weight =1)
        {
            this.l = p_l;
            this.weight = p_weight;
            normal_direction = l.to_vector().Perpendicular(ESign.COUNTERCLOCKWISE);
            normal_unit = normal_direction / normal_direction.Len_2D();
            normal = (normal_unit * weight);
        }

        public Line2D line_at_one()
        {
            throw new NotImplementedException();
            /*
          Transformation translate(CGAL.TRANSLATION, normal);
          return Line_2(translate(l));
            */
        }

        public Line2D line_at(double now)
        {
            throw new NotImplementedException();
            /*
            Transformation translate(CGAL.TRANSLATION, normal * now );
            return Line_2(translate(l));
            */
        }
    };
}