module Fsky.Term.Routes

open System

open Terminal.Gui

open Navs
open Navs.Terminal.Gui

open Fsky.Term.Views
open Fsky.Term.Controllers
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging


let build (services: IServiceProvider) : IRouter<Window> =
  let logFactory = services.GetRequiredService<ILoggerFactory>()

  TerminalGuiRouter(
    [
      Route.define("home", "/", Home.controller Home.view)
      Route.define("about", "/about", About.controller About.view)
      Route.define(
        "login",
        "/login?username",
        Login.controller {
          services = Login.buildServices(logFactory, services)
          renderView = Login.view
        }
      )
    ]
  )

let findDeepLink argv =

  argv
  |> Array.tryPick(fun arg ->
    try
      let uri = Uri(arg)
      if uri.Scheme = "fsky" then Some(uri) else None
    with _ ->
      None
  )
  |> Option.map(fun url -> url.PathAndQuery + url.Fragment)

let validateAndSetDeepLink (config: IConfiguration, argv) =
  match config.GetValue("DeepLink") with
  | null ->
    match findDeepLink argv with
    | None -> config["DeepLink"] <- null
    | Some deepLink -> config["DeepLink"] <- deepLink
  | deepLink ->
    try
      let uri = Uri(deepLink)

      if uri.Scheme <> "fsky" then
        ()

      config["DeepLink"] <- uri.PathAndQuery + uri.Fragment
    with _ ->
      config["DeepLink"] <- null
