namespace ServiceHost.Installer
{
    public sealed class InstallerationCompled
    {
        public bool Succesfull { get; }
        public string Error { get; }

        public InstallerationCompled(bool succesfull, string error)
        {
            Succesfull = succesfull;
            Error = error;
        }
    }
}