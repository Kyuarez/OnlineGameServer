using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKNet
{
    public class CPacketBufferManager
    {
        static object _bufferLock = new object();
        static Stack<CPacket> _pool;
        static int _poolCapacity;

        public static void Initialize(int capacity)
        {
            _pool = new Stack<CPacket>();
            _poolCapacity = capacity;
            Allocate();
        }

        static void Allocate()
        {
            for (int i = 0; i < _poolCapacity; i++)
            {
                _pool.Push(new CPacket());
            }
        }

        public static CPacket Pop()
        {
            lock (_bufferLock)
            {
                if (_pool.Count <= 0)
                {
                    Console.WriteLine("reallocate.");
                    Allocate();
                }

                return _pool.Pop();
            }
        }

        public static void Push(CPacket packet)
        {
            lock (_bufferLock)
            {
                _pool.Push(packet);
            }
        }
    }
}
