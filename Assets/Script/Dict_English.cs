using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using UtilGS9;


namespace XML_Data
{
	//string => hashValue
	//value문자열의 인덱스값으로 key해쉬 값을 찾는다 
	public class ValueToKeyMap
	{

		private Dictionary<int,int> _valueToKeyMap = new Dictionary<int, int> ();


		public void Add(int src_idx, int dst_idx)
		{
			if (_valueToKeyMap.ContainsKey (src_idx)) 
			{
				//키값이 이미 있으면 값을 갱신한다
				_valueToKeyMap [src_idx] = dst_idx;
			} else 
			{
				_valueToKeyMap.Add (src_idx, dst_idx);
			}
		}


		public int GetDstHash(int src_idx)
		{

			if (_valueToKeyMap.ContainsKey (src_idx)) 
			{
				return _valueToKeyMap [src_idx];
			}

			throw new KeyNotFoundException ();

			return -1; //존재하지 않는 키값
		}

	}

    public class Meaning : List<int> { }

    //<eng> 정보
    public class VocaInfo
    {
        public int groupNum = -1;
        public DictInfo.eKind kind = DictInfo.eKind.None;
        public int hashKey;

    }

    public class VocaInfoList : List<VocaInfo>
    {
        public VocaInfo GetVocaHashKey(int sequenceNum)
        {
            if(sequenceNum < base.Count)
                return this[sequenceNum];
            
            //잘못된 순서값
            return null;
        }
    }

	public class DictInfo
	{
		public enum eKind
		{
			None,
			Vocabulary,	//단어 
			Idiom,		//숙어 
			Sentence,	//문장
			Part,		//문장의 부분
		}

		
		private int _id_number = -1; //고유번호 
		private int _hashTitle = -1;

		private ValueToKeyMap _valueToKeyMap = new ValueToKeyMap();
		private Dictionary<int, Meaning> _data = new Dictionary<int, Meaning> ();

		//xml 데이터의 순서를 기록한다. 가사를 순서대로 재생하기 위하여 필요함 
        private Dictionary<eKind, VocaInfoList> _sequenceKind = new Dictionary<eKind, VocaInfoList>(); //<종류, 해쉬목록> 
        private Dictionary<int, VocaInfoList> _sequenceGroupNum = new Dictionary<int, VocaInfoList>(); //<그룹번호, 해쉬목록>

		public void SetID_Number(int number)
		{
			_id_number = number;
		}
		public int GetID_Number()
		{
			return _id_number;
		}

		public Dictionary<int, Meaning> GetData()
		{
			return _data;
		}

        public VocaInfoList GetSequence(eKind kind)
		{
			return _sequenceKind[kind];
		}

        public VocaInfoList GetSequence(int groupNum)
		{
			return _sequenceGroupNum[groupNum];
		}


		public string GetTitle()
		{
			return HashToStringMap.GetString(_hashTitle);
		}
		public void SetTitle(int hashTitle)
		{
			_hashTitle = hashTitle;
		}

		public void Add(int hashKey, Meaning mMore, int groupNum, eKind kind)
		{
			
			if (_data.ContainsKey (hashKey)) 
			{
				//키값이 이미 있으면 값을 갱신한다
				_data [hashKey] = mMore;
			} else 
			{
				_data.Add (hashKey, mMore);
			}

			foreach (int mOne in mMore) 
			{
				//DebugWide.LogBlue (mOne +  " ----  " + hashKey);
				_valueToKeyMap.Add (mOne, hashKey);
			}

			VocaInfo voca = new VocaInfo ();
			voca.groupNum = groupNum;
			voca.kind = kind;
			voca.hashKey = hashKey;
			this.AddSequence_Kind(voca); //순서저장
			this.AddSequence_GroupNum(voca);
		}

		public void AddSequence_Kind(VocaInfo voca )
		{
			//DebugWide.LogBlue ("AddSequence ............ " + kind + "__" + hashKey ); //chamto test
			
            VocaInfoList getList = null;
			if (false == _sequenceKind.TryGetValue (voca.kind, out getList)) 
			{
                getList = new VocaInfoList ();
				_sequenceKind.Add (voca.kind, getList);
			}
			getList.Add (voca);
		}

