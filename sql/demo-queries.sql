SELECT COUNT(*) AS total_posts FROM "Posts";

SELECT "Title", "CreatedAtUtc", "LikesCount", "DislikesCount"
FROM "Posts"
ORDER BY "CreatedAtUtc" DESC;

SELECT *
FROM vw_post_overview
ORDER BY created_at_utc DESC;

SELECT *
FROM vw_tag_post_counts;

SELECT *
FROM vw_daily_post_counts;
