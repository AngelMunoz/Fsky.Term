namespace Fsky.Term.Views

open Terminal.Gui
open Fsky.Term

module Home =

  type ViewArgs = {
    title: string
    requestNavigation: string -> unit
  }

  let view onNavigate =
    let label = Label($"Home (Press %O{Application.QuitKey} to quit)")

    let homeBtn =
      Button("About")
        .Y(Pos.Bottom(label))
        .OnAccept(fun _ -> onNavigate("/about"))

    let login =
      Button("Login")
        .Y(Pos.Bottom(homeBtn))
        .OnAccept(fun _ -> onNavigate("/login"))

    Window("Home")
      .Content(label, homeBtn, login)
