namespace ClassDesignCase;

public class Distance
{
    private decimal meters;
    private const decimal metersToMiles = 0.00062137119223733m;

    public Distance() : this(0)
    {
        meters = 0;
    }

    public Distance(decimal meters)
    {
        this.meters = meters;
    }

    public decimal AsMeaters()
    {
        return meters;
    }

    public decimal AsMiles()
    {
        return meters * 0.00062137119223733m;

    }
    public static Distance operator +(Distance d1, Distance d2)
    {
        return new Distance(d1.meters + d2.meters);
    }
    public static Distance FromMiles(decimal miles)
    {
        return new Distance(miles / 0.00062137119223733m);
    }
}