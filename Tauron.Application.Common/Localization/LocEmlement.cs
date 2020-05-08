namespace Tauron.Localization
{
    public abstract class LocEmlement<TValue>
    {
        public string Key { get; }

        public virtual TValue Value { get; }

        protected LocEmlement(string key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}