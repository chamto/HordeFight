
public class CSingleton<T> where T : new()
{
    private static T instance;
    private CSingleton() { }

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }

            return instance; 
        }
    }
}