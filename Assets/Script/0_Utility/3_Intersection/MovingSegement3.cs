using UnityEngine;
using System;


namespace UtilGS9
{
    //움직이는 선분의 교차요소 구하기
    public class MovingSegement3
    {
        public LineSegment3 _prev_seg_A;
        public LineSegment3 _prev_seg_B;
        public LineSegment3 _cur_seg_A;
        public LineSegment3 _cur_seg_B;

        //삼각형 0,1 합친모양의 사각형
        private Tetragon3 _tetr01;
        private Tetragon3 _tetr23;

        private IntrTriangle3Triangle3 _intr_0_2;
        private IntrTriangle3Triangle3 _intr_0_3;
        private IntrTriangle3Triangle3 _intr_1_2;
        private IntrTriangle3Triangle3 _intr_1_3;
        public Vector3 _minV, _maxV;
        public Vector3 _meetPt;
        public Vector3 _meetPt_A, _meetPt_B;
        public float _radius_A = 0f;
        public float _radius_B = 0f;

        public MovingSegement3()
        {
            _tetr01 = new Tetragon3();
            _tetr23 = new Tetragon3();
            _tetr01.tri0 = Triangle3.Zero();
            _tetr01.tri1 = Triangle3.Zero();
            _tetr23.tri0 = Triangle3.Zero();
            _tetr23.tri1 = Triangle3.Zero();
            _intr_0_2 = new IntrTriangle3Triangle3(_tetr01.tri0, _tetr23.tri0);
            _intr_0_3 = new IntrTriangle3Triangle3(_tetr01.tri0, _tetr23.tri1);
            _intr_1_2 = new IntrTriangle3Triangle3(_tetr01.tri1, _tetr23.tri0);
            _intr_1_3 = new IntrTriangle3Triangle3(_tetr01.tri1, _tetr23.tri1);
        }

        //CalcSegment_PushPoint 함수 호출전 초기값 prev 와 cur 을 같게 만들어 줘야 한다  
        //public void InitSegAB(LineSegment3 segA, LineSegment3 segB)
        //{
        //    _cur_seg_A = segA;
        //    _prev_seg_A = segA;

        //    _cur_seg_B = segB;
        //    _prev_seg_B = segB;
        //}


        public void Draw()
        {
            //if (__result_meet)
            //{
            //    //min, max
            //    DebugWide.DrawCircle(_minV, 0.02f, Color.white);
            //    DebugWide.DrawCircle(_maxV, 0.04f, Color.white);

            //    //meetPt
            //    //DebugWide.DrawCircle(_meetPt, 0.5f, Color.red); //chamto test
            //}

            if (__isSeg_A && __isSeg_B)
            {   //선분과 선분

                _prev_seg_A.Draw(Color.blue);
                _prev_seg_B.Draw(Color.magenta);
                //if (true == __intr_A_B_inside)
                //{
                //    DebugWide.DrawCircle(__cpPt0, 0.05f, Color.red);
                //    //DebugWide.DrawCircle(__cpPt1, 0.05f, Color.red);
                //    //DebugWide.DrawLine(__cpPt0, __cpPt1, Color.red);    
                //}


            }
            else
            {   //삼각형과 삼각형 

                _tetr01.Draw(Color.blue);
                _tetr23.Draw(Color.magenta);
                _intr_0_2.Draw(Color.red);
                _intr_0_3.Draw(Color.red);
                _intr_1_2.Draw(Color.red);
                _intr_1_3.Draw(Color.red);

            }

        }

        //ctp : 접촉점 
        //stand : 정렬하기 위한 기준점 
        public void SortMinMax(Vector3 ctP, Vector3 stand,
                                         ref Vector3 minV, ref Vector3 maxV, ref float minD, ref float maxD)
        {
            float a = (stand - ctP).sqrMagnitude;
            if (a <= minD)
            {
                minV = ctP;
                minD = a;
            }
            if (a > maxD)
            {
                maxV = ctP;
                maxD = a;
            }
        }

