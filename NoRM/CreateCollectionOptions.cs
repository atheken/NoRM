namespace NoRM
{
    public class CreateCollectionOptions
    {
        public string Name{ get; set;}
        public bool Capped{ get; set;}
        public int Size{ get; set;}
        public long? Max { get; set; }
        public bool AutoIndexId{ get; set;}
        public string Create { get; set; }

        public CreateCollectionOptions(){}
        public CreateCollectionOptions(string name)
        {
            Name = name;
            Capped = true;
            Size = 100000;
            AutoIndexId = false;
        }
    }
}