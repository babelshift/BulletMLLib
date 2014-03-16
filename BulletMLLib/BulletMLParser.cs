//#define ExpandedBulletML

using System;
using System.Diagnostics;
using System.Xml;

namespace BulletMLLib
{
	public class BulletMLParser
	{
		public BulletMLTree tree;

		//static void Main(string[] args)
		//{
		//    Random r = new Random();
		//    BulletMLSystem.Init(r);
		//    BulletMLParser parser = new BulletMLParser();
		//    parser.ParseXML("test.xml");
		//    BulletMLSrc mover = new BulletMLSrc(parser.tree);
		//    for (int i = 0; i < 200; i++)
		//    {
		//        mover.Update();
		//    }
		//    Debug.Write("\n--end--\n");
		//    Debug.Read();

		//}

		public void ParseXML(string xmlFileName)
		{
			//Debug.WriteLine(" ----- " + xmlFileName + " ----- ");
			XmlReaderSettings settings = new XmlReaderSettings();

			settings.DtdProcessing = DtdProcessing.Ignore;

#if WINDOWS
            settings.ValidationType = ValidationType.DTD;
#endif

			BulletMLParser parser = new BulletMLParser();

			try
			{
				using (XmlReader reader = XmlReader.Create(xmlFileName, settings))
				{
					while (reader.Read())
					{
						switch (reader.NodeType)
						{
							case XmlNodeType.Element: // The node is an element.
								//Debug.Write("<" + reader.Name + ">\n");

								BulletMLTree element = new BulletMLTree();
								element.name = parser.StringToName(reader.Name);
								if (reader.HasAttributes)
								{
									element.type = parser.StringToType(reader.GetAttribute("type"));
									element.label = reader.GetAttribute("label");
#if ExpandedBulletML
                                element.visible = reader.GetAttribute("visible") == "false" ? false : true;
                                element.bulletName = reader.GetAttribute("name");
#endif
								}

								if (tree == null)
									tree = element;
								else
								{
									tree.children.Add(element);
									if (tree.children.Count > 1)
										tree.children[tree.children.Count - 2].next = tree.children[tree.children.Count - 1];

									element.parent = tree;
									if (!reader.IsEmptyElement)
										tree = element;
								}

								break;

							case XmlNodeType.Text: //Display the text in each element.

								//Debug.WriteLine(reader.Value +"\n");

								string line = reader.Value;
								string word = "";
								for (int i = 0; i < line.Length; i++)
								{
									float num;
									if (('0' <= line[i] && line[i] <= '9') || line[i] == '.')
									{
										word = word + line[i];
										if (i < line.Length - 1) //まだ続きがあれば
											continue;
									}

									if (word != "")
									{
										if (float.TryParse(word, out num))
										{
											tree.values.Add(new BulletValue(BLValueType.Number, num));
											word = "";
											//Debug.WriteLine("数値を代入" + num);
										}
										else
										{
											//Debug.WriteLine("構文にエラーがあります : " + line[i]);
										}
									}

									if (line[i] == '$')
									{
										if (line[i + 1] >= '0' && line[i + 1] <= '9')
										{
											tree.values.Add(new BulletValue(BLValueType.Param, Convert.ToInt32(line[i + 1].ToString())));
											i++;
											//Debug.WriteLine("パラメータを代入");
										}
										else if (line.Substring(i, 5) == "$rank")
										{
											//Debug.WriteLine("ランクを代入");
											i += 4;
											tree.values.Add(new BulletValue(BLValueType.Rank, 0));
										}
										else if (line.Substring(i, 5) == "$rand")
										{
											//Debug.WriteLine("Randを代入");
											i += 4;
											tree.values.Add(new BulletValue(BLValueType.Rand, 0));
										}
									}
									else if (line[i] == '*' || line[i] == '/' || line[i] == '+' || line[i] == '-' || line[i] == '(' || line[i] == ')')
									{
										tree.values.Add(new BulletValue(BLValueType.Operator, line[i]));
										//Debug.WriteLine("演算子を代入 " + line[i]);
									}
									else if (line[i] == ' ' || line[i] == '\n')
									{
									}
									else
									{
										//Debug.WriteLine("構文にエラーがあります : " + line[i]);
									}
								}

								break;

							case XmlNodeType.EndElement: //Display the end of the element.
								if (tree.parent != null)
									tree = tree.parent;

								//Debug.Write("</" + reader.Name + ">\n");
								break;
						}
					}
				}
			}
			catch (Exception e)
			{
				throw e;
			}
		}

		private string[] name2string = {
	        "bullet", "action", "fire", "changeDirection", "changeSpeed", "accel",
	        "wait", "repeat", "bulletRef", "actionRef", "fireRef", "vanish",
	        "horizontal", "vertical", "term", "times", "direction", "speed", "param",
	        "bulletml"
                                   };

		private BLType StringToType(string str)
		{
			if (str == "aim") return BLType.Aim;
			else if (str == "absolute") return BLType.Absolute;
			else if (str == "relative") return BLType.Relative;
			else if (str == "sequence") return BLType.Sequence;
			else if (str == null) return BLType.None;
			//else Debug.WriteLine("BulletML parser: unknown type " + str);

			return BLType.None;
		}

		//タグ文字列をBLNameに変換する
		private BLName StringToName(string str)
		{
			Debug.WriteLine(" tag " + str);
			if (str == "bulletml") return BLName.Bulletml;
			else if (str == "bullet") return BLName.Bullet;
			else if (str == "action") return BLName.Action;
			else if (str == "fire") return BLName.Fire;
			else if (str == "changeDirection") return BLName.ChangeDirection;
			else if (str == "changeSpeed") return BLName.ChangeSpeed;
			else if (str == "accel") return BLName.Accel;
			else if (str == "vanish") return BLName.Vanish;
			else if (str == "wait") return BLName.Wait;
			else if (str == "repeat") return BLName.Repeat;
			else if (str == "direction") return BLName.Direction;
			else if (str == "speed") return BLName.Speed;
			else if (str == "horizontal") return BLName.Horizontal;
			else if (str == "vertical") return BLName.Vertical;
			else if (str == "term") return BLName.Term;
			else if (str == "bulletRef") return BLName.BulletRef;
			else if (str == "actionRef") return BLName.ActionRef;
			else if (str == "fireRef") return BLName.FireRef;
			else if (str == "param") return BLName.Param;
			else if (str == "times") return BLName.Times;
			else if (str == "") return BLName.None;
			//else Debug.WriteLine("BulletML parser: unknown tag " + str);

			return BLName.None;
		}
	}
}