namespace Melomania
{
    /// <summary>
    /// Used to signal that a side effect was produced.
    /// </summary>
    public struct Unit
    {
        public static Unit Value => new Unit();
    }
}