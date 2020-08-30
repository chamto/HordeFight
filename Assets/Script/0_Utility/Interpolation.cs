using UnityEngine;

namespace UtilGS9
{
	public class Interpolation
	{

        public enum eKind
        {
            linear,
            clerp,
            spring,
            punch,

            easeInQuad,
            easeOutQuad,
            easeInOutQuad,

            easeInCubic,
            easeOutCubic,
            easeInOutCubic,

            easeInQuart,
            easeOutQuart,
            easeInOutQuart,

            easeInSine,
            easeOutSine,
            easeInOutSine,

            easeInExpo,
            easeOutExpo,
            easeInOutExpo,

            easeInCirc,
            easeOutCirc,
            easeInOutCirc,

            easeInBounce,
            easeOutBounce,
            easeInOutBounce,

            easeInBack,
            easeOutBack,
            easeInOutBack,

            easeInElastic,
            easeOutElastic,
            easeInOutElastic,
        }


        static public float Calc(eKind kind, float start, float end, float value)
        {
            float rate = 0;
            switch(kind)
            {
                case eKind.linear:
                    return linear(start, end, value);
                case eKind.clerp:
                    return clerp(start, end, value);
                case eKind.spring:
                    return spring(start, end, value);
                case eKind.punch:
                    return punch(end, value); //end를 진폭값으로 사용 

                case eKind.easeInQuad:
                    return easeInQuad(start, end, value);
                case eKind.easeOutQuad:
                    return easeOutQuad(start, end, value);
                case eKind.easeInOutQuad:
                    return easeInOutQuad(start, end, value);

                case eKind.easeInCubic:
                    return easeInCubic(start, end, value);
                case eKind.easeOutCubic:
                    return easeOutCubic(start, end, value);
                case eKind.easeInOutCubic:
                    return easeInOutCubic(start, end, value);

                case eKind.easeInQuart:
                    return easeInQuart(start, end, value);
                case eKind.easeOutQuart:
                    return easeOutQuart(start, end, value);
                case eKind.easeInOutQuart:
                    return easeInOutQuart(start, end, value);

                case eKind.easeInSine:
                    return easeInSine(start, end, value);
                case eKind.easeOutSine:
                    return easeOutSine(start, end, value);
                case eKind.easeInOutSine:
                    return easeInOutSine(start, end, value);

                case eKind.easeInExpo:
                    return easeInExpo(start, end, value);
                case eKind.easeOutExpo:
                    return easeOutExpo(start, end, value);
                case eKind.easeInOutExpo:
                    return easeInOutExpo(start, end, value);

                case eKind.easeInCirc:
                    return easeInCirc(start, end, value);
                case eKind.easeOutCirc:
                    return easeOutCirc(start, end, value);
                case eKind.easeInOutCirc:
                    return easeInOutCirc(start, end, value);

                case eKind.easeInBounce:
                    return easeInBounce(start, end, value);
                case eKind.easeOutBounce:
                    return easeOutBounce(start, end, value);
                case eKind.easeInOutBounce:
                    return easeInOutBounce(start, end, value);

                case eKind.easeInBack:
                    return easeInBack(start, end, value);
                case eKind.easeOutBack:
                    return easeOutBack(start, end, value);
                case eKind.easeInOutBack:
                    return easeInOutBack(start, end, value);

                case eKind.easeInElastic:
                    return easeInElastic(start, end, value);
                case eKind.easeOutElastic:
                    return easeOutElastic(start, end, value);
                case eKind.easeInOutElastic:
                    return easeInOutElastic(start, end, value);
            }

            return rate;
        }



		static public float linear(float start, float end, float value){
			return Mathf.Lerp(start, end, value);
		}

		static public float clerp(float start, float end, float value){
			float min = 0.0f;
			float max = 360.0f;
			float half = Mathf.Abs((max - min) / 2.0f);
			float retval = 0.0f;
			float diff = 0.0f;
			if ((end - start) < -half){
				diff = ((max - start) + end) * value;
				retval = start + diff;
			}else if ((end - start) > half){
				diff = -((max - end) + start) * value;
				retval = start + diff;
			}else retval = start + (end - start) * value;
			return retval;
		}

		static public float spring(float start, float end, float value){
			value = Mathf.Clamp01(value);
			value = (Mathf.Sin(value * Mathf.PI * (0.2f + 2.5f * value * value * value)) * Mathf.Pow(1f - value, 2.2f) + value) * (1f + (1.2f * (1f - value)));
			return start + (end - start) * value;
		}

		static public float easeInQuad(float start, float end, float value){
			end -= start;
			return end * value * value + start;
		}

		static public float easeOutQuad(float start, float end, float value){
			end -= start;
			return -end * value * (value - 2) + start;
		}

		static public float easeInOutQuad(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value + start;
			value--;
			return -end / 2 * (value * (value - 2) - 1) + start;
		}

		static public float easeInCubic(float start, float end, float value){
			end -= start;
			return end * value * value * value + start;
		}

		static public float easeOutCubic(float start, float end, float value){
			value--;
			end -= start;
			return end * (value * value * value + 1) + start;
		}

