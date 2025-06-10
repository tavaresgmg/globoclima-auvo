using System.Collections.Generic;

namespace GloboClima.Infrastructure.Services;

public static class TranslationService
{
    private static readonly Dictionary<string, string> WeatherConditions = new()
    {
        // Weather conditions - English to Portuguese
        {"clear sky", "céu limpo"},
        {"few clouds", "poucas nuvens"},
        {"scattered clouds", "nuvens dispersas"},
        {"broken clouds", "nuvens fragmentadas"},
        {"shower rain", "chuva de banho"},
        {"rain", "chuva"},
        {"thunderstorm", "tempestade"},
        {"snow", "neve"},
        {"mist", "neblina"},
        {"fog", "nevoeiro"},
        {"haze", "nebulosidade"},
        {"dust", "poeira"},
        {"sand", "areia"},
        {"ash", "cinza vulcânica"},
        {"squall", "rajada"},
        {"tornado", "tornado"},
        
        // Weather API specific conditions
        {"sunny", "ensolarado"},
        {"partly cloudy", "parcialmente nublado"},
        {"cloudy", "nublado"},
        {"overcast", "nublado"},
        {"light rain", "chuva fraca"},
        {"moderate rain", "chuva moderada"},
        {"heavy rain", "chuva forte"},
        {"drizzle", "garoa"},
        {"showers", "pancadas de chuva"},
        {"thundery outbreaks possible", "possíveis tempestades"},
        {"blowing snow", "neve com vento"},
        {"blizzard", "nevasca"},
        {"freezing fog", "nevoeiro congelante"},
        {"patchy rain possible", "possibilidade de chuva"},
        {"patchy snow possible", "possibilidade de neve"},
        {"patchy sleet possible", "possibilidade de granizo"},
        {"patchy freezing drizzle possible", "possibilidade de garoa congelante"}
    };

    private static readonly Dictionary<string, string> CountryNames = new()
    {
        // Country names - English to Portuguese
        {"Afghanistan", "Afeganistão"},
        {"Albania", "Albânia"},
        {"Algeria", "Argélia"},
        {"Argentina", "Argentina"},
        {"Armenia", "Armênia"},
        {"Australia", "Austrália"},
        {"Austria", "Áustria"},
        {"Azerbaijan", "Azerbaijão"},
        {"Bahrain", "Bahrein"},
        {"Bangladesh", "Bangladesh"},
        {"Belarus", "Bielorrússia"},
        {"Belgium", "Bélgica"},
        {"Bolivia", "Bolívia"},
        {"Bosnia and Herzegovina", "Bósnia e Herzegovina"},
        {"Brazil", "Brasil"},
        {"Bulgaria", "Bulgária"},
        {"Cambodia", "Camboja"},
        {"Canada", "Canadá"},
        {"Chile", "Chile"},
        {"China", "China"},
        {"Colombia", "Colômbia"},
        {"Croatia", "Croácia"},
        {"Cuba", "Cuba"},
        {"Cyprus", "Chipre"},
        {"Czech Republic", "República Tcheca"},
        {"Denmark", "Dinamarca"},
        {"Ecuador", "Equador"},
        {"Egypt", "Egito"},
        {"Estonia", "Estônia"},
        {"Finland", "Finlândia"},
        {"France", "França"},
        {"Germany", "Alemanha"},
        {"Greece", "Grécia"},
        {"Hungary", "Hungria"},
        {"Iceland", "Islândia"},
        {"India", "Índia"},
        {"Indonesia", "Indonésia"},
        {"Iran", "Irã"},
        {"Iraq", "Iraque"},
        {"Ireland", "Irlanda"},
        {"Israel", "Israel"},
        {"Italy", "Itália"},
        {"Japan", "Japão"},
        {"Jordan", "Jordânia"},
        {"Kazakhstan", "Cazaquistão"},
        {"Kenya", "Quênia"},
        {"Kuwait", "Kuwait"},
        {"Latvia", "Letônia"},
        {"Lebanon", "Líbano"},
        {"Lithuania", "Lituânia"},
        {"Luxembourg", "Luxemburgo"},
        {"Malaysia", "Malásia"},
        {"Mexico", "México"},
        {"Mongolia", "Mongólia"},
        {"Morocco", "Marrocos"},
        {"Netherlands", "Países Baixos"},
        {"New Zealand", "Nova Zelândia"},
        {"Norway", "Noruega"},
        {"Pakistan", "Paquistão"},
        {"Peru", "Peru"},
        {"Philippines", "Filipinas"},
        {"Poland", "Polônia"},
        {"Portugal", "Portugal"},
        {"Qatar", "Catar"},
        {"Romania", "Romênia"},
        {"Russia", "Rússia"},
        {"Saudi Arabia", "Arábia Saudita"},
        {"Serbia", "Sérvia"},
        {"Singapore", "Singapura"},
        {"Slovakia", "Eslováquia"},
        {"Slovenia", "Eslovênia"},
        {"South Africa", "África do Sul"},
        {"South Korea", "Coreia do Sul"},
        {"Spain", "Espanha"},
        {"Sweden", "Suécia"},
        {"Switzerland", "Suíça"},
        {"Thailand", "Tailândia"},
        {"Turkey", "Turquia"},
        {"Ukraine", "Ucrânia"},
        {"United Arab Emirates", "Emirados Árabes Unidos"},
        {"United Kingdom", "Reino Unido"},
        {"United States", "Estados Unidos"},
        {"United States of America", "Estados Unidos da América"},
        {"Uruguay", "Uruguai"},
        {"Venezuela", "Venezuela"},
        {"Vietnam", "Vietnã"}
    };

    public static string TranslateWeatherCondition(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return condition;

        var lowerCondition = condition.ToLower();
        return WeatherConditions.TryGetValue(lowerCondition, out var translation) 
            ? translation 
            : condition;
    }

    public static string TranslateCountryName(string countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName))
            return countryName;

        return CountryNames.TryGetValue(countryName, out var translation) 
            ? translation 
            : countryName;
    }
}