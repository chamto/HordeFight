using System;
using System.Collections.Generic;
using UnityEngine;

namespace Buckland
{
    //ref : https://www.codeproject.com/Articles/8531/Using-generics-for-calculations
    public interface ICalculator<T1> 
    { 
        T1 Add(T1 a,T1 b); 
        T1 Divide(T1 a,float b);

    } 

    public struct FloatCalc : ICalculator<float> 
    { 
        public float Add(float a,float b) { return a+b; } 
        public float Divide(float a,float b) { return a/b; } 

    }

    public struct Vectoc2Calc : ICalculator<Vector2>
    {
        public Vector2 Add(Vector2 a, Vector2 b) { return a + b; }
        public Vector2 Divide(Vector2 a, float b) { return a / b; }

    }

    //public class SmootherType
    //{
    //    public static Smoother<float, FloatCalc> CreateFloat(int SampleSize, float ZeroValue)
    //    {
    //        return new Smoother<float, FloatCalc>(SampleSize, ZeroValue);
    //    }

    //    public static Smoother<float, Vectoc2Calc> CreateVector2(int SampleSize, Vector2 ZeroValue)
    //    {
    //        return new Smoother<float, Vectoc2Calc>(SampleSize, ZeroValue);
    //    }
    //}

    public class Smoother<T1, C>
        where C : ICalculator<T1> , new()
    {

        //this holds the history
        List<T1> m_History;

        int m_iNextUpdateSlot;

        //an example of the 'zero' value of the type to be smoothed. This
        //would be something like Vector2D(0,0)
        T1 m_ZeroValue;

        private C calculator = new C();

        //to instantiate a Smoother pass it the number of samples you want
        //to use in the smoothing, and an exampe of a 'zero' type
        public Smoother(int SampleSize, T1 ZeroValue)
        {
            
            m_History = new List<T1>(SampleSize);
            for (int i = 0; i < SampleSize; i++)
            {
                m_History.Add(ZeroValue);
            }
            //DebugWide.LogBlue(SampleSize + "  " + m_History.Count);

            m_ZeroValue = ZeroValue;
            m_iNextUpdateSlot = 0;
        }

        //each time you want to get a new average, feed it the most recent value
        //and this method will return an average over the last SampleSize updates
        public T1 Update(T1 MostRecentValue)
        {
            if (0 == m_History.Count) return default(T1);
            
            m_iNextUpdateSlot++;
            m_iNextUpdateSlot %= m_History.Count; 

            //overwrite the oldest value with the newest
            m_History[m_iNextUpdateSlot] = MostRecentValue;

            //make sure m_iNextUpdateSlot wraps around. 
            //if (m_iNextUpdateSlot == m_History.Count) m_iNextUpdateSlot = 0;

            //now to calculate the average of the history list
            //bool test = false;
            T1 sum = m_ZeroValue;
            for (int i = 0; i < m_History.Count; i++)
            {
                //if (typeof(T) == typeof(Vector2))
                //    test = true;
                //else if (typeof(T) == typeof(float))
                //test = true;

                //sum = sum + m_History[i];

                sum = calculator.Add(sum, m_History[i]);
            }

            //return sum / (float)m_History.Count;
            return calculator.Divide(sum, (float)m_History.Count);
        }
    }
}

 
