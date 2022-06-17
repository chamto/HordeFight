using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UtilGS9
{

    // 2008년 루나 이웅주 제작
    public class MemoryPool<Data>  where Data : new()
    {

        struct Setting
        {
            public uint BaseSize;
            public uint IncreaseSize;
            public string Name;

            //public Setting()
            //{
            //    memset(this, 0, sizeof( *this) );
            //}
        }

        HashSet<Data> _AllocatedDataSet = new HashSet<Data>();
        HashSet<Data> _ReservedDataSet = new HashSet<Data>();
        LinkedList<Data> _MemoryList = new LinkedList<Data>();
        Setting _Setting = new Setting();

        public MemoryPool() { }


        public void Release()
        {
            _MemoryList.Clear();
            _ReservedDataSet.Clear();
            _AllocatedDataSet.Clear();
        }

        public void Init(uint baseSize, uint increaseSize, string name )
        {
            _Setting.BaseSize        = baseSize;
            _Setting.IncreaseSize    = increaseSize;
            _Setting.Name = name;


            Increase(baseSize );
        }

        public void Free(Data info )
        {
            Deallocate(info);
        }

        public Data Alloc()
        {
            return Allocate();
        }


        private void Increase(uint size)
        {
            for(int i=0;i<size;i++)
            {
                Data data = new Data();
                _MemoryList.AddLast(data);
                _ReservedDataSet.Add(data); //뒤에서 부터 넣을 이유가 없음 
            }

        }

        private Data Allocate()
        {
            if(0 == _ReservedDataSet.Count )
            {
                Increase(_Setting.IncreaseSize );

                // 081117 LUJ, 용량을 늘렸는데도 할당가능 공간이 없으면 진행불가 
                if(0 == _ReservedDataSet.Count )
                {
                    return default(Data);
                }
            }

            //열거형의 첫번째값을 가져오기 위해 foreach 사용 
            Data data = default(Data);
            foreach(Data get in _ReservedDataSet)
            {
                data = get; break;
            }
            _AllocatedDataSet.Add(data);
            _ReservedDataSet.Remove(data);

            return data;
        }

        private  void Deallocate(Data data )
        {
            if (_AllocatedDataSet.Remove(data))
            {
                _ReservedDataSet.Add(data);
            }
        }

        private bool IsAllocated(Data data)
        {
            return _AllocatedDataSet.Contains(data);
        }

        private bool IsReserved(Data data)
        {
            return _ReservedDataSet.Contains(data);
        }


    }

    //======================================================

    //* 문제있는 방식 , 자원해제시 단편화가 발생 , 할당객체가 모자르면 밴드를 생성해서 붙인다 , 밴드가 계속 불어나게 된다 
    //안좋은 알고리즘으로 제작된 메모리풀이기 때문에 사용안한다. 이하 소스 제거
    //public class CBand<DataType> where DataType : new(){}
    //public class CMemoryPoolTempl<Type>{}


}//end namespace

