using System;
using FluentAssertions;
using UsefulMonads;
using Xunit;

namespace Tests
{
  public class MaybeTests
  {
    [Fact]
    public void TestResolve()
    {
      var sut = Maybe<int>.Create(42);
      sut.Should().NotBe(null);
      sut.Resolve(0, x => x).Should().Be(42);
    }

    [Fact]
    public void TestMap()
    {
      var sut = Maybe<int>.Create(42);
      var actual = sut.Map(x => 2 * x);
      var expected = Maybe<int>.Create(84);
      expected.Should().Be(actual);
    }

    [Fact]
    public void TestDo()
    {
      var sut = Maybe<int>.Create(42);
      int somethingDone = 0;
      sut.Do(() => somethingDone = -1, x => somethingDone = x);
      somethingDone.Should().Be(42);
      
      var sut2 = Maybe<int?>.Create(default);
      somethingDone = 0;
      sut2.Do(() => somethingDone = -1, x => somethingDone = x.Value);
      somethingDone.Should().Be(-1);
    }

    [Fact]
    public void TestEquals()
    {
      var sut1 = Maybe<int>.Create(42);
      var actual1 = Maybe<int>.Create(42);
      sut1.Equals(actual1).Should().BeTrue();
      sut1.Equals(null).Should().BeFalse();
      
      var sut2 = Maybe<int?>.Create(default);
      var actual2 = Maybe<int?>.Create(default);
      sut2.Equals(actual2).Should().BeTrue();
    }

    [Fact]
    public void TestHashCode()
    {
      var sut = Maybe<int>.Create(42);
      var actual = sut.GetHashCode();
      var expected = HashCode.Combine(42);
      expected.Should().Be(actual);
    }

    [Fact]
    public void TestBind()
    {
      var sut = Maybe<int>.Create(42);
      Func<int, Maybe<string>> map = x => Maybe<string>.Create(x.ToString());
      var actual = sut.Bind(map);
      actual.Resolve("", x => x).Should().Be("42");

      var sut2 = Maybe<int?>.Create(null);
      Func<int?, Maybe<string>> map2 = x => Maybe<string>.Create(x.ToString());
      var actual2 = sut2.Bind(map2);
      actual2.Resolve("FBV", x => x).Should().Be("FBV");
    }

    [Fact]
    public void TestSelect()
    {
      var sut = Maybe<int>.Create(42);
      var q =
        from s in sut
        select s.ToString().Length;
      q.Resolve(0, x => x).Should().Be(2);

      var sut2 = Maybe<int?>.Create(null);
      var q2 =
        from s in sut2
        select s.ToString().Length;
      q2.Resolve(0, s => s).Should().Be(0);
    }

    [Fact]
    public void TestSelectMany()
    {
      var sut1 = Maybe<int>.Create(42);
      var sut2 = Maybe<bool>.Create(true);
      var sut3 = Maybe<double>.Create(17.42);

      var q =
        from s1 in sut1
        from s2 in sut2
        from s3 in sut3
        select s2 ? s3 * s1 : 0;

      q.Resolve(0, x => x).Should().BeApproximately(731.64, 0.0001);
    }
  }
}
