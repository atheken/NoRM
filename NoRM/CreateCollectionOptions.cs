namespace NoRM
{
    public class CreateCollectionOptions
    {
        public string Name{ get; private set;}
        public bool Capped{ get; set;}
        public int Size{ get; set;}
        public long? Max { get; set; }
        public bool AutoIndex{ get; set;}

        public CreateCollectionOptions(string name)
        {
            Name = name;
            Capped = true;
            Size = 100000;
            AutoIndex = false;
        }
    }
}