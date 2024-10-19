using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;
using ZstdSharp.Unsafe;

namespace SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery]SearchParams searchParams)
        {
            var query =  DB.PagedSearch<Item>();
            query.Sort(x=>x.Ascending(a=>a.Make));
            if(!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full,searchParams.SearchTerm).SortByTextScore();
            }
            query=searchParams.OrderBy switch
            {
                "make"  => (PagedSearch<Item>)query.Sort(x=>x.Ascending(a=>a.Make)),
                "model" =>(PagedSearch<Item>)query.Sort(x=>x.Ascending(a=>a.Model)),
                "year"  =>(PagedSearch<Item>)query.Sort(x=>x.Ascending(a=>a.Year)),
                "color" =>(PagedSearch<Item>)query.Sort(x=>x.Ascending(a=>a.Color)),
                "new"   =>(PagedSearch<Item>)query.Sort(x=>x.Descending(a=>a.CreatedAt)),
                 _ =>(PagedSearch<Item>)query.Sort(x=>x.Ascending(a=>a.AuctionEnd))

            };
            query=searchParams.FilterBy switch
            {
                "finished"  =>(PagedSearch<Item>)query.Match(x=>x.AuctionEnd<DateTime.UtcNow),
                "endingsoon" =>(PagedSearch<Item>)query.Match(x=>x.AuctionEnd<DateTime.UtcNow.AddHours(6)
                                            &&x.AuctionEnd>DateTime.UtcNow),
                 _ =>(PagedSearch<Item>)query.Match(x=>x.AuctionEnd>DateTime.UtcNow)

            };
            if(!string.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(x=>x.Seller==searchParams.Seller);
            }
            if(!string.IsNullOrEmpty(searchParams.Winner))
            {
                query.Match(x=>x.Winner==searchParams.Winner);
            }
            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);
            var result = await query.ExecuteAsync();
            return Ok(new{
                results=result.Results,
                PageCount=result.PageCount,
                TotalCount=result.TotalCount
            });

        }
    }
}