		static public float easeInOutCubic(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value + start;
			value -= 2;
			return end / 2 * (value * value * value + 2) + start;
		}

		static public float easeInQuart(float start, float end, float value){
			end -= start;
			return end * value * value * value * value + start;
		}

		static public float easeOutQuart(float start, float end, float value){
			value--;
			end -= start;
			return -end * (value * value * value * value - 1) + start;
		}

		static public float easeInOutQuart(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value * value + start;
			value -= 2;
			return -end / 2 * (value * value * value * value - 2) + start;
		}

		static public float easeInQuint(float start, float end, float value){
			end -= start;
			return end * value * value * value * value * value + start;
		}

		static public float easeOutQuint(float start, float end, float value){
			value--;
			end -= start;
			return end * (value * value * value * value * value + 1) + start;
		}

		static public float easeInOutQuint(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * value * value * value * value * value + start;
			value -= 2;
			return end / 2 * (value * value * value * value * value + 2) + start;
		}

		static public float easeInSine(float start, float end, float value){
			end -= start;
			return -end * Mathf.Cos(value / 1 * (Mathf.PI / 2)) + end + start;
		}

		static public float easeOutSine(float start, float end, float value){
			end -= start;
			return end * Mathf.Sin(value / 1 * (Mathf.PI / 2)) + start;
		}

		static public float easeInOutSine(float start, float end, float value){
			end -= start;
			return -end / 2 * (Mathf.Cos(Mathf.PI * value / 1) - 1) + start;
		}

		static public float easeInExpo(float start, float end, float value){
			end -= start;
			return end * Mathf.Pow(2, 10 * (value / 1 - 1)) + start;
		}

		static public float easeOutExpo(float start, float end, float value){
			end -= start;
			return end * (-Mathf.Pow(2, -10 * value / 1) + 1) + start;
		}

		static public float easeInOutExpo(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return end / 2 * Mathf.Pow(2, 10 * (value - 1)) + start;
			value--;
			return end / 2 * (-Mathf.Pow(2, -10 * value) + 2) + start;
		}

		static public float easeInCirc(float start, float end, float value){
			end -= start;
			return -end * (Mathf.Sqrt(1 - value * value) - 1) + start;
		}

		static public float easeOutCirc(float start, float end, float value){
			value--;
			end -= start;
			return end * Mathf.Sqrt(1 - value * value) + start;
		}

		static public float easeInOutCirc(float start, float end, float value){
			value /= .5f;
			end -= start;
			if (value < 1) return -end / 2 * (Mathf.Sqrt(1 - value * value) - 1) + start;
			value -= 2;
			return end / 2 * (Mathf.Sqrt(1 - value * value) + 1) + start;
		}

		/* GFX47 MOD START */
		static public float easeInBounce(float start, float end, float value){
			end -= start;
			float d = 1f;
			return end - easeOutBounce(0, end, d-value) + start;
		}
		/* GFX47 MOD END */

		/* GFX47 MOD START */
		//static public float bounce(float start, float end, float value){
		static public float easeOutBounce(float start, float end, float value){
			value /= 1f;
			end -= start;
			if (value < (1 / 2.75f)){
				return end * (7.5625f * value * value) + start;
			}else if (value < (2 / 2.75f)){
				value -= (1.5f / 2.75f);
				return end * (7.5625f * (value) * value + .75f) + start;
			}else if (value < (2.5 / 2.75)){
				value -= (2.25f / 2.75f);
				return end * (7.5625f * (value) * value + .9375f) + start;
			}else{
				value -= (2.625f / 2.75f);
				return end * (7.5625f * (value) * value + .984375f) + start;
			}
		}
		/* GFX47 MOD END */

		/* GFX47 MOD START */
		static public float easeInOutBounce(float start, float end, float value){
			end -= start;
			float d = 1f;
			if (value < d/2) return easeInBounce(0, end, value*2) * 0.5f + start;
			else return easeOutBounce(0, end, value*2-d) * 0.5f + end*0.5f + start;
		}
		/* GFX47 MOD END */

		static public float easeInBack(float start, float end, float value){
			end -= start;
			value /= 1;
			float s = 1.70158f;
			return end * (value) * value * ((s + 1) * value - s) + start;
		}

		static public float easeOutBack(float start, float end, float value){
			float s = 1.70158f;
			end -= start;
			value = (value / 1) - 1;
			return end * ((value) * value * ((s + 1) * value + s) + 1) + start;
		}

		static public float easeInOutBack(float start, float end, float value){
			float s = 1.70158f;
			end -= start;
			value /= .5f;
			if ((value) < 1){
				s *= (1.525f);
				return end / 2 * (value * value * (((s) + 1) * value - s)) + start;
			}
			value -= 2;
			s *= (1.525f);
			return end / 2 * ((value) * value * (((s) + 1) * value + s) + 2) + start;
		}

		static public float punch(float amplitude, float percentage){
			float s = 9;
			if (percentage == 0){
				return 0;
			}
			if (percentage == 1){
				return 0;
			}
			float period = 1 * 0.3f;
			s = period / (2 * Mathf.PI) * Mathf.Asin(0);
			return (amplitude * Mathf.Pow(2, -10 * percentage) * Mathf.Sin((percentage * 1 - s) * (2 * Mathf.PI) / period));
		}

