using System;
using System.Globalization;
using System.Runtime;
using System.Text;
using System.Threading;
namespace System
{
	public class UriBuilder
	{
		private bool m_changed = true;
		private string m_fragment = string.Empty;
		private string m_host = "localhost";
		private string m_password = string.Empty;
		private string m_path = "/";
		private int m_port = -1;
		private string m_query = string.Empty;
		private string m_scheme = "http";
		private string m_schemeDelimiter = Uri.SchemeDelimiter;
		private Uri m_uri;
		private string m_username = string.Empty;
		private string Extra
		{
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (value.Length <= 0)
				{
					this.Fragment = string.Empty;
					this.Query = string.Empty;
					return;
				}
				if (value[0] == '#')
				{
					this.Fragment = value.Substring(1);
					return;
				}
				if (value[0] == '?')
				{
					int num = value.IndexOf('#');
					if (num == -1)
					{
						num = value.Length;
					}
					else
					{
						this.Fragment = value.Substring(num + 1);
					}
					this.Query = value.Substring(1, num - 1);
					return;
				}
				throw new ArgumentException("value");
			}
		}

		public string Fragment
		{
			get
			{
				return this.m_fragment;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (value.Length > 0)
				{
					value = '#' + value;
				}
				this.m_fragment = value;
				this.m_changed = true;
			}
		}

		public string Host
		{
			get
			{
				return this.m_host;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				this.m_host = value;
				if (this.m_host.IndexOf(':') >= 0 && this.m_host[0] != '[')
				{
					this.m_host = "[" + this.m_host + "]";
				}
				this.m_changed = true;
			}
		}
		
		public string Password
		{
			get
			{
				return this.m_password;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				this.m_password = value;
				this.m_changed = true;
			}
		}
		
		public string Path
		{
			get
			{
				return this.m_path;
			}
			set
			{
				if (value == null || value.Length == 0)
				{
					value = "/";
				}
				this.m_path = Uri.EscapeUriString(this.ConvertSlashes(value));
				this.m_changed = true;
			}
		}

		public int Port
		{
			get
			{
				return this.m_port;
			}
			set
			{
				if (value < -1 || value > 65535)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				this.m_port = value;
				this.m_changed = true;
			}
		}

		public string Query
		{
			get
			{
				return this.m_query;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				if (value.Length > 0)
				{
					value = '?' + value;
				}
				this.m_query = value;
				this.m_changed = true;
			}
		}

		public string Scheme
		{
			get
			{
				return this.m_scheme;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				int num = value.IndexOf(':');
				if (num != -1)
				{
					value = value.Substring(0, num);
				}
				if (value.Length != 0)
				{
					if (!Uri.CheckSchemeName(value))
					{
						throw new ArgumentException("value");
					}
					value = value.ToLower(CultureInfo.InvariantCulture);
				}
				this.m_scheme = value;
				this.m_changed = true;
			}
		}
		
		public Uri Uri
		{
			get
			{
				if (this.m_changed)
				{
					this.m_uri = new Uri(this.ToString());
					this.SetFieldsFromUri(this.m_uri);
					this.m_changed = false;
				}
				return this.m_uri;
			}
		}
		
