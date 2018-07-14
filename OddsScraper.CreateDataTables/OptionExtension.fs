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
        | Some vt -> 
          let ou = fu vt
          ou

    let inline Bind_Nullable (vt : 'T) (fu : 'T -> Option<'U>) : Option<'U> =
        match vt with
        | null -> None
        | _ -> 
          let ou = fu vt
          ou

    let Delay ft : Option<'T> = ft ()

    type OptionBuilder() =
        member inline x.Return v       = Return v
        member inline x.ReturnFrom v   = ReturnFrom v
        member inline x.ReturnFrom v   = ReturnFrom_Nullable v
        member inline x.Bind (t, fu)   = Bind t fu
        member inline x.Bind (t, fu)   = Bind_Nullable t fu
        member inline x.Delay ft       = Delay ft

    let inline ofObj o =
        match o with
        | null -> None
        | _ -> Some o