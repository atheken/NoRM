using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Norm.BSON
{
    /// <summary>
    /// Provides a mechanism for parsing JSON into an Expando.
    /// </summary>
    public class ObjectParser
    {
        private static readonly Regex _rxObject = new Regex(@"\s*{\s*(?<obj>.*)\s*}\s*(,|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex _rxArray = new Regex("\\s*\\[\\s*(?<arrayValues>.*)\\s*]\\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// deals with most of the possible quote escapes, but a few remain.
        /// </summary>
        private static readonly Regex _rxPair = new Regex(@"\s*""(?<key>.*?)((?<!\\)(?<!\\)"")\s*:\s*(?<value>(((?'Open'\[)[^[]*)(?'Close-Open']))|((.*?)|(""(.*?)((?<!\\)(?<!\\)""))))\s*(,|$)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _rxArrayMember = new Regex(@"\s*(?<value>(((?'Open'\[)[^[]*)(?'Close-Open']))|((.*?)|(""(.*?)((?<!\\)(?<!\\)""))))\s*(,|$)",
           RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _rxBool = new Regex(@"^\s*(true)|(false)\s*(,|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _rxNull = new Regex(@"^\s*null\s*(,|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex _rxNumber = new Regex(@"^\s*-?\s*(([0-9]*[.]?[0-9]*)|([0-9]+))\s*(e(\+|-)?[0-9]+)?\s*(,|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Convert a string to an IExpando.
        /// </summary>
        /// <exception cref="">Throws an exception when the string passed in cannot be parsed.</exception>
        /// <param name="jsonToParse"></param>
        /// <returns></returns>
        public Expando ParseJSON(String jsonToParse)
        {
            Expando retval = new Expando();
            var memberstring = _rxObject.Match(jsonToParse).Groups["obj"].Value;
            Match m;
            do
            {
                m = _rxPair.Match(memberstring);
                if(m.Success)
                {
                    retval[m.Groups["key"].Value] = this.ParseMember(m.Groups["value"].Value);
                    memberstring = memberstring.Remove(0, m.Length);
                }

            } while (m.Success && memberstring.Length > 0);
            return retval;
        }

        private Object[] ParseJSONArray(String jsonToParse)
        {
            var retval = new List<Object>();
            var memberstring = _rxArray.Match(jsonToParse).Groups["arrayValues"].Value;
            Match m;
            do
            {
                m = _rxArrayMember.Match(memberstring);
                if (m.Success)
                {
                    retval.Add(this.ParseMember(m.Groups["value"].Value));
                    memberstring = memberstring.Remove(0, m.Length);
                }

            } while (m.Success && memberstring.Length > 0);
            return retval.ToArray();
        }

        private Object ParseMember(String member)
        {
            member = member ?? "";
            object retval = null;
            if (_rxObject.IsMatch(member))
            {
                //construct object.
                retval = this.ParseJSON(member);
            }
            else if (_rxArray.IsMatch(member))
            {
                retval = this.ParseJSONArray(member);
            }
            else if (_rxNull.IsMatch(member))
            {
                retval = null;
            }
            else if (_rxBool.IsMatch(member))
            {
                retval = Boolean.Parse(member);
            }
            else if (_rxNumber.IsMatch(member))
            {
                retval = double.Parse(member);
            }
            else
            {
                //scrub the quotes.
                member = member.Trim();
                if (member.StartsWith("\"") && member.EndsWith("\""))
                {
                    member = member.Remove(0, 1);
                    member = member.Substring(0, member.Length - 1);
                }
                retval = member;
            }

            return retval;
        }

    }
}
