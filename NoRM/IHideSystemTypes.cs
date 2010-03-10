using System;
using System.ComponentModel;

namespace NoRM
{
    /// <summary>
    /// This hides the object members below for things like fluent configuration
    /// where "ToString" makes no sense.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();
        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();
        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);
    }
}