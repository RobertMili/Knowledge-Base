using Microsoft.AspNetCore.Mvc;
using WebApplication2.Entities;

namespace WebApplication1.Controller;

[Route("api/[controller]")]
[ApiController]
public class SuperHeroController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SuperHero>>> GetAllHeroes()
    {
        var heroes = new List<SuperHero>
        {
            new SuperHero
            {
                Id = 1,
                Name = "Superman",
                FirstName = "Peter",
                LastName = "Parker",
                Place = "New York City"
            }
        };
        return Ok(heroes);

    }

}