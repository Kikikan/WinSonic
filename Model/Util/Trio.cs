namespace WinSonic.Model.Util
{
    public class Trio<L, M, R> : Pair<L, R>
    {
        public M Middle { get; private set; }
        public Trio(L left, M middle, R right) : base(left, right)
        {
            Middle = middle;
        }
    }
}
