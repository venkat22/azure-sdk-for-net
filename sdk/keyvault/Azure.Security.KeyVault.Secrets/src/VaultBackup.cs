﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

using System.Text.Json;

namespace Azure.Security.KeyVault.Secrets
{
    internal class VaultBackup : IJsonDeserializable, IJsonSerializable
    {
        public byte[] Value { get; set; }

        void IJsonDeserializable.ReadProperties(JsonElement json)
        {
            if (json.TryGetProperty("value", out JsonElement value))
            {
                Value = Base64Url.Decode(value.GetString());
            }
        }

        void IJsonSerializable.WriteProperties(Utf8JsonWriter json)
        {
            json.WriteString("value", Base64Url.Encode(Value));
        }
    }
}
