using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TRS parser.
/// T : Translation *  R : Rotate * S : Scale
/// multiOrder : "s(x y z) rz0 ry0 rx0 t(x y z)" 
/// </summary>
public class TRSParser : List<TRSParser.Sentences>
{

	//text > sentence > commansList > command

	public class Sentences : List<Command> 
	{
		public enum eKind
		{
			None = 0,
			Translate,
			Rotate,
			Quaternion,
			Scale,

		}

		public eKind kind = eKind.None;
		public string text = "";

	}

	public class Command
	{
		public enum eKind
		{
			None = 0,
			X,
			Y,
			Z,
			XYZ,
		}
		
		public Command(eKind kind, float degree) { this.degree = degree; this.kind = kind; }
		public Command(Vector3 xyz) { this.xyz = xyz; kind = eKind.XYZ;}

		public eKind kind;
		public float degree;
		public Vector3 xyz;
	}

	//Parsing -> _incision -> SentenceDecomposition -> CommandDecomposition

	//"s(x y z) rz0 ry0 rx0 t(x y z)";
	//"s(x y z) q(x y z) t(x y z)";
	public void Parsing(string text)
	{
		this.Clear ();
		List<string> list = this._incision (text);

		foreach (string stsText in list) 
		{
			this.Add ( this.SentenceDecomposition (stsText));
		}
	}

	//자르기 - 명령어 구분하기
	private List<string> _incision(string text)
	{
		int L = text.Length - 1;

		int start = -1;
		int end = -1;

		List<string> list = new List<string> ();

		for(int i=0;i<text.Length;i++)
		{
			//scale , translate , quaternion
			if ('s' == text[i] || 't' == text[i] || 'q' == text[i]) 
			{
				start = text.IndexOf ('(', i);
				end = text.IndexOf (')', start);

				list.Add( text.Substring (i, end - i + 1));
			}

			//rotate
			if ('r' == text [i]) 
			{
				end = -1;
				if (L >= i + 1) 
				{
					if('x' == text [i + 1] || 'y' == text [i + 1] || 'z' == text [i + 1]) 
					{
						end = text.IndexOf (' ', i);
						if (end < 0)
							end = L+1;

						list.Add( text.Substring (i, end - i));
						//DebugWide.LogBlue ("  : " + end + " " + i + "  " + (end - i)); //chamto test

					}
				}
			}
		}

		return list;
	}

	//문장 분해
	public Sentences SentenceDecomposition(string text)
	{
		Sentences sts = new Sentences ();
		sts.text = text;
		if ('t' == text [0]) 
		{
			sts.kind = Sentences.eKind.Translate;
		}
		if ('r' == text [0]) 
		{
			sts.kind = Sentences.eKind.Rotate;
		}
		if ('q' == text [0]) 
		{
			sts.kind = Sentences.eKind.Quaternion;
		}
		if ('s' == text [0]) 
		{
			sts.kind = Sentences.eKind.Scale;
		}
		CommandDecomposition (sts);

		return sts;
	}

	//명령어 분해
	//"s(x y z) rz0 ry0 rx0 t(x y z)";
	public void CommandDecomposition(Sentences sts)
	{
		//int L = sts.text - 1;

		//int start = -1;
		string temp = "";
		string[] split = null;
		Vector3 xyz = Vector3.zero;

		if (Sentences.eKind.Translate == sts.kind || Sentences.eKind.Scale == sts.kind || Sentences.eKind.Quaternion == sts.kind)
		{
			temp = sts.text.Trim ('t','s','q', ')',' ');
			temp = temp.Trim ('(', ' ');
			temp = temp.Trim ();
			split = temp.Split(' ');

			foreach(string s in split)
			{
				if (s.Length != 0 && 'x' == s[0]) 
				{
					temp = s.TrimStart ('x');
					if (false == float.TryParse (temp, out xyz.x)) {
						xyz.x = 0;
					}
				}
				if (s.Length != 0 && 'y' == s[0]) 
				{
					temp = s.TrimStart ('y');
					if (false == float.TryParse (temp, out xyz.y)) {
						xyz.y = 0;
					}
				}
				if (s.Length != 0 && 'z' == s[0]) 
				{
					temp = s.TrimStart ('z');
					if (false == float.TryParse (temp, out xyz.z)) {
						xyz.z = 0;
					}
				}
			}
			sts.Add (new Command (xyz));
		}
		if (Sentences.eKind.Rotate == sts.kind) 
		{
			temp = sts.text.Trim ('r',' ');

			if (temp.Length != 0 && 'x' == temp[0]) 
			{
				float value = 0;
				temp = temp.TrimStart ('x');
				if (false == float.TryParse (temp, out value)) {
					value = 0;
				}
				sts.Add(new Command(TRSParser.Command.eKind.X, value));
			}
			if (temp.Length != 0 && 'y' == temp[0]) 
			{
				float value = 0;
				temp = temp.TrimStart ('y');
				if (false == float.TryParse (temp, out value)) {
					value = 0;
				}
				sts.Add(new Command(TRSParser.Command.eKind.Y, value));
			}
			if (temp.Length != 0 && 'z' == temp[0]) 
			{
				float value = 0;
				temp = temp.TrimStart ('z');
				if (false == float.TryParse (temp, out value)) {
					value = 0;
				}
				sts.Add(new Command(TRSParser.Command.eKind.Z, value));
			}

		}
	}



	public void TestPrint(string text)
	{
		List<string> list = this._incision (text);
		foreach (string s in list) 
		{
			DebugWide.LogBlue (s);
		}
	}

}

//----------------------------------------------------------------------
//----------------------------------------------------------------------
//----------------------------------------------------------------------


static public class TRSHelper
{

