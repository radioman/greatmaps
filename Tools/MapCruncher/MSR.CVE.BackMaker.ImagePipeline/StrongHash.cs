using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class StrongHash : IRobustHash
	{
		public const string xmlTag = "StrongHash";
		private const string ValueAttr = "Value";
		private MemoryStream ms = new MemoryStream();
		private byte[] hashValue;
		private int shortHashValue;
		private static UTF8Encoding encoding = new UTF8Encoding();
		public static bool operator ==(StrongHash h1, StrongHash h2)
		{
			return h1.Equals(h2);
		}
		public static bool operator !=(StrongHash h1, StrongHash h2)
		{
			return !h1.Equals(h2);
		}
		private void accBytes(byte[] buf)
		{
			this.ms.Write(buf, 0, buf.Length);
		}
		public void Accumulate(int input)
		{
			this.accBytes(BitConverter.GetBytes(input));
		}
		public void Accumulate(long input)
		{
			this.accBytes(BitConverter.GetBytes(input));
		}
		public void Accumulate(Size size)
		{
			this.Accumulate(size.Width);
			this.Accumulate(size.Height);
		}
		public void Accumulate(double value)
		{
			this.Accumulate(value.GetHashCode());
		}
		public void Accumulate(string value)
		{
			this.accBytes(StrongHash.encoding.GetBytes(value));
		}
		public void Accumulate(bool value)
		{
			this.accBytes(BitConverter.GetBytes(value));
		}
		public override int GetHashCode()
		{
			this.DoHash();
			return this.shortHashValue;
		}
		private void DoHash()
		{
			if (this.hashValue == null)
			{
				HashAlgorithm hashAlgorithm = new SHA1Managed();
				byte[] array = this.ms.ToArray();
				hashAlgorithm.ComputeHash(array, 0, array.Length);
				this.hashValue = hashAlgorithm.Hash;
				this.ComputeShortHashValue();
				this.ms.Dispose();
				this.ms = null;
			}
		}
		private void ComputeShortHashValue()
		{
			int num = 0;
			for (int i = 0; i < this.hashValue.Length; i++)
			{
				num = num * 131 + (int)this.hashValue[i];
			}
			this.shortHashValue = num;
		}
		public override bool Equals(object obj)
		{
			if (obj is StrongHash)
			{
				StrongHash strongHash = (StrongHash)obj;
				this.DoHash();
				strongHash.DoHash();
				return this.ArraysEqual(strongHash);
			}
			return false;
		}
		private bool ArraysEqual(StrongHash rh2)
		{
			bool result = true;
			if (this.hashValue.Length != rh2.hashValue.Length)
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < this.hashValue.Length; i++)
				{
					if (this.hashValue[i] != rh2.hashValue[i])
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}
		public override string ToString()
		{
			StrongHash strongHash = new StrongHash();
			strongHash.ms.Write(this.ms.GetBuffer(), 0, (int)this.ms.Length);
			strongHash.DoHash();
			return StrongHash.ByteArrayToHex(strongHash.hashValue);
		}
		public static string ByteArrayToHex(byte[] byteArray)
		{
			StringBuilder stringBuilder = new StringBuilder(byteArray.Length * 2);
			for (int i = 0; i < byteArray.Length; i++)
			{
				byte b = byteArray[i];
				int index = b >> 4 & 15;
				int index2 = (int)(b & 15);
				stringBuilder.Append("0123456789ABCDEF"[index]);
				stringBuilder.Append("0123456789ABCDEF"[index2]);
			}
			return stringBuilder.ToString();
		}
		public StrongHash()
		{
		}
		public StrongHash(MashupParseContext context)
		{
			XMLTagReader xMLTagReader = context.NewTagReader("StrongHash");
			this.hashValue = Convert.FromBase64String(context.GetRequiredAttribute("Value"));
			this.ComputeShortHashValue();
			xMLTagReader.SkipAllSubTags();
		}
		public void WriteXML(XmlTextWriter writer)
		{
			this.DoHash();
			writer.WriteStartElement("StrongHash");
			writer.WriteAttributeString("Value", Convert.ToBase64String(this.hashValue));
			writer.WriteEndElement();
		}
	}
}
