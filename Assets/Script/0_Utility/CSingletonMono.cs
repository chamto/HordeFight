using UnityEngine;

public class CSingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T instance;

    /**
       Returns the instance of this singleton.
    */
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType(typeof(T));

                //20130804 chamto - 반복문에서 객체가 없을때 에러메시지 계속 호출되는것을 막는다.
                //if (instance == null)
                //{
                //    DebugWide.LogError("An instance of " + typeof(T) +
                //       " is needed in the scene, but there is none.");
                //}
            }

            return instance;
        }
    }
}