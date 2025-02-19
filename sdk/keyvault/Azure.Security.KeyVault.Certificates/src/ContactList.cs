﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for
// license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Azure.Security.KeyVault.Certificates
{
    internal class ContactList : IJsonDeserializable, IJsonSerializable
    {
        private IEnumerable<Contact> _contacts;

        public ContactList()
        {

        }

        public ContactList(IEnumerable<Contact> contacts)
        {
            _contacts = contacts;
        }

        public IList<Contact> ToList()
        {
            IList<Contact> ret = _contacts as IList<Contact>;

            if (ret == null)
            {
                ret = _contacts.ToList();

                _contacts = ret;
            }

            return ret;
        }

        private const string ContactsPropertyName = "contacts";

        void IJsonDeserializable.ReadProperties(JsonElement json)
        {
            var contacts = new List<Contact>();

            if(json.TryGetProperty(ContactsPropertyName, out JsonElement contactsElement))
            {
                foreach(JsonElement entry in contactsElement.EnumerateArray())
                {
                    var contact = new Contact();

                    ((IJsonDeserializable)contact).ReadProperties(entry);

                    contacts.Add(contact);
                }
            }

            _contacts = contacts;
        }

        private static readonly JsonEncodedText ContactsPropertyNameBytes = JsonEncodedText.Encode(ContactsPropertyName);

        void IJsonSerializable.WriteProperties(Utf8JsonWriter json)
        {
            if(_contacts != null)
            {
                json.WriteStartArray(ContactsPropertyNameBytes);

                foreach(var contact in _contacts)
                {
                    json.WriteStartObject();

                    ((IJsonSerializable)contact).WriteProperties(json);

                    json.WriteEndObject();
                }

                json.WriteEndArray();
            }
        }
    }
}
