using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
namespace MSR.CVE.BackMaker
{
	public class RemoteFoxitServer
	{
		private string pipeGuid;
		private FoxitViewer foxitViewer;
		public void ConsumeArgs(List<string> args)
		{
			if (args.Count == 0)
			{
				throw new Exception("Expected PipeGuid argument");
			}
			this.pipeGuid = args[0];
			args.RemoveAt(0);
		}
		public int Run()
		{
			NamedPipeClient namedPipeClient = new NamedPipeClient(this.pipeGuid);
			bool flag = true;
			while (!flag)
			{
				D.Sayf(0, "Waiting for debugger.", new object[0]);
				Thread.Sleep(250);
			}
			namedPipeClient.RunServer(new NamedPipeBase.ServerHandler(this.Server));
			return 0;
		}
		internal bool Server(object genericRequest, ref ISerializable reply)
		{
			if (genericRequest is OpenRequest)
			{
				OpenRequest openRequest = (OpenRequest)genericRequest;
				if (this.foxitViewer != null)
				{
					reply = new ExceptionMessageRecord("Already open");
					return true;
				}
				try
				{
					this.foxitViewer = new FoxitViewer(openRequest.filename, openRequest.pageNumber);
					reply = new RectangleFRecord(this.foxitViewer.GetPageSize());
					bool result = true;
					return result;
				}
				catch (Exception ex)
				{
					reply = new ExceptionMessageRecord(ex.Message);
					bool result = false;
					return result;
				}
			}
			if (genericRequest is RenderRequest)
			{
				RenderRequest renderRequest = (RenderRequest)genericRequest;
				if (this.foxitViewer == null)
				{
					reply = new ExceptionMessageRecord("Not open");
					return true;
				}
				try
				{
					reply = this.foxitViewer.RenderBytes(renderRequest.outputSize, renderRequest.topLeft, renderRequest.pageSize, renderRequest.transparentBackground);
					bool result = true;
					return result;
				}
				catch (Exception ex2)
				{
					reply = new ExceptionMessageRecord(ex2.Message);
					bool result = true;
					return result;
				}
			}
			if (genericRequest is QuitRequest)
			{
				reply = new AckRecord();
				return false;
			}
			reply = new ExceptionMessageRecord("Unrecognized request type " + genericRequest.GetType().ToString());
			return true;
		}
	}
}
