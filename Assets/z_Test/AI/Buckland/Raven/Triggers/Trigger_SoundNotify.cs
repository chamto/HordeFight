using Buckland;

namespace Raven
{
    ///////////////////////////////////////////////////////////////////////////////

    public class Trigger_SoundNotify : Trigger_LimitedLifetime<Raven_Bot>
    {

        //a pointer to the bot that has made the sound
        Raven_Bot m_pSoundSource;


        public Trigger_SoundNotify(Raven_Bot source,
                                     float range) : base((int)(Const.FrameRate / Params.Bot_TriggerUpdateFreq))
        {
            m_pSoundSource = source;

            //set position and range
            SetPos(m_pSoundSource.Pos());

            SetBRadius(range);

            //create and set this trigger's region of fluence
            AddCircularTriggerRegion(Pos(), BRadius());
        }


        override public void Try(Raven_Bot pBot)
        {
            //is this bot within range of this sound
            if (isTouchingTrigger(pBot.Pos(), pBot.BRadius()))
            {

                MessageDispatcher.Instance().DispatchMsg(Const.SEND_MSG_IMMEDIATELY,
                                        Const.SENDER_ID_IRRELEVANT,
                                        pBot.ID(),
                                        (int)eMsg.GunshotSound,
                                        m_pSoundSource);
            }
        }

        override public void Render() { }

    }

}


