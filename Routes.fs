module Fsky.Term.Routes

open System

open Terminal.Gui

open Navs
open Navs.Terminal.Gui

open Fsky.Term.Views
open Fsky.Term.Controllers


let router: IRouter<Window> =
  TerminalGuiRouter(
    [
      Route.define("home", "/", Home.controller Home.view)
      Route.define("about", "/about", About.controller About.view)
      Route.define("login", "/Login", Login.controller Login.view)
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
  |> Option.defaultValue("/")