		public void AddSequence_GroupNum(VocaInfo voca)
		{
            VocaInfoList getList = null;
			if (false == _sequenceGroupNum.TryGetValue (voca.groupNum, out getList)) 
			{
                getList = new VocaInfoList ();
				_sequenceGroupNum.Add (voca.groupNum, getList);
			}
			getList.Add (voca);
		}


		public Meaning GetMeaningFromKey(int hashKey)
		{
			if (_data.ContainsKey (hashKey)) 
			{
				return _data [hashKey];
			}

			throw new KeyNotFoundException ();

			return null; //존재하지 않는 키값
		}

		public Meaning GetMeaningFromKey(string textKey)
		{
			return this.GetMeaningFromKey (textKey.GetHashCode ());
		}

		public string GetMeaningFromKey_First(string textKey)
		{
			int hashValue = GetMeaningFromKey (textKey).ElementAt (0); 

			return HashToStringMap.GetString (hashValue);
		}


		public string GetTextFromValue(int hashValue)
		{
			int hashKey = _valueToKeyMap.GetDstHash (hashValue);
			return HashToStringMap.GetString (hashKey);
		}

		public string GetTextFromValue(string meaningValue)
		{
			return this.GetTextFromValue (meaningValue.GetHashCode());
		}


	}
	
	public class Dict_English
	{

		private string m_strFileName = "Dict_English.xml";

		private bool _bCompleteLoad = false;
		public bool bCompleteLoad
		{
			get { return _bCompleteLoad; }
		}

		public Dictionary<int, DictInfo> _dictInfoMap = new Dictionary<int, DictInfo>();


        //글종류에 해당하는 전체 목록을 반환 
        public VocaInfoList GetVocaInfoList(int XML_dictInfoNum, XML_Data.DictInfo.eKind eKind)
        {
            return this._dictInfoMap[XML_dictInfoNum].GetSequence(eKind);
        }

        //글묶음 번호에 해당하는 목록을 반환 
        public VocaInfoList GetVocaInfoGroup(int XML_dictInfoNum, int groupNum)
        {
            return this._dictInfoMap[XML_dictInfoNum].GetSequence(groupNum);
        }



		public void Print ()
		{
            DebugWide.LogBlue ("==================== "+ m_strFileName +" ==================== " +_dictInfoMap.Count);
			foreach(DictInfo info in _dictInfoMap.Values)
			{
				DebugWide.LogBlue ("=== DictInfo - title : " + info.GetTitle ());

				foreach (KeyValuePair<int,Meaning> kv in info.GetData()) 
				{
					DebugWide.LogBlue ("========= DictInfo - <eng> : " + HashToStringMap.GetString(kv.Key));

					foreach (int v in kv.Value) 
					{
						DebugWide.LogBlue ("========= DictInfo - <kor> : " + HashToStringMap.GetString(v));
					}
				}

			}
		}

		public void ClearAll()
		{
			_bCompleteLoad = false;
			_dictInfoMap.Clear ();
		}

		public IEnumerator LoadXML()
		{
			//내부 코루틴 부분
			//------------------------------------------------------------------------
			//DebugWide.LogBlue(GlobalConstants.ASSET_PATH + m_strFileName); //chamto test
			MemoryStream stream = null;
            //IEnumerator irator = this.FileLoading(ConstV.ASSET_PATH + m_strFileName, value => stream = value);
            IEnumerator irator = UtilGS9.FileToStream.FileLoading(PathInfo.WWW_PATH + m_strFileName, null);
			yield return irator;

			stream = irator.Current as MemoryStream; //이뮬레이터의 양보반환값을 가져온다
			if (null == stream)
			{
				DebugWide.Log("error : failed LoadFromFile : " + PathInfo.WWW_PATH + m_strFileName);
				yield break;
			}
			this.Parse_MemoryStream (stream);


			//Print(); //chamto test

		}

