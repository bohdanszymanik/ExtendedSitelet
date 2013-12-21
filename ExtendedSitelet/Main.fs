namespace Website2

open IntelliFactory.Html
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Sitelets

type Action =
    | Home
    | About
    | Login


module Controls =

    [<Sealed>]
    type EntryPoint() =
        inherit Web.Control()

        [<JavaScript>]
        override __.Body =
            Client.Main() :> _

    open IntelliFactory.WebSharper.Html
    type MenuControl() =
        inherit Web.Control()
        [<JavaScript>]
        override this.Body =
            I [Text "Some client stuff embedded in a control"; Attr.Style "color: green"] :> IPagelet

    type ClockControl() =
        inherit Web.Control()
        let now = System.DateTime.Now.ToString()
        [<JavaScript>]
        override this.Body = 
            Div [
                Span [Text "Current time: "]
                Span [Text now ] 
            ] :> IPagelet

module Skin =
    open System.Web

    type Page =
        {
            Title : string
            Menu : list<Content.HtmlElement>
            Body : list<Content.HtmlElement>
            Footer : list<Content.HtmlElement>
        }

    let MainTemplate =
        Content.Template<Page>("~/Main.html")
            .With("title", fun x -> x.Title)
            .With("menu", fun x -> x.Menu)
            .With("body", fun x -> x.Body)
            .With("footer", fun x -> x.Footer)

    let WithTemplate title body footer : Content<Action> =
        Content.WithTemplate MainTemplate <| fun context ->
            {
                Title = title
                Menu = [ Div [Text "test"] ] 
                Body = body context
                Footer = footer context
            }

module Site =

    let ( => ) text url =
        A [HRef url] -< [Text text]

    let Links (ctx: Context<Action>) =
        UL [
            LI ["Home" => ctx.Link Home]
//            LI ["About" => ctx.Link About]
//            LI ["Login" => ctx.Link Login]
        ]

    let HomePage =
        Skin.WithTemplate "HomePage" <| ( fun ctx ->
            [
                Div [Text "HOME"]
                Div [new Controls.EntryPoint()]
                Links ctx
            ] )
            <| fun ctx ->
            [
                Div [Text "Footer I guess..."]
                Div [new Controls.ClockControl()]
            ]

    let AboutPage =
        Skin.WithTemplate "AboutPage" <| fun ctx ->
                [
                Div [new Controls.MenuControl() ] -< [
                    P [Align "center"]  -<
                        [Text "Explain what this is all about"]
                ]
                Links ctx
            ]

    let LoginPage =
        Skin.WithTemplate "LoginPage" <| fun ctx ->
            [
                Div [Text "LOGIN"]
                Links ctx
            ]

    let Main =
        Sitelet.Sum [
            Sitelet.Content "/" Home HomePage
//            Sitelet.Content "/About" About AboutPage
//            Sitelet.Content "/Login" Login LoginPage
        ]

[<Sealed>]
type Website() =
    interface IWebsite<Action> with
        member this.Sitelet = Site.Main
        member this.Actions = [Home; About; Login]

type Global() =
    inherit System.Web.HttpApplication()

    member g.Application_Start(sender: obj, args: System.EventArgs) =
        ()

[<assembly: Website(typeof<Website>)>]
do ()
