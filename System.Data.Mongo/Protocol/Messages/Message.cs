using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace System.Data.Mongo.Protocol
{
    internal class Message
    {
        private TcpClient _client;

        public MessageHeader Header
        {
            get;
            protected set;
        }

        public Message() { }


        public Message(TcpClient client, MongoOp messageType, byte[] body)
        {

        }



        public byte[] RequestBytes 
        {
            get
            {
                return new byte[0];
            }
        }

        public byte[] ResponseBytes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        
    }
}
