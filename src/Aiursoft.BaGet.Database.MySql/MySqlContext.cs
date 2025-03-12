using Aiursoft.BaGet.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaGet.Database.MySql;

public class MySqlContext(DbContextOptions<MySqlContext> options) : AbstractContext(options);