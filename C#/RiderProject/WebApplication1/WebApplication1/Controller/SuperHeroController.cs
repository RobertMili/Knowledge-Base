using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Entities;
using WebApplication1.Service;

namespace WebApplication1.Controller;

[Route("api/[controller]")]
[ApiController]
public class SuperHeroController : ControllerBase
{
    private readonly SuperHeroService _superHeroService;


    public SuperHeroController(SuperHeroService superHeroService)
    {
        _superHeroService = superHeroService;
    }


    [HttpGet]
    public async Task<ActionResult<List<SuperHeroDto>>> GetAllHeroes()
    {
        var heroes = await _superHeroService.GetAllHeroesAsync();
        return Ok(heroes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<List<SuperHeroDto>>> GetAllHero(int id)
    {
        var hero = await _superHeroService.GetHeroByIdAsync(id);
        return Ok(hero);
    }

    [HttpPost]
    public async Task<ActionResult<List<SuperHero>>> AddHero(SuperHeroCreateDto hero)
    {
        var addedHero = await _superHeroService.AddHeroAsync(hero);
        return Ok(addedHero);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<List<SuperHero>>> UpdateHero(int id, SuperHeroDto hero)
    {
        if (id != hero.Id)
            return BadRequest("Id mismatch");

        var updatedHero = await _superHeroService.UpdateHeroAsync(hero);
        if (updatedHero is null)
            return NotFound("Hero not found");

        return Ok(updatedHero);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<List<SuperHero>>> DeleteHero(int id)
    {
        var dbHero = await _superHeroService.DeleteHeroAsync(id);

        if (dbHero == null)
        {
            return BadRequest($"Can't find competition with CompetitionID: {id}");
        }

        return Ok(dbHero);
    }
}