using System;
using System.IO;
using System.Net;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public abstract class WebTileFetch : Verb
	{
		public abstract void AccumulateRobustHash(IRobustHash hash);
		protected abstract string GetTileURL(TileAddress ta);
		public Present Evaluate(Present[] paramList)
		{
			Present result;
			try
			{
				TileAddress ta = (TileAddress)paramList[0];
				if (BuildConfig.theConfig.injectTemporaryTileFailures && new Random().Next() % 2 == 0)
				{
					result = new InjectedTileFailure();
				}
				else
				{
					HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(this.GetTileURL(ta));
					httpWebRequest.Timeout = 9000;
					HttpWebResponse httpWebResponse;
					try
					{
						httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
					}
					catch (WebException ex)
					{
						if (ex.Response is HttpWebResponse)
						{
							HttpWebResponse httpWebResponse2 = (HttpWebResponse)ex.Response;
							if (httpWebResponse2.StatusCode == HttpStatusCode.BadRequest || httpWebResponse2.StatusCode == HttpStatusCode.NotFound)
							{
								result = new UnretryableFailure(ex);
								return result;
							}
						}
						result = new RetryableFailure(ex, "Timeout waiting for tile in WebTileFetch");
						return result;
					}
					if (httpWebResponse.StatusCode != HttpStatusCode.OK)
					{
						throw new Exception(string.Format("HTTP {0} from web source", httpWebResponse.StatusCode.ToString()));
					}
					Stream responseStream = httpWebResponse.GetResponseStream();
					GDIBigLockedImage image = GDIBigLockedImage.FromStream(responseStream);
					httpWebResponse.Close();
					result = new ImageRef(new ImageRefCounted(image));
				}
			}
			catch (Exception ex2)
			{
				result = new PresentFailureCode(ex2);
			}
			return result;
		}
	}
}
