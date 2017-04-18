using YTII.ModelFactory.Models;

namespace YTII.Droid.App
{
    interface IVideoModelCache<T>
        where T : IVideoModel
    {
        void Add(T item);

        bool IsCached(string videoId);

        T GetItem(string videoId);
    }
}