        //!!! count 2 일때, 즉 선분검사시 제대로 정렬을 못해줌. 대체함수 사용하기 
        //count : 삼각형 vs 삼각형의 찾은 접촉점(최대6개)에 대하여 검색할 최대 개수 지정 , 삼각형 2개가 합쳐진 사각형이 대상이 아님  
        // 예) 찾은 접촉점이 점과 선분에 해당한다면 개수를 2로 지정 , 3이상은 평면
        public bool GetMinMax_ContactPt(Vector3 comparisonSt, out Vector3 minV, out Vector3 maxV, int count)
        {

            //_intr_0_2.Draw(Color.black);
            //_intr_0_3.Draw(Color.black);
            //_intr_1_2.Draw(Color.black);
            //_intr_1_3.Draw(Color.black);

            bool result = false;
            minV = ConstV.v3_zero;
            maxV = ConstV.v3_zero;
            float minD = 1000000, maxD = 0;
            //int count = 2;
            for (int i = 0; i < count; i++)
            {
                //DebugWide.DrawLine(comparisonSt, _intr_0_2.mPoint[i], Color.green);
                //DebugWide.DrawLine(comparisonSt, _intr_0_3.mPoint[i], Color.green);
                //DebugWide.DrawLine(comparisonSt, _intr_1_2.mPoint[i], Color.green);
                //DebugWide.DrawLine(comparisonSt, _intr_1_3.mPoint[i], Color.green);
                //DebugWide.PrintText(_intr_0_2.mPoint[i], Color.green , (comparisonSt- _intr_0_2.mPoint[i]).magnitude + "");
                //DebugWide.PrintText(_intr_0_3.mPoint[i], Color.green , (comparisonSt - _intr_0_3.mPoint[i]).magnitude + "");
                //DebugWide.PrintText(_intr_1_2.mPoint[i], Color.green , (comparisonSt - _intr_1_2.mPoint[i]).magnitude + "");
                //DebugWide.PrintText(_intr_1_3.mPoint[i], Color.green , (comparisonSt - _intr_1_3.mPoint[i]).magnitude + "");


                //교점이 점 , 선분일때 처리 
                if (i < _intr_0_2.mQuantity)
                {
                    SortMinMax(_intr_0_2.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i02:"+_intr_0_2.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
                if (i < _intr_0_3.mQuantity)
                {
                    SortMinMax(_intr_0_3.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i03:" + _intr_0_3.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
                if (i < _intr_1_2.mQuantity)
                {
                    SortMinMax(_intr_1_2.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i12:" + _intr_1_2.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
                if (i < _intr_1_3.mQuantity)
                {
                    SortMinMax(_intr_1_3.mPoint[i], comparisonSt, ref minV, ref maxV, ref minD, ref maxD);
                    result = true;
                    //DebugWide.LogBlue("    i13:" + _intr_1_3.mPoint[i] + "   min:" + minV + "   max:" + maxV);
                }
            }

                //DebugWide.DrawCircle(minV, 0.03f, Color.red);
                //DebugWide.DrawCircle(maxV, 0.07f, Color.red);

                //_intr_0_2.Draw(Color.black);
                //_intr_0_3.Draw(Color.black);
                //_intr_1_2.Draw(Color.black);
                //_intr_1_3.Draw(Color.black);

            return result;
        }

        //선분과 점 처리 전용함수 
        public bool GetMinMax_Segement(Vector3 dir, out Vector3 min, out Vector3 max)
        {

            bool contact = false;
            Vector3[] arr = new Vector3[8];
            arr[0] = _intr_0_2.mPoint[0];
            arr[1] = _intr_0_2.mPoint[1];
            arr[2] = _intr_0_3.mPoint[0];
            arr[3] = _intr_0_3.mPoint[1];
            arr[4] = _intr_1_2.mPoint[0];
            arr[5] = _intr_1_2.mPoint[1];
            arr[6] = _intr_1_3.mPoint[0];
            arr[7] = _intr_1_3.mPoint[1];

            min = ConstV.v3_zero;
            max = ConstV.v3_zero;

            if (1 <= _intr_0_2.mQuantity || 1 <= _intr_0_3.mQuantity || 1 <= _intr_1_2.mQuantity || 1 <= _intr_1_3.mQuantity)
            {
                contact = true;

                //선택정렬을 한다 
                Vector3 temp;
                for (int a = 1; a < 8; a++)
                {
                    for (int i = a; i < 8; i++)
                    {
                        if (0 > Vector3.Dot(dir, arr[i] - arr[a - 1]))
                        {
                            temp = arr[a - 1];
                            arr[a - 1] = arr[i];
                            arr[i] = temp;
                        }
                    }
                }

                for (int i = 0; i < 8; i++)
                {
                    //DebugWide.LogBlue(i + "  " + arr[i]);
                    if (!Misc.IsZero(arr[i]))
                    {
                        min = arr[i];
                        break;
                    }
                }
                for (int i = 0; i < 8; i++)
                {
                    if (!Misc.IsZero(arr[7 - i]))
                    {
                        max = arr[7 - i];
                        break;
                    }
                }
            }//end if

            //_tetr01.Draw(Color.blue);
            //_tetr23.Draw(Color.magenta);
            //if(true == contact)
            //{
            //    for (int i = 0; i < 8; i++)
            //    {
            //        Color color = Color.green;
            //        if (Misc.IsZero(arr[i]))
            //            color = Color.black;
            //        if(Misc.IsZero(arr[i]-min))
            //            color = Color.black;
            //        if (Misc.IsZero(arr[i] - max))
            //            color = Color.black;
            //        DebugWide.DrawCircle(arr[i], 0.008f + 0.005f * i, color);
            //    }
            //    //DebugWide.DrawCircle(min, 0.03f, Color.red);
            //    //DebugWide.DrawCircle(max, 0.07f, Color.red);

            //    //_intr_0_2.Draw(Color.black);
            //    //_intr_0_3.Draw(Color.black);
            //    //_intr_1_2.Draw(Color.black);
            //    //_intr_1_3.Draw(Color.black);
            //}

            return contact;

        }

        // ** meetPt 를 지나는 적당한(근사값) 선분 구하기 ** 
        // ** start 와 end 의 길이가 같다는 전제로 작성된 알고리즘 **
        //선분이 start => end 로 이동시 사다리꼴 모양의 평면이 만들어진다 
        //사다리꼴의 위쪽을 v_up , 아래쪽을 v_down 하자
        //v_up 과 v_down 의 길이가 작은쪽을 기준으로 새로운 선분의 시작점을(origin) 구한다
        //새로운 선분은 origin 에서 meetPt 를 지난다 
        private void CalcSegment(Vector3 meetPt, LineSegment3 start, LineSegment3 end, out LineSegment3 newSeg)
        {

            Vector3 v_up = end.last - start.last;
            Vector3 v_down = end.origin - start.origin;
            float len_up = v_up.sqrMagnitude;
            float len_down = v_down.sqrMagnitude;

            Vector3 n_left = VOp.Normalize(start.direction);
            Vector3 n_right = VOp.Normalize(end.direction);
            float len_proj_left = Vector3.Dot(n_left, (meetPt - start.origin));
            float len_proj_right = Vector3.Dot(n_right, (meetPt - end.origin));
            float len_perp_left = ((n_left * len_proj_left + start.origin) - meetPt).magnitude;
            float len_perp_right = ((n_right * len_proj_right + end.origin) - meetPt).magnitude;
            float rate = len_perp_left / (len_perp_left + len_perp_right);

            Vector3 origin, last;
            float len_start = start.direction.magnitude;
            //작은쪽을 선택 
            if (len_up > len_down)
            {
                origin = v_down * rate + start.origin;

                last = VOp.Normalize(meetPt - origin) * len_start + origin;

            }
            else
            {
                last = v_up * rate + start.last;

                origin = VOp.Normalize(meetPt - last) * len_start + last;
            }
            newSeg = new LineSegment3(origin, last);

        }


        //fixedOriginPt : 고정된 새로운 선분의 출발점 
        private void CalcSegment(bool allowFixed, Vector3 fixedOriginPt, Vector3 meetPt, LineSegment3 start, LineSegment3 end, out LineSegment3 newSeg)
        {
            Vector3 origin, last;
            float len_start = start.direction.magnitude;

            if (allowFixed)
            {
                origin = fixedOriginPt;
                last = VOp.Normalize(meetPt - origin) * len_start + origin;
            }
            else
            {
                Vector3 v_up = end.last - start.last;
                Vector3 v_down = end.origin - start.origin;
                float len_up = v_up.sqrMagnitude;
                float len_down = v_down.sqrMagnitude;

                Vector3 n_left = VOp.Normalize(start.direction);
                Vector3 n_right = VOp.Normalize(end.direction);
                float len_proj_left = Vector3.Dot(n_left, (meetPt - start.origin));
                float len_proj_right = Vector3.Dot(n_right, (meetPt - end.origin));
                float len_perp_left = ((n_left * len_proj_left + start.origin) - meetPt).magnitude;
                float len_perp_right = ((n_right * len_proj_right + end.origin) - meetPt).magnitude;
                float rate = len_perp_left / (len_perp_left + len_perp_right);


                //NaN 예외처리 추가 
                if (Misc.IsZero(len_perp_left + len_perp_right))
                {
                    //DebugWide.LogYellow("prev: " + start + " cur: " + end + "  left: " + len_perp_left +"   right: "+ len_perp_right);
                    rate = 0;
                }


                //작은쪽을 선택 
                if (len_up > len_down)
                {
                    origin = v_down * rate + start.origin;

                    last = VOp.Normalize(meetPt - origin) * len_start + origin;

                }
                else
                {
                    last = v_up * rate + start.last;

                    origin = VOp.Normalize(meetPt - last) * len_start + last;
                }
            }


            newSeg = new LineSegment3(origin, last);
        }



        LineSegment3[] __list_line_A = new LineSegment3[3];
        LineSegment3[] __list_line_B = new LineSegment3[3];
        public void ClosestLine(out Vector3 min_A, out Vector3 min_B)
        {
            int count_A = 1;
            int count_B = 1;

            __list_line_A[0] = _cur_seg_A;
            __list_line_B[0] = _cur_seg_B;
            if(false == __isSeg_A)
            {
                count_A = 3;

                __list_line_A[1] = _tetr01.GetLine_Last();
                __list_line_A[2] = _tetr01.GetLine_Origin();
            }

            if (false == __isSeg_B)
            {
                count_B = 3;

                __list_line_B[1] = _tetr23.GetLine_Last();
                __list_line_B[2] = _tetr23.GetLine_Origin();
            }

            Vector3 pt_close_A, pt_close_B, dir_A_B;
            min_A = ConstV.v3_zero; min_B = ConstV.v3_zero;
            float min = 10000000f;
            float cur = 0f;
            for (int a = 0; a < count_A;a++)
            {
                for (int b = 0; b < count_B; b++)
                {
                    LineSegment3.ClosestPoints(out pt_close_A, out pt_close_B, __list_line_A[a], __list_line_B[b]);
                    dir_A_B = pt_close_B - pt_close_A;
                    cur = dir_A_B.sqrMagnitude;
                    if(min > cur)
                    {
                        min = cur;
                        min_A = pt_close_A;
                        min_B = pt_close_B;
                    }
                }   
            }
        }


        public bool __intr_A_B_inside = false;
        public bool __intr_A_B_outside = false;
        public float __intr_min_length = 0;
        public int __test_value = 0;
        public Vector3 __dir_move_A = ConstV.v3_zero;
        public Vector3 __dir_move_B = ConstV.v3_zero;
        public bool Find_TGuard_vs_TGuard(float rateAtoB, bool allowFixed_a, bool allowFixed_b , Transform root_0, Transform root_1)
        {
            bool result_contact = false;
            bool isContact_onPlan = false;
            Vector3 minV = ConstV.v3_zero, maxV = ConstV.v3_zero;
            Vector3 meetPt = ConstV.v3_zero;
            Vector3 pt_close_A, pt_close_B;
            float rad_AB = _radius_A + _radius_B;

            __localRota_A = Quaternion.identity;
            __localRota_B = Quaternion.identity;
            __dir_move_A = ConstV.v3_zero;
            __dir_move_B = ConstV.v3_zero;

            LineSegment3.ClosestPoints(out pt_close_A, out pt_close_B, _cur_seg_A, _cur_seg_B);
            __cur_A_B_order = pt_close_B - pt_close_A;

            //DebugWide.DrawLine(_cur_seg_A.origin, _cur_seg_A.origin + __prev_A_B_order, Color.yellow);

            __intr_A_B_inside = false;
            if (rad_AB * rad_AB > (pt_close_A - pt_close_B).sqrMagnitude)
            {
                //DebugWide.LogRed(rad_AB + "  " + (pt_end - pt_start).magnitude);
                if (0 < Vector3.Dot(__prev_A_B_order, __cur_A_B_order))
                    __intr_A_B_inside = true;
            }

            //=========================
            LineSegment3 newSegA = _cur_seg_A, newSegB = _cur_seg_B;
            __test_value = 0;
            //if (0 < Vector3.Dot(__prev_A_B_order, __cur_A_B_order))
            //{
            //    if (rad_AB * rad_AB > (pt_close_A - pt_close_B).sqrMagnitude)
            //        __intr_A_B_inside = true;
            //}
            //else
            if (0 > Vector3.Dot(__prev_A_B_order, __cur_A_B_order))
            {
                __test_value = 2;
                //DebugWide.LogGreen("!! 사각꼴(선분) vs 사각꼴(선분) 검사 ");

                _intr_0_2.Find_Twice();
                _intr_0_3.Find_Twice();
                _intr_1_2.Find_Twice();
                _intr_1_3.Find_Twice();

                //사각꼴이 서로 같은 평면에서 만난경우
                if (eIntersectionType.PLANE == _intr_0_2.mIntersectionType ||
                    eIntersectionType.PLANE == _intr_0_3.mIntersectionType ||
                    eIntersectionType.PLANE == _intr_1_2.mIntersectionType ||
                    eIntersectionType.PLANE == _intr_1_3.mIntersectionType)
                {
                    isContact_onPlan = true;

                    //선분과 사각꼴이 같은 평면에서 만난경우
                    if (true == __isSeg_A && false == __isSeg_B)
                    {
                        //DebugWide.LogBlue("!! 선분 vs 사각꼴 ");
                        result_contact = GetMinMax_ContactPt(_prev_seg_B.origin, out minV, out maxV, 4);
                    }
                    else if (false == __isSeg_A && true == __isSeg_B)
                    {
                        //DebugWide.LogBlue("!! 사각꼴 vs 선분 ");
                        result_contact = GetMinMax_ContactPt(_prev_seg_A.origin, out minV, out maxV, 4);
                    }
                    //사각꼴과 사각꼴이 같은 평면에서 만난경우 
                    else if (false == __isSeg_A && false == __isSeg_B)
                    {
                        //!!!! 초기에 prev 와 cur 을 같게 안만들어주면 여기로 처리가 들어오게 된다
                        //  잘못된 정보에 의해 접촉한것으로 계산하게 되며 초기값에 의해 방향성을 가지게 되어 
                        //  dropping 에서 잘못된 방향으로 무한히 떨어뜨리게 된다.
                        //DebugWide.LogRed("!! 사각꼴과 사각꼴이 같은 평면에서 만난경우 ");
                        result_contact = GetMinMax_ContactPt(_cur_seg_A.origin, out minV, out maxV, 6);

                    }
                    meetPt = minV + (maxV - minV) * rateAtoB;

                    if(result_contact)
                        DebugWide.LogBlue("!! 사각꼴(선분)이 같은 평면에서 만난 경우 ");

                    //DebugWide.DrawCircle(minV, 0.02f, Color.red);
                    //DebugWide.DrawCircle(meetPt, 0.04f, Color.red);
                    //DebugWide.DrawCircle(maxV, 0.06f, Color.red);

                }
                //사각꼴(선분)이 서로 엇갈려 만난경우
                else
                {
                    __test_value = 3;
                    if (true == __isSeg_A && true == __isSeg_B)
                    {
                        __test_value = 4;

                        LineSegment3 mer_a = LineSegment3.Merge(_prev_seg_A, _cur_seg_A);
                        LineSegment3 mer_b = LineSegment3.Merge(_prev_seg_B, _cur_seg_B);
                        //mer_a.Draw(Color.black);
                        //mer_b.Draw(Color.white);

                        LineSegment3.ClosestPoints(out minV, out maxV, mer_a, mer_b);
                        //LineSegment3.ClosestPoints(out minV, out maxV, _cur_seg_A, _cur_seg_B);
                        //DebugWide.DrawCircle(minV, 0.01f, Color.green);
                        //DebugWide.DrawCircle(maxV, 0.03f, Color.green);
                        //DebugWide.LogBlue("교점이 있음 1  : " + (minV - maxV).sqrMagnitude);
                        //if(float.Epsilon > (minV-maxV).sqrMagnitude)
                        //적당히 작은값을 설정해준다. 너무 작은값은 검사를 통과하지 못한다 
                        if (0.0000001f > (minV - maxV).sqrMagnitude)
                        {
                            //DebugWide.LogBlue("교점이 있음 2");
                            //교점이 있음 
                            isContact_onPlan = true;
                            result_contact = true;
                        }

                        DebugWide.LogBlue(result_contact + "  " + __test_value);
                    }
                    else
                    {
                        __test_value = 5;
                        result_contact = GetMinMax_Segement(__dir_A, out minV, out maxV);
                        DebugWide.LogBlue(result_contact + "  " + __test_value);
                    }

                    //GetMinMax_ContactPt 함수는 선분값 상태에서 최소/최대값을 제대로 못찾는다. 선분값에서는 GetMinMax_Segement 함수 사용하기
                    //result_contact = GetMinMax_ContactPt(_cur_seg_A.origin, out _minV, out _maxV, 2);

                    //DebugWide.LogBlue("엇갈려 " + result_contact + "  " );

                    if (true == result_contact)
                    {

                        //사각꼴과 선분이 만난 경우 : 교점이 하나만 나오므로 max를 따로 구해야 한다 
                        if (true == __isSeg_A && false == __isSeg_B)
                        {

                            //min 구하기 
                            Vector3 none;
                            Line3.ClosestPoints(out minV, out none, new Line3(maxV, __dir_B), new Line3(_cur_seg_B.origin, _cur_seg_B.direction));
                            //DebugWide.LogBlue("aaa ");

                        }
                        else if (false == __isSeg_A && true == __isSeg_B)
                        {

                            //max 구하기
                            Vector3 none;
                            Line3.ClosestPoints(out maxV, out none, new Line3(minV, __dir_A), new Line3(_cur_seg_A.origin, _cur_seg_A.direction));
                            //DebugWide.LogBlue("bbb ");
                        }
                    }


                    meetPt = minV + (maxV - minV) * rateAtoB;

                    //DebugWide.DrawLine(minV, maxV, Color.gray);


                }

                //==================================================



                if (result_contact)
                {
                    __test_value = 10;
                    //DebugWide.LogGreen("!! 사각꼴(선분)이 서로 엇갈려 만난 경우  r:" + result_contact + "  sA:" + __isSeg_A + "  sB:" + __isSeg_B);    

                    Vector3 dir_drop = maxV - minV;
                    if(true == isContact_onPlan)
                    {
                        //평면위에 접촉점이 있는 경우 
                        dir_drop = __dir_A + __dir_B; //ab 둘다 방향값이 있을때 성립하는지 모르겠다.. 
                    }
                    dir_drop = VOp.Normalize(dir_drop);

                    float drop_sign = 1f;
                    if (0 > Vector3.Dot(dir_drop, __prev_A_B_order))
                    {
                        //DebugWide.LogRed("min max 방향이 달라졌음 ! aa");
                        drop_sign = -1f;
                    }

                    Vector3 lastPt1 = meetPt, lastPt2 = meetPt;
                    float dropping = (_radius_A + _radius_B) * (1f - rateAtoB);
                    if (true == allowFixed_a)
                    {

                        lastPt1 += -dir_drop * drop_sign * dropping; //dropping 처리 
                        _meetPt_A = lastPt1;
                        //이전 선분에 회전적용
                        //Vector3 firstPt = CalcTGuard_FirstPt2(lastPt1 ,root_0, _prev_seg_A);
                        //RotateTGuard_FirstToLast(firstPt, lastPt1, root_0.position, _prev_seg_A, out newSegA, out __localRota_A);    

                        //현재 선분에 회전적용
                        Vector3 firstPt = CalcTGuard_FirstPt2(lastPt1, root_0, _cur_seg_A);
                        RotateTGuard_FirstToLast(firstPt, lastPt1, root_0.position, _cur_seg_A, out newSegA, out __localRota_A);

                        //DebugWide.DrawCircle(lastPt1, _radius_A, Color.blue);

                    }
                    else
                    {
                        
                        lastPt1 += -dir_drop * drop_sign * dropping;
                        __dir_move_A = lastPt1 - maxV;
                        _meetPt_A = lastPt1;
                        newSegA = LineSegment3.Move(_prev_seg_A, __dir_move_A);
                        //DebugWide.DrawCircle(lastPt1, _radius_A, Color.blue);
                    }

                    dropping = (_radius_A + _radius_B) * (rateAtoB);
                    if (true == allowFixed_b)
                    {
                        
                        lastPt2 += dir_drop * drop_sign * dropping; //dropping 처리
                        _meetPt_B = lastPt2;
                        //이전 선분에 회전적용
                        //Vector3 firstPt = CalcTGuard_FirstPt2(lastPt2, root_1, _prev_seg_B);
                        //RotateTGuard_FirstToLast(firstPt, lastPt2, root_1.position, _prev_seg_B, out newSegB, out __localRota_B);    

                        //현재 선분에 회전적용
                        Vector3 firstPt = CalcTGuard_FirstPt2(lastPt2, root_1, _cur_seg_B);
                        RotateTGuard_FirstToLast(firstPt, lastPt2, root_1.position, _cur_seg_B, out newSegB, out __localRota_B);

                        //DebugWide.DrawCircle(lastPt2, _radius_B, Color.magenta);

                    }
                    else
                    {
                        
                        //pt_close_A
                        lastPt2 += dir_drop * drop_sign * dropping;
                        __dir_move_B = lastPt2 - minV;
                        _meetPt_B = lastPt2;
                        newSegB = LineSegment3.Move(_prev_seg_B, __dir_move_B);
                        //DebugWide.DrawCircle(lastPt2, _radius_B, Color.magenta);
                    }


                    //_cur_seg_A = newSegA;
                    //_cur_seg_B = newSegB;


                }else
                {
                    //--------------------------------
                    //최소선분 다시 구함 
                    __intr_A_B_outside = false;
                    ClosestLine(out pt_close_A, out pt_close_B);
                    if (rad_AB * rad_AB > (pt_close_A - pt_close_B).sqrMagnitude)
                    {
                        __intr_A_B_outside = true;   
                        __prev_A_B_order = __cur_A_B_order; //순서가 바뀌므로 갱신시켜 줘야함 
                    }
                    DebugWide.LogYellow(" --- intr_ " + __intr_A_B_outside);

                }

            }

            if (true == __intr_A_B_inside || true == __intr_A_B_outside)
            {
                __test_value = 1;
                //DebugWide.LogGreen("!! 현재선분검사  " + __isSeg_A + "  " + __isSeg_B);

                minV = maxV = meetPt = pt_close_A;
                result_contact = true;

                Vector3 dir_A_B = (pt_close_B - pt_close_A);
                float penetration = (_radius_A + _radius_B) - dir_A_B.magnitude;
                Vector3 n_close_BA = -VOp.Normalize(dir_A_B);
                Vector3 pt_first_A, pt_center_A;
                Vector3 pt_first_B, pt_center_B;

                //DebugWide.DrawCircle(pt_close_A, _radius_A, Color.blue);
                //DebugWide.DrawCircle(pt_close_B, _radius_B, Color.magenta);

                if (true == allowFixed_a)
                {
                    CalcTGuard_First_ClosePt(false, penetration * (1f - rateAtoB), pt_close_A, n_close_BA, root_0, out pt_center_A, out pt_first_A);
                    RotateTGuard_FirstToLast(pt_close_A, pt_first_A, root_0.position, _cur_seg_A, out newSegA, out __localRota_A);
                    _meetPt_A = pt_first_A;
                }
                else
                {
                    pt_first_A = pt_close_A + n_close_BA * (penetration * (1f - rateAtoB));
                    __dir_move_A = pt_first_A - pt_close_A;
                    //DebugWide.LogBlue(VOp.ToString(__dir_move_A));
                    _meetPt_A = pt_first_A;
                    //newSegA = LineSegment3.Move(_prev_seg_A, __dir_move_A);
                    newSegA = LineSegment3.Move(_cur_seg_A, __dir_move_A);
                }

                if (true == allowFixed_b)
                {
                    CalcTGuard_First_ClosePt(false, penetration * (rateAtoB), pt_close_B, -n_close_BA, root_1, out pt_center_B, out pt_first_B);
                    RotateTGuard_FirstToLast(pt_close_B, pt_first_B, root_1.position, _cur_seg_B, out newSegB, out __localRota_B);
                    _meetPt_B = pt_first_B;
                }
                else
                {
                    pt_first_B = pt_close_B + -n_close_BA * (penetration * (rateAtoB));
                    __dir_move_B = pt_first_B - pt_close_B;
                    _meetPt_B = pt_first_B;
                    //newSegB = LineSegment3.Move(_prev_seg_B, __dir_move_B);
                    newSegB = LineSegment3.Move(_cur_seg_B, __dir_move_B);
                }

                DebugWide.LogBlue(result_contact + "  " + __test_value);
                //DebugWide.DrawCircle(pt_first_A, _radius_A, Color.blue);
                //DebugWide.DrawCircle(pt_first_B, _radius_B, Color.magenta);
                //DebugWide.DrawLine(pt_close_A, pt_close_B, Color.red);
                //===============


                //LineSegment3.ClosestPoints(out pt_close_A, out pt_close_B, _cur_seg_A, _cur_seg_B);
                //__cur_A_B_order = pt_close_B - pt_close_A;
                //penetration = (_radius_A + _radius_B) - __cur_A_B_order.magnitude;
                //DebugWide.LogBlue("  len:" + __cur_A_B_order.magnitude + "  p:" + penetration + "  " + (pt_first_A-pt_first_B).magnitude);

            }

            LineSegment3.ClosestPoints(out pt_close_A, out pt_close_B, newSegA, newSegB);
            __cur_A_B_order2 = pt_close_B - pt_close_A;
            if (0 > Vector3.Dot(__prev_A_B_order, __cur_A_B_order2))
            {
                DebugWide.LogRed("선분이 통과되었음 !!!! " + __test_value + "  " + result_contact);
            }


            _cur_seg_A = newSegA;
            _cur_seg_B = newSegB;
            //_prev_seg_A = _cur_seg_A;
            //_prev_seg_B = _cur_seg_B;
            _prev_seg_A = newSegA;
            _prev_seg_B = newSegB;

            _minV = minV;
            _maxV = maxV;
            _meetPt = meetPt;
            //__result_meet = result_contact;

            if (false == result_contact && false == Misc.IsZero(__cur_A_B_order))
                __prev_A_B_order = __cur_A_B_order;


            return result_contact;
        }



        //n_dir 은 노멀값이어야 함 
        public void CalcTGuard_First_ClosePt(bool draw, float penetration, Vector3 pt_close, Vector3 n_dir, Transform root_0,
                                                out Vector3 pt_center, out Vector3 pt_first)
        {
            const float ERROR_RATE = 1f; //20프로 정도 침투길이를 늘려 계산할려는 목적 
            //float penetration = (_radius_A + _radius_B) - __cur_A_B_order.magnitude;
            //Vector3 n_dir = (pt_close_A - pt_close_B).normalized;
            Vector3 meetPt_A = pt_close + (n_dir * penetration);

            //===============
            Vector3 up_center = Vector3.Cross(root_0.rotation * ConstV.v3_forward, n_dir);
            pt_center = Line3.ClosestPoint(new Line3(root_0.position, up_center), pt_close);
            pt_center = root_0.position; //한계위치에서의 계산을 위해 임시로 처리 
            //===============
            //코사인 제2법칙 이용, a 와 c 는 길이가 같은점을 이용하여 식을 정리 
            //    0
            //  a   b
            // 0  c   0
            //
            // a2 = b2+c2-2bc*cosa
            // a2 - b2 - c2 = -2bc * cosa
            // (a2 - b2 - c2)/-2bc = cosa
            // a == c
            // b/2c = cosa
            float c = (pt_close - pt_center).magnitude;
            float b = penetration * ERROR_RATE; //오차율로 인해 ab선분의 반지름 합을 못 벗어나는 문제가 있음. 이를 제거함  
            float cos_a = b / (2 * c);
            float angle_a = Mathf.Acos(cos_a) * Mathf.Rad2Deg;

            Vector3 up = Vector3.Cross((pt_center - pt_close), (meetPt_A - pt_close));
            Vector3 dir_rot = Quaternion.AngleAxis(angle_a, up) * ((pt_center - pt_close).normalized * b);

            pt_first = pt_close + dir_rot;


            if (draw)
            {
                DebugWide.DrawLine(pt_center, pt_close, Color.yellow);
                DebugWide.DrawLine(meetPt_A, pt_close, Color.green);
                DebugWide.DrawLine(pt_first, pt_close, Color.black);
                DebugWide.LogBlue("  len:" + __cur_A_B_order.magnitude + "  p:" + penetration + "  ang:" + angle_a);
                DebugWide.LogBlue((pt_close - meetPt_A).magnitude + "  => " + (dir_rot).magnitude);
            }


        }



        //LineSegment3 __min_max = new LineSegment3();
        int __count = 0;

        public Quaternion __localRota_A = Quaternion.identity;
        public Quaternion __localRota_B = Quaternion.identity;

        //엉터리 함수 쓰지 말것 
        public bool CalcLimit(Vector3 meetPt, Vector3 min, Vector3 max,
                               Vector3 pos_t_root,
                                 LineSegment3 t_sub, out Vector3 limitPt)
        {

            //균등하지 않은 T서브일 수 있음 , min에서 가까운 T서브의 한쪽끝점을 사용한다
            Vector3 subPt = t_sub.origin;
            if ((min - t_sub.origin).sqrMagnitude > (min - t_sub.last).sqrMagnitude)
            {
                subPt = t_sub.last;
            }

            //원인모를 오차를 제거해 본다 0.002 - 오차가 생길 수 밖에 없음. 가정이 잘못되었음 
            float a = (subPt - pos_t_root).magnitude - 0.002f; //삼각형내의 최대 길이 
            //float a = (meetPt - pos_t_root).magnitude; //삼각형내의 최대 길이 

            //루트선분에 기울어진 서브선분을 제대로 판단못함 . 잘못된 처리 
            if ((meetPt - pos_t_root).sqrMagnitude < a * a)
            {
                limitPt = ConstV.v3_zero;
                return false;
            }

            Vector3 n_ab = VOp.Normalize(min - max);
            float ab2 = Vector3.Dot(n_ab, (pos_t_root - max));
            Vector3 b2 = max + (n_ab * ab2);


            //피타고라스의 정리를 이용 
            float pb2 = a * a - (b2 - pos_t_root).sqrMagnitude;
            pb2 = (float)Math.Sqrt(pb2);
            limitPt = b2 + (-n_ab * pb2);

            DebugWide.LogBlue((limitPt - pos_t_root).magnitude + "   " + a);
            DebugWide.DrawLine(max, b2, Color.green);
            DebugWide.DrawLine(b2, pos_t_root, Color.green);
            DebugWide.DrawLine(limitPt, pos_t_root, Color.magenta);

            return true;
        }

        //t0 구할대상 , t1 움직이는 T가드  
        public void CalcTGuard(Vector3 meetPt_first, Vector3 meetPt_last,
                               Vector3 pos_t0_root,
                                LineSegment3 t0_sub_start, LineSegment3 t1_sub_end,
                               out LineSegment3 new_t0_sub, out Quaternion localRot)
        {
            //---------------------------------------

            LineSegment3 ls_AB = new LineSegment3(t1_sub_end.origin, pos_t0_root);
            Vector3 dir_rootS_min = meetPt_first - pos_t0_root;

            //코사인 제2법칙 이용
            float c = ls_AB.Length(); //1# root_start , seg_start 사이의 거리 
            float a = (dir_rootS_min).magnitude; //2# root_start , sub 선분의 접촉점 사이의 거리 
            float dt2 = c * c - a * a;

            //근의공식 부호 결정 
            float value_sign = Vector3.Dot(dir_rootS_min, -ls_AB.direction);
            if (value_sign > 0) value_sign = -1; else value_sign = 1;

            Vector3 dir_seg_rate = meetPt_last - t1_sub_end.origin;
            Vector3 up_seg_rate = Vector3.Cross(ls_AB.direction, dir_seg_rate);
            float angle_seg_rate = UtilGS9.Geo.AngleSigned(ls_AB.direction, dir_seg_rate, up_seg_rate);

            float cosA = (float)Math.Cos(angle_seg_rate * Mathf.Deg2Rad);
            float dt1 = -2f * c * cosA; //dt1 : -2cCosA

            //이차방정식의 근의공식 이용 , disc = 판별값 
            float disc = dt1 * dt1 - 4 * dt2;
            float b_1 = (-dt1 + value_sign * (float)Math.Sqrt(disc)) / 2f; //가까운점 

            //---------------------------------------

            Vector3 new_dir_ls_seg1 = Quaternion.AngleAxis(angle_seg_rate, up_seg_rate) * ls_AB.direction;
            Vector3 meetPt_rate2 = t1_sub_end.origin + VOp.Normalize(new_dir_ls_seg1) * b_1;
            DebugWide.DrawCircle(meetPt_rate2, 0.08f, Color.green); //chamto test

            Vector3 up_t = Vector3.Cross(meetPt_first - pos_t0_root, meetPt_rate2 - pos_t0_root);
            float angle_t = Geo.AngleSigned(meetPt_first - pos_t0_root, meetPt_rate2 - pos_t0_root, up_t);

            //--------

            //tr_t1_root.rotation = Quaternion.AngleAxis(angle_t, up_t) * tr_t0_root.rotation;

            //--------

            Vector3 ori, last;
            localRot = Quaternion.AngleAxis(angle_t, up_t);

            //trs부모 * trs자식 * vertex
            //  2        1      대상정점   <=  정점에 적용되는 순서 , s크기 <= r회전 <= t이동 순으로 곱해진다  
            //t0_sub_start = t부모 * trs자식 * vertex
            //r부모 * t부모 * trs자식 * vertex : 현재 부모의 trs순서가 안맞아 문제가 생김 , r이 먼저 곱해져야 하는데 t가 먼저 곱해짐 
            //r부모 * t0_sub_start : 이상태임

            //trs 순서가 안맞아 문제가 있는 계산
            //ori = rota * (t0_sub_start.origin);
            //last = rota * (t0_sub_start.last);

            //t부모를 제거한후 r부모를 적용한다. 그다음 t부모를 다시 적용한다 
            ori = (localRot * (t0_sub_start.origin - pos_t0_root)) + pos_t0_root;
            last = (localRot * (t0_sub_start.last - pos_t0_root)) + pos_t0_root;

            new_t0_sub = new LineSegment3(ori, last);

            //---------------------------------------

        }

        public Vector3 CalcTGuard_LastPt(Vector3 meetPt_first, Vector3 meetPt_last,
                               Vector3 pos_t0_root,
                                LineSegment3 t1_sub_end)
        {
            LineSegment3 ls_AB = new LineSegment3(t1_sub_end.origin, pos_t0_root);
            Vector3 dir_rootS_min = meetPt_first - pos_t0_root;

            //코사인 제2법칙 이용
            float c = ls_AB.Length(); //1# root_start , seg_start 사이의 거리 
            float a = (dir_rootS_min).magnitude; //2# root_start , sub 선분의 접촉점 사이의 거리 
            float dt2 = c * c - a * a;

            //근의공식 부호 결정 
            float value_sign = Vector3.Dot(dir_rootS_min, -ls_AB.direction);
            if (value_sign > 0) value_sign = -1; else value_sign = 1;
            //value_sign *= -1; //chamto test

            Vector3 dir_seg_rate = meetPt_last - t1_sub_end.origin;
            Vector3 up_seg_rate = Vector3.Cross(ls_AB.direction, dir_seg_rate);
            float angle_seg_rate = UtilGS9.Geo.AngleSigned(ls_AB.direction, dir_seg_rate, up_seg_rate);

            float cosA = (float)Math.Cos(angle_seg_rate * Mathf.Deg2Rad);
            float dt1 = -2f * c * cosA; //dt1 : -2cCosA

            //이차방정식의 근의공식 이용 , disc = 판별값 
            float disc = dt1 * dt1 - 4 * dt2;
            float b_1 = (-dt1 + value_sign * (float)Math.Sqrt(disc)) / 2f; //가까운점 

            //---------------------------------------

            Vector3 new_dir_ls_seg1 = Quaternion.AngleAxis(angle_seg_rate, up_seg_rate) * ls_AB.direction;
            Vector3 meetPt_rate2 = t1_sub_end.origin + VOp.Normalize(new_dir_ls_seg1) * b_1;

            DebugWide.DrawCircle(meetPt_rate2, 0.08f, Color.green); //chamto test
            //DebugWide.DrawLine(meetPt_rate2, pos_t0_root, Color.red); //a
            //DebugWide.DrawLine(meetPt_rate2, t1_sub_end.origin, Color.red); //b
            //DebugWide.DrawLine(pos_t0_root, t1_sub_end.origin, Color.red); //c
            //float d_a = (meetPt_rate2 - pos_t0_root).magnitude;
            //float d_b = (meetPt_rate2 - t1_sub_end.origin).magnitude;
            //float d_c = (pos_t0_root - t1_sub_end.origin).magnitude;
            //DebugWide.LogBlue(d_a + "  " + d_b + "   " + d_c);
            //DebugWide.LogBlue(value_sign + "   " + b_1 + "   " + disc);
            //DebugWide.DrawCircle(pos_t0_root, d_a, Color.green);

            return meetPt_rate2;

        }


        public Vector3 CalcTGuard_FirstPt(Vector3 meetPt, Vector3 pos_t0_root,
                                LineSegment3 t0_sub_prev)
        {
            float a = (meetPt - pos_t0_root).magnitude;

            float sqrlen0 = (t0_sub_prev.origin - meetPt).sqrMagnitude;
            float sqrlen1 = (t0_sub_prev.last - meetPt).sqrMagnitude;

            LineSegment3 seg = t0_sub_prev;

            //방향결정하는 방법을 찾지 못함 , 두가지 상황이 모두 성립하므로 더 적합한것을 선택해야 한다 
            if (sqrlen0 > sqrlen1)
            {
                seg.origin = t0_sub_prev.last;
                seg.last = t0_sub_prev.origin;


                //DebugWide.LogBlue("seq change !!");
            }
            seg.origin = seg.origin + -seg.direction; //선분의 시작점이 원의 안쪽으로 들어가 접촉점 계산이 잘못되는 문제해결을 위해 선분연장 
            //한계검사를 통해 meetPt값이 조절되지 않으면 연장된 선분에 의해 교점검사 실패시 범위를 넘어가는 값이 나올 수 있다 

            //float ddd = (t0_sub_prev.origin - pos_t0_root).magnitude;
            //DebugWide.LogBlue(a + "   " + ddd);
            DebugWide.DrawCircle(pos_t0_root, a, Color.gray);

            Vector3 pt_first;
            if (true == Geo.IntersectLineSegment(pos_t0_root, a, seg, out pt_first))
            {
                DebugWide.DrawCircle(pt_first, 0.08f, Color.cyan); //chamto test    
            }
            else
            {
                //Vector3 n_left = VOp.Normalize(t0_sub_prev.direction);
                //float len_proj_left = Vector3.Dot(n_left, (meetPt - t0_sub_prev.origin));
                //pt_first = len_proj_left * n_left + t0_sub_prev.origin;
                //float aa = (pt_first - pos_t0_root).magnitude;
                //DebugWide.LogBlue(a + "   " + aa);

                DebugWide.DrawCircle(pt_first, 0.08f, Color.yellow); //chamto test    
            }

            return pt_first;
        }

        public Vector3 CalcTGuard_FirstPt2(Vector3 meetPt, Transform root_0,
                                           LineSegment3 t0_sub_prev)
        {

            float a = (meetPt - root_0.position).magnitude;

            //선분의 시작점이 원의 안쪽으로 들어가 접촉점 계산이 잘못되는 문제해결을 위해 선분연장 
            //한계검사를 통해 meetPt값이 조절되지 않으면 연장된 선분에 의해 교점검사 실패시 범위를 넘어가는 값이 나올 수 있다 
            LineSegment3 seg = t0_sub_prev;
            seg.last = seg.last + seg.direction;
            seg.origin = seg.origin + -seg.direction;

            //원과 접하는 두개의 교점을 찾느다 이중 meetPT와 가까운 교점이 찾으려는 점이다 , CalcTGuard_FirstPt 함수의 문제를 해결 
            Vector3 pt_first1, pt_first2;
            bool r1 = Geo.IntersectLineSegment(root_0.position, a, seg, out pt_first1);
            Vector3 temp = seg.origin;
            seg.origin = seg.last;
            seg.last = temp;
            bool r2 = Geo.IntersectLineSegment(root_0.position, a, seg, out pt_first2);

            float sqrlen1 = (pt_first1 - meetPt).sqrMagnitude;
            float sqrlen2 = (pt_first2 - meetPt).sqrMagnitude;
            bool r = r1;
            Vector3 closePt = pt_first1;
            if (sqrlen1 > sqrlen2)
            {
                closePt = pt_first2;
                r = r2;
            }

            //DebugWide.LogBlue(Mathf.Sqrt(sqrlen1) + " vs " + Mathf.Sqrt(sqrlen2) + "  ");
            //seg.Draw(Color.cyan);
            //DebugWide.DrawCircle(root_0.position, a, Color.cyan); //chamto test        
            //if(true == r)
            //    DebugWide.DrawCircle(closePt, 0.06f, Color.cyan); //chamto test        
            //else
            //DebugWide.DrawCircle(closePt, 0.06f, Color.yellow); //chamto test        

            return closePt;
        }

        public void RotateTGuard_FirstToLast(Vector3 meetPt_first, Vector3 meetPt_last,
                               Vector3 pos_t0_root,
                                LineSegment3 t0_sub_start,
                               out LineSegment3 new_t0_sub, out Quaternion localRot)
        {
            //---------------------------------------

            float angle_t = 0;
            localRot = Quaternion.identity;
            Vector3 up_t = Vector3.Cross(meetPt_first - pos_t0_root, meetPt_last - pos_t0_root);
            if (false == Misc.IsZero(up_t))
            {
                angle_t = Geo.AngleSigned(meetPt_first - pos_t0_root, meetPt_last - pos_t0_root, up_t);
                localRot = Quaternion.AngleAxis(angle_t, up_t);
                //DebugWide.LogBlue(meetPt_first + "   " + meetPt_last + " ---  "+ up_t + "  " + angle_t);
            }

            //DebugWide.LogBlue(" --  "+ up_t + "  " + angle_t);
            //--------

            new_t0_sub = t0_sub_start.Rotate(pos_t0_root, localRot);

            //---------------------------------------

        }



        //==================================================


        public void Update_Tetra(Vector3 a0_s, Vector3 a0_e, Vector3 a1_s, Vector3 a1_e,
                                 Vector3 b0_s, Vector3 b0_e, Vector3 b1_s, Vector3 b1_e)
        {
            _cur_seg_A = new LineSegment3(a1_s, a1_e);
            _cur_seg_B = new LineSegment3(b1_s, b1_e);

            //선분의 이동방향
            __dir_A = (a1_s - a0_s) + (a1_e - a0_e);
            __dir_B = (b1_s - b0_s) + (b1_e - b0_e);
            //선분상태인지 사각꼴 상태인지 검사 
            //__isSeg_A = Misc.IsZero(a0_s - a1_s) && Misc.IsZero(a0_e - a1_e);
            //__isSeg_B = Misc.IsZero(b0_s - b1_s) && Misc.IsZero(b0_e - b1_e);
            __isSeg_A = Misc.IsZero(__dir_A);
            __isSeg_B = Misc.IsZero(__dir_B);

            _tetr01.Set(a0_s, a0_e, a1_s, a1_e);
            _tetr23.Set(b0_s, b0_e, b1_s, b1_e);


            {
                _intr_0_2.Find_Twice();
                _intr_0_3.Find_Twice();
                _intr_1_2.Find_Twice();
                _intr_1_3.Find_Twice();
            }

            _prev_seg_A = new LineSegment3(a0_s, a0_e);
            _prev_seg_B = new LineSegment3(b0_s, b0_e);
        }



        public bool __isSeg_A, __isSeg_B;
        //float __cpS, __cpT;
        Vector3 __cpPt0;
        public Vector3 __dir_A = ConstV.v3_zero;
        public Vector3 __dir_B = ConstV.v3_zero;
        public Vector3 __prev_A_B_order = ConstV.v3_zero;
        public Vector3 __cur_A_B_order = ConstV.v3_zero;
        public Vector3 __cur_A_B_order2 = ConstV.v3_zero;


        public void Input_TGuard(LineSegment3 prev_segA, LineSegment3 prev_segB, LineSegment3 cur_segA, LineSegment3 cur_segB)
        {

            _cur_seg_A = cur_segA;
            _cur_seg_B = cur_segB;
            _prev_seg_A = prev_segA;
            _prev_seg_B = prev_segB;
            //--------------------

            //선분의 이동방향
            __dir_A = (_cur_seg_A.origin - _prev_seg_A.origin) + (_cur_seg_A.last - _prev_seg_A.last);
            __dir_B = (_cur_seg_B.origin - _prev_seg_B.origin) + (_cur_seg_B.last - _prev_seg_B.last);


            //방향이 같으면 0벡터가 되는 외적의 성질을 이용해 선분상태를 구함 
            Vector3 test;
            test = Vector3.Cross(__dir_A, _cur_seg_A.direction);
            __isSeg_A = Misc.IsZero(test);
            test = Vector3.Cross(__dir_B, _cur_seg_B.direction);
            __isSeg_B = Misc.IsZero(test);

            //DebugWide.LogBlue(__isSeg_A + "  " + __isSeg_B);

            //선분상태를 유지한 채 이동한 경우 선분으로 안나옴
            //__isSeg_B = Misc.IsZero(__dir_B);
            //__isSeg_A = Misc.IsZero(__dir_A);


            _tetr01.Set(_prev_seg_A, _cur_seg_A);
            _tetr23.Set(_prev_seg_B, _cur_seg_B);

        }
    }//end class


}