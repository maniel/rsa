using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSA {
    public static class ArrayPadding {        
        public static T[] pad<T>(this T[] ary, int length) {
            T[] newAry = new T[length];
            Array.Copy(ary, 0, newAry, length - ary.Length, ary.Length);
            return newAry;
        }
    }
}
