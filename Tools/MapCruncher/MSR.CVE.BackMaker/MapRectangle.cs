namespace MSR.CVE.BackMaker
{
    using MSR.CVE.BackMaker.ImagePipeline;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Xml;

    public class MapRectangle : IRobustlyHashable
    {
        public LatLon ll0;
        public LatLon ll1;
        private const string MapRectangleTag = "MapRectangle";

        public MapRectangle()
        {
        }

        public MapRectangle(MapRectangle mr)
        {
            this.ll0 = mr.ll0;
            this.ll1 = mr.ll1;
            this.AssertOrder();
        }

        public MapRectangle(LatLon NW, LatLon SE)
        {
            this.ll0 = new LatLon(SE.lat, NW.lon);
            this.ll1 = new LatLon(NW.lat, SE.lon);
            this.AssertOrder();
        }

        public MapRectangle(MashupParseContext context, CoordinateSystemIfc coordSys)
        {
            XMLTagReader reader = context.NewTagReader("MapRectangle");
            List<LatLon> list = new List<LatLon>();
            while (reader.FindNextStartTag())
            {
                if (reader.TagIs(LatLon.GetXMLTag()))
                {
                    list.Add(new LatLon(context, coordSys));
                }
            }
            reader.SkipAllSubTags();
            if (list.Count != 2)
            {
                throw new InvalidMashupFile(context, string.Format("{0} should contain exactly 2 {1} subtags", "MapRectangle", LatLon.GetXMLTag()));
            }
            this.ll0 = list[0];
            this.ll1 = list[1];
            this.AssertOrder();
        }

        public MapRectangle(double lat0, double lon0, double lat1, double lon1)
        {
            this.ll0 = new LatLon(lat0, lon0);
            this.ll1 = new LatLon(lat1, lon1);
            this.AssertOrder();
        }

        public void AccumulateRobustHash(IRobustHash hash)
        {
            this.ll0.AccumulateRobustHash(hash);
            this.ll1.AccumulateRobustHash(hash);
        }

        internal static MapRectangle AddToBoundingBox(MapRectangle box, LatLon ll)
        {
            if (box == null)
            {
                return new MapRectangle(ll.lat, ll.lon, ll.lat, ll.lon);
            }
            return new MapRectangle(Math.Min(ll.lat, box.lat0), Math.Min(ll.lon, box.lon0), Math.Max(ll.lat, box.lat1), Math.Max(ll.lon, box.lon1));
        }

        private void AssertOrder()
        {
            D.Assert(this.ll0.lat <= this.ll1.lat);
        }

        private static bool betweenInclusive(double subject, double low, double high)
        {
            D.Assert(low <= high);
            return ((low <= subject) && (subject <= high));
        }

        internal MapRectangle ClipTo(MapRectangle clipRect)
        {
            double num = Math.Max(this.lat0, clipRect.lat0);
            double num2 = Math.Max(this.lon0, clipRect.lon0);
            double num3 = Math.Max(num, Math.Min(this.lat1, clipRect.lat1));
            return new MapRectangle(num, num2, num3, Math.Max(num2, Math.Min(this.lon1, clipRect.lon1)));
        }

        public override bool Equals(object o2)
        {
            MapRectangle rectangle = (MapRectangle)o2;
            if (rectangle == null)
            {
                return false;
            }
            return (this == rectangle);
        }

        public string FilenameString()
        {
            return string.Format("rect.{0}.{1}.{2}.{3}", new object[] { this.lat0, this.lon0, this.lat1, this.lon1 });
        }

        internal LatLon GetCenter()
        {
            return new LatLon((this.lat0 + this.lat1) * 0.5, (this.lon0 + this.lon1) * 0.5);
        }

        public override int GetHashCode()
        {
            return (this.ll0.GetHashCode() ^ this.ll1.GetHashCode());
        }

        internal LatLon GetNE()
        {
            return new LatLon(this.lat1, this.lon1);
        }

        internal LatLon GetNW()
        {
            return new LatLon(this.lat1, this.lon0);
        }

        internal LatLon GetSE()
        {
            return new LatLon(this.lat0, this.lon1);
        }

        internal LatLon GetSW()
        {
            return new LatLon(this.lat0, this.lon0);
        }

        public static string GetXMLTag()
        {
            return "MapRectangle";
        }

        internal MapRectangle GrowFraction(double p)
        {
            return new MapRectangle(this.lat0 - (p * (this.lat1 - this.lat0)), this.lon0 - (p * (this.lon1 - this.lon0)), this.lat1 + (p * (this.lat1 - this.lat0)), this.lon1 + (p * (this.lon1 - this.lon0)));
        }

        internal MapRectangle Intersect(MapRectangle other)
        {
            return new MapRectangle
            {
                ll0 = new LatLon(Math.Max(this.lat0, other.lat0), Math.Max(this.lon0, other.lon0)),
                ll1 = new LatLon(Math.Min(this.lat1, other.lat1), Math.Min(this.lon1, other.lon1))
            };
        }

        public bool intersects(MapRectangle othr)
        {
            bool flag = ((betweenInclusive(this.ll0.lat, othr.ll0.lat, othr.ll1.lat) || betweenInclusive(this.ll1.lat, othr.ll0.lat, othr.ll1.lat)) || betweenInclusive(othr.ll0.lat, this.ll0.lat, this.ll1.lat)) || betweenInclusive(othr.ll1.lat, this.ll0.lat, this.ll1.lat);
            bool flag2 = ((betweenInclusive(this.ll0.lon, othr.ll0.lon, othr.ll1.lon) || betweenInclusive(this.ll1.lon, othr.ll0.lon, othr.ll1.lon)) || betweenInclusive(othr.ll0.lon, this.ll0.lon, this.ll1.lon)) || betweenInclusive(othr.ll1.lon, this.ll0.lon, this.ll1.lon);
            return (flag && flag2);
        }

        public bool IsEmpty()
        {
            if (this.lat1 > this.lat0)
            {
                return (this.lon1 <= this.lon0);
            }
            return true;
        }

        public static MapRectangle MapRectangleIgnoreOrder(LatLon NW, LatLon SE)
        {
            return new MapRectangle
            {
                ll0 = new LatLon(SE.lat, NW.lon),
                ll1 = new LatLon(NW.lat, SE.lon)
            };
        }

        public static bool operator ==(MapRectangle mr1, MapRectangle mr2)
        {
            if (object.ReferenceEquals(mr1, null) && object.ReferenceEquals(mr2, null))
            {
                return true;
            }
            if (object.ReferenceEquals(mr1, null) || object.ReferenceEquals(mr2, null))
            {
                return false;
            }
            return ((mr1.ll0 == mr2.ll0) && (mr1.ll1 == mr2.ll1));
        }

        public static bool operator !=(MapRectangle mr1, MapRectangle mr2)
        {
            return !(mr1 == mr2);
        }

        public Size SizeWithAspectRatio(int longDimension)
        {
            double num = (this.lat1 - this.lat0) / (this.lon1 - this.lon0);
            if (num > 1.0)
            {
                return new Size((int)(((double)longDimension) / num), longDimension);
            }
            return new Size(longDimension, (int)(longDimension * num));
        }

        public MapRectangle SquareOff()
        {
            double num = Math.Max((double)(this.lon1 - this.lon0), (double)(this.lat1 - this.lat0));
            return new MapRectangle(this.lat0, this.lon0, this.lat0 + num, this.lon0 + num);
        }

        public override string ToString()
        {
            return string.Format("MapRectangle({0},{1},{2},{3})", new object[] { this.lat0, this.lon0, this.lat1, this.lon1 });
        }

        internal MapRectangle Transform(IPointTransformer transformer)
        {
            MapRectangle box = null;
            return AddToBoundingBox(AddToBoundingBox(AddToBoundingBox(AddToBoundingBox(box, transformer.getTransformedPoint((PointD)this.GetSW())), transformer.getTransformedPoint((PointD)this.GetSE())), transformer.getTransformedPoint((PointD)this.GetNW())), transformer.getTransformedPoint((PointD)this.GetNE()));
        }

        internal static MapRectangle Union(MapRectangle box1, MapRectangle box2)
        {
            if (box1 == null)
            {
                return box2;
            }
            if (box2 == null)
            {
                return box1;
            }
            return AddToBoundingBox(AddToBoundingBox(box1, box2.GetSW()), box2.GetNE());
        }

        public void WriteXML(XmlTextWriter writer)
        {
            writer.WriteStartElement("MapRectangle");
            this.ll0.WriteXML(writer);
            this.ll1.WriteXML(writer);
            writer.WriteEndElement();
        }

        public double lat0
        {
            get
            {
                return this.ll0.lat;
            }
        }

        public double lat1
        {
            get
            {
                return this.ll1.lat;
            }
        }

        public double LatExtent
        {
            get
            {
                return (this.ll1.lat - this.ll0.lat);
            }
        }

        public double lon0
        {
            get
            {
                return this.ll0.lon;
            }
        }

        public double lon1
        {
            get
            {
                return this.ll1.lon;
            }
        }

        public double LonExtent
        {
            get
            {
                return (this.ll1.lon - this.ll0.lon);
            }
        }
    }
}
