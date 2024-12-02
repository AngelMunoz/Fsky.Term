namespace Fsky.Term.Views

open Terminal.Gui
open Fsky.Term
open FSharp.Data.Adaptive
open FsToolkit.ErrorHandling

open FishyFlip

module Home =
  open System.Threading.Tasks

  type ViewArgs = {
    handle: string aval
    requestNavigation: string -> unit
    feeds: Lexicon.App.Bsky.Feed.GeneratorView array aval
    requestFeed: Lexicon.App.Bsky.Feed.GeneratorView -> Task<unit>
    selectedFeed: (string option * Lexicon.App.Bsky.Feed.FeedViewPost) option aval
  }

  let feedSelector (feeds: Lexicon.App.Bsky.Feed.GeneratorView seq, onSelectFeed) =
    let displayFeeds = feeds |> Seq.map(fun f -> f.DisplayName |> Option.ofObj |> Option.defaultValue "No name")

    RadioGroup(displayFeeds)
      .OnSelectedItemChanged(fun args ->
        feeds |> Seq.tryItem args.SelectedItem |> onSelectFeed
      )

  let feedPost (post: Lexicon.App.Bsky.Feed.FeedViewPost) =
    let textContent = option {
      post.Post.Record
      let! (record: Lexicon.App.Bsky.Feed.FeedViewPost) = post.Post
      return! record.Text |> Option.ofNull
    }

    // top bar
    let author =
      Label($"@{post.Post.Author.DisplayName}")
        .Height(Dim.Percent(10))

    // content
    let content =
      Label(textContent |> Option.defaultValue "")
        .Width(Dim.Fill())
        .Height(Dim.Auto())

    // bottom bar
    let likes =
      Label($"{post.Post.LikeCount}")
        .Width(Dim.Percent(33))

    let replies =
      Label($"{post.Post.ReplyCount}")
        .Width(Dim.Percent(33))

    let reposts =
      Label($"{post.Post.RepostCount}")
        .Width(Dim.Percent(33))

    let postedAt =
      Label(post.Post.IndexedAt.ToString("yyyy-MM-dd HH:mm:ss"))
        .Width(Dim.Auto())

    postedAt.X(Pos.Right(author) + Pos(1)) |> ignore
    content.Y(Pos.Bottom(author) + Pos(1)) |> ignore

    likes.Y(Pos.Bottom(content)) |> ignore

    replies
      .X(Pos.Right(likes))
      .Y(Pos.Bottom(content))
    |> ignore

    reposts
      .X(Pos.Right(replies))
      .Y(Pos.Bottom(content))
    |> ignore

    FrameView()
      .Content(author, postedAt, content, likes, replies, reposts)

  let feedContent (content: Models.FeedViewPost array) =
    let scrollable = ScrollView()

    for post in content do
      scrollable.Add(feedPost(post)) |> ignore

    scrollable

  let view (args: ViewArgs) =

    let userFeeds: View aval = adaptive {
      let! feeds = args.feeds

      return
        feedSelector(
          feeds,
          fun feed ->
            task {
              match feed with
              | Some feed -> do! args.requestFeed feed
              | None -> ()
            }
            |> Task.FireAndForget
        )
    }

    let timeline: View aval = adaptive {
      let! selectedFeed = args.selectedFeed

      match selectedFeed with
      | None -> return Label("No feed selected")
      | Some(_, selectedFeed) -> return feedContent(selectedFeed.Feed)
    }

    let userFeeds =
      FrameView()
        .Content(userFeeds)
        .X(Pos.Center())
        .Height(Dim.Auto())
        .Width(Dim.Fill())

    let timeline =
      FrameView()
        .Content(timeline)
        .Y(Pos.Bottom(userFeeds) + Pos(1))
        .X(Pos.Center())
        .Height(Dim.Fill())
        .Width(Dim.Fill())

    Window()
      .Title(args.handle |> AVal.map(fun h -> $"@{h} feeds:"))
      .Content(userFeeds, timeline)
