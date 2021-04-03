using Bedrock.Framework.Protocols;
using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace ReadAsyncRepro
{
    public class CustomProtocol : IMessageReader<ReadOnlyMemory<byte>>
    {
        private static byte[] crlf = new byte[] { 0x0D, 0x0A };
        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out ReadOnlyMemory<byte> message)
        {
            //This check can be used to avoid the SequenceReader ctor exception, but another exception
            //will be thrown after a few attempts at the second message

            /*if (input.IsEmpty)
            {
                message = default;
                return false;
            }*/

            var reader = new SequenceReader<byte>(input);

            if (reader.TryReadTo(out ReadOnlySequence<byte> packetSequence, crlf, advancePastDelimiter: true))
            {
                SequenceMarshal.TryGetReadOnlyMemory(packetSequence, out message);
                consumed = reader.Position;
                examined = consumed;
                return true;
            }

            consumed = input.Start;
            examined = input.End;
            message = default;
            return false;
        }


    }
}
