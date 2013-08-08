using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
namespace MSR.CVE.BackMaker
{
    public class Manifest
    {
        public delegate void TellManifestDirty();
        public class ManifestRecord
        {
            private const string ManifestRecordTag = "ManifestRecord";
            private const string PathAttr = "Path";
            private const string FileExistsAttr = "FileExists";
            private const string FileLengthAttr = "FileLength";
            private const string IndirectManifestBlockIdAttr = "IndirectManifestBlockId";
            private Manifest.ManifestBlock _block;
            private string _path;
            private bool _fileExists;
            private long _fileLength;
            private int _indirectManifestBlockId;
            private static Manifest.ManifestRecord _tailRecord;
            public Manifest.ManifestBlock block
            {
                get
                {
                    return this._block;
                }
            }
            public string path
            {
                get
                {
                    return this._path;
                }
            }
            public bool fileExists
            {
                get
                {
                    return this._fileExists;
                }
                set
                {
                    this._fileExists = value;
                    this.block.SetDirty();
                }
            }
            public long fileLength
            {
                get
                {
                    return this._fileLength;
                }
                set
                {
                    this._fileLength = value;
                    this.block.SetDirty();
                }
            }
            public int indirectManifestBlockId
            {
                get
                {
                    return this._indirectManifestBlockId;
                }
            }
            public static Manifest.ManifestRecord TailRecord
            {
                get
                {
                    if (Manifest.ManifestRecord._tailRecord == null)
                    {
                        Manifest.ManifestRecord._tailRecord = new Manifest.ManifestRecord(null, null, false, -1L, -1);
                    }
                    return Manifest.ManifestRecord._tailRecord;
                }
            }
            public bool IsTailRecord
            {
                get
                {
                    return this.path == null;
                }
            }
            public ManifestRecord(Manifest.ManifestBlock block, string path, bool fileExists, long fileLength, int indirectManifestBlockId)
            {
                this._block = block;
                this._path = path;
                this._fileExists = fileExists;
                this._fileLength = fileLength;
                this._indirectManifestBlockId = indirectManifestBlockId;
                D.Assert(block == null || path != null);
            }
            internal void WriteXML(XmlTextWriter xtw)
            {
                xtw.WriteStartElement("ManifestRecord");
                xtw.WriteAttributeString("Path", this.path);
                xtw.WriteAttributeString("FileExists", this.fileExists.ToString(CultureInfo.InvariantCulture));
                xtw.WriteAttributeString("FileLength", this.fileLength.ToString(CultureInfo.InvariantCulture));
                xtw.WriteAttributeString("IndirectManifestBlockId", this.indirectManifestBlockId.ToString(CultureInfo.InvariantCulture));
                xtw.WriteEndElement();
            }
            public ManifestRecord(MashupParseContext context, Manifest.ManifestBlock block)
            {
                this._block = block;
                XMLTagReader xMLTagReader = context.NewTagReader("ManifestRecord");
                this._path = context.GetRequiredAttribute("Path");
                this._fileExists = context.GetRequiredAttributeBoolean("FileExists");
                this._fileLength = context.GetRequiredAttributeLong("FileLength");
                this._indirectManifestBlockId = context.GetRequiredAttributeInt("IndirectManifestBlockId");
                xMLTagReader.SkipAllSubTags();
            }
            internal static string GetXmlTag()
            {
                return "ManifestRecord";
            }
            public override string ToString()
            {
                return string.Format("MR({0}, {1})", this.path, this.fileExists);
            }
            internal Manifest.ManifestRecord ReplaceBlock(Manifest.ManifestBlock manifestBlock)
            {
                return new Manifest.ManifestRecord(manifestBlock, this.path, this.fileExists, this.fileLength, this.indirectManifestBlockId);
            }
            internal Manifest.ManifestRecord ReplaceIndirect(int newIndirectBlockId)
            {
                return new Manifest.ManifestRecord(this.block, this.path, this.fileExists, this.fileLength, newIndirectBlockId);
            }
        }
        public class ManifestSuperBlock
        {
            private const string NextUnassignedBlockIdAttr = "NextUnassignedBlockId";
            private const string SplitThresholdAttr = "SplitThreshold";
            private int _splitThreshold = 2000;
            private int _nextUnassignedBlockId;
            private Manifest.TellManifestDirty tellDirty;
            public int splitThreshold
            {
                get
                {
                    return this._splitThreshold;
                }
                set
                {
                    this._splitThreshold = value;
                    this.tellDirty();
                }
            }
            public int nextUnassignedBlockId
            {
                get
                {
                    return this._nextUnassignedBlockId;
                }
                set
                {
                    this._nextUnassignedBlockId = value;
                    this.tellDirty();
                }
            }
            public ManifestSuperBlock(int nextUnassignedBlockId, Manifest.TellManifestDirty tellDirty)
            {
                this._nextUnassignedBlockId = nextUnassignedBlockId;
                this.tellDirty = tellDirty;
            }
            public ManifestSuperBlock(MashupParseContext context, Manifest.TellManifestDirty tellDirty)
            {
                this.tellDirty = tellDirty;
                XMLTagReader xMLTagReader = context.NewTagReader(Manifest.ManifestSuperBlock.GetXmlTag());
                this._nextUnassignedBlockId = context.GetRequiredAttributeInt("NextUnassignedBlockId");
                this._splitThreshold = context.GetRequiredAttributeInt("SplitThreshold");
                xMLTagReader.SkipAllSubTags();
            }
            internal void WriteXML(XmlTextWriter xtw)
            {
                xtw.WriteStartElement(Manifest.ManifestSuperBlock.GetXmlTag());
                xtw.WriteAttributeString("NextUnassignedBlockId", this._nextUnassignedBlockId.ToString(CultureInfo.InvariantCulture));
                xtw.WriteAttributeString("SplitThreshold", this._splitThreshold.ToString(CultureInfo.InvariantCulture));
                xtw.WriteEndElement();
            }
            internal static string GetXmlTag()
            {
                return "SuperBlock";
            }
        }
        public class ManifestBlock : IEnumerable<Manifest.ManifestRecord>, IEnumerable
        {
            public delegate Manifest.ManifestBlock CreateBlock();
            private const string ManifestsDir = "manifests";
            private const string ManifestBlockTag = "ManifestBlock";
            private List<Manifest.ManifestRecord> recordList = new List<Manifest.ManifestRecord>();
            private Manifest.ManifestSuperBlock _superBlock;
            public int blockId;
            private bool dirty;
            private Manifest.TellManifestDirty tellManifestDirty;
            public Manifest.ManifestSuperBlock superBlock
            {
                get
                {
                    return this._superBlock;
                }
            }
            public int Count
            {
                get
                {
                    return this.recordList.Count;
                }
            }
            public void SetDirty()
            {
                this.dirty = true;
                this.tellManifestDirty();
            }
            internal void CommitChanges(Manifest manifest)
            {
                if (!this.dirty)
                {
                    return;
                }
                this.WriteChanges(manifest.storageMethod);
                this.dirty = false;
            }
            private string manifestFilename(int blockId)
            {
                return string.Format("{0}.xml", blockId);
            }
            private void WriteChanges(RenderOutputMethod outputMethod)
            {
                Stream w = outputMethod.MakeChildMethod("manifests").CreateFile(this.manifestFilename(this.blockId), "text/xml");
                XmlTextWriter xmlTextWriter = new XmlTextWriter(w, Encoding.UTF8);
                using (xmlTextWriter)
                {
                    xmlTextWriter.Formatting = Formatting.Indented;
                    xmlTextWriter.WriteStartDocument(true);
                    xmlTextWriter.WriteStartElement("ManifestBlock");
                    if (this._superBlock != null)
                    {
                        this._superBlock.WriteXML(xmlTextWriter);
                    }
                    foreach (Manifest.ManifestRecord current in this)
                    {
                        current.WriteXML(xmlTextWriter);
                    }
                    xmlTextWriter.WriteEndElement();
                    xmlTextWriter.Close();
                }
            }
            public ManifestBlock(Manifest.TellManifestDirty tellManifestDirty, RenderOutputMethod outputMethod, int blockId)
            {
                this.tellManifestDirty = tellManifestDirty;
                this.blockId = blockId;
                try
                {
                    Stream input = outputMethod.MakeChildMethod("manifests").ReadFile(this.manifestFilename(blockId));
                    XmlTextReader xmlTextReader = new XmlTextReader(input);
                    using (xmlTextReader)
                    {
                        MashupParseContext mashupParseContext = new MashupParseContext(xmlTextReader);
                        while (mashupParseContext.reader.Read())
                        {
                            if (mashupParseContext.reader.NodeType == XmlNodeType.Element && mashupParseContext.reader.Name == "ManifestBlock")
                            {
                                XMLTagReader xMLTagReader = mashupParseContext.NewTagReader("ManifestBlock");
                                while (xMLTagReader.FindNextStartTag())
                                {
                                    if (xMLTagReader.TagIs(Manifest.ManifestRecord.GetXmlTag()))
                                    {
                                        this.recordList.Add(new Manifest.ManifestRecord(mashupParseContext, this));
                                    }
                                    else
                                    {
                                        if (xMLTagReader.TagIs(Manifest.ManifestSuperBlock.GetXmlTag()))
                                        {
                                            this._superBlock = new Manifest.ManifestSuperBlock(mashupParseContext, new Manifest.TellManifestDirty(this.SetDirty));
                                        }
                                    }
                                }
                                return;
                            }
                        }
                        throw new InvalidMashupFile(mashupParseContext, "No ManifestBlock tag");
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (blockId == 0 && this._superBlock == null)
                    {
                        this._superBlock = new Manifest.ManifestSuperBlock(1, new Manifest.TellManifestDirty(this.SetDirty));
                    }
                }
            }
            public void Insert(Manifest.ManifestRecord newRecord, Manifest.ManifestRecord afterRecord)
            {
                D.Assert(afterRecord.IsTailRecord || afterRecord.block == this);
                D.Assert(newRecord.block == this);
                if (afterRecord.IsTailRecord)
                {
                    this.recordList.Add(newRecord);
                }
                else
                {
                    this.recordList.Insert(this.recordList.FindIndex((Manifest.ManifestRecord mrb) => mrb.path == afterRecord.path), newRecord);
                }
                this.SetDirty();
            }
            public IEnumerator<Manifest.ManifestRecord> GetEnumerator()
            {
                return this.recordList.GetEnumerator();
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.recordList.GetEnumerator();
            }
            internal void Split(Manifest.ManifestBlock.CreateBlock createBlock)
            {
                int num = 2;
                Manifest.ManifestBlock[] subBlocks = new Manifest.ManifestBlock[num];
                for (int j = 0; j < num; j++)
                {
                    subBlocks[j] = createBlock();
                }
                List<Manifest.ManifestRecord> list = new List<Manifest.ManifestRecord>();
                Converter<Manifest.ManifestRecord, Manifest.ManifestRecord> converter = null;
                for (int i = 0; i < num; i++)
                {
                    int index = (int)((((double)i) / ((double)num)) * this.recordList.Count);
                    int num4 = (int)(((i + 1.0) / ((double)num)) * this.recordList.Count);
                    if (converter == null)
                    {
                        converter = mr => mr.ReplaceBlock(subBlocks[i]);
                    }
                    subBlocks[i].recordList = this.recordList.GetRange(index, num4 - index).ConvertAll<Manifest.ManifestRecord>(converter);
                    Manifest.ManifestRecord item = this.recordList[index].ReplaceIndirect(subBlocks[i].blockId);
                    list.Add(item);
                    subBlocks[i].SetDirty();
                }
                this.recordList = list;
                this.SetDirty();
            }
        }

