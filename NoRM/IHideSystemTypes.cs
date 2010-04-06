using System;
using System.ComponentModel;

namespace Norm
{
    /// <summary>
    /// This hides the object members below for things like fluent configuration
    /// where "ToString" makes no sense.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IHideObjectMembers
    {
        /// <summary>TODO::Description.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();

        /// <summary>TODO::Description.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        /// <summary>TODO::Description.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        string ToString();

        /// <summary>TODO::Description.</summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);
    }
}