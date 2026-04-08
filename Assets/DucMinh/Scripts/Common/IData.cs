namespace DucMinh
{
    public interface IData : ISerialize, IDeserialize
    {
        
    }

    public interface ISerialize
    {
        string Serialize();
    }

    public interface IDeserialize
    {
        void Deserialize(string json);
    }
}