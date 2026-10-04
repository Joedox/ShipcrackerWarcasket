using Xunit;

namespace ShipcrackerWarcasket.Tests;

// Breach Jump flight time: distance over speed, the space burn's speed factor and cap, and the
// flyer def's minimum, which outranks the cap.
public class FlightSecondsTests
{
    private const float Speed = 12f;
    private const float Precision = 1e-5f;

    private static BreachJumpExtension SpaceBurn(float factor = 3f, float maxSeconds = 4f) =>
        new() { spaceFlightSpeedFactor = factor, spaceFlightMaxSeconds = maxSeconds };

    [Fact]
    public void Planet_IsDistanceOverSpeed()
    {
        Assert.Equal(2f, BreachJumpExtension.FlightSeconds(24f, Speed, null, 0f), Precision);
    }

    [Fact]
    public void Planet_IgnoresTheSpaceCap()
    {
        Assert.Equal(20f, BreachJumpExtension.FlightSeconds(240f, Speed, null, 0f), Precision);
    }

    [Fact]
    public void ShortHop_CountsAsOneCell()
    {
        Assert.Equal(1f / Speed, BreachJumpExtension.FlightSeconds(0.3f, Speed, null, 0f), Precision);
    }

    [Fact]
    public void Minimum_FloorsAShortFlight()
    {
        Assert.Equal(0.5f, BreachJumpExtension.FlightSeconds(3f, Speed, null, 0.5f), Precision);
    }

    [Fact]
    public void Space_DividesByTheSpeedFactor()
    {
        Assert.Equal(2f, BreachJumpExtension.FlightSeconds(72f, Speed, SpaceBurn(), 0.5f), Precision);
    }

    [Fact]
    public void Space_CapsALongFlight()
    {
        Assert.Equal(4f, BreachJumpExtension.FlightSeconds(1000f, Speed, SpaceBurn(), 0.5f), Precision);
    }

    [Fact]
    public void Space_MinimumOutranksTheCap()
    {
        Assert.Equal(0.5f, BreachJumpExtension.FlightSeconds(1000f, Speed, SpaceBurn(maxSeconds: 0.2f), 0.5f), Precision);
    }

    [Fact]
    public void Space_ZeroFactorIsClampedRatherThanDividingByZero()
    {
        var seconds = BreachJumpExtension.FlightSeconds(12f, Speed, SpaceBurn(factor: 0f, maxSeconds: 1000f), 0f);

        Assert.Equal(100f, seconds, 1e-3f);
    }
}
