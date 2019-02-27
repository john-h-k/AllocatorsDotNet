namespace AllocatorsDotNet
{
    public interface IFixable<T> : IPinnableRef<T> where T : unmanaged { }

    public interface IPinnableRef<T>
    {
        ref T GetPinnableReference();
    }
}