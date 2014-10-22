namespace System
{
#if NETFX_CORE || WINDOWS_PHONE
    public interface ICloneable
    {
        object Clone();
    }
#endif
}