	static public Quaternion GetQuaternion(TRSParser parser)
	{
		Quaternion u_q = Quaternion.identity;


		foreach (TRSParser.Sentences sts in parser) 
		{
			if (0 == sts.Count)
				continue;

			switch (sts.kind) 
			{

			case TRSParser.Sentences.eKind.Quaternion:
				{
					///1.
					//u_q.eulerAngles = sts[0].xyz;


					///2.
					IvQuat iv_q = new IvQuat ();
					Vector3 v3Rad = sts [0].xyz * Mathf.Deg2Rad;
					iv_q.Set (v3Rad.z , v3Rad.y , v3Rad.x); 

					u_q.w = iv_q.w;
					u_q.x = iv_q.x;
					u_q.y = iv_q.y;
					u_q.z = iv_q.z;
				}
				break;
			
			}
		}

		return u_q;
	}

	static public Matrix4x4 ParsingMatrix(TRSParser parser , string order)
	{
		parser.Parsing(order);

		Matrix4x4 trs = Matrix4x4.identity;
		foreach (TRSParser.Sentences sts in parser) 
		{
			if (0 == sts.Count)
				continue;

			switch (sts.kind) 
			{
			case TRSParser.Sentences.eKind.Translate:
				{
					trs = trs * TRSHelper.GetTranslate (sts [0].xyz);
				}
				break;
			case TRSParser.Sentences.eKind.Rotate:
				{
					if (TRSParser.Command.eKind.X == sts [0].kind)
						trs = trs * TRSHelper.GetRotateX (sts [0].degree);
					if (TRSParser.Command.eKind.Y == sts [0].kind)
						trs = trs * TRSHelper.GetRotateY (sts [0].degree);
					if (TRSParser.Command.eKind.Z == sts [0].kind)
						trs = trs * TRSHelper.GetRotateZ (sts [0].degree);
				}
				break;
			case TRSParser.Sentences.eKind.Quaternion:
				{
					IvQuat q = new IvQuat ();
					Vector3 v3Rad = sts [0].xyz * Mathf.Deg2Rad;
					q.Set (v3Rad.z , v3Rad.y , v3Rad.x); 
					Matrix4x4 m = q.GetMatrix ();
					trs = trs * m;

					//chamto test - print fixedAngles of IvQuat
					Vector3 angles = IvQuat.GetFixedAnglesFrom (ref m);
					angles *= Mathf.Rad2Deg;
					DebugWide.LogRed ("1: IvQuat angles : "+angles + "\n"); //chamto test

				}
				break;
			case TRSParser.Sentences.eKind.Scale:
				trs = trs * TRSHelper.GetScale (sts [0].xyz);
				break;
			}
		}

		return trs;

	}

	// 유니티엔진의 회전행렬 결합 순서 : 오일러각 : y(월드) => x(로컬) => z(로컬)

	//열우선 행렬 : v' = m * v
	//v1: 00 01 02 03
	//v2: 10 11 12 13
	//v3: 20 21 22 23
	//v4: 30 31 32 33

	static public Matrix4x4 GetScale(Vector3 scale)
	{
		//s1 : 00
		//s2 : 11
		//s3 : 22

		Matrix4x4 m = Matrix4x4.identity;
		m.m00 = scale.x;
		m.m11 = scale.y;
		m.m22 = scale.z;

		return m;
	}

	static public Matrix4x4 GetTranslate(Vector3 pos)
	{
		//t1 : 03
		//t2 : 13
		//t3 : 23

		Matrix4x4 m = Matrix4x4.identity;
		m.SetColumn (3, new Vector4 (pos.x, pos.y, pos.z, 1));

		return m;
	}

	static public Matrix4x4 GetRotateX(float degree)
	{
		// 1   0    0 
		// 0  cos -sin 
		// 0  sin  cos

		float theta = degree * Mathf.Deg2Rad;
		float cos = Mathf.Cos (theta);
		float sin = Mathf.Sin (theta);

		Matrix4x4 m = Matrix4x4.identity;
		m.SetRow (0, new Vector4 (1,   0,    0, 0));
		m.SetRow (1, new Vector4 (0, cos, -sin, 0));
		m.SetRow (2, new Vector4 (0, sin,  cos, 0));
		m.SetRow (3, new Vector4 (0,   0,    0, 1));

		//m = m.transpose; //chamto test
		return m;

	}

	static public Matrix4x4 GetRotateY(float degree)
	{
		// cos  0  sin
		//  0   1   0
		//-sin  0  cos

		float theta = degree * Mathf.Deg2Rad;
		float cos = Mathf.Cos (theta);
		float sin = Mathf.Sin (theta);

		Matrix4x4 m = Matrix4x4.identity;
		m.SetRow (0, new Vector4 ( cos,  0,  sin, 0));
		m.SetRow (1, new Vector4 (   0,  1,    0, 0));
		m.SetRow (2, new Vector4 (-sin,  0,  cos, 0));
		m.SetRow (3, new Vector4 (   0,  0,    0, 1));

		//m = m.transpose; //chamto test
		return m;

	}

	static public Matrix4x4 GetRotateZ(float degree)
	{
		// cos -sin  0
		// sin  cos  0
		//  0    0   1

		float theta = degree * Mathf.Deg2Rad;
		float cos = Mathf.Cos (theta);
		float sin = Mathf.Sin (theta);

		Matrix4x4 m = Matrix4x4.identity;
		m.SetRow (0, new Vector4 (cos, -sin, 0, 0));
		m.SetRow (1, new Vector4 (sin,  cos, 0, 0));
		m.SetRow (2, new Vector4 (  0,    0, 1, 0));
		m.SetRow (3, new Vector4 (  0,    0, 0, 1));

		//m = m.transpose; //chamto test
		return m;
	}
}
