
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Entities;
using WebApplication1.Service;
using Xunit;
using Xunit.Abstractions;
using Assert = Xunit.Assert;

public class SuperHeroServiceTests
{
    private readonly DataContext _context;
    private readonly SuperHeroService _service;
    private readonly ITestOutputHelper _testOutputHelper;

    public SuperHeroServiceTests( ITestOutputHelper testOutputHelper)
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new DataContext(options);
        _service = new SuperHeroService(_context);

        _testOutputHelper = testOutputHelper;
    }

    private async Task CleanAndVerifyDatabaseAsync()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        var initialCount = await _context.SuperHeroes.CountAsync();
        Assert.Equal(0, initialCount);
    }

    [Fact]
    public async Task AddMultipleHeroesWithSameDateAndGetCorrectCount()
    {
        await CleanAndVerifyDatabaseAsync();

        // Arrange
        var now = DateTime.UtcNow;
        var heroesDto = new List<SuperHeroCreateDto>
        {
            new SuperHeroCreateDto { Name = "Hero1", FirstName = "First1", LastName = "Last1", Place = "Place1" },
            new SuperHeroCreateDto { Name = "Hero2", FirstName = "First2", LastName = "Last2", Place = "Place2" },
            new SuperHeroCreateDto { Name = "Hero3", FirstName = "First3", LastName = "Last3", Place = "Place3" }
        };

        foreach (var heroDto in heroesDto)
        {
            await _service.AddHeroAsync(heroDto);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllHeroesAsync();


        // Assert the count first
        Assert.Equal(heroesDto.Count, result.Count);
    }

    [Fact]
    public async Task AddDuplicateHeroesAndGetCorrectCount()
    {
        await CleanAndVerifyDatabaseAsync();

        // Arrange
        var hero = new SuperHeroCreateDto { Name = "Hero1", FirstName = "First1", LastName = "Last1", Place = "Place1" };
        var heroes = new List<SuperHeroCreateDto> { hero, hero };

        await _service.AddHeroAsync(heroes[0]);
        await _service.AddHeroAsync(heroes[1]);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllHeroesAsync();

        // Print the result details
        foreach (var heroe in result)
        {
            _testOutputHelper.WriteLine($"Hero: {hero.Name}, {hero.FirstName}, {hero.LastName}, {hero.Place}");
        }

        // Assert the count first
        Assert.Equal(heroes.Count, result.Count);

    }

}