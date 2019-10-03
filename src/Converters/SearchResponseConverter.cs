using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Victoria.Enums;
using Victoria.Responses.Rest;

namespace Victoria.Converters
{
    internal sealed class SearchResponseConverter : JsonConverter<SearchResponse>
    {
        /// <inheritdoc />
        public override SearchResponse Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            var searchResponse = new SearchResponse();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                var index = reader.ValueSpan[0];
                reader.Read();

                if (index == 108)
                    GetLoadType(ref searchResponse, ref reader);
                else if (index == 112)
                    BuildPlaylistInfo(ref searchResponse, ref reader);
                else if (index == 101)
                    BuildRestException(ref searchResponse, ref reader);
                else if (index == 116)
                    BuildTracksList(ref searchResponse, ref reader);
                else
                    throw new JsonException($"Unhandled index type: {index}");
            }

            return searchResponse;
        }

        private void GetLoadType(ref SearchResponse response, ref Utf8JsonReader reader)
            => response.LoadType = (LoadType) reader.ValueSpan[0];

        private void BuildPlaylistInfo(ref SearchResponse response, ref Utf8JsonReader reader)
        {
            var playlist = new PlaylistInfo();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                var index = reader.ValueSpan[0];
                reader.Read();

                if (index == 110)
                    playlist.WithName(reader.GetString());

                if (index == 115)
                    playlist.WithTrack(reader.GetInt32());
            }

            response.PlaylistInfo = playlist;
        }

        private void BuildRestException(ref SearchResponse response, ref Utf8JsonReader reader)
        {
            var exception = new RestException();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                var index = reader.ValueSpan[0];
                reader.Read();

                if (index == 109)
                    exception.WithMessage(reader.GetString());

                if (index == 115)
                    exception.WithSeverity(reader.GetString());
            }

            response.Exception = exception;
        }

        private void BuildTracksList(ref SearchResponse response, ref Utf8JsonReader reader)
        {
            var set = new HashSet<LavaTrack>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType != JsonTokenType.StartObject)
                    continue;

                var track = new LavaTrack();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType == JsonTokenType.PropertyName
                        && reader.ValueTextEquals("track")
                        && reader.Read())
                        track.WithHash(reader.GetString());

                    if (reader.TokenType != JsonTokenType.StartObject)
                        continue;

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                            break;

                        if (reader.TokenType != JsonTokenType.PropertyName)
                            continue;

                        if (reader.ValueTextEquals("identifier"))
                        {
                            reader.Read();
                            track.WithId(reader.GetString());
                        }
                        else if (reader.ValueTextEquals("isSeekable"))
                        {
                            reader.Read();
                            track.WithSeek(reader.GetBoolean());
                        }
                        else if (reader.ValueTextEquals("author"))
                        {
                            reader.Read();
                            track.WithAuthor(reader.GetString());
                        }
                        else if (reader.ValueTextEquals("length"))
                        {
                            reader.Read();
                            track.WithDuration(reader.GetInt64());
                        }
                        else if (reader.ValueTextEquals("isStream"))
                        {
                            reader.Read();
                            track.WithStream(reader.GetBoolean());
                        }
                        else if (reader.ValueTextEquals("title"))
                        {
                            reader.Read();
                            track.WithTitle(reader.GetString());
                        }
                        else if (reader.ValueTextEquals("uri"))
                        {
                            reader.Read();
                            track.WithUrl(reader.GetString());
                        }
                    }
                }

                set.Add(track);
            }

            response.Tracks = set;
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, SearchResponse value, JsonSerializerOptions options)
        {
        }
    }
}