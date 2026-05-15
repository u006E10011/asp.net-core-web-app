using DevLog.Data;
using Microsoft.EntityFrameworkCore;

namespace DevLog.Services;

public static class DatabaseInitializationService
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<DevLogDbContext>();

        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await context.Database.EnsureCreatedAsync();
                await EnsureViewsAsync(context);
                return;
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }

    private static async Task EnsureViewsAsync(DevLogDbContext context)
    {
        const string sql = """
            CREATE OR REPLACE VIEW vw_post_overview AS
            SELECT
                p."Id" AS post_id,
                p."Title" AS title,
                p."CreatedAtUtc" AS created_at_utc,
                p."UpdatedAtUtc" AS updated_at_utc,
                p."LikesCount" AS likes_count,
                p."DislikesCount" AS dislikes_count,
                STRING_AGG(t."Name", ', ' ORDER BY t."Name") AS tags
            FROM "Posts" p
            LEFT JOIN "PostTags" pt ON pt."PostId" = p."Id"
            LEFT JOIN "Tags" t ON t."Id" = pt."TagId"
            GROUP BY p."Id", p."Title", p."CreatedAtUtc", p."UpdatedAtUtc", p."LikesCount", p."DislikesCount";

            CREATE OR REPLACE VIEW vw_tag_post_counts AS
            SELECT
                t."Name" AS tag_name,
                COUNT(pt."PostId") AS post_count
            FROM "Tags" t
            LEFT JOIN "PostTags" pt ON pt."TagId" = t."Id"
            GROUP BY t."Name"
            ORDER BY post_count DESC, tag_name ASC;

            CREATE OR REPLACE VIEW vw_daily_post_counts AS
            SELECT
                DATE(p."CreatedAtUtc") AS post_date,
                COUNT(*) AS post_count
            FROM "Posts" p
            GROUP BY DATE(p."CreatedAtUtc")
            ORDER BY post_date;
            """;

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
