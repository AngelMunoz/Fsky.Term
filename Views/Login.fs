namespace Fsky.Term.Views

open System.Threading.Tasks

open FSharp.Data.Adaptive

open Terminal.Gui

open Fsky.Term

open Navs.Terminal.Gui

module Login =
  type ViewArgs = {
    title: string
    username: string option
    requestLogin: (string * string) -> Task<unit>
  }

  let view (args: ViewArgs) =
    let window = Window($"{args.title} (Press %O{Application.QuitKey} to quit)")

    let enableButton = cval true
    let username = cval (defaultArg args.username "")
    let password = cval ""


    let usernameLabel = Label("Username:")

    let usernameField =
      TextField()
        .X(Pos.Right(usernameLabel) + Pos(1))
        .Width(Dim.Fill())
        .Text(username)

    let passwordLabel =
      Label("Password:")
        .X(Pos.Left(usernameLabel))
        .Y(Pos.Bottom(usernameLabel) + Pos(1))

    let PasswordField =
      TextField()
        .Secret(true)
        .X(Pos.Left(usernameField))
        .Y(Pos.Top(passwordLabel))
        .Width(Dim.Fill())
        .Text(password)

    let btnLogin =
      Button("Login")
        .X(Pos.Bottom(passwordLabel) + Pos(1))
        .Y(Pos.Center())
        .IsDefault(true)
        .Enabled(enableButton)
        .OnAccept(fun _ ->
          task {
            if username.Value = "" || password.Value = "" then
              MessageBox.Query("Missing fields", "Username and Password are required") |> ignore
              return ()
            else
              AVal.setValue enableButton false
              try
                do! args.requestLogin(username.Value, password.Value)

              finally
                AVal.setValue enableButton true
              return ()
          }
          |> Task.FireAndForget
        )
    let logingMsgLabel =
      Label("Logging in...")
        .Visible(enableButton |> AVal.map not)
        .X(Pos.Right(btnLogin) + Pos(1))
        .Y(Pos.Center())

    window.Content(
      usernameLabel,
      usernameField,
      passwordLabel,
      PasswordField,
      btnLogin,
      logingMsgLabel
    )
