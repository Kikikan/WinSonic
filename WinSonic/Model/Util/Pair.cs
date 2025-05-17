namespace WinSonic.Model.Util
{
    internal class Pair<L, R>
    {
        public L Left { get; private set; }
        public R Right { get; private set; }
        public Pair(L left, R right)
        {
            Left = left;
            Right = right;
        }
    }
}
