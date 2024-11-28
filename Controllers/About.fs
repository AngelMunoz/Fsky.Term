namespace Fsky.Term.Controllers

open Terminal.Gui

module About =

  type OnNavigate = string -> unit

  type AboutView = OnNavigate -> Window

  let controller (view: AboutView) _ _ =
    view(fun route -> printfn "Navigating to %s" route)
