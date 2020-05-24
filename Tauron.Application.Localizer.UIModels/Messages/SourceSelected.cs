using System;
using Tauron.Application.Localizer.UIModels.Views;

namespace Tauron.Application.Localizer.UIModels.Messages
{
    public sealed class SourceSelected
    {
        public string? Source { get; }
        public OpenFileMode Mode { get; }

        public SourceSelected(string? source, OpenFileMode mode)
        {
            Source = source;
            Mode = mode;
        }

        public static Func<SourceSelected> From(Func<string?> data, OpenFileMode mode)
            => () => new SourceSelected(data(), mode);
    }
}