namespace Fsky.Term.Controllers

open System
open System.Threading.Tasks

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open FishyFlip

open Terminal.Gui

open Navs
open UrlTemplates.RouteMatcher

open Fsky.Term
open Fsky.Term.Views

module Login =

  type Services = { logger: ILogger; atProto: ATProtocol }

  type CtrlArgs = {
    services: Services
    renderView: Login.ViewArgs -> Window
  }

  let buildServices
    (loggerFactory: ILoggerFactory, services: IServiceProvider)
    =
    {
      logger = loggerFactory.CreateLogger("LoginCtrl")
      atProto = services.GetRequiredService<ATProtocol>()
    }


  let controller (args: CtrlArgs) (ctx: RouteContext) (nav: INavigable<_>) =
    let { logger = log; atProto = atProto } = args.services

    let username =
      ctx.urlMatch
      |> UrlMatch.getParamFromQuery "username"
      |> Option.ofValueOption

    let onLogin (username, password) = task {
      try
        let! session = atProto.AuthenticateWithPasswordAsync(username, password)

        match session with
        | null -> log.LogError("Login failed.")
        | session ->
          log.LogInformation("Login for {handle} successful.", session.Handle)
        return! nav.Navigate("/") :> Task
      with ex ->
        log.LogError(ex, "Login failed.")
        return ()
    }

    let url =
      match atProto.BaseAddress with
      | null -> ""
      | url -> url.ToString()

    args.renderView {
      requestLogin = onLogin
      title = $"Login into %O{url}"
      username = username
    }
