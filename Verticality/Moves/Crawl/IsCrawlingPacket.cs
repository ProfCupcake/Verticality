using ProtoBuf;

namespace Verticality.Moves.Crawl
{
    [ProtoContract(ImplicitFields=ImplicitFields.AllFields)]
    public class IsCrawlingPacket
    {
        public bool isCrawling;
    }
}
