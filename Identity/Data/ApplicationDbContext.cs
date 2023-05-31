﻿using Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Data
{
	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions options) : base(options)
		{

		}

		public DbSet<ApplicationUser> ApplicationUsers { get; set; }
	}
}
