using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/auctions")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly AuctionDBContext _context;
        private readonly IMapper _mapper;

        public AuctionController(AuctionDBContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions()
        {
            var auctions=await _context.Auctions
                .Include(x => x.Item)
                .OrderBy(x => x.Item.Make)
                .ToListAsync();

                return _mapper.Map<List<AuctionDTO>>(auctions);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDTO>> GetByID(Guid id)
        {
            var auctions=await _context.Auctions
                .Include(x=>x.Item)
                .FirstOrDefaultAsync(x=>x.Id==id);
                if(auctions==null)
                return NotFound();
                return _mapper.Map<AuctionDTO>(auctions);
        }
        [HttpPost]
        public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDTO auctionDTO)
        {
            var auction = _mapper.Map<Auction>(auctionDTO);
            // TODO: add current user as seller
            auction.Seller="test";
             _context.Auctions.Add(auction);
             var result = await _context.SaveChangesAsync()>0;
             if(!result) return BadRequest("Coludn`t save changed to the DB");
             return CreatedAtAction(nameof(GetByID),new{auction.Id},_mapper.Map<AuctionDTO>(auction));
        }
        [HttpPut("{Id}")]
        public async Task<ActionResult> UpdateAuction(Guid Id,UpdateAuctionDTO updateAuctionDTO)
        {
            var auction=await _context.Auctions
                .Include(x=>x.Item)
                .FirstOrDefaultAsync(x=>x.Id == Id);
                if(auction==null) return NotFound();
                // TODO: Check Seller==Username
                auction.Item.Make=updateAuctionDTO.Make??auction.Item.Make;
                auction.Item.Model=updateAuctionDTO.Model??auction.Item.Model;
                auction.Item.Color=updateAuctionDTO.Color??auction.Item.Color;
                auction.Item.Mileage=updateAuctionDTO.Mileage??auction.Item.Mileage;
                auction.Item.Year=updateAuctionDTO.Year??auction.Item.Year;
                
                var result=await _context.SaveChangesAsync()>0;
                if(result) return Ok();
                return BadRequest("Problem Saving Changes");
        }
        [HttpDelete("{Id}")]
        public async Task<ActionResult> DeleteAuction(Guid Id)
        {
            var auction= await _context.Auctions.FindAsync(Id);
            if(auction==null) return NotFound();
            // TODO: Check Seller==Username
             _context.Remove(auction);
             var result=await _context.SaveChangesAsync()>0;
             if(!result) return BadRequest("Problem Deleting Auction");
             return Ok();

        }
    }
}