		public string UserName
		{
			get
			{
				return this.m_username;
			}
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				this.m_username = value;
				this.m_changed = true;
			}
		}
		
		public UriBuilder()
		{
		}
		
		public UriBuilder(string uri)
		{
			Uri uri2 = new Uri(uri, UriKind.RelativeOrAbsolute);
			if (uri2.IsAbsoluteUri)
			{
				this.Init(uri2);
				return;
			}
			uri = Uri.UriSchemeHttp + Uri.SchemeDelimiter + uri;
			this.Init(new Uri(uri));
		}
		
		public UriBuilder(Uri uri)
		{
			if (uri == null)
			{
				throw new ArgumentNullException("uri");
			}
			this.Init(uri);
		}
		
		public UriBuilder(string schemeName, string hostName)
		{
			this.Scheme = schemeName;
			this.Host = hostName;
		}
		
		public UriBuilder(string scheme, string host, int portNumber) : this(scheme, host)
		{
			this.Port = portNumber;
		}
		
		public UriBuilder(string scheme, string host, int port, string pathValue) : this(scheme, host, port)
		{
			this.Path = pathValue;
		}
		
		public UriBuilder(string scheme, string host, int port, string path, string extraValue) : this(scheme, host, port, path)
		{
			try
			{
				this.Extra = extraValue;
			}
			catch (Exception ex)
			{
				if (ex is ThreadAbortException || ex is StackOverflowException || ex is OutOfMemoryException)
				{
					throw;
				}
				throw new ArgumentException("extraValue");
			}
		}
		
		public override bool Equals(object rparam)
		{
			return rparam != null && this.Uri.Equals(rparam.ToString());
		}
		
		public override int GetHashCode()
		{
			return this.Uri.GetHashCode();
		}
		
		public override string ToString()
		{
			if (this.m_username.Length == 0 && this.m_password.Length > 0)
			{
				throw new UriFormatException("net_uri_BadUserPassword");
			}
			if (this.m_scheme.Length != 0)
			{
            //UriParser syntax = UriParser.GetSyntax(this.m_scheme);
            //if (syntax != null)
            //{
            //   this.m_schemeDelimiter = ((syntax.InFact(UriSyntaxFlags.MustHaveAuthority) || (this.m_host.Length != 0 && syntax.NotAny(UriSyntaxFlags.MailToLikeUri) && syntax.InFact(UriSyntaxFlags.OptionalAuthority))) ? Uri.SchemeDelimiter : ":");
            //}
            //else
				{
					this.m_schemeDelimiter = ((this.m_host.Length != 0) ? Uri.SchemeDelimiter : ":");
				}
			}
			string text = (this.m_scheme.Length != 0) ? (this.m_scheme + this.m_schemeDelimiter) : string.Empty;
			return string.Concat(new string[]
			{
				text,
				this.m_username,
				(this.m_password.Length > 0) ? (":" + this.m_password) : string.Empty,
				(this.m_username.Length > 0) ? "@" : string.Empty,
				this.m_host,
				(this.m_port != -1 && this.m_host.Length > 0) ? (":" + this.m_port) : string.Empty,
				(this.m_host.Length > 0 && this.m_path.Length != 0 && this.m_path[0] != '/') ? "/" : string.Empty,
				this.m_path,
				this.m_query,
				this.m_fragment
			});
		}
		private void Init(Uri uri)
		{
			this.m_fragment = uri.Fragment; 
			this.m_query = uri.Query;
			this.m_host = uri.Host;
			this.m_path = uri.AbsolutePath;
			this.m_port = uri.Port;
			this.m_scheme = uri.Scheme;
         this.m_schemeDelimiter = (/*uri.HasAuthority*/!string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Host) ? Uri.SchemeDelimiter : ":");
			string userInfo = uri.UserInfo;
			if (!string.IsNullOrEmpty(userInfo))
			{
				int num = userInfo.IndexOf(':');
				if (num != -1)
				{
					this.m_password = userInfo.Substring(num + 1);
					this.m_username = userInfo.Substring(0, num);
				}
				else
				{
					this.m_username = userInfo;
				}
			}
			this.SetFieldsFromUri(uri);
		}
		private string ConvertSlashes(string path)
		{
			StringBuilder stringBuilder = new StringBuilder(path.Length);
			for (int i = 0; i < path.Length; i++)
			{
				char c = path[i];
				if (c == '\\')
				{
					c = '/';
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}
		private void SetFieldsFromUri(Uri uri)
		{
			this.m_fragment = uri.Fragment;
			this.m_query = uri.Query;
			this.m_host = uri.Host;
			this.m_path = uri.AbsolutePath;
			this.m_port = uri.Port;
			this.m_scheme = uri.Scheme;
         this.m_schemeDelimiter = (/*uri.HasAuthority*/!string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Host) ? Uri.SchemeDelimiter : ":");
			string userInfo = uri.UserInfo;
			if (userInfo.Length > 0)
			{
				int num = userInfo.IndexOf(':');
				if (num != -1)
				{
					this.m_password = userInfo.Substring(num + 1);
					this.m_username = userInfo.Substring(0, num);
					return;
				}
				this.m_username = userInfo;
			}
		}
	}
}
