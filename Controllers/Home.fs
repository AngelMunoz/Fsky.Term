namespace Fsky.Term.Controllers

open System
open System.Threading.Tasks

open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection

open Terminal.Gui

open FSharp.Data.Adaptive

open Fsky.Term
open Fsky.Term.Services
open Fsky.Term.Views


module Home =

  type Services = {
    logger: ILogger
    feedService: FeedService
    authService: AuthService
  }

  type CtrlArgs = {
    services: Services
    renderView: Home.ViewArgs -> Window
  }

  let buildServices
    (loggerFactory: ILoggerFactory, services: IServiceProvider)
    =
    {
      logger = loggerFactory.CreateLogger("HomeCtrl")
      feedService = services.GetRequiredService<FeedService>()
      authService = services.GetRequiredService<AuthService>()
    }

  let controller (args: CtrlArgs) _ _ =
    let {
          feedService = feedService
          authService = auth
        } =
      args.services

    task {
      do! feedService.LoadFeeds()
      let loaded = feedService.Feeds |> AVal.force

      match loaded |> List.tryHead with
      | Some feed -> do! feedService.LoadFeed(feed.Uri)
      | None -> ()
    }
    |> Task.FireAndForget

    args.renderView {
      handle =
        auth.Handle
        |> AVal.map(fun h ->
          h |> Option.map(fun h -> h.Handle) |> Option.defaultValue ""
        )
      requestNavigation = ignore
      feeds = feedService.Feeds
      requestFeed = fun _ -> task { }
      selectedFeed = feedService.Posts
    }
