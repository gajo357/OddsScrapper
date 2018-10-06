module OddsScraper.FSharp.CommonScraping.CanopyAgent

open OddsScraper.FSharp.CommonScraping
open CanopyExtensions

type Link = string
type Html = string
type LinkToHtml = Link -> Html
type Message = AsyncReplyChannel<Html> * Link * LinkToHtml


type CanopyAgent() =
    let agent = MailboxProcessor<Message>.Start(fun inbox -> async {
        let rec loop () = async {
            let! channel, link, action = inbox.Receive()

            channel.Reply(action link)

            return! loop()
        }

        return! loop()
    })
    
    member __.GetPageHtml link = agent.PostAndAsyncReply(fun channel -> (channel, link, getPageHtml))

    member __.Login username password = 
        agent.PostAndAsyncReply(fun channel -> (channel, "", fun _ -> 
                loginToOddsPortalWithData username password
                getCurrentUrl()))

