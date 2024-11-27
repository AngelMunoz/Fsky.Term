open Terminal.Gui
open System
open Sampoxo

open Navs
open Navs.Terminal.Gui


let Login _ _ =
  let window = Window($"Example App (%O{Application.QuitKey} to quit)")

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
      .OnAccept(fun _ ->
        if userNameText.Text = "admin" && passwordText.Text = "password" then
          MessageBox.Query("Logging In", "Login Successful", "Ok") |> ignore
          Application.RequestStop()
        else
          MessageBox.ErrorQuery(
            "Logging In",
            "Incorrect username or password",
            "Ok"
          )
          |> ignore
      )

  window.Content(
    usernameLabel,
    userNameText,
    passwordLabel,
    passwordText,
    btnLogin
  )

let Home _ (navigable: INavigable<Window>) =
  let label = Label("Welcome to the Home Page")

  let homeBtn =
    Button("About")
      .Y(Pos.Bottom(label))
      .OnAccept(fun _ ->
        navigable.NavigateByName("about")
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.StartImmediate
      )

  let login =
    Button("Login")
      .Y(Pos.Bottom(homeBtn))
      .OnAccept(fun _ ->
        navigable.NavigateByName("login")
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.StartImmediate
      )

  Window("Home")
    .Content(label, homeBtn, login)

let About _ (navigable: INavigable<Window>) =
  let label = Label("Welcome to the About Page")

  let homeBtn =
    Button("Home")
      .Y(Pos.Bottom(label))
      .OnAccept(fun _ ->
        navigable.NavigateByName("home")
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.StartImmediate
      )

  let login =
    Button("Login")
      .Y(Pos.Bottom(homeBtn))
      .OnAccept(fun _ ->
        navigable.NavigateByName("login")
        |> Async.AwaitTask
        |> Async.Ignore
        |> Async.StartImmediate
      )

  Window("About")
    .Content(label, homeBtn, login)


let routes = [
  Route.define("home", "/", Home)
  Route.define("about", "/about", About)
  Route.define("login", "/Login", Login)
]

let router: IRouter<Window> = TerminalGuiRouter(routes)

[<EntryPoint>]
let main argv =
  Application.Init()

  let deepLink =
    argv
    |> Array.tryFind(fun arg -> arg.StartsWith("sampoxo:"))
    |> Option.bind(fun arg -> 
      try 
        Uri(arg) |> Some
      with _ -> None
    )
    |> Option.map(fun url -> 
       url.PathAndQuery + url.Fragment
     )
    |> Option.defaultValue("/")

  router.Navigate(deepLink) |> Task.FireAndForget

  Application.Run(new RouterOutlet(router))
  // Before the application exits, reset Terminal.Gui for clean shutdown
  Application.Shutdown()

  0 // return an integer exit code
