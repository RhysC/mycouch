﻿using System.IO;
using System.Text;
using EnsureThat;
using Newtonsoft.Json;

namespace MyCouch.Serialization
{
    public class DefaultSerializer : ISerializer
    {
        protected readonly SerializationConfiguration Configuration;
        protected readonly JsonSerializer InternalSerializer;

        public DefaultSerializer(SerializationConfiguration configuration)
        {
            Ensure.That(configuration, "configuration").IsNotNull();

            Configuration = configuration;
            InternalSerializer = JsonSerializer.Create(Configuration.Settings);
        }

        public virtual string Serialize<T>(T item) where T : class
        {
            var content = new StringBuilder();
            using (var stringWriter = new StringWriter(content))
            {
                using (var jsonWriter = Configuration.WriterFactory(typeof(T), stringWriter))
                {
                    InternalSerializer.Serialize(jsonWriter, item);
                }
            }
            return content.ToString();
        }

        public virtual T Deserialize<T>(string data) where T : class
        {
            if (string.IsNullOrWhiteSpace(data))
                return null;

            using (var reader = new StringReader(data))
            {
                using (var jsonReader = Configuration.ReaderFactory(typeof(T), reader))
                {
                    return InternalSerializer.Deserialize<T>(jsonReader);
                }
            }
        }

        public virtual T Deserialize<T>(Stream data) where T : class
        {
            if (data == null || data.Length < 1)
                return null;

            using (var reader = new StreamReader(data, MyCouchRuntime.DefaultEncoding))
            {
                using (var jsonReader = Configuration.ReaderFactory(typeof(T), reader))
                {
                    return InternalSerializer.Deserialize<T>(jsonReader);
                }
            }
        }
    }
}