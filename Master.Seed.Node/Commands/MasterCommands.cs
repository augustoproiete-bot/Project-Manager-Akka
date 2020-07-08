using Petabridge.Cmd;

namespace Master.Seed.Node.Commands
{
    public static class MasterCommands
    {
        public static readonly CommandDefinition Kill = 
            new CommandDefinitionBuilder()
               .WithDescription("Kills Compled Cluster")
               .WithName("kill")
               .Build();

        public static readonly CommandDefinition ListServices =
            new CommandDefinitionBuilder()
                .WithDescription("List All Registrated Services")
                .WithName("ListServices")
                .Build();

        public static readonly CommandPalette MasterPalette = new CommandPalette("master", 
            new []{ Kill });
    }
}