namespace GloboClima.Domain.Models.Country;

public class CountryResponse
{
    public Name Name { get; set; } = new();
    public List<string> Tld { get; set; } = new();
    public string Cca2 { get; set; } = string.Empty;
    public string Ccn3 { get; set; } = string.Empty;
    public string Cca3 { get; set; } = string.Empty;
    public string Cioc { get; set; } = string.Empty;
    public bool Independent { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool UnMember { get; set; }
    public Dictionary<string, Currency> Currencies { get; set; } = new();
    public Idd Idd { get; set; } = new();
    public List<string> Capital { get; set; } = new();
    public List<string> AltSpellings { get; set; } = new();
    public string Region { get; set; } = string.Empty;
    public string Subregion { get; set; } = string.Empty;
    public Dictionary<string, string> Languages { get; set; } = new();
    public Dictionary<string, Translation> Translations { get; set; } = new();
    public List<double> Latlng { get; set; } = new();
    public bool Landlocked { get; set; }
    public List<string> Borders { get; set; } = new();
    public double Area { get; set; }
    public Dictionary<string, Demonym> Demonyms { get; set; } = new();
    public string Flag { get; set; } = string.Empty;
    public Maps Maps { get; set; } = new();
    public long Population { get; set; }
    public Dictionary<string, double> Gini { get; set; } = new();
    public string Fifa { get; set; } = string.Empty;
    public Car Car { get; set; } = new();
    public List<string> Timezones { get; set; } = new();
    public List<string> Continents { get; set; } = new();
    public Flags Flags { get; set; } = new();
    public CoatOfArms CoatOfArms { get; set; } = new();
    public string StartOfWeek { get; set; } = string.Empty;
    public CapitalInfo CapitalInfo { get; set; } = new();
}

public class Name
{
    public string Common { get; set; } = string.Empty;
    public string Official { get; set; } = string.Empty;
    public Dictionary<string, NativeName> NativeName { get; set; } = new();
}

public class NativeName
{
    public string Official { get; set; } = string.Empty;
    public string Common { get; set; } = string.Empty;
}

public class Currency
{
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
}

public class Idd
{
    public string Root { get; set; } = string.Empty;
    public List<string> Suffixes { get; set; } = new();
}

public class Translation
{
    public string Official { get; set; } = string.Empty;
    public string Common { get; set; } = string.Empty;
}

public class Demonym
{
    public string F { get; set; } = string.Empty;
    public string M { get; set; } = string.Empty;
}

public class Maps
{
    public string GoogleMaps { get; set; } = string.Empty;
    public string OpenStreetMaps { get; set; } = string.Empty;
}

public class Car
{
    public List<string> Signs { get; set; } = new();
    public string Side { get; set; } = string.Empty;
}

public class Flags
{
    public string Png { get; set; } = string.Empty;
    public string Svg { get; set; } = string.Empty;
    public string Alt { get; set; } = string.Empty;
}

public class CoatOfArms
{
    public string Png { get; set; } = string.Empty;
    public string Svg { get; set; } = string.Empty;
}

public class CapitalInfo
{
    public List<double> Latlng { get; set; } = new();
}