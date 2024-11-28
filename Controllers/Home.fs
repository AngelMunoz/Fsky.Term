namespace Fsky.Term.Controllers

open Terminal.Gui


module Home =

  type OnNavigate = string -> unit

  type HomeView = OnNavigate -> Window

  let controller (home: HomeView) _ _ =
    home(fun route -> printfn "Navigating to %s" route)
