using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NoRM.BSON.DbTypes
{
    public class GridReadStream : Stream
    {
        private GridFile _gridFile;
        private MongoCollection<GridFile> _collection;
        private long _offset = 0;

        public GridReadStream(GridFile file, MongoCollection<GridFile> rootCollection)
        {
            this._gridFile = file;
            this._collection = rootCollection;
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                //for now, if you'd like to implement streaming, be our guest.
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException("This stream doesn't write.");
        }

        public override long Length
        {
            get { return this._gridFile.length.Value; }
        }

        public override long Position
        {
            get
            {
                return this._offset;
            }
            set
            {
                throw new NotSupportedException("This stream cannot seek, if you would like it to, consider contributing to the project.");
            }
        }

        /// <summary>
        /// Get the bytes from the server.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            //do some math to figure out which chunk we want..
            var location = Math.Floor((double)this.Length / this._gridFile.chunkSize.Value);
            int retval = 0;
            
            //locate the first chunk based
            int chunkNumber = (int)Math.Floor((double)(this.Position + offset) / this.Length);
            var skip = (int)((this.Position + offset) % this.Length);
            this._offset += skip;

            int bufferOffset = 0;
            do
            {
                var chunk = this._collection.GetChildCollection<GridFileChunk>("chunks")
                    .FindOne(new { file_id = this._gridFile._id, n = chunkNumber });
                if (chunk != null)
                {
                    var readBytes = Math.Min(count, chunk.data.Length);
                    
                    //boy, I sure hope I am not off by one...
                    Buffer.BlockCopy(chunk.data, skip, buffer, bufferOffset, readBytes);
                    count -= readBytes;
                    bufferOffset += readBytes;
                    retval += readBytes;
                    this._offset += readBytes;
                    chunkNumber++;
                    skip = 0;
                }
            } while (count > 0);

            return retval;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("This stream cannot seek, if you want it to, implement seeking and contribute it to the project");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("This stream is read-only.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("You cannot write to this stream, you can overwrite file contents via GridFile.");
        }
    }
}
