namespace Fsky.Term.Views

open Terminal.Gui
open Fsky.Term

module About =
  let view navigateTo =
    let label = Label("Welcome to the About Page")

    let homeBtn =
      Button("Home")
        .Y(Pos.Bottom(label))
        .OnAccept(fun _ -> navigateTo("/"))

    let login =
      Button("Login")
        .Y(Pos.Bottom(homeBtn))
        .OnAccept(fun _ -> navigateTo("/login"))

    Window("About")
      .Content(label, homeBtn, login)
