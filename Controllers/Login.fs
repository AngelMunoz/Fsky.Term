namespace Fsky.Term.Controllers

open System
open System.Threading.Tasks

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Terminal.Gui

open Navs
open UrlTemplates.RouteMatcher

open Fsky.Term
open Fsky.Term.Services
open Fsky.Term.Views

module Login =

  type Services = { logger: ILogger; auth: AuthService }

  type CtrlArgs = {
    services: Services
    renderView: Login.ViewArgs -> Window
  }

  let buildServices
    (loggerFactory: ILoggerFactory, services: IServiceProvider)
    =
    {
      logger = loggerFactory.CreateLogger("LoginCtrl")
      auth = services.GetRequiredService<AuthService>()
    }

  let inline private onLogin (authService: AuthService, nav: INavigable<_>) =
    fun (username, password) -> task {
      match! authService.Login(username, password) with
      | Ok() -> return! nav.Navigate("/") :> Task
      | Error(UnableToAuthenticate) ->
        MessageBox.ErrorQuery(
          "Unable to authenticate.",
          "We were able to connect to the server, but the credentials were not accepted."
        )
        |> ignore

        return ()
      | Error(AuthException _) ->

        MessageBox.ErrorQuery(
          "Unable to authenticate.",
          "The error has been recorded"
        )
        |> ignore

        return ()
    }

  let controller (args: CtrlArgs) (ctx: RouteContext) (nav: INavigable<_>) =
    let { auth = auth } = args.services

    let username =
      ctx.urlMatch
      |> UrlMatch.getParamFromQuery "username"
      |> Option.ofValueOption

    let url =
      match auth.BaseUrl with
      | None -> ""
      | Some url -> url.ToString()

    args.renderView {
      requestLogin = onLogin(auth, nav)
      title = $"Login into %O{url}"
      username = username
    }
