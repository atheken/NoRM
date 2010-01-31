using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BSONLib;

namespace System.Data.Mongo.Protocol.Messages
{
    internal class ReplyMessage<T> : Message where T : class, new()
    {

        private List<T> _results;

        /// <summary>
        /// Processes a response stream.
        /// </summary>
        /// <param name="reply"></param>
        internal ReplyMessage(MongoContext context,
            String fullyQualifiedCollestionName, byte[] reply) :
            base(context, fullyQualifiedCollestionName)
        {
            this._messageLength = BitConverter.ToInt32(reply, 0);
            this._requestID = BitConverter.ToInt32(reply, 4);
            this._responseID = BitConverter.ToInt32(reply, 8);
            this._op = (MongoOp)BitConverter.ToInt32(reply, 12);
            this.HasError = BitConverter.ToInt32(reply, 16) == 1 ? true : false;
            this.CursorID = BitConverter.ToInt64(reply, 20);
            this.CursorPosition = BitConverter.ToInt32(reply, 28);
            this.ResultsReturned = BitConverter.ToInt32(reply, 32);

            this._results = new List<T>(100);//arbitrary number seems like a sweet spot for many queries.
            var memstream = new MemoryStream(reply.Skip(36).ToArray());
            memstream.Position = 0;
            var bin = new BinaryReader(memstream);
            if (!this.HasError)
            {
                while (bin.BaseStream.Position < bin.BaseStream.Length)
                {
                    this._results.Add(Message._serializer.Deserialize<T>(bin));
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
        /// The number of results returned form this request.
        /// </summary>
        public int ResultsReturned
        {
            get;
            protected set;
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
