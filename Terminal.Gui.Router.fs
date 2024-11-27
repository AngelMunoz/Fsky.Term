namespace Navs.Terminal.Gui

open System
open System.Threading
open System.Threading.Tasks
open System.Runtime.InteropServices

open Terminal.Gui

open FSharp.Data.Adaptive

open Navs
open Navs.Router


type TerminalGuiRouter(routes, [<Optional>] ?splash: Func<Window>) =
  let router =
    let splash = splash |> Option.map(fun f -> fun () -> f.Invoke())

    Router.build<Window>(routes, ?splash = splash)

  interface IRouter<Window> with

    member _.State = router.State

    member _.StateSnapshot = router.StateSnapshot

    member _.Route = router.Route

    member _.RouteSnapshot = router.RouteSnapshot

    member _.Content = router.Content

    member _.ContentSnapshot = router.ContentSnapshot


    member _.Navigate(a, [<Optional>] ?b) =
      router.Navigate(a, ?cancellationToken = b)

    member _.NavigateByName(a, [<Optional>] ?b, [<Optional>] ?c) =
      router.NavigateByName(a, ?routeParams = b, ?cancellationToken = c)

[<Class; Sealed>]
type Route =

  static member define
    (name, path, handler: RouteContext -> INavigable<Window> -> Async<#Window>)
    : RouteDefinition<Window> =
    Navs.Route.define<Window>(
      name,
      path,
      fun c n -> async {
        let! result = handler c n
        return result :> Window
      }
    )

  static member define
    (
      name,
      path,
      handler:
        RouteContext -> INavigable<Window> -> CancellationToken -> Task<#Window>
    ) : RouteDefinition<Window> =
    Navs.Route.define(
      name,
      path,
      fun c n t -> task {
        let! value = handler c n t
        return value :> Window
      }
    )

  static member define
    (name, path, handler: RouteContext -> INavigable<Window> -> #Window)
    : RouteDefinition<Window> =
    Navs.Route.define<Window>(name, path, (fun c n -> handler c n :> Window))

module Interop =

  type Route =
    [<CompiledName "Define">]
    static member inline define
      (name, path, handler: Func<RouteContext, INavigable<Window>, #Window>)
      =
      Navs.Route.define(name, path, (fun c n -> handler.Invoke(c, n) :> Window))

    [<CompiledName "Define">]
    static member inline define
      (
        name,
        path,
        handler:
          Func<
            RouteContext,
            INavigable<Window>,
            CancellationToken,
            Task<#Window>
           >
      ) =
      Navs.Route.define(
        name,
        path,
        (fun c n t -> task {
          let! value = handler.Invoke(c, n, t)
          return value :> Window
        })
      )

type RouterOutlet(router: IRouter<Window>) as this =
  inherit Toplevel()

  let disposables = ResizeArray()

  do
    router.Content.AddCallback(fun window ->
      match window with
      | ValueSome w ->
        this.RemoveAll()
        this.Add(w :> View) |> ignore
      | ValueNone -> this.RemoveAll()
    )
    |> disposables.Add


  override _.Dispose(disposing: bool) : unit =
    base.Dispose(disposing: bool)
    disposables |> Seq.iter(fun d -> d.Dispose())
