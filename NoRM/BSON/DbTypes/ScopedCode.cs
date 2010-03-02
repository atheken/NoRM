namespace NoRM
{
    /// <summary>
    /// A class that represents code with scoping - will be serialized to 
    /// </summary>
    public class ScopedCode<T>:ScopedCode
    {        
        /// <summary>
        /// The Scope this this code.
        /// </summary>
        public new T Scope { get; set; }
    }

    public class ScopedCode
    {
        /// <summary>
        /// The scope code.
        /// </summary>
        public string CodeString { get; set; }

        /// <summary>
        /// The scope for this code.
        /// </summary>
        public object Scope { get; set; }
    }
}
