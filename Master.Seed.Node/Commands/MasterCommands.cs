using Petabridge.Cmd;

namespace Master.Seed.Node.Commands
{
    public static class MasterCommands
    {
        public static CommandDefinition Kill = 
            new CommandDefinitionBuilder()
               .WithDescription("Kills Compled Cluster")
               .WithName("kill")
               .Build();

        public static CommandPalette MasterPalette = new CommandPalette("master", 
            new []{ Kill });
    }
}