		/* GFX47 MOD START */
		static public float easeInElastic(float start, float end, float value){
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (value == 0) return start;

			if ((value /= d) == 1) return start + end;

			if (a == 0f || a < Mathf.Abs(end)){
				a = end;
				s = p / 4;
			}else{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}

			return -(a * Mathf.Pow(2, 10 * (value-=1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
		}		
		/* GFX47 MOD END */

		/* GFX47 MOD START */
		//static public float elastic(float start, float end, float value){
		static public float easeOutElastic(float start, float end, float value){
			/* GFX47 MOD END */
			//Thank you to rafael.marteleto for fixing this as a port over from Pedro's UnityTween
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (value == 0) return start;

			if ((value /= d) == 1) return start + end;

			if (a == 0f || a < Mathf.Abs(end)){
				a = end;
				s = p / 4;
			}else{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}

			return (a * Mathf.Pow(2, -10 * value) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) + end + start);
		}		

		/* GFX47 MOD START */
		static public float easeInOutElastic(float start, float end, float value){
			end -= start;

			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;

			if (value == 0) return start;

			if ((value /= d/2) == 2) return start + end;

			if (a == 0f || a < Mathf.Abs(end)){
				a = end;
				s = p / 4;
			}else{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}

			if (value < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (value-=1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p)) + start;
			return a * Mathf.Pow(2, -10 * (value-=1)) * Mathf.Sin((value * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
		}


		//============================

        static public void CalcScale(Transform thisTransform, Vector3 start, Vector3 end, float percentage , eKind kind, bool reverse)
        {
            if (reverse) percentage = 1f - percentage;

            //calculate:
            Vector3 vv = ConstV.v3_zero; 
            vv.x = Calc(kind, start.x, end.x, percentage);
            vv.y = Calc(kind, start.y, end.y, percentage);
            vv.z = Calc(kind, start.z, end.z, percentage);

            //apply:
            thisTransform.localScale = vv;

            //dial in:
            if (percentage >= 1f)
            {
                thisTransform.localScale = end;
            }
        }
		
        //percentage : 0~1당 , 1을 넘어가는 값을 넣으면 정상동작 안함 
        static public void CalcShakePosition(Transform thisTransform, Vector3 start, Vector3 end, float percentage)
		{
			//impact:
            if (percentage <= float.Epsilon) 
            {
                thisTransform.Translate(end);
			}
			
			//reset:
            thisTransform.position=start;
			
			//generate:
			float diminishingControl = 1-percentage; //퍼센트가 1을 넘어가면 값이 0으로 되게 한다. 퍼센트의 최대값이 1일때만 해
            Vector3 v_rand = UtilGS9.ConstV.v3_zero;
			v_rand.x= UnityEngine.Random.Range(-end.x*diminishingControl, end.x*diminishingControl);
			v_rand.y= UnityEngine.Random.Range(-end.y*diminishingControl, end.y*diminishingControl);
			v_rand.z= UnityEngine.Random.Range(-end.z*diminishingControl, end.z*diminishingControl);

			//apply:	
            thisTransform.position+=v_rand;
			
		}	

        static public void CalcShakeScale(Transform thisTransform, Vector3 start, Vector3 end, float percentage)
		{
			//impact:
            if (percentage <= float.Epsilon)
            {
                thisTransform.localScale = end;
            }
			
			//reset:
			thisTransform.localScale=start; //초기값
			
			//generate:
			float diminishingControl = 1-percentage;
            Vector3 v_rand = UtilGS9.ConstV.v3_zero;
			v_rand.x= UnityEngine.Random.Range(-end.x*diminishingControl, end.x*diminishingControl);
			v_rand.y= UnityEngine.Random.Range(-end.y*diminishingControl, end.y*diminishingControl);
			v_rand.z= UnityEngine.Random.Range(-end.z*diminishingControl, end.z*diminishingControl);
            //DebugWide.LogBlue(v_rand + "  " + percentage);
			//apply:
            thisTransform.localScale+=v_rand;
		}

        static public void CalcShakeRotation(Transform thisTransform, Vector3 start, Vector3 end, float percentage)
		{
			//impact:
            if (percentage <= float.Epsilon)
            {
                thisTransform.Rotate(end);
            }
			
			//reset:
			thisTransform.eulerAngles=start;
			
			//generate:
			float diminishingControl = 1-percentage;
            Vector3 v_rand = UtilGS9.ConstV.v3_zero;
			v_rand.x= UnityEngine.Random.Range(-end.x*diminishingControl, end.x*diminishingControl);
			v_rand.y= UnityEngine.Random.Range(-end.y*diminishingControl, end.y*diminishingControl);
			v_rand.z= UnityEngine.Random.Range(-end.z*diminishingControl, end.z*diminishingControl);

			//apply:
            thisTransform.Rotate(v_rand);
			
		}//end func
	
		

	}//end class

}//end namespace

