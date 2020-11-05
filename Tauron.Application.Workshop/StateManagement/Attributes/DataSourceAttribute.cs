using System;
using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.DataFactorys;

namespace Tauron.Application.Workshop.StateManagement.Attributes
{
    [PublicAPI]
    [MeansImplicitUse]
    [BaseTypeRequired(typeof(AdvancedDataSourceFactory))]
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DataSourceAttribute : Attribute
    {
        
    }
}