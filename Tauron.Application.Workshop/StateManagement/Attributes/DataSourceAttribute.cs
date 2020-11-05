using System;
using JetBrains.Annotations;
using Tauron.Application.Workshop.StateManagement.DataFactorys;

namespace Tauron.Application.Workshop.StateManagement.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    [BaseTypeRequired(typeof(AdvancedDataSourceFactory))]
    public sealed class DataSourceAttribute : Attribute
    {
        
    }
}