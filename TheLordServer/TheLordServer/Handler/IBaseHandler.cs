using Photon.SocketServer;

namespace TheLordServer.Handler
{
    using Table.Structure;
    public interface IBaseHandler
    {
        void AddListener ( );
        void RemoveListener ( );
    }
}