        private delegate bool StopHere(string recP);
        private RenderOutputMethod storageMethod;
        private Manifest.ManifestBlock rootBlock;
        internal Dictionary<int, Manifest.ManifestBlock> blockCache = new Dictionary<int, Manifest.ManifestBlock>();
        private int dirtyCount;
        public Manifest(RenderOutputMethod storageMethod)
        {
            this.storageMethod = storageMethod;
            this.rootBlock = this.FetchBlock(0);
        }
        public void Test_SetSplitThreshold(int splitThreshold)
        {
            this.rootBlock.superBlock.splitThreshold = splitThreshold;
        }
        private Manifest.ManifestBlock FetchBlock(int blockId)
        {
            if (this.blockCache.ContainsKey(blockId))
            {
                return this.blockCache[blockId];
            }
            Manifest.ManifestBlock manifestBlock = new Manifest.ManifestBlock(new Manifest.TellManifestDirty(this.SetDirty), this.storageMethod, blockId);
            this.blockCache[blockId] = manifestBlock;
            return manifestBlock;
        }
        private Manifest.ManifestBlock CreateBlock()
        {
            Manifest.ManifestBlock manifestBlock = new Manifest.ManifestBlock(new Manifest.TellManifestDirty(this.SetDirty), this.storageMethod, this.rootBlock.superBlock.nextUnassignedBlockId);
            this.rootBlock.superBlock.nextUnassignedBlockId++;
            D.Assert(!this.blockCache.ContainsKey(manifestBlock.blockId));
            this.blockCache[manifestBlock.blockId] = manifestBlock;
            return manifestBlock;
        }
        private Manifest.ManifestRecord Search(Manifest.StopHere stopHere)
        {
            ManifestBlock rootBlock = this.rootBlock;
            ManifestRecord tailRecord = ManifestRecord.TailRecord;
            while (true)
            {
                ManifestRecord record2 = null;
                bool flag = false;
                foreach (ManifestRecord record3 in new List<ManifestRecord>(rootBlock) { tailRecord })
                {
                    if (stopHere(record3.path))
                    {
                        if ((record2 == null) || (record2.indirectManifestBlockId < 0))
                        {
                            return record3;
                        }
                        rootBlock = this.FetchBlock(record2.indirectManifestBlockId);
                        tailRecord = record3;
                        flag = true;
                        break;
                    }
                    record2 = record3;
                }
                if (!flag)
                {
                    D.Assert(false, "Should have stopped inside loop.");
                }
            }
        }
        internal Manifest.ManifestRecord FindFirstGreaterThan(string p)
        {
            return this.Search((string recP) => recP == null || recP.CompareTo(p) > 0);
        }
        internal Manifest.ManifestRecord FindFirstGreaterEqual(string p)
        {
            return this.Search((string recP) => recP == null || recP.CompareTo(p) >= 0);
        }
        internal Manifest.ManifestRecord FindFirstEqual(string path)
        {
            Manifest.ManifestRecord manifestRecord = this.FindFirstGreaterEqual(path);
            if (manifestRecord.path == path)
            {
                return manifestRecord;
            }
            return Manifest.ManifestRecord.TailRecord;
        }
        internal void Add(string path, long fileLength)
        {
            Manifest.ManifestRecord manifestRecord = this.FindFirstGreaterEqual(path);
            if (manifestRecord.path == path)
            {
                manifestRecord.fileExists = true;
                manifestRecord.fileLength = fileLength;
                return;
            }
            Manifest.ManifestBlock manifestBlock = (manifestRecord.block == null) ? this.rootBlock : manifestRecord.block;
            Manifest.ManifestRecord newRecord = new Manifest.ManifestRecord(manifestBlock, path, true, fileLength, -1);
            manifestBlock.Insert(newRecord, manifestRecord);
            if (manifestBlock.Count > this.rootBlock.superBlock.splitThreshold)
            {
                manifestBlock.Split(new Manifest.ManifestBlock.CreateBlock(this.CreateBlock));
            }
        }
        public void Remove(string p)
        {
            Manifest.ManifestRecord manifestRecord = this.FindFirstEqual(p);
            if (manifestRecord.IsTailRecord)
            {
                return;
            }
            manifestRecord.fileExists = false;
            manifestRecord.fileLength = -1L;
        }
        public void CommitChanges()
        {
            foreach (Manifest.ManifestBlock current in this.blockCache.Values)
            {
                current.CommitChanges(this);
            }
        }
        internal void SetDirty()
        {
            this.dirtyCount++;
            if (this.dirtyCount > 100)
            {
                this.CommitChanges();
                this.dirtyCount = 0;
            }
        }
    }
}
