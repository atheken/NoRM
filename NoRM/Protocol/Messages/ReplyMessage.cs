using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NoRM.BSON;

namespace NoRM.Protocol.Messages
{
    internal class ReplyMessage<T> : Message where T : class, new()
    {
        private List<T> _results;

        /// <summary>
        /// Processes a response stream.
        /// </summary>
        /// <param name="reply"></param>
        internal ReplyMessage(MongoServer context,
            String fullyQualifiedCollestionName, BinaryReader reply) :
            base(context, fullyQualifiedCollestionName)
        {
            this._messageLength = reply.ReadInt32();
            this._requestID = reply.ReadInt32();
            this._responseID = reply.ReadInt32();
            this._op = (MongoOp)reply.ReadInt32();
            this.HasError = reply.ReadInt32() == 1 ? true : false;
            this.CursorID = reply.ReadInt64();
            this.CursorPosition = reply.ReadInt32();
            //this.ResultsReturned = reply.ReadInt32();
            int read = reply.ReadInt32();

            //decrement the length for all the reads.
            this._messageLength -= (4 + 4 + 4 + 4 + 4 + 4 + 8 + 4 + 4);

            this._results = new List<T>(100);//arbitrary number seems like it would be a sweet spot for many queries.

            if (!this.HasError)
            {
                int length = 0;
                while (this._messageLength > 0)
                {
                    length = reply.ReadInt32();
                    if (length > 0)
                    {
                        var bin = BitConverter.GetBytes(length).Concat(
                        reply.ReadBytes(length - 4)).ToArray();

                        IDictionary<WeakReference, Flyweight> outProps = new Dictionary<WeakReference, Flyweight>(0);
                        var obj = BSONSerializer.Deserialize<T>(bin, ref outProps);
                        this._results.Add(obj);
                        if (this._context.EnableExpandoProperties)
                        {
                            ExpandoProps.SetFlyWeightObjects(outProps);
                        }
                    }
                    this._messageLength -= length;
                }
            }
            else
            {
                //TODO: load the error document.
            }
        }

        /// <summary>
        /// The cursor to be used in future calls to "get more"
        /// </summary>
        public long CursorID
        {
            get;
            protected set;
        }

        /// <summary>
        /// The location of the cursor.
        /// </summary>
        public int CursorPosition
        {
            get;
            protected set;
        }

        /// <summary>
        /// If "HasError" is set, 
        /// </summary>
        public bool HasError
        {
            get;
            protected set;
        }

        /// <summary>
        /// The number of results returned from this request.
        /// </summary>
        public int Count
        {
            get
            {
                return this._results.Count;
            }
        }

        public IEnumerable<T> Results
        {
            get
            {
                return this._results.AsEnumerable();
            }
        }
    }
}
