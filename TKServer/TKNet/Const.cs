using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TKNet
{
    /// <summary>
    /// 불변 객체 감싸는 래퍼 클래스 : 데이터 안전성
    /// </summary>
    public struct Const<T>
    {
        private readonly T _value;
        
        public Const(T value)
        {
            _value = value;
        }

        public T Value => _value;
    }
}
