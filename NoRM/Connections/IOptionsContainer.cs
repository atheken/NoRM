namespace NoRM
{
    internal interface IOptionsContainer
    {
        void SetQueryTimeout(int timeout);
        void SetEnableExpandoProperties(bool enabled);
        void SetStrictMode(bool strict);
        void SetPoolSize(int size);
        void SetPooled(bool pooled);
    }
}