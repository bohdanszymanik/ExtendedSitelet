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

    // I've written a client side control inside here but I guess really it should be in the client module
    open IntelliFactory.WebSharper.Html
    type MenuControl() =
        inherit Web.Control()
        [<JavaScript>]
        override this.Body =
            I [Text "Some client stuff embedded in a control"; Attr.Style "color: green"] :> IPagelet

    // ditto
    type ClockControl() =
        inherit Web.Control()
        let now = System.DateTime.Now.ToString()
        [<JavaScript>]
        override this.Body = 
            Div [
                Span [Text "Another client control this time embedding the current time: "]
                Span [Text now ] 
            ] :> IPagelet

module Skin =
    open System.Web

    // let's make our templated page a little more complex
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
                Menu = [ Div [Text "A bit of server side html combinator stuff embedded in code creating an instance of the page record"] ] // great I can just embed combinators here
                Body = body context
                Footer = footer context
            }

module Site =

    let ( => ) text url =
        A [HRef url] -< [Text text]

    let Links (ctx: Context<Action>) =
        UL [
            LI ["Home" => ctx.Link Home]
            LI ["About" => ctx.Link About]
            LI ["Login" => ctx.Link Login]
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
                // this is going to be footer stuff - rather than doing this every time this should really be a function shouldn't it...
                Div [Text "This is a one off footer on the home page"]
            ]



    let aFooter : Content.HtmlElement list =
        // so I could also stick this out in another file I suppose
        [
            // this is going to be footer stuff - rather than doing this every time this should really be a function shouldn't it...
            Div [Text "This is my footer with something appropriate to put at the bottom of every page... let's display the current date and time from our webcontrol"]
            Div [new Controls.ClockControl()]
        ]

    let AboutPage =
        Skin.WithTemplate "AboutPage" <| (fun ctx ->
                [
                Div [Text "About Page"]
                Div [new Controls.MenuControl() ] -< [
                    P [Align "center"]  -<
                        [Text "Explain what this is all about"]
                ]
                Links ctx
            ] )
            <| fun ctx -> aFooter

    let LoginPage =
        Skin.WithTemplate "LoginPage" <| (fun ctx ->
            [
                Div [Text "Login Page"]
                Links ctx
            ] ) <| fun ctx -> aFooter

    let Main =
        Sitelet.Sum [
            Sitelet.Content "/" Home HomePage
            Sitelet.Content "/About" About AboutPage
            Sitelet.Content "/Login" Login LoginPage
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
