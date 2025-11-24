// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace NetGPT.Domain.ValueObjects
{
    public record MessageContent
    {
        public MessageContent(string text, IEnumerable<Attachment>? attachments = null)
        {
            if (string.IsNullOrWhiteSpace(text) && (attachments == null || !attachments.Any()))
            {
                throw new ArgumentException("Message must have text or attachments");
            }

            Text = text ?? string.Empty;
            Attachments = attachments?.ToList() ?? [];
        }

        public string Text { get; init; }

        public IReadOnlyList<Attachment> Attachments { get; init; }

        public static MessageContent FromText(string text)
        {
            return new(text);
        }
    }
}
