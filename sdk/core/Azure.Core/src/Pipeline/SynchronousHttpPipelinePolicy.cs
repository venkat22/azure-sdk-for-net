﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;

namespace Azure.Core.Pipeline
{
    public abstract class SynchronousHttpPipelinePolicy : HttpPipelinePolicy
    {
        public override void Process(HttpPipelineMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
        {
            OnSendingRequest(message);
            ProcessNext(message, pipeline);
            OnReceivedResponse(message);
        }

        public override async Task ProcessAsync(HttpPipelineMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
        {
            OnSendingRequest(message);
            await ProcessNextAsync(message, pipeline).ConfigureAwait(false);
            OnReceivedResponse(message);
        }

        public virtual void OnSendingRequest(HttpPipelineMessage message) { }

        public virtual void OnReceivedResponse(HttpPipelineMessage message) { }
    }
}
