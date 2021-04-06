using Bedrock.Framework.Protocols;
using System;
using System.Buffers;

namespace ReadAsyncRepro
{
    public class CustomProtocol : IMessageReader<ReadOnlyMemory<byte>>
    {
        private static byte delimiter = (byte)'\n';

        public byte[] buffer = new byte[256];
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ReadOnlyMemory<byte> message)
        {
            var reader = new SequenceReader<byte>(input);

            int position = 0;

            while (reader.TryRead(out byte value))
            {
                buffer[position] = value;

                if (value == delimiter)
                {
                    consumed = reader.Position;
                    examined = consumed;
                    message = buffer.AsMemory()[..(position + 1)];
                    return true;
                }

                ++position;
            }

            consumed = input.Start;
            examined = input.End;
            message = default;
            return false;
        }


    }
}
