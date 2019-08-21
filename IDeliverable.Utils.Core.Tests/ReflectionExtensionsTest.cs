using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IDeliverable.Utils.Core.Tests
{
    [TestClass]
    public class ReflectionExtensionsTest
    {
        [TestMethod]
        public void IsSubclassOfGenericTest()
        {
            Assert.IsTrue(typeof(ChildGeneric).IsSubclassOfGeneric(typeof(BaseGeneric<>)), " 1");
            Assert.IsFalse(typeof(ChildGeneric).IsSubclassOfGeneric(typeof(WrongBaseGeneric<>)), " 2");
            Assert.IsTrue(typeof(ChildGeneric).IsSubclassOfGeneric(typeof(IBaseGeneric<>)), " 3");
            Assert.IsFalse(typeof(ChildGeneric).IsSubclassOfGeneric(typeof(IWrongBaseGeneric<>)), " 4");
            Assert.IsTrue(typeof(IChildGeneric).IsSubclassOfGeneric(typeof(IBaseGeneric<>)), " 5");
            Assert.IsFalse(typeof(IWrongBaseGeneric<>).IsSubclassOfGeneric(typeof(ChildGeneric2<>)), " 6");
            Assert.IsTrue(typeof(ChildGeneric2<>).IsSubclassOfGeneric(typeof(BaseGeneric<>)), " 7");
            Assert.IsTrue(typeof(ChildGeneric2<Class1>).IsSubclassOfGeneric(typeof(BaseGeneric<>)), " 8");
            Assert.IsTrue(typeof(ChildGeneric).IsSubclassOfGeneric(typeof(BaseGeneric<Class1>)), " 9");
            Assert.IsFalse(typeof(ChildGeneric).IsSubclassOfGeneric(typeof(WrongBaseGeneric<Class1>)), "10");
            Assert.IsTrue(typeof(ChildGeneric).IsSubclassOfGeneric(typeof(IBaseGeneric<Class1>)), "11");
            Assert.IsFalse(typeof(ChildGeneric).IsSubclassOfGeneric(typeof(IWrongBaseGeneric<Class1>)), "12");
            Assert.IsTrue(typeof(IChildGeneric).IsSubclassOfGeneric(typeof(IBaseGeneric<Class1>)), "13");
            Assert.IsFalse(typeof(BaseGeneric<Class1>).IsSubclassOfGeneric(typeof(ChildGeneric2<Class1>)), "14");
            Assert.IsTrue(typeof(ChildGeneric2<Class1>).IsSubclassOfGeneric(typeof(BaseGeneric<Class1>)), "15");
            Assert.IsFalse(typeof(ChildGeneric).IsSubclassOfGeneric(typeof(ChildGeneric)), "16");
            Assert.IsFalse(typeof(IChildGeneric).IsSubclassOfGeneric(typeof(IChildGeneric)), "17");
            Assert.IsFalse(typeof(IBaseGeneric<>).IsSubclassOfGeneric(typeof(IChildGeneric2<>)), "18");
            Assert.IsTrue(typeof(IChildGeneric2<>).IsSubclassOfGeneric(typeof(IBaseGeneric<>)), "19");
            Assert.IsTrue(typeof(IChildGeneric2<Class1>).IsSubclassOfGeneric(typeof(IBaseGeneric<>)), "20");
            Assert.IsFalse(typeof(IBaseGeneric<Class1>).IsSubclassOfGeneric(typeof(IChildGeneric2<Class1>)), "21");
            Assert.IsTrue(typeof(IChildGeneric2<Class1>).IsSubclassOfGeneric(typeof(IBaseGeneric<Class1>)), "22");
            Assert.IsFalse(typeof(IBaseGeneric<Class1>).IsSubclassOfGeneric(typeof(BaseGeneric<Class1>)), "23");
            Assert.IsTrue(typeof(BaseGeneric<Class1>).IsSubclassOfGeneric(typeof(IBaseGeneric<Class1>)), "24");
            Assert.IsFalse(typeof(IBaseGeneric<>).IsSubclassOfGeneric(typeof(BaseGeneric<>)), "25");
            Assert.IsTrue(typeof(BaseGeneric<>).IsSubclassOfGeneric(typeof(IBaseGeneric<>)), "26");
            Assert.IsTrue(typeof(BaseGeneric<Class1>).IsSubclassOfGeneric(typeof(IBaseGeneric<>)), "27");
            Assert.IsFalse(typeof(IBaseGeneric<Class1>).IsSubclassOfGeneric(typeof(IBaseGeneric<Class1>)), "28");
            Assert.IsTrue(typeof(BaseGeneric2<Class1>).IsSubclassOfGeneric(typeof(IBaseGeneric<Class1>)), "29");
            Assert.IsFalse(typeof(IBaseGeneric<>).IsSubclassOfGeneric(typeof(BaseGeneric2<>)), "30");
            Assert.IsTrue(typeof(BaseGeneric2<>).IsSubclassOfGeneric(typeof(IBaseGeneric<>)), "31");
            Assert.IsTrue(typeof(BaseGeneric2<Class1>).IsSubclassOfGeneric(typeof(IBaseGeneric<>)), "32");
            Assert.IsTrue(typeof(ChildGenericA).IsSubclassOfGeneric(typeof(BaseGenericA<,>)), "33");
            Assert.IsFalse(typeof(ChildGenericA).IsSubclassOfGeneric(typeof(WrongBaseGenericA<,>)), "34");
            Assert.IsTrue(typeof(ChildGenericA).IsSubclassOfGeneric(typeof(IBaseGenericA<,>)), "35");
            Assert.IsFalse(typeof(ChildGenericA).IsSubclassOfGeneric(typeof(IWrongBaseGenericA<,>)), "36");
            Assert.IsTrue(typeof(IChildGenericA).IsSubclassOfGeneric(typeof(IBaseGenericA<,>)), "37");
            Assert.IsFalse(typeof(IWrongBaseGenericA<,>).IsSubclassOfGeneric(typeof(ChildGenericA2<,>)), "38");
            Assert.IsTrue(typeof(ChildGenericA2<,>).IsSubclassOfGeneric(typeof(BaseGenericA<,>)), "39");
            Assert.IsTrue(typeof(ChildGenericA2<ClassA, ClassB>).IsSubclassOfGeneric(typeof(BaseGenericA<,>)), "40");
            Assert.IsTrue(typeof(ChildGenericA).IsSubclassOfGeneric(typeof(BaseGenericA<ClassA, ClassB>)), "41");
            Assert.IsFalse(typeof(ChildGenericA).IsSubclassOfGeneric(typeof(WrongBaseGenericA<ClassA, ClassB>)), "42");
            Assert.IsTrue(typeof(ChildGenericA).IsSubclassOfGeneric(typeof(IBaseGenericA<ClassA, ClassB>)), "43");
            Assert.IsFalse(typeof(ChildGenericA).IsSubclassOfGeneric(typeof(IWrongBaseGenericA<ClassA, ClassB>)), "44");
            Assert.IsTrue(typeof(IChildGenericA).IsSubclassOfGeneric(typeof(IBaseGenericA<ClassA, ClassB>)), "45");
            Assert.IsFalse(typeof(BaseGenericA<ClassA, ClassB>).IsSubclassOfGeneric(typeof(ChildGenericA2<ClassA, ClassB>)), "46");
            Assert.IsTrue(typeof(ChildGenericA2<ClassA, ClassB>).IsSubclassOfGeneric(typeof(BaseGenericA<ClassA, ClassB>)), "47");
            Assert.IsFalse(typeof(ChildGenericA).IsSubclassOfGeneric(typeof(ChildGenericA)), "48");
            Assert.IsFalse(typeof(IChildGenericA).IsSubclassOfGeneric(typeof(IChildGenericA)), "49");
            Assert.IsFalse(typeof(IBaseGenericA<,>).IsSubclassOfGeneric(typeof(IChildGenericA2<,>)), "50");
            Assert.IsTrue(typeof(IChildGenericA2<,>).IsSubclassOfGeneric(typeof(IBaseGenericA<,>)), "51");
            Assert.IsTrue(typeof(IChildGenericA2<ClassA, ClassB>).IsSubclassOfGeneric(typeof(IBaseGenericA<,>)), "52");
            Assert.IsFalse(typeof(IBaseGenericA<ClassA, ClassB>).IsSubclassOfGeneric(typeof(IChildGenericA2<ClassA, ClassB>)), "53");
            Assert.IsTrue(typeof(IChildGenericA2<ClassA, ClassB>).IsSubclassOfGeneric(typeof(IBaseGenericA<ClassA, ClassB>)), "54");
            Assert.IsFalse(typeof(IBaseGenericA<ClassA, ClassB>).IsSubclassOfGeneric(typeof(BaseGenericA<ClassA, ClassB>)), "55");
            Assert.IsTrue(typeof(BaseGenericA<ClassA, ClassB>).IsSubclassOfGeneric(typeof(IBaseGenericA<ClassA, ClassB>)), "56");
            Assert.IsFalse(typeof(IBaseGenericA<,>).IsSubclassOfGeneric(typeof(BaseGenericA<,>)), "57");
            Assert.IsTrue(typeof(BaseGenericA<,>).IsSubclassOfGeneric(typeof(IBaseGenericA<,>)), "58");
            Assert.IsTrue(typeof(BaseGenericA<ClassA, ClassB>).IsSubclassOfGeneric(typeof(IBaseGenericA<,>)), "59");
            Assert.IsFalse(typeof(IBaseGenericA<ClassA, ClassB>).IsSubclassOfGeneric(typeof(IBaseGenericA<ClassA, ClassB>)), "60");
            Assert.IsTrue(typeof(BaseGenericA2<ClassA, ClassB>).IsSubclassOfGeneric(typeof(IBaseGenericA<ClassA, ClassB>)), "61");
            Assert.IsFalse(typeof(IBaseGenericA<,>).IsSubclassOfGeneric(typeof(BaseGenericA2<,>)), "62");
            Assert.IsTrue(typeof(BaseGenericA2<,>).IsSubclassOfGeneric(typeof(IBaseGenericA<,>)), "63");
            Assert.IsTrue(typeof(BaseGenericA2<ClassA, ClassB>).IsSubclassOfGeneric(typeof(IBaseGenericA<,>)), "64");
            Assert.IsFalse(typeof(BaseGenericA2<ClassB, ClassA>).IsSubclassOfGeneric(typeof(IBaseGenericA<ClassA, ClassB>)), "65");
            Assert.IsFalse(typeof(BaseGenericA<ClassB, ClassA>).IsSubclassOfGeneric(typeof(ChildGenericA2<ClassA, ClassB>)), "66");
            Assert.IsFalse(typeof(BaseGenericA2<ClassB, ClassA>).IsSubclassOfGeneric(typeof(BaseGenericA<ClassA, ClassB>)), "67");
            Assert.IsTrue(typeof(ChildGenericA3<ClassA, ClassB>).IsSubclassOfGeneric(typeof(BaseGenericB<ClassA, ClassB, ClassC>)), "68");
            Assert.IsTrue(typeof(ChildGenericA4<ClassA, ClassB>).IsSubclassOfGeneric(typeof(IBaseGenericB<ClassA, ClassB, ClassC>)), "69");
            Assert.IsFalse(typeof(ChildGenericA3<ClassB, ClassA>).IsSubclassOfGeneric(typeof(BaseGenericB<ClassA, ClassB, ClassC>)), "68-2");
            Assert.IsTrue(typeof(ChildGenericA3<ClassA, ClassB2>).IsSubclassOfGeneric(typeof(BaseGenericB<ClassA, ClassB, ClassC>)), "68-3");
            Assert.IsFalse(typeof(ChildGenericA3<ClassB2, ClassA>).IsSubclassOfGeneric(typeof(BaseGenericB<ClassA, ClassB, ClassC>)), "68-4");
            Assert.IsFalse(typeof(ChildGenericA4<ClassB, ClassA>).IsSubclassOfGeneric(typeof(IBaseGenericB<ClassA, ClassB, ClassC>)), "69-2");
            Assert.IsTrue(typeof(ChildGenericA4<ClassA, ClassB2>).IsSubclassOfGeneric(typeof(IBaseGenericB<ClassA, ClassB, ClassC>)), "69-3");
            Assert.IsFalse(typeof(ChildGenericA4<ClassB2, ClassA>).IsSubclassOfGeneric(typeof(IBaseGenericB<ClassA, ClassB, ClassC>)), "69-4");
            Assert.IsFalse(typeof(bool).IsSubclassOfGeneric(typeof(IBaseGenericB<ClassA, ClassB, ClassC>)), "70");
        }
    }

    public class Class1 { }
    public class BaseGeneric<T> : IBaseGeneric<T> { }
    public class BaseGeneric2<T> : IBaseGeneric<T>, IInterfaceBidon { }
    public interface IBaseGeneric<T> { }
    public class ChildGeneric : BaseGeneric<Class1> { }
    public interface IChildGeneric : IBaseGeneric<Class1> { }
    public class ChildGeneric2<Class1> : BaseGeneric<Class1> { }
    public interface IChildGeneric2<Class1> : IBaseGeneric<Class1> { }
    public class WrongBaseGeneric<T> { }
    public interface IWrongBaseGeneric<T> { }
    public interface IInterfaceBidon { }
    public class ClassA { }
    public class ClassB { }
    public class ClassC { }
    public class ClassB2 : ClassB { }
    public class BaseGenericA<T, U> : IBaseGenericA<T, U> { }
    public class BaseGenericB<T, U, V> { }
    public interface IBaseGenericB<ClassA, ClassB, ClassC> { }
    public class BaseGenericA2<T, U> : IBaseGenericA<T, U>, IInterfaceBidonA { }
    public interface IBaseGenericA<T, U> { }
    public class ChildGenericA : BaseGenericA<ClassA, ClassB> { }
    public interface IChildGenericA : IBaseGenericA<ClassA, ClassB> { }
    public class ChildGenericA2<ClassA, ClassB> : BaseGenericA<ClassA, ClassB> { }
    public class ChildGenericA3<ClassA, ClassB> : BaseGenericB<ClassA, ClassB, ClassC> { }
    public class ChildGenericA4<ClassA, ClassB> : IBaseGenericB<ClassA, ClassB, ClassC> { }
    public interface IChildGenericA2<ClassA, ClassB> : IBaseGenericA<ClassA, ClassB> { }
    public class WrongBaseGenericA<T, U> { }
    public interface IWrongBaseGenericA<T, U> { }
    public interface IInterfaceBidonA { }
}
