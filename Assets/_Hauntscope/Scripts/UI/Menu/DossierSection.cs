namespace Hauntscope.UI.Menu
{
    // One section of a Bestiary case file; a locked one is shown redacted with what it takes to open it.
    public readonly struct DossierSection
    {
        public DossierSection(string title, string body, bool isLocked, string lockHint)
        {
            Title = title;
            Body = body;
            IsLocked = isLocked;
            LockHint = lockHint;
        }

        public string Title { get; }

        public string Body { get; }

        public bool IsLocked { get; }

        public string LockHint { get; }
    }
}
