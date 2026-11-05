namespace KGallery.Abstractions
{
    public interface IOpenDialog<TParameter>
    {
        void Open(TParameter parameter);
    }
}
