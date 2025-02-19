﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Core.Http;
using Azure.Core.Pipeline;

namespace Azure.Core.Testing
{
    public class MockRequest : Request
    {
        public MockRequest()
        {
            ClientRequestId = Guid.NewGuid().ToString();
        }

        private readonly Dictionary<string, List<string>> _headers = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        public override HttpPipelineRequestContent Content
        {
            get { return base.Content; }
            set
            {
                if (value != null && value.TryComputeLength(out long length))
                {
                    _headers["Content-Length"] = new List<string> { length.ToString() };

                }
                else
                {
                    _headers.Remove("Content-Length");
                }
                base.Content = value;
            }
        }


#if HAS_INTERNALS_VISIBLE_CORE
        internal
#endif
        protected override void AddHeader(string name, string value)
        {
            if (!_headers.TryGetValue(name, out List<string> values))
            {
                _headers[name] = values = new List<string>();
            }

            values.Add(value);
        }

#if HAS_INTERNALS_VISIBLE_CORE
        internal
#endif
        protected override bool TryGetHeader(string name, out string value)
        {
            if (_headers.TryGetValue(name, out List<string> values))
            {
                value = JoinHeaderValue(values);
                return true;
            }

            value = null;
            return false;
        }

#if HAS_INTERNALS_VISIBLE_CORE
        internal
#endif
        protected override bool TryGetHeaderValues(string name, out IEnumerable<string> values)
        {
            var result = _headers.TryGetValue(name, out List<string> valuesList);
            values = valuesList;
            return result;
        }

#if HAS_INTERNALS_VISIBLE_CORE
        internal
#endif
        protected override bool ContainsHeader(string name)
        {
            return TryGetHeaderValues(name, out _);
        }

#if HAS_INTERNALS_VISIBLE_CORE
        internal
#endif
        protected override bool RemoveHeader(string name)
        {
            return _headers.Remove(name);
        }

#if HAS_INTERNALS_VISIBLE_CORE
        internal
#endif
        protected override IEnumerable<HttpHeader> EnumerateHeaders() => _headers.Select(h => new HttpHeader(h.Key, JoinHeaderValue(h.Value)));

        private static string JoinHeaderValue(IEnumerable<string> values)
        {
            return string.Join(",", values);
        }

        public override string ClientRequestId { get; set; }

        public override string ToString() => $"{Method} {UriBuilder}";

        public override void Dispose()
        {
        }
    }
}
