using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Entities;

namespace WebApplication1.Service;

public class SuperHeroService
{
    private readonly DataContext _context;

    public SuperHeroService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<SuperHeroDto>> GetAllHeroesAsync()
    {
      return await _context.SuperHeroes.Select(hero => new SuperHeroDto
      {
          Id = hero.Id,
          Name = hero.Name,
          FirstName = hero.FirstName,
          LastName = hero.LastName,
          Place = hero.Place
      }).ToListAsync();
    }

    public async Task<SuperHeroDto> GetHeroByIdAsync(int id)
    {
        var hero = await _context.SuperHeroes.FindAsync(id);
        if (hero is null)
            return null;
        return new SuperHeroDto
        {
            Id = hero.Id,
            Name = hero.Name,
            FirstName = hero.FirstName,
            LastName = hero.LastName,
            Place = hero.Place
        };
    }

    public async Task<SuperHeroDto> AddHeroAsync(SuperHeroCreateDto superHero)
    {
        var hero = new SuperHero
        {
            Name = superHero.Name,
            FirstName = superHero.FirstName,
            LastName = superHero.LastName,
            Place = superHero.Place
        };

        await _context.SuperHeroes.AddAsync(hero);
        await _context.SaveChangesAsync();
        return new SuperHeroDto
        {
            Id = hero.Id,
            Name = hero.Name,
            FirstName = hero.FirstName,
            LastName = hero.LastName,
            Place = hero.Place
        };
    }

    public async Task<SuperHeroDto> UpdateHeroAsync(SuperHeroDto hero)
    {
        var dbHero = await _context.SuperHeroes.FindAsync(hero.Id);
        if (dbHero is null)
            return null;

        dbHero.Name = hero.Name;
        dbHero.FirstName = hero.FirstName;
        dbHero.LastName = hero.LastName;
        dbHero.Place = hero.Place;

        await _context.SaveChangesAsync();

        return new SuperHeroDto
        {
            Id = dbHero.Id,
            Name = dbHero.Name,
            FirstName = dbHero.FirstName,
            LastName = dbHero.LastName,
            Place = dbHero.Place
        };
    }


    public async Task<SuperHeroDto> DeleteHeroAsync(int id)
    {
        var dbHero = await _context.SuperHeroes.FindAsync(id);
        if (dbHero is null)
            return null;

        _context.SuperHeroes.Remove(dbHero);
        await _context.SaveChangesAsync();

        return new SuperHeroDto
        {
            Id = dbHero.Id,
            Name = dbHero.Name,
            FirstName = dbHero.FirstName,
            LastName = dbHero.LastName,
            Place = dbHero.Place
        };
    }
}