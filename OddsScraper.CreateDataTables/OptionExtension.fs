namespace OddsScraper.CreateDataTables

// Basically like the classic `maybe` monad
//  but with added support for nullable types
module OptionExtension =
    let inline Return v : Option<'T> = Some v

    let inline ReturnFrom t : Option<'T> = t
    let inline ReturnFrom_Nullable ot : Option<'T> =
        match ot with
        | null -> None
        | _ -> Some ot

    let inline Bind (ot : Option<'T>) (fu : 'T -> Option<'U>) : Option<'U> =
        match ot with
        | None -> None
        | Some vt -> fu vt

    let inline Bind_Nullable (vt : 'T) (fu : 'T -> Option<'U>) : Option<'U> =
        Bind (ReturnFrom_Nullable vt) fu

    let Delay ft : Option<'T> = ft ()

    type OptionBuilder() =
        member inline __.Return v       = Return v
        member inline __.ReturnFrom v   = ReturnFrom v
        member inline __.ReturnFrom v   = ReturnFrom_Nullable v
        member inline __.Bind (t, fu)   = Bind t fu
        member inline __.Bind (t, fu)   = Bind_Nullable t fu
        member inline __.Delay ft       = Delay ft

    let inline ofObj o =
        match o with
        | null -> None
        | _ -> Some o

    let opt = OptionBuilder()
    