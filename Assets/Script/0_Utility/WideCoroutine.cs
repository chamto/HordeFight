using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

/************************************************************************/
/* 20130722 chamto - 유니티 코루틴의 MonoBehaviour 상에서만 쓸수 있는 제약 때문에 
 * 범용적으로 쓸 수 있는 코루틴 함수를 제작
 * 유니티코루틴 함수와 다른점 : 
 *    !!! 유니티코루틴
 *      유니티엔진외 사용불가
 *      MonoBehaviour 객체가 없이 사용불가
 *      (유니티코루틴은 유니티엔진상에서 싱글톤 MonoBehaviour 함수를 만들어 그객체에서 코루틴을 호출하여 범용적으로 사용하는 방법이 있음)
 *    !!! 범용코루틴
 *      유니티엔진외 씨샵서버에서도 사용가능 
 *      MonoBehaviour 객체가 없어도 사용가능
 *      유니티코루틴에 있는 일정시간뒤에 코루틴이 동작하는 기능이 없음(추가 구현 필요)
 *      코루틴을 동기적으로 사용할 수 있다. (유니티에디터 상에서는 돌아가나, 디바이스에 들어가면 동작하지 않음) <= 유니티에디터 상에서는 쓰면 안됨
 *      콜백 함수를 등록할수 있다. 코루틴의 처리가 끝나면 등록된 콜백함수가 자동 호출된다.
 *      유니티코루틴과 사용방법이 약간 다름 (인터페이스 통일화 작업 필요)   
 *      유니티코루틴의 자식코루틴에 대한 처리가 다름
 *   
/************************************************************************/


//*********** "코루틴 내부"에서 코루틴을 호출했을 때의 비교 ***********
//StartCoroutine(InnerCoroutionTest_2());
//1. 단순히 코루틴을 새로 등록하는 것임, 부모 코루틴과 상관없이 별개로 동작한다.

//yield return StartCoroutine(InnerCoroutionTest_2());
//2. 자식 코루틴을 만나면, 자식코루틴이 끝날때까지 자동 대기한다.

//yield return InnerCoroutionTest_2(); 
//3. 코루틴 수행이 안됨, 이렇게 호출하는 것에 어떠한 경고메시지도 안나오니 주의 필요

//**************************************************************************

//WideUseCoroutine.StartCoroutine(InnerCoroutionTest_2(),null,true);
//1. 단순히 코루틴을 새로 등록하는 것임, 부모 코루틴과 상관없이 별개로 동작한다.

//yield return StartCoroutine(InnerCoroutionTest_2(),null,true);
//2. 단순히 코루틴을 새로 등록하는 것임, 부모 코루틴과 상관없이 별개로 동작한다.

//yield return InnerCoroutionTest_2();
//3. 자식 코루틴을 만나면, 자식코루틴이 끝날때까지 자동 대기한다.
//**************************************************************************


public class WideCoroutine 
{
	public const bool MODE_SYNCHRONOUS = false;
	public const bool MODE_ASYNCHRONOUS = true;

    //-------------------------------------------------------------------------------------
    //                               씨샵에서의 코루틴 호출함수
    //-------------------------------------------------------------------------------------
    public delegate void coroutineCallBack(object param);
    private  List<coroutineJob> listCoroutine = new List<coroutineJob>();
    private  List<coroutineJob> listEndCoroutine = new List<coroutineJob>();
    //private Dictionary<UnityEngine.Coroutine, bool> dicCommonState = new Dictionary<Coroutine, bool>(); //Coroutine 반환형 객체의 상태를 공유하기 위한 자료구조 , 부모코루틴은 이상태에 따라 정지/진행을 하게 된다.

    /// <summary>
    ///  비동기 코루틴을 처리하기 위한 작업묶음 객체
    ///  코루틴 내부에서만 처리하는 객체이기 때문에 외부에서 접근할 필요가 없음
    /// </summary>
    private class coroutineJob
    {
        public Stack<IEnumerator> stackPrev = new Stack<IEnumerator>(); //전단계의 코루틴을 스택에 저장한다.
        public IEnumerator etorCurrentDepth = null;                     //현재 깊이의 코루틴
        public IEnumerator etorRoot = null;                             //최상위 코루틴(부모)
        public coroutineCallBack callBack = null;

        //유니티엔진용 내부코루틴을 처리하기 위한 객체
        //public UnityEngine.Coroutine innerUnityCoroutine = null;       

    }
//	public IEnumerator StartCoroutine (IEnumerator routine)
//	{
//		return routine;
//	}

	//동기 - 처리 끝날때까지 대기
	public void Start_Sync(IEnumerator routine, coroutineCallBack callBack = null, string strName = "")
	{
		this.Start(routine, callBack, MODE_SYNCHRONOUS, strName );
	}

	//비동기
	public void Start_Async(IEnumerator routine, coroutineCallBack callBack = null, string strName = "")
	{
		this.Start(routine, callBack, MODE_ASYNCHRONOUS, strName );
	}

