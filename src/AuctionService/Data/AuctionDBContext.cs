using System;
using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

public class AuctionDBContext :DbContext
{
    public AuctionDBContext(DbContextOptions options) : base(options)  
    {
        
    }
    public DbSet<Auction> Auctions { get; set; }
    

}
