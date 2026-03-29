using Microsoft.EntityFrameworkCore;
using Wisdom.Course.Domain.Entities;
using Wisdom.Course.Infrastructure.Data.Configurations;
namespace Wisdom.Course.Infrastructure.Data;

public class CourseDbContext(DbContextOptions<CourseDbContext> options) : DbContext(options)
{
    public DbSet<CourseEntity> Courses => Set<CourseEntity>();
    public DbSet<LessonEntity> Lessons => Set<LessonEntity>();
    public DbSet<TagEntity> Tags => Set<TagEntity>();
    public DbSet<CourseTagEntity> CourseTags => Set<CourseTagEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CourseConfiguration());
        modelBuilder.ApplyConfiguration(new LessonConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());
        modelBuilder.ApplyConfiguration(new CourseTagConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
