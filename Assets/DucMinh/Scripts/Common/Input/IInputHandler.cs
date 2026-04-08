namespace DucMinh
{
    public interface IInputHandler<THit>
    {
        bool OnTap(THit data);
        bool OnDrag(THit data);
        void OnRelease(THit data);
    }
}