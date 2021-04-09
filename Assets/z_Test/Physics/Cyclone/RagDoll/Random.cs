using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Cyclone
{
    /**
     * Keeps track of one random stream: i.e. a seed and its output.
     * This is used to get random numbers. Rather than a funcion, this
     * allows there to be several streams of repeatable random numbers
     * at the same time. Uses the RandRotB algorithm.
     */
    public class Random
    {
        // Internal mechanics
        int p1, p2;
        uint[] buffer = new uint[17];

        Stopwatch stopWatch = Stopwatch.StartNew(); //new Stopwatch();

        static Random _sRandom = new Random(); 
        public static Random GetI()
        {
            return _sRandom;
        }

        /**
         * Creates a new random number stream with a seed based on
         * timing data.
         */
        public Random()
        {
            seed(0);
        }

        /**
         * Creates a new random stream with the given seed.
         */
        public Random(uint s)
        {
            seed(s);
        }

        /**
         * Sets the seed value for the random stream.
         */
        public void seed(uint s)
        {
            if (s == 0)
            {
                //s = (uint)clock(); //ms단위로 반환 (함수설명에는 tick이지만 ms로 반환한다) 
                s = (uint)stopWatch.ElapsedMilliseconds;
            }

            // Fill the buffer with some basic random numbers
            for (uint i = 0; i < 17; i++)
            {
                // Simple linear congruential generator
                s = s * 2891336453 + 1;
                buffer[i] = s;
            }

            // Initialize pointers into the buffer
            p1 = 0; p2 = 10;
        }

        /**
         * Returns the next random bitstring from the stream. This is
         * the fastest method.
         */
        //bit rotate : _lrotl
        //ref : https://docs.microsoft.com/en-us/cpp/c-runtime-library/reference/lrotl-lrotr?view=msvc-160
        //ref : https://mycsharp.de/forum/threads/26720/bit-rotation-in-csharp?page=1#forumpost-145733
        public uint randomBits()
        {
            uint result;

            // Rotate the buffer and store it back to itself
            //result = buffer[p1] = _lrotl(buffer[p2], 13) + _lrotl(buffer[p1], 9);
            uint bL13  = ((buffer[p2] << 13) | (buffer[p2] >> (32 - 13)));
            uint bL9 = ((buffer[p1] << 9) | (buffer[p1] >> (32 - 9)));
            result = buffer[p1] = bL13 + bL9;

            // Rotate pointers
            if (--p1 < 0) p1 = 16;
            if (--p2 < 0) p2 = 16;

            // Return result
            return result;
        }

        //ref : https://wvbiz.tistory.com/entry/C-%EA%B0%95%EC%A2%8C-C-%ED%94%84%EB%A1%9C%EA%B7%B8%EB%9E%98%EB%B0%8D-9-%EA%B5%AC%EC%A1%B0%EC%B2%B4
        [StructLayout(LayoutKind.Explicit)]
        public struct Union_1
        {

            [FieldOffset(0)]    //메모리내 시작 위치
            public double value;

            [FieldOffset(4)]    //0에서 시작 //윈도우와 유닉스간의 비트읽는 순서가 달라 이렇게 하드코딩하면 안될것 같음. 원래 코드에서는 0이었으나 정상동작 안되어 4에서 시작되게 함 
            public uint word;

        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Union_2
        {

            [FieldOffset(0)]    //메모리내 시작 위치
            public double value;

            [FieldOffset(0)]    //0에서 시작
            public uint words_0;
            [FieldOffset(4)]    
            public uint words_1;

        }

        /**
         * Returns a random floating point number between 0 and 1.
         */
        public double randomReal_1()
        {
            // Get the random number
            uint bits = randomBits();

            // Set up a reinterpret structure for manipulation
            //union {
            //    real value;
            //    unsigned word;
            //}
            //convert;
            Union_1 convert = new Union_1();

            // Now assign the bits to the word. This works by fixing the ieee
            // sign and exponent bits (so that the size of the result is 1-2)
            // and using the bits to create the fraction part of the float.
            convert.word = (bits >> 9) | 0x3f800000;

            //DebugWide.LogBlue(UtilGS9.Misc.IntToBinaryString((int)bits) + " ==== 1");
            //DebugWide.LogBlue(UtilGS9.Misc.IntToBinaryString((int)(bits >> 9)) + " ==== 2");
            //DebugWide.LogBlue(UtilGS9.Misc.IntToBinaryString((int)(0x3f800000)) + " ==== 3");
            //DebugWide.LogBlue(UtilGS9.Misc.IntToBinaryString(-1) + " ==== 4");
            //DebugWide.LogBlue(UtilGS9.Misc.IntToBinaryString((int)convert.word) + " ==== 5");
            //DebugWide.LogBlue(UtilGS9.Misc.DoubleToBinaryString(convert.value) + " ==== 6");
            //DebugWide.LogBlue(UtilGS9.Misc.DoubleToBinaryString(-1) + " ==== 7");
            //DebugWide.LogBlue(UtilGS9.Misc.DoubleToBinaryString(convert.value - 1.0) + " ==== 8");
            // And return the value
            return convert.value - 1.0;
        }

        public double randomReal_2()
        {
            // Get the random number
            uint bits = randomBits();

            // Set up a reinterpret structure for manipulation
            //union {
            //    real value;
            //    unsigned words[2];
            //}
            //convert;
            Union_2 convert = new Union_2();

            // Now assign the bits to the words. This works by fixing the ieee
            // sign and exponent bits (so that the size of the result is 1-2)
            // and using the bits to create the fraction part of the float. Note
            // that bits are used more than once in this process.
            convert.words_0 = bits << 20; // Fill in the top 16 bits
            convert.words_1 = (bits >> 12) | 0x3FF00000; // And the bottom 20

            // And return the value
            return convert.value - 1.0;
        }

        //randomBits 가 정상동작 안되어 씨샵함수 사용함  
        private static System.Random rand = new System.Random();
        public double randomReal()
        {
            return ((float)rand.Next() / (float)(int.MaxValue));
        }

        /**
         * Returns a random floating point number between 0 and scale.
         */
        public double randomReal(double scale)
        {
            return randomReal() * scale;
        }

        /**
         * Returns a random floating point number between min and max.
         */
        public double randomReal(double min, double max)
        {
            return randomReal() * (max - min) + min;
        }

        /**
         * Returns a random integer less than the given value.
         */
        public uint randomInt(uint max)
        {
            return randomBits() % max;
        }

        /**
         * Returns a random binomially distributed number between -scale
         * and +scale.
         */
        public double randomBinomial(double scale)
        {
            return (randomReal() - randomReal()) * scale;
        }

        /**
         * Returns a random vector where each component is binomially
         * distributed in the range (-scale to scale) [mean = 0.0f].
         */
        public Vector3 randomVector(double scale)
        {
            return new Vector3(
                (float)randomBinomial(scale),
                (float)randomBinomial(scale),
                (float)randomBinomial(scale)
                );
        }

        /**
         * Returns a random vector where each component is binomially
         * distributed in the range (-scale to scale) [mean = 0.0f],
         * where scale is the corresponding component of the given
         * vector.
         */
        public Vector3 randomVector( Vector3 scale)
        {
            return new Vector3(
                (float)randomBinomial(scale.x),
                (float)randomBinomial(scale.y),
                (float)randomBinomial(scale.z)
                );
        }

        /**
         * Returns a random vector in the cube defined by the given
         * minimum and maximum vectors. The probability is uniformly
         * distributed in this region.
         */
        public Vector3 randomVector(Vector3 min, Vector3 max)
        {
            return new Vector3(
                (float)randomReal(min.x, max.x),
                (float)randomReal(min.y, max.y),
                (float)randomReal(min.z, max.z)
                );
        }

        /**
         * Returns a random vector where each component is binomially
         * distributed in the range (-scale to scale) [mean = 0.0f],
         * except the y coordinate which is zero.
         */
        public Vector3 randomXZVector(double scale)
        {
            return new Vector3(
                (float)randomBinomial(scale),
                0,
                (float)randomBinomial(scale)
                );
        }

        /**
         * Returns a random orientation (i.e. normalized) quaternion.
         */
        public Quaternion randomQuaternion()
        {
            Quaternion q = new Quaternion(
                (float)randomReal(),
                (float)randomReal(),
                (float)randomReal(),
                (float)randomReal()
        
                );
            q.normalise();
            return q;
        }

    }
}
