using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TKNet
{
    public class CUserToken
    {
        public Socket socket { get; set; }
        public SocketAsyncEventArgs ReceiveEventArgs { get; private set; }
        public SocketAsyncEventArgs SendEventArgs { get; private set; }
        
        private CMessageResolver messageResolver;
        private object sendingLock = new object();
        private Queue<CPacket> sendingQueue;
        
        private IPeer peer;

        public CUserToken()
        {
            this.sendingLock = new object();
            this.messageResolver = new CMessageResolver();
            this.sendingQueue = new Queue<CPacket>();
            this.peer = null;
        }

        public void SetPeer(IPeer peer)
        {
            this.peer = peer;
        }

        public void SetEventArgs(SocketAsyncEventArgs receiveArgs, SocketAsyncEventArgs sendArgs)
        {
            this.ReceiveEventArgs = receiveArgs;
            this.SendEventArgs = sendArgs;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer">소켓으로부터 수신된 데이터가 들어있는 바이트 배열</param>
        /// <param name="offset">데이터 시작 위치</param>
        /// <param name="bytesTransferred">수신된 데이터의 바이트 수(데이터 크기)</param>
        public void OnReceive(byte[] buffer, int offset, int bytesTransferred)
        {
            this.messageResolver.OnReceive(buffer, offset, bytesTransferred, OnMessage);
        }

        private void OnMessage(Const<byte[]> buffer)
        {
            if(this.peer != null)
            {
                this.peer.OnMessage(buffer);
            }
        }

        public void OnRemoved()
        {
            this.sendingQueue.Clear();
            
            if(this.peer != null)
            {   
                this.peer.OnRemoved();
            }
        }

        public void Send(CPacket msg)
        {
            CPacket clonePacket = new CPacket();
            msg.CopyTo(clonePacket);

            //@tk : 이거 왜 락객체 별도로 선언하지 않고 직접 객체를 넣지?
            lock (this.sendingLock)
            {
                if (this.sendingQueue.Count <= 0)
                {
                    this.sendingQueue.Enqueue(msg);
                    StartSend();
                    return;
                }
                
                this.sendingQueue.Enqueue(msg);
            }

        }

        /// <summary>
        /// 비동기 전송
        /// </summary>
        private void StartSend()
        {
            //@스레드 호출이 아니지만, ProcessSend에서 queue에 접근하기에 lock 처리 필요
            lock (this.sendingLock) 
            {
                //전송 완료가 아니기에 peek으로 데이터 살린다.
                CPacket msg = this.sendingQueue.Peek();
                //패킷 헤더에 데이터 사이즈 기록
                msg.RecordSize();

                this.SendEventArgs.SetBuffer(this.SendEventArgs.Offset, msg.Position);

                Array.Copy(msg.Buffer, 0, this.SendEventArgs.Buffer, this.SendEventArgs.Offset, msg.Position);

                //비동기 전송
                bool pending = this.socket.SendAsync(this.SendEventArgs);
                if (!pending)
                {
                    ProcessSend(this.SendEventArgs);
                }
            }
        }

        /// <summary>
        /// 비동기 전송 완료 시 호출되는 콜백 메소드
        /// </summary>
        /// <param name="args"></param>
        public void ProcessSend(SocketAsyncEventArgs args)
        {
            //TODO : Process
            this.sendingQueue.Dequeue();

            if(this.sendingQueue.Count > 0)
            {
                StartSend();
            }
        }

        public void Disconnect()
        {
            try
            {
                this.socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception e)
            {

            }

            this.socket.Close();
        }

        //TODO : KeepAlive
    }
}
