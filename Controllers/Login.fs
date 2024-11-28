namespace Fsky.Term.Controllers

open Terminal.Gui

module Login =

  type OnLogin = (string * string) -> unit
  type LoginView = OnLogin -> Window

  let controller (view: LoginView) _ _ =
    // orchestrate everhthing here
    // state management, api calls, etc

    // then call the view with the according parameters
    view(fun (username, password) ->
      printfn "Username: %s, Password: %s" username password
    )
