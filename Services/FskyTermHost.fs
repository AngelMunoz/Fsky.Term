namespace Fsky.Term.Services

open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging

open Terminal.Gui

open Navs
open Navs.Terminal.Gui
open Fsky.Term

type FskyTermHost
  (
    logger: ILogger<FskyTermHost>,
    config: IConfiguration,
    router: IRouter<Window>
  ) =
  interface IHostedLifecycleService with
    member _.StartingAsync(cancellationToken) = task {
      Application.QuitKey <- Key.F10
      Application.Init()

      match config.GetValue("DeepLink") with
      | null -> do! router.Navigate("/login", cancellationToken) :> Task
      | deepLink ->
        let! result = router.Navigate deepLink

        match result with
        | Error e ->
          match e with
          | CantDeactivate _
          | SameRouteNavigation -> return failwith "This should not happen"
          | NavigationFailed(message) ->
            logger.LogError(
              "Navigation failed: {message}, navigating to login",
              message
            )

            do! router.Navigate("/login", cancellationToken) :> Task
          | RouteNotFound(url) ->
            logger.LogWarning(
              "Route not found: {url}, navigating to login",
              url
            )

            do! router.Navigate("/login", cancellationToken) :> Task
          | CantActivate(activatedRoute) ->
            logger.LogWarning(
              "Missing conditions to activate route: {activatedRoute}, navigating to login",
              activatedRoute
            )

            do! router.Navigate("/login", cancellationToken) :> Task
          | NavigationCancelled -> logger.LogInformation("Navigation cancelled")
          | GuardRedirect(redirectTo) ->
            logger.LogInformation(
              "Guard redirecting to: {redirectTo}",
              redirectTo
            )
        | Ok _ -> logger.LogInformation("Deep link navigation successful")

      return! Task.CompletedTask
    }

    member _.StopAsync _ =
      Application.Shutdown()
      Task.CompletedTask

    member _.StartedAsync _ =
      task { Application.Run(new RouterOutlet(router)) } |> Task.FireAndForget
      Task.CompletedTask

    member _.StartAsync _ = Task.CompletedTask

    member _.StoppedAsync _ = Task.CompletedTask

    member _.StoppingAsync _ = Task.CompletedTask
