﻿using Akka.Actor;
using ServiceManager.ProjectDeployment.Data;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.Master.Commands.Deployment.Repository;

namespace ServiceManager.ProjectDeployment.Build
{
    public sealed class BuildRequest
    {
        public Reporter Source { get; }

        public AppData AppData { get; }
        public RepositoryApi RepositoryApi { get; }

        public BuildRequest(Reporter source, AppData appData, RepositoryApi repositoryApi)
        {
            Source = source;
            AppData = appData;
            RepositoryApi = repositoryApi;
        }
    }
}