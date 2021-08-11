using Discography.Core.Enums;
using Discography.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discography.Data.Context
{
    public class SeedData
    {

        public static void Initialize(IServiceProvider services)
        {
            // Perform the initial DB migration if the DB doesn't exist
            // Eliminates need to call Update-Database during project setup
            var context = services.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            // Seed Bands
            if (!context.Bands.Any())
            {
                context.Bands.Add(new Band()
                {
                    Name = "Led Zeppelin",
                    AlsoKnownAs = "New Yardbirds",
                    ActivePeriods = new List<ActivePeriod>()
                    {
                        new ActivePeriod()
                        {
                            StartYear = 1968,
                            EndYear = 1980
                        },
                        new ActivePeriod()
                        {
                            StartYear = 1985
                        },
                        new ActivePeriod()
                        {
                            StartYear = 1988
                        },
                        new ActivePeriod()
                        {
                            StartYear = 1995
                        },
                        new ActivePeriod()
                        {
                            StartYear = 2007
                        }
                    },
                    YearOfFormation = 1968,
                    Description = "Led Zeppelin were an English rock band formed in London in 1968. The group consisted of vocalist Robert Plant, guitarist Jimmy Page, bassist/keyboardist " +
                    "John Paul Jones, and drummer John Bonham. With a heavy, guitar-driven sound, they are cited as one of the progenitors of hard rock and heavy metal, although their style " +
                    "drew from a variety of influences, including blues and folk music. Led Zeppelin have been credited as significantly impacting the nature of the music industry, particularly" +
                    " in the development of album-oriented rock (AOR) and stadium rock. Many critics consider Led Zeppelin one of the most innovative and influential rock groups in history.",
                    Genres = Genres.Blues | Genres.Rock | Genres.Folk
                });

                context.SaveChanges();
            }

            var bandId = context.Bands.Single(b => b.Name == "Led Zeppelin").Id;

            //Seed roles
            if (!context.Musicians.Any())
            {
                context.Musicians.Add(new Musician()
                {
                    FirstName = "James",
                    LastName = "Page",
                    AlsoKnownAs = "Jimmy Page",
                    Biography = "James Patrick Page OBE (born 9 January 1944)[1][2] is an English musician, songwriter, " +
                    "multi-instrumentalist and record producer who achieved international success as the guitarist and founder of the rock band Led Zeppelin.",
                    DateOfBirth = new DateTime(1944, 1, 9),
                    DateOfDeath = null,
                    Instruments = Instruments.Guitar,
                    BandId = bandId
                });
                context.Musicians.Add(new Musician()
                {
                    FirstName = "John",
                    MiddleName = "Richard",
                    LastName = "Baldwin",
                    AlsoKnownAs = "John Paul Jones",
                    Biography = "John Richard Baldwin (born 3 January 1946),[1] better known by his stage name John Paul Jones, is an English musician and record producer " +
                    "who was the bassist and keyboardist for the rock band Led Zeppelin. Prior to forming the band with Jimmy Page in 1968, he was a session musician and arranger. " +
                    "After the death of drummer John Bonham in 1980, Led Zeppelin disbanded, and Jones developed a solo career. He has collaborated with musicians across a variety of " +
                    "genres,[2] including Josh Homme and Dave Grohl with the supergroup Them Crooked Vultures.",
                    DateOfBirth = new DateTime(1946, 1, 3),
                    DateOfDeath = null,
                    Instruments = Instruments.Bass | Instruments.Organ,
                    BandId = bandId
                });
                context.Musicians.Add(new Musician()
                {
                    FirstName = "Robert",
                    MiddleName = "Anthony",
                    LastName = "Plant",
                    AlsoKnownAs = "Robert Plant",
                    Biography = "Robert Anthony Plant CBE (born 20 August 1948) is a British singer and songwriter, best known as the lead singer and lyricist of the English rock band Led Zeppelin.",
                    DateOfBirth = new DateTime(1948, 8, 20),
                    DateOfDeath = null,
                    Instruments = Instruments.Harmonica,
                    BandId = bandId
                });
                context.Musicians.Add(new Musician()
                {
                    FirstName = "John",
                    MiddleName = "Henry",
                    LastName = "Bonham",
                    AlsoKnownAs = "Bonzo",
                    Biography = "John Henry 'Bonzo' Bonham (31 May 1948 – 25 September 1980) was an English musician and songwriter, best known as the drummer for the English rock band Led Zeppelin. " +
                    "Esteemed for his speed, power, fast single-footed kick drumming, distinctive sound, and feel for the groove,[1] he is regarded as one of the greatest and most influential rock drummers in history.",
                    DateOfBirth = new DateTime(1948, 5, 31),
                    DateOfDeath = new DateTime(1980, 9, 25),
                    Instruments = Instruments.Drums,
                    BandId = bandId
                });

                context.SaveChanges();
            }

            if (!context.Albums.Any())
            {
                context.Albums.Add(new Album()
                {
                    Title = "Led Zeppelin",
                    DateReleased = new DateTime(1969, 1, 12),
                    Label = "Atlantic",
                    BandId = bandId
                });

                context.SaveChanges();
            }


            if (!context.Songs.Any())
            {
                var albumId = context.Albums.Single(a => a.Title == "Led Zeppelin").Id;

                var composerId = context.Musicians.Single(a => a.FirstName == "James").Id;

                var lyricistId = context.Musicians.Single(a => a.FirstName == "Robert").Id;

                context.Songs.Add(new Song()
                {
                    Title = "Good Times Bad Times",
                    DateReleased = new DateTime(1969, 3, 10),
                    AlbumId = albumId,
                    BandId = bandId,
                    DurationInSeconds = 163,
                    ComposerId = composerId,
                    LyricistId = lyricistId,
                    DateRecorded = new DateTime(1968, 10, 5),
                    Genres = Genres.Blues | Genres.Rock,
                    Lyrics = "Good time bad times..."
                });

                context.SaveChanges();
            }
        }
    }
}
