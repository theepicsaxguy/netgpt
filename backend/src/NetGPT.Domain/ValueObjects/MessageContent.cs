// <copyright file="MessageContent.cs" company="NetGPT">
// Copyright (c) NetGPT. All rights reserved.
// Repo owner: theepicsaxguy
// </copyright>

namespace NetGPT.Domain.ValueObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public record MessageContent
    {
        public string Text { get; init; }

        public IReadOnlyList<Attachment> Attachments { get; init; }

        public MessageContent(string text, IEnumerable<Attachment>? attachments = null)
        {
            if (string.IsNullOrWhiteSpace(text) && (attachments == null || !attachments.Any()))
            {
                throw new ArgumentException("Message must have text or attachments");
            }

            this.Text = text ?? string.Empty;
            this.Attachments = attachments?.ToList() ?? [];
        }

        public static MessageContent FromText(string text)
        {
            return new(text);
        }
    }
}
