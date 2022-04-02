using UnityEngine;

public struct AgeInfo
{
    public int years;
    public int days;

    public AgeInfo(int years, float days)
    {
        this.years = years;
        this.days = Mathf.FloorToInt(days);
    }

    public AgeInfo(int days)
    {
        years = days / 365;
        this.days = days - years * 365;
    }

    override public string ToString()
    {
        var months = new int[] {
            31, // Jan
            28, // Feb
            31, // Mar
            30, // Apr
            31, // May
            30, // Jun
            30, // Jul
            31, // Aug
            30, // Sep
            31, // Oct
            30, // Nov
            31, // Dec
        };
        var d = days;
        for (int i = 0; i < months.Length; i++)
        {
            if (d < months[i])
            {
                if (i == 0)
                {
                    return string.Format("{0}Y {1}D", years, days);
                }
                return string.Format("{0}Y {1}M", years, i);
            }
            d -= months[i];
        }
        return string.Format("{0} years", years + 1);
    }

    public int Age {
        get
        {
            return years * 365 + days; 
        }
    }
}
