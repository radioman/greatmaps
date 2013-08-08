using MSR.CVE.BackMaker.MCDebug;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
namespace MSR.CVE.BackMaker.ImagePipeline
{
	public class DiskCache
	{
		private class DeferredWriteRecord : IDisposable
		{
			public Present result;
			public string freshPath;
			public string debugOriginInfo;
			public DeferredWriteRecord(Present result, string freshPath, string debugOriginInfo)
			{
				this.result = result.Duplicate("DiskCache.DeferredWriteRecord");
				this.freshPath = freshPath;
				this.debugOriginInfo = debugOriginInfo;
			}
			public void Dispose()
			{
				this.result.Dispose();
			}
		}
		private const long freshCountMax = 524288000L;
		private const long stableFreshCountAccuracy = 10485760L;
		private const string stableFreshCountFilename = "FreshCount.txt";
		public const string CacheControlExtension = ".cc";
		private const string FreshSide = "fresh.";
		private const string StaleSide = "stale.";
		private string cacheDir;
		private string stableFreshCountPathname;
		private bool disposed;
		private bool demoting;
		private PresentDiskDispatcher presentDiskDispatcher = new PresentDiskDispatcher();
		private long freshCount = -1L;
		private long lastStableFreshCount;
		private EventWaitHandle plowCacheEvent = new CountedEventWaitHandle(false, EventResetMode.AutoReset, "DiskCache.plowCacheEvent");
		private long delayedIncrementFreshCount;
		private Queue<DiskCache.DeferredWriteRecord> deferredWriteQueue = new Queue<DiskCache.DeferredWriteRecord>();
		private EventWaitHandle writeQueueNonEmptyEvent = new CountedEventWaitHandle(false, EventResetMode.AutoReset, "DiskCache.WriteQueueNonemptyEvent");
		private ResourceCounter resourceCounter;
		public DiskCache()
		{
			this.cacheDir = Path.Combine(Environment.GetEnvironmentVariable("TMP"), "mapcache\\");
			this.stableFreshCountPathname = Path.Combine(this.cacheDir, "FreshCount.txt");
			this.CreateCacheDirIfNeeded();
			DebugThreadInterrupter.theInstance.AddThread("DiskCache.DeferredWriteThread", new ThreadStart(this.DeferredWriteThread), ThreadPriority.Normal);
			DebugThreadInterrupter.theInstance.AddThread("DiskCache.EvictThread", new ThreadStart(this.EvictThread), ThreadPriority.Normal);
			this.resourceCounter = DiagnosticUI.theDiagnostics.fetchResourceCounter("DiskCache", -1);
		}
		public void Dispose()
		{
			Monitor.Enter(this);
			try
			{
				this.disposed = true;
				if (this.plowCacheEvent != null)
				{
					this.plowCacheEvent.Set();
				}
				if (this.writeQueueNonEmptyEvent != null)
				{
					this.writeQueueNonEmptyEvent.Set();
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		public Present Get(IFuture future, string refCredit)
		{
			string text = this.makeCachePathname(future, "fresh.");
			string text2 = this.makeCachePathname(future, "stale.");
			Monitor.Enter(this);
			try
			{
				long num;
				Present present = this.Fetch(text, out num);
				if (present != null)
				{
					D.Sayf(10, "fresh hit! {0}", new object[]
					{
						"fresh."
					});
					Present result = present;
					return result;
				}
				present = this.Fetch(text2, out num);
				if (present != null)
				{
					File.Move(text2, text);
					this.IncrementFreshCount(num);
					D.Sayf(10, "stale hit! {0} {1}", new object[]
					{
						"stale.",
						num
					});
					Present result = present;
					return result;
				}
			}
			finally
			{
				Monitor.Exit(this);
			}
			Present result2 = future.Realize(refCredit);
			this.ScheduleDeferredWrite(result2, text, future.ToString());
			D.Sayf(10, "miss", new object[0]);
			return result2;
		}
		private void IncrementFreshCount(long increment)
		{
			if (this.demoting)
			{
				this.delayedIncrementFreshCount += increment;
				return;
			}
			this.freshCount += increment;
			if (this.freshCount - this.lastStableFreshCount > 10485760L)
			{
				this.UpdateStableFreshCount();
			}
			if (this.freshCount > 524288000L)
			{
				this.plowCacheEvent.Set();
			}
			this.resourceCounter.crement((int)increment);
		}
		private void UpdateStableFreshCount()
		{
			try
			{
				FileStream fileStream = File.Open(this.stableFreshCountPathname, FileMode.Create);
				StreamWriter streamWriter = new StreamWriter(fileStream);
				streamWriter.Write(this.freshCount.ToString());
				streamWriter.Close();
				fileStream.Close();
				this.lastStableFreshCount = this.freshCount;
			}
			catch (IOException)
			{
			}
		}
		private string makeCachePathname(IFuture future, string cacheSide)
		{
			string path = cacheSide + RobustHashTools.Hash(future).ToString() + ".cc";
			return Path.Combine(this.cacheDir, path);
		}
		private Present Fetch(string path, out long length)
		{
			if (File.Exists(path))
			{
				try
				{
					return this.presentDiskDispatcher.ReadObject(path, out length);
				}
				catch (Exception arg)
				{
					File.Delete(path);
					D.Say(0, string.Format("Removing corrupt file at {0}; ex {1}", path, arg));
				}
			}
			length = -1L;
			return null;
		}
		private void CreateCacheDirIfNeeded()
		{
			try
			{
				if (!Directory.Exists(this.cacheDir))
				{
					Directory.CreateDirectory(this.cacheDir);
					this.freshCount = 0L;
				}
				else
				{
					try
					{
						FileStream fileStream = File.Open(this.stableFreshCountPathname, FileMode.Open);
						StreamReader streamReader = new StreamReader(fileStream);
						string s = streamReader.ReadToEnd();
						fileStream.Close();
						this.freshCount = long.Parse(s);
					}
					catch (Exception)
					{
						this.freshCount = 0L;
					}
				}
				this.lastStableFreshCount = this.freshCount;
				this.freshCount += 10485760L;
			}
			catch (Exception innerException)
			{
				throw new ConfigurationException(string.Format("TileCache cannot create or access cache directory {0}", this.cacheDir), innerException);
			}
		}
		private void ScheduleDeferredWrite(Present result, string freshPath, string debugOriginInfo)
		{
			Monitor.Enter(this);
			try
			{
				DiskCache.DeferredWriteRecord item = new DiskCache.DeferredWriteRecord(result, freshPath, debugOriginInfo);
				this.deferredWriteQueue.Enqueue(item);
				this.writeQueueNonEmptyEvent.Set();
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		private void WriteRecord(DiskCache.DeferredWriteRecord record)
		{
			try
			{
				long increment;
				this.presentDiskDispatcher.WriteObject(record.result, record.freshPath, out increment);
				Monitor.Enter(this);
				try
				{
					this.IncrementFreshCount(increment);
				}
				finally
				{
					Monitor.Exit(this);
				}
			}
			catch (WriteObjectFailedException ex)
			{
				D.Sayf(1, "disk cache write failed; ignoring: {0}", new object[]
				{
					ex
				});
			}
			record.Dispose();
		}
		private void DeferredWriteThread()
		{
			while (!this.disposed)
			{
				this.writeQueueNonEmptyEvent.WaitOne();
				int num = 0;
				while (!this.disposed)
				{
					DiskCache.DeferredWriteRecord deferredWriteRecord = null;
					Monitor.Enter(this);
					try
					{
						if (this.deferredWriteQueue.Count > 0)
						{
							deferredWriteRecord = this.deferredWriteQueue.Dequeue();
						}
					}
					finally
					{
						Monitor.Exit(this);
					}
					if (deferredWriteRecord == null)
					{
						break;
					}
					this.WriteRecord(deferredWriteRecord);
					num++;
				}
			}
			Monitor.Enter(this);
			try
			{
				this.writeQueueNonEmptyEvent.Close();
				this.writeQueueNonEmptyEvent = null;
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
		private void EvictThread()
		{
            Label_0000:
            try
            {
                this.plowCacheEvent.WaitOne();
                lock (this)
                {
                    if (this.disposed)
                    {
                        lock (this)
                        {
                            this.plowCacheEvent.Close();
                            this.plowCacheEvent = null;
                        }
                        return;
                    }
                    if (this.freshCount <= 0x1f400000L)
                    {
                        goto Label_0000;
                    }
                }
                D.Sayf(1, "Before evict: freshCount {0} kB", new object[] { this.freshCount >> 10 });
                this.EvictStaleFiles();
                this.EvictDemoteFreshFiles();
                D.Sayf(1, "After evict: freshCount {0} kB", new object[] { this.freshCount >> 10 });
            }
            catch (Exception exception)
            {
                D.Sayf(1, "DiskCache.EvictThread ignores ex {0}", new object[] { exception });
            }
            goto Label_0000;
		}
		private void EvictStaleFiles()
		{
			D.Say(0, "DiskCache.EvictStaleFiles");
			Monitor.Enter(this);
			string[] files;
			try
			{
				files = Directory.GetFiles(this.cacheDir, "stale.*.cc");
			}
			finally
			{
				Monitor.Exit(this);
			}
			int num = 0;
			string[] array = files;
			for (int i = 0; i < array.Length; i++)
			{
				string cacheControlFilePath = array[i];
				Monitor.Enter(this);
				try
				{
					if (this.disposed)
					{
						break;
					}
					try
					{
						try
						{
							this.presentDiskDispatcher.DeleteCacheFile(cacheControlFilePath);
							num++;
						}
						catch (IOException)
						{
						}
					}
					catch (Exception ex)
					{
						D.Sayf(1, "DiskCache.EvictStaleFiles ignores exception {0}", new object[]
						{
							ex
						});
					}
				}
				finally
				{
					Monitor.Exit(this);
				}
			}
			D.Sayf(1, "EvictStaleFiles: Examined {0} files, removed {1} files", new object[]
			{
				files.Length,
				num
			});
		}
		private void EvictDemoteFreshFiles()
		{
			D.Say(0, "DiskCache.EvictDemoteFreshFiles");
			Monitor.Enter(this);
			try
			{
				this.demoting = true;
			}
			finally
			{
				Monitor.Exit(this);
			}
			string[] files = Directory.GetFiles(this.cacheDir, "fresh.*.cc");
			int num = 0;
			string[] array = files;
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				Monitor.Enter(this);
				try
				{
					if (this.disposed)
					{
						break;
					}
					try
					{
						string fileName = Path.GetFileName(text);
						if (!fileName.StartsWith("fresh."))
						{
							D.Sayf(1, "Certainly didn't expect wildcard to return filename {0}", new object[]
							{
								text
							});
						}
						else
						{
							string path = fileName.Replace("fresh.", "stale.");
							string text2 = Path.Combine(Path.GetDirectoryName(text), path);
							File.Move(text, text2);
							long num2 = this.presentDiskDispatcher.CacheFileLength(text2);
							this.freshCount -= num2;
							num++;
						}
					}
					catch (Exception ex)
					{
						if (!(ex is IOException) || !ex.ToString().StartsWith("System.IO.IOException: The process cannot access the file"))
						{
							D.Sayf(1, "DiskCache.EvictDemoteFreshFiles ignores exception {0}", new object[]
							{
								ex
							});
						}
					}
				}
				finally
				{
					Monitor.Exit(this);
				}
			}
			D.Sayf(1, "EvictDemoteFreshFiles: Examined {0} files, demoted {1} files", new object[]
			{
				files.Length,
				num
			});
			Monitor.Enter(this);
			try
			{
				D.Sayf(1, "EvictDemoteFreshFiles: At end of pass, {0} bytes unaccounted for. Writing off.", new object[]
				{
					this.freshCount
				});
				this.freshCount = 0L;
				this.demoting = false;
				this.IncrementFreshCount(this.delayedIncrementFreshCount);
				this.delayedIncrementFreshCount = 0L;
				this.UpdateStableFreshCount();
			}
			finally
			{
				Monitor.Exit(this);
			}
		}
	}
}
