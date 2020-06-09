using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Akrual.DDD.Utils.Internal.Serializer
{
    public class MessagePackSerializerLz4
    {
        public static MessagePackSerializerLz4 Instance = new MessagePackSerializerLz4();

        public T Deserialize<T>(byte[] serializedObject)
        {
            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            return MessagePackSerializer.Deserialize<T>(serializedObject, lz4Options);
        }

        public dynamic Deserialize(Type type, byte[] serializedObject)
        {
            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            return MessagePackSerializer.Deserialize(type, serializedObject, lz4Options);
        }

        public byte[] Serialize(object item)
        {
            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            var result = MessagePackSerializer.Serialize(item, lz4Options);

            return result;
        }
    }
}
