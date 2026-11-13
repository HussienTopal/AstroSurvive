using System;
using UnityEngine;

namespace AstroSurvive.Menu
{
    [System.Serializable]
    public class PlanetFact
    {
        public string planetName;
        public string emoji;
        public string headline;
        [TextArea(2, 4)] public string fact1;
        [TextArea(2, 4)] public string fact2;
        [TextArea(2, 4)] public string fact3;
        public string statTag;
        public Color themeColor = Color.white;
        public bool isSecretEasterEgg = false;
    }

    /// <summary>
    /// Database of fun, kid-friendly solar system facts specifically tailored for ages 7–12.
    /// Short sentences, exciting tidbits, clear comparisons.
    /// </summary>
    public static class PlanetFactDatabase
    {
        public static PlanetFact[] GetDefaultFacts()
        {
            return new PlanetFact[]
            {
                new PlanetFact
                {
                    planetName = "Sun",
                    emoji = "☀️",
                    headline = "The Super-Star Powerhouse!",
                    fact1 = "The Sun is so giant that 1.3 million Earths could fit inside it!",
                    fact2 = "Its core is a scorching 15 million degrees Celsius—hotter than anything on Earth!",
                    fact3 = "Light from the Sun zooms through space at super-speed, reaching us in just 8 minutes!",
                    statTag = "Surface Temp: 5,500°C | Age: 4.6 Billion Years",
                    themeColor = new Color(1.0f, 0.75f, 0.2f)
                },
                new PlanetFact
                {
                    planetName = "Mercury",
                    emoji = "🪨",
                    headline = "The Speedy Little Speedster!",
                    fact1 = "Mercury is the smallest planet and zips around the Sun faster than any other planet!",
                    fact2 = "During the day it's super baking hot, but at night it freezes below -170°C!",
                    fact3 = "One year on Mercury is only 88 Earth days long—you'd celebrate birthdays all the time!",
                    statTag = "Year Length: 88 Earth Days | Moons: 0",
                    themeColor = new Color(0.8f, 0.7f, 0.6f)
                },
                new PlanetFact
                {
                    planetName = "Venus",
                    emoji = "🌋",
                    headline = "The Super-Hot Greenhouse Planet!",
                    fact1 = "Venus is the hottest planet in the solar system, even hotter than an oven!",
                    fact2 = "Thick, swirling yellow clouds trap all the heat like a giant space blanket.",
                    fact3 = "Venus spins backwards compared to most planets—on Venus, the Sun rises in the West!",
                    statTag = "Surface Temp: 465°C | Atmosphere: Super Thick",
                    themeColor = new Color(0.95f, 0.8f, 0.4f)
                },
                new PlanetFact
                {
                    planetName = "Earth",
                    emoji = "🌍",
                    headline = "Our Wonderful Blue Home!",
                    fact1 = "The only known planet in the entire universe packed with liquid water, plants, and life!",
                    fact2 = "Earth has a protective magnetic shield that guards us from dangerous cosmic rays.",
                    fact3 = "We travel through space around the Sun at an incredible 107,000 kilometers per hour!",
                    statTag = "Distance to Sun: 150M km | Moons: 1 (The Moon!)",
                    themeColor = new Color(0.2f, 0.65f, 1.0f)
                },
                new PlanetFact
                {
                    planetName = "Mars",
                    emoji = "🔴",
                    headline = "The Red Planet - Our Mission Target!",
                    fact1 = "Mars gets its rusty red color from iron in its soil, just like rusty metal!",
                    fact2 = "It has Olympus Mons, a volcano three times taller than Mount Everest!",
                    fact3 = "AstroSurvive Astronaut: This is where we are headed! Keep your oxygen and power full!",
                    statTag = "Gravity: 38% of Earth | Mission: Survival Landing",
                    themeColor = new Color(0.95f, 0.35f, 0.2f)
                },
                new PlanetFact
                {
                    planetName = "Jupiter",
                    emoji = "🌀",
                    headline = "The King of the Planets!",
                    fact1 = "Jupiter is the biggest planet of all—it's more than double the size of all others combined!",
                    fact2 = "The Great Red Spot is a monstrous swirling storm that has raged for over 300 years!",
                    fact3 = "Jupiter is a gas giant with no solid ground to walk on, and has 95 known moons!",
                    statTag = "Moons: 95 Moons! | Type: Gas Giant",
                    themeColor = new Color(0.85f, 0.65f, 0.45f)
                },
                new PlanetFact
                {
                    planetName = "Saturn",
                    emoji = "🪐",
                    headline = "The Dazzling Lord of the Rings!",
                    fact1 = "Saturn's spectacular rings are made of billions of sparkling chunks of ice and rock!",
                    fact2 = "Even though it's huge, Saturn is so light it would actually float in a giant bathtub of water!",
                    fact3 = "Winds on Saturn can howl up to an unbelievable 1,800 kilometers per hour!",
                    statTag = "Ring Width: 282,000 km | Moons: 146 Moons!",
                    themeColor = new Color(0.9f, 0.8f, 0.55f)
                },
                new PlanetFact
                {
                    planetName = "Uranus",
                    emoji = "❄️",
                    headline = "The Tilted Ice Giant!",
                    fact1 = "Uranus rolls around the Sun on its side, like a giant cosmic bowling ball!",
                    fact2 = "It is surrounded by 13 faint rings and glows with a beautiful icy-blue color from methane gas.",
                    fact3 = "It's one of the coldest places in the solar system, dropping down to -224°C!",
                    statTag = "Tilt: 98 degrees | Type: Ice Giant",
                    themeColor = new Color(0.45f, 0.85f, 0.9f)
                },
                new PlanetFact
                {
                    planetName = "Neptune",
                    emoji = "🌊",
                    headline = "The Wild Windy Blue Giant!",
                    fact1 = "Neptune is the farthest planet from the Sun, deep in the dark, icy outer solar system.",
                    fact2 = "It has the fastest winds recorded anywhere in our solar system—faster than supersonic fighter jets!",
                    fact3 = "It takes Neptune 165 Earth years just to make a single trip around the Sun!",
                    statTag = "Wind Speed: 2,100 km/h | Year: 165 Earth Years",
                    themeColor = new Color(0.2f, 0.45f, 0.95f)
                },
                new PlanetFact
                {
                    planetName = "Pluto",
                    emoji = "⛄",
                    headline = "The Beloved Dwarf Planet!",
                    fact1 = "Even though Pluto is small, it has a famous bright heart-shaped glacier of frozen nitrogen!",
                    fact2 = "Pluto has five moons of its own—its biggest moon, Charon, is half Pluto's size!",
                    fact3 = "It is so far away that sunlight looks just like a bright twinkling evening star from Pluto!",
                    statTag = "Classification: Dwarf Planet | Feature: The Great Ice Heart",
                    themeColor = new Color(0.75f, 0.7f, 0.65f)
                },
                new PlanetFact
                {
                    planetName = "Black Hole",
                    emoji = "🕳️",
                    headline = "SECRET DISCOVERED! The Gravity Monster!",
                    fact1 = "WOAH! You found the hidden cosmic secret lurking at the edge of the universe!",
                    fact2 = "A Black Hole packs so much matter into a tiny point that its gravity swallows EVERYTHING—even light!",
                    fact3 = "Around its edge, called the Event Horizon, time actually slows down like magic!",
                    statTag = "Danger Level: MAXIMUM | Gravity: Super-Massive",
                    themeColor = new Color(0.8f, 0.2f, 1.0f),
                    isSecretEasterEgg = true
                }
            };
        }
    }
}