    public void Start(IEnumerator routine, coroutineCallBack callBack = null, bool asynchronous = false, string strName = "")
    {
        if (null == routine) return;

        coroutineJob job = new coroutineJob();
        if (true == asynchronous)
        {   //********* 비동기 **********


            job.etorRoot = routine;
            job.etorCurrentDepth = routine;
            job.callBack = callBack;
            listCoroutine.Add(job);

        }
        else
        {   //********** 동기 **********

            //코루틴 수행
            job.etorRoot = routine;
            job.etorCurrentDepth = routine;
            //int coroutineCount = 0;

            //float startTime = 0f;
            //float endTime = 0f;
            //float timeElapsed = 0f;
            //while(true)
            int processCount = 1;
            for (int i = 0; i < processCount; i++) //chamto test , while로 무한대로 돌리면 유니티에디터가 정지상태가 됨
            {
                //Time 같은 유니티 전용함수를 쓰면 서버에서 컴파일이 안됨
                //startTime = Time.realtimeSinceStartup;
                //timeElapsed += (endTime - startTime);

                //---------------------------------------------------------------------------------------------------
                if (false == job.etorCurrentDepth.MoveNext())
                {   //******* 현재 코루틴 완료 *******

                    if (job.etorRoot == job.etorCurrentDepth)
                    {   //최상위 부모 코루틴으로 돌아왔을때  콜백호출후, 코루틴 처리를 마친다.

                        if (null != callBack)
                        {   //콜백요청이 있다면 처리해 준다.
                            callBack(job.etorCurrentDepth.Current);
                        }

                        //DebugWide.Log("현재 코루틴 완료" + "   " + strName);
                        break;
                    }
                    else
                    {
                        processCount++;
                        job.etorCurrentDepth = job.stackPrev.Pop();
                    }

                }
                //---------------------------------------------------------------------------------------------------
                else
                {   //******* 현재 코루틴 진행중 *******
                    processCount++;

                    //DebugWide.Log("현재 코루틴 진행중" + "   " + strName);
                    if (null != job.etorCurrentDepth.Current && job.etorCurrentDepth.Current is IEnumerator)
                    {
                        job.stackPrev.Push(job.etorCurrentDepth);
                        job.etorCurrentDepth = job.etorCurrentDepth.Current as IEnumerator;
                    }
                }//end else

                //System.Threading.Thread.Sleep(10); //while 문에서 CPU계속 점유하는 현상이 있어 넣어봄
                //---------------------------------------------------------------------------------------------------
                //endTime = Time.realtimeSinceStartup;
            }//end while

        }//end else
    }


    public  void Update()
    {
        //foreach (coroutineJob etor in listCoroutine)
        coroutineJob etor = null;
        for (int i = 0; i < listCoroutine.Count; i++)
        {
            etor = listCoroutine[i] as coroutineJob;
            if (null == etor)
            {
                //Debug.Log("WideUseCoroutine-Update cast Error+----------------");
                continue;
            }
            //---------------------------------------------------------------------------------------------------
            //자식코루틴까지 탐색하여 모두 진행시킨다.
            if (false == etor.etorCurrentDepth.MoveNext())
            {   //******* 현재 코루틴 완료 *******

                //DebugWide.Log("false == etor.etorCurrentDepth.MoveNext");

                if (etor.etorCurrentDepth == etor.etorRoot)
                {   //최상위 부모 코루틴이 완료되었다면, 완료된 코루틴이기 때문에 제거한다.
                    listEndCoroutine.Add(etor);
                    if (null != etor.callBack)
                    {
                        etor.callBack(etor.etorRoot.Current);
                    }
                }
                else
                {   //자식코루틴이 완료되었다면, 전 부모 코루틴으로 변경한다.
                    etor.etorCurrentDepth = etor.stackPrev.Pop();
                }
            }
            //---------------------------------------------------------------------------------------------------
            else
            {   //******* 현재 코루틴 진행중 *******

                //DebugWide.Log("true == etor.etorCurrentDepth.MoveNext : " + etor.etorCurrentDepth.Current + "  type :" + etor.etorCurrentDepth.Current.GetType() + "  :  "+typeof(IEnumerator));

                //IEnumerator 자식코루틴이 있는지 검사
                if (null != etor.etorCurrentDepth.Current && etor.etorCurrentDepth.Current is IEnumerator)
                {
                    //DebugWide.Log("자식 코루틴 있음 : " + etor.etorCurrentDepth.Current);

                    //자식 코루틴이 있다면, 현재코루틴을 스택에 넣고, 자식코루틴을 현재코루틴으로 한다.
                    etor.stackPrev.Push(etor.etorCurrentDepth);
                    etor.etorCurrentDepth = etor.etorCurrentDepth.Current as IEnumerator;
                }//end if

            }//end if
            //---------------------------------------------------------------------------------------------------


        }//end foreach


        //작업이 끝난 코루틴 모두 제거
        foreach (coroutineJob endEtor in listEndCoroutine)
        {
            listCoroutine.Remove(endEtor);
        }

        listEndCoroutine.Clear();
    }
}
