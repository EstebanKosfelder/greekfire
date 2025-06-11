namespace GFL
{
    public struct EdgePtrPair
    {
        public WavefrontEdge Left;
        public WavefrontEdge Right;

        public EdgePtrPair(WavefrontEdge left, WavefrontEdge right)
        {
            Left = left;
            Right = right;
        }

        public static implicit operator (WavefrontEdge, WavefrontEdge)(EdgePtrPair value)
        {
            return (value.Left, value.Right);
        }

        public static implicit operator EdgePtrPair((WavefrontEdge left, WavefrontEdge right) value)
        {
            return new EdgePtrPair(value.left, value.right);
        }
    };
}