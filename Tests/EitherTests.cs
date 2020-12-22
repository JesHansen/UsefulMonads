using System;
using FluentAssertions;
using UsefulMonads;
using Xunit;

namespace Tests
{
  public class EitherTests
  {
    [Fact]
    public void TestBind()
    {
      var sut = new Either<string, int>(42);
      var actual = sut.Bind(i => new Either<string, double>(i * i - i));
      actual.Resolve(x => 0, d => d).Should().BeApproximately(1722, 0.00001);

      var sut2 = new Either<string, int>("Brrr");
      var actual2 = sut2.Bind(i => new Either<string, double>(i * i - i));
      actual2.Resolve(x => 0, d => d).Should().BeApproximately(0, 0.00001);
    }

    [Fact]
    public void TestEquals()
    {
      var sut = new Either<string, int>(42);
      var actual = new Either<string, int>(42);
      sut.Equals(actual).Should().BeTrue();
      actual.Equals(sut).Should().BeTrue();

      var sut2 = new Either<string, int>("Brrr");
      sut.Equals(sut2).Should().BeFalse();
      sut2.Equals(sut).Should().BeFalse();
    }

    [Fact]
    public void TestGetHashCode()
    {
      var sut = new Either<string, int>(42);
      var actual = HashCode.Combine(false, 42);
      sut.GetHashCode().Should().Be(actual);
      
      var sut2 = new Either<string, int>("Brrr");
      var actual2 = HashCode.Combine(true, "Brrr");
      sut2.GetHashCode().Should().Be(actual2);
    }

    [Fact]
    public void TestMapError()
    {
      var sut = new Either<string, int>(42);
      var actual = sut.MapError(s => s.ToUpper());
      sut.Equals(actual).Should().BeTrue();

      var sut2 = new Either<string, int>("Brrr");
      var actual2 = sut2.MapError(s => s.ToUpper());
      actual2.Resolve(x => x, i => i.ToString()).Should().Be("BRRR");
      sut2.Equals(actual2).Should().BeFalse();
    }

    [Fact]
    public void TestMapOk()
    {
      var sut = new Either<string, int>(42);
      var actual = sut.MapOk(s => s < 1800);
      actual.Resolve(x => false, x => x).Should().BeTrue();

      var sut2 = new Either<string, int>("Brrr");
      var actual2 = sut2.MapOk(s => s < 1800);
      actual2.Resolve(x => x, i => i.ToString()).Should().Be("Brrr");
    }

    [Fact]
    public void TestResolve()
    {
      var sut = new Either<string, int>(42);
      sut.Resolve(s => s.Length, i => i).Should().Be(42);
      int? canary = null;
      sut.Resolve(s => { canary = -1; }, i => { canary = 1;});
      canary.Should().Be(1);

      var sut2 = new Either<string, int>("Brrr");
      sut2.Resolve(s => s.Length, i => i).Should().Be(4);
      int? canary2 = null;
      sut2.Resolve(s => { canary2 = -1; }, i => { canary2 = 1; });
      canary2.Should().NotBeNull();
      canary2.Should().Be(-1);
    }

    [Fact]
    public void TestSelect()
    {
      var sut = new Either<string, int>(42);
      var q =
        from s in sut
        select s * 3;
      q.Resolve(s => s.Length, i => i).Should().Be(126);
    }

    [Fact]
    public void TestSelectMany()
    {
      var sut = new Either<string, int>(42);
      var sut2 = new Either<string, int>(3);
      var q =
        from s in sut
        from r in sut2
        select s * r;
      q.Resolve(s => s.Length, i => i).Should().Be(126);
    }

  }
}
