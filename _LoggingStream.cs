using System;
using System.Collections.Generic;
using System.Text;

namespace IBAPI
{
	public class LoggingStream : System.IO.Stream
	{
		private readonly System.IO.Stream _stream;
		private readonly System.IO.Stream _receiveStream;
		private readonly System.IO.Stream _sendStream;

		public LoggingStream(System.IO.Stream stream, System.IO.Stream receiveStream, System.IO.Stream sendStream)
		{
			_stream = stream;
			_receiveStream = receiveStream;
			_sendStream = sendStream;
		}

		public override bool CanRead { get { return true; } }
		public override bool CanSeek { get { return true; } }
		public override bool CanWrite { get { return true; } }

		public override long Length
		{
			get { return _stream.Length; }
		}

		public override long Position
		{
			get { return _stream.Position; }
			set { _stream.Position = value; }
		}

		public override void Flush()
		{
			_stream.Flush();
			_receiveStream.Flush();
			_sendStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int bytesRead = _stream.Read(buffer, offset, count);
			_receiveStream.Write(buffer, offset, bytesRead);
			return bytesRead;
		}

        public override int ReadByte()
        {
			int n = _stream.ReadByte();
			if (n != -1)
				_receiveStream.WriteByte((byte)n);
			return n;
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
		{
			return _stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_sendStream.Write(buffer, offset, count);
			_stream.Write(buffer, offset, count);
		}

	}
}
