using System.Xml;
using Tracer.Core;
using Tracer.Serialization.Abstractions;

namespace Tracer.Serialization.Xml;

public sealed class XmlTraceResultSerializer : ITraceResultSerializer
{
    public string Format => "xml";

    public void Serialize(TraceResult traceResult, Stream to)
    {
        var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = false };

        using var xw = XmlWriter.Create(to, settings);

        xw.WriteStartElement("root");

        foreach (var t in traceResult.Threads)
        {
            xw.WriteStartElement("thread");
            xw.WriteAttributeString("id", t.Id.ToString());
            xw.WriteAttributeString("time", $"{t.TimeMs}ms");

            foreach (var m in t.Methods)
                WriteMethod(xw, m);

            xw.WriteEndElement(); // thread
        }

        xw.WriteEndElement(); // root
        xw.Flush();
    }

    private static void WriteMethod(XmlWriter xw, MethodTraceResult m)
    {
        xw.WriteStartElement("method");
        xw.WriteAttributeString("name", m.Name);
        xw.WriteAttributeString("class", m.Class);
        xw.WriteAttributeString("time", $"{m.TimeMs}ms");

        foreach (var c in m.Methods)
            WriteMethod(xw, c);

        xw.WriteEndElement();
    }
}