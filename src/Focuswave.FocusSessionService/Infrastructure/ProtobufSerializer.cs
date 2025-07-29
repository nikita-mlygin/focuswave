using Confluent.Kafka;
using Google.Protobuf;

namespace Focuswave.FocusSessionService.Infrastructure;

public class ProtobufSerializer<T> : ISerializer<T>, IDeserializer<T>
    where T : IMessage<T>, new()
{
    public byte[] Serialize(T data, Confluent.Kafka.SerializationContext context)
    {
        if (data == null)
            return [];
        return data.ToByteArray();
    }

    public T Deserialize(
        ReadOnlySpan<byte> data,
        bool isNull,
        Confluent.Kafka.SerializationContext context
    )
    {
        if (isNull || data.IsEmpty)
            return default!;

        var parser = new MessageParser<T>(() => new T());
        return parser.ParseFrom(data);
    }
}
