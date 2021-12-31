using System;

namespace Proto_AI_4
{
    public struct Telegram : IEquatable<Telegram>, IComparable<Telegram>
    {

        public const float SEND_MSG_IMMEDIATELY = 0.0f;

        //the entity that sent this telegram
        public int Sender;

        //the entity that is to receive this telegram
        public int Receiver;

        //the message itself. These are all enumerated in the file
        //"MessageTypes.h"
        public int Msg;

        //messages can be dispatched immediately or delayed for a specified amount
        //of time. If a delay is necessary this field is stamped with the time 
        //the message should be dispatched.
        public float DispatchTime;

        //any additional information that may accompany the message
        public object ExtraInfo;


        public void Init()
        {
            Sender = -1;
            Receiver = -1;
            Msg = -1;
            DispatchTime = -1f;
        }

        //these telegrams will be stored in a priority queue. Therefore the >
        //operator needs to be overloaded so that the PQ can sort the telegrams
        //by time priority. Note how the times must be smaller than
        //SmallestDelay apart before two Telegrams are considered unique.
        const float SmallestDelay = 0.25f;
        public bool Equals(Telegram other)
        {
            return (Math.Abs(DispatchTime - other.DispatchTime) < SmallestDelay) &&
              (Sender == other.Sender) &&
              (Receiver == other.Receiver) &&
              (Msg == other.Msg);
        }

        //https://docs.microsoft.com/en-us/dotnet/api/system.int32.compareto?view=net-5.0
        public int CompareTo(Telegram other)
        {
            if (DispatchTime < other.DispatchTime)
            {
                return -1;
            }
            else if(DispatchTime > other.DispatchTime)
            {
                return 1;
            }

            return 0; //같은경우 , Equals 로 비교한것이 아니기 떄문에 완전 같은지는 모름 
        }

        public Telegram(float time,
                 int sender,
                 int receiver,
                 int msg,
                 object exInfo)
        {
            Sender = sender;
            Receiver = receiver;
            Msg = msg;
            DispatchTime = time;
            ExtraInfo = exInfo;
        }

    }
}

