using ParkyAPI.Data;
using ParkyAPI.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParkyAPI_XTest
{
    public class DummyDBDataInitializer
    {
        public DummyDBDataInitializer()
        {

        }
        public void Seed(ApplicationDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.NationalParks.AddRange(
                new NationalPark() { Id = 1, Created = DateTime.Now, Established = DateTime.Now, Name = "TestNP1", Picture = null, State = "TestNP1State" },
                new NationalPark() { Id = 2, Created = DateTime.Now, Established = DateTime.Now, Name = "TestNP2", Picture = null, State = "TestNP1State" },
                new NationalPark() { Id = 3, Created = DateTime.Now, Established = DateTime.Now, Name = "TestNP3", Picture = null, State = "TestNP1State" },
                new NationalPark() { Id = 4, Created = DateTime.Now, Established = DateTime.Now, Name = "TestNP4", Picture = null, State = "TestNP1State" }
            );

            context.Trails.AddRange(
                new Trail() 
                {
                    Id = 1,
                    DateCreated = DateTime.Now,
                    Difficulty = Trail.DifficultyType.Easy,
                    Distance = 10.5f,
                    Elevation = 10.5f,
                    Name = "Easy",
                    NationalParkId = 1
                },
                new Trail() 
                {
                    Id = 2,
                    DateCreated = DateTime.Now,
                    Difficulty = Trail.DifficultyType.Moderate,
                    Distance = 10.5f,
                    Elevation = 10.5f,
                    Name = "Med",
                    NationalParkId = 2
                }
            );


            context.Users.AddRange(
                new User() { Id = 1, Username="Patel", Password="Test", Role="Admin", Token="" },
                new User() { Id = 1, Username = "Patil", Password = "Test", Role = "Admin", Token = "" }
            );


            context.SaveChanges();
        }
    }
}
