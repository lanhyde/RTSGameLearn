//  at least 3 lines comment required.
// aaa
// abcde
using System;
using UnityEngine;

namespace CSharpCodeAnalyzer
{

    public struct Vector3h
    {
        public short x, y, z;
    }

    /// <summary>
    /// class comment
    /// </summary>
    public class TestClassA
    {
        private int prvivateField = 0, privateField2 = 1;
        protected int protectedField = 0;
        public int publicField = 0;

        readonly int privateFieldWithoutModifier = 0;

        public int PublicProperty { get; set; }
        public bool IsValid => protectedField == 0;

        public int PropertyTest
        {
            get { return protectedField + 1; }
            set { protectedField++; }
        }

        public const string ConstantString = "Constant string";
        public readonly int ReadonlyField = 1;

        public void PublicMethod()
        {
            // Public method
            int a = 1, b = 2;
            string str = String.Empty;
            float speed = Time.deltaTime * 1.2f;
            Time.timeScale = 1.2f;
            Console.WriteLine("Public Method");
        }

        protected void ProtectedMethod()
        {
            // Protected method
            Console.WriteLine("Protected Method");
        }

        private void PrivateMethod()
        {
            // Private method
            Console.WriteLine("Private Method");
        }
    }
}