open Terminal.Gui
open Navs.Terminal.Gui

open Fsky.Term
open Fsky.Term.Routes

[<EntryPoint>]
let main argv =
  Application.Init()

  let deepLink = Routes.findDeepLink argv
  router.Navigate(deepLink) |> Task.FireAndForget

  Application.Run(new RouterOutlet(router))
  // Before the application exits, reset Terminal.Gui for clean shutdown
  Application.Shutdown()

  0 // return an integer exit code
