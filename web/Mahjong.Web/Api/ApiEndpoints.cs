using System.Security.Claims;
using System.Text.RegularExpressions;
using Mahjong.Web.Client.Api;
using Mahjong.Web.Data;
using Mahjong.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Mahjong.Web.Api;

public static partial class ApiEndpoints
{
    public const string GamesRateLimit = "games";

    public static void MapMahjongApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/client-config", (IOptions<AdsOptions> ads, IOptions<GameOptions> game) =>
            new ClientConfig(ads.Value.ClientId, ads.Value.RailSlot, ads.Value.BannerSlot, ads.Value.ResultsSlot, game.Value.ShowHiddenLayouts));

        api.MapGet("/leaderboards/{layout}", async (string layout, GameService games) =>
            Results.Ok(await games.GetLeaderboardAsync(layout)));

        var games = api.MapGroup("/games").RequireAuthorization().RequireRateLimiting(GamesRateLimit);

        games.MapPost("/", async (StartGameRequest request, ClaimsPrincipal user, GameService service, IOptions<GameOptions> options) =>
        {
            var started = await service.StartAsync(UserId(user), request.Layout, options.Value.ShowHiddenLayouts);
            return started is null ? Results.BadRequest($"Unknown layout \"{request.Layout}\".") : Results.Ok(started);
        });

        games.MapPost("/{id:guid}/finish", async (Guid id, FinishGameRequest request, ClaimsPrincipal user, GameService service) =>
        {
            var result = await service.FinishAsync(UserId(user), id, request.Record);
            return result.Outcome == GameActionOutcome.Ok ? Results.Ok(result.Response) : ToResult(result.Outcome, result.Error);
        });

        games.MapPost("/{id:guid}/pause", async (Guid id, ClaimsPrincipal user, GameService service) =>
            ToResult(await service.SetPausedAsync(UserId(user), id, paused: true)));

        games.MapPost("/{id:guid}/resume", async (Guid id, ClaimsPrincipal user, GameService service) =>
            ToResult(await service.SetPausedAsync(UserId(user), id, paused: false)));

        var me = api.MapGroup("/me").RequireAuthorization();

        me.MapGet("/", async (ClaimsPrincipal principal, UserManager<ApplicationUser> users) =>
        {
            var user = await users.GetUserAsync(principal);
            return user is null
                ? Results.Unauthorized()
                : Results.Ok(new PlayerProfile(user.DisplayName, user.PreferredTileSet, user.GamesPlayed, user.GamesWon));
        });

        me.MapPut("/", async (UpdateProfileRequest request, ClaimsPrincipal principal, UserManager<ApplicationUser> users) =>
        {
            var user = await users.GetUserAsync(principal);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (request.DisplayName is { } name)
            {
                name = name.Trim();
                if (name.Length == 0 || name.Length > ApplicationUser.DisplayNameMaxLength)
                {
                    return Results.BadRequest($"Display names must be 1-{ApplicationUser.DisplayNameMaxLength} characters.");
                }

                user.DisplayName = name;
            }

            if (request.PreferredTileSet is { } tileSet)
            {
                if (!TileSetId().IsMatch(tileSet))
                {
                    return Results.BadRequest("Unknown tile set.");
                }

                user.PreferredTileSet = tileSet;
            }

            await users.UpdateAsync(user);
            return Results.NoContent();
        });
    }

    private static IResult ToResult(GameActionOutcome outcome, string? error = null) => outcome switch
    {
        GameActionOutcome.Ok => Results.NoContent(),
        GameActionOutcome.NotFound => Results.NotFound(),
        GameActionOutcome.Forbidden => Results.Forbid(),
        GameActionOutcome.AlreadyFinished => Results.Conflict("This game has already been finished."),
        _ => Results.BadRequest(error),
    };

    private static string UserId(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [GeneratedRegex("^[a-z0-9-]{1,32}$")]
    private static partial Regex TileSetId();
}
