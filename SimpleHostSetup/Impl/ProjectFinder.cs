using System;
using Serilog;

namespace SimpleHostSetup.Impl
{
    public sealed class ProjectFinder
    {
        private readonly ILogger _logger = Log.ForContext<ProjectFinder>();

        private readonly string _searchStart;
        private string? _root;

        public ProjectFinder(string? searchStart)
        {
            if (string.IsNullOrWhiteSpace(searchStart))
                throw new ArgumentNullException(nameof(searchStart), "No Search Path Provided");

            _searchStart = searchStart;
        }

        public string Search(string name)
        {
            _logger.

            if (string.IsNullOrWhiteSpace(_root))
            {

            }
        }
    }
}