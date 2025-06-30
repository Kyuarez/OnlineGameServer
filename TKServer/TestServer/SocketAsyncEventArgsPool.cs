using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /// <summary>
    /// MSDNMicroSoft Dev 코드 : AsyncEventArgs use resuable
    /// </summary>
    public class SocketAsyncEventArgsPool
    {
        private Stack<SocketAsyncEventArgs> _pools;

        public SocketAsyncEventArgsPool(int capacity)
        {
            _pools = new Stack<SocketAsyncEventArgs>(capacity);
        }
        public int Count
        {
            get { return _pools.Count; }
        }


        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPoo can not be null");
            }

            lock (_pools)
            {
                _pools.Push(item);
            }
        }

        public SocketAsyncEventArgs Pop()
        {
            lock (_pools)
            {
                return _pools.Pop();    
            }
        }
    }
}
