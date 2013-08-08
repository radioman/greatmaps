using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
public class SupportClass
{
	[Serializable]
	public class TextNumberFormat
	{
		private enum formatTypes
		{
			General,
			Number,
			Currency,
			Percent
		}
		private NumberFormatInfo numberFormat;
		private int numberFormatType;
		private bool groupingActivated;
		private string separator;
		private int maxIntDigits;
		private int minIntDigits;
		private int maxFractionDigits;
		private int minFractionDigits;
		public bool GroupingUsed
		{
			get
			{
				return this.groupingActivated;
			}
			set
			{
				this.groupingActivated = value;
			}
		}
		public int MinIntDigits
		{
			get
			{
				return this.minIntDigits;
			}
			set
			{
				this.minIntDigits = value;
			}
		}
		public int MaxIntDigits
		{
			get
			{
				return this.maxIntDigits;
			}
			set
			{
				this.maxIntDigits = value;
			}
		}
		public int MinFractionDigits
		{
			get
			{
				return this.minFractionDigits;
			}
			set
			{
				this.minFractionDigits = value;
			}
		}
		public int MaxFractionDigits
		{
			get
			{
				return this.maxFractionDigits;
			}
			set
			{
				this.maxFractionDigits = value;
			}
		}
		public TextNumberFormat()
		{
			this.numberFormat = new NumberFormatInfo();
			this.numberFormatType = 0;
			this.groupingActivated = true;
			this.separator = this.GetSeparator(0);
			this.maxIntDigits = 127;
			this.minIntDigits = 1;
			this.maxFractionDigits = 3;
			this.minFractionDigits = 0;
		}
		public void setMaximumIntegerDigits(int newValue)
		{
			this.maxIntDigits = newValue;
			if (newValue <= 0)
			{
				this.maxIntDigits = 0;
				this.minIntDigits = 0;
			}
			else
			{
				if (this.maxIntDigits < this.minIntDigits)
				{
					this.minIntDigits = this.maxIntDigits;
				}
			}
		}
		public void setMinimumIntegerDigits(int newValue)
		{
			this.minIntDigits = newValue;
			if (newValue <= 0)
			{
				this.minIntDigits = 0;
			}
			else
			{
				if (this.maxIntDigits < this.minIntDigits)
				{
					this.maxIntDigits = this.minIntDigits;
				}
			}
		}
		public void setMaximumFractionDigits(int newValue)
		{
			this.maxFractionDigits = newValue;
			if (newValue <= 0)
			{
				this.maxFractionDigits = 0;
				this.minFractionDigits = 0;
			}
			else
			{
				if (this.maxFractionDigits < this.minFractionDigits)
				{
					this.minFractionDigits = this.maxFractionDigits;
				}
			}
		}
		public void setMinimumFractionDigits(int newValue)
		{
			this.minFractionDigits = newValue;
			if (newValue <= 0)
			{
				this.minFractionDigits = 0;
			}
			else
			{
				if (this.maxFractionDigits < this.minFractionDigits)
				{
					this.maxFractionDigits = this.minFractionDigits;
				}
			}
		}
		private TextNumberFormat(SupportClass.TextNumberFormat.formatTypes theType, int digits)
		{
			this.numberFormat = NumberFormatInfo.CurrentInfo;
			this.numberFormatType = (int)theType;
			this.groupingActivated = true;
			this.separator = this.GetSeparator((int)theType);
			this.maxIntDigits = 127;
			this.minIntDigits = 1;
			this.maxFractionDigits = 3;
			this.minFractionDigits = 0;
		}
		private TextNumberFormat(SupportClass.TextNumberFormat.formatTypes theType, CultureInfo cultureNumberFormat, int digits)
		{
			this.numberFormat = cultureNumberFormat.NumberFormat;
			this.numberFormatType = (int)theType;
			this.groupingActivated = true;
			this.separator = this.GetSeparator((int)theType);
			this.maxIntDigits = 127;
			this.minIntDigits = 1;
			this.maxFractionDigits = 3;
			this.minFractionDigits = 0;
		}
		public static SupportClass.TextNumberFormat getTextNumberInstance()
		{
			return new SupportClass.TextNumberFormat(SupportClass.TextNumberFormat.formatTypes.Number, 3);
		}
		public static SupportClass.TextNumberFormat getTextNumberCurrencyInstance()
		{
			SupportClass.TextNumberFormat textNumberFormat = new SupportClass.TextNumberFormat(SupportClass.TextNumberFormat.formatTypes.Currency, 3);
			return textNumberFormat.setToCurrencyNumberFormatDefaults(textNumberFormat);
		}
		public static SupportClass.TextNumberFormat getTextNumberPercentInstance()
		{
			SupportClass.TextNumberFormat textNumberFormat = new SupportClass.TextNumberFormat(SupportClass.TextNumberFormat.formatTypes.Percent, 3);
			return textNumberFormat.setToPercentNumberFormatDefaults(textNumberFormat);
		}
		public static SupportClass.TextNumberFormat getTextNumberInstance(CultureInfo culture)
		{
			return new SupportClass.TextNumberFormat(SupportClass.TextNumberFormat.formatTypes.Number, culture, 3);
		}
		public static SupportClass.TextNumberFormat getTextNumberCurrencyInstance(CultureInfo culture)
		{
			SupportClass.TextNumberFormat textNumberFormat = new SupportClass.TextNumberFormat(SupportClass.TextNumberFormat.formatTypes.Currency, culture, 3);
			return textNumberFormat.setToCurrencyNumberFormatDefaults(textNumberFormat);
		}
		public static SupportClass.TextNumberFormat getTextNumberPercentInstance(CultureInfo culture)
		{
			SupportClass.TextNumberFormat textNumberFormat = new SupportClass.TextNumberFormat(SupportClass.TextNumberFormat.formatTypes.Percent, culture, 3);
			return textNumberFormat.setToPercentNumberFormatDefaults(textNumberFormat);
		}
		public object Clone()
		{
			return this;
		}
		public override bool Equals(object obj)
		{
			bool result;
			if (obj == null || base.GetType() != obj.GetType())
			{
				result = false;
			}
			else
			{
				SupportClass.TextNumberFormat textNumberFormat = (SupportClass.TextNumberFormat)obj;
				result = (this.numberFormat == textNumberFormat.numberFormat && this.numberFormatType == textNumberFormat.numberFormatType && this.groupingActivated == textNumberFormat.groupingActivated && this.separator == textNumberFormat.separator && this.maxIntDigits == textNumberFormat.maxIntDigits && this.minIntDigits == textNumberFormat.minIntDigits && this.maxFractionDigits == textNumberFormat.maxFractionDigits && this.minFractionDigits == textNumberFormat.minFractionDigits);
			}
			return result;
		}
		public override int GetHashCode()
		{
			return this.numberFormat.GetHashCode() ^ this.numberFormatType ^ this.groupingActivated.GetHashCode() ^ this.separator.GetHashCode() ^ this.maxIntDigits ^ this.minIntDigits ^ this.maxFractionDigits ^ this.minFractionDigits;
		}
		public string FormatDouble(double number)
		{
			string result;
			if (this.groupingActivated)
			{
				result = this.SetIntDigits(number.ToString(this.GetCurrentFormatString() + this.GetNumberOfDigits(number), this.numberFormat));
			}
			else
			{
				result = this.SetIntDigits(number.ToString(this.GetCurrentFormatString() + this.GetNumberOfDigits(number), this.numberFormat).Replace(this.separator, ""));
			}
			return result;
		}
		public string FormatLong(long number)
		{
			string result;
			if (this.groupingActivated)
			{
				result = this.SetIntDigits(number.ToString(this.GetCurrentFormatString() + this.minFractionDigits, this.numberFormat));
			}
			else
			{
				result = this.SetIntDigits(number.ToString(this.GetCurrentFormatString() + this.minFractionDigits, this.numberFormat).Replace(this.separator, ""));
			}
			return result;
		}
		private string SetIntDigits(string number)
		{
			string str = "";
			int i = number.IndexOf(this.numberFormat.NumberDecimalSeparator);
			string text;
			if (i > 0)
			{
				str = number.Substring(i);
				text = number.Substring(0, i).Replace(this.numberFormat.NumberGroupSeparator, "");
			}
			else
			{
				text = number.Replace(this.numberFormat.NumberGroupSeparator, "");
			}
			text = text.PadLeft(this.MinIntDigits, '0');
			if ((i = text.Length - this.MaxIntDigits) > 0)
			{
				text = text.Remove(0, i);
			}
			if (this.groupingActivated)
			{
				for (i = text.Length; i > 3; i -= 3)
				{
					text = text.Insert(i - 3, this.numberFormat.NumberGroupSeparator);
				}
			}
			text += str;
			string result;
			if (text.Length == 0)
			{
				result = "0";
			}
			else
			{
				result = text;
			}
			return result;
		}
		public static CultureInfo[] GetAvailableCultures()
		{
			return CultureInfo.GetCultures(CultureTypes.AllCultures);
		}
		private string GetCurrentFormatString()
		{
			string result = "n";
			switch (this.numberFormatType)
			{
			case 0:
				result = "n";
				break;
			case 1:
				result = "n";
				break;
			case 2:
				result = "c";
				break;
			case 3:
				result = "p";
				break;
			}
			return result;
		}
		private string GetSeparator(int numberFormatType)
		{
			string result = " ";
			switch (numberFormatType)
			{
			case 0:
				result = this.numberFormat.NumberGroupSeparator;
				break;
			case 1:
				result = this.numberFormat.NumberGroupSeparator;
				break;
			case 2:
				result = this.numberFormat.CurrencyGroupSeparator;
				break;
			case 3:
				result = this.numberFormat.PercentGroupSeparator;
				break;
			}
			return result;
		}
		private SupportClass.TextNumberFormat setToCurrencyNumberFormatDefaults(SupportClass.TextNumberFormat format)
		{
			format.maxFractionDigits = 2;
			format.minFractionDigits = 2;
			return format;
		}
		private SupportClass.TextNumberFormat setToPercentNumberFormatDefaults(SupportClass.TextNumberFormat format)
		{
			format.maxFractionDigits = 0;
			format.minFractionDigits = 0;
			return format;
		}
		private int GetNumberOfDigits(double number)
		{
			int num = 0;
			double num2 = Math.Abs(number);
			while (num2 % 1.0 > 0.0)
			{
				num2 *= 10.0;
				num++;
			}
			return (num < this.minFractionDigits) ? this.minFractionDigits : ((num < this.maxFractionDigits) ? num : this.maxFractionDigits);
		}
	}
	private class BackStringReader : StringReader
	{
		private char[] buffer;
		private int position = 1;
		public BackStringReader(string s) : base(s)
		{
			this.buffer = new char[this.position];
		}
		public override int Read()
		{
			int result;
			if (this.position >= 0 && this.position < this.buffer.Length)
			{
				result = (int)this.buffer[this.position++];
			}
			else
			{
				result = base.Read();
			}
			return result;
		}
		public override int Read(char[] array, int index, int count)
		{
			int num = this.buffer.Length - this.position;
			int result;
			if (count <= 0)
			{
				result = 0;
			}
			else
			{
				if (num > 0)
				{
					if (count < num)
					{
						num = count;
					}
					Array.Copy(this.buffer, this.position, array, index, num);
					count -= num;
					index += num;
					this.position += num;
				}
				if (count > 0)
				{
					count = base.Read(array, index, count);
					if (count == -1)
					{
						if (num == 0)
						{
							result = -1;
						}
						else
						{
							result = num;
						}
					}
					else
					{
						result = num + count;
					}
				}
				else
				{
					result = num;
				}
			}
			return result;
		}
		public void UnRead(int unReadChar)
		{
			this.position--;
			this.buffer[this.position] = (char)unReadChar;
		}
		public void UnRead(char[] array, int index, int count)
		{
			this.Move(array, index, count);
		}
		public void UnRead(char[] array)
		{
			this.Move(array, 0, array.Length - 1);
		}
		private void Move(char[] array, int index, int count)
		{
			for (int i = index + count; i >= index; i--)
			{
				this.UnRead((int)array[i]);
			}
		}
	}
	public class StreamTokenizerSupport
	{
		private const string TOKEN = "Token[";
		private const string NOTHING = "NOTHING";
		private const string NUMBER = "number=";
		private const string EOF = "EOF";
		private const string EOL = "EOL";
		private const string QUOTED = "quoted string=";
		private const string LINE = "], Line ";
		private const string DASH = "-.";
		private const string DOT = ".";
		private const int TT_NOTHING = -4;
		private const sbyte ORDINARYCHAR = 0;
		private const sbyte WORDCHAR = 1;
		private const sbyte WHITESPACECHAR = 2;
		private const sbyte COMMENTCHAR = 4;
		private const sbyte QUOTECHAR = 8;
		private const sbyte NUMBERCHAR = 16;
		private const int STATE_NEUTRAL = 0;
		private const int STATE_WORD = 1;
		private const int STATE_NUMBER1 = 2;
		private const int STATE_NUMBER2 = 3;
		private const int STATE_NUMBER3 = 4;
		private const int STATE_NUMBER4 = 5;
		private const int STATE_STRING = 6;
		private const int STATE_LINECOMMENT = 7;
		private const int STATE_DONE_ON_EOL = 8;
		private const int STATE_PROCEED_ON_EOL = 9;
		private const int STATE_POSSIBLEC_COMMENT = 10;
		private const int STATE_POSSIBLEC_COMMENT_END = 11;
		private const int STATE_C_COMMENT = 12;
		private const int STATE_STRING_ESCAPE_SEQ = 13;
		private const int STATE_STRING_ESCAPE_SEQ_OCTAL = 14;
		private const int STATE_DONE = 100;
		public const int TT_EOF = -1;
		public const int TT_EOL = 10;
		public const int TT_NUMBER = -2;
		public const int TT_WORD = -3;
		private sbyte[] attribute = new sbyte[256];
		private bool eolIsSignificant = false;
		private bool slashStarComments = false;
		private bool slashSlashComments = false;
		private bool lowerCaseMode = false;
		private bool pushedback = false;
		private int lineno = 1;
		private SupportClass.BackReader inReader;
		private SupportClass.BackStringReader inStringReader;
		private SupportClass.BackInputStream inStream;
		private StringBuilder buf;
		public double nval;
		public string sval;
		public int ttype;
		private int read()
		{
			int result;
			if (this.inReader != null)
			{
				result = this.inReader.Read();
			}
			else
			{
				if (this.inStream != null)
				{
					result = this.inStream.Read();
				}
				else
				{
					result = this.inStringReader.Read();
				}
			}
			return result;
		}
		private void unread(int ch)
		{
			if (this.inReader != null)
			{
				this.inReader.UnRead(ch);
			}
			else
			{
				if (this.inStream != null)
				{
					this.inStream.UnRead(ch);
				}
				else
				{
					this.inStringReader.UnRead(ch);
				}
			}
		}
		private void init()
		{
			this.buf = new StringBuilder();
			this.ttype = -4;
			this.WordChars(65, 90);
			this.WordChars(97, 122);
			this.WordChars(160, 255);
			this.WhitespaceChars(0, 32);
			this.CommentChar(47);
			this.QuoteChar(39);
			this.QuoteChar(34);
			this.ParseNumbers();
		}
		private void setAttributes(int low, int hi, sbyte attrib)
		{
			int num = Math.Max(0, low);
			int num2 = Math.Min(255, hi);
			for (int i = num; i <= num2; i++)
			{
				this.attribute[i] = attrib;
			}
		}
		private bool isWordChar(int data)
		{
			char c = (char)data;
			return data != -1 && (c > 'ÿ' || this.attribute[(int)c] == 1 || this.attribute[(int)c] == 16);
		}
		public StreamTokenizerSupport(StringReader reader)
		{
			string text = "";
			for (int num = reader.Read(); num != -1; num = reader.Read())
			{
				text += (char)num;
			}
			reader.Close();
			this.inStringReader = new SupportClass.BackStringReader(text);
			this.init();
		}
		public StreamTokenizerSupport(StreamReader reader)
		{
			this.inReader = new SupportClass.BackReader(new StreamReader(reader.BaseStream, reader.CurrentEncoding).BaseStream, 2, reader.CurrentEncoding);
			this.init();
		}
		public StreamTokenizerSupport(Stream stream)
		{
			this.inStream = new SupportClass.BackInputStream(new BufferedStream(stream), 2);
			this.init();
		}
		public virtual void CommentChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
			{
				this.attribute[ch] = 4;
			}
		}
		public virtual void EOLIsSignificant(bool flag)
		{
			this.eolIsSignificant = flag;
		}
		public virtual int Lineno()
		{
			return this.lineno;
		}
		public virtual void LowerCaseMode(bool flag)
		{
			this.lowerCaseMode = flag;
		}
		public virtual int NextToken()
		{
			char c = '\0';
			char c2 = '\0';
			int num = 0;
			int result;
			if (this.pushedback)
			{
				this.pushedback = false;
				result = this.ttype;
			}
			else
			{
				this.ttype = -4;
				int num2 = 0;
				this.nval = 0.0;
				this.sval = null;
				this.buf.Length = 0;
				do
				{
					int num3 = this.read();
					char c3 = c;
					c = (char)num3;
					switch (num2)
					{
					case 0:
						if (num3 == -1)
						{
							this.ttype = -1;
							num2 = 100;
						}
						else
						{
							if (c > 'ÿ')
							{
								this.buf.Append(c);
								this.ttype = -3;
								num2 = 1;
							}
							else
							{
								if (this.attribute[(int)c] == 4)
								{
									num2 = 7;
								}
								else
								{
									if (this.attribute[(int)c] == 1)
									{
										this.buf.Append(c);
										this.ttype = -3;
										num2 = 1;
									}
									else
									{
										if (this.attribute[(int)c] == 16)
										{
											this.ttype = -2;
											this.buf.Append(c);
											if (c == '-')
											{
												num2 = 2;
											}
											else
											{
												if (c == '.')
												{
													num2 = 4;
												}
												else
												{
													num2 = 3;
												}
											}
										}
										else
										{
											if (this.attribute[(int)c] == 8)
											{
												c2 = c;
												this.ttype = (int)c;
												num2 = 6;
											}
											else
											{
												if ((this.slashSlashComments || this.slashStarComments) && c == '/')
												{
													num2 = 10;
												}
												else
												{
													if (this.attribute[(int)c] == 0)
													{
														this.ttype = (int)c;
														num2 = 100;
													}
													else
													{
														if (c == '\n' || c == '\r')
														{
															this.lineno++;
															if (this.eolIsSignificant)
															{
																this.ttype = 10;
																if (c == '\n')
																{
																	num2 = 100;
																}
																else
																{
																	if (c == '\r')
																	{
																		num2 = 8;
																	}
																}
															}
															else
															{
																if (c == '\r')
																{
																	num2 = 9;
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
						break;
					case 1:
						if (this.isWordChar(num3))
						{
							this.buf.Append(c);
						}
						else
						{
							if (num3 != -1)
							{
								this.unread((int)c);
							}
							this.sval = this.buf.ToString();
							num2 = 100;
						}
						break;
					case 2:
						if (num3 == -1 || this.attribute[(int)c] != 16 || c == '-')
						{
							if (this.attribute[(int)c] == 4 && char.IsNumber(c))
							{
								this.buf.Append(c);
								num2 = 3;
							}
							else
							{
								if (num3 != -1)
								{
									this.unread((int)c);
								}
								this.ttype = 45;
								num2 = 100;
							}
						}
						else
						{
							this.buf.Append(c);
							if (c == '.')
							{
								num2 = 4;
							}
							else
							{
								num2 = 3;
							}
						}
						break;
					case 3:
						if (num3 == -1 || this.attribute[(int)c] != 16 || c == '-')
						{
							if (char.IsNumber(c) && this.attribute[(int)c] == 1)
							{
								this.buf.Append(c);
							}
							else
							{
								if (c == '.' && this.attribute[(int)c] == 2)
								{
									this.buf.Append(c);
								}
								else
								{
									if (num3 != -1 && this.attribute[(int)c] == 4 && char.IsNumber(c))
									{
										this.buf.Append(c);
									}
									else
									{
										if (num3 != -1)
										{
											this.unread((int)c);
										}
										try
										{
											this.nval = double.Parse(this.buf.ToString());
										}
										catch (FormatException)
										{
										}
										num2 = 100;
									}
								}
							}
						}
						else
						{
							this.buf.Append(c);
							if (c == '.')
							{
								num2 = 4;
							}
						}
						break;
					case 4:
						if (num3 == -1 || this.attribute[(int)c] != 16 || c == '-' || c == '.')
						{
							if (this.attribute[(int)c] == 4 && char.IsNumber(c))
							{
								this.buf.Append(c);
							}
							else
							{
								if (num3 != -1)
								{
									this.unread((int)c);
								}
								string text = this.buf.ToString();
								if (text.Equals("-."))
								{
									this.unread(46);
									this.ttype = 45;
								}
								else
								{
									if (text.Equals(".") && 1 == this.attribute[(int)c3])
									{
										this.ttype = 46;
									}
									else
									{
										try
										{
											this.nval = double.Parse(text);
										}
										catch (FormatException)
										{
										}
									}
								}
								num2 = 100;
							}
						}
						else
						{
							this.buf.Append(c);
							num2 = 5;
						}
						break;
					case 5:
						if (num3 == -1 || this.attribute[(int)c] != 16 || c == '-' || c == '.')
						{
							if (num3 != -1)
							{
								this.unread((int)c);
							}
							try
							{
								this.nval = double.Parse(this.buf.ToString());
							}
							catch (FormatException)
							{
							}
							num2 = 100;
						}
						else
						{
							this.buf.Append(c);
						}
						break;
					case 6:
						if (num3 == -1 || c == c2 || c == '\r' || c == '\n')
						{
							this.sval = this.buf.ToString();
							if (c == '\r' || c == '\n')
							{
								this.unread((int)c);
							}
							num2 = 100;
						}
						else
						{
							if (c == '\\')
							{
								num2 = 13;
							}
							else
							{
								this.buf.Append(c);
							}
						}
						break;
					case 7:
						if (num3 == -1)
						{
							this.ttype = -1;
							num2 = 100;
						}
						else
						{
							if (c == '\n' || c == '\r')
							{
								this.unread((int)c);
								num2 = 0;
							}
						}
						break;
					case 8:
						if (c != '\n' && num3 != -1)
						{
							this.unread((int)c);
						}
						num2 = 100;
						break;
					case 9:
						if (c != '\n' && num3 != -1)
						{
							this.unread((int)c);
						}
						num2 = 0;
						break;
					case 10:
						if (c == '*')
						{
							num2 = 12;
						}
						else
						{
							if (c == '/')
							{
								num2 = 7;
							}
							else
							{
								if (num3 != -1)
								{
									this.unread((int)c);
								}
								this.ttype = 47;
								num2 = 100;
							}
						}
						break;
					case 11:
						if (num3 == -1)
						{
							this.ttype = -1;
							num2 = 100;
						}
						else
						{
							if (c == '/')
							{
								num2 = 0;
							}
							else
							{
								if (c != '*')
								{
									num2 = 12;
								}
							}
						}
						break;
					case 12:
						if (c == '*')
						{
							num2 = 11;
						}
						if (c == '\n')
						{
							this.lineno++;
						}
						else
						{
							if (num3 == -1)
							{
								this.ttype = -1;
								num2 = 100;
							}
						}
						break;
					case 13:
						if (num3 == -1)
						{
							this.sval = this.buf.ToString();
							num2 = 100;
						}
						else
						{
							num2 = 6;
							if (c == 'a')
							{
								this.buf.Append(7);
							}
							else
							{
								if (c == 'b')
								{
									this.buf.Append('\b');
								}
								else
								{
									if (c == 'f')
									{
										this.buf.Append(12);
									}
									else
									{
										if (c == 'n')
										{
											this.buf.Append('\n');
										}
										else
										{
											if (c == 'r')
											{
												this.buf.Append('\r');
											}
											else
											{
												if (c == 't')
												{
													this.buf.Append('\t');
												}
												else
												{
													if (c == 'v')
													{
														this.buf.Append(11);
													}
													else
													{
														if (c >= '0' && c <= '7')
														{
															num = (int)(c - '0');
															num2 = 14;
														}
														else
														{
															this.buf.Append(c);
														}
													}
												}
											}
										}
									}
								}
							}
						}
						break;
					case 14:
						if (num3 == -1 || c < '0' || c > '7')
						{
							this.buf.Append((char)num);
							if (num3 == -1)
							{
								this.sval = this.buf.ToString();
								num2 = 100;
							}
							else
							{
								this.unread((int)c);
								num2 = 6;
							}
						}
						else
						{
							int num4 = num * 8 + (int)(c - '0');
							if (num4 < 256)
							{
								num = num4;
							}
							else
							{
								this.buf.Append((char)num);
								this.buf.Append(c);
								num2 = 6;
							}
						}
						break;
					}
				}
				while (num2 != 100);
				if (this.ttype == -3 && this.lowerCaseMode)
				{
					this.sval = this.sval.ToLower();
				}
				result = this.ttype;
			}
			return result;
		}
		public virtual void OrdinaryChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
			{
				this.attribute[ch] = 0;
			}
		}
		public virtual void OrdinaryChars(int low, int hi)
		{
			this.setAttributes(low, hi, 0);
		}
		public virtual void ParseNumbers()
		{
			for (int i = 48; i <= 57; i++)
			{
				this.attribute[i] = 16;
			}
			this.attribute[46] = 16;
			this.attribute[45] = 16;
		}
		public virtual void PushBack()
		{
			if (this.ttype != -4)
			{
				this.pushedback = true;
			}
		}
		public virtual void QuoteChar(int ch)
		{
			if (ch >= 0 && ch <= 255)
			{
				this.attribute[ch] = 8;
			}
		}
		public virtual void ResetSyntax()
		{
			this.OrdinaryChars(0, 255);
		}
		public virtual void SlashSlashComments(bool flag)
		{
			this.slashSlashComments = flag;
		}
		public virtual void SlashStarComments(bool flag)
		{
			this.slashStarComments = flag;
		}
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder("Token[");
			int num = this.ttype;
			switch (num)
			{
			case -4:
				stringBuilder.Append("NOTHING");
				break;
			case -3:
				stringBuilder.Append(this.sval);
				break;
			case -2:
				stringBuilder.Append("number=");
				stringBuilder.Append(this.nval);
				break;
			case -1:
				stringBuilder.Append("EOF");
				break;
			default:
				if (num == 10)
				{
					stringBuilder.Append("EOL");
				}
				break;
			}
			if (this.ttype > 0)
			{
				if (this.attribute[this.ttype] == 8)
				{
					stringBuilder.Append("quoted string=");
					stringBuilder.Append(this.sval);
				}
				else
				{
					stringBuilder.Append('\'');
					stringBuilder.Append((char)this.ttype);
					stringBuilder.Append('\'');
				}
			}
			stringBuilder.Append("], Line ");
			stringBuilder.Append(this.lineno);
			return stringBuilder.ToString();
		}
		public virtual void WhitespaceChars(int low, int hi)
		{
			this.setAttributes(low, hi, 2);
		}
		public virtual void WordChars(int low, int hi)
		{
			this.setAttributes(low, hi, 1);
		}
	}
	public class BackReader : StreamReader
	{
		private char[] buffer;
		private int position = 1;
		public BackReader(Stream streamReader, int size, Encoding encoding) : base(streamReader, encoding)
		{
			this.buffer = new char[size];
			this.position = size;
		}
		public BackReader(Stream streamReader, Encoding encoding) : base(streamReader, encoding)
		{
			this.buffer = new char[this.position];
		}
		public bool MarkSupported()
		{
			return false;
		}
		public void Mark(int position)
		{
			throw new IOException("Mark operations are not allowed");
		}
		public void Reset()
		{
			throw new IOException("Mark operations are not allowed");
		}
		public override int Read()
		{
			int result;
			if (this.position >= 0 && this.position < this.buffer.Length)
			{
				result = (int)this.buffer[this.position++];
			}
			else
			{
				result = base.Read();
			}
			return result;
		}
		public override int Read(char[] array, int index, int count)
		{
			int num = this.buffer.Length - this.position;
			int result;
			if (count <= 0)
			{
				result = 0;
			}
			else
			{
				if (num > 0)
				{
					if (count < num)
					{
						num = count;
					}
					Array.Copy(this.buffer, this.position, array, index, num);
					count -= num;
					index += num;
					this.position += num;
				}
				if (count > 0)
				{
					count = base.Read(array, index, count);
					if (count == -1)
					{
						if (num == 0)
						{
							result = -1;
						}
						else
						{
							result = num;
						}
					}
					else
					{
						result = num + count;
					}
				}
				else
				{
					result = num;
				}
			}
			return result;
		}
		public bool IsReady()
		{
			return this.position >= this.buffer.Length || this.BaseStream.Position >= this.BaseStream.Length;
		}
		public void UnRead(int unReadChar)
		{
			this.position--;
			this.buffer[this.position] = (char)unReadChar;
		}
		public void UnRead(char[] array, int index, int count)
		{
			this.Move(array, index, count);
		}
		public void UnRead(char[] array)
		{
			this.Move(array, 0, array.Length - 1);
		}
		private void Move(char[] array, int index, int count)
		{
			for (int i = index + count; i >= index; i--)
			{
				this.UnRead((int)array[i]);
			}
		}
	}
	public class BackInputStream : BinaryReader
	{
		private byte[] buffer;
		private int position = 1;
		public BackInputStream(Stream streamReader, int size) : base(streamReader)
		{
			this.buffer = new byte[size];
			this.position = size;
		}
		public BackInputStream(Stream streamReader) : base(streamReader)
		{
			this.buffer = new byte[this.position];
		}
		public bool MarkSupported()
		{
			return false;
		}
		public override int Read()
		{
			int result;
			if (this.position >= 0 && this.position < this.buffer.Length)
			{
				result = (int)this.buffer[this.position++];
			}
			else
			{
				result = base.Read();
			}
			return result;
		}
		public virtual int Read(sbyte[] array, int index, int count)
		{
			int num = count + index;
			byte[] array2 = SupportClass.ToByteArray(array);
			int num2 = 0;
			while (this.position < this.buffer.Length && index < num)
			{
				array2[index++] = this.buffer[this.position++];
				num2++;
			}
			if (index < num)
			{
				num2 += base.Read(array2, index, num - index);
			}
			for (int i = 0; i < array2.Length; i++)
			{
				array[i] = (sbyte)array2[i];
			}
			return num2;
		}
		public void UnRead(int element)
		{
			this.position--;
			if (this.position >= 0)
			{
				this.buffer[this.position] = (byte)element;
			}
		}
		public void UnRead(byte[] array, int index, int count)
		{
			this.Move(array, index, count);
		}
		public void UnRead(byte[] array)
		{
			this.Move(array, 0, array.Length - 1);
		}
		public long Skip(long numberOfBytes)
		{
			return this.BaseStream.Seek(numberOfBytes, SeekOrigin.Current) - this.BaseStream.Position;
		}
		private void Move(byte[] array, int index, int count)
		{
			for (int i = index + count; i >= index; i--)
			{
				this.UnRead((int)array[i]);
			}
		}
	}
	public static Random Random = new Random();
	public static byte[] ToByteArray(sbyte[] sbyteArray)
	{
		byte[] array = null;
		if (sbyteArray != null)
		{
			array = new byte[sbyteArray.Length];
			for (int i = 0; i < sbyteArray.Length; i++)
			{
				array[i] = (byte)sbyteArray[i];
			}
		}
		return array;
	}
	public static byte[] ToByteArray(string sourceString)
	{
		return Encoding.UTF8.GetBytes(sourceString);
	}
	public static byte[] ToByteArray(object[] tempObjectArray)
	{
		byte[] array = null;
		if (tempObjectArray != null)
		{
			array = new byte[tempObjectArray.Length];
			for (int i = 0; i < tempObjectArray.Length; i++)
			{
				array[i] = (byte)tempObjectArray[i];
			}
		}
		return array;
	}
	public static sbyte[] ToSByteArray(byte[] byteArray)
	{
		sbyte[] array = null;
		if (byteArray != null)
		{
			array = new sbyte[byteArray.Length];
			for (int i = 0; i < byteArray.Length; i++)
			{
				array[i] = (sbyte)byteArray[i];
			}
		}
		return array;
	}
	public static void WriteStackTrace(Exception throwable, TextWriter stream)
	{
		stream.Write(throwable.StackTrace);
		stream.Flush();
	}
	public static void Serialize(Stream stream, object objectToSend)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Serialize(stream, objectToSend);
	}
	public static void Serialize(BinaryWriter binaryWriter, object objectToSend)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Serialize(binaryWriter.BaseStream, objectToSend);
	}
	public static object Deserialize(BinaryReader binaryReader)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		return binaryFormatter.Deserialize(binaryReader.BaseStream);
	}
}