		private void Parse_MemoryStream(MemoryStream stream)
		{
			_bCompleteLoad = false;
			_dictInfoMap.Clear();

			//------------------------------------------------------------------------
			//<root>
			//			<DictInfo title="start_of_game" >
			//				<eng num="0" kind="vocabulary" text="memory">
			//					<kor meaning="추억"  />
			//					<kor meaning="기억"  />
			//				</eng>
			//				<eng num="1" kind="idiom" text="used to ~">
			//					<kor meaning="뭐뭐 하곤 했다" />
			//					<kor meaning="한때는 뭐뭐 이었다" />
			//					<kor meaning="뭐뭐에 익숙하다" />
			//				</eng>
			//			</DictInfo>
			//</root>

			XmlDocument Xmldoc = new XmlDocument();
			Xmldoc.Load(stream);

			XmlElement root_element = Xmldoc.DocumentElement; 	//<root>		
			XmlNodeList secondList = root_element.ChildNodes;	//<DictInfo>
			XmlNodeList thirdList = null;	//<eng>
			XmlNodeList fourthList = null;	//<kor>
			XmlAttributeCollection attrs = null;
			XmlNode xmlNode = null;
			//Debug.Log ("loadXML : " + secondList.Count); //chamto test
			DictInfo item = null;
			for (int i = 0; i < secondList.Count; ++i) 
			{
				//DebugWide.LogBlue(secondList[i] + " ------- i:" + i);
				if (false == (secondList [i] is System.Xml.XmlElement)) //System.Xml.XmlComment 주석일 때는 처리 하지 않는다
					continue;
				
				item = new DictInfo();

				//==================== <DictInfo title> ====================
				xmlNode = secondList[i].Attributes.GetNamedItem("num");
				item.SetID_Number (int.Parse (xmlNode.Value));
				xmlNode = secondList[i].Attributes.GetNamedItem("title");
                HashToStringMap.Add (xmlNode.Value.GetHashCode (), xmlNode.Value);
				item.SetTitle(xmlNode.Value.GetHashCode());


				Meaning meaning = null;
				int 			groupNum = -1;
				DictInfo.eKind 	eKind = DictInfo.eKind.None;
				//==================== <eng> ====================
				thirdList = secondList[i].ChildNodes; //<DictInfo>
				for (int j = 0; j < thirdList.Count; ++j) 
				{
					//DebugWide.LogBlue(thirdList[j] + " ------- j:" + j);
					if (false == (thirdList [j] is System.Xml.XmlElement)) //System.Xml.XmlComment 주석일 때는 처리 하지 않는다
						continue;
					
					//==================== <kor> ====================
					meaning = new Meaning ();
					fourthList = thirdList[j].ChildNodes; //<kor>
					for (int k = 0; k < fourthList.Count; ++k) 
					{
						//DebugWide.LogBlue(fourthList[k] + " ------- k:" + k);
						if (false == (fourthList [k] is System.Xml.XmlElement)) //System.Xml.XmlComment 주석일 때는 처리 하지 않는다
							continue;
						
						attrs = fourthList[k].Attributes;
						foreach(XmlNode n in attrs)
						{
							switch(n.Name)
							{
							case "meaning":
                                    HashToStringMap.Add (n.Value.GetHashCode (), n.Value);
    								meaning.Add (n.Value.GetHashCode ());
    								//DebugWide.LogBlue ("     <kor> " + n.Name + "  =  " + n.Value + "  " + eKind.ToString());
								break;
							}
						}
					}
					//====================//====================
					int textHashCode = 0; //공백문자의 해쉬값이 0 이다
					attrs = thirdList[j].Attributes; //<eng>
					foreach(XmlNode n in attrs)
					{
						switch(n.Name)
						{
						case "group":
							groupNum = int.Parse (n.Value);
							break;
						case "kind":
							if ("vocabulary" == n.Value)
								eKind = DictInfo.eKind.Vocabulary;
							if ("idiom" == n.Value)
								eKind = DictInfo.eKind.Idiom;
							if ("sentence" == n.Value)
								eKind = DictInfo.eKind.Sentence;
							if ("part" == n.Value)
								eKind = DictInfo.eKind.Part;
							
							//DebugWide.LogGreen (eKind + "____");
							break;
						case "text": //kind -> text 순일때만 정상처리함
							{
								textHashCode = n.Value.GetHashCode ();
                                    HashToStringMap.Add (textHashCode, n.Value);
								//DebugWide.LogBlue ("<eng> " + n.Name + "  =  " + n.Value );
							}
							break;
						}
					}

					if(0 != textHashCode)
						item.Add (textHashCode, meaning ,groupNum, eKind);

				}
				//DebugWide.LogBlue (xmlNode.Value + "  ----  " + xmlNode.Value.GetHashCode());
				_dictInfoMap.Add(item.GetID_Number(), item);
			}

			_bCompleteLoad = true;
		}//func end


	}//class end

}//namespace end


