﻿using JetBrains.Annotations;

namespace Tauron.Application.Workshop.StateManagement
{
    [PublicAPI]
    public sealed class AutofacOptions
    {
        public static readonly AutofacOptions Default = new();

        public bool ResolveEffects { get; set; } = true;

        public bool ResolveMiddleware { get; set; } = true;

        public bool RegisterSuperviser { get; set; } = true;

        public bool AutoRegisterInContainer { get; set; } = true;

    }
}