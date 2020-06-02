namespace Tauron.Localization
{
    public abstract class LocEmlement<TValue>
    {
        protected LocEmlement(string key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }

        public virtual TValue Value { get; }
    }
}