namespace Fsky.Term.Views

open Terminal.Gui
open Fsky.Term

module Login =

  let view onLogin =
    let window =
      Window($"Bluesky Login (Press %O{Application.QuitKey} to quit)")

    let usernameLabel = Label("Username:")

    let userNameText =
      TextField()
        .X(Pos.Right(usernameLabel) + Pos(1))
        .Width(Dim.Fill())

    let passwordLabel =
      Label("Password:")
        .X(Pos.Left(usernameLabel))
        .Y(Pos.Bottom(usernameLabel) + Pos(1))

    let passwordText =
      TextField()
        .Secret(true)
        .X(Pos.Left(userNameText))
        .Y(Pos.Top(passwordLabel))
        .Width(Dim.Fill())

    let btnLogin =
      Button("Login")
        .X(Pos.Bottom(passwordLabel) + Pos(1))
        .Y(Pos.Center())
        .IsDefault(true)
        .OnAccept(fun _ -> onLogin(userNameText.Text, passwordText.Text))

    window.Content(
      usernameLabel,
      userNameText,
      passwordLabel,
      passwordText,
      btnLogin
    )
