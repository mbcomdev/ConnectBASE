// this class is just á swagger example
// generatetd from https://quicktype.io/csharp/
namespace connectBase.Services.swagger.examples
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class SchemeExample
    {
        [JsonProperty("Artikel")]
        public Dictionary<string, Artikel> Artikel { get; set; }
    }

    public partial class Artikel
    {
        [JsonProperty("Feldname")]
        public string Feldname { get; set; }

        [JsonProperty("Feldinfo")]
        public string Feldinfo { get; set; }

        [JsonProperty("Feldart")]
        public Feldart Feldart { get; set; }
    }

    public enum Feldart { FdaAutoInc, FdaBild, FdaBoolean, FdaByte, FdaDate, FdaDateTime, FdaDouble, FdaInfo, FdaInteger, FdaSingle, FdaString, FdaUnicodeString };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                FeldartConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class FeldartConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Feldart) || t == typeof(Feldart?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "fdaAutoInc":
                    return Feldart.FdaAutoInc;
                case "fdaBild":
                    return Feldart.FdaBild;
                case "fdaBoolean":
                    return Feldart.FdaBoolean;
                case "fdaByte":
                    return Feldart.FdaByte;
                case "fdaDate":
                    return Feldart.FdaDate;
                case "fdaDateTime":
                    return Feldart.FdaDateTime;
                case "fdaDouble":
                    return Feldart.FdaDouble;
                case "fdaInfo":
                    return Feldart.FdaInfo;
                case "fdaInteger":
                    return Feldart.FdaInteger;
                case "fdaSingle":
                    return Feldart.FdaSingle;
                case "fdaString":
                    return Feldart.FdaString;
                case "fdaUnicodeString":
                    return Feldart.FdaUnicodeString;
            }
            throw new Exception("Cannot unmarshal type Feldart");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Feldart)untypedValue;
            switch (value)
            {
                case Feldart.FdaAutoInc:
                    serializer.Serialize(writer, "fdaAutoInc");
                    return;
                case Feldart.FdaBild:
                    serializer.Serialize(writer, "fdaBild");
                    return;
                case Feldart.FdaBoolean:
                    serializer.Serialize(writer, "fdaBoolean");
                    return;
                case Feldart.FdaByte:
                    serializer.Serialize(writer, "fdaByte");
                    return;
                case Feldart.FdaDate:
                    serializer.Serialize(writer, "fdaDate");
                    return;
                case Feldart.FdaDateTime:
                    serializer.Serialize(writer, "fdaDateTime");
                    return;
                case Feldart.FdaDouble:
                    serializer.Serialize(writer, "fdaDouble");
                    return;
                case Feldart.FdaInfo:
                    serializer.Serialize(writer, "fdaInfo");
                    return;
                case Feldart.FdaInteger:
                    serializer.Serialize(writer, "fdaInteger");
                    return;
                case Feldart.FdaSingle:
                    serializer.Serialize(writer, "fdaSingle");
                    return;
                case Feldart.FdaString:
                    serializer.Serialize(writer, "fdaString");
                    return;
                case Feldart.FdaUnicodeString:
                    serializer.Serialize(writer, "fdaUnicodeString");
                    return;
            }
            throw new Exception("Cannot marshal type Feldart");
        }

        public static readonly FeldartConverter Singleton = new FeldartConverter();
    }
}
