namespace RimArchive;

public class BiDirectionDic<T>  where T : class, new()
{
    private Dictionary<T, T> _dictionary = new Dictionary<T, T>();

    public void Add(T key, T value)
    {
        _dictionary.Add(key, value);
        _dictionary.Add(value, key);
    }

    public bool TryGetValue(T key, out T value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public void Remove(T key)
    {
        if (_dictionary.TryGetValue(key, out T value))
        {
            _dictionary.Remove(key);
            _dictionary.Remove(value);
        }
    }
}
