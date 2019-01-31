using System;
using System.Collections.Generic;
using System.Linq;

namespace Melomania
{
    public struct Error
    {
        public Error(IEnumerable<string> messages)
            : this(messages?.ToArray())
        {
        }

        public Error(params string[] messages)
        {
            Messages = messages ?? throw new ArgumentNullException("Tried to create an Error with a null message.");
            Date = DateTime.Now;
        }

        public DateTime Date { get; }
        public IReadOnlyList<string> Messages { get; }

        public static implicit operator Error(string message) =>
            new Error(message);

        public static implicit operator Error(string[] messages) =>
            new Error(messages);

        public static implicit operator Error(Error[] errors) =>
            errors.Aggregate(MergeErrors);

        public static implicit operator string(Error error) =>
                    error.ToString();

        public static Error MergeErrors(Error first, Error second) =>
            new Error(first.Messages.Concat(second.Messages));

        public override string ToString() =>
            string.Join($"{Environment.NewLine}", Messages);
    }